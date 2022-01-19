using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;
using UnityEngine.UI;
using UnityEngine.Networking;
using CustomUi;
using PlayerIdentification;

namespace SplitTimer{
	public class CheckpointUI : ModBehaviour {
		[System.NonSerialized]
		public float[] fastestTimes;
		public TrailTimer trailTimer;
		public Text primaryTimer;
		public Text checkpointTimer;
		public Text checkpointComparisonTimer;
		public CanvasGroup checkpointUi;
		public CanvasGroup greenFlash;
		public RawImage green;
		bool shouldIncrement;
		bool paused = false;
		private float timeCount;
		public bool isCheckpointUIEnabled = true;
		public UI ui = UI.Instance;
		public FastestSplitTimes fastest_split_times;
		Coroutine checkpointTimerDisable;
		Coroutine checkpointComparisonTimerDisable;
		Coroutine disableTimerCoroutine;
		GameObject player_human;
		Vector3 previous_position;

		public struct FastestSplitTimes{
			public float[] fastest_split_times;
		}
		void Start(){
			checkpointUi.alpha = 0;
			GetFastestTimes();
			StartCoroutine(KeepFastestTimesUpdated());
			ui = UI.Instance;
		}
		void Update () {
			if (player_human == null){
				player_human = GameObject.Find("Player_Human");
			}
			else{
				if (Vector3.Distance(player_human.transform.position, previous_position) > 6){
					trailTimer.OnDeath();
				}
				previous_position = player_human.transform.position;
			}
			if (shouldIncrement && !paused){
				timeCount += Time.deltaTime;
				primaryTimer.text = FormatTime(timeCount);
			}
			if ((Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha1))){
				isCheckpointUIEnabled = !isCheckpointUIEnabled;
				if (checkpointUi.alpha == 0){
					checkpointUi.alpha = 1;
				}
				else{
					checkpointUi.alpha = 0;
				}
			}
			if (ui.isShowing){
				checkpointUi.alpha = 0;
			}
		}
		IEnumerator KeepFastestTimesUpdated(){
			while (true){
				yield return new WaitForSeconds(10f);
				GetFastestTimes();
			}
		}
		public void StopCheckpointElementsTimeout(){
			if (checkpointTimerDisable != null){
				StopCoroutine(checkpointTimerDisable);
			}
			if (checkpointComparisonTimerDisable != null){
				StopCoroutine(checkpointComparisonTimerDisable);
			}
			if (disableTimerCoroutine != null){
				StopCoroutine(disableTimerCoroutine);
			}
		}
        private void EnableCheckpointElements()
        {
            checkpointTimer.gameObject.SetActive(true);
            checkpointComparisonTimer.gameObject.SetActive(true);
			checkpointUi.alpha = 1;
        }
		
		public void OnStartCheckpoint(){
            StopCheckpointElementsTimeout();
            EnableCheckpointElements();
			checkpointTimer.text = "Yours: 00:00:000";
			checkpointComparisonTimer.text = "Fastest: 00:00:000";
			shouldIncrement = true;
			timeCount = 0;
			primaryTimer.text = FormatTime(timeCount);
			checkpointTimerDisable = StartCoroutine(DisableText(checkpointTimer.gameObject));
			checkpointComparisonTimerDisable = StartCoroutine(DisableText(checkpointComparisonTimer.gameObject));
		}
		public void OnIntermediateCheckpoint(){
			Debug.Log("SplitTimer.CheckpointUI - OnIntermediateCheckpoint()");
			paused = false;
			if (shouldIncrement){
				StopCheckpointElementsTimeout();
				EnableCheckpointElements();
				checkpointTimer.text = "Yours: " + primaryTimer.text;
				try{
					Debug.Log(fastest_split_times);
					float fastSplitTime = fastest_split_times.fastest_split_times[trailTimer.current_checkpoint_num-1];
					checkpointComparisonTimer.text = "Fastest: " + FormatTime(fastSplitTime).ToString();
					FlashOnTimeDifference(fastSplitTime, timeCount);
				}
				catch (System.IndexOutOfRangeException){
					Debug.Log("SplitTimer.CheckpointUI - OnIntermediateCheckpoint() - Checkpoint is not on server!");
					checkpointComparisonTimer.text = "Fastest: NONE";
				}
				checkpointTimerDisable = StartCoroutine(DisableText(checkpointTimer.gameObject));
				checkpointComparisonTimerDisable = StartCoroutine(DisableText(checkpointComparisonTimer.gameObject));
			}
		}
		public void OnPauseCheckpoint(){
			PauseTimer();
		}
		public void OnFinishCheckpoint(){
            StopCheckpointElementsTimeout();
            EnableCheckpointElements();
			try{
				float fastSplitTime = fastest_split_times.fastest_split_times[trailTimer.current_checkpoint_num-1];
				checkpointComparisonTimer.text = "Fastest: " + FormatTime(fastSplitTime).ToString();
				FlashOnTimeDifference(fastSplitTime, timeCount);
			}
			catch (System.IndexOutOfRangeException){
				Debug.Log("SplitTimer.CheckpointUI - OnFinishCheckpoint() - Checkpoint is not on server!");
				checkpointComparisonTimer.text = "Fastest: NONE";
			}
			StopTimer();
		}
		void FlashOnTimeDifference(float fastSplitTime, float ourSplitTime){
			if (fastSplitTime - ourSplitTime < 0) // is slower
			{
				green.color = Color.red;
			}
			else if (fastSplitTime - ourSplitTime > 0) // is faster
			{
				green.color = Color.green;
			}
			greenFlash.alpha = 1;
			StartCoroutine(FadeOutFlasher());
		}
        IEnumerator FadeOutFlasher(){
			while (greenFlash.alpha != 0){
				greenFlash.alpha -= 0.01f;
				yield return new WaitForSeconds(0.01f);
			}
		}
		public void StopTimer(){
			StopCheckpointElementsTimeout();
			shouldIncrement = false;
			disableTimerCoroutine = StartCoroutine(DisableTimer());
		}
		public void PauseTimer(){
			paused = true;
		}
		public void ResumeTimer(){
			paused = false;
		}
		IEnumerator DisableText(GameObject toDisable){
			yield return new WaitForSeconds(5f);
			toDisable.SetActive(false);
		}
		void GetFastestTimes(){
			if (ServerInfo.Instance.isOnline){
				StartCoroutine(CoroGetFastestTimes(trailTimer.trail_name));
			}
		}
		IEnumerator CoroGetFastestTimes(string trail_name){
			using (UnityWebRequest webRequest = UnityWebRequest.Get(
				ServerInfo.Instance.server
				+ "/API/DESCENDERS-GET-FASTEST-TIME"
				+ "?trail_name="
				+ trailTimer.trail_name
				+ "&steam_id=" + new SteamIntegration().getSteamId()
				+ "&steam_name=" + new SteamIntegration().getName()
				+ "&world_name=" + SplitTimer.Instance.world_name))
			{
				yield return webRequest.SendWebRequest();
				string data = webRequest.downloadHandler.text;
				//Debug.Log(data);
				fastest_split_times = JsonUtility.FromJson<FastestSplitTimes>(data);
			}
		}
		IEnumerator DisableTimer(){
			yield return new WaitForSeconds(15f);
			checkpointUi.alpha = 0;

		}
		private string FormatTime(float time)
        {
            int intTime = (int)time;
            int minutes = intTime / 60;
            int seconds = intTime % 60;
            float fraction = time * 1000;
            fraction = (fraction % 1000);
            string timeText = System.String.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, fraction);
            return timeText;
        }
	}
}