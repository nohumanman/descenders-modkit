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
		public static List<TrailTimer> trailTimerInstances = new List<TrailTimer>();
		public GameObject boundrys_objs;
		public float timeCount;
		float staticTime;
		public SplitTimerAPI splitTimerAPI;
		[System.NonSerialized]
		public List<Checkpoint> checkpoints = new List<Checkpoint>();
		[System.NonSerialized]
		public List<Boundry> boundrys = new List<Boundry>();
		public CheckpointUI checkpointUI;
		private SteamIntegration steamIntegration = new SteamIntegration();
		public BikeType forcedBikeType;
		void Awake(){
			trailTimerInstances.Add(this);
		}
		void Update(){
			if (checkpointUI.shouldIncrement){
				timeCount += Time.deltaTime;
			}
			else{
				timeCount = staticTime;
			}
		}
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
		public void OnBoundryEnter(GameObject boundry){
			SplitTimer.Instance.splitTimerApi.OnBoundryEnter(this, boundry, timeCount);
		}
		public void OnBoundryExit(GameObject boundry){
			SplitTimer.Instance.splitTimerApi.OnBoundryExit(this, boundry, timeCount);
		}
		public void OnCheckpointEnter(Checkpoint checkpoint){
			staticTime = timeCount;
			if (checkpoint.checkpointType == CheckpointType.start){
				timeCount = 0;
				staticTime = 0;
				current_checkpoint_num = 0;
					if (forcedBikeType != BikeType.any){
					BikeSwitcherHandler x = new BikeSwitcherHandler();
					if (forcedBikeType == BikeType.downhill){
						x.ToDowhill();
					}
					else if (forcedBikeType == BikeType.enduro){
						x.ToEnduro();
					}
					else if (forcedBikeType == BikeType.hardtail){
						x.ToHardtail();
					}
				}
			}
			SplitTimer.Instance.splitTimerApi.OnCheckpointEnter(this, checkpoint, staticTime);
		}
		public void InvalidateTime(string message){
			Debug.LogWarning(message);
			checkpointUI.StopTimer();
			timeCount = 0;
			checkpointUI.primaryTimer.text = message;
		}
	}
	public enum BikeType{
		downhill,
		enduro,
		hardtail,
		any
	}
}
