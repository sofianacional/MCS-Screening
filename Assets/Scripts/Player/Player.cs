using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour {
	
	[SerializeField] private PlayerShoot playerShoot;
	
	// PLAYER DATA
	public int TotalScore { get; set; }

	public Gem CurrentGem; // to shoot
	
	public UnityEvent Evt_OnShoot = new();
	
	private void Awake() {
		SingletonManager.Register(this);
		playerShoot = GetComponent<PlayerShoot>();
	}

	public void SetNewGem(Gem newGem) {
		playerShoot.SetNewGem(newGem);
	}

	public void OnGemDestroy(int points) {
		TotalScore += points;
	}
}
