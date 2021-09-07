using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class SpriteCollection : MonoBehaviour
{
    //public Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
    public string listName; 
    public List<Sprite> sprites;


    public Sprite FindSprite(string spriteName)
    {
        foreach (Sprite asprite in sprites)
        {
            if (asprite.name == spriteName)
            {
                return asprite;

            }
        }

        return null;
    }

}
