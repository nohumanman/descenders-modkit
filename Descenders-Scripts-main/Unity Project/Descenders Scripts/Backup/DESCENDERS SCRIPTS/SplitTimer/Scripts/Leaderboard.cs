using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ModTool.Interface;
using UnityEngine.Networking;

namespace SplitTimer{
	public struct LeaderboardData{
		public string[] steam_ids;
		public float[] time_ids;
		public float[] timestamps;
		public float[] trail_names;
		public string[] was_monitoreds;
		public float[] total_times;
		public string[] steam_names;
		public string[] is_competitors;
	}

	public class Leaderboard : ModBehaviour {
		public TextMesh textMesh;
		public TrailTimer trailTimer;
		public void Start(){
			UpdateLeaderboard();
		}
		public void UpdateLeaderboard(){
			// make get request to refresh leaderboard.
			Debug.Log("Updating Leaderboard");
			StartCoroutine(CoroUpdateLeaderboard(10));
			StartCoroutine(KeepUpdatingLeaderboard(10));
		}
		IEnumerator KeepUpdatingLeaderboard(int num){
			while (true){
				yield return new WaitForSeconds(5f);
				StartCoroutine(CoroUpdateLeaderboard(num));
			}
		}
		IEnumerator CoroUpdateLeaderboard(int num){
			// make get request to refresh leaderboard.
			using (
				UnityWebRequest webRequest
				= UnityWebRequest.Get(
					SplitTimer.Instance.splitTimerApi.server
					+ "/API/DESCENDERS-LEADERBOARD?num="
					+ num.ToString()
					+ "&trail_name="
					+ trailTimer.trail_name))
			{
				yield return webRequest.SendWebRequest();
				string data = webRequest.downloadHandler.text;
				LeaderboardData leaderboardData = JsonUtility.FromJson<LeaderboardData>(data);
				UpdateText(leaderboardData);
			}
		}
		public void UpdateText(LeaderboardData leaderboardData){
			string new_text = "";
			int i = 0;
			foreach(string steam_id in leaderboardData.steam_ids){
				new_text +=  FormatTime(leaderboardData.total_times[i]);
				new_text += " - ";
				new_text += leaderboardData.steam_names[i];
				new_text += "\n";
				i++;
			}
			textMesh.text = new_text;
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