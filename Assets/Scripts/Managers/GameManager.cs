using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour {

	private Player player;
	private Gem nextGem;
	private ScoreSystem scoreSystem;
	
	[Header("Pre-Spawn Gems Data")]
	[SerializeField] private GameObject gemGroup;
	[SerializeField] private Gem gemPrefab; //Gem Prefabs - pero one muna for now for testing
	
	[SerializeField] private GemData[] gemDataList;
	[SerializeField] private List<GemData> availableGemTypes = new();

	[SerializeField] private Transform gemSpawnPoint;
	
	[Space]
	[SerializeField] private Gem[] sampleLevelGems;
	
	[Header("Post-Spawn Gems Data")]
	[SerializeField] private int similarGemsCounter;

	[SerializeField] private List<Gem> spawnedGems = new();
	[SerializeField] private List<Gem> similarGems = new();
	[SerializeField] private List<Gem> similarGemsAdjacent = new();
	[SerializeField] private List<Gem> totalAdjacentGems = new();

	[SerializeField] private List<Gem> floatingGems = new();

	private Coroutine currentDestroyCoroutine;
	private Coroutine loweringCeilingCoroutine;
	
	[Header("Environment")]
	[SerializeField] private Ceiling ceiling;
	[SerializeField] private Bounds bounds;

	[Space]
	[SerializeField] private float loweringInterval;
	
	public UnityEvent<int> Evt_PlayerWin = new();
	public UnityEvent<int> Evt_GameOver = new();
	public UnityEvent Evt_ExitGame = new();
	
	private int tempCount;
	
	private void Awake() {
		SingletonManager.Register(this);
	}
	
	private IEnumerator Start() {
		player = SingletonManager.Get<Player>();
		player.Evt_OnShoot.AddListener(SpawnGem);

		scoreSystem = GetComponent<ScoreSystem>();
		scoreSystem.Evt_AddPoints.AddListener(player.OnGemDestroy);

		SetLevelGemData();

		yield return new WaitForSeconds(.1f);
		
		// Initiate Game
		SpawnGem();
		loweringCeilingCoroutine = StartCoroutine(LowerCeilingInterval());
	}

	private void RemoveSingleton() {
		SingletonManager.Remove<GameManager>();
	}

	private void SpawnGem() {
		if(spawnedGems.Count <= 0) return;
		
		do {
			if(nextGem) player.SetNewGem(nextGem);
			
			availableGemTypes = GetAvailableGemTypes();
			int randIndex = Random.Range(0, availableGemTypes.Count);
			
			Gem newGem = Instantiate(gemPrefab, gemSpawnPoint.position, Quaternion.identity);
			newGem.Initialize(availableGemTypes[randIndex]);
			
			//newGem.transform.SetParent(gemGroup.transform);
			newGem.Evt_OnHitOtherGem.AddListener(AttachGemToGem);
			newGem.Evt_OnHitCeiling.AddListener(AttachGemToCeiling);
			newGem.Evt_OnHitBounds.AddListener(GameOver);
			nextGem = newGem;
			
		} while (!player.CurrentGem);
	}

	private List<GemData> GetAvailableGemTypes() {
		List<GemData> availableGemTypes = new();
		
		foreach (var gem in spawnedGems) {
			if (!availableGemTypes.Contains(gem.Data)) {
				availableGemTypes.Add(gem.Data);
			}
		}
		return availableGemTypes;
	}

	#region Initialize Sample Level

	private void SetLevelGemData() {
		foreach (var g in sampleLevelGems) {
			int randIndex = Random.Range(0, gemDataList.Length);
			g.Initialize(gemDataList[randIndex]);
			g.EnableComponents();
			g.DisableMovement();
			g.UpdateAdjacentGemsList();
			
			spawnedGems.Add(g);
		}
	}

	#endregion

	#region Game Conditions

	private void GameOver() {
		Evt_GameOver.Invoke(player.TotalScore);
		player.DisableShoot();
		PauseGame();
	}

	private void WinGame() {
		Evt_PlayerWin.Invoke(player.TotalScore);
		player.DisableShoot();
		PauseGame();
	}

	public void PauseGame() {
		player.DisableShoot();
		Time.timeScale = 0;
	}

	public void ResumeGame() {
		player.EnableShoot();
		Time.timeScale = 1;
	}

	public void ExitGame() {
		RemoveSingleton();
		player.RemoveSingleton();
		scoreSystem.RemoveSingleton();
		
		SceneManager.LoadScene("MainMenu");
	}

	#endregion
	
	#region Gem Attachment
	
	private void AttachGemToGem(Gem invoker, Gem collidedGem) {
		invoker.transform.SetParent(gemGroup.transform);

		foreach (var g in collidedGem.Sides) {
			if (g.AttachedGem == invoker) {
				if (g.CanAttach) {
					invoker.transform.position = g.Origin.position;
				}
				else {
					invoker.transform.position = FindNearestAvailableAttachPoint(invoker, collidedGem);
				}
			}
		}
		CheckAdjacentGemTypes(invoker);
		spawnedGems.Add(invoker);
	}

	private Vector2 FindNearestAvailableAttachPoint(Gem invoker, Gem collidedGem) {
		Vector3 minDistance = Vector3.zero;
		float currentDistance = Vector3.Distance(collidedGem.Sides[0].Origin.position, invoker.transform.position);
		float lowestDistance = currentDistance;
		
		foreach (var g in collidedGem.Sides) {
			currentDistance = Vector3.Distance(g.Origin.position, invoker.transform.position);
			if (currentDistance <= lowestDistance && g.CanAttach) {
				lowestDistance = currentDistance;
				minDistance = g.Origin.position;
			}
		}

		return minDistance;
	}
	
	private void AttachGemToCeiling(Gem gemToAttach) {
		gemToAttach.transform.position = ceiling.GetNearestPoint(gemToAttach.transform.position);
		gemToAttach.transform.SetParent(gemGroup.transform);
		
		spawnedGems.Add(gemToAttach);
	}

	private void CheckAdjacentGemTypes(Gem invoker) {
		similarGems.Clear();
		similarGemsAdjacent.Clear();
		similarGemsCounter = 1;
		
		print("TYPE " + invoker.GemType);
		similarGems.Add(invoker);

		CountSimilarGemTypes(invoker);
		print("TOTAL COUNT = " + similarGemsCounter);
		
		if (similarGemsCounter >= 3) currentDestroyCoroutine = StartCoroutine(DestroySimilarGemsDelay());
		else if (bounds.OnHitBounds) GameOver();
	}
	
	private void CountSimilarGemTypes(Gem gem) {
		foreach (var a in gem.Sides) {
			if (!IsLocatedInList(a.AttachedGem, similarGems) && a.AttachedGem && gem.GemType == a.AttachedGem.GemType) {
				// SIMILAR GEM, unchecked gem, can add
				similarGemsCounter++;
				similarGems.Add(a.AttachedGem);
				CountSimilarGemTypes(a.AttachedGem);
			}
		}
	}

	private bool IsLocatedInList(Gem gemToAdd, List<Gem> listOfGems) {
		foreach (var g in listOfGems) {
			if (gemToAdd == g) return true;
		}
		return false;
	}

	private void DestroyGem(Gem gemToDestroy) {
		gemToDestroy.Evt_OnGemDestroyed.Invoke();
		spawnedGems.Remove(gemToDestroy);
	}

	private void CollateGemAdjacentGems() {
		foreach (var a in similarGems) { // Checking each similar gem ADJACENT GEM
			foreach (var b in a.AdjacentGems) {
				if (!IsLocatedInList(b, similarGems) && !IsLocatedInList(b, similarGemsAdjacent) && b) {
					// If gem (b) contains a gem and is not in the Similar Gems list and Similar Gem-Adjacent Gems list
                	similarGemsAdjacent.Add(b);
                }
			}
		}
	}

	private IEnumerator DestroySimilarGemsDelay() {
		// GET ADJACENT GEMS (to look for potential floating gems)
		CollateGemAdjacentGems();

		yield return new WaitForSeconds(0.1f);
		
		// IF LIST CONTAINS AT LEAST ONE ELEMENT
		if (similarGemsAdjacent.Count > 0) { 
			FindFloatingGems();
		}
		
		yield return new WaitForSeconds(1f);
		
		foreach (var g in similarGems) {
			DestroyGem(g);
			Destroy(g.gameObject);
		}
		scoreSystem.ComputeSimilarGemsScore(similarGemsCounter);
		
		yield return new WaitForSeconds(0.1f);
		
		if(floatingGems.Count > 0) currentDestroyCoroutine = StartCoroutine(DestroyFloatingGemsDelay());
		
		if (spawnedGems.Count <= 0) WinGame();
		if (bounds.OnHitBounds) GameOver();
	}
	
	// FLOATING GEMS ** need recursion function that repeatedly calls itself to look for more floating gems
	
	private void FindFloatingGems() {
		// get every attached gems
		foreach (var adjacentGem in similarGemsAdjacent) { // Main checker for each similar gem adjacent
			List<Gem> tempList = new List<Gem>(); // temp list = list of gems that are adjacent to each other, until on ceiling/nothing 
			tempList.Add(adjacentGem);
			
			if (IsFloating(tempList)) {
				if (tempList.Count <= 0) return;
				
				foreach (var g in tempList) {
					if (!floatingGems.Contains(g)) floatingGems.Add(g);
				}
			}
		}
		totalAdjacentGems.Clear();
	}

	private bool IsFloating(List<Gem> checkedGemsList) { // Returns if on ceiling/floating
		List<Gem> queueList = new List<Gem>(); // adds "unchecked" gems to the queue list first
		int queueIndex = 0;
		
		Gem gemToCheck = checkedGemsList[0];
		queueList.Add(gemToCheck);

		while(!gemToCheck.IsOnCeiling){ // Loop until gem to check is attached to ceiling / all gems have been checked but none are on ceiling
			//print("Gem to check: " + gemToCheck.GemID);
			int emptyCounter = 0;
			var checkedGemCounter = 0;
			
			foreach (var adjacentGem in gemToCheck.AdjacentGems) { // CHECKS ALL ADJACENT GEM OF CURRENT GEM; run through all current gem adjacent gem
				if (adjacentGem && adjacentGem.IsOnCeiling) { // adjacent gem is on ceiling so everything attached to gemToCheck is not floating
					return false;
				}
				if ( adjacentGem && IsLocatedInList(adjacentGem, checkedGemsList) && !IsLocatedInList(adjacentGem, queueList)) { // already in temp list, already checked
					checkedGemCounter++;
					//print("already in list");
					if (checkedGemCounter >= 6) 
						return false;
				}
				else if (!adjacentGem || IsLocatedInList(adjacentGem, similarGems)) { // no adjacent gem OR not in similar gem
					//print("empty");
					emptyCounter++;
					if (emptyCounter >= 6) { // floating, no adjacent gem
						//print("floating, return");
						return true; 
					}
				}
				/*else if (adjacentGem && IsLocatedInList(adjacentGem, similarGemsAdjacent) && !IsLocatedInList(adjacentGem, checkedGemsList)) { 
					
				}*/
				else if (adjacentGem && !IsLocatedInList(adjacentGem, checkedGemsList) && !IsLocatedInList(adjacentGem, queueList)) { // has adjacent, hasn't been checked
					//print( adjacentGem.GemID + " = has adjacent, put gem to queue");
					queueList.Add(adjacentGem);
				}
				
				if(!checkedGemsList.Contains(gemToCheck)) checkedGemsList.Add(gemToCheck); // have been checked and 
				if(!totalAdjacentGems.Contains(gemToCheck)) totalAdjacentGems.Add(gemToCheck);
			}

			if (AreAllChecked(gemToCheck, queueList)) {
				//print("all gems are checked");
				return true;
			}
			
			//print(gemToCheck.GemID + " ?? " + queueList[queueIndex].GemID);
			
			// change gem to check
			if (gemToCheck == queueList[queueIndex]) {
				queueIndex++;
				//print("queue index: " + queueIndex + "; queue list count: " + queueList.Count);
				if (queueIndex >= queueList.Count) {
					//print("no more next element");
					return true;
				}
				
				gemToCheck = queueList[queueIndex];
				//print("next element = " + gemToCheck);
				if(gemToCheck.IsOnCeiling) break;
			}
			else {
				//print("not supposed to enter here but im here to prevent the infinite loop");
				break;
			}
		}
		//print("on ceiling");
		return false;
	}

	private bool AreAllChecked(Gem gemToCheck, List<Gem> queueList) {
		int count = 0;
		if (IsLocatedInList(gemToCheck, queueList)) count++;
		
		foreach (var gemInQueue in queueList) {
			foreach (var adjacentGem in gemToCheck.AdjacentGems) {
				if (!adjacentGem) continue;
				if (gemInQueue == adjacentGem) count++;
			}
		}
		return count >= 7; // true if checked is >= 7, including self
	}

	private IEnumerator DestroyFloatingGemsDelay() {
		yield return new WaitForSeconds(1f);
		foreach (var f in floatingGems) {
			DestroyGem(f);
			Destroy(f.gameObject);
		}
		scoreSystem.ComputeFloatingGemsScore(floatingGems.Count);
		
		yield return new WaitForSeconds(0.1f);
		
		floatingGems.Clear();
		
		if (spawnedGems.Count <= 0) WinGame();
	}
	
	#endregion

	#region Lowering of Ceiling
	
	private IEnumerator LowerCeilingInterval() {
		var ceilingTransform = ceiling.transform;
		var gemGroupTransform = gemGroup.transform;
		
		while (ceilingTransform.position.y > -6f && !bounds.OnHitBounds) {
			yield return new WaitForSeconds(loweringInterval);
			
			ceilingTransform.Translate(0f, -1f, 0f);
			gemGroupTransform.Translate(0f, -1f, 0f);
		}
	}

	#endregion
	
}
