using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RegisterPaper : MonoBehaviour
{
    public Sprite paperIn;
    public Sprite paperOut;
    public SpriteCollection spriteCollection;
    public SpriteRenderer spriteRendererPaper;
    public SpriteRenderer spriteRendererItem;
    public Text textCount;

    // Start is called before the first frame update
    void Start()
    {
        spriteRendererPaper = GetComponent<SpriteRenderer>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
