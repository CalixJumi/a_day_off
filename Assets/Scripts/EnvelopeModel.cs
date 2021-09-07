using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum RoomGroup { Intro, Simple, Complex, Clothes, Chairs, Music, Layers, Story }
public enum RoomType { Task, Advanced, Expert, Note}
public class PuzzleModel
{

    public TextAsset puzzleFile;
    public bool isCompleted = false;

    public RoomGroup roomGroup = RoomGroup.Intro;
    public RoomType roomType = RoomType.Task;
    public int roomNumber = 0;
    public string roomLetter = "";
    public int roomVersion = 0;
    public string roomFileName;
    public string roomPaperName;

    //Story ones
    public string text;
    public TextAnchor alignment = TextAnchor.MiddleCenter;
    public string listSprite;
    public string detailSprite;
    public string drawingSprite;
    public string storyScene;
    public string songName = "puzzle_song";
    public List<string> tags = new List<string>();

    public string puzzleCode;
    public List<string> registerIns = new List<string>();
    public List<string> registerOuts = new List<string>();

    public void complete()
    {
        isCompleted = true;
        PlayerPrefs.SetInt(roomFileName, 1);
    }
       
    public PuzzleModel(TextAsset puzzleFile)
    {

        this.puzzleFile = puzzleFile;
        bool isStory = (puzzleFile.name.Substring(0, 1) == "s");

        registerIns = new List<string>();
        registerOuts = new List<string>();
        roomFileName = System.IO.Path.GetFileNameWithoutExtension(puzzleFile.name + ".txt").ToLower().Substring(2);

        isCompleted = (PlayerPrefs.GetInt(roomFileName) > 0);
        string[] ptags = roomFileName.Split('-');


        if (isStory)
        {
            roomPaperName = ptags[0];
        }
        else
        {
            roomNumber = int.Parse(ptags[0]);
            string sub = ptags[1]; int anum = -1;
            if (int.TryParse(sub, out anum)) {
                roomVersion = anum;
            } else {
                roomLetter = sub.ToUpper();
                if (ptags.Length >= 3) {
                    roomVersion = int.Parse(ptags[2]);}
            }
        }


        string atext = puzzleFile.text;
        atext = atext.Replace(": ", ":").Replace("\n\n", "\n");
        string[] lines = atext.Split('\n');


        for (int i = 0; i < lines.Length; i+= 1)
        {
            string line = lines[i];
            if (line.Replace(" ", "") == "") { continue; }
            string[] comps = line.Split(':');
            if (comps.Length == 0) { continue; }

            string atag = comps[0];
            if (atag.Length < 2) { continue; }
            if (atag.Substring(0,2) == "--") { continue; }

            if (atag == "type")
            {
                string atype = comps[1];
                if (atype == "intro") { roomGroup = RoomGroup.Intro; }
                if (atype == "simple") { roomGroup = RoomGroup.Simple; }
                if (atype == "complex") { roomGroup = RoomGroup.Complex; }
                if (atype == "clothes") { roomGroup = RoomGroup.Clothes; }
                if (atype == "chairs") { roomGroup = RoomGroup.Chairs; }
                if (atype == "music") { roomGroup = RoomGroup.Music; }
                if (atype == "layers") { roomGroup = RoomGroup.Layers; }
                if (atype == "story") { roomGroup = RoomGroup.Story; }
            }
            else if (atag == "in")
            {
                registerIns = new List<string>(comps[1].Split(','));
            }
            else if (atag == "out")
            {
                registerOuts = new List<string>(comps[1].Split(','));
            }
            else if (atag == "puzzle")
            {
                string ss = "puzzle:\n";
                int iof = atext.IndexOf(ss, System.StringComparison.Ordinal);
                if (iof <= 0) { return; }
                puzzleCode = atext.Substring(iof + ss.Length);
            }else if (atag == "text")
            {
                if ((text == null)||(text == "")) { text = line.Replace("text:", ""); }
                else { text += "\n" + line.Replace("text:", ""); }
            }
            else if (atag == "icon")
            {
                listSprite = comps[1];
            }
            else if (atag == "sprite")
            {
                detailSprite = comps[1];
            }
            else if (atag == "drawing")
            {
                drawingSprite = comps[1];
            }
            else if (atag == "alignment")
            {
                string s = comps[1];
                if (s == "left") { alignment = TextAnchor.MiddleLeft; }
                if (s == "center") { alignment = TextAnchor.MiddleCenter; }
                if (s == "right") { alignment = TextAnchor.MiddleRight; }
                if (s == "top") { alignment = TextAnchor.UpperCenter; }
                if (s == "bottom") { alignment = TextAnchor.LowerCenter; }
                if (s == "top-left") { alignment = TextAnchor.UpperLeft; }
                if (s == "top-right") { alignment = TextAnchor.UpperRight; }
                if (s == "bottom-left") { alignment = TextAnchor.LowerLeft; }
                if (s == "bottom-right") { alignment = TextAnchor.LowerRight; }
            }else if (atag == "tags")
            {
                tags = new List<string>(comps[1].Split(','));
            }else if (atag == "song")
            {
                songName = comps[1];
            }
        }

    }

