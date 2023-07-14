using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GemMovement : MonoBehaviour {

	[SerializeField] private float speed;
	
	public bool CanMove = false;
	public Vector3 MoveDirection;
	
	private void Update() {
		Move();
	}

	private void Move() {
		if(!CanMove) return;
		transform.position = Vector3.MoveTowards(transform.position, MoveDirection, speed * Time.deltaTime);
	}

	private void OnTriggerEnter2D(Collider2D col) {
		if (col.gameObject.GetComponent<TilemapCollider2D>()) {
			// if wall = bounce; if ceiling = attach
		}	
	}
}
