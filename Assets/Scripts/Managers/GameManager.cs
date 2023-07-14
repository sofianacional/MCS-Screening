using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class GameManager : MonoBehaviour {

	private Player player;
	private Gem nextGem;
	
	//Gem Prefabs - pero one muna for now for testing
	[SerializeField] private Gem gemPrefab;
	[SerializeField] private Transform gemSpawnPoint;
	
	private void Start() {
		player = SingletonManager.Get<Player>();
		
		// Initiate Game
		SpawnGem();
		if(nextGem) player.SetNewGem(nextGem);
		SpawnGem();
	}

	private void SpawnGem() {
		Gem newGem = Instantiate(gemPrefab, gemSpawnPoint.position, Quaternion.identity);
		// + set parent to gems parent obj
		nextGem = newGem;
	}
	
}
