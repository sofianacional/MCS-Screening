using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
	
	[SerializeField] private PlayerShoot playerShoot;

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
