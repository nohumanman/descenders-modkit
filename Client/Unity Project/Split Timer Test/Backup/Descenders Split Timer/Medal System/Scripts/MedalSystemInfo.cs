using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;
using UnityEditor;
public class MedalSystemInfo : ModBehaviour {
    [Header("Rainbow Medal")]
    public GameObject rainbowMedalGot;
    public GameObject rainbowMedalNotGot;
    [Header("Gold Medal")]
    public GameObject goldMedalGot;
    public GameObject goldMedalNotGot;
    [Header("Silver Medal")]
    public GameObject silverMedalGot;
    public GameObject silverMedalNotGot;
    [Header("Bronze Medal")]
    public GameObject bronzeMedalGot;
    public GameObject bronzeMedalNotGot;
    [Header("Config")]
    public string trailName;
    [MenuItem("Tools/DescCompTools/AutoAssignMedals")]
    public static void AttemptBoundaryAutoAssign(){
        foreach(MedalSystemInfo medalSystemInfo in FindObjectsOfType<MedalSystemInfo>()){
            foreach(Transform x in medalSystemInfo.gameObject.transform){
                GameObject q = (GameObject)x.gameObject;
                if (q.name == "RainbowMedal"){
                    medalSystemInfo.rainbowMedalGot = q.transform.GetChild(0).GetChild(0).gameObject;
                    medalSystemInfo.rainbowMedalNotGot = q.transform.GetChild(0).GetChild(1).gameObject;
                }
                if (q.name == "GoldMedal"){
                    medalSystemInfo.goldMedalGot = q.transform.GetChild(0).GetChild(0).gameObject;
                    medalSystemInfo.goldMedalNotGot = q.transform.GetChild(0).GetChild(1).gameObject;
                }
                if (q.name == "SilverMedal"){
                    medalSystemInfo.silverMedalGot = q.transform.GetChild(0).GetChild(0).gameObject;
                    medalSystemInfo.silverMedalNotGot = q.transform.GetChild(0).GetChild(1).gameObject;
                }
                if (q.name == "BronzeMedal"){
                    medalSystemInfo.bronzeMedalGot = q.transform.GetChild(0).GetChild(0).gameObject;
                    medalSystemInfo.bronzeMedalNotGot = q.transform.GetChild(0).GetChild(1).gameObject;
                }
            }
            medalSystemInfo.trailName = medalSystemInfo.gameObject.name;
        }
    }
}