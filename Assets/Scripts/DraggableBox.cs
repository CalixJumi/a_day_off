using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum CDirection { Up, Down, Left, Right }
public enum PieceType { Wall, Block, Joint, Static, PullPush, Slim, EmptyBox, BoxContent}
public enum ParcelStatus { RegisterIn, RegisterOut, StayIn, Unknown }

public class DraggableBox : MonoBehaviour
{
    // Start is called before the first frame update
    public string debugText = "";
    public bool dragged = false;
    public bool canDrag = true;
    bool flagAdjust = false;
    public bool shakeFade = false;
    public bool showDebug = false;

    public bool isDragged()
    {
        return dragged;
    }

    public GameMakerInterface gmi;
    float xoff = 0; float xspeed = 0;
    float yoff = 0; float yspeed = 0;

    public PieceType pieceType = PieceType.Block;
    public string pieceCode = "o";
    public int pieceTag = 0;


    public ParcelStatus parcel = ParcelStatus.StayIn;

    public PuzzleController puzzleController;

    public int zindex = 0;
    public bool slimPiece = false;
    public SpriteRenderer spriteRenderer;

    public int row = -1;
    public int column = -1;

    public float targetAlpha = 1;
    float alpha = 0;
    bool toDestroy = false;

    public Vector2 baseSpriteSize;
    public Vector2 spriteSize;

    public float xposclip;
    public float yposclip;

    public void Start()
    {

        gmi = GetComponent<GameMakerInterface>();
        xposclip = gmi.x;
        yposclip = gmi.y;

        //spriteRenderer = GetComponent<SpriteRenderer>();
        Color invis = Color.white;
        invis.a = 0;
        spriteRenderer.color = invis;
        targetAlpha = 1;
        alpha = UnityEngine.Random.Range(-0.2f, 0);

        if (canDrag == false){ spriteRenderer.enabled = false; }

        baseSpriteSize = spriteRenderer.size;
        spriteSize = baseSpriteSize;

    }

    public void HideAndDestroy()
    {
        toDestroy = true;
        targetAlpha = 0;
        alpha *= 0;
    }

    public void UpdateGridValue()
    {
        if ((spriteRenderer != null)&&(baseSpriteSize.x == 0)&&(baseSpriteSize.y == 0)){ 
            baseSpriteSize = spriteRenderer.size; 
        }

        float fr = -gmi.y / 64 + puzzleController.rows / 2.0f - 0.5f;
        float fc = gmi.x / 64 + puzzleController.columns / 2.0f - 0.5f;
        if (baseSpriteSize.x > 84) { fc += Mathf.Sign(gmi.x) * 84 / baseSpriteSize.x; }
        if (baseSpriteSize.y > 84) { fr -= Mathf.Sign(gmi.y) * 84 / baseSpriteSize.y; }

        debugText = "Column: " + fc.ToString() + ", Row: " + fr.ToString();
        row = (int)Mathf.Floor(fr + 0.5f);// + Mathf.Sign(gmi.y));
        column = (int)Mathf.Floor(fc + 0.5f);// + Mathf.Sign(gmi.y));
        //This might cause trouble with double pieces
    }

