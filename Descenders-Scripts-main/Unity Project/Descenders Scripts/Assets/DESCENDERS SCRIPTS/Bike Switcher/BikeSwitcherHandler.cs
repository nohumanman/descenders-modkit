using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;

public class BikeSwitcherHandler : ModBehaviour {      
	public void ToEnduro()
	{
		GameObject.Find("loaderRockLeagu").SendMessage("ToEnduro");
	}
	public void ToDowhill()
	{
		GameObject.Find("loaderRockLeagu").SendMessage("ToDowhill");
	}
	public void ToHardtail()
	{
		GameObject.Find("loaderRockLeagu").SendMessage("ToHardtail");
	}
}
