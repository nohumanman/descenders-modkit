using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;
using SplitTimer;

public class BikeSwitcherHandler : ModBehaviour {      
	public void ToEnduro()
	{
		SplitTimer.SplitTimer.Instance.splitTimerApi.OnBikeSwitch("enduro");
		Debug.Log("BikeSwitcherHandler - Switching to Enduro!");
		GameObject.Find("loaderRockLeagu").SendMessage("ToEnduro");
	}
	public void ToDowhill()
	{
		SplitTimer.SplitTimer.Instance.splitTimerApi.OnBikeSwitch("downhill");
		Debug.Log("BikeSwitcherHandler - Switching to Downhill!");
		GameObject.Find("loaderRockLeagu").SendMessage("ToDowhill");
	}
	public void ToHardtail()
	{
		SplitTimer.SplitTimer.Instance.splitTimerApi.OnBikeSwitch("hardtail");
		Debug.Log("BikeSwitcherHandler - Switching to Hardtail!");
		GameObject.Find("loaderRockLeagu").SendMessage("ToHardtail");
	}
}
