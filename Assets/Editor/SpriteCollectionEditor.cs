using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpriteCollection))]
public class SpriteCollectionEditor : Editor
{
   /*public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();

        EditorGUILayout.LabelField("TEST");

        SpriteCollection collection = (SpriteCollection)target;
        //serializedObject.Update();

        //foreach (var stuff in myController)

        //serializedObject.ApplyModifiedProperties();
        int c = collection.sprites.Count;
        if (c == 0)
        {
            Debug.Log("Was Empty");
            collection.sprites = new Dictionary<string, Sprite>();
            collection.sprites.Add("New key", null);
            Debug.Log("Count " + collection.sprites.Count.ToString());
            EditorUtility.SetDirty(collection);
            PrefabUtility.RecordPrefabInstancePropertyModifications(target);
            //RecordPrefabInstancePropertyModifications(

            return;
        }

        if (GUILayout.Button("New Key"))
        {
            collection.sprites.Add("New key", null);
            return;
        }

        foreach (KeyValuePair<string, Sprite> vpair in collection.sprites)
        {
            //Now you can access the key and value both separately from this attachStat as:
            //Debug.Log(attachStat.Key);
            //Debug.Log(attachStat.Value);

            EditorGUILayout.BeginHorizontal();
            string akey = EditorGUILayout.TextField("", vpair.Key, GUILayout.Width(120));
            bool didChange = false;
            if (akey == "")
            {
                collection.sprites.Remove(vpair.Key);
                didChange = true;
            }

            //public static Object ObjectField(Rect position, Object obj, Type objType, bool allowSceneObjects);

            Sprite asprite = (Sprite)EditorGUILayout.ObjectField("", vpair.Value, typeof(Sprite), true);
            if (asprite != vpair.Value)
            {
                collection.sprites[akey] = asprite;
                didChange = true;
            }

            if ((akey != vpair.Key)&&(!collection.sprites.ContainsKey(akey)))
            {
                collection.sprites.Add(akey, asprite);
                collection.sprites.Remove(vpair.Key);
                didChange = true;
            }

            if (didChange)
            {
                EditorGUILayout.EndHorizontal();
                EditorUtility.SetDirty(collection);
                List<string> keyList = new List<string>(collection.sprites.Keys);
                Debug.Log("Did change (" + string.Join(",", keyList.ToArray() + ")"));
                PrefabUtility.RecordPrefabInstancePropertyModifications(target);

                return;
            }


            EditorGUILayout.EndHorizontal();
        }

        //serializedObject.ApplyModifiedProperties();
        return;
    }*/
}
