using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerIdentification;

namespace SplitTimer{
	public class PlayerInfo : MonoBehaviour {
		SteamIntegration steamIntegration = new SteamIntegration();
		GameObject PlayerHuman;
		Vector3 PreviousPos;
		public float speed;
		bool hasLoadedPlayer = false;
		public static PlayerInfo Instance { get; private set; }
		void Awake(){
			if (Instance != null && Instance != this) 
				Destroy(this); 
			else
				Instance = this; 
		}
		public void NetStart(){
			OnMapEnter(MapInfo.Instance.MapId, MapInfo.Instance.MapName);
			NetClient.Instance.SendData("VERSION|0.1.2");
			NetClient.Instance.SendData("STEAM_ID|" + steamIntegration.getSteamId());
			NetClient.Instance.SendData("STEAM_NAME|" + steamIntegration.getName());
			NetClient.Instance.SendData("WORLD_NAME|" + MapInfo.Instance.MapName);
			foreach (Trail trail in GameObject.FindObjectsOfType<Trail>())
            {
				// Debug.Log("PlayerInfo | Looking for leaderboard texts on trail '" + trail.name + "'");
				if (trail.leaderboardText != null)
                {
					// Debug.Log("PlayerInfo | Found Speedrun.com Leaderboard");
					NetClient.Instance.SendData("SPEEDRUN_DOT_COM_LEADERBOARD|" + trail.name);
				}
				if (trail.autoLeaderboardText != null)
                {
					// Debug.Log("PlayerInfo | Found auto Leaderboard");
					NetClient.Instance.SendData("LEADERBOARD|" + trail.name);
				}
            }
		}
		void Update () {
			if (PlayerHuman == null)
				PlayerHuman = GameObject.Find("Player_Human");
			if (PlayerHuman != null){
				if (!hasLoadedPlayer)
					GetComponent<BikeSwitcher>().ToEnduro();
				hasLoadedPlayer = true;
				if (Vector3.Distance(
						PlayerHuman.transform.position,
						PreviousPos
					) > 6){
					OnRespawn();
					PreviousPos = PlayerHuman.transform.position;
				}
				speed = Vector3.Distance(PlayerHuman.transform.position, PreviousPos) / Time.deltaTime;
				PreviousPos = PlayerHuman.transform.position;
			}
		}
		public void OnRespawn(){
			NetClient.Instance.SendData("RESPAWN");
		}
		public void OnBikeSwitch(string old_bike, string new_bike){
			NetClient.Instance.SendData("BIKE_SWITCH|" + old_bike + "|" + new_bike);
		}
		public void OnBoundryEnter(string trail_name, string boundry_guid){
			NetClient.Instance.SendData("BOUNDRY_ENTER|" + trail_name + "|" + boundry_guid);
		}
		public void OnBoundryExit(string trail_name, string boundry_guid){
			NetClient.Instance.SendData("BOUNDRY_EXIT|" + trail_name + "|" + boundry_guid);
		}
		public void OnCheckpointEnter(string trail_name, string type, int total_checkpoints, string client_time){
			NetClient.Instance.SendData("CHECKPOINT_ENTER|" + trail_name + "|" + type + "|" + total_checkpoints.ToString() + "|" + client_time);
		}
		public void OnMapEnter(string map_id, string map_name){
			NetClient.Instance.SendData("MAP_ENTER|" + map_id + "|" + map_name);
		}
		public void OnMapExit(){
			NetClient.Instance.SendData("MAP_EXIT");
		}
	}
}
