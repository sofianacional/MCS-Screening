using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
	[SerializeField] private CircleCollider2D collider;
	
	
	public GemType GemType;
	public List<Side> Sides;
	public List<Gem> AdjacentGems;
	
	public UnityEvent<Gem> Evt_OnHitCeiling = new();
	public UnityEvent<Gem, Gem> Evt_OnHitOtherGem = new();
	
	private void Awake() {
		gemMovement = GetComponent<GemMovement>();
		collider = GetComponent<CircleCollider2D>();
	}

	public void Initialize() {
		// Disables the Gem's physics and other attributes on spawn
		collider.enabled = false;
	}

	private void EnableComponents() {
		collider.enabled = true;
	}
	
	public void StartMovement() {
		Vector3 movePos = -(transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition));
		gemMovement.MoveDirection = movePos;
		
		gemMovement.CanMove = true;
		EnableComponents();
		gemMovement.Move();
		
		transform.parent = null;
	}

	public void UpdateAdjacentGemsList() {
		for (int i = 0; i < Sides.Count; i++) {
			AdjacentGems[i] = Sides[i].AttachedGem;
		}
	}
	
	/*private List<Gem> GetAdjacentGems() {
		//List<Gem> adjacentGems = new List<Gem>();
		
		for (int i = 0; i < Sides.Count; i++) {
			AdjacentGems[i] = Sides[i].AttachedGem;
		}

		return AdjacentGems;
	}*/

	private void RemoveAdjacentGem(Gem gemToRemove) {
		foreach (var g in AdjacentGems) {
			if (g == gemToRemove) AdjacentGems.Remove(gemToRemove);
		}
	}
}
