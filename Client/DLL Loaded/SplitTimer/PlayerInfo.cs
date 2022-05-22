using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerIdentification;

namespace SplitTimer{
	public class PlayerInfo : MonoBehaviour {
		SteamIntegration steamIntegration = new SteamIntegration();
		GameObject PlayerHuman;
		Vector3 PreviousPos;
		public static PlayerInfo Instance { get; private set; }
		void Awake(){
			if (Instance != null && Instance != this) 
				Destroy(this); 
			else
				Instance = this; 
		}
		public void NetStart(){
			OnMapEnter(MapInfo.Instance.MapId, MapInfo.Instance.MapName);
			NetClient.Instance.SendData("STEAM_ID|" + steamIntegration.getSteamId());
			NetClient.Instance.SendData("STEAM_NAME|" + steamIntegration.getName());
			NetClient.Instance.SendData("WORLD_NAME|" + MapInfo.Instance.MapName);
		}
		void Update () {
			if (PlayerHuman == null)
				PlayerHuman = GameObject.Find("Player_Human");
			if (PlayerHuman != null){
				if (Vector3.Distance(
						PlayerHuman.transform.position,
						PreviousPos
					) > 6){
					OnRespawn();
					PreviousPos = PlayerHuman.transform.position;
				}
			}
		}
		public void OnRespawn(){
			NetClient.Instance.SendData("RESPAWN");
		}
		public void OnBikeSwitch(string old_bike, string new_bike){
			NetClient.Instance.SendData("BIKE_SWITCH|" + old_bike + "|" + new_bike);
		}
		public void OnBoundryEnter(string boundry_guid, float client_time){
			NetClient.Instance.SendData("BOUNDRY_ENTER|" + boundry_guid + "|" + client_time.ToString());
		}
		public void OnBoundryExit(string boundry_guid, float client_time){
			NetClient.Instance.SendData("BOUNDRY_EXIT|" + boundry_guid + "|" + client_time.ToString());
		}
		public void OnCheckpointEnter(int checkpoint_num, int total_checkpoints, float client_time){
			NetClient.Instance.SendData("CHECKPOINT_ENTER|" + checkpoint_num + "|" + total_checkpoints + "|" + client_time.ToString());
		}
		public void OnMapEnter(string map_id, string map_name){
			NetClient.Instance.SendData("MAP_ENTER|" + map_id + "|" + map_name);
		}
		public void OnMapExit(){
			NetClient.Instance.SendData("MAP_EXIT");
		}
	}
}
