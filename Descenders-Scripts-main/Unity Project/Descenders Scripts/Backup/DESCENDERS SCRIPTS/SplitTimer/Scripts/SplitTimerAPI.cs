using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using ModTool.Interface;
using PlayerIdentification;

namespace SplitTimer{
	public class SplitTimerAPI {
		public SteamIntegration steamIntegration = new SteamIntegration();
		public string server = "https://descenders-api.nohumanman.com:8443";
		public void OnMapEnter(SplitTimer splitTimer){
			Debug.Log(
				"SplitTimer.SplitTimerAPI - OnMapEnter(). Steam Name '"
				+ steamIntegration.getName()
				+ "' and id '"
				+ steamIntegration.getSteamId()
				+ "'"
			);
			splitTimer.StartCoroutine(CoroOnMapEnter());
		}
		public void OnMapExit(){
			Debug.Log("SplitTimer.SplitTimerAPI - OnMapExit()!");
			IEnumerator e = CoroOnMapExit();
			if (e != null){
				if (!e.MoveNext()){
					e = null;
				}
			}
		}
		public void OnCheckpointEnter(TrailTimer trailTimer, Checkpoint checkpoint){
			Debug.Log("SplitTimer.SplitTimerAPI - OnCheckpointEnter()");
			trailTimer.StartCoroutine(CoroOnCheckpointEnter(trailTimer, checkpoint));
		}
		public void OnDeath(TrailTimer trailTimer){
			Debug.Log("SplitTimer.SplitTimerAPI - OnDeath()");
			trailTimer.StartCoroutine(CoroOnDeath(trailTimer));
		}
		public void OnBoundryEnter(TrailTimer trailTimer){
			Debug.Log("SplitTimer.SplitTimerAPI - OnBoundryEnter()");
			trailTimer.StartCoroutine(CoroOnBoundryEnter(trailTimer));
		}
		public void OnBoundryExit(TrailTimer trailTimer){
			Debug.Log("SplitTimer.SplitTimerAPI - OnBoundryEnter()");
			trailTimer.StartCoroutine(CoroOnBoundryExit(trailTimer));
		}
		IEnumerator CoroOnMapEnter(){
			using (
				UnityWebRequest webRequest =
				UnityWebRequest.Get(
					server
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
				if (webRequest.downloadHandler.text == "INVALID"){
					SplitTimer.Instance.OnPlayerBanned();
				}
			}
		}
		IEnumerator CoroOnDeath(TrailTimer trailTimer){
			string trail_name = trailTimer.trail_name;
			using (
				UnityWebRequest webRequest =
				UnityWebRequest.Get(
					server
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
					server
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

		IEnumerator CoroOnBoundryEnter(TrailTimer trailTimer){
			using (
				UnityWebRequest webRequest =
				UnityWebRequest.Get(
					server
					+ "/API/DESCENDERS/ON-BOUNDRY-ENTER"
					+ "?trail_name="
					+ trailTimer.trail_name
					+ "&steam_name="
					+ steamIntegration.getName()
					+ "&steam_id="
					+ steamIntegration.getSteamId()
					+ "&world_name="
					+ SplitTimer.Instance.world_name
					)
			)
			{
				yield return webRequest.SendWebRequest();
				if (webRequest.downloadHandler.text != "valid"){
					trailTimer.InvalidateTime(webRequest.downloadHandler.text);
				}
			}
		}
		IEnumerator CoroOnBoundryExit(TrailTimer trailTimer){
			using (
				UnityWebRequest webRequest =
				UnityWebRequest.Get(
					server
					+ "/API/DESCENDERS/ON-BOUNDRY-EXIT"
					+ "?trail_name="
					+ trailTimer.trail_name
					+ "&steam_name="
					+ steamIntegration.getName()
					+ "&steam_id="
					+ steamIntegration.getSteamId()
					+ "&world_name="
					+ SplitTimer.Instance.world_name
					)
			)
			{
				yield return webRequest.SendWebRequest();
				if (webRequest.downloadHandler.text != "valid"){
					trailTimer.InvalidateTime(webRequest.downloadHandler.text);
				}
			}
		}
		IEnumerator CoroOnCheckpointEnter(TrailTimer trailTimer, Checkpoint checkpoint){
			string trail_name = trailTimer.trail_name;
			string checkpoint_num = trailTimer.current_checkpoint_num.ToString();
			string total_checkpoints = trailTimer.checkpoints.Count.ToString();
			string checkpoint_type = checkpoint.checkpointType.ToString();
			
			using (
				UnityWebRequest webRequest =
				UnityWebRequest.Get(
					server
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
					)
			)
			{
				yield return webRequest.SendWebRequest();
				Debug.Log("Here!");
				Debug.Log(webRequest.downloadHandler.text);
				if (webRequest.downloadHandler.text != "valid"){
					trailTimer.InvalidateTime(webRequest.downloadHandler.text);
				}
				else{
					trailTimer.checkpointUI.EnterCheckpoint(checkpoint);
				}
				yield return null;
			}
		}
	}
}

