using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using ModTool.Interface;
using CustomUi;

namespace CustomTeleporter {
	public class Teleporter : ModBehaviour {
		public GameObject playerButtons;
		public RectTransform rectTransformOfCanvas;
		public RectTransform youUiRectTransform;
		public RectTransform preemptiveSpawnPoint;
		public UI uI;
		public Camera cameraOverTerrain;
		List<ArrayList> allPlayers;
		GameObject player;
		Button[] buttons;

		void Start () {
			StartCoroutine(lateStartInitiator());
			buttons = playerButtons.GetComponentsInChildren<Button>();
			foreach(Button button in buttons){
				button.gameObject.transform.parent.gameObject.SetActive(false);
			}
		}
		
		IEnumerator lateStartInitiator(){
			yield return new WaitForSeconds(5f);
			LateStart();
		}

		void OnEnable()
		{
			Debug.Log("Map Enabled");
			Refresh();
		}

		void LateStart(){
			Debug.Log("LateStart Called");
			Refresh();
		}
		
		public struct Name{
			public string playerName;
			public string steamID;
		}

		public struct PlayerInfo{
			public GameObject gameObj;
			public string playerName;
			public string steamId;
		}

		List<ArrayList> GetAllPlayers(){
			Debug.Log("Fetching all players");
			List<ArrayList> arr = new List<ArrayList>();
			object[] obj = GameObject.FindObjectsOfType(typeof(GameObject));
			foreach (UnityEngine.Object m_player in obj){
				if (m_player.name == "PlayerInfo_Networked"){
					/*Component PlayerInfo = ((GameObject)m_player).GetComponent("PlayerInfoImpact");
					string json = JsonUtility.ToJson(PlayerInfo);
					json = json.Replace("a^sXf\u0083Y", "playerName");
					json = json.Replace("r~xs{n", "steamID");
					json = json.Replace("W\u0082oQHKm", "gameObj");
					Name id = JsonUtility.FromJson<Name>(json);
					PlayerInf x = JsonUtility.FromJson<PlayerInf>(json);
					x.gameObj.transform.position = new Vector3(0, 0, 0);
					*/
					Component PlayerInfo = ((GameObject)m_player).GetComponent("PlayerInfoImpact");
					string json = JsonUtility.ToJson(PlayerInfo);
					json = json.Replace("W\u0082oQHKm", "gameObj");
					json = json.Replace("a^sXf\u0083Y", "playerName");
					json = json.Replace("r~xs{n", "steamId");
					PlayerInfo playerInfo = JsonUtility.FromJson<PlayerInfo>(json);
					ArrayList temp_arr = new ArrayList(3);
					temp_arr.Add(playerInfo.gameObj);
					temp_arr.Add(playerInfo.playerName);
					temp_arr.Add(playerInfo.steamId);
					arr.Add(temp_arr);
				}
			}
			return arr;
		}

		public void SpawnAtPoint(GameObject point){
			if (Application.isEditor){
				Debug.LogWarning("Not spawning at point, you are in the editor!");
				return;
			}
			Vector3 coords = point.transform.position;
			Debug.Log("Spawning player at coordinates x: " + coords.x.ToString() + ", y: " + coords.y.ToString() + ", z: " + coords.z.ToString());
			GameObject human = GameObject.Find("Player_Human");
			human.transform.position = point.transform.position;
			human.transform.rotation = point.transform.rotation;
		}

		public void Refresh(){
			try{
				Debug.Log("Refreshing Map.");
				allPlayers = GetAllPlayers();
				player = GameObject.Find("Player_Human");
				int i = 0;
				foreach(Button button in buttons){
					button.gameObject.transform.parent.gameObject.SetActive(false);
				}
				foreach (ArrayList pla in allPlayers){
					if (i < allPlayers.Count){
						GameObject m_player = (GameObject)(pla[0]);
						buttons[i].gameObject.transform.parent.gameObject.SetActive(true);
						buttons[i].onClick.RemoveAllListeners();
						buttons[i].onClick.AddListener(delegate() {SpawnAtPoint(m_player);});
						buttons[i].onClick.AddListener(delegate() {uI.DisableUI();});
						buttons[i].gameObject.GetComponentInChildren<Text>().text = (string)allPlayers[i][1];
					}
					else{
						buttons[i].gameObject.transform.parent.gameObject.SetActive(false);
					}
					i++;
				}
			}
			catch(Exception e){
				Debug.Log(e);
			}
		}

		void Update () {
			SetPositionFromGameobjectOverTerrain(player, rectTransformOfCanvas, youUiRectTransform.GetComponent<RectTransform>());
			int i = 0;
			try{
				foreach (ArrayList m_player in allPlayers){
					SetPositionFromGameobjectOverTerrain((GameObject)m_player[0], rectTransformOfCanvas, buttons[i].transform.parent.gameObject.GetComponent<RectTransform>());
					i++;
				}
			}
			catch(Exception){
				// if there's an error here,
				// it's because one of the players
				// has disconnected, so refresh.
				Refresh();
			}
		}

		public void SetPositionFromGameobjectOverTerrain(GameObject player, RectTransform canvasRectTransform, RectTransform thisRectTransform){
			if (Application.isEditor){
				return;
			}
			Vector2 ViewportPosition = 
				cameraOverTerrain.WorldToViewportPoint(
					player.transform.position
				);
			Vector2 WorldObject_ScreenPosition=new Vector2(
				(
					(ViewportPosition.x * 
					canvasRectTransform.sizeDelta.x
					)
					-
					(canvasRectTransform.sizeDelta.x * 
					0.5f)
				),
				(
					(ViewportPosition.y * 
					canvasRectTransform.sizeDelta.y
					)
					-
					(canvasRectTransform.sizeDelta.y *
					0.5f)
				));
			thisRectTransform.anchoredPosition = 
				WorldObject_ScreenPosition;
		}
	}
}