using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour {
	
	[SerializeField] private GameObject playerGun;
	private float gunAngle;
	
	
	
	private void Update() {
		Aim();

		if (Input.GetMouseButton(0)) {
			
		}
		
	}
	
	private void Aim() {
		Vector3 direction = Input.mousePosition - Camera.main.WorldToScreenPoint(playerGun.transform.position);
		gunAngle = -Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
		
		if (gunAngle < -90 || gunAngle > 90) return;
		playerGun.transform.rotation = Quaternion.AngleAxis(gunAngle, Vector3.forward);
	}

	private void Shoot() {
		
	}

}
