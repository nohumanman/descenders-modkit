using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;
using UnityEngine.UI;

namespace SplitTimer{
	public class CheckpointUI : ModBehaviour {
		[System.NonSerialized]
		public float[] fastestTimes;
		public TrailTimer trailTimer;
		public Text primaryTimer;
		public Text checkpointTimer;
		public CanvasGroup checkpointUi;
		bool shouldIncrement;
		private float timeCount;
		public bool isCheckpointUIEnabled = true;
		void Start(){
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
		public void EnterCheckpoint(){
			checkpointTimer.gameObject.SetActive(true);
			checkpointTimer.text = primaryTimer.text;
			StartCoroutine(DisableText(checkpointTimer.gameObject));
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
			StartCoroutine(DisableText(primaryTimer.gameObject));
			StartCoroutine(DisableText(checkpointTimer.gameObject));
		}
		IEnumerator DisableText(GameObject toDisable){
			yield return new WaitForSeconds(5f);
			toDisable.SetActive(false);
		}
		public void GetFastestTimes(){
			StartCoroutine(CoroGetFastestTimes(trailTimer.trail_name));
		}
		IEnumerator CoroGetFastestTimes(string trail_name){
			yield return null;
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