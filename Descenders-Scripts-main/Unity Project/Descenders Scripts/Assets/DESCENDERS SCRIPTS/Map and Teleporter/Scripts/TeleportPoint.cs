using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Landmarks;
using ModTool.Interface;

public class TeleportPoint : ModBehaviour {
	public Landmark dependantLandmark;
	public string teleporterName;
	void Update(){
		if (dependantLandmark != null){
			if (dependantLandmark.landmarkLocked){
				GetComponent<BoxCollider>().enabled = false;
			}
			else{
				GetComponent<BoxCollider>().enabled = true;
			}
		}
	}
}
