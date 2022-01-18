using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;

public class BikeSwitcherHandler : ModBehaviour {      
	public void ToEnduro()
	{
		GameObject.Find("loaderRockLeagu").SendMessage("ToEnduro");
		Debug.Log("BikeSwitcherHandler - Switching to Enduro!");
	}
	public void ToDowhill()
	{
		GameObject.Find("loaderRockLeagu").SendMessage("ToDowhill");
		Debug.Log("BikeSwitcherHandler - Switching to Downhill!");
	}
	public void ToHardtail()
	{
		GameObject.Find("loaderRockLeagu").SendMessage("ToHardtail");
		Debug.Log("BikeSwitcherHandler - Switching to Hardtail!");
	}
}
