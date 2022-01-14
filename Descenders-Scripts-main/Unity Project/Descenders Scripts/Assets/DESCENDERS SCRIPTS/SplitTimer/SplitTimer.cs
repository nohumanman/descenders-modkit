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
		public SplitTimerAPI api;
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
			api = gameObject.GetComponent<SplitTimerAPI>();
			Debug.Log("SplitTimer - Started! Using server " + api.server + " on port " + api.port + ". In world" + world_name);
			string steam_name = steamIntegration.getName();
			string steam_id = steamIntegration.getSteamId();
			api.LoadIntoMap(world_name, steam_name, steam_id);
		}
	}
}
