using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsController : SceneController
{
    public Slider sliderMusic;
    public Slider sliderSFX;

    public CJButton buttonDeleteProgress;
    public Text textDeleteProgress;
    public Text textSkip;

    public override void PerformEvent(CJEventType eventType, CJButton sender)
    {
        base.PerformEvent(eventType, sender);
        string sname = sender.buttonName;

        if (sname == "resume")
        {
            DismissScene(true);
            SoundManager.Instance.PlaySFX("sfx_clap");
        } else if (sname == "delete") {
            if (sender.isSelected)
            {
                LevelManager lm = (LevelManager)parentController;
                lm.ResetProgress();
                DismissScene(true);
                SoundManager.Instance.PlaySFX("magicpaper_destroy");
                textDeleteProgress.text = "Deleted!";
            }
            else
            {
                sender.isSelected = true;
                textDeleteProgress.text = "Sure?";
            }
        }
        else if (sname == "skip")
        {
            if (sender.isSelected)
            {
                PuzzleController pc = (PuzzleController)parentController;
                DismissScene(true);

                pc.puzzleModel.isCompleted = true;
                PlayerPrefs.SetInt(pc.puzzleModel.roomFileName, 1);
                pc.ConfigurableEvent("GotoScene", "");
                SoundManager.Instance.PlaySFX("magicpaper_destroy");
                textSkip.text = "Sent!";
            }
            else
            {
                sender.isSelected = true;
                textSkip.text = "Sure?";
            }
        }
    }

    bool soundValuesLoaded = false;
    public override void SceneDidLoad()
    {
        base.SceneDidLoad();
        Debug.Log("volumes music: " + SoundManager.Instance.MusicSource.volume.ToString());
        sliderMusic.value = SoundManager.Instance.MusicSource.volume;
        Debug.Log("volumes effects: " + SoundManager.Instance.EffectsSource.volume.ToString());
        sliderSFX.value = SoundManager.Instance.EffectsSource.volume;
        soundValuesLoaded = true;
    }

    public override void Update()
    {
        base.Update();

    }

    public void OnMouseUp()
    {
        buttonDeleteProgress.isSelected = false;
        dragCount = 1;
    }

    float dragCount = 1;

    public void ChangeValue(float value)
    {
        Debug.Log("Change value " + value.ToString());
        if (soundValuesLoaded == false) { return; }

        SoundManager.Instance.MusicSource.volume = sliderMusic.value;
        SoundManager.Instance.EffectsSource.volume = sliderSFX.value;
        PlayerPrefs.SetFloat("volume-music", sliderMusic.value);
        PlayerPrefs.SetFloat("volume-effects", sliderSFX.value);
        dragCount += Time.deltaTime;

        if (dragCount > 0.12){
            dragCount = 0;
            SoundManager.Instance.PlaySFX("piece_dragging");
        }
    }

}
