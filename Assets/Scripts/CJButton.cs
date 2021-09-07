using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CJEventType { ButtonDownUp, ButtonDown }
public class CJButton : MonoBehaviour
{
    // Start is called before the first frame update
    public string buttonName;
    public SceneController controller;

    public Sprite spriteNormal;
    public Sprite spriteHighlight;
    public Sprite spriteDisabled;
    public Sprite spriteSelected;
    public bool isSelected = false;
    public bool isDisabled = false;
    public SpriteRenderer spriteRenderer;
    bool isMouseDown = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

    }

    // Update is called once per frame
    void ChangeSprite(Sprite sprite)
    {
        if (sprite == null) { return; }
        if (spriteRenderer == null) { return;  }
        spriteRenderer.sprite = sprite;
    }

    Vector2 opos = new Vector2();

    void Update()
    {
        Vector2 mp = GMManager.mousePosition();
        if (GMManager.Instance.globalMouseButton() == false) { 
            isMouseDown = false; 
        }else if (GMManager.PointDistance(opos.x, opos.y, mp.x, mp.y) > 16)
        {
            isMouseDown = false;
        }

        if (isDisabled)
        {
            ChangeSprite(spriteDisabled);
        }
        else if (isMouseDown)
        {
            ChangeSprite(spriteHighlight);
        }else if (isSelected)
        {
            ChangeSprite(spriteSelected);
        }
        else
        {
            ChangeSprite(spriteNormal);
        }
    }


    private void OnMouseDown()
    {
        isMouseDown = true;
        opos = GMManager.mousePosition();
    }

    private void OnMouseUp()
    {
        if (isMouseDown)
        {
            if (buttonName != null)
            {
                controller.PerformEvent(CJEventType.ButtonDownUp, this);
            }
        }
    }

    private void OnMouseExit()
    {
        isMouseDown = false;
    }


}
