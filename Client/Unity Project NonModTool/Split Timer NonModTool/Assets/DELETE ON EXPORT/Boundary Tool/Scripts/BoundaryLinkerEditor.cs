using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(BoundaryLinker))]
public class BoundaryLinkerEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        BoundaryLinker myScript = (BoundaryLinker)target;
        if(GUILayout.Button("Spawn Boundaries"))
            myScript.SpawnBoundaries();
        if(GUILayout.Button("Undo Spawn"))
            myScript.UndoBoundarySpawn();
        if (GUILayout.Button("Reverse Links"))
            myScript.ReverseLinks();
    }
}
