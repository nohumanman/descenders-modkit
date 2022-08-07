using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;

public class BikeSwitcherHandler : ModBehaviour {      
	public void ToEnduro()
	{
		Debug.Log("BikeSwitcherHandler - Switching to Enduro!");
		try{
			GameObject.Find("DescendersSplitTimerModLoaded").SendMessage("ToEnduro");
		}
		catch (System.NullReferenceException){
			Debug.Log("Bike Switcher DLL not present.");
		}
	}
	public void ToDownhill()
	{
		Debug.Log("BikeSwitcherHandler - Switching to Downhill!");
		try{
			GameObject.Find("DescendersSplitTimerModLoaded").SendMessage("ToDownhill");
		}
		catch (System.NullReferenceException){
			Debug.Log("Bike Switcher DLL not present.");
		}
	}
	public void ToHardtail()
	{
		Debug.Log("BikeSwitcherHandler - Switching to Hardtail!");
		try{
			GameObject.Find("DescendersSplitTimerModLoaded").SendMessage("ToHardtail");
		}
		catch (System.NullReferenceException){
			Debug.Log("Bike Switcher DLL not present.");
		}
	}
}
