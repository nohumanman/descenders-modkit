using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ModTool.Interface;
using UnityEngine.Networking;
using UnityEngine.Events;
using PlayerIdentification;

namespace BanHandlerSystem{
	public class BanHandler : ModBehaviour {
		public GameObject banCanvas;
		public Text Message;
		public GameObject InitialLoadupCanvas;
		public string map = "";
		string isFirst = "TRUE";
		void Start () {
			StartCoroutine(CloseInitialLoadup());
			StartCoroutine(checkInternetConnection((isConnected)=>{
				if (!isConnected){IsDisconnected();} // not connected
				else{StartCoroutine(UpdateBans());} // connected
			}));
		}

		void IsDisconnected(){
			Message.text = "This mod requires an online handshake.\n\nYou must be online to use this mod.\nWhen you connect to the internet, this will disappear.";
			banCanvas.SetActive(true);
			StartCoroutine(CheckConnection());
		}

		IEnumerator CheckConnection(){
			bool shouldBreak = false;
			while (true){
				Debug.Log("checking internet again...");
				StartCoroutine(checkInternetConnection((isConnected)=>{
					if (isConnected){
						banCanvas.SetActive(false);
						StartCoroutine(UpdateBans());
						shouldBreak = true;
					}
				// handle connection status here
				}));
				if (shouldBreak){
					yield break;
				}
				yield return new WaitForSeconds(2f);
			}
		}

		IEnumerator CloseInitialLoadup(){
			yield return new WaitForSeconds(5f);
			InitialLoadupCanvas.SetActive(false);
		}

		IEnumerator checkInternetConnection(System.Action<bool> action){
			Debug.Log("Checking Internet Connection...");
			WWW www = new WWW("https://www.cloudflare.com/");
			yield return www;

			if (www.error != null) {
				action (false);
			}
			else {
				action (true);
			}
		}

		string GetSteamID(){
			return new SteamIntegration().getName();
		}

		public void Ban(string message){
			Debug.Log("Banned User.");
			Message.text = "It seems you don't have access to this map :/\n\nIt could be for any of the following reasons: You are banned, you are using an illigitimate version of Descenders, or you are not logged in.\n\nIf you wish to contest this, you can contact the map maker on discord.\n\nMessage from server: \"" + message +"\"";
			banCanvas.SetActive(true);
			banCanvas.SetActive(true);
			try{GameObject.Find("Player_Human").SetActive(false);}
			catch{Debug.Log("Player not found :/");}
		}

		public struct BanResponse{
			public string is_banned;
			public string is_god;
			public string message;
		}

		public void BecomeGod(){
			
		}

		IEnumerator HandleBan(string steam_id, string is_first, string map){
			Debug.Log("Handling Ban.");
			string url = "http://descenders-api.nohumanman.com:8080/ban-handler/check/" + steam_id + "?first=" + is_first + "&map=" + map;
			using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
			{
				yield return webRequest.SendWebRequest();
				string json = webRequest.downloadHandler.text;
				Debug.Log(json);
				BanResponse banResponse = JsonUtility.FromJson<BanResponse>(json);
				Debug.Log(banResponse.is_banned);
				Debug.Log(banResponse.message);
				if (banResponse.is_banned == "True"){
					Ban(banResponse.message);
				}
				if (banResponse.is_god == "True"){
					BecomeGod();
				}
			}
			isFirst = "FALSE";
			yield return null;
		}

		IEnumerator UpdateBans(){
			while (true){
				Debug.Log(isFirst);
				StartCoroutine(HandleBan(GetSteamID(), isFirst, map));
				yield return new WaitForSeconds(10f);
			}
		}
	}
}