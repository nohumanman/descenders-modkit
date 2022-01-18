using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerIdentification;
using ModTool.Interface;

namespace SplitTimer{
	public class TrailTimer : ModBehaviour {
		public int current_checkpoint_num = 0;
		public string trail_name;
		[Tooltip("Make sure to include the start and finish as well!")]
		public GameObject checkpoints_objs;
		public GameObject boundrys_objs;
		public SplitTimerAPI splitTimerAPI;
		[System.NonSerialized]
		public List<Checkpoint> checkpoints = new List<Checkpoint>();
		[System.NonSerialized]
		public List<Boundry> boundrys = new List<Boundry>();
		public CheckpointUI checkpointUI;
		private SteamIntegration steamIntegration = new SteamIntegration();
		void Start(){
			foreach (Checkpoint checkpoint_obj in checkpoints_objs.GetComponentsInChildren<Checkpoint>()){
				checkpoints.Add(checkpoint_obj);
			}
			foreach (Boundry boundry_obj in boundrys_objs.GetComponentsInChildren<Boundry>()){
				boundrys.Add(boundry_obj);
			}
		}
		public void OnDeath(){
			SplitTimer.Instance.splitTimerApi.OnDeath(this);
		}
		public void OnBoundryEnter(){
			SplitTimer.Instance.splitTimerApi.OnBoundryEnter(this);
		}
		public void OnBoundryExit(){
			SplitTimer.Instance.splitTimerApi.OnBoundryExit(this);
		}
		public void OnCheckpointEnter(Checkpoint checkpoint){
			if (checkpoint.checkpointType == CheckpointType.start){
				current_checkpoint_num = 0;
			}
			SplitTimer.Instance.splitTimerApi.OnCheckpointEnter(this, checkpoint);
		}
		public void InvalidateTime(string message){
			Debug.LogWarning(message);
			checkpointUI.StopTimer();
			checkpointUI.primaryTimer.text = message;
		}
	}
}
