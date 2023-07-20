using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public enum GemType {
	Bronze,
	Silver,
	Gold,
	Red,
	Blue,
	Green
}
public class Gem : MonoBehaviour {

	public int GemID;

	[SerializeField] private GemMovement gemMovement;
	[SerializeField] private CircleCollider2D gemCollider;
	
	
	public GemType GemType;
	public List<Side> Sides;
	public List<Gem> AdjacentGems;
	
	public bool IsOnCeiling;
	
	public UnityEvent<Gem> Evt_OnHitCeiling = new();
	public UnityEvent<Gem, Gem> Evt_OnHitOtherGem = new();

	public UnityEvent Evt_OnGemDestroyed = new();
	public UnityEvent<Gem> Evt_OnBecomeFloatingGem = new();
	
	private void Awake() {
		gemMovement = GetComponent<GemMovement>();
		gemCollider = GetComponent<CircleCollider2D>();
	}
	
	public void Initialize() {
		// Disables the Gem's physics and other attributes on spawn
		gemCollider.enabled = false;
		
		// generate temp gemID for debugging
		GemID = Random.Range(1000, 2000);
	}

	private void EnableComponents() {
		gemCollider.enabled = true;
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
		CheckSurroundingObjects();
	}

	public void CheckSurroundingObjects() {
		foreach (var s in Sides) {
			s.GetNearbyObjects();
		}
	}
}
