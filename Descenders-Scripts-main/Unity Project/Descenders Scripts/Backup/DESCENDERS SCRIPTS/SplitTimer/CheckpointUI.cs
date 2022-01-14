using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace SplitTimer{
	public class CheckpointUI : ModBehaviour {
		[System.NonSerialized]
		public float[] fastestTimes;
		public TrailTimer trailTimer;
		public Text primaryTimer;
		public Text checkpointTimer;
		public Text checkpointComparisonTimer;
		public CanvasGroup checkpointUi;
		bool shouldIncrement;
		private float timeCount;
		public bool isCheckpointUIEnabled = true;
		public FastestSplitTimes fastest_split_times;
		public struct FastestSplitTimes{
			public float[] fastest_split_times;
		}
		void Start(){
			checkpointUi.alpha = 0;
			GetFastestTimes();
		}
		void Update () {
			if (shouldIncrement){
				timeCount += Time.deltaTime;
			}
			primaryTimer.text = FormatTime(timeCount);
			if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha1)){
				isCheckpointUIEnabled = !isCheckpointUIEnabled;
				if (checkpointUi.alpha == 0){
					checkpointUi.alpha = 1;
				}
				else{
					checkpointUi.alpha = 0;
				}
			}
		}
		IEnumerator KeepFastestTimesUpdated(){
			while (true){
				yield return new WaitForSeconds(10f);
				GetFastestTimes();
			}
		}
		public void EnterCheckpoint(){
			checkpointTimer.gameObject.SetActive(true);
			checkpointComparisonTimer.gameObject.SetActive(true);
			checkpointTimer.text = "Yours: " + primaryTimer.text;
			if (trailTimer.current_checkpoint_num != 0){
				checkpointComparisonTimer.text = "Fastest: " + FormatTime(fastest_split_times.fastest_split_times[trailTimer.current_checkpoint_num-1]).ToString();
			}
			else{
				checkpointComparisonTimer.text = "Fastest: 00:00:00";
			}
			if (isCheckpointUIEnabled){
				checkpointUi.alpha = 1;
			}
			StartCoroutine(DisableText(checkpointTimer.gameObject));
			StartCoroutine(DisableText(checkpointComparisonTimer.gameObject));
		}
		public void RestartTimer(){
			timeCount = 0;
			primaryTimer.text = FormatTime(timeCount);
			primaryTimer.gameObject.SetActive(true);
			checkpointTimer.gameObject.SetActive(true);
			shouldIncrement = true;
		}
		public void StopTimer(){
			shouldIncrement = false;
			StartCoroutine(DisableTimer());
		}
		IEnumerator DisableText(GameObject toDisable){
			yield return new WaitForSeconds(5f);
			toDisable.SetActive(false);
		}
		public void GetFastestTimes(){
			StartCoroutine(CoroGetFastestTimes(trailTimer.trail_name));
		}
		IEnumerator CoroGetFastestTimes(string trail_name){
			using (UnityWebRequest webRequest = UnityWebRequest.Get(SplitTimer.Instance.api.contact + "/API/DESCENDERS-GET-FASTEST-TIME?trail_name=" + trailTimer.trail_name))
			{
				yield return webRequest.SendWebRequest();
				string data = webRequest.downloadHandler.text;
				Debug.Log(data);
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