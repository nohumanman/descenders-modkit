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
		try{
			GameObject.Find("DescendersSplitTimerModLoaded").SendMessage("ToEnduro");
		}
		catch (System.NullReferenceException){
			Debug.Log("Bike Switcher DLL not present.");
		}
	}
	public void ToDowhill()
	{
		SplitTimer.SplitTimer.Instance.splitTimerApi.OnBikeSwitch("downhill");
		Debug.Log("BikeSwitcherHandler - Switching to Downhill!");
		try{
			GameObject.Find("DescendersSplitTimerModLoaded").SendMessage("ToDowhill");
		}
		catch (System.NullReferenceException){
			Debug.Log("Bike Switcher DLL not present.");
		}
	}
	public void ToHardtail()
	{
		SplitTimer.SplitTimer.Instance.splitTimerApi.OnBikeSwitch("hardtail");
		Debug.Log("BikeSwitcherHandler - Switching to Hardtail!");
		try{
			GameObject.Find("DescendersSplitTimerModLoaded").SendMessage("ToHardtail");
		}
		catch (System.NullReferenceException){
			Debug.Log("Bike Switcher DLL not present.");
		}
	}
}
