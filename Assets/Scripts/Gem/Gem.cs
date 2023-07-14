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

	private GemAttachment gemAttachment;
	private GemMovement gemMovement;

	public GemType GemType;
	
	private void Awake() {
		gemAttachment = GetComponent<GemAttachment>();
		gemMovement = GetComponent<GemMovement>();
	}
	
	
	
}
