using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;
using UnityEngine.UI;
public class TimerCopierTemp : ModBehaviour {
    public Text textFrom;
    public Text textTo;
    void Update () {
        textTo.text = textFrom.text;
    }
}
