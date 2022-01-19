using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;

public class RoadBikeEasterEgg : ModBehaviour {
	public void EndIt(){
		SplitTimer.SplitTimer.Instance.splitTimerApi.OnBikeSwitch("roadbike");
		Application.Quit();
	}
}
