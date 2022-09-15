using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ModTool.Interface;
public class TimerInfo : ModBehaviour {
    public GameObject boundaries;
    public GameObject startCheckpoint;
    public GameObject endCheckpoint;
    public GameObject leaderboardText;
    public GameObject autoLeaderboardText;
    [MenuItem("Tools/MatthewsTools/Split Timer/Disable MeshRenderer on boundaries")]
    public static void GlobalDisableMeshRenderer(){
        foreach(TimerInfo timerInf in FindObjectsOfType<TimerInfo>()){
            foreach(Transform boundary in timerInf.boundaries.transform){
                boundary.gameObject.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }
    [MenuItem("Tools/MatthewsTools/Split Timer/Enable MeshRenderer on boundaries")]
    public static void GlobalEnableMeshRenderer(){
        foreach(TimerInfo timerInf in FindObjectsOfType<TimerInfo>())
            foreach(Transform boundary in timerInf.boundaries.transform)
                boundary.gameObject.GetComponent<MeshRenderer>().enabled = true;
    }
    [MenuItem("Tools/MatthewsTools/Split Timer/Verify Script Config")]
    public static void VerifyScriptConfig(){
        int errors = 0;
        int warnings = 0;
        if (FindObjectOfType<APILoaderScript.ModLoader>() == null){
            Debug.LogWarning("APILoaderScript.ModLoader is not present!");
            warnings += 1;
        }
        foreach(TimerInfo timerInf in FindObjectsOfType<TimerInfo>()){
            if (timerInf.startCheckpoint == null){
                Debug.LogError("No startCheckpoint on TimerInfo!", timerInf.transform);
                errors += 1;
            }
            if (timerInf.endCheckpoint == null){
                Debug.LogError("No endCheckpoint on TimerInfo!", timerInf.transform);
                errors += 1;
            }
            if (timerInf.leaderboardText == null){
                Debug.LogWarning("No leaderboardText on TimerInfo!", timerInf.transform);
                warnings += 1;
            }
            if (timerInf.autoLeaderboardText == null){
                Debug.LogWarning("No autoLeaderboardText on TimerInfo!", timerInf.transform);
                warnings += 1;
            }
            if (timerInf.boundaries == null){
                Debug.LogError("No boundary gameobject on TimerInfo!", timerInf.transform);
                errors += 1;
            }
            else{
                foreach(Transform boundary in timerInf.boundaries.transform){
                    if (boundary.gameObject.GetComponent<MeshRenderer>() == null){
                        Debug.LogError("Boundary has no MeshRenderer!", boundary);
                        errors += 1;
                    }
                    else {
                        if (boundary.gameObject.GetComponent<MeshRenderer>().enabled){
                            Debug.LogWarning("MeshRenderer for boundary is enabled - consider Disabling all before export!", boundary);
                            warnings += 1;
                        }
                        if (boundary.gameObject.GetComponent<MeshRenderer>().sharedMaterial == null){
                            Debug.LogWarning("MeshRenderer has null material!", boundary);
                            warnings += 1;
                        }
                    }
                }
                foreach(Transform checkpoint in timerInf.endCheckpoint.transform.parent.transform){
                    if (checkpoint.gameObject.GetComponent<MeshRenderer>() == null){
                        Debug.LogError("Checkpoint has no MeshRenderer!", checkpoint);
                        errors += 1;
                    }
                    else {
                        if (checkpoint.gameObject.GetComponent<MeshRenderer>().enabled){
                            Debug.LogWarning("MeshRenderer for checkpoint is enabled - consider Disabling all before export!", checkpoint);
                            warnings += 1;
                        }
                        if (checkpoint.gameObject.GetComponent<MeshRenderer>().sharedMaterial == null){
                            Debug.LogWarning("MeshRenderer for checkpoint has null material!", checkpoint);
                            warnings += 1;
                        }
                    }
                }
            }
        }
        foreach(Camera cam in FindObjectsOfType<Camera>()){
            warnings += 1;
            Debug.LogWarning("Camera found! Remember cameras do NOT play well with Descenders!", cam);
        }
        if (errors == 0)
            Debug.Log("Scripts verified! (with " + warnings + " warnings)");
        else
            Debug.LogError("Scripts not verified!");
    }
    [MenuItem("Tools/MatthewsTools/Split Timer/Attempt auto-assign boundaries")]
    public static void AttemptBoundaryAutoAssign(){
        foreach(TimerInfo timerInf in FindObjectsOfType<TimerInfo>())
            if (timerInf.boundaries == null)
                foreach(Transform obj in timerInf.transform)
                    if (obj.name == "Boundaries")
                        timerInf.boundaries = obj.gameObject;
    }
    [MenuItem("Tools/MatthewsTools/Split Timer/Select all boundaries")]
    public static void SelectAllBoundaries(){
        List<GameObject> boundaries = new List<GameObject>();
        foreach(TimerInfo timerInf in FindObjectsOfType<TimerInfo>())
            foreach(Transform obj in timerInf.boundaries.transform)
                boundaries.Add(obj.gameObject);
        GameObject[] x = new GameObject[boundaries.Count];
        int i = 0;
        foreach(GameObject boundary in boundaries){
            x[i] = boundary;
            i++;
        }
        Selection.objects = x;
    }
}
