using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionManager : MonoBehaviour
{
    public List<GameMakerInterface> collisionObjects;
    public bool shouldRemapCollisionObjects = true;
    // Start is called before the first frame update
    void Start()
    {
     
    }

    // Update is called once per frame
    void Update()
    {
        if (shouldRemapCollisionObjects == true){
            MapCollisionObjects();
        }
    }

    public void MapCollisionObjects()
    {
        collisionObjects = new List<GameMakerInterface>();

        foreach (Transform child in transform)//collisionManager.transform)
        {
            //Debug.Log("Manager Transform has Child");
            GameMakerInterface other = child.GetComponent<GameMakerInterface>();
            if (other != null) { collisionObjects.Add(other); }
        }

        shouldRemapCollisionObjects = false;
    }

    void CheckCollisions(GameMakerInterface source)
    {
        foreach (GameMakerInterface child in transform)
        {
            if (child == source) { continue; }

        }
    }
}
