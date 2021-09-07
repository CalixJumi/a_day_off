using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushPullPiece : MonoBehaviour
{
    float flipCD = 0;
    bool willPush = false;
    Vector2 ppos = new Vector2();
    GameMakerInterface gmi;
    DraggableBox draggableBox; 
    // Start is called before the first frame update
    void Start()
    {
        gmi = GetComponent<GameMakerInterface>();
        draggableBox = GetComponent<DraggableBox>();
    }

    // Update is called once per frame
    void Update()
    {
        OnMouseDownCheck();
        OnMouseDragCheck();
        OnMouseUpCheck();
        if (flipCD > 0) { flipCD -= 1; }
    }

    private void OnMouseDownCheck()
    {
        if (GMManager.Instance.globalMouseButtonPressed() == false) { return; }
        if (draggableBox.CanBeDragged() == false) { return; }
        //If someone else is on top, return;
        Vector3 mp3 = GMManager.mousePosition();
        Vector2 mp = new Vector2(mp3.x, mp3.y);
        Rect tr = GameMakerInterface.RectTransformToScreenSpace(gmi.rect);
        if (tr.Contains(mp) == false) { return; }
        willPush = true;
        ppos = new Vector2(gmi.x, gmi.y);
    }

    private void OnMouseDragCheck()
    {
        if ((Mathf.Abs(ppos.x - gmi.x) > 1)||(Mathf.Abs(ppos.y - gmi.y) > 1)){
            willPush = false;
        }
    }

    private void OnMouseUpCheck()
    {
        if (GMManager.Instance.globalMouseButtonReleased() == false) { return; }

        if (willPush)
        {
            if (CanFlip())
            {
                Flip();
            }
        }

        willPush = false;
    }

    public void Flip()
    {
        if (draggableBox.targetAlpha < 0.5) { return; }

        if (flipCD > 0) { return; }
        if (CanFlip() == false) { return;  }
        flipCD = 12;
        draggableBox.zindex = (draggableBox.zindex == 0) ? 1 : 0;

        SoundManager.Instance.PlaySFX("suction_pop", volume: 0.42f);
    }

    public bool CanFlip()
    {
        bool canChange = true;
        List<GameMakerInterface> colls = gmi.CollisionOnPoint(gmi.x, gmi.y);
        canChange = (colls.Count == 0);
        return canChange;
    }
}
