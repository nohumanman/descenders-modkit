﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerIdentification;
using ModLoaderSolution;
using UnityEngine.SceneManagement;
using System;

namespace SplitTimer{
	public class PlayerInf : MonoBehaviour {
		SteamIntegration steamIntegration = new SteamIntegration();
		GameObject PlayerHuman;
		Vector3 PreviousPos;
		public string version = "0.2.10";
		public float speed;
		bool hasLoadedPlayer = false;
		bool wasBailed = false;
		public static PlayerInf Instance { get; private set; }
		void Awake(){
			Debug.Log("PlayerInfo | LocalApplicationData '" + Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "'");
			Debug.Log("PlayerInfo | Version number " + version);
			if (Instance != null && Instance != this) 
				Destroy(this); 
			else
				Instance = this; 
		}
		public void NetStart(){
			OnMapEnter("idhere", Utilities.instance.GetCurrentMap());
			//OnMapEnter("IDHERE", MapInfo.Instance.ModName);
			if (MapInfo.Instance != null)
            {
				MapInfo.Instance.AddMetric("version", version);
				MapInfo.Instance.AddMetric("steam_id", steamIntegration.getSteamId());
				MapInfo.Instance.AddMetric("steam_name", steamIntegration.getName());
				MapInfo.Instance.AddMetric("world_name", MapInfo.Instance.ModName);
			}
			NetClient.Instance.SendData("SET_BIKE|" + Utilities.instance.GetBike());
			NetClient.Instance.SendData("VERSION|" + version);
			NetClient.Instance.SendData("STEAM_ID|" + steamIntegration.getSteamId());
			NetClient.Instance.SendData("STEAM_NAME|" + steamIntegration.getName());
			NetClient.Instance.SendData("WORLD_NAME|" + MapInfo.Instance.ModName);
			NetClient.Instance.SendData("BIKE_TYPE|" + GetComponent<BikeSwitcher>().oldBike);
			foreach (Trail trail in FindObjectsOfType<Trail>())
            {
				Debug.Log("PlayerInfo | Looking for leaderboard texts on trail '" + trail.name + "'");
				if (trail.leaderboardText != null)
                {
					Debug.Log("PlayerInfo | Found Speedrun.com Leaderboard for '" + trail.name + "'");
					NetClient.Instance.SendData("SPEEDRUN_DOT_COM_LEADERBOARD|" + trail.name);
				}
				if (trail.autoLeaderboardText != null)
                {
					Debug.Log("PlayerInfo | Found auto Leaderboard for '" + trail.name + "'");
					NetClient.Instance.SendData("LEADERBOARD|" + trail.name);
				}
            }
		}
		string prevMap = "";
		void Update () {
			if (Utilities.instance.GetCurrentMap() != prevMap)
            {
				Debug.Log("MAP CHANGED!");
				OnMapEnter("idhere", Utilities.instance.GetCurrentMap());
				prevMap = Utilities.instance.GetCurrentMap();
			}
			if (PlayerHuman == null)
				PlayerHuman = GameObject.Find("Player_Human");
			if (Utilities.instance.hasBailed() && !wasBailed)
				OnRespawn();			
			wasBailed = Utilities.instance.hasBailed();
			if (PlayerHuman != null){
				//if (!hasLoadedPlayer)
				//	GetComponent<BikeSwitcher>().ToEnduro();
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
			//Debug.Log(PhotonNetwork.CloudRegion);
			//Debug.Log(PhotonNetwork.CreateRoom("6969"));
			//Debug.Log(PhotonNetwork.JoinRoom("6969"));
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
