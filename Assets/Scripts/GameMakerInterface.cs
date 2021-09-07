using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMakerInterface : MonoBehaviour
{
    public RectTransform rect;

    public Vector2 prevPos = new Vector2(0,0);
    Vector2 previousPositionB = new Vector2(0, 0);


    public CollisionManager collisionManager;
    public SpriteRenderer spriteRenderer;

    public float speed
    {
        get { return (new Vector2(hspeed, vspeed)).magnitude; }
        set
        {

            hspeed = Mathf.Cos(direction * Mathf.Deg2Rad ) * value;
            vspeed = Mathf.Sin(direction * Mathf.Deg2Rad ) * value;
        }
    }

    public float direction
    {
        get { return (Vector2.SignedAngle( new Vector2(1,0), new Vector2(hspeed, vspeed))); }
        set
        {
            hspeed = Mathf.Cos(value * Mathf.Deg2Rad) * speed;
            vspeed = Mathf.Sin(value * Mathf.Deg2Rad) * speed;

        }
    }



    public float _x;
    public float _y;
    public float _z;
    public float _hspeed;
    public float _vspeed;
    public float _xScale;
    public float _yScale;
    public float _angle;

    public float x { get { return _x; } set { _x = value; UpdatePosition(); } }
    public float y { get { return _y; } set { _y = value; UpdatePosition(); } }
    public float z { get { return _z; } set { _z = value; UpdatePosition(); } }
    public float hspeed { get { return _hspeed; } set { _hspeed = value; UpdatePosition(); } }
    public float vspeed { get { return _vspeed; } set { _vspeed = value; UpdatePosition(); } }

    public float xScale { get { return _xScale; } set { _xScale = value; UpdatePosition(); } }
    public float yScale { get { return _yScale; } set { _yScale = value; UpdatePosition(); } }

    public float angle { get { return _angle; } set { _angle = value; UpdatePosition(); } }

    // Start is called before the first frame update
    bool gmi_started = false;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        x = transform.position.x;
        y = transform.position.y;
        z = transform.position.z;
        xScale = transform.localScale.x;
        yScale = transform.localScale.y;
        angle = transform.rotation.z;
        gmi_started = true;
        rect = GetComponent<RectTransform>();

    }

    // Update is called once per frame
    void Update()
    {


        prevPos = previousPositionB;
        previousPositionB = new Vector2(x, y);

        Step();
    }

    public void Step()
    {
        x += hspeed;
        y += vspeed;
    }

    public void UpdatePosition()
    {
        if (gmi_started == false) { return;  }
        transform.position = new Vector3(x, y, z);
        transform.localScale = new Vector3(xScale, yScale, transform.localScale.z);
        transform.eulerAngles = new Vector3(0, 0, angle);                       
    }
    public void MotionAdd(float dir, float s)
    {
        float a = Mathf.Deg2Rad * dir;
        hspeed += Mathf.Cos(a) * s;
        vspeed += Mathf.Sin(a) * s;
    }

    public void MotionSet(float dir, float s)
    {
        float a = Mathf.Deg2Rad * dir;
        hspeed = Mathf.Cos(a) * s;
        vspeed = Mathf.Sin(a) * s;

    }

    public bool CollisionAgainst(GameMakerInterface other)
    {

        return false;
    }

    public static Rect RectTransformToScreenSpace(RectTransform transform)
    {
        Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
        return new Rect((Vector2)transform.position - (size * 0.500f), size);
    }



    public List<GameMakerInterface> CollisionOnPoint(float ax, float ay)
    {
        if (collisionManager == null) { return null; }

        List<GameMakerInterface> collisions = new List<GameMakerInterface>();
        List<GameMakerInterface> children = collisionManager.collisionObjects;

        foreach (GameMakerInterface other in children)//collisionManager.transform)
        {
            if (other != this) {
                Rect thisRect = RectTransformToScreenSpace(rect);
                thisRect.center = new Vector2(ax, ay);
                thisRect.size = new Vector2(thisRect.size.x, thisRect.size.y);

                bool shareSpace = thisRect.Overlaps(RectTransformToScreenSpace(other.rect));
                if (shareSpace) { collisions.Add(other); }
            }
        }

        return collisions;
    }

    public Rect Frame()
    {
        Sprite asprite = spriteRenderer.sprite;
        Rect r = new Rect();
        float fx = (1 / asprite.pixelsPerUnit) * xScale;
        float fy = (1 / asprite.pixelsPerUnit) * yScale;
        r.width = asprite.rect.width * fx;
        r.height = asprite.rect.height * fy;
        r.x = x - asprite.pivot.x * fx;
        r.y = y - asprite.pivot.y * fy;
        return r;
    }

  

}
