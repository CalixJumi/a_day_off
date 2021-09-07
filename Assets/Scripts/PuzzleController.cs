using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;


public class PuzzleController : SceneController
{
    public string debugText = "";
    public List<Transform> pieces;
    public Dictionary<string, Transform> piecesDictionary;
    public Transform backWall;

    public Animator sceneAnimator;
    public Animator sceneUIAnimator;
    public SpriteCollection spriteCollectionPieces;
    public SpriteCollection spriteCollectionBackgrounds;

    public SpriteRenderer spriteRoof;
    public SpriteRenderer spriteBlackScreen;

    public Transform tableCanvas;
    public CanvasGroup groupPuzzle;
    public EnvelopeModel envelope;
    public EnvelopeModel envelopeIcon;

    public PuzzleModel puzzleModel;
    public RegisterPaper registerPaperPrefab;
    public bool isDismissing = false;

    public CJButton buttonSolve;

    //public string puzzleCode = "";
    //public string levelName = "1-C";
    //public List<string> piecesIn = new List<string>();
    //public List<string> piecesOut = new List<string>();

    ScreenStatus screenStatus = ScreenStatus.Waiting;
    public CollisionManager collisionManager;

    public List<DraggableBox> boxes = new List<DraggableBox>();

    enum ScreenStatus { Waiting, Starting, Active, Finishing, Finished }



    //public override void PerformEvent(CJEventType eventType, CJButton sender)
    bool finishing = false;
    public override void PerformEvent(CJEventType eventType, CJButton sender)
    {
        if (isDismissing) { return; }
        base.PerformEvent(eventType, sender);
        string sname = sender.buttonName;

        if (sname == "options")
        {
            PresentScene("OptionsScene", true, true);
            SoundManager.Instance.PlaySFX("sfx_clip", volume: 0.52f);
        }
        else if ((sname == "finish")||(sname == "solve"))
        {
            if (sname == "solve")
            {
                triggerShakeEnding();
                return;
            }

            if (finishing) { return; }
            finishing = true;
            sceneAnimator.SetTrigger("sealed");

            puzzleModel.isCompleted = true;
            PlayerPrefs.SetInt(puzzleModel.roomFileName, 1);

            StartCoroutine(GotoLevelSelection(1.28f));
            SoundManager.Instance.PlaySFX("puzzle_completed", volume: 0.72f);
            SoundManager.Instance.PlaySFX("sfx_pencil", volume: 0.86f, delay: 0.16f);
        }
        else if (sname == "restart")
        {
            ErasePuzzle();
            StartCoroutine(RefreshPuzzleAnimation(0.12f));
            SoundManager.Instance.PlaySFX("sfx_doubleclip", volume: 0.52f);
        }else if (sname == "back")
        {
            isDismissing = true;
            StartCoroutine(GotoLevelSelection(0.28f));
        }
    }

    // Start is called before the first frame update
    public override void SceneDidLoad()
    {
        //Construct piecesDictionary
        piecesDictionary = new Dictionary<string, Transform>();
        foreach (Transform box in pieces)
        {
            DraggableBox db = box.GetComponent<DraggableBox>();
            piecesDictionary.Add(db.pieceCode, box);
        }

        spriteRoof.sprite = spriteCollectionBackgrounds.FindSprite("backgrounds_" + puzzleModel.roomGroup.ToString().ToLower());

        if (!Debug.isDebugBuild && (buttonSolve != null))
        {
            buttonSolve.gameObject.SetActive(false);
        }
    }

