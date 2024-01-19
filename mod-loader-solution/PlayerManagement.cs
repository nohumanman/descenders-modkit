using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerIdentification;
using ModLoaderSolution;
using UnityEngine.SceneManagement;
using System;

namespace ModLoaderSolution
{
	public class PlayerManagement : MonoBehaviour {
		SteamIntegration steamIntegration = new SteamIntegration();
		GameObject PlayerHuman;
		Vector3 PreviousPos;
		public string version = "0.2.40";
		public float speed;
		bool hasLoadedPlayer = false;
		bool wasBailed = false;
		public static PlayerManagement Instance { get; private set; }
		void Awake(){
			Utilities.Log("LocalApplicationData '" + Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "'");
			Utilities.Log("Version number " + version);
			if (Instance != null && Instance != this) 
				Destroy(this); 
			else
				Instance = this; 
		}
		public void NetStart(){
			NetClient.Instance.SendData("BIKE_TYPE|" + GetComponent<BikeSwitcher>().oldBike);
			OnMapEnter("idhere", Utilities.instance.GetCurrentMap());
			//OnMapEnter("IDHERE", MapInfo.Instance.ModName);
			//NetClient.Instance.SendData("SET_BIKE|" + Utilities.instance.GetBike());
			NetClient.Instance.SendData("VERSION|" + version);
			NetClient.Instance.SendData("STEAM_ID|" + steamIntegration.getSteamId());
			NetClient.Instance.SendData("STEAM_NAME|" + steamIntegration.getName());
			foreach (Trail trail in FindObjectsOfType<Trail>())
            {
				Utilities.Log("Looking for leaderboard texts on trail '" + trail.name + "'");
				//trail.leaderboardText.GetComponent<TextMesh>().font = AssetBundling.Instance.bundle.LoadAsset<UnityEngine.UI.Text>("");
				if (trail.leaderboardText != null)
                {
					Utilities.Log("Found Speedrun.com Leaderboard for '" + trail.name + "'");
					NetClient.Instance.SendData("SPEEDRUN_DOT_COM_LEADERBOARD|" + trail.name);
				}
				if (trail.autoLeaderboardText != null)
                {
					Utilities.Log("Found auto Leaderboard for '" + trail.name + "'");
					NetClient.Instance.SendData("LEADERBOARD|" + trail.name);
				}
            }
		}
		string prevMap = "";
		IEnumerator ChangeMapPresence(string map_name)
        {
			long startTimestamp = Utilities.ToUnixTime(DateTime.UtcNow);
			while (Utilities.instance.GetCurrentMap() == map_name)
            {
				Utilities.instance.UpdateDiscordPresence(startTimestamp);
				yield return new WaitForSeconds(3f);
			}
		}
		void Update () {
			if (Utilities.instance.GetCurrentMap() != prevMap)
            {
				StartCoroutine(ChangeMapPresence(Utilities.instance.GetCurrentMap()));
				Utilities.Log("Map Change Detected");
				OnMapEnter("idhere", Utilities.instance.GetCurrentMap());
				// if not a bike park or a mod
				if (!Utilities.instance.isBikePark() && !Utilities.instance.isMod() && !(Utilities.instance.GetCurrentMap() == "0"))
				{
					StatsModification.instance.ResetStats();
					StatsModification.instance.permitted = false;
				}
				else
					StatsModification.instance.permitted = true;
				StatsModification.instance.DirtyStats();
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
			//Utilities.Log(PhotonNetwork.CloudRegion);
			//Utilities.Log(PhotonNetwork.CreateRoom("6969"));
			//Utilities.Log(PhotonNetwork.JoinRoom("6969"));
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
		public void OnBoundryExit(string trail_name, string boundry_guid, string boundry_obj_name){
			NetClient.Instance.SendData("BOUNDRY_EXIT|" + trail_name + "|" + boundry_guid + "|" + boundry_obj_name);
		}
		public void OnCheckpointEnter(string trail_name, string type, int total_checkpoints, string client_time){
			NetClient.Instance.SendData("CHECKPOINT_ENTER|" + trail_name + "|" + type + "|" + total_checkpoints.ToString() + "|" + client_time);
		}
		public void OnMapEnter(string map_id, string map_name){
			NetClient.Instance.SendData("MAP_ENTER|" + map_id + "|" + map_name);
			// if map_name is 0 we are in lobby
			if (map_name == "0")
				Destroy(GameObject.Find("sign_modoftheyear"));
		}
		public void OnMapExit(){
			NetClient.Instance.SendData("MAP_EXIT");
		}
	}
}
