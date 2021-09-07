using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PuzzleController))]
[CanEditMultipleObjects]
public class PuzzleManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        /*PuzzleController myController = (PuzzleController)target;
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("sceneAnimator"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("outroAnimators"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("pieces"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("collisionManager"), true);
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Puzzle");
        if (GUILayout.Button("Refresh"))
        {
            myController.RefreshPuzzle();
        }

        EditorGUILayout.EndHorizontal();

        //myController.puzzleModel.puzzleCode = EditorGUILayout.TextArea(myController.puzzleModel.puzzleCode);

            //EditorGUILayout.IntField(board.columns, GUILayout.Width(22));
            */
    }
}
