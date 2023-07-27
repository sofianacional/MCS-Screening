using UnityEngine;

public class Bounds : MonoBehaviour {

	public bool OnHitBounds = false;
	
	private void OnTriggerEnter2D(Collider2D col) {
		if (col.GetComponent<Gem>()) {
			if(!col.GetComponent<Gem>().IsAttached) return;
			print("on hit bounds");
			OnHitBounds = true;
		}
	}
	
}