    Vector2 localMovement;
    bool wasPushed = false;
    void Update()
    {
        float aalpha = spriteRenderer.color.a + (IsInside() ? 1 : 0.88f);
        Color c = spriteRenderer.color; c.a = aalpha; spriteRenderer.color = c;


        if (GMManager.Instance.globalMouseButton() == false) { dragged = false; }
        //debugText = "inside: " + IsInside().ToString() + ", register: " + CorrectRegister().ToString() + "[" + column.ToString() + "," + row.ToString() +"]";
        if (toDestroy) { targetAlpha = 0; }

        float efa = shakeFade ? UnityEngine.Random.Range(0.84f, 0.97f) : 0.62f;
        float ssfa = (1 - 1 / (2 + 2 * efa * (Time.deltaTime * 60 - 1)));
        alpha = alpha * efa + targetAlpha * (1 - efa);

        Color acolor = Color.white; acolor.a = alpha;

        bool shouldDarken = (!IsInside() && ((parcel == ParcelStatus.RegisterIn) || (parcel == ParcelStatus.StayIn)));
        //bool shouldHighlight = (CorrectRegister() && ((parcel == ParcelStatus.RegisterIn) || (parcel == ParcelStatus.RegisterOut)));        
        bool shouldHighlight = (CorrectRegister() && (parcel == ParcelStatus.RegisterOut));
        if (shouldDarken)
        {
            acolor.r *= 0.76f;
            acolor.g *= 0.88f;
            acolor.b *= 0.98f;
            acolor.a *= 0.8f;
            spriteSize *= 0.8f;
        }else if (shouldHighlight)
        {
            spriteSize *= 1.44f;
        }
        else
        {
            acolor.r *= 0.96f;
            acolor.g *= 0.98f;
            acolor.b *= 1.0f;
            //acolor.a *= 0.94f;
        }

        spriteRenderer.color = acolor;
        if ((alpha <= 0.12f) && (toDestroy))
        {
            Destroy(this.gameObject);
        }

        if (puzzleController.childController != null) { return; }

        AdjustZPosition();
        OnMouseUpCheck(); 
        OnMouseDownCheck();

        //spriteSize = (spriteSize * 0.78f + baseSpriteSize * 0.22f);
        float ef = 0.22f;
        float ssf = (1 - 1 / (2 + 2 * ef * (Time.deltaTime * 60 - 1)));
        spriteSize = spriteSize * ef + baseSpriteSize * ( 1 - ef);

        spriteRenderer.size = (spriteRenderer.size * 0.78f + spriteSize * 0.22f);


        if (pieceType != PieceType.Wall)
        {
            gmi.z = ((zindex == 0) ? -1 : 1);
        }
        else
        {
            gmi.z = 10;
        }

        recursiveSafe = 0;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = (zindex == 0) ? acolor : new Color(0.48f, 0.5f, 0.88f, alpha);
        }

        float mx = GMManager.mousePosition().x;
        float my = GMManager.mousePosition().y;

        float xd = (gmi.x - (mx - xoff)) * 1;
        float yd = (gmi.y - (my - yoff)) * 1;

        float maxMov = 64 * (Time.deltaTime * 60);
        if (isDragged())
        {
            //Stop at 16's
            //if (Mathf.Abs(xposclip - gmi.x) > maxMov) { xd = (gmi.x - (Mathf.Floor(mx / maxMov + 0.5f) * maxMov)) * 1; }
            //if (Mathf.Abs(yposclip - gmi.y) > maxMov) { yd = (gmi.y - (Mathf.Floor(my / maxMov + 0.5f) * maxMov)) * 1; }

            if (Mathf.Abs(yd) > Mathf.Abs(xd))//(Math.Abs(yd) > Math.Abs(xd))
            {
                if (Math.Abs(yd) > 0.99f) { PushDirection((yd > 0) ? CDirection.Down : CDirection.Up, Mathf.Min(Mathf.Abs(yd) * (1), maxMov), dDrag: true); }
                if (Math.Abs(xd) > 0.99f) { PushDirection((xd > 0) ? CDirection.Left : CDirection.Right, Mathf.Min(Mathf.Abs(xd) * (1), maxMov), dDrag: true); }
            }
            else
            {
                if (Math.Abs(xd) > 0.99f) { PushDirection((xd > 0) ? CDirection.Left : CDirection.Right, Mathf.Min(Mathf.Abs(xd) * (1), maxMov), dDrag: true); }
                if (Math.Abs(yd) > 0.99f) { PushDirection((yd > 0) ? CDirection.Down : CDirection.Up, Mathf.Min(Mathf.Abs(yd) * (1), maxMov), dDrag: true); }
            }
        }

        UpdateGridValue();

        bool isClothes = (pieceCode == "b") || (pieceCode == "d");
        float tx = Mathf.Floor(gmi.x / 32.0f + 0.5f) * 32.0f;
        float ty = Mathf.Floor(gmi.y / 32.0f + 0.5f) * 32.0f;

        //if (!isClothes)
        // {
        float radi = 64.0f;

             bool fixHor = (puzzleController.oddColumns != (baseSpriteSize.x >= 168));
             bool fixVer = (puzzleController.oddRows != (baseSpriteSize.y >= 168));

             float xf = (fixHor ? 0 : 0.5f);
             float yf = (fixVer ? 0 : -0.5f);
             tx = (Mathf.Floor(gmi.x / radi + 0.5f - xf) + xf) * radi;
             ty = (Mathf.Floor(gmi.y / radi + 0.5f - yf) + yf) * radi;
        //}

        bool mousePressed = (GMManager.Instance.globalMouseButtonPressed() == false);
        bool closeVToGrid = (Mathf.Abs(gmi.y - ty) < 4.0f);
        bool closeHToGrid = (Mathf.Abs(gmi.x - tx) < 4.0f);

