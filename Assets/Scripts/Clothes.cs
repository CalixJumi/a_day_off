using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Clothes : MonoBehaviour
{
   
    public string debugText = "";
    public List<Clothes> groupClothes;
    public Clothes beingPulledByClothes;
    public List<Clothes> pullingClothes = new List<Clothes>();

    public GameMakerInterface gmi;
    public DraggableBox draggableBox;
    public bool moved;

    // Start is called before the first frame update
    void Start()
    {        
        gmi = GetComponent<GameMakerInterface>();
        draggableBox = GetComponent<DraggableBox>();
    }

    // Update is called once per frame

    public bool forceDrag = false;
    public bool isMainDrag()
    {
        return draggableBox.isDragged() || forceDrag ;
    }


    public bool isConnectedPulled()
    {
        bool someoneIsDragged = false;
        foreach (Clothes bcloth in groupClothes)
        {
            if (bcloth.isMainDrag()) { 
                someoneIsDragged = true;
                if (!isMainDrag()) { beingPulledByClothes = bcloth; }
                break; 
            }
        }
        return someoneIsDragged;
    }

    public float isReincorporating = 0;
    public Clothes previousTarget;
    public Clothes TargetClothes(bool findAnyways = false)
    {
        if (previousTarget != null)
        {
            if (previousTarget.isMainDrag())
            {
                if (Mathf.Abs(gmi.x - previousTarget.gmi.x) + Mathf.Abs(gmi.y - previousTarget.gmi.y) < 128 - 64)
                {
                    return previousTarget;
                }
            }
        }

        if (groupClothes.Count <= 1) {
            return null; 
        }

        if ((isMainDrag()) && (findAnyways == false))
        {
            return null;
        }

        Clothes targetClothes = null;

        foreach (Clothes ac in groupClothes)
        {
            if (ac == this) { continue; }
            if (targetClothes == null) { targetClothes = ac; continue; }

            float ndx = ac.gmi.prevPos.x - gmi.prevPos.x;
            float tdx = targetClothes.gmi.prevPos.x - gmi.prevPos.x;
            float ndy = ac.gmi.prevPos.y - gmi.prevPos.y;
            float tdy = targetClothes.gmi.prevPos.y - gmi.prevPos.y;

            float andx = Mathf.Abs(ac.gmi.prevPos.x - gmi.prevPos.x);
            float atdx = Mathf.Abs(targetClothes.gmi.prevPos.x - gmi.prevPos.x);
            float andy = Mathf.Abs(ac.gmi.prevPos.y - gmi.prevPos.y);
            float atdy = Mathf.Abs(targetClothes.gmi.prevPos.y - gmi.prevPos.y);

            float sndx = (andx > 2) ? Mathf.Sign(ndx) : 0;
            float stdx = (atdx > 2) ? Mathf.Sign(tdx) : 0;
            float sndy = (andy > 2) ? Mathf.Sign(ndy) : 0;
            float stdy = (atdy > 2) ? Mathf.Sign(tdy) : 0;

            bool isHOposite = (Mathf.Abs(sndx - stdx) > 1.5f);
            bool isVOposite = (Mathf.Abs(sndy - stdy) > 1.5f);
            bool isOposite = (isHOposite || isVOposite);//&&(andx + atdx > 2);

            float dd = GMManager.PointDistance(gmi.prevPos.x, gmi.prevPos.y, targetClothes.gmi.prevPos.x, targetClothes.gmi.prevPos.y);
            float dtc = atdx + atdy + dd;
            bool disInc = false; //Mathf.Abs(dd - (atdx + atdy)) > 8;
            bool dpimd = (targetClothes.previousTarget != null) ? targetClothes.previousTarget.isMainDrag() : false;
            if (targetClothes.isMainDrag() || dpimd) {
                if (previousTarget == targetClothes) { dtc += 8; }
                if ((isOposite) || (disInc))
                {
                    dtc -= 128;//128;
                }
                else
                {
                    dtc -= 64;//64
                }
            }

            float nd = GMManager.PointDistance(gmi.prevPos.x, gmi.prevPos.y, ac.gmi.prevPos.x, ac.gmi.prevPos.y);
            float ntc = andx + andy + nd;
            bool nisInc = false; //Mathf.Abs(nd - (andx + andy)) > 8;
            bool npimd = (ac.previousTarget != null) ? ac.previousTarget.isMainDrag() : false;
            if (ac.isMainDrag() || npimd) {
                if (previousTarget == ac) { ntc += 8; }
                if ((isOposite)||(nisInc)) { 
                    ntc -= 128;
                }
                else
                {
                    ntc -= 64;
                }
            }

            if (ntc < dtc) {   
                targetClothes = ac; 
            }
        }

        previousTarget = targetClothes;
        return targetClothes;
    }

    int prow = 0;
    int pcol = 0;
    bool wasMainDrag = false;
    public bool disableStatic = false;
    public bool isTShape = false;
    CDirection tDirection = CDirection.Down;

    void Update()
    {
        OnMouseDownCheck();
        OnMouseDragCheck();
        OnMouseUpCheck();
        
        Clothes targetClothes = TargetClothes(true);
        if (targetClothes == null) { return; }

        if ((prow != draggableBox.row)||(pcol != draggableBox.column))
        {
            prow = draggableBox.row;
            pcol = draggableBox.column;
            previousTarget = null;
        }

        //Rearrange groupClothes
        if (isReincorporating <= 0) {
            groupClothes = groupClothes.OrderBy(c => GMManager.PointDistance(gmi.prevPos.x, gmi.prevPos.y, c.gmi.prevPos.x, c.gmi.prevPos.y)).ToList();
            groupClothes.Reverse();
        }

        if (forceDrag && !GMManager.Instance.globalMouseButton())
        {
            float dds = Mathf.Abs(targetClothes.gmi.x - gmi.x) + Mathf.Abs(targetClothes.gmi.y - gmi.y);
            if ((dds > 96))
            {
                targetClothes.forceDrag = true;
                //forceDrag = false;
            }


        }


        if (isMainDrag()) {

            //If Reincirporate condiitons
            /*
            if ((groupClothes.Count == 3)&&(draggableBox.dragged == true))
            {
                isTShape = true;
                Vector2 sumPos = new Vector2();
                Vector2 avPos = new Vector2();

                //TODO check isMid by calculating everyone's point and if it's aligned to one axis
                foreach (Clothes ac in groupClothes)
                {
                    sumPos += new Vector2(ac.gmi.x, ac.gmi.y);
                    if (ac == this) { continue; }
                    float dd = (Mathf.Abs(ac.gmi.x - gmi.x) + Mathf.Abs(ac.gmi.y - gmi.y));
                    if (Mathf.Abs(dd - 96) > 1) { isTShape = false; }                    
                }

                avPos = sumPos / 3;

                Vector2 mPos = GMManager.mouseDelta();
                bool congSide = false;
                if ((avPos.x < gmi.x) && (mPos.x > 0)) { congSide = true; tDirection = CDirection.Left; }//Go left
                if ((avPos.x > gmi.x) && (mPos.x < 0)) { congSide = true; tDirection = CDirection.Right; }//Go Right
                if ((avPos.y < gmi.y) && (mPos.y > 0)) { congSide = true; tDirection = CDirection.Down; }//Go Down
                if ((avPos.y > gmi.y) && (mPos.y < 0)) { congSide = true; tDirection = CDirection.Up; }//Go Up

                bool isMidPiece = ((Mathf.Abs(avPos.x - gmi.x) < 1)|| (Mathf.Abs(avPos.y - gmi.y) < 1));                
                debugText = "C:" + congSide.ToString() + "avpos.x: " + avPos.x.ToString() + ", gmi.x: " + gmi.x.ToString() + ", mPos.x: " + mPos.x.ToString();

                if (isReincorporating == false)
                {
                    if (congSide && isTShape && isMidPiece)
                    {
                        isReincorporating = true;
                    }
                }
            }
            */
            if (GMManager.Instance.globalMouseButton() == false) {
                if (isReincorporating > 0) {
                    isReincorporating -= 1;
                }
                else
                {
                    isReincorporating = 0;
                }
                
            }
            

            //activate
            //If Reincorporate ended, cancel it

            forceDrag = true;
            foreach (Clothes ac in groupClothes)
            {
                if (ac == this) { continue; }
                
                if (isReincorporating > 0)
                {
                    //Push away
                    PushAway(ac);
                }
                else if (!isTShape)
                {
                    ac.UpdateClothes();
                }
            }

        }

        disableStatic = false;
        wasMainDrag = isMainDrag();

        if ((GMManager.Instance.globalMouseButtonPressed())&&(forceDrag)) {
            Clothes tc = TargetClothes(true);
            //if (((Mathf.Abs(tc.gmi.x - gmi.x) + Mathf.Abs(tc.gmi.y - gmi.y)) <= 64)||(isTShape)||(tc.isTShape))
            if (((Mathf.Abs(tc.gmi.x - gmi.x) + Mathf.Abs(tc.gmi.y - gmi.y)) <= 64))
            {
                forceDrag = false;
            }
        }

        if ((!GMManager.Instance.globalMouseButton())&&(!forceDrag)){
            Clothes tc = TargetClothes(true);
            float dx = Mathf.Abs(tc.gmi.x - gmi.x);
            float dy = Mathf.Abs(tc.gmi.y - gmi.y);
            if ((Mathf.Max(dx,dy) > 64)||(dx + dy >= 128))
            {
                forceDrag = true;
            }
        }
    }


    void PushAway(Clothes aclothes)
    {
        if (Mathf.Abs(aclothes.gmi.x - gmi.x) < 64)
        {
            bool sDir = (aclothes.gmi.x < gmi.x);
            if ((tDirection == CDirection.Up)|| (tDirection == CDirection.Down))
            {
                aclothes.draggableBox.PushDirection(sDir ? CDirection.Left : CDirection.Right, 16, true, dDrag: false);
            }
        }

        if (Mathf.Abs(aclothes.gmi.y - gmi.y) < 64)
        {
            bool sDir = (aclothes.gmi.y < gmi.y);
            if ((tDirection == CDirection.Left)||(tDirection == CDirection.Right))
            {
                aclothes.draggableBox.PushDirection(sDir? CDirection.Down : CDirection.Up, 16, true, dDrag: false);
            }                
        }

    }

    void UpdateClothes()
    {
        Clothes targetClothes = TargetClothes(true);       
        if (targetClothes == null) { return;  }

        if (targetClothes != null)
        {
            //If target is main and coming at me, move aside
            float dx = Mathf.Abs(gmi.prevPos.x - targetClothes.gmi.prevPos.x);
            float dy = Mathf.Abs(gmi.prevPos.y - targetClothes.gmi.prevPos.y);

            if (dx + dy > 64)
            {
                bool isVert = (dx < dy);
                bool isInc = ((dx > 0.5f) && (dy > 0.5f));

                if (isInc)
                {
                  
                    if (isVert)
                    {
                        UpdateClothesV();
                        UpdateClothesH();
                    }
                    else
                    {
                        UpdateClothesH();
                        UpdateClothesV();
                    }
                }
                else
                {
                    if (isVert) { UpdateClothesV(); } else { UpdateClothesH(); }
                }
            }
        }
    }

    void UpdateClothesH()
    {
        Clothes targetClothes = TargetClothes(true);

        float dx = Mathf.Abs(gmi.prevPos.x - targetClothes.gmi.prevPos.x);
        float dy = Mathf.Abs(gmi.prevPos.y - targetClothes.gmi.prevPos.y);
        bool isVert = (dx < dy);
        bool isInc = ((dx > 0.5f) && (dy > 0.5f));
        bool checkVert = (isVert || isInc);
        bool checkHor = (!isVert || isInc);
        bool weakPush = !isMainDrag();// && !isConnectedPulled();

         if (gmi.prevPos.x < targetClothes.gmi.prevPos.x - 1.5f)
        {
            float dd = (targetClothes.gmi.prevPos.x - gmi.prevPos.x) - ((checkHor) ? 0 : 64);
            //Debug.Log("d-right: " + dd.ToString());

            draggableBox.PushDirection(CDirection.Right, dd * 0.25f, weakPush, false);
        }
        else if (gmi.prevPos.x > targetClothes.gmi.prevPos.x + 1.5f)
        {
            float dd = (gmi.prevPos.x - targetClothes.gmi.prevPos.x) - ((checkHor) ? 0 : 64) ;
            //Debug.Log("d-left: " + dd.ToString() + ", inc: " + isInc.ToString() + ", isVert: " + isVert.ToString());
            draggableBox.PushDirection(CDirection.Left, dd * 0.25f, weakPush, false);
        }
    }

    void UpdateClothesV()
    {
        Clothes targetClothes = TargetClothes(true);

        float dx = Mathf.Abs(gmi.prevPos.x - targetClothes.gmi.prevPos.x);
        float dy = Mathf.Abs(gmi.prevPos.y - targetClothes.gmi.prevPos.y);
        bool isVert = (dx < dy);
        bool isInc = ((dx > 0.5f) && (dy > 0.5f));
        bool checkVert = (isVert || isInc);
        bool checkHor = (!isVert || isInc);
        bool weakPush = !isMainDrag();// || isConnectedPulled();

        if (gmi.prevPos.y < targetClothes.gmi.prevPos.y - 1.5f)
        {
            float dd = (targetClothes.gmi.prevPos.y - gmi.prevPos.y) - ((checkVert) ? 0 : 64);
            //Debug.Log("d-up: " + dd.ToString());
            draggableBox.PushDirection(CDirection.Up, dd * 0.25f, weakPush, false);
        }
        else if (gmi.prevPos.y > targetClothes.gmi.prevPos.y + 1.5f)
        {
            float dd = (gmi.prevPos.y - targetClothes.gmi.prevPos.y) - ((checkVert) ? 0 : 64);
            //Debug.Log("d-down: " + dd.ToString());
            draggableBox.PushDirection(CDirection.Down, dd * 0.25f, weakPush, false);
        }
    }


    bool willPush = false;
    Vector2 ppos = new Vector2();
    private void OnMouseDownCheck()
    {
        if (GMManager.Instance.globalMouseButtonPressed() == false) { return; }
        if (draggableBox.CanBeDragged() == false) { return; }
        //If someone else is on top, return;
        Vector3 mp3 = GMManager.mousePosition();
        Vector2 mp = new Vector2(mp3.x, mp3.y);
        Rect tr = GameMakerInterface.RectTransformToScreenSpace(gmi.rect);
        if (tr.Contains(mp) == false) { return; }
        willPush = true;
        ppos = new Vector2(gmi.x, gmi.y);
    }

    private void OnMouseDragCheck()
    {
        if ((Mathf.Abs(ppos.x - gmi.x) > 1) || (Mathf.Abs(ppos.y - gmi.y) > 1))
        {
            willPush = false;
        }
    }

    private void OnMouseUpCheck()
    {
        if (GMManager.Instance.globalMouseButtonReleased() == false) { return; }

        if (willPush)
        {
            Punch();
        }

        willPush = false;
    }

    private void Punch()
    {        
        if (groupClothes == null) {  return; }
        if (groupClothes.Count <= 2) {  return; }
        //if (!isTShape) { Debug.Log("no T shape"); return; }

        SoundManager.Instance.PlaySFX("suction_pop", volume: 0.62f);

        int i = 0;
        
        //draggableBox.PushDirection(CDirection.Up, 64);
        List<Clothes> gclothes = new List<Clothes>(groupClothes);
        isReincorporating = 6;

        CDirection rdir = CDirection.Right;
        CDirection adir = CDirection.Left;
        CDirection odir = CDirection.Up;

        if (draggableBox.CanPushDirection(CDirection.Right, new List<GameMakerInterface>(), true))
        {            
            if (groupClothes[1].draggableBox.row == draggableBox.row)
            {
                Clothes db = groupClothes[1];
                groupClothes[1] = groupClothes[2];
                groupClothes[2] = db;
            }

            rdir = CDirection.Right; adir = CDirection.Left;
            odir = (groupClothes[1].draggableBox.row < draggableBox.row) ? CDirection.Down : CDirection.Up;

        }
        else if (draggableBox.CanPushDirection(CDirection.Left, new List<GameMakerInterface>(), true))
        {
            if (groupClothes[1].draggableBox.row == draggableBox.row)
            {
                Clothes db = groupClothes[1];
                groupClothes[1] = groupClothes[2];
                groupClothes[2] = db;
            }

            rdir = CDirection.Left; adir = CDirection.Right;
            odir = (groupClothes[1].draggableBox.row < draggableBox.row) ? CDirection.Down : CDirection.Up;
        }
        else if (draggableBox.CanPushDirection(CDirection.Up, new List<GameMakerInterface>(), true))
        {
            if (groupClothes[1].draggableBox.column != draggableBox.column)
            {
                Clothes db = groupClothes[1];
                groupClothes[1] = groupClothes[2];
                groupClothes[2] = db;
            }

            rdir = CDirection.Up; adir = CDirection.Down;
            odir = (groupClothes[1].draggableBox.column > draggableBox.column) ? CDirection.Left : CDirection.Right;
        }
        else if (draggableBox.CanPushDirection(CDirection.Down, new List<GameMakerInterface>(), true))
        {
            if (groupClothes[1].draggableBox.row != draggableBox.row)
            {
                Clothes db = groupClothes[1];
                groupClothes[1] = groupClothes[2];
                groupClothes[2] = db;
            }
            rdir = CDirection.Down; adir = CDirection.Up;
            odir = (groupClothes[1].draggableBox.column > draggableBox.column) ? CDirection.Left : CDirection.Right;
        }
        else { return; }

        Clothes c1 = groupClothes[1];
        Clothes c2 = groupClothes[2];

        groupClothes = new List<Clothes>();

        draggableBox.PushDirection(rdir, 16);
        draggableBox.PushDirection(rdir, 16);
        draggableBox.PushDirection(rdir, 16);
        c1.draggableBox.PushDirection(odir, 8);        
        draggableBox.PushDirection(rdir, 8);
        c1.draggableBox.PushDirection(odir, 8);
        draggableBox.PushDirection(rdir, 8);
        c1.draggableBox.PushDirection(odir, 8);
        c1.draggableBox.PushDirection(odir, 16);
        

        //isReincorporating = false;
        groupClothes = gclothes;
        /*if (groupClothes[1].draggableBox.row == draggableBox.row) {
                        groupClothes[1].draggableBox.PushDirection(CDirection.Left, 64);
                        groupClothes[2].draggableBox.PushDirection(CDirection.Down, 64);
                    }*/

        //groupClothes[1].draggableBox.PushDirection(CDirection.Up, 64);
        

        //Move to an empty place
        //Move second to that


    }
}
