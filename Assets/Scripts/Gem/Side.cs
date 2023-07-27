using UnityEngine;
using UnityEngine.Tilemaps;

public class Side : MonoBehaviour {

    private Gem gem;
    [SerializeField] private BoxCollider2D boxCol;
    
    public bool CanAttach = true;
    public Transform Origin;
    public Gem AttachedGem;

    private void Awake() {
        gem = GetComponentInParent<Gem>();
        boxCol = GetComponent<BoxCollider2D>();
    }

    public void OnTriggerEnter2D(Collider2D col) {
        //if(AttachedGem || col.transform.parent == gameObject.transform.parent || !CanAttach) return;
        if(col.transform == gameObject.transform.parent || !CanAttach) return;
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
                //print("trigger exit");
                ClearSideGem();
            }
        }
    }

    private void ClearSideGem() {
        //print(gameObject.name + "clear");
        if(AttachedGem) AttachedGem.Evt_OnGemDestroyed.RemoveListener(ClearSideGem);
        AttachedGem = null;
        gem.UpdateAdjacentGemsList();
    }

    public void GetNearbyObjects() {
        Vector2 pos = (Vector2)transform.position - boxCol.offset;
        //Collider2D[] listOfColliders = Physics2D.OverlapCircleAll(pos, 0.05f);
        Collider2D[] listOfColliders = Physics2D.OverlapBoxAll(pos, boxCol.size,0f);
        
        foreach (var c in listOfColliders) {
            if (c.GetComponentInParent<TilemapCollider2D>()) {
                if (c.gameObject.layer == 6) gem.IsOnCeiling = true;
                CanAttach = false;
            }

            if (c.gameObject.layer == 11) { // on hit bounds
                gem.Evt_OnHitBounds.Invoke();
            }
            
            /*if(c.transform.parent == gameObject.transform.parent || !CanAttach) continue;
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
