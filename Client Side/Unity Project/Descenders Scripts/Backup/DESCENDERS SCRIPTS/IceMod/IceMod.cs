using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;

public class IceMod : ModBehaviour {
	public void OnTriggerEnter(Collider other){
		if (other.transform.name == "Bike" && other.transform.root.name == "Player_Human")
		{
			GameObject.Find("DescendersSplitTimerModLoaded").SendMessage("EnableIce");
		}
	}
	public void OnTriggerExit(Collider other){
		if (other.transform.name == "Bike" && other.transform.root.name == "Player_Human")
		{
			GameObject.Find("DescendersSplitTimerModLoaded").SendMessage("DisableIce");
		}
	}
}
