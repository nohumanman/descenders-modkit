using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CableSpawner : EditorWindow {
    List<GameObject> selection = new List<GameObject>();
    public Object source;
    [MenuItem("Tools/MatthewsTools/Cable Spawner")]
    static void OpenWindow()
    {
        GetWindow<CableSpawner>();
    }
    void OnGUI()
    {
        GUILayout.Label(
            "\n1. Select each gameobject in sequence (use ctrl to select multiple).\n\n2. Put the gameobject for the cables below.",
            EditorStyles.wordWrappedLabel
        );
        source = EditorGUILayout.ObjectField("Cables", source, typeof(GameObject), true);
        GUILayout.Label(
            "3. Press the button below to spawn the cables.",
            EditorStyles.wordWrappedLabel
        );
        if (GUILayout.Button("Spawn based on selection")){
            GameObject parent = new GameObject();
            parent.name = "Cables";
            int i = 0;
            foreach(GameObject x in selection){
                if (i < selection.Count-1){
                    GameObject cable = SpawnCableBetween(x.transform.position, selection[i+1].transform.position);
                    cable.transform.SetParent(parent.transform);
                    cable.name = "Cable (" + i + ")";
                }
                i++;
            }
        }
        GUILayout.Label("\n\nThis must be done while this window is open, it will not work otherwise.");
    }
    GameObject SpawnCableBetween(Vector3 from, Vector3 to){
        Vector3 _center = from - to;
        float _distance = _center.magnitude;
        Vector3 _spawnPos = from - (_center / 2);
        GameObject cable = (GameObject)Instantiate(source);
        cable.transform.position = _spawnPos;
        cable.transform.LookAt(to);
        Vector3 _cableScale = cable.transform.localScale;
        cable.transform.localScale = new Vector3(
            _cableScale.x,
            _cableScale.y,
            _distance
        );
        return cable;
    }
    void OnSelectionChange()
    {
        foreach(GameObject x in Selection.gameObjects)
            if (!selection.Contains(x))
                selection.Add(x);
        if (selection.Count != Selection.gameObjects.Length)
            selection.Clear();
    }
}
