using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDetector : MonoBehaviour {
	public void OnTriggerEnter2D(Collider2D col) {
		print("trigger");
		if (col.GetComponent<Side>()) {
			print("disable side");
			col.gameObject.SetActive(false);
		}
	}
	
}
