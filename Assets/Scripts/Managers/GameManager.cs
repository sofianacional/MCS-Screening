using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour {

	private Player player;
	
	private Gem nextGem;
	[SerializeField] private GameObject gemGroup;
	[SerializeField] private Gem gemPrefab; //Gem Prefabs - pero one muna for now for testing
	[SerializeField] private Transform gemSpawnPoint;

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
			
			Gem newGem = Instantiate(gemPrefab, gemSpawnPoint.position, Quaternion.identity);
			newGem.Initialize();
			newGem.Evt_OnHitOtherGem.AddListener(AttachGemToGem);
			newGem.Evt_OnHitCeiling.AddListener(AttachGemToCeiling);
			nextGem = newGem;
		} while (!player.CurrentGem);
	}

	#region Gem Attachment
	
	// Gem attachment is handled by GM because there are multiple possible areas that a gem can attach to
	// Namely, to ANOTHER GEM or to the CEILING 

	private void AttachGemToGem(Gem invoker, Gem collidedObject) {
		invoker.transform.SetParent(gemGroup.transform);
		foreach (var g in collidedObject.Quadrants) {
			if (g.QuadrantGem == invoker) {
				print("gem to attach has been located");
				invoker.transform.position = g.Origin.position;
				break;
			}
		}
	}

	private void AttachGemToCeiling(Gem gemToAttach) {
		gemToAttach.transform.position = ceiling.GetNearestPoint(gemToAttach.transform.position);
	}
	
	#endregion
}
