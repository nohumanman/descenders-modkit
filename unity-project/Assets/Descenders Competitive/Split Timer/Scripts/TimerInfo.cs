using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ModTool.Interface;
using UnityEngine.SceneManagement;
using ModTool.Shared;


public class TimerInfo : ModBehaviour {
    public GameObject boundaries;
    public GameObject startCheckpoint;
    public GameObject endCheckpoint;
    public GameObject leaderboardText;
    public GameObject autoLeaderboardText;
    [MenuItem("Tools/Descenders Competitive/Boundaries/DisableMeshRenderer")]
    public static void GlobalDisableMeshRenderer(){
        foreach(TimerInfo timerInf in FindObjectsOfType<TimerInfo>()){
            foreach(Transform boundary in timerInf.boundaries.transform){
                boundary.gameObject.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }
    [MenuItem("Tools/Descenders Competitive/Boundaries/EnableMeshRenderers")]
    public static void GlobalEnableMeshRenderer(){
        foreach(TimerInfo timerInf in FindObjectsOfType<TimerInfo>())
            foreach(Transform boundary in timerInf.boundaries.transform)
                boundary.gameObject.GetComponent<MeshRenderer>().enabled = true;
    }
    
}
