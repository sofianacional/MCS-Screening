using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GemMovement : MonoBehaviour {

	private Gem gem;
	private Vector2 lastVelocity;
	
	[SerializeField] private Rigidbody2D rb;
	[SerializeField] private float speed;

	[SerializeField] private List<Gem> gemsInContact = new();
	[SerializeField] private bool canDetectCollision = true;
	
	public bool CanMove = false;
	public Vector3 MoveDirection;
	
	private void Awake() {
		gem = GetComponent<Gem>();
		rb = GetComponent<Rigidbody2D>();
	}

	private void FixedUpdate() {
		lastVelocity = rb.velocity;
	}

	public void Move() {
		if(!CanMove) return;
		transform.rotation = Quaternion.identity;
		rb.velocity = MoveDirection * speed;
	}

	private void Bounce(Collision2D col) {
		var newSpeed = lastVelocity.magnitude;
		
		Vector2 newVelocity = Vector2.Reflect(lastVelocity.normalized, col.contacts[0].normal);
		rb.velocity = newVelocity * Mathf.Max(newSpeed, 0);
	}
	
	public void OnCollisionEnter2D(Collision2D col) {
		if(!canDetectCollision == col.gameObject == gameObject) return;
		
		if (col.gameObject.GetComponent<TilemapCollider2D>()) {
			// if wall = bounce; if ceiling = attach
			var colObj = col.gameObject;
			if (colObj.gameObject.layer == 7) // Wall layer
				Bounce(col);
			else if (col.gameObject.gameObject.layer == 6) { // Ceiling layer
				//print("hit ceiling");
				gem.IsOnCeiling = true;
				gem.Evt_OnHitCeiling.Invoke(gem);
				DisableMovement();
			}
		}
		
		if(col.gameObject.GetComponent<Gem>()){
			gem.UpdateAdjacentGemsList();
			gem.Evt_OnHitOtherGem.Invoke(gem, col.gameObject.GetComponent<Gem>());
			DisableMovement();
		}
		
		
	}

	public void DisableMovement() {
		CanMove = false;
		canDetectCollision = false;
		rb.velocity = Vector3.zero;
		rb.constraints = RigidbodyConstraints2D.FreezeAll;
		transform.rotation = Quaternion.identity;
		
		// Check for nearby tilemap cols 
		gem.CheckSurroundingObjects();
	}
}
