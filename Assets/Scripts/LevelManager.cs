using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using E7.Native;

public class LevelManager : SceneController
{
    public bool clickToComplete = false;
    // Start is called before the first frame update
    public TextAsset progressionFile;
    public List<TextAsset> puzzleFiles;
    static public List<PuzzleModel> puzzleModels;
    public List<PuzzleModel> availablePuzzleModels;

    public TextAsset selectedPuzzleFile;
    static PuzzleModel selectedPuzzleModel;
    public EnvelopeModel selectedEnvelope;

    public Transform envelopePrefab;
    public Transform spacePrefab;
    public List<CJButton> buttons = new List<CJButton>();
    public Transform contentView;
    public ScrollRect scrollView;
    public Animator animatorCanvasUI;
    public CJButton buttonTray;
    public CJButton buttonArchive;
    public CJButton buttonFolder;
    public CJButton buttonDebug;

    bool isDebug = false;

    bool showCarousel = false;

    public List<int> completedLevels = new List<int>();

    public enum ShowingMode { Tray, Archive, Folder, None }

    static ShowingMode savedMode = ShowingMode.None;
    static float savedScrollPosition = 1;
    public ShowingMode mode = ShowingMode.None;

    void SetMode(ShowingMode amode)
    {
        mode = amode;
        savedMode = amode;
        savedScrollPosition = scrollView.verticalNormalizedPosition;
    }

    public float yoffset = 160;
    public float heightEnvelope = 160 - 12;

    public override void SceneDidLoad()
    {
        base.SceneDidLoad();

        LoadPuzzleModels();
        GMManager.CreateInstance();

        if (!Debug.isDebugBuild && (buttonDebug != null))
        { buttonDebug.gameObject.SetActive(false); }


        PuzzleModel pl = LevelManager.selectedPuzzleModel;
        if (pl != null) {
            Debug.Log("Puzzle: " + pl.puzzleFile.name + ", completed: " + pl.isCompleted.ToString());
            if (pl.isCompleted == true)
            {
                animatorCanvasUI.SetBool("completed", true);
                SoundManager.Instance.PlaySFX("tinbox_open", volume: 0.42f, delay: 0.52f);// : "tinbox_close", volume: 0.52f);
                SoundManager.Instance.PlaySFX("tinbox_close", volume: 0.40f, delay: 0.64f);

            }
            selectedEnvelope.puzzleModel = pl;
            selectedEnvelope.UpdateContent();
        }
    }

    public override void SceneDidAppear(bool animated = true)
    {
        base.SceneDidAppear(animated);

        string songName = "levelselect_song";

        Debug.Log("DidAppear");
        bool playItNow = true;
        if (SoundManager.Instance.MusicSource.clip != null)
        {
            string sname = SoundManager.Instance.MusicSource.clip.name;
            if ((sname == "puzzle_making")||(sname == "puzzle_train_full")||(sname == "puzzle_action")||(sname == "sfx_wind"))
            {
                songName = sname;
            }else if (sname == "levelselect_loop")
            {
                playItNow = false;
            }
        }

        if (playItNow) { SoundManager.Instance.PlayMusic(songName); }
        else { SoundManager.Instance.PlayMusicNext(songName); }
        
        SoundManager.Instance.PlaySFX("door_open");        

        mode = savedMode;
        ReloadLevelFiles();
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollView.GetComponent<RectTransform>());       
        scrollView.verticalNormalizedPosition = savedScrollPosition;

