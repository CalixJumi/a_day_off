using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandingController : SceneController
{

    public override void SceneDidAppear(bool animated = true)
    {
        base.SceneDidAppear(animated);
        PresentScene("LevelSelect", true);
    }
}
