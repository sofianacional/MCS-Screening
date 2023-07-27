using System.Collections;
using UnityEngine;

public class PlayerShoot : MonoBehaviour {

	private Player player;
	
	[SerializeField] private GameObject playerGun;
	[SerializeField] private Transform firePoint;

	[SerializeField] private bool canShoot;
	[SerializeField] private float fireRate; // in seconds

	private bool disableShoot;
	private bool canAim;
	private Vector3 aimDirection;
	private float gunAngle;

	private void Start() {
		player = GetComponent<Player>();
		EnableShoot();
	}

	private void Update() {
		if(disableShoot) return;
		
		Vector2 mousePos = Input.mousePosition;
		if (mousePos.x >= Screen.width * 0.75) {
			canAim = false;
		}
		else canAim = true;
		
		Aim();
		
		if (Input.GetMouseButton(0) && canAim) {
			Shoot();
		}
		
	}
	
	private void Aim() {
		if(!canAim) return;
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
		if (newGem == null) return;
		player.CurrentGem = newGem;
		player.CurrentGem.transform.SetParent(playerGun.transform);
		player.CurrentGem.transform.position = firePoint.position;

		canShoot = true;
	}

	public void DisableShoot() {
		disableShoot = true;
		canAim = false;
		canShoot = false;
	}
	
	public void EnableShoot() {
		disableShoot = false;
		canAim = true;
		canShoot = true;
	}
}
