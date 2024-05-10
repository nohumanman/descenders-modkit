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
		Vector3 PreviousPos = Vector3.zero;
		public float speed;
		bool wasBailed = false;
		public static PlayerManagement Instance { get; private set; }
		void Awake(){
			Utilities.Log("LocalApplicationData '" + Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "'");
			Utilities.Log("Version number " + NetClient.GetVersion());
			if (Instance != null && Instance != this) 
				Destroy(this); 
			else
				Instance = this; 
		}
		public void NetStart(){
			OnMapEnter("idhere", Utilities.instance.GetCurrentMap());
			NetClient.Instance.SendData("BIKE_TYPE", GetComponent<BikeSwitcher>().GetBike());
			NetClient.Instance.SendData("VERSION", NetClient.GetVersion());
			NetClient.Instance.SendData("STEAM_ID", steamIntegration.getSteamId());
			NetClient.Instance.SendData("STEAM_NAME", steamIntegration.getName());
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
			string currentMap = Utilities.instance.GetCurrentMap();
			if (currentMap != prevMap)
            {
				StartCoroutine(ChangeMapPresence(currentMap));
				Utilities.Log("Map Change Detected");
				OnMapEnter("idhere", currentMap);
				// if not a bike park or a mod
				if (!Utilities.instance.isBikePark() && !Utilities.instance.isMod() && !(currentMap == "0"))
				{
					StatsModification.instance.ResetStats();
					StatsModification.instance.permitted = false;
				}
				else
					StatsModification.instance.permitted = true;
				//StatsModification.instance.DirtyStats();
				prevMap = currentMap;
			}
			if (PlayerHuman == null)
				PlayerHuman = Utilities.GetPlayer();
			if (Utilities.instance.hasBailed() && !wasBailed)
				OnRespawn();
			wasBailed = Utilities.instance.hasBailed();
			if (PlayerHuman != null){
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
            SplitTimerText.Instance.hidden = true;
			NetClient.Instance.SendData("RESPAWN");
		}
		public void OnBikeSwitch(string old_bike, string new_bike){
			NetClient.Instance.SendData("BIKE_SWITCH|" + old_bike + "|" + new_bike);
		}
		public void OnBoundaryEnter(string trail_name, string boundary_guid){
			NetClient.Instance.SendData("BOUNDARY_ENTER|" + trail_name + "|" + boundary_guid);
		}
		public void OnBoundaryExit(string trail_name, string boundary_guid, string boundary_obj_name){
			NetClient.Instance.SendData("BOUNDARY_EXIT|" + trail_name + "|" + boundary_guid + "|" + boundary_obj_name);
		}
		public void OnCheckpointEnter(string trail_name, string type, int total_checkpoints, string client_time, string hash){
			NetClient.Instance.SendData("CHECKPOINT_ENTER|" + trail_name + "|" + type + "|" + total_checkpoints.ToString() + "|" + client_time + "|" + hash);
		}
		public void OnMapEnter(string map_id, string map_name){
			NetClient.Instance.SendData("MAP_ENTER|" + map_id + "|" + map_name);
			// if map_name is 0 we are in lobby
			if (map_name == "0")
				Destroy(Utilities.GameObjectFind("sign_modoftheyear"));
			if (map_name == "Ced's Downhill Park-1.0")
			{
				string[] strs = new string[] { "C1", "C2", "C3", "C4", "C1.5" };
				foreach(string str in strs)
				{
                    Utilities.GameObjectFind(str).tag = "Checkpoint";
                }
				
            }
			if (Utilities.instance.isMod()) // only do this to mods to not mess things up
				Utilities.instance.NormaliseModSongs();
		}
		public void OnMapExit(){
			NetClient.Instance.SendData("MAP_EXIT");
		}
	}
}
