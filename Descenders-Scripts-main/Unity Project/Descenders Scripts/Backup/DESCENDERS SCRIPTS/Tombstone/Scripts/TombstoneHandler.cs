using UnityEngine;
using System.Collections;
using PlayerIdentification;
using UnityEngine.Networking;
using System.Collections.Generic;
using ModTool.Interface;
using UnityEngine.UI;

namespace TombstoneSystem{
	public class TombstoneHandler : ModBehaviour {
		[Tooltip("The name of your map (if you want to reset the gravestones on your map, change this!)")]
		public string mapName;
		[Header("Configuration")]
		public bool justMe = false;
		public bool justToday = true;
		public bool justRecentDeaths = true;
		[Header("Debugging")]
		public Toggle justMeToggle;
		public Toggle justTodayToggle;
		public Toggle justRecentDeathsToggle;
		[Tooltip("The url to the server (e.g. http://google.com:8023)")]
		public string url;
		[Tooltip("The object that acts as a tombstone (must have a tombstone script on it!)")]
		public GameObject tombstone;
		
		List<GameObject> tombstones = new List<GameObject>();
		private bool justDied = false;
		const string gameobjectThatUnchildsName = "Cyclist";
		string messsageOnJustDie = "literally just then";
		bool objectSet = false;		
		GameObject gameobjectThatUnchilds;
		GameObject player;
		Vector3 playerPos = new Vector3(0, 0, 0);
		string additionalmessage = "";

		void Start () {
			StartCoroutine(SpawnTombstones());
			StartCoroutine(DelayedStartInitiator());
			justMeToggle.isOn = justMe;
			justTodayToggle.isOn = justToday;
			justRecentDeathsToggle.isOn = justRecentDeaths;
			//StartCoroutine(SendDeathInfo(new Vector3(0, 0, 0)));
		}

		public void ToggleJustMe(){
			justMe = !justMe;
		}
		public void ToggleJustToday(){
			justToday = !justToday;
		}

		public void ToggleJustRecentDeaths(){
			justRecentDeaths = !justRecentDeaths;
		}

		public void DestroyAllTombstones(){
			if (tombstones.Count > 0){
				foreach (GameObject obj in tombstones){
					Destroy(obj);
				}
				tombstones.Clear();
				Debug.Log("Done!");
			}
		}

		private IEnumerator DelayedStartInitiator(){
			yield return new WaitForSeconds(10f);
			DelayedStart();
		}

		void DelayedStart(){
			gameobjectThatUnchilds = GameObject.Find("Cyclist");
			player = GameObject.Find("Player_Human");
			objectSet = true;
		}

		public IEnumerator SendDeathInfo(Vector3 location){
			Debug.Log("Sending death info");
			Debug.Log("Location of death: " + " " + location.x.ToString() + " " + location.y.ToString() + " " + location.z.ToString());
			string name = new SteamIntegration().getName();
			string steamId = new SteamIntegration().getSteamId();
			string xPos = location.x.ToString();
			string yPos = location.y.ToString();
			string zPos = location.z.ToString();
			string req = url + "/submit_tombstone_data" + "?name=" + name + "&map=" + mapName + "&steamId=" + steamId + "&xPos=" + xPos + "&yPos=" + yPos + "&zPos=" + zPos;
			using (UnityWebRequest webRequest = UnityWebRequest.Get(req))	
        	{
				yield return webRequest.SendWebRequest();
			}
		}

		public void InitiateSpawnTombstones(){
			StartCoroutine(SpawnTombstones());
		}

		public IEnumerator SpawnTombstones()
		{
			DestroyAllTombstones();
			string result;// = "{\"steamNames\" : [\"antgrass\"], \"dates\" : [\"1227th January 2021\"], \"steamIds\" : [\"123123\"], \"xPoss\" : [\"123.2\"], \"yPoss\" : [\"12.2\"], \"zPoss\" : [\"12345\"]}";
			using (UnityWebRequest webRequest = UnityWebRequest.Get(url + "/get_message_on_just_die"))
        	{
				// Request and wait for the desired page.
				yield return webRequest.SendWebRequest();
				messsageOnJustDie = webRequest.downloadHandler.text;
			}
			using (UnityWebRequest webRequest = UnityWebRequest.Get(url + "/get_additional_message"))
        	{
				// Request and wait for the desired page.
				yield return webRequest.SendWebRequest();
				additionalmessage = webRequest.downloadHandler.text;
			}
			string steamId = new SteamIntegration().getSteamId();
			using (UnityWebRequest webRequest = UnityWebRequest.Get(url + "/get_tombstone_data?map=" + mapName + "&just_today=" + justToday.ToString() + "&just_me=" + justMe.ToString() + "&just_recent_deaths=" + justRecentDeaths.ToString() + "&steamId=" + steamId.ToString()))
        	{
				// Request and wait for the desired page.
				yield return webRequest.SendWebRequest();
				result = webRequest.downloadHandler.text;
			}
			TombstoneData tombstoneData = JsonUtility.FromJson<TombstoneData>(result);
			for (int i = 0; i < tombstoneData.steamNames.Count; i++){
				GameObject instantiatedTombstone = Instantiate(tombstone);
				if (additionalmessage == ""){
					instantiatedTombstone.GetComponent<Tombstone>().text.text =
						"Here lies " + tombstoneData.steamNames[i] + "\n\n" + tombstoneData.dates[i];
				}
				else{
					instantiatedTombstone.GetComponent<Tombstone>().text.text = additionalmessage;
				}
				instantiatedTombstone.transform.position = new Vector3(float.Parse(tombstoneData.xPoss[i]), float.Parse(tombstoneData.yPoss[i]), float.Parse(tombstoneData.zPoss[i]));
				instantiatedTombstone.transform.rotation = Quaternion.Euler(new Vector3(0, Random.Range(-360f, 370f), 0));
				tombstones.Add(instantiatedTombstone);
			}			
		}
		void Update(){
			if (objectSet && (gameobjectThatUnchilds.transform.parent == null)){
				if (!justDied){
					Debug.Log("Player Died, sending information to server!");
					StartCoroutine(SendDeathInfo(playerPos));
					justDied = true;
					string name = new SteamIntegration().getName();
					GameObject instantiatedTombstone = Instantiate(tombstone);
					instantiatedTombstone.GetComponent<Tombstone>().text.text =
						"Here lies " + name + "\n\n" + messsageOnJustDie;
					instantiatedTombstone.transform.position = playerPos;
					tombstones.Add(instantiatedTombstone);
				}
			}
			else{
				try{
					playerPos = player.transform.position;
				}
				catch{
					// Debug.Log("Playing in Editor");
				}
				justDied = false;
			}
		}
	}

	public struct TombstoneData{
		public List<string> steamNames;
		public List<string> dates;
		public List<string> steamIds;
		public List<string> xPoss;
		public List<string> yPoss;
		public List<string> zPoss;
	}

}