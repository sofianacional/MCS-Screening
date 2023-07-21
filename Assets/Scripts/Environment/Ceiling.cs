using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ceiling : MonoBehaviour {

	[SerializeField] private Transform[] attachPoints;

	public Vector3 GetNearestPoint(Vector3 obj) {
		Vector3 minDistance = Vector3.zero;
		float currentDistance = Vector3.Distance(attachPoints[0].position, obj);
		float lowestDistance = currentDistance;
		
		foreach (var p in attachPoints) {
			currentDistance = Vector3.Distance(p.position, obj);
			if (currentDistance <= lowestDistance) {
				lowestDistance = currentDistance;
				minDistance = p.position;
			}
		}
		
		return minDistance;
	}
	
}
