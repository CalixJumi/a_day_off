using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chair : MonoBehaviour
{
    public Chair chairPair;
    public GameMakerInterface gmi;
    public DraggableBox draggableBox;
    public SpriteRenderer spriteRenderer;
    
// Start is called before the first frame update
    void Start()
    {
        gmi = GetComponent<GameMakerInterface>();
        draggableBox = GetComponent<DraggableBox>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        //Find nearest chair
        //Set as chair pair
        CollisionManager cm = gmi.collisionManager;
        float d = 96;
        foreach(Transform child in cm.transform)
        {
            Chair achair = child.GetComponent<Chair>();
            if (achair == null) { continue; }
            if (achair == this) { continue; }
            Vector3 p = transform.position;
            Vector3 pp = achair.transform.position;
            float dd = GMManager.PointDistance(p.x, p.y, pp.x, pp.y);
            if (dd < d){
                d = dd;
                chairPair = achair;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (draggableBox.isDragged() == true)
        {
            DidPushUpdate();
            //vertically, move it
        }
    }

    public void DidPushUpdate()
    {
        if (chairPair == null) { return; }
        //Move chair pair
        //If position of other doesn't fit horizontally, move it
        bool isLShape = (draggableBox.pieceCode != chairPair.draggableBox.pieceCode);
        bool isHorizontal = ((draggableBox.pieceCode == "g") && (chairPair.draggableBox.pieceCode == "g"));


        float leftComp = (gmi.x - chairPair.gmi.x) - (isLShape ? 32 : (isHorizontal ? 64 : 0));

        if (leftComp > 0)
        {
            chairPair.draggableBox.PushDirection(CDirection.Right, leftComp, true);
        }

        float rightComp = (chairPair.gmi.x - gmi.x) - (isLShape ? 32 : (isHorizontal ? 64 : 0));
        if (rightComp > 0)
        {
            chairPair.draggableBox.PushDirection(CDirection.Left, rightComp, true);
        }

        float downComp = (gmi.y - chairPair.gmi.y) - (isLShape ? 32 : (isHorizontal ? 0 : 64));
        if (downComp > 0)
        {
            chairPair.draggableBox.PushDirection(CDirection.Up, downComp, true);
        }

        float upComp = (chairPair.gmi.y - gmi.y) - (isLShape ? 32 : (isHorizontal ? 0 : 64));
        if (upComp > 0)
        {
            chairPair.draggableBox.PushDirection(CDirection.Down, upComp, true);
        }
    }
}
