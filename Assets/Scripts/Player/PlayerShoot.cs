using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour {
	
	[SerializeField] private GameObject playerGun;
	[SerializeField] private Transform firePoint;
	
	private float gunAngle;

	public Gem CurrentGem; // to shoot
	
	private void Update() {
		Aim();

		if (Input.GetMouseButton(0)) {
			Shoot();
		}
		
	}
	
	private void Aim() {
		Vector3 direction = Input.mousePosition - Camera.main.WorldToScreenPoint(playerGun.transform.position);
		gunAngle = -Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
		
		if (gunAngle < -90 || gunAngle > 90) return;
		playerGun.transform.rotation = Quaternion.AngleAxis(gunAngle, Vector3.forward);
	}

	private void Shoot() {
		if(!CurrentGem) return;
		
		// Spawn gem will move to direction, then does it's own thing (bounce, attach, etc.)
		// Player will get new gem > GM will spawn new random gem
	}

	public void SetNewGem(Gem newGem) {
		CurrentGem = newGem;
		CurrentGem.transform.position = firePoint.position;
	}
}