using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerIdentification;
using ModTool.Interface;

namespace SplitTimer{
	public class TrailTimer : ModBehaviour {
		public int current_checkpoint_num = 0;
		public string trail_name;
		[System.NonSerialized]
		public bool timeInvalid = false;
		[Tooltip("Make sure to include the start and finish as well!")]
		public GameObject checkpoints_objs;
		public GameObject borders_objs;
		[System.NonSerialized]
		public List<Checkpoint> checkpoints = new List<Checkpoint>();
		[System.NonSerialized]
		public List<Border> borders = new List<Border>();
		public CheckpointUI checkpointUI;
		private SteamIntegration steamIntegration = new SteamIntegration();
		void Start(){
			foreach (Checkpoint checkpoint_obj in checkpoints_objs.GetComponentsInChildren<Checkpoint>()){
				checkpoints.Add(checkpoint_obj);
			}
			foreach (Border border_obj in borders_objs.GetComponentsInChildren<Border>()){
				borders.Add(border_obj);
			}
		}
		public void InvalidateTime(string message){
			Debug.LogWarning(message);
			checkpointUI.StopTimer();
			checkpointUI.primaryTimer.text = message;
			timeInvalid = true;
		}
		public void EnteredCheckpoint(Checkpoint checkpoint){
			if (checkpoint.checkpointType == CheckpointType.start){
				Debug.Log("TrailTimer - Entered startline!");
				checkpointUI.RestartTimer();
				current_checkpoint_num = 0;
				timeInvalid = false;
			}
			else if (checkpoint.checkpointType == CheckpointType.intermediate){
				Debug.Log("TrailTimer - Entered checkpoint intermediate!");
				current_checkpoint_num++;
			}
			else if (checkpoint.checkpointType == CheckpointType.pause){
				Debug.LogWarning("TrailTimer - Entered pause!");
			}
			else if (checkpoint.checkpointType == CheckpointType.stop){
				Debug.Log("TrailTimer - Entered Finish Line!");
				checkpointUI.StopTimer();
				current_checkpoint_num++;
				if (current_checkpoint_num != checkpoints.Count-1){
					checkpointUI.primaryTimer.text = "INVALID; CHECKPOINTS SKIPPED!";
				}
			}
			if (!timeInvalid){
				SplitTimer splitTimer = SplitTimer.Instance.gameObject.GetComponent<SplitTimer>();
				splitTimer.api.EnterCheckpoint(
					trail_name,
					steamIntegration.getName(),
					steamIntegration.getSteamId(),
					current_checkpoint_num.ToString(),
					checkpoints.Count.ToString()
					);
				checkpointUI.EnterCheckpoint();
			}
		}
		public void TimeInvalidated(){
			current_checkpoint_num = 0;
			Debug.Log("TrailTimer - Time Invalidated!");
		}
	}
}
