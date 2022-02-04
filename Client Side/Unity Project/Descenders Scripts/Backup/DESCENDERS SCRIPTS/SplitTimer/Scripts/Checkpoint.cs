using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;

namespace SplitTimer{
	public class Checkpoint : ModBehaviour {
		public TrailTimer trailTimer;
		bool checkpointShown = false;
		public CheckpointType checkpointType;
		public void OnTriggerEnter(Collider other){
            if (other.transform.name == "Bike" && other.transform.root.name == "Player_Human")
            {
				trailTimer.OnCheckpointEnter(this);
			}
		}
		public void Update(){
			if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.Alpha0)){
				checkpointShown = !checkpointShown;
				ShowCheckpoint(checkpointShown);
			}
		}
		public void ShowCheckpoint(bool shouldShow = true){
			MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
			if (meshRenderer != null){
				meshRenderer.enabled = shouldShow;
			}
		}
	}
	public enum CheckpointType{
		start,
		intermediate,
		pause,
		stop
	}

}
