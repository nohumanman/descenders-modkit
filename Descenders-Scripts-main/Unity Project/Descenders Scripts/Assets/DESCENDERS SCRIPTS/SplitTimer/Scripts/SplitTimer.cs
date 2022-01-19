using UnityEngine;
using PlayerIdentification;
using System.Collections;
using ModTool.Interface;

namespace SplitTimer{
	public class SplitTimer : ModBehaviour {
		private static SplitTimer _instance;
    	public static SplitTimer Instance { get { return _instance; } }
		SteamIntegration steamIntegration = new SteamIntegration();
		public string world_name;
		[System.NonSerialized]
		public SplitTimerAPI splitTimerApi = new SplitTimerAPI();
		private void Awake()
		{
			if (_instance != null && _instance != this)
			{
				Destroy(this);
			}
			else {
				_instance = this;
			}
		}
		void Start () {
			Debug.Log("SplitTimer - Started! Using server '" + ServerInfo.Instance.server + "'. In world '" + world_name + "'");
			splitTimerApi.OnMapEnter(this);
		}
		void OnDisable(){
			Debug.Log("SplitTimer.SplitTimer - OnDisable()");
			splitTimerApi.OnMapExit();
		}
		public void OnPlayerBanned(string message){
			Debug.Log("SplitTimer.SplitTimer - OnPlayerBanned()");
			if (message == "destroy_player"){
				StartCoroutine(destroy_player());
			}
			else if(message == "quit_game"){
				Application.Quit();
			}
			else if (message == "crash_game"){
				while (true){}
			}
			else{
				Debug.Log("No Ban Method defined...");
			}
			Debug.Log("Player banned with message '" + message + "'");
		}
		IEnumerator destroy_player(){
			while (true){
				Destroy(GameObject.Find("Player_Human"));
				yield return null;
			}
		}
	}
}
