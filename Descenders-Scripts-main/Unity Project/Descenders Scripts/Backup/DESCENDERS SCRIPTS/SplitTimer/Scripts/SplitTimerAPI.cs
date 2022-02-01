using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using ModTool.Interface;
using PlayerIdentification;

namespace SplitTimer{
	public class SplitTimerAPI {
		public SteamIntegration steamIntegration = new SteamIntegration();
		public void OnMapEnter(SplitTimer splitTimer){
			if (ServerInfo.Instance.isOnline){
				splitTimer.StartCoroutine(CoroOnMapEnter());
				splitTimer.StartCoroutine(PeriodicallyCheckBanStatus(splitTimer));
			}
		}
		IEnumerator PeriodicallyCheckBanStatus(SplitTimer splitTimer){
			while (true){
				yield return new WaitForSeconds(10f);
				CheckBanStatus(splitTimer);
			}
		}
		public void CheckBanStatus(SplitTimer splitTimer){
			splitTimer.StartCoroutine(CoroCheckBanStatus(splitTimer));
		}
		IEnumerator CoroCheckBanStatus(SplitTimer splitTimer){
			if (ServerInfo.Instance.isOnline){
				using (
					UnityWebRequest webRequest =
					UnityWebRequest.Get(
						ServerInfo.Instance.server
						+ "/API/DESCENDERS/GET-BAN-STATUS"
						+ "?world_name="
						+ SplitTimer.Instance.world_name
						+ "&steam_name="
						+ steamIntegration.getName()
						+ "&steam_id="
						+ steamIntegration.getSteamId()
						)
				)
				{
					yield return webRequest.SendWebRequest();
					if (webRequest.downloadHandler.text != "valid"){
						foreach (TrailTimer trailTimer in TrailTimer.trailTimerInstances){
							SplitTimer.Instance.OnPlayerBanned(webRequest.downloadHandler.text);
						}
					}
				}
			}
		}
		public void OnBikeSwitch(string new_bike){
			if (ServerInfo.Instance.isOnline){
				SplitTimer.Instance.StartCoroutine(CoroOnBikeSwitch(new_bike));
			}
		}
		IEnumerator CoroOnBikeSwitch(string new_bike){
			using (
				UnityWebRequest webRequest =
				UnityWebRequest.Get(
					ServerInfo.Instance.server
					+ "/API/DESCENDERS/ON-BIKE-SWITCH"
					+ "?world_name="
					+ SplitTimer.Instance.world_name
					+ "&steam_name="
					+ steamIntegration.getName()
					+ "&steam_id="
					+ steamIntegration.getSteamId()
					+ "&new_bike="
					+ new_bike
					)
			)
			{
				yield return webRequest.SendWebRequest();
				if (webRequest.downloadHandler.text != "valid"){
					foreach (TrailTimer trailTimer in TrailTimer.trailTimerInstances){
						trailTimer.InvalidateTime(webRequest.downloadHandler.text);
					}
				}
			}
		}
		public void OnMapExit(){
			Debug.Log("SplitTimer.SplitTimerAPI - OnMapExit()!");
			if (ServerInfo.Instance.isOnline){
				IEnumerator e = CoroOnMapExit();
				if (e != null){
					if (!e.MoveNext()){
						e = null;
					}
				}
			}
		}
		public void OnCheckpointEnter(TrailTimer trailTimer, Checkpoint checkpoint, float time){
			Debug.Log("SplitTimer.SplitTimerAPI - OnCheckpointEnter()");
			if (ServerInfo.Instance.isOnline){
				trailTimer.StartCoroutine(CoroOnCheckpointEnter(trailTimer, checkpoint, time));
			}
		}
		public void OnDeath(TrailTimer trailTimer){
			Debug.Log("SplitTimer.SplitTimerAPI - OnDeath()");
			if (ServerInfo.Instance.isOnline){
				trailTimer.StartCoroutine(CoroOnDeath(trailTimer));
			}
		}
		public void OnBoundryEnter(TrailTimer trailTimer, GameObject boundry, float timeCount){
			Debug.Log("SplitTimer.SplitTimerAPI - OnBoundryEnter()");
			if (ServerInfo.Instance.isOnline){
				trailTimer.StartCoroutine(CoroOnBoundryEnter(trailTimer, boundry, timeCount));
			}
		}
		public void OnBoundryExit(TrailTimer trailTimer, GameObject boundry, float timeCount){
			Debug.Log("SplitTimer.SplitTimerAPI - OnBoundryEnter()");
			if (ServerInfo.Instance.isOnline){
				trailTimer.StartCoroutine(CoroOnBoundryExit(trailTimer, boundry, timeCount));
			}
		}
		IEnumerator CoroOnMapEnter(){
			using (
				UnityWebRequest webRequest =
				UnityWebRequest.Get(
					ServerInfo.Instance.server
					+ "/API/DESCENDERS/ON-MAP-ENTER"
					+ "?world_name="
					+ SplitTimer.Instance.world_name
					+ "&steam_name="
					+ steamIntegration.getName()
					+ "&steam_id="
					+ steamIntegration.getSteamId()
					)
			)
			{
				yield return webRequest.SendWebRequest();
				if (webRequest.downloadHandler.text != "valid"){
					SplitTimer.Instance.OnPlayerBanned(webRequest.downloadHandler.text);
				}
			}
		}
		IEnumerator CoroOnDeath(TrailTimer trailTimer){
			string trail_name = trailTimer.trail_name;
			using (
				UnityWebRequest webRequest =
				UnityWebRequest.Get(
					ServerInfo.Instance.server
					+ "/API/DESCENDERS/ON-DEATH"
					+ "?steam_name="
					+ steamIntegration.getName()
					+ "&steam_id="
					+ steamIntegration.getSteamId()
					+ "&world_name="
					+ SplitTimer.Instance.world_name
					+ "&trail_name="
					+ trail_name
					)
			)
			{
				yield return webRequest.SendWebRequest();
				if (webRequest.downloadHandler.text != "valid"){
					trailTimer.InvalidateTime("TIME INVALID; YOU RESPAWNED/DIED.");
				}
			}
		}
		IEnumerator CoroOnMapExit(){
			using (
				UnityWebRequest webRequest =
				UnityWebRequest.Get(
					ServerInfo.Instance.server
					+ "/API/DESCENDERS/ON-MAP-EXIT"
					+ "?steam_name="
					+ steamIntegration.getName()
					+ "&steam_id="
					+ steamIntegration.getSteamId()
					+ "&world_name="
					+ SplitTimer.Instance.world_name
					)
			)
			{
				yield return webRequest.SendWebRequest();
			}
		}

