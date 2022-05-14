using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using ModTool.Interface;

namespace SpeedRunDot{
	[System.Serializable]
	public class PlayerID {
		[SerializeField]
		public string id;
		[SerializeField]
		public string name;
	}
	[System.Serializable]
	public class Time {
		[SerializeField]
		public float primary_t;
	}
	[System.Serializable]
	public class Run {
		[SerializeField]
		public string id;
		[SerializeField]
		public string weblink;
		[SerializeField]
		public Time times;
		[SerializeField]
		public PlayerID[] players;
	}
	[System.Serializable]
	public class LeaderboardPlace {
		[SerializeField]
		public int place;
		[SerializeField]
		public Run run;
	}
	[System.Serializable]
	public class Data {
		[SerializeField]
		public string game;
		[SerializeField]
		public LeaderboardPlace[] runs;
	}
	[System.Serializable]
	public class LeaderboardData {
		[SerializeField]
		public Data data;
	}
	[System.Serializable]
	public class NameData {
		[SerializeField]
		public string international;
	}
	[System.Serializable]
	public class PlayerData {
		[SerializeField]
		public NameData names;
	}
	[System.Serializable]
	public class PlayerReq {
		[SerializeField]
		public ReqData data;
	}
	[System.Serializable]
	public class ReqData {
		[SerializeField]
		public PlayerName names;
	}
	[System.Serializable]
	public class PlayerName {
		[SerializeField]
		public string international;
	}
	[System.Serializable]
	public class SpeedrunDotCom : ModBehaviour {
		public string gameName = "Descenders";
		public string categoryName = "7dg4yg4d";
		public string levelName = "Canyon";
		public int cap = 10;
		void Start () {
			
			StartCoroutine(StartCoro());
		}
		IEnumerator StartCoro(){
			string jsonA;
			using (UnityWebRequest webRequest = UnityWebRequest.Get(
				"https://www.speedrun.com/api/v1/leaderboards/"
				+ gameName + "/level/"
				+ levelName + "/"
				+ categoryName))
			{
				yield return webRequest.SendWebRequest();
				jsonA = webRequest.downloadHandler.text;
				LeaderboardData leaderboardData = JsonUtility.FromJson<LeaderboardData>(jsonA);
				Debug.Log("LEADERBOARD - " + jsonA);
				string jsonB;
				string leaderboardText = "";
				int currentPlace = 0;
				Debug.Log("Trying foreach:");
				Debug.Log(JsonUtility.ToJson(leaderboardData));
				Debug.Log(leaderboardData.data);
				Debug.Log(leaderboardData.data.runs);
				
				foreach(LeaderboardPlace place in leaderboardData.data.runs){
					Debug.Log("Place Here");
					if (cap == currentPlace){
						break;
					}
					using (UnityWebRequest webRequest1 = UnityWebRequest.Get(
						"https://www.speedrun.com/api/v1/users/"
						+ place.run.players[0].id
					))
					{
						yield return webRequest1.SendWebRequest();
						jsonB = webRequest1.downloadHandler.text;
						try{
							PlayerReq playerDat = JsonUtility.FromJson<PlayerReq>(jsonB);
							Debug.Log("LEADERBOARD - " + playerDat.data.names.international);
							Debug.Log("LEADERBOARD - " + place.run.times.primary_t);
							leaderboardText +=
								place.place
								+ ". "
								+ playerDat.data.names.international
								+ " - "
								+ FormatTime(place.run.times.primary_t).ToString()
								+ "\n";
						}
						catch(System.Exception e){
							Debug.Log("Error LEADERBOARD - " + e);
						}
					}
					currentPlace++;
				}
				GetComponent<TextMesh>().text = leaderboardText;
				Debug.Log("LEADERBOARD - " + leaderboardText);
			}
			
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
