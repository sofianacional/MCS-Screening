using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Side : MonoBehaviour {

    private Gem gem;

    [SerializeField] private bool canAttach = true;
    [SerializeField] private BoxCollider2D boxCol;
    
    public Transform Origin;
    public Gem AttachedGem;

    private void Awake() {
        gem = GetComponentInParent<Gem>();
        boxCol = GetComponent<BoxCollider2D>();
    }

    public void OnTriggerEnter2D(Collider2D col) {
        //if(AttachedGem || col.transform.parent == gameObject.transform.parent || !canAttach) return;
        if(col.transform == gameObject.transform.parent || !canAttach) return;
        if (col.GetComponent<Gem>()) {
            AttachedGem = col.GetComponent<Gem>();
            gem.UpdateAdjacentGemsList();
            
            AttachedGem.Evt_OnGemDestroyed.AddListener(ClearSideGem);
            //print("Gem " + transform.parent.gameObject + " to " + AttachedGem.gameObject);
        }
    }

    public void OnTriggerExit2D(Collider2D col) {
        if (col.GetComponent<Gem>()) {
            if (AttachedGem == col.GetComponent<Gem>()) {
                //print("object " + gem.GemID + " " + gameObject.name);
                print("trigger exit");
                ClearSideGem();
            }
        }
    }

    private void ClearSideGem() {
        print(gameObject.name + "clear");
        if(AttachedGem) AttachedGem.Evt_OnGemDestroyed.RemoveListener(ClearSideGem);
        AttachedGem = null;
        gem.UpdateAdjacentGemsList();
    }

    public void GetNearbyObjects() {
        Vector2 pos = (Vector2)transform.position - boxCol.offset;
        //print(pos);
        //Collider2D[] listOfColliders = Physics2D.OverlapCircleAll(pos, 0.05f);
        Collider2D[] listOfColliders = Physics2D.OverlapBoxAll(pos, new Vector2(0.06f, 0.06f),0f);
        
        foreach (var c in listOfColliders) {
            if (c.GetComponent<TilemapCollider2D>()) {
                if (c.gameObject.layer == 6) gem.IsOnCeiling = true;
                print(c);
                canAttach = false;
            }
            
            /*if(c.transform.parent == gameObject.transform.parent || !canAttach) continue;
            if (c.GetComponentInParent<Gem>()) {
                if (!AttachedGem) {
                    AttachedGem = c.GetComponentInParent<Gem>();
                    gem.UpdateAdjacentGemsList();
            
                    AttachedGem.Evt_OnGemDestroyed.AddListener(ClearSideGem);
                }
                else if (AttachedGem != c.GetComponentInParent<Gem>()) { // Update if containing incorrect gem
                    print("update incorrect gem");
                    ClearSideGem();
                    AttachedGem = c.GetComponentInParent<Gem>();
                    gem.UpdateAdjacentGemsList();
            
                    AttachedGem.Evt_OnGemDestroyed.AddListener(ClearSideGem);
                }
            }*/
        }
    }
}