    public override void SceneDidAppear(bool animated = true)
    {
        base.SceneDidAppear(animated);
        bool playItNow = true;
        if (SoundManager.Instance.MusicSource.clip != null)
        {
            string sname = SoundManager.Instance.MusicSource.clip.name;
            if ((puzzleModel.songName == "levelselect_song") && (sname == "levelselect_loop"))
            {
                playItNow = false;
            }
        }

        if (playItNow) { SoundManager.Instance.PlayMusic(puzzleModel.songName); }
        else { SoundManager.Instance.PlayMusicNext(puzzleModel.songName); }
        

        //Wait
        ErasePuzzle();
        StartCoroutine(RefreshPuzzleAnimation(0.12f));

        envelope.puzzleModel = puzzleModel;
        envelope.UpdateContent();
        envelopeIcon.puzzleModel = puzzleModel;
        envelopeIcon.UpdateContent();

        foreach( DraggableBox box in boxes)
        {
            box.Animate(false);
        }
    }

    // Update is called once per frame
    float noInteractionCount = 0;
    public override void Update()
    {
        debugText = "boxes: " + boxes.Count.ToString();
        if (screenStatus == ScreenStatus.Waiting) { StartPuzzle(); }
        ShowEnvelopeIfNeeded();

        noInteractionCount += Time.deltaTime;
        if (GMManager.Instance.globalMouseButtonPressed()){ noInteractionCount = 0; }

        if (noInteractionCount > 5)
        {
            noInteractionCount -= 1.2f;
            foreach (DraggableBox abox in boxes)
            {
                abox.Animate(true);
            }
        }


    }

    void StartPuzzle()
    {
        screenStatus = ScreenStatus.Starting;

        //Check each thing on the code

        screenStatus = ScreenStatus.Active;
    }

    IEnumerator RefreshPuzzleAnimation(float time)
    {
        yield return new WaitForSeconds(time);
        RefreshPuzzle();
    }

    IEnumerator GotoLevelSelection(float time)
    {
        yield return new WaitForSeconds(time * 2/3);
        ErasePuzzle();
        PresentScene("LevelSelect", true);
    }

    public bool IsEmpty(string c)
    {
        return ((c == "x") || (c == "-") || (c == "l"));
    }

    public void ErasePuzzle()
    {
        List<Transform> children = new List<Transform>();
        if (collisionManager == null) { return; }

        foreach (Transform child in collisionManager.transform) { children.Add(child); }
        foreach (Transform child in children)
        {
            if (Application.isEditor) { DestroyImmediate(child.gameObject); }
            else {
                DraggableBox abox = child.GetComponent<DraggableBox>();
                if (abox != null) { 
                    abox.HideAndDestroy();
                }
                else
                {
                    Destroy(child.gameObject);
                }

            }
        }

        boxes = new List<DraggableBox>();    
    }

    public void HidePuzzle()
    {
        foreach (Transform child in collisionManager.transform) {
            DraggableBox abox = child.GetComponent<DraggableBox>();
            if (abox != null) { 
                abox.targetAlpha = 0;
                abox.shakeFade = true;
                continue;
            }

            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = new Color(0,0,0, 0);
            }

            //abox.shakeFade = true;
            //TODO box.alphaChangeSpeed = 0.1; //make it slower to change
            //TODO make pizzabg alpha to 0 part of the animation
            //TODO targetAlpha for boxes and background piece
        }

