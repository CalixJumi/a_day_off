using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteController : SceneController
{
    public PuzzleModel puzzleModel;

    public SpriteRenderer spriteDetail;
    public SpriteRenderer spriteDrawing;
    public Text text;
    public SpriteCollection sprites;
    public SpriteRenderer spriteDimBackground;


    public override void PerformEvent(CJEventType eventType, CJButton sender)
    {
        base.PerformEvent(eventType, sender);
        string sname = sender.buttonName;

        if (sname == "return")
        {
            puzzleModel.complete();
            LevelManager lm = (LevelManager)parentController;
            if (lm.mode != LevelManager.ShowingMode.Folder) {
                lm.ReloadLevelFiles();
            }

            SoundManager.Instance.PlaySFX("paper_close");
            spriteDrawing.sprite = null;
            DismissScene(true);
        }
    }

    public override void SceneDidAppear(bool animated = true)
    {
        base.SceneDidAppear(animated);
        ReloadData();
        if ((puzzleModel.text == "")||(puzzleModel.text == null))
        {
            spriteDimBackground.enabled = false;
        }

        if (puzzleModel.songName != "puzzle_song")
        {
            SoundManager.Instance.PlayMusic(puzzleModel.songName);
        }
    }

    public override void SceneDidDisappear(bool animated = true)
    {
        base.SceneDidDisappear(animated);
        if (puzzleModel.songName != "puzzle_song")
        {
            SoundManager.Instance.PlayMusic("levelselect_loop");
            SoundManager.Instance.PlayMusicNext("levelselect_song");
        }
    }

    public void ReloadData()
    {
        if (puzzleModel == null) { return; }
        text.text = puzzleModel.text;
        text.alignment = puzzleModel.alignment;
        Debug.Log("Filename: " + puzzleModel.puzzleFile.name);
        Debug.Log("Detail: " + puzzleModel.detailSpriteName());
        Debug.Log("Drawing: " + puzzleModel.drawingSpriteName());
        spriteDetail.sprite = sprites.FindSprite(puzzleModel.detailSpriteName());
        spriteDrawing.sprite = sprites.FindSprite(puzzleModel.drawingSpriteName());
    }
}
