using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;

namespace SplitTimer{
	public class Checkpoint : ModBehaviour {
		public TrailTimer trailTimer;
		public CheckpointType checkpointType;
		public void OnTriggerEnter(Collider other){
            if (other.transform.name == "Bike")
            {
				trailTimer.EnteredCheckpoint(this);
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
