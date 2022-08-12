using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;

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
}
