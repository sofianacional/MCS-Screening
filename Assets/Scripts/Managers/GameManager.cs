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
				break;
			}
		}
	}

	private void AttachGemToCeiling(Gem gemToAttach) {
		gemToAttach.transform.position = ceiling.GetNearestPoint(gemToAttach.transform.position);
	}

	private void CheckAdjacentGemTypes(Gem invoker) {
		similarGems.Clear();
		counter = 1;
		
		print("TYPE " + invoker.GemType);
		similarGems.Add(invoker);

		CountSimilarGemTypes(invoker);
		print("TOTAL COUNT = " + counter);
		
		if (counter >= 3) {
			DestroyGems();
		}
	}
	
	private void CountSimilarGemTypes(Gem gem) {
		foreach (var a in gem.Sides) {
			if (CanAddToList(a.AttachedGem) && a.AttachedGem && gem.GemType == a.AttachedGem.GemType) { // unchecked gem, can add
				counter++;
				similarGems.Add(a.AttachedGem);
				
				CountSimilarGemTypes(a.AttachedGem);
			}
		}
	}

	private bool CanAddToList(Gem gemToAdd) {
		foreach (var g in similarGems) {
			if (gemToAdd == g) return false;
		}
		return true;
	}
	
	private void DestroyGems() {
		foreach (var g in similarGems) {
			Destroy(g.gameObject);
		}
	}
	
	#endregion
}
