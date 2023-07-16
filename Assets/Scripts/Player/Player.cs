using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour {
	
	[SerializeField] private PlayerShoot playerShoot;

	public Gem CurrentGem; // to shoot
	
	
	public UnityEvent Evt_OnShoot = new();
	
	private void Awake() {
		SingletonManager.Register(this);
		playerShoot = GetComponent<PlayerShoot>();
	}

	private void Start() {
		
	}

	public void SetNewGem(Gem newGem) {
		playerShoot.SetNewGem(newGem);
	}
}