        //Check if needs to auto show anything
        //PlayerPrefs.SetInt(puzzleModel.roomFileName, 1);
        PuzzleModel am = puzzlesInTray[0];
        if (am.puzzleFile.name == "s-intro_gaby") {            
            PresentPuzzle(am);
        } else {
            int amode = PlayerPrefs.GetInt("mode");
            if ((amode == 1)&&(mode != ShowingMode.Folder)) { ShowMode(ShowingMode.Folder); }
            if ((amode == 2) && (mode != ShowingMode.Tray)) { ShowMode(ShowingMode.Tray); }
            if ((amode == 3) && (mode != ShowingMode.Archive)) { ShowMode(ShowingMode.Archive); }

        }
    }

    public override void PerformEvent(CJEventType eventType, CJButton sender)
    {

        if (childController != null) { return; }
        base.PerformEvent(eventType, sender);
        string sname = sender.buttonName;

        if (sname == "folder")
        {
            ShowMode(ShowingMode.Folder);
        }
        else if (sname == "tray")
        {
            ShowMode(ShowingMode.Tray);
        }
        else if (sname == "archive")
        {
            ShowMode(ShowingMode.Archive);
        }
        else if (sname == "options")
        {            
            SoundManager.Instance.PlaySFX("sfx_clip", volume: 0.52f);
            PresentScene("OptionsHomeScene", true, true);
        }else if (sname == "debug")
        {
            isDebug = !isDebug;
            buttonDebug.spriteRenderer.color = new Color(1, 1, 1, isDebug ? 1 : 0.45f);
        } else
        {
            int index = buttons.IndexOf(sender);
            PuzzleModel am = availablePuzzleModels[index];
            PresentPuzzle(am);
        }
    }

    void ShowMode(ShowingMode nmode, bool saveState = true)
    {
        ShowingMode amode = (mode == nmode) ? ShowingMode.None : nmode;
        StartCoroutine(EnvelopesOutAnimation(showCarousel ? 0.02f : 0.22f, amode));

        if (amode == ShowingMode.Folder)
        {
            if (saveState) { PlayerPrefs.SetInt("mode", 1); }
            SoundManager.Instance.PlaySFX((amode != ShowingMode.Folder) ? "paper_open" : "paper_close", volume: 0.52f);
        }
        else if (amode == ShowingMode.Tray)
        {
            if (saveState) { PlayerPrefs.SetInt("mode", 2); }
            SoundManager.Instance.PlaySFX((amode != ShowingMode.Tray) ? "paper_open" : "paper_close", volume: 0.52f);
        }
        else if (amode == ShowingMode.Archive)
        {
            if (saveState) { PlayerPrefs.SetInt("mode", 3); }
            SoundManager.Instance.PlaySFX((amode != ShowingMode.Archive) ? "tinbox_open" : "tinbox_close", volume: 0.52f);
        }
        else
        {
            if (saveState) { PlayerPrefs.SetInt("mode", 0); }
        }
        
    }

    void PresentPuzzle(PuzzleModel am)
    {
        selectedPuzzleFile = am.puzzleFile;

        if (clickToComplete)
        {
            am.complete();
            ReloadLevelFiles();
            return;
        }

        savedScrollPosition = (buttons.Count <= 1) ? 1 : scrollView.verticalNormalizedPosition;
        LevelManager.selectedPuzzleModel = am;

        string sscene = "PuzzleScene";

        if (am.storyScene != null)
        {
            sscene = am.storyScene;
        }
        else if (am.isStory())
        {
            sscene = "NoteScene";
        }

        bool isModal = am.isStory();
        if (!isModal) { EraseLevels(); }

        SoundManager.Instance.PlaySFX("paper_open", volume: 0.72f);
        SoundManager.Instance.PlaySFX("paper_transition", volume: 0.22f);

        if (mode != ShowingMode.Folder)
        {
            SoundManager.Instance.PlaySFX("puzzle_goto", volume: 0.72f);
        }

        PresentScene(sscene, true, isModal);
    }

    IEnumerator EnvelopesOutAnimation(float time, ShowingMode amode)
    {
        showCarousel = false;
        yield return new WaitForSeconds(time);
        SetMode(amode);
        ReloadLevelFiles();

        scrollView.verticalNormalizedPosition = 1;
    }

    //bool wasDragging = false;

    float ypaperMark = 0;
    float noInteractionCount = 0;
    Vector3 trayScale = new Vector3(0.6f, 0.6f, 0.6f);
    public override void Update()
    {
        base.Update();

        float fp = 3 / (float)scrollView.content.transform.childCount;
        float np = (float)scrollView.verticalNormalizedPosition;
        if (Mathf.Abs(np - ypaperMark) > fp)
        {
            if (ypaperMark > np) { 
                ypaperMark -= fp;
                //SoundManager.Instance.PlaySFX("coin_drop", volume: 0.11f, unique: true);
            } else { 
                ypaperMark += fp;
                //SoundManager.Instance.PlaySFX("coin_take", volume: 0.11f, unique: true);
            }

        }

        if (!showCarousel)
        {
            float yy = (scrollView.transform.position.y - 540)/2;
            scrollView.transform.position = new Vector3(0, yy, 0);
            if (scrollView.verticalNormalizedPosition < 1) {
                scrollView.verticalNormalizedPosition = 1;
            }

        }
        else
        {
            float yy = (scrollView.transform.position.y - 0) / 2;
            scrollView.transform.position = new Vector3(0, yy, 0);
        }

        noInteractionCount += Time.deltaTime;
        if (GMManager.Instance.globalMouseButtonPressed()) { noInteractionCount = 0; }

        if (noInteractionCount > 5)
        {
            noInteractionCount -= 1.2f;
            if ((puzzlesInTray.Count > 0)&&(mode != ShowingMode.Tray))
            { trayScale = new Vector3(0.76f, 1.12f, 1); }
        }

        trayScale = (new Vector3(0.6f, 0.6f, 0.6f) * 0.62f + trayScale * 0.38f) + (trayScale - new Vector3(0.6f, 0.6f, 0.6f)) * 0.18f;
        buttonTray.transform.localScale = (buttonTray.transform.localScale + trayScale)/2;
    }

    public void LoadPuzzleModels()
    {
        if (puzzleModels != null) { return; }

        //Break each line
        string filetext = progressionFile.text.Replace("\n", "\r");
        string[] lines = filetext.Split('\r');


        if (lines.Length == 0) {
            LevelManager.puzzleModels = new List<PuzzleModel>();
            return; 
        }

        List<PuzzleModel> pm = new List<PuzzleModel>();

        foreach (string ss in lines)
        {
            if (ss.Length < 3) { continue; }
            if (ss.Substring(0,2) == "//") { continue; }

            string s = System.IO.Path.GetFileNameWithoutExtension(ss);
            TextAsset textAsset = puzzleFiles.Find(p => (p.name == s));
            if (textAsset != null)
            {
                PuzzleModel apuzzleModel = new PuzzleModel(textAsset);
                pm.Add(apuzzleModel);
            }
        }
        //For each line, find an asset in puzzleFiles
        //Make it and add it

        LevelManager.puzzleModels = pm;

    }

    public void EraseLevels()
    {
        if (contentView == null) {
            Debug.Log("WHAT?");
        }

        List<Transform> children = new List<Transform>();
        foreach (Transform child in contentView.transform) { children.Add(child); }
        foreach (Transform child in children)
        {
            if (Application.isEditor) { DestroyImmediate(child.gameObject); }
            else
            {
                Destroy(child.gameObject);
            }
        }
        buttons = new List<CJButton>();
    }

    int envelopeCount = 0;

    List<PuzzleModel> puzzlesInTray = new List<PuzzleModel>();
    List<PuzzleModel> puzzlesInArchive = new List<PuzzleModel>();
    List<PuzzleModel> puzzlesInFolder = new List<PuzzleModel>();

    public void ReloadLevelFiles(bool scrollToTop = true)
    {
        //Destroy all levels
        EraseLevels();

        puzzlesInTray.Clear();
        puzzlesInArchive.Clear();
        puzzlesInFolder.Clear();

        foreach (PuzzleModel puzzleModel in puzzleModels)
        {
            bool isStory = puzzleModel.isStory();
            bool isCompleted = puzzleModel.isCompleted;
            if (!isCompleted) { puzzlesInTray.Add(puzzleModel); }
            else if (isStory){ puzzlesInFolder.Add(puzzleModel); }
            else { puzzlesInArchive.Add(puzzleModel); }
        }

        puzzlesInFolder.Reverse();
        puzzlesInArchive.Reverse();

        //Create them
        buttons = new List<CJButton>();
        availablePuzzleModels = new List<PuzzleModel>();

        if (mode == ShowingMode.Folder)
        {
            availablePuzzleModels = puzzlesInFolder;

        }else if (mode == ShowingMode.Archive)
        {
            availablePuzzleModels = puzzlesInArchive;
        }
        else if (mode == ShowingMode.Tray)
        {
            int cmax = 3;
            foreach (PuzzleModel amodel in puzzlesInTray)
            {
                if (isDebug) { availablePuzzleModels.Add(amodel); continue; }                
                //If it's a version, but not story version, continue
                PuzzleModel sm = availablePuzzleModels.Find(p => ((p.roomNumber == amodel.roomNumber)&&(!p.isStory())));
                if ((sm != null)&&(!amodel.isStory())) {
                    if (amodel.roomLetter != "") { break; }
                    continue; 
                }

                //If still can add
                if (cmax > availablePuzzleModels.Count)
                {
                    availablePuzzleModels.Add(amodel);
                    //If letter: Break
                    if (amodel.roomLetter != "") { break; }
                } else { break; }

                if (amodel.roomNumber == 68) { break; }
            }
        }

        Transform aSpace = Instantiate(spacePrefab, new Vector3(), Quaternion.identity);
        aSpace.transform.SetParent(contentView.transform);

        var i = 0;
        foreach (PuzzleModel amodel in availablePuzzleModels)
        {
            Transform envelopeT = Instantiate(envelopePrefab, new Vector3(), Quaternion.identity);
            EnvelopeModel env = envelopeT.GetComponent<EnvelopeModel>();
            env.puzzleModel = amodel;
            env.UpdateContent();
            env.transform.SetParent(contentView.transform);
            CJButton ab = envelopeT.GetComponent<CJButton>();
            ab.controller = this;
            buttons.Add(ab);
            i += 1;
        }

        Transform bSpace = Instantiate(spacePrefab, new Vector3(), Quaternion.identity);
        bSpace.transform.SetParent(contentView.transform);

        envelopeCount = availablePuzzleModels.Count;
        showCarousel = true; //(envelopeCount != 0);

        if (envelopeCount == 0)
        {
            Transform cSpace = Instantiate(spacePrefab, new Vector3(), Quaternion.identity);
            cSpace.transform.SetParent(contentView.transform);
            Transform dSpace = Instantiate(spacePrefab, new Vector3(), Quaternion.identity);
            dSpace.transform.SetParent(contentView.transform);
        }

        scrollView.verticalScrollbar.handleRect.GetComponent<Image>().enabled = (envelopeCount != 0);

        buttonTray.isDisabled = (puzzlesInTray.Count == 0);
        buttonTray.isSelected = (mode == ShowingMode.Tray);
        buttonArchive.isSelected = (mode == ShowingMode.Archive);
        buttonFolder.isSelected = (mode == ShowingMode.Folder);

        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollView.GetComponent<RectTransform>());
        //Wait a frame?
        //scrollView.verticalNormalizedPosition = 1000;

    }


    public override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        base.OnSceneLoaded(scene, mode);
        Debug.Log("OnSceneLoaded: " + scene.name);

        GameObject[] roots = scene.GetRootGameObjects();
        List<GameObject> gos = new List<GameObject>(roots);
        GameObject gob = gos.Where(x => (x.GetComponent<PuzzleController>() != null)).SingleOrDefault();
        if (gob != null)
        {
            PuzzleController pm = gob.GetComponent<PuzzleController>();
            //pm.puzzleModel = new PuzzleModel(selectedPuzzleFile);
            pm.puzzleModel = LevelManager.selectedPuzzleModel;
        }

        GameObject nob = gos.Where(x => (x.GetComponent<NoteController>() != null)).SingleOrDefault();
        if (nob != null)
        {
            NoteController nm = nob.GetComponent<NoteController>();
            //nm.puzzleModel = new PuzzleModel(selectedPuzzleFile);
            nm.puzzleModel = LevelManager.selectedPuzzleModel;
            nm.puzzleModel.complete();
            //ReloadLevelFiles();
        }
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();

        PlayerPrefs.SetFloat("volume-music", SoundManager.Instance.MusicSource.volume);
        PlayerPrefs.SetFloat("volume-effects", SoundManager.Instance.EffectsSource.volume);

        puzzleModels = null;
        LoadPuzzleModels();
        ReloadLevelFiles();
    }
}
