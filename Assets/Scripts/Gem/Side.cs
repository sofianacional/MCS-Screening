using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;

public class Side : MonoBehaviour {

    private Gem gem;
    
    public Transform Origin;
    public Gem AttachedGem;

    private void Awake() {
        gem = GetComponentInParent<Gem>();
    }

    public void OnTriggerEnter2D(Collider2D col) {
        if(AttachedGem || col.transform.parent == gameObject.transform.parent) return;
        
        if (col.GetComponentInParent<Gem>()) {
            AttachedGem = col.GetComponentInParent<Gem>();
            gem.UpdateAdjacentGemsList();
            //print("Gem " + transform.parent.gameObject + " to " + AttachedGem.gameObject);
        }
    }

    public void OnTriggerExit2D(Collider2D col) {
        if (col.GetComponentInParent<Gem>()) {
            if (AttachedGem == col.GetComponentInParent<Gem>()) {
                AttachedGem = null;
                gem.UpdateAdjacentGemsList();
            }
        }
    }

    private void SetQuadrantGem(Gem gemToAttach) {
        AttachedGem = gemToAttach;
        //AttachedGem.transform.position = Origin.position;
        /*
        AttachedGem.transform.SetParent(transform);
        AttachedGem.transform.position = Origin.position;
        AttachedGem.transform.rotation = Quaternion.identity; // reset rotation */
    }
}
