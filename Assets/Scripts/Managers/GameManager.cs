using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour {

	private Player player;
	
	private Gem nextGem;
	[SerializeField] private GameObject gemGroup;
	[SerializeField] private Gem[] gemPrefabs; //Gem Prefabs - pero one muna for now for testing
	[SerializeField] private Transform gemSpawnPoint;

	[SerializeField] private int counter;
	
	[SerializeField] private List<Gem> similarGems = new();
	[SerializeField] private List<Gem> similarGemsAdjacent = new();
	[SerializeField] private List<Gem> totalAdjacentGems = new();

	[SerializeField] private List<Gem> floatingGems = new();

	private Coroutine currentDestroyCoroutine;
	
	[SerializeField] private Ceiling ceiling;
	
	private void Start() {
		player = SingletonManager.Get<Player>();
		player.Evt_OnShoot.AddListener(SpawnGem);
		
		// Initiate Game
		SpawnGem();
	}

	private void SpawnGem() {
		do {
			if(nextGem) player.SetNewGem(nextGem);

			int randIndex = Random.Range(0, gemPrefabs.Length);
			Gem newGem = Instantiate(gemPrefabs[randIndex], gemSpawnPoint.position, Quaternion.identity);
			newGem.Initialize();
			newGem.transform.SetParent(gemGroup.transform);
			newGem.Evt_OnHitOtherGem.AddListener(AttachGemToGem);
			newGem.Evt_OnHitCeiling.AddListener(AttachGemToCeiling);
			nextGem = newGem;
		} while (!player.CurrentGem);
	}

	#region Gem Attachment
	
	// Gem attachment is handled by GM because there are multiple possible areas that a gem can attach to
	// Namely, to ANOTHER GEM or to the CEILING 

	private void AttachGemToGem(Gem invoker, Gem collidedGem) {
		invoker.transform.SetParent(gemGroup.transform);
		foreach (var g in collidedGem.Sides) {
			if (g.AttachedGem == invoker) {
				//print("gem to attach has been located");
				invoker.transform.position = g.Origin.position;
				CheckAdjacentGemTypes(invoker);
				//break;
			}
		}
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
		print("invoke evt");
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
		
		foreach (var g in similarGems) {
			DestroyGem(g);
		}

		yield return new WaitForSeconds(0.1f);
		
		// IF LIST CONTAINS AT LEAST ONE ELEMENT
		if (similarGemsAdjacent.Count > 0) { 
			//print("find floating gem");
			FindFloatingGems();
		}
		
		yield return new WaitForSeconds(1f);
		
		foreach (var g in similarGems) {
			Destroy(g.gameObject);
		}
		
		yield return new WaitForSeconds(0.1f);
		
		if(floatingGems.Count > 0) currentDestroyCoroutine = StartCoroutine(DestroyFloatingGemsDelay());
	}
	
	// FLOATING GEMS ** need recursion function that repeatedly calls itself to look for more floating gems
	
	private void FindFloatingGems() {
		// get every attached gems
		foreach (var adjacentGem in similarGemsAdjacent) {
			List<Gem> tempGemsList = new List<Gem>(); // temp list for the adjacent gem's adjacent gems
			
			tempGemsList.Add(adjacentGem);
			//totalAdjacentGems.Add(adjacentGem);
			CheckForMoreAdjacentGems(adjacentGem, tempGemsList);
			
			if (IsFloating(tempGemsList)) { // Gems are not attached to anything
				foreach (var g in tempGemsList) {
					if(!floatingGems.Contains(g)) floatingGems.Add(g);
				}
			}

			foreach (var g in tempGemsList) {
				totalAdjacentGems.Add(g);
			}
		}
		totalAdjacentGems.Clear();
	}

	private bool IsFloating(List<Gem> listOfGems) {
		foreach (var g in listOfGems) {
			if (g.IsOnCeiling) return false;
		}

		return true;
	}

	private void CheckForMoreAdjacentGems(Gem gem, List<Gem> tempGemsList) {
		foreach (var g in gem.AdjacentGems) { // "unregistered" gem
			if (!g) continue;
			if (!IsLocatedInList(g, similarGems) && !IsLocatedInList(g, similarGemsAdjacent) && !IsLocatedInList(g, totalAdjacentGems) && !IsLocatedInList(g, tempGemsList)) {
				tempGemsList.Add(g);
				//totalAdjacentGems.Add(g);
				CheckForMoreAdjacentGems(g, tempGemsList);
			}
		}
	}

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
