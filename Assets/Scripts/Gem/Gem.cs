using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GemType {
	Bronze,
	Silver,
	Gold,
	Red,
	Blue,
	Green
}
public class Gem : MonoBehaviour {
	
	[SerializeField] private GemMovement gemMovement;
	
	public GemType GemType;
	
	private void Awake() {
		gemMovement = GetComponent<GemMovement>();
	}

	public void StartMovement(Vector3 movePos) {
		gemMovement.CanMove = true;
		gemMovement.MoveDirection = movePos;
		transform.parent = null;
	}
	
}