    public string roomSpriteName()
    {
        if (isStory()) { return ""; }
        return "bg-" + roomGroup.ToString().ToLower();
    }


    public string envelopeSpriteName()
    {
        if (listSprite != null) { return listSprite; }
        return "env-room";
    }

    public string drawingSpriteName()
    {
        return drawingSprite;
    }
    public string detailSpriteName()
    {
        return detailSprite;
    }

    public bool isStory()
    {
        return ((roomType == RoomType.Note) || (roomGroup == RoomGroup.Story));
    }

    public bool isFinal()
    {
        return tags.Contains("screenshake");
    }
}

public class EnvelopeModel : MonoBehaviour
{
    public SpriteCollection sprites;

    public SpriteRenderer spriteEnvelope;
    public SpriteRenderer spriteApproval;
    public SpriteRenderer spriteSeal;
    public SpriteRenderer spriteSignature;
    public SpriteRenderer spriteRoomSticker;
    public SpriteRenderer spriteType;
    public Text textRoomIdentifier;

    public SpriteRenderer[] spriteRs;


    public bool isFinished;
    public PuzzleModel puzzleModel;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateContent()
    {
        //Set each asset
        if (spriteEnvelope != null) { 
            spriteEnvelope.sprite = sprites.FindSprite(puzzleModel.envelopeSpriteName());
        }

        if (spriteRoomSticker != null) { spriteRoomSticker.sprite = sprites.FindSprite(puzzleModel.roomSpriteName()); }

        if ((textRoomIdentifier != null)&&(puzzleModel.isStory() == false)) { 

            textRoomIdentifier.text = puzzleModel.roomNumber.ToString() + puzzleModel.roomLetter.ToString();
        } else { textRoomIdentifier.text = ""; }


        Color acolor = Color.white;
        acolor.a = ((puzzleModel.isCompleted && !puzzleModel.isStory()) ? 1 : 0);

        if (spriteApproval != null) { spriteApproval.color = acolor; }
        if (spriteSeal != null) { spriteSeal.color = acolor; }
        if (spriteSignature != null) { spriteSignature.color = acolor; }

        int rv = puzzleModel.roomVersion;
        int i = 0;
        for (i = 0; i<spriteRs.Length; i += 1)
        {
            SpriteRenderer sr = spriteRs[i];
            sr.enabled = ((i < rv) && ( !(puzzleModel.isStory()) ));
        }

        bool hasIns = (puzzleModel.registerIns.Count > 0);
        bool hasOut = (puzzleModel.registerOuts.Count > 0);
        string stname = (hasIns && hasOut) ? "p_type1" : ((hasIns ? "p_type3" : "p_type2"));

        if (spriteType != null){
            spriteType.enabled = (hasIns || hasOut);
            spriteType.sprite = sprites.FindSprite(stname);
        }        
    }

    void FinishAndSeal()
    {
        //Make animation

        //Save into the system
    }
}
