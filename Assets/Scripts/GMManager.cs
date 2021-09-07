using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using E7.Native;

public class GMManager : MonoBehaviour
{

    private static GMManager instance;
    public static GMManager Instance { get {  return instance; } }

    public static GMManager CreateInstance()
    {
        if (instance != null)
        {
            // Instance is not null. Please call CreateInstance once only!!!
            return instance;
        }

        GameObject go = new GameObject(typeof(GMManager).Name);
        instance = go.AddComponent<GMManager>();

        instance.OnCreated();

        DontDestroyOnLoad(go);

        NativeTouch.Start();


        return instance;

    }

    protected virtual void OnDestroy() { instance = null; }
    protected virtual void OnCreated() { }

    public static Vector2 mousePositionFuture()
    {
        Vector2 mp = new Vector2(mousePosition().x, mousePosition().y);
        return mp;        
        //return mp - mouseDelta() * 2;
    }

    static Vector2 previousMousePositionB;
    public static Vector2 previousMousePosition;
    public static Vector2 mouseDelta()
    {
        Vector2 v = new Vector2();

#if UNITY_IPHONE||UNITY_ANDROID
        if (Input.touchCount == 0)
        {
            Vector3 mp = mousePosition();
            v = previousMousePosition - new Vector2(mp.x, mp.y);
        }
        else
        {
            v = Input.touches[0].deltaPosition;
            //v = Input.GetTouch(0).deltaPosition;
            v.y = -v.y;
        }

#else
        v = Input.GetTouch(0).deltaPosition;
#endif    
        return v;
    }

    bool wasMouseButtonPressedB = false;
    bool wasMouseButtonPressed = false;

    public bool globalMouseButtonPressed()
    {
        return (wasMouseButtonPressed == false) && (globalMouseButton());
    }

    public bool globalMouseButtonReleased()
    {
        return (wasMouseButtonPressed == true) && (!globalMouseButton());
    }

    public bool globalMouseButton()
    {
#if UNITY_IPHONE
        return (Input.touchCount > 0) || Input.GetMouseButton(0);
#else
        return Input.GetMouseButton(0);
#endif

    }

    public static Vector3 mousePosition()
    {
        Vector3 orig = new Vector3();

#if UNITY_IPHONE||UNITY_ANDROID

        if (NativeTouch.Started)
        {
            while (NativeTouch.touches.TryGetAndMoveNext(out NativeTouchData ntd))
            {
                orig = new Vector3(ntd.X, ntd.Y, 0);
            }
        }
        else
        {
            if (Input.touchCount == 0)
            {
                orig = Input.mousePosition;
                //Debug.Log("Unity iPhone (Not really, using mouse) GetTouch: " + orig.x + ", " + orig.y);
            }
            else
            {
                //orig = Input.GetTouch(0).position;
                orig = Input.touches[0].position;

                //Debug.Log("Unity iPhone GetTouch: " + orig.x + ", " + orig.y);
            }
        }


            

#else
        orig = Input.mousePosition;
        Debug.Log("Unity Mouse GetTouch: " + orig.x + ", " + orig.y);
#endif
        orig = Camera.main.ScreenToWorldPoint(new Vector3(orig.x, orig.y, Camera.main.nearClipPlane));

        return orig;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        previousMousePosition = previousMousePositionB;
        previousMousePositionB = mousePosition();

        wasMouseButtonPressed = wasMouseButtonPressedB;
        wasMouseButtonPressedB = globalMouseButton();
        /*
        if (globalMouseButtonPressed())
        {
            Debug.Log("Global Mouse Button Pressed");
        }

        if (globalMouseButtonReleased())
        {
            Debug.Log("Global Mouse Button Released");
        }*/
    }

    public static float PointDistance(float x1, float y1, float x2, float y2)
    {
        return Mathf.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
    }

    public static List<GameMakerInterface> CollisionForPoint(CollisionManager collisionManager, float ax, float ay)
    {
        if (collisionManager == null) { return null; }
        List<GameMakerInterface> collisions = new List<GameMakerInterface>();
        List<GameMakerInterface> children = collisionManager.collisionObjects;

        foreach (GameMakerInterface other in children)
        {
            Rect arect = RectTransformToScreenSpace(other.rect);
            //Debug.Log("x:" + ax.ToString() + ", y:" + ay.ToString() + ", rect:" + arect.ToString());
            bool shareSpace = arect.Contains(new Vector2(ax, ay)); 
            if (shareSpace) { collisions.Add(other); }
        }
        return collisions;
    }

    public static Rect RectTransformToScreenSpace(RectTransform transform)
    {
        Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
        return new Rect((Vector2)transform.position - (size * 0.500f), size);
    }
}