        //Fix position X
        if (((closeHToGrid)||(!GMManager.Instance.globalMouseButton())) && !wasPushed)// || (Mathf.Abs(yd) > Mathf.Abs(xd) * 8))//(canDrag && !dragged))
        {
            if (showDebug) { Debug.Log("Can do H"); }
            if (tx < gmi.x - 0.9f)
            {
                float d = Mathf.Max(Mathf.Min((gmi.x - tx) * 0.25f, maxMov), 1.0f);
                PushDirection(CDirection.Left, d, true);
                xoff += d;
            }
            else if (tx > gmi.x + 0.9f)
            {
                float d = Mathf.Max(Mathf.Min((tx - gmi.x) * 0.25f, maxMov), 1.0f);
                PushDirection(CDirection.Right, d, true);
                xoff -= d;
            }

            if (Mathf.Abs(tx - gmi.x) < 1.0f) { gmi.x = tx; }
        }

        //Fix position Y
        if (((closeVToGrid) || (!GMManager.Instance.globalMouseButton())) && !wasPushed)// || (Mathf.Abs(xd) > Mathf.Abs(yd) * 8))//|| (canDrag && !dragged))
        {
            if (ty < gmi.y - 0.9f)
            {
                float d = Mathf.Max(Mathf.Min((gmi.y - ty) * 0.25f, maxMov), 1.0f);
                PushDirection(CDirection.Down, d, true);
                yoff += d;
            }
            else if (ty > gmi.y + 0.9f)
            {
                float d = Mathf.Max(Mathf.Min((ty - gmi.y) * 0.25f, maxMov), 1.0f);
                PushDirection(CDirection.Up, d, true);
                yoff -= d;
            }

            if (Mathf.Abs(ty - gmi.y) < 1.0f) { gmi.y = ty; }
        }

        //Box interactions
        if (pieceType == PieceType.BoxContent)//"vynil")
        {
            List<GameMakerInterface> colls = gmi.CollisionOnPoint(gmi.x, gmi.y);

            foreach (GameMakerInterface agmi in colls)
            {
                DraggableBox otherBox = agmi.GetComponent<DraggableBox>();

                if (otherBox.pieceType == PieceType.EmptyBox)//"turntable")
                {
                    float d = GMManager.PointDistance(gmi.x, gmi.y, agmi.x, agmi.y);
                    if (d < 16.0f)
                    {
                        Transform musicbox = Instantiate(puzzleController.piecesDictionary["m"], otherBox.transform.position, Quaternion.identity);
                        musicbox.SetParent(this.transform.parent);
                        DraggableBox db = musicbox.GetComponent<DraggableBox>();
                        puzzleController.SetupPiece(musicbox);

                        List<string> routs = puzzleController.puzzleModel.registerOuts;
                        List<string> rins = puzzleController.puzzleModel.registerIns;


                        if (routs.Contains("m")|| routs.Contains("t")|| routs.Contains("v"))
                        {
                            db.parcel = ParcelStatus.RegisterOut;
                        }else if (rins.Contains("m") || rins.Contains("t") || rins.Contains("v"))
                        {
                            db.parcel = ParcelStatus.RegisterIn;
                        }

                        puzzleController.boxes.Remove(otherBox);
                        puzzleController.boxes.Remove(this);
                        Destroy(otherBox.gameObject);
                        Destroy(gameObject);
                        gmi.collisionManager.shouldRemapCollisionObjects = true;
                        SoundManager.Instance.PlaySFX("piece_drop", volume: 0.42f);
                        break;
                    }
                }
            }
        }

        //Clic clic sounds
        float r = 64.0f;
        if (pieceType != PieceType.Wall)
        {
            if (Mathf.Abs(xposclip - gmi.x) > r/ 2.0f)
            {
                spriteSize = new Vector2(baseSpriteSize.x * 0.96f, baseSpriteSize.x * 1.04f);
                if (dragged) { SoundManager.Instance.PlaySFX("piece_dragging", volume: 0.62f); }
                xposclip += ((xposclip - gmi.x) > 0) ? -r : r;
            }

            if (Mathf.Abs(yposclip - gmi.y) > r/ 2.0f)
            {
                spriteSize = new Vector2(baseSpriteSize.x * 1.04f, baseSpriteSize.x * 0.96f);
                if (dragged) { SoundManager.Instance.PlaySFX("piece_dragging", volume: 0.62f); }
                yposclip += ((yposclip - gmi.y) > 0) ? -r : r;
            }
        }