		IEnumerator CoroOnBoundryEnter(TrailTimer trailTimer, GameObject boundry, float timeCount){
			using (
				UnityWebRequest webRequest =
				UnityWebRequest.Get(
					ServerInfo.Instance.server
					+ "/API/DESCENDERS/ON-BOUNDRY-ENTER/" + boundry.GetInstanceID().ToString()
					+ "?trail_name="
					+ trailTimer.trail_name
					+ "&steam_name="
					+ steamIntegration.getName()
					+ "&steam_id="
					+ steamIntegration.getSteamId()
					+ "&world_name="
					+ SplitTimer.Instance.world_name
					+ "&client_time="
					+ timeCount.ToString()
					)
			)
			{
				yield return webRequest.SendWebRequest();
				if (webRequest.downloadHandler.text != "valid"){
					trailTimer.InvalidateTime(webRequest.downloadHandler.text);
				}
			}
		}
		IEnumerator CoroOnBoundryExit(TrailTimer trailTimer, GameObject boundry, float timeCount){
			using (
				UnityWebRequest webRequest =
				UnityWebRequest.Get(
					ServerInfo.Instance.server
					+ "/API/DESCENDERS/ON-BOUNDRY-EXIT/" + boundry.GetInstanceID().ToString()
					+ "?trail_name="
					+ trailTimer.trail_name
					+ "&steam_name="
					+ steamIntegration.getName()
					+ "&steam_id="
					+ steamIntegration.getSteamId()
					+ "&world_name="
					+ SplitTimer.Instance.world_name
					+ "&client_time="
					+ timeCount.ToString()
					)
			)
			{
				yield return webRequest.SendWebRequest();
				if (webRequest.downloadHandler.text != "valid"){
					trailTimer.InvalidateTime(webRequest.downloadHandler.text);
				}
			}
		}
		IEnumerator CoroOnCheckpointEnter(TrailTimer trailTimer, Checkpoint checkpoint, float time){
			string trail_name = trailTimer.trail_name;
			string checkpoint_num = trailTimer.current_checkpoint_num.ToString();
			string total_checkpoints = trailTimer.checkpoints.Count.ToString();
			string checkpoint_type = checkpoint.checkpointType.ToString();
			string time_string = time.ToString();
			
			using (
				UnityWebRequest webRequest =
				UnityWebRequest.Get(
					ServerInfo.Instance.server
					+ "/API/DESCENDERS/ON-CHECKPOINT-ENTER/"
					+ checkpoint_num.ToString()
					+ "?trail_name="
					+ trail_name
					+ "&steam_name="
					+ steamIntegration.getName()
					+ "&steam_id="
					+ steamIntegration.getSteamId()
					+ "&total_checkpoints="
					+ total_checkpoints.ToString()
					+ "&world_name="
					+ SplitTimer.Instance.world_name
					+ "&checkpoint_type="
					+ checkpoint_type
					+ "&client_time="
					+ time_string
				)
			)
			{
				yield return webRequest.SendWebRequest();
				if (webRequest.downloadHandler.text != "valid"){
					trailTimer.InvalidateTime(webRequest.downloadHandler.text);
				}
				else{
					if (checkpoint.checkpointType == CheckpointType.start){
						trailTimer.checkpointUI.OnStartCheckpoint();
					}
					else if (checkpoint.checkpointType == CheckpointType.intermediate){
						trailTimer.checkpointUI.OnIntermediateCheckpoint(time);
					}
					else if (checkpoint.checkpointType == CheckpointType.pause){
						trailTimer.checkpointUI.OnPauseCheckpoint();
					}
					else if (checkpoint.checkpointType == CheckpointType.stop){
						trailTimer.checkpointUI.OnFinishCheckpoint(time);
					}
					trailTimer.current_checkpoint_num++;
				}
				yield return null;
			}
		}
	}
}

