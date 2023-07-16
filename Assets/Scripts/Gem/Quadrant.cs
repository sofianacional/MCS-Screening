using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;

public class Quadrant : MonoBehaviour {
    
    public Transform Origin;
    public Gem QuadrantGem;
    
    public void OnTriggerEnter2D(Collider2D col) {
        if(QuadrantGem || col.transform.parent == gameObject.transform.parent) return;
        
        if (col.GetComponentInParent<Gem>()) {
            QuadrantGem = col.GetComponentInParent<Gem>();
            print("Gem " + transform.parent.gameObject + " to " + QuadrantGem.gameObject);
        }
    }

    private void SetQuadrantGem(Gem gemToAttach) {
        QuadrantGem = gemToAttach;
        //QuadrantGem.transform.position = Origin.position;
        /*
        QuadrantGem.transform.SetParent(transform);
        QuadrantGem.transform.position = Origin.position;
        QuadrantGem.transform.rotation = Quaternion.identity; // reset rotation */
    }
}
