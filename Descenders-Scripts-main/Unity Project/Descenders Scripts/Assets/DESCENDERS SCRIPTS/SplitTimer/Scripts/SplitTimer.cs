using UnityEngine;
using PlayerIdentification;
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
				Destroy(this.gameObject);
			}
			else {
				_instance = this;
			}
		}
		void Start () {
			Debug.Log("SplitTimer - Started! Using server '" + splitTimerApi.server + "'. In world '" + world_name + "'");
			splitTimerApi.OnMapEnter(this);
		}
		void OnDisable(){
			Debug.Log("SplitTimer.SplitTimer - OnDisable()");
			splitTimerApi.OnMapExit();
		}
		public void OnPlayerBanned(){
			Debug.Log("SplitTimer.SplitTimer - OnPlayerBanned()");
			Destroy(GameObject.Find("Player_Human"));
		}
	}
}
