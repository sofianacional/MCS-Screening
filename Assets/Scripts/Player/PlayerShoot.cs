using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour {

	private Player player;
	
	[SerializeField] private GameObject playerGun;
	[SerializeField] private Transform firePoint;

	[SerializeField] private bool canShoot;
	[SerializeField] private float fireRate; // in seconds

	private Vector3 aimDirection;
	private float gunAngle;

	private void Awake() {
		player = SingletonManager.Get<Player>();
	}

	private void Update() {
		Aim();

		if (Input.GetMouseButton(0)) {
			Shoot();
		}
		
	}
	
	private void Aim() {
		aimDirection = Input.mousePosition - Camera.main.WorldToScreenPoint(playerGun.transform.position);
		gunAngle = -Mathf.Atan2(aimDirection.x, aimDirection.y) * Mathf.Rad2Deg;
		
		if (gunAngle < -90 || gunAngle > 90) return;
		playerGun.transform.rotation = Quaternion.AngleAxis(gunAngle, Vector3.forward);
	}
	
	private void Shoot() {
		if(!player.CurrentGem || !canShoot) return;
		StartCoroutine(ShootInterval());
	}

	private IEnumerator ShootInterval() {
		canShoot = false;
		
		// Spawn gem will move to direction, then does it's own thing (bounce, attach, etc.)
		player.CurrentGem.StartMovement();
		player.CurrentGem = null;
		
		yield return new WaitForSeconds(fireRate);
		
		// Player will get new gem > GM will spawn new random gem
		player.Evt_OnShoot.Invoke();
	}
	
	public void SetNewGem(Gem newGem) {
		player.CurrentGem = newGem;
		player.CurrentGem.transform.SetParent(playerGun.transform);
		player.CurrentGem.transform.position = firePoint.position;

		canShoot = true;
	}
	
}
