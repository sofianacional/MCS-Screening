using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour {

	private Player player;
	
	private Gem nextGem;
	[SerializeField] private GameObject gemGroup;
	[SerializeField] private Gem gemPrefab; //Gem Prefabs - pero one muna for now for testing
	[SerializeField] private GemData[] gemDataList;
	[SerializeField] private Transform gemSpawnPoint;

	[SerializeField] private Gem[] sampleLevelGems;
	
	[SerializeField] private int counter;
	
	[SerializeField] private List<Gem> similarGems = new();
	[SerializeField] private List<Gem> similarGemsAdjacent = new();
	[SerializeField] private List<Gem> totalAdjacentGems = new();

	[SerializeField] private List<Gem> floatingGems = new();

	private Coroutine currentDestroyCoroutine;
	
	[SerializeField] private Ceiling ceiling;

	private int tempCount;
	
	private void Start() {
		player = SingletonManager.Get<Player>();
		player.Evt_OnShoot.AddListener(SpawnGem);

		SetLevelGemData();
		
		// Initiate Game
		SpawnGem();
	}

	private void SpawnGem() {
		do {
			if(nextGem) player.SetNewGem(nextGem);

			int randIndex = Random.Range(0, gemDataList.Length);
			Gem newGem = Instantiate(gemPrefab, gemSpawnPoint.position, Quaternion.identity);
			newGem.Initialize(gemDataList[randIndex]);
			newGem.transform.SetParent(gemGroup.transform);
			newGem.Evt_OnHitOtherGem.AddListener(AttachGemToGem);
			newGem.Evt_OnHitCeiling.AddListener(AttachGemToCeiling);
			nextGem = newGem;
		} while (!player.CurrentGem);
	}

	#region Initialize Sample Level

	private void SetLevelGemData() {
		foreach (var g in sampleLevelGems) {
			int randIndex = Random.Range(0, gemDataList.Length);
			g.Initialize(gemDataList[randIndex]);
			g.EnableComponents();
			g.DisableMovement();
			g.UpdateAdjacentGemsList();
		}
	}

	#endregion
	
	#region Gem Attachment
	
	// Gem attachment is handled by GM because there are multiple possible areas that a gem can attach to
	// Namely, to ANOTHER GEM or to the CEILING 

	private void AttachGemToGem(Gem invoker, Gem collidedGem) {
		invoker.transform.SetParent(gemGroup.transform);

		foreach (var g in collidedGem.Sides) {
			if (g.AttachedGem == invoker) {
				if (g.CanAttach) {
					invoker.transform.position = g.Origin.position;
					//CheckAdjacentGemTypes(invoker);
					//break;
				}
				else {
					print("find nearest attach point");
					invoker.transform.position = FindNearestAvailableAttachPoint(invoker, collidedGem);
				}
			}
		}
		CheckAdjacentGemTypes(invoker);
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
	}

	private void CheckAdjacentGemTypes(Gem invoker) {
		similarGems.Clear();
		similarGemsAdjacent.Clear();
		counter = 1;
		
		print("TYPE " + invoker.GemType);
		similarGems.Add(invoker);

		CountSimilarGemTypes(invoker);
		print("TOTAL COUNT = " + counter);
		
		if (counter >= 3) {
			// DestroySimilarGems();
			currentDestroyCoroutine = StartCoroutine(DestroySimilarGemsDelay());
		}
	}
	
	private void CountSimilarGemTypes(Gem gem) {
		foreach (var a in gem.Sides) {
			if (!IsLocatedInList(a.AttachedGem, similarGems) && a.AttachedGem && gem.GemType == a.AttachedGem.GemType) {
				// SIMILAR GEM, unchecked gem, can add
				counter++;
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
		//print("invoke evt");
		gemToDestroy.Evt_OnGemDestroyed.Invoke();
		//Destroy(gemToDestroy.gameObject);
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
			//print("find floating gem");
			FindFloatingGems();
		}
		
		yield return new WaitForSeconds(1f);
		
		foreach (var g in similarGems) {
			DestroyGem(g);
			Destroy(g.gameObject);
		}
		
		yield return new WaitForSeconds(0.1f);
		
		if(floatingGems.Count > 0) currentDestroyCoroutine = StartCoroutine(DestroyFloatingGemsDelay());
	}
	
	// FLOATING GEMS ** need recursion function that repeatedly calls itself to look for more floating gems
	
	private void FindFloatingGems() {
		// get every attached gems
		/*foreach (var adjacentGem in similarGemsAdjacent) {
			if(IsLocatedInList(adjacentGem, totalAdjacentGems)) continue;
			
			List<Gem> tempGemsList = new List<Gem>(); // temp list for the adjacent gem's adjacent gems
			tempGemsList = GetAllAdjacentGems(adjacentGem, tempGemsList);

			tempCount++;
			print("Cycle " + tempCount);
			if (IsFloating(tempGemsList)) {
				// Gems are not attached to anything
				foreach (var g in tempGemsList) {
					if (!floatingGems.Contains(g)) floatingGems.Add(g);
				}
			}
		}*/
		
		foreach (var adjacentGem in similarGemsAdjacent) {
			List<Gem> tempGemsList = new List<Gem>();
			
			if (IsFloating(adjacentGem, tempGemsList)) {
				if (tempGemsList.Count <= 0) return;
				
				foreach (var g in tempGemsList) {
					if (!floatingGems.Contains(g)) floatingGems.Add(g);
				}
			}
		}
		totalAdjacentGems.Clear();
	}

	private bool IsFloating(List<Gem> listOfGems) {
		if (listOfGems.Count <= 0) {
			print("list is empty");
			return false;
		}
		
		foreach (var g in listOfGems) {
			print(g.GemID + " is on ceiling = " + g.IsOnCeiling);
			if (g.IsOnCeiling) {
				print("is not floating");
				return false;
			}
		}
		print("is floating");
		return true;
	}
	
	private List<Gem> GetAllAdjacentGems(Gem gemToCheck, List<Gem> adjacentGemsList) {
		// get all adjacent until none
		//List<Gem> tempGemsList = new List<Gem>(); // temp list for the adjacent gem's adjacent gems
		if(!adjacentGemsList.Contains(gemToCheck)) adjacentGemsList.Add(gemToCheck);
		if(!totalAdjacentGems.Contains(gemToCheck)) totalAdjacentGems.Add(gemToCheck);
		
		foreach (var adjacentGem in gemToCheck.AdjacentGems) {
			if(!adjacentGem) continue;
			/*if (IsNotRegistered(adjacentGem, adjacentGemsList)) {
				//tempGemsList.Add(adjacentGem);
				adjacentGemsList.Add(adjacentGem);
				totalAdjacentGems.Add(adjacentGem);
				GetAllAdjacentGems(adjacentGem, adjacentGemsList);
			}*/
			
			adjacentGemsList.Add(adjacentGem);
			if(!totalAdjacentGems.Contains(gemToCheck)) totalAdjacentGems.Add(adjacentGem);
			
			if (!adjacentGem.IsOnCeiling && !IsLocatedInList(adjacentGem, adjacentGemsList) && !IsLocatedInList(gemToCheck, adjacentGemsList)) {
				//tempGemsList.Add(adjacentGem);
				//print("recursion");
				GetAllAdjacentGems(adjacentGem, adjacentGemsList);
			}
		}
		return adjacentGemsList;
	}

	private bool IsFloating(Gem initialGem, List<Gem> tempList) { // if connected to anything
		int count = 0;
		Gem gemToCheck = initialGem;
		while(!gemToCheck.IsOnCeiling){
			foreach (var adjacentGem in gemToCheck.AdjacentGems) {
				if (!adjacentGem || IsLocatedInList(adjacentGem, similarGems)) { // no adjacent gem + not in similar gem
					count++;
				}
				else if (adjacentGem || adjacentGem.IsOnCeiling) {
					tempList.Add(gemToCheck); // ADD IT TOO BEFORE CHANGING IN CASE FLOATING ?? IDK HELP
					gemToCheck = adjacentGem; // change gem to check
					break;
				}
			}
			
			if (count >= 6) { // floating
				tempList.Add(gemToCheck); // ** INSERT HERE PROPER WAY OF ADDING TO FLOATING GEMS LIST, YES
				return true; 
            }
        }
		return false;
	}
	
	private void CheckForMoreAdjacentGems(Gem gem, List<Gem> tempGemsList) {
		foreach (var g in gem.AdjacentGems) { // "unregistered" gem
			if (!g) continue;
			if (!IsNotRegistered(gem, tempGemsList)) {
				print("enter");
				tempGemsList.Add(g);
				totalAdjacentGems.Add(g);
				CheckForMoreAdjacentGems(g, tempGemsList);
			}
		}
	}

	private bool IsNotRegistered(Gem gemToCheck, List<Gem> tempGemsList) { // "registered" = within any existing lists
		return !IsLocatedInList(gemToCheck, similarGems) && !IsLocatedInList(gemToCheck, similarGemsAdjacent)
             && !IsLocatedInList(gemToCheck, totalAdjacentGems) && !IsLocatedInList(gemToCheck, tempGemsList);
	}
	
	/*private void CheckForMoreAdjacentGems(Gem gem, List<Gem> tempGemsList) {
		foreach (var g in gem.AdjacentGems) { // "unregistered" gem
			if (!g) continue;
			if (!IsLocatedInList(g, similarGems) && !IsLocatedInList(g, similarGemsAdjacent) 
			     && !IsLocatedInList(g, totalAdjacentGems) && !IsLocatedInList(g, tempGemsList)) {
				print("enter");
				tempGemsList.Add(g);
				totalAdjacentGems.Add(g);
				CheckForMoreAdjacentGems(g, tempGemsList);
			}
		}
	}*/

	private IEnumerator DestroyFloatingGemsDelay() {
		yield return new WaitForSeconds(1f);
		foreach (var f in floatingGems) {
			DestroyGem(f);
			Destroy(f.gameObject);
		}
		yield return new WaitForSeconds(0.1f);
		floatingGems.Clear();
	}
	
	#endregion
}