        //Destroy all shadows?
    }

    public int rows = 0;
    public int columns = 0;
    public bool oddColumns = false;
    public bool oddRows = false;
    public List<int> outColumns = new List<int>();
    public List<int> outRows = new List<int>();


    public void RefreshPuzzle()
    {

        //Update Contents from PuzzleModel
        string puzzleCode = puzzleModel.puzzleCode;

        string result = Regex.Replace(puzzleCode, @"^\s*$[\r\n]*", string.Empty, RegexOptions.Multiline);
        string[] lines = result.Split('\n');
        if (lines.Length == 0) { return; }
        rows = lines.Length;
        columns = lines[0].Length;
        oddRows = ((rows % 2) == 1);
        oddColumns = ((columns % 2) == 1);

        List<Clothes> clothesb = new List<Clothes>();
        List<Clothes> clothesd = new List<Clothes>();
        List<CDirection> exitSides = new List<CDirection>();
        List<Transform> obs = new List<Transform>();

        //Make all inner objects

        for (int j = 0; j<lines.Length; j++) 
        {
            string line = lines[j]; 
            for (int i = 0; i < line.Length; i++)
            {
                float xpos = (-(columns - 1) * 32 + 64 * i);
                float ypos = ((rows - 1) * 32 - 64 * j);
                Vector3 iipos = new Vector3(xpos, ypos, transform.position.z);
                Vector3 ipos = new Vector3(xpos, ypos, transform.position.z);
                Transform nbox = null;
                Transform abox = null;
                string c = System.Convert.ToString(line[i]);

                //o - ignore
                if (c == "o") { }

                //y - yellow 1x1 box
                if (c == "y") {
                    nbox = Instantiate(piecesDictionary["y"], ipos, Quaternion.identity);
                }

                //r - red 2x1 box
                if (c == "r")
                {
                    ipos.x += 32; 
                    nbox = Instantiate(piecesDictionary["r"], ipos, Quaternion.identity);
                    line = line.Substring(0,i + 1) + "o" + line.Substring(i + 2,columns - i - 2);
                }
                //p - purple 1x2 box
                if (c == "p")
                {
                    ipos.y -= 32;
                    nbox = Instantiate(piecesDictionary["p"], ipos, Quaternion.identity);
                    string aline = lines[j + 1];
                    aline = aline.Substring(0, i) + "o" + aline.Substring(i + 1, columns - i - 1);
                    lines[j + 1] = aline;
                }

                //g - chairs above g-q
                if ((c == "g") || (c == "q"))
                {
                    if (i < line.Length - 1)
                    {
                        string nchar = line.Substring(i + 1, 1);
                        if ((nchar == "g") || (nchar == "q"))
                        {
                            ipos.x = xpos + 32; ipos.y = ypos; ipos.z = (nchar != c) ? 1 : 0;
                            nbox = Instantiate(piecesDictionary["g"], ipos, Quaternion.identity);
                            DraggableBox ddbox = nbox.GetComponent<DraggableBox>();
                            ddbox.zindex = (nchar == c) ? 0 : 1;
                            if (false)//(nchar != c)
                            {
                                string cchar = ((c == "g") ? "q" : "g");
                                line = line.Substring(0, i + 1) + cchar + line.Substring(i + 2, columns - i - 2);
                                ddbox.AdjustIfNeeded();
                            }
                        }
                    }

                    if (j < lines.Length - 1)
                    {
                        string nchar = lines[j + 1].Substring(i, 1);
                        if ((nchar == "g") || (nchar == "q"))
                        {
                            ipos.x = xpos; ipos.y = ypos - 32; ipos.z = (nchar != c) ? 1 : 0;
                            abox = Instantiate(piecesDictionary["q"], ipos, Quaternion.identity);
                            DraggableBox ddbox = abox.GetComponent<DraggableBox>();
                            ddbox.zindex = (nchar == c) ? 0 : 1;
                            if (false)//(nchar != c)
                            {
                                string aline = lines[j + 1];
                                string cchar = ((c == "g") ? "q" : "g");
                                aline = aline.Substring(0, i) + cchar + aline.Substring(i + 1, columns - i - 1);
                                lines[j + 1] = aline;
                                ddbox.AdjustIfNeeded();
                            }

                        }
                    }
                }

                //j - chairs above j-i
                if ((c == "j") || (c == "i"))
                {
                    if (i < line.Length - 1)
                    {
                        string nchar = line.Substring(i + 1, 1);
                        if ((nchar == "j") || (nchar == "i"))
                        {
                            ipos.x = xpos + 32; ipos.y = ypos; ipos.z = (nchar != c) ? 1 : 0;
                            nbox = Instantiate(piecesDictionary["g"], ipos, Quaternion.identity);
                            DraggableBox ddbox = nbox.GetComponent<DraggableBox>();
                            ddbox.zindex = (nchar == c) ? 0 : 1;
                            if (false)//(nchar == c)
                            {
                                string cchar = ((c == "j") ? "i" : "j");
                                line = line.Substring(0, i + 1) + cchar + line.Substring(i + 2, columns - i - 2);
                                ddbox.AdjustIfNeeded();
                            }
                        }
                    }

                    if (j < lines.Length - 1)
                    {
                        string nchar = lines[j + 1].Substring(i, 1);
                        if ((nchar == "j") || (nchar == "i"))
                        {
                            ipos.x = xpos; ipos.y = ypos - 32; ipos.z = (nchar != c) ? 1 : 0;
                            abox = Instantiate(piecesDictionary["q"], ipos, Quaternion.identity);
                            DraggableBox ddbox = abox.GetComponent<DraggableBox>();
                            ddbox.zindex = (nchar == c) ? 0 : 1;
                            if (false)//(nchar == c)
                            {
                                string aline = lines[j + 1];
                                string cchar = ((c == "j") ? "i" : "j");
                                aline = aline.Substring(0, i) + cchar + aline.Substring(i + 1, columns - i - 1);
                                lines[j + 1] = aline;
                                ddbox.AdjustIfNeeded();
                            }
                        }
                    }
                }

                //k- line chairs
                if (c == "k")
                {
                    if (i < line.Length - 1)
                    {
                        string nchar = line.Substring(i + 1, 1);
                        if (nchar == "k")
                        {
                            ipos.x = xpos + 32; ipos.y = ypos; ipos.z = (nchar != c) ? 1 : 0;
                            nbox = Instantiate(piecesDictionary["g"], ipos, Quaternion.identity);
                            abox = Instantiate(piecesDictionary["g"], ipos, Quaternion.identity);
                            DraggableBox ddbox = abox.GetComponent<DraggableBox>();
                            ddbox.AdjustIfNeeded();
                        }
                    }

                    if (j < lines.Length - 1)
                    {
                        string nchar = lines[j + 1].Substring(i, 1);
                        if (nchar == "k")
                        {
                            ipos.x = xpos; ipos.y = ypos - 32; ipos.z = (nchar != c) ? 1 : 0;
                            abox = Instantiate(piecesDictionary["q"], ipos, Quaternion.identity);
                            nbox = Instantiate(piecesDictionary["q"], ipos, Quaternion.identity);
                            DraggableBox ddbox = abox.GetComponent<DraggableBox>();
                            ddbox.AdjustIfNeeded();
                        }
                    }
                }

                //a- a big box
                if (c == "a"){
                    ipos.x = xpos + 32; ipos.y = ypos - 32;
                    nbox = Instantiate(piecesDictionary["a"], ipos, Quaternion.identity);
                    line = line.Substring(0, i + 1) + "o" + line.Substring(i + 2, columns - i - 2);
                    string aline = lines[j + 1];
                    aline = aline.Substring(0, i) + "oo" + aline.Substring(i + 2, columns - i - 2);
                    lines[j + 1] = aline;
                }


                //b - clothes
                if (c == "b"){
                    nbox = Instantiate(piecesDictionary["b"], ipos, Quaternion.identity);
                    clothesb.Add(nbox.GetComponent<Clothes>());
                }

                if (c == "d"){
                    nbox = Instantiate(piecesDictionary["d"], ipos, Quaternion.identity);
                    clothesd.Add(nbox.GetComponent<Clothes>());
                }

                //m - full turntable
                if (c == "m") { nbox = Instantiate(piecesDictionary["m"], ipos, Quaternion.identity); }

                //t - turnable
                if (c == "t") { nbox = Instantiate(piecesDictionary["t"], ipos, Quaternion.identity); }

                //v - vynil
                if (c == "v") { 
                    nbox = Instantiate(piecesDictionary["v"], ipos, Quaternion.identity);
                    Vector3 p = nbox.position; p.z = -3; nbox.position = p;
                }

                //c - suitcase
                if (c == "c") { nbox = Instantiate(piecesDictionary["c"], ipos, Quaternion.identity); }

                //s - surfboard
                if (c == "s"){
                    ipos.x += 32;
                    nbox = Instantiate(piecesDictionary["s"], ipos, Quaternion.identity);
                    line = line.Substring(0, i + 1) + "o" + line.Substring(i + 2, columns - i - 2);
                }

                //x - block

                if ((c == "x")||(c == "-")||(c == "l")){

                    //nbox = Instantiate(piecesDictionary["x"], ipos, Quaternion.identity);
                    if (c == "-")
                    {
                        if (outRows.Contains(j) == false) { outRows.Add(j); }
                        if (j == 0) { exitSides.Add(CDirection.Up); }
                        if (j == lines.Length - 1) { exitSides.Add(CDirection.Down); }
                    }
                    if (c == "l")
                    {
                        if (outColumns.Contains(i) == false) { outColumns.Add(i); }
                        if (i == 0) { exitSides.Add(CDirection.Left); }
                        if (i == line.Length - 1) { exitSides.Add(CDirection.Right); }
                    }

                } //else { }

                if (nbox != null) { obs.Add(nbox); }
                if (abox != null) { obs.Add(abox); }
            }
        }

        for (int j = 0; j < lines.Length; j++)
        {
            string line = lines[j];
            for (int i = 0; i < line.Length; i++)
            {
                float xpos = (-(columns - 1) * 32 + 64 * i);
                float ypos = ((rows - 1) * 32 - 64 * j);
                Vector3 iipos = new Vector3(xpos, ypos, transform.position.z);
                Vector3 ipos = new Vector3(xpos, ypos, transform.position.z);
                string c = System.Convert.ToString(line[i]);

                if ((c != "x") && (c != "-") && (c != "l"))
                {
                    Transform bw = Instantiate(backWall, iipos, Quaternion.identity);
                    if (outColumns.Contains(i) || outRows.Contains(j))
                    {
                        bw.GetComponent<SpriteRenderer>().sprite = spriteCollectionPieces.FindSprite("exitzone"); //("wall-w");
                    }
                    bw.SetParent(collisionManager.transform);
                }
            }
        }

        foreach (Transform box in obs) { SetupPiece(box); }

        bool emptyLeft = exitSides.Contains(CDirection.Left);
        bool emptyRight = exitSides.Contains(CDirection.Right);
        bool emptyUp = exitSides.Contains(CDirection.Up);
        bool emptyDown = exitSides.Contains(CDirection.Down);

        for (int j = 0; j < lines.Length; j++)
        {
            string line = lines[j];
            for (int i = 0; i < line.Length; i++)
            {
                float xpos = (-(columns - 1) * 32 + 64 * i);
                float ypos = ((rows - 1) * 32 - 64 * j);

                string c = System.Convert.ToString(line[i]);
                bool isEmpty = ((c == "x") || (c == "-") || (c == "l"));

                //Corners
                bool isTopLeft = (i == 0) && (j == 0) && (!isEmpty);
                bool isBotLeft = (i == 0) && (j == lines.Length - 1) && (!isEmpty);
                bool isTopRight = (i == line.Length - 1) && (j == 0) && (!isEmpty);
                bool isBotRight = ((i == line.Length - 1) && (j == lines.Length - 1) && (!isEmpty));

                bool isUnderEmpty = (j == 0) || ((j > 0) && (!isEmpty) && (IsEmpty(System.Convert.ToString(lines[j - 1][i]))));
                bool isAboveEmpty = (j == lines.Length - 1) || ((j < lines.Length - 1) && (!isEmpty) && (IsEmpty(System.Convert.ToString(lines[j + 1][i]))));
                bool isRightToEmpty = (i == 0) || ((i > 0) && (!isEmpty) && (IsEmpty(System.Convert.ToString(lines[j][i - 1]))));
                bool isLeftToEmpty = (i == line.Length - 1) || ((i < line.Length - 1) && (!isEmpty) && (IsEmpty(System.Convert.ToString(lines[j][i + 1]))));

                bool isTopLeftEmpty = (i == 0) || (j == 0);
                if (!isTopLeftEmpty) { isTopLeftEmpty = IsEmpty(System.Convert.ToString(lines[j - 1][i - 1])); }

                bool isTopRightEmpty = (i == line.Length - 1) || (j == 0);
                if (!isTopRightEmpty) { isTopRightEmpty = IsEmpty(System.Convert.ToString(lines[j - 1][i + 1])); }

                bool isBotLeftEmpty = (i == 0) || (j == lines.Length - 1);
                if (!isBotLeftEmpty) { isBotLeftEmpty = IsEmpty(System.Convert.ToString(lines[j + 1][i - 1])); }

                bool isBotRightEmpty = (i == line.Length - 1) || (j == lines.Length - 1);
                if (!isBotRightEmpty) { isBotRightEmpty = IsEmpty(System.Convert.ToString(lines[j + 1][i + 1])); }

                //Walls
                float fv = 48;// - (isEmpty ? 64 : 0);

                if (isRightToEmpty)
                {
                    Vector3 pos = new Vector3(xpos - fv, ypos, transform.position.z);
                    Transform awall = Instantiate(piecesDictionary["x"], pos, Quaternion.identity);
                    awall.localScale = new Vector3(1, 2, 1) * 0.75f * 0.5f;
                    SetupPiece(awall);
                    if ((emptyLeft)&&(i == 0)||(isEmpty)) { awall.GetComponent<SpriteMask>().enabled = false; }
                }

                if (isUnderEmpty)
                {
                    Vector3 pos = new Vector3(xpos, ypos + fv, transform.position.z);
                    Transform awall = Instantiate(piecesDictionary["x"], pos, Quaternion.identity);
                    awall.localScale = new Vector3(2, 1, 1) * 0.75f * 0.5f;
                    SetupPiece(awall);
                    if ((emptyUp)&&(j == 0)||(isEmpty)) { awall.GetComponent<SpriteMask>().enabled = false; }
                }


                if (isLeftToEmpty)
                {
                    Vector3 pos = new Vector3(xpos + fv, ypos, transform.position.z);
                    Transform awall = Instantiate(piecesDictionary["x"], pos, Quaternion.identity);
                    awall.localScale = new Vector3(1, 2, 1) * 0.75f * 0.5f;
                    SetupPiece(awall);
                    if ((emptyRight)&&(i >= line.Length - 1)||(isEmpty)) { awall.GetComponent<SpriteMask>().enabled = false; }
                }

                if (isAboveEmpty)
                {
                    Vector3 pos = new Vector3(xpos, ypos - fv, transform.position.z);
                    Transform awall = Instantiate(piecesDictionary["x"], pos, Quaternion.identity);
                    awall.localScale = new Vector3(2, 1, 1) * 0.75f * 0.5f;
                    SetupPiece(awall);
                    if ((emptyDown)&&(j >= lines.Length - 1)||(isEmpty)) { awall.GetComponent<SpriteMask>().enabled = false; }
                }

                bool isExit = (outColumns.Contains(i) || outRows.Contains(j));
                if (isExit) { continue; }

                bool doTopLeft = isTopLeft || (isTopLeftEmpty && isUnderEmpty && isRightToEmpty);
                bool doTopRight = isTopRight || (isTopRightEmpty && isUnderEmpty && isLeftToEmpty);
                bool doBotLeft = isBotLeft || (isBotLeftEmpty && isAboveEmpty && isRightToEmpty);
                bool doBotRight = isBotRight || (isBotRightEmpty && isAboveEmpty && isLeftToEmpty);



                if (doTopLeft && !isEmpty)
                {
                    Vector3 pos = new Vector3(xpos - fv, ypos + fv, transform.position.z);
                    Transform awall = Instantiate(piecesDictionary["x"], pos, Quaternion.identity);
                    awall.localScale = new Vector3(1, 1, 1) * 0.75f * 0.5f;
                    SetupPiece(awall);
                }

                if (doBotLeft && !isEmpty)
                {
                    Vector3 pos = new Vector3(xpos - fv, ypos - fv, transform.position.z);
                    Transform awall = Instantiate(piecesDictionary["x"], pos, Quaternion.identity);
                    awall.localScale = new Vector3(1, 1, 1) * 0.75f * 0.5f;
                    SetupPiece(awall);
                }

                if (doTopRight && !isEmpty)
                {
                    Vector3 pos = new Vector3(xpos + fv, ypos + fv, transform.position.z);
                    Transform awall = Instantiate(piecesDictionary["x"], pos, Quaternion.identity);
                    awall.localScale = new Vector3(1, 1, 1) * 0.75f * 0.5f;
                    SetupPiece(awall);
                }

                if (doBotRight && !isEmpty)
                {
                    Vector3 pos = new Vector3(xpos + fv, ypos - fv, transform.position.z);
                    Transform awall = Instantiate(piecesDictionary["x"], pos, Quaternion.identity);
                    awall.localScale = new Vector3(1, 1, 1) * 0.75f * 0.5f;
                    SetupPiece(awall);
                }
            }
        }


        foreach (Clothes acloth in clothesb) { acloth.groupClothes = clothesb; }
        foreach (Clothes acloth in clothesd) { acloth.groupClothes = clothesd; }

        collisionManager.shouldRemapCollisionObjects = true;


        //Make Register Papers
        float radd = 64;

        Dictionary<string, int> adictRin = new Dictionary<string, int>();
        foreach (string sin in puzzleModel.registerIns){ 
            if (adictRin.ContainsKey(sin)) { adictRin[sin] += 1; } else { adictRin.Add(sin, 1); }}
        Dictionary<string, int> adictRout = new Dictionary<string, int>();
        foreach (string sout in puzzleModel.registerOuts){
            if (adictRout.ContainsKey(sout)) { adictRout[sout] += 1; } else { adictRout.Add(sout, 1); }}

        float aiv = 0; float t = adictRin.Count + adictRout.Count;

        bool hasBoth = ((puzzleModel.registerIns.Count > 0) && (puzzleModel.registerOuts.Count > 0));
        t += hasBoth ? 0.5f : 1; aiv = hasBoth? 0.5f : 1; 

        foreach (KeyValuePair<string, int> pair in adictRin)
        {
            string reg = pair.Key; int v = pair.Value;
            Vector3 apos = new Vector3(-radd * t / 2 + aiv * radd, 96, -20); aiv += 1;
            Transform paperIn = Instantiate(registerPaperPrefab.transform, apos, Quaternion.identity);
            RegisterPaper rp = paperIn.GetComponent<RegisterPaper>();
            if (spriteCollectionPieces == null) { Debug.Log("No collection!"); }
            rp.spriteRendererPaper.sprite = rp.paperIn;
            rp.spriteRendererItem.sprite = spriteCollectionPieces.FindSprite(SpriteNameForCode(reg));
            rp.textCount.text = (v <= 1) ? "" : v.ToString();
            paperIn.SetParent(tableCanvas);
            paperIn.localPosition = apos;
        }

        if (hasBoth) { aiv += 0.5f; }

        foreach (KeyValuePair<string, int> pair in adictRout)
        {
            string reg = pair.Key; int v = pair.Value;
            Vector3 apos = new Vector3(-radd * t / 2 + aiv * radd, 96, -20); aiv += 1;
            Transform paperOut = Instantiate(registerPaperPrefab.transform, apos, Quaternion.identity);
            RegisterPaper rp = paperOut.GetComponent<RegisterPaper>();
            if (spriteCollectionPieces == null) { Debug.Log("No collection!"); }
            rp.spriteRendererPaper.sprite = rp.paperOut;
            rp.spriteRendererItem.sprite = spriteCollectionPieces.FindSprite(SpriteNameForCode(reg));
            rp.textCount.text = (v <= 1) ? "" : v.ToString();
            paperOut.SetParent(tableCanvas);
            paperOut.localPosition = apos;
        }
    }

    public string SpriteNameForCode(string code)
    {
        switch (code)
        {
            case "x": return "block-box";
            case "b": return "blue-clothes";
            case "d": return "purple-clothes";
            case "c": return "case-box";
            case "g": return "green-chairh";
            case "q": return "green-chairv";
            case "t": return "pink-table";
            case "m": return "pink-tablevynil";
            case "v": return "pink-vynil";
            case "p": return "purple-box";
            case "r": return "red-box";
            case "s": return "surf-board";
            case "y": return "yellow-box";
            case "a": return "big-box";        
        }
        return "";
    }

    public void SetupPiece(Transform nbox)
    {
        if (nbox != null)
        {
            nbox.SetParent(collisionManager.transform);
            GameMakerInterface gmi = nbox.GetComponent<GameMakerInterface>();
            gmi.collisionManager = collisionManager;
            DraggableBox box = nbox.GetComponent<DraggableBox>();
            box.puzzleController = this;

            if (box.pieceType != PieceType.Wall)
            {
                gmi.x = nbox.transform.position.x;
                gmi.y = nbox.transform.position.y;

                //box.baseSpriteSize = box.spriteRenderer.size;
                box.UpdateGridValue(); 
                Debug.Log("Fixing " + box.pieceCode + " to " + box.column.ToString() + "," + box.row.ToString() + ". rc:" + columns.ToString() + "-" + rows.ToString());
                Debug.Log("Size: " + box.baseSpriteSize.ToString());
                box.CheckParcelStatus(puzzleModel.registerIns, puzzleModel.registerOuts);
                boxes.Add(box);
            }
        }
    }

    override public void ConfigurableEvent(string eventName, string info)
    {
        if (eventName == "GotoScene")
        {
            StartCoroutine(GotoLevelSelection(0.28f));
        }
    }

    bool wasSolved = false;
    void ShowEnvelopeIfNeeded()
    {
        if (puzzleModel == null) { return; }

        //Ask all boxes
        int corrects = 0;
        bool isSolved = true;
        string cause = "";
        foreach (DraggableBox box in boxes)
        {
            if (IsEmpty(box.pieceCode)) {
                cause = cause + "empty: " + box.pieceCode;
                continue;  
            }
            bool isCounted = ((box.parcel == ParcelStatus.RegisterIn)||(box.parcel == ParcelStatus.RegisterOut));
            if (box.CorrectRegister())
            {
                if (isCounted) { 
                    corrects += 1;
                }
                else
                {
                    cause = cause + "no count: " + box.pieceCode;
                }
            }
            else if (!isCounted)//Counts neutral 
            {
                cause = cause + ", culprit: " + box.pieceCode + ", " + box.column.ToString() + "," + box.row.ToString();
                isSolved = false;
                break;
            }

        }

        int count = puzzleModel.registerIns.Count + puzzleModel.registerOuts.Count; 
        isSolved = isSolved && (corrects >= count) && (!isDismissing);

        if (puzzleModel.isFinal() && isSolved)
        {
            if (isDismissing) { return; }
            triggerShakeEnding();
            //Go back to Level Selection
            //Play grumble sonud
        }
        else
        {
            sceneAnimator.SetBool("solved", isSolved);
            debugText = "C/C: " + corrects.ToString() + "/" + count.ToString() + ", isSolved: " + isSolved.ToString() + ". " + cause;

            if ((isSolved) && (!wasSolved))
            {
                SoundManager.Instance.PlaySFX("puzzle_solved");
            }

            wasSolved = isSolved;
        }


    }

    void triggerShakeEnding()
    {
        sceneUIAnimator.SetBool("screenshake", true);
        puzzleModel.complete();
        StartCoroutine(GotoLevelSelection(12.48f));

        isDismissing = true;
        HidePuzzle();

        SoundManager.Instance.PlayMusic("sfx_collapse");
        SoundManager.Instance.PlayMusicNext("puzzle_train_full");

    }

}