        wasPushed = false;

        if (shakeFade)
        {
            gmi.x += UnityEngine.Random.Range(-2f, 2f);
            gmi.y += UnityEngine.Random.Range(-2f, 2f);
        }

        
    }

    public void CheckParcelStatus(List<string> registerIn, List<string> registerOut)
    {
        bool isTorV = ((pieceCode == "t") || (pieceCode == "v"));
        foreach (string rin in registerIn)
        {
            if ((rin == pieceCode) && (!IsInside()))
            {
                parcel = ParcelStatus.RegisterIn;
                break;
            }
        }

        foreach (string rin in registerOut)
        {
            if ((rin == pieceCode) && (IsInside()))
            {
                parcel = ParcelStatus.RegisterOut;
                break;
            }
        }


    }

    //TODO to framework
    public Rect RectTransformToScreenSpace(RectTransform transform)
    {
        Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
        return new Rect((Vector2)transform.position - (size * 0.5f), size);
    }


    private void OnMouseDownCheck()
   {
        if (!canDrag) { return; }
        if (GMManager.Instance.globalMouseButtonPressed() == false) { return; }
        if (CanBeDragged() == false) { return; }

        //If rect is not on mouse location
        Vector3 mp3 = GMManager.mousePosition();
        Vector2 mp = new Vector2(mp3.x, mp3.y);

        Rect tr = RectTransformToScreenSpace(gmi.rect);

        if (tr.Contains(mp) == false)
        {
            return;
        }

        if (canDrag == false) {
            return; 
        }

        dragged = true;
        xoff = mp3.x - gmi.x;
        yoff = mp3.y - gmi.y;

        //SoundManager.Instance.PlaySFX("piece_take");
        SoundManager.Instance.PlaySFX("coin_take", volume: 0.14f);
        spriteSize = new Vector2(baseSpriteSize.x * 1.45f, baseSpriteSize.x * 1.45f);
    }

    private void OnMouseUpCheck()
    {
        if (!canDrag) { return; }
        if (GMManager.Instance.globalMouseButtonReleased() == false) { return; }

        if (dragged)
        {
            SoundManager.Instance.PlaySFX("coin_drop", volume: 0.12f);
            spriteSize = new Vector2(baseSpriteSize.x * 0.85f, baseSpriteSize.x * 0.85f);
        }

        dragged = false;
    }

    public bool CanBeDragged()
    {
        Vector3 mp3 = GMManager.mousePosition();
        List<GameMakerInterface> cols = GMManager.CollisionForPoint(puzzleController.collisionManager, mp3.x, mp3.y);


        foreach (GameMakerInterface agmi in cols)
        {
            if (agmi == gmi) { continue; }
            DraggableBox box = agmi.GetComponent<DraggableBox>();
            if (zindex > box.zindex) { return false; }
        }

        return true;
    }

    int recursiveSafe = 0;

    public bool CanPushDirection(CDirection cDirection, List<GameMakerInterface> pushingBlocks, bool isWeak = false, float distance = 1)
    {
        recursiveSafe += 1;
        if (recursiveSafe >= 256)
        {
            Debug.Log("Recursive limit!" + this.GetInstanceID().ToString());
            return false;
        }

        if (canDrag == false) {
            //Debug.Log("Can't Drag " + pieceType.ToString());
            return false; 
        }

        if (pushingBlocks.Contains(gmi)) {
            Debug.Log("Already contained " + pieceType.ToString());
            return true;
        } else {
            pushingBlocks.Add(gmi);
        }

        float v = 0.9f;//Amount to test for movement, currently movement is actually 1
        float ngmix = gmi.x; float ngmiy = gmi.y;
        if (cDirection == CDirection.Up) { ngmiy += v; }
        else if (cDirection == CDirection.Down) { ngmiy -= v; }
        else if (cDirection == CDirection.Right) { ngmix += v; }
        else if (cDirection == CDirection.Left) { ngmix -= v; }

        Chair thischair = GetComponent<Chair>();
        if (thischair != null)
        {
            Chair thisochair = thischair.chairPair;
            if (thisochair != null) {
                if (pushingBlocks.Contains(thisochair.gmi) == false)
                {
                    bool isLShape = (pieceCode != thisochair.draggableBox.pieceCode);
                    bool isHorizontal = ((pieceCode == "g") && (thisochair.draggableBox.pieceCode == "g"));

                    float d = 0;
                    if (cDirection == CDirection.Left)
                        d = (thisochair.gmi.x - ngmix) - (isLShape ? 32 : (isHorizontal ? 64 : 0));
                    if (cDirection == CDirection.Right)
                        d = (ngmix - thisochair.gmi.x) - (isLShape ? 32 : (isHorizontal ? 64 : 0));
                    if (cDirection == CDirection.Down)
                        d = (thisochair.gmi.y - ngmiy) - (isLShape ? 32 : (isHorizontal ? 0 : 64));
                    if (cDirection == CDirection.Up)
                        d = (ngmiy - thisochair.gmi.y) - (isLShape ? 32 : (isHorizontal ? 0 : 64));

                    if (d > 0)
                    {
                        if (thisochair.draggableBox.CanPushDirection(cDirection, pushingBlocks, false, distance) == false)
                        {
                            return false;
                        }
                    }
                }
            }
        }

        Clothes thisClothes = GetComponent<Clothes>();
        if (thisClothes != null)
        {
            //If is being pulled act as a unit
            Clothes targetClothes = thisClothes.TargetClothes();
            if (targetClothes != null)
            {
                if (thisClothes.isConnectedPulled() == false)
                {
                    //If im already v far, nope
                    foreach (Clothes aclothes in thisClothes.groupClothes)
                    {
                        if (pushingBlocks.Contains(aclothes.gmi) == false)
                        {
                            if (aclothes.draggableBox.CanPushDirection(cDirection, pushingBlocks, distance: distance) == false)
                            {
                                //Retialate
                                aclothes.forceDrag = true;
                                return false;
                            }
                        }
                    }
                }
            }
        }


        Rect thisRect = GameMakerInterface.RectTransformToScreenSpace(gmi.rect);
        thisRect.center = new Vector2(ngmix, ngmiy);


        List<GameMakerInterface> colls = gmi.CollisionOnPoint(ngmix, ngmiy);
        if (colls == null) { return true; }
        if (colls.Count == 0) {
            return true; }

        foreach (GameMakerInterface coll in colls)
        {
            if (pushingBlocks.Contains(coll)) { continue; }

            DraggableBox otherBox = coll.GetComponent<DraggableBox>();

            //Sensitive direction
            if ((cDirection == CDirection.Left) && (gmi.x < otherBox.gmi.x)) { continue; }
            if ((cDirection == CDirection.Right) && (gmi.x > otherBox.gmi.x)) { continue; }
            if ((cDirection == CDirection.Up) && (gmi.y > otherBox.gmi.y)) { continue; }
            if ((cDirection == CDirection.Down) && (gmi.y < otherBox.gmi.y)) { continue; }

            //If they don't z collide
            if ((zindex != otherBox.zindex) && (slimPiece) && (otherBox.slimPiece)){ continue;}

            //If they don't box - empty box collide
            if ((pieceType == PieceType.BoxContent) && (otherBox.pieceType == PieceType.EmptyBox)){ continue; }

            //If they actually collide

            if (isWeak) {
                //Debug.Log("Is weak " + pieceType.ToString());
                return false; 
            }

            if (otherBox.dragged)
            {
                //Debug.Log("Box is dragged " + otherBox.pieceType.ToString());
                return false;
            }

            if (otherBox.CanPushDirection(cDirection, pushingBlocks, isWeak, distance) == false)
            {
                //If they collide but can swap layers
                if ((slimPiece) && (otherBox.slimPiece))
                {
                    PushPullPiece thisPPP = GetComponent<PushPullPiece>();
                    PushPullPiece otherPPP = coll.GetComponent<PushPullPiece>();
                    if ((thisPPP != null) && (otherPPP != null))
                    {
                        if ((thisPPP.CanFlip() == false) || (otherPPP.CanFlip() == false))
                        {
                            return false;
                        }
                        else
                        {
                            if (zindex == 0) { otherPPP.Flip(); } else { thisPPP.Flip(); }
                        }
                    }
                    else if (thisPPP != null)
                    {
                        //TODO Unless otherPPP is chairs and other dir ?
                        thisPPP.Flip();
                    }
                    else if (otherPPP != null)
                    {
                        otherPPP.Flip();
                    }
                }

                return false;
            }
        }

        return true;
    }

    public void PushDirection(CDirection cDirection, float adistance, bool isWeak = false, bool dDrag = true)
    {
        float distance = adistance;

        if (dDrag)
        {
            Vector2 md = GMManager.mouseDelta();
            if ((md.x > 0.1f) && (cDirection == CDirection.Left)) { distance -= Mathf.Abs(md.x); }
            if ((md.x < -0.1f) && (cDirection == CDirection.Right)) { distance -= Mathf.Abs(md.x); }
            if ((md.y > 0.1f) && (cDirection == CDirection.Down)) { distance -= Mathf.Abs(md.y); }
            if ((md.y < -0.1f) && (cDirection == CDirection.Up)) { distance -= Mathf.Abs(md.y); }
        }

        //If moving out return no
        float f = 1.0f;
        float c = Math.Min(distance, 16 * (Time.deltaTime * 60)) * 1 / f;
        for (int i = 0; i<c; i++)
        {
            List<GameMakerInterface> pushing = new List<GameMakerInterface>();
            //pushing.Add(gmi);

            if (CanPushDirection(cDirection, pushing, isWeak))
            {
                //Move all
                foreach (GameMakerInterface agmi in pushing)
                {
                    DraggableBox otherBox = agmi.GetComponent<DraggableBox>();

                    float ngmix = agmi.x; float ngmiy = agmi.y;
                    if (cDirection == CDirection.Up) { ngmiy += f; }
                    else if (cDirection == CDirection.Down) { ngmiy -= f; }
                    else if (cDirection == CDirection.Right) { ngmix += f; }
                    else if (cDirection == CDirection.Left) { ngmix -= f; }
                    agmi.x = ngmix; agmi.y = ngmiy;

                    Rect thisRect = GameMakerInterface.RectTransformToScreenSpace(gmi.rect);
                    thisRect.center = new Vector2(ngmix, ngmiy);
                }
            }
            else
            {
                if ((i * f/(Time.deltaTime * 60) > 4)&&(!isWeak)){
                    string sfxs = ((pieceCode == "b") || (pieceCode == "d")) ? "coin_drop" : "piece_hit";
                    SoundManager.Instance.PlaySFX(sfxs, volume: 0.12f);
                    //Animation
                    if ((cDirection == CDirection.Up)|| (cDirection == CDirection.Down))
                    {
                        spriteSize = new Vector2(baseSpriteSize.x * 1.65f, baseSpriteSize.x * 0.7f);
                    }
                    else
                    {
                        spriteSize = new Vector2(baseSpriteSize.x * 0.7f, baseSpriteSize.x * 1.65f);
                    }
                }
                //Update position
                wasPushed = true;

                return;
            }
        }
    }



    public bool CorrectRegister()
    {
        if (parcel == ParcelStatus.Unknown) { return false; }

        if ((row == -1)||(column == -1)){ return false;  }

        if (IsInside())
        {
            return ((parcel == ParcelStatus.RegisterIn) || (parcel == ParcelStatus.StayIn));
        }
        else
        {
            return (parcel == ParcelStatus.RegisterOut);
        }
    }

    public bool IsInside()
    {
        bool isOut = (puzzleController.outRows.Contains(row)) || (puzzleController.outColumns.Contains(column));
        //Debug.Log("isOut [" + column.ToString() + "," + row.ToString() + "] " + (isOut ? "yes" : "no"));
        return !isOut;
    }

    public void AdjustIfNeeded()
    {
        flagAdjust = true;
    }

    void AdjustZPosition()
    {
        if (flagAdjust == false) { return; }
        Debug.Log("Adjust (" + gmi.x + ")");
        List<GameMakerInterface> cols = gmi.CollisionOnPoint(gmi.x, gmi.y);

        foreach (GameMakerInterface agmi in cols)
        {
            if (agmi == gmi) { continue; }
            DraggableBox box = agmi.GetComponent<DraggableBox>();
            Debug.Log("Checkin");
            if (box.zindex == zindex)
            {
                Debug.Log("GASP");
                zindex = (zindex == 0) ? 1 : 0;
                break;
            }
        }

        if (cols.Count == 0) { zindex = 0; }

        flagAdjust = false;
    }

    public void Animate(bool ifInOut)
    {
        if ((ifInOut) && (CorrectRegister())) { return; }

        float f = ifInOut ? 2.12f : 1.22f;
        spriteSize = new Vector2(baseSpriteSize.x * f, baseSpriteSize.x * f);
    }    
    
}
