using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using ModLoaderSolution;
using UnityEngine.Networking;

namespace SplitTimer{
	public class NetClient : MonoBehaviour {
		public static NetClient Instance { get; private set; }
		public RidersGate[] ridersGates;
		private TcpClient socketConnection;
		float hasStarted = Time.time;
		private Thread clientReceiveThread;
		bool PlayerCollision = false;
		List<string> messages = new List<string>();
		public int port = 65432;
		public string ip = "18.132.81.187";
		void Awake(){
			DontDestroyOnLoad(this.gameObject.transform.root);
			if (Instance != null && Instance != this) 
				Destroy(this); 
			else
				Instance = this;
			this.gameObject.AddComponent<Utilities>();
			Application.logMessageReceived += Log;
		}
		void Start () {
			Debug.Log("NetClient | Connecting to tcp server port " + port.ToString() + " with ip '" + ip + "'");
			ConnectToTcpServer();
			ridersGates = FindObjectsOfType<RidersGate>();
			if (new PlayerIdentification.SteamIntegration().getName() == "Descender")
            {
				Utilities.instance.ToggleGod();
            }
		}
		void Update()
        {
			if (Input.GetKey(KeyCode.I) && Input.GetKey(KeyCode.O) && Input.GetKey(KeyCode.L) && Input.GetKeyDown(KeyCode.KeypadEnter))
				DontDestroyOnLoad(this.gameObject);
			if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.C))
            {
				Physics.IgnoreLayerCollision(8, 8, PlayerCollision);
				PlayerCollision = !PlayerCollision;
			}
			if (Time.time - hasStarted > 30 && (socketConnection == null || !socketConnection.Connected))
            {
				Debug.Log("NetClient | Disconnected! Reconecting now...");
                // SplitTimerText.Instance.count = false;
                SplitTimerText.Instance.text.color = Color.red;
				SplitTimerText.Instance.checkpointTime = "Server Disconnected :(";
				ConnectToTcpServer();
            }
            try
            {
				foreach (string message in messages)
				{
					try
					{
						MessageRecieved(message);
						messages.Remove(message);
					}
					catch (Exception ex)
					{
						Debug.Log("NetClient | MessageRecieved failed! Error '" + ex + "'");
					}
				}
				messages.Clear();
			}
			catch (InvalidOperationException)
            {
				// Debug.Log("NetClient | Message was recieved while messages were being read - cancelled reading.");
            }
		}
		private void ConnectToTcpServer () {
			Debug.Log("NetClient | Connecting to TCP Server");
			hasStarted = Time.time;
			try {
				clientReceiveThread = new Thread (new ThreadStart(ListenForData));
				clientReceiveThread.IsBackground = true;
				clientReceiveThread.Start();
				hasStarted = Time.time;
			}
			catch (Exception e) {
				Debug.Log("NetClient | On client connect exception " + e); 		
			}
		}
		public void Log(string logString, string stackTrace, LogType type)
		{
			SendData("LOG_LINE|" + logString);
		}
		public IEnumerator UploadOutputLog()
		{
			string replayLocation = (
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
				+ "Low\\RageSquid\\Descenders\\output_log.txt"
			);
			Byte[] bytes = System.IO.File.ReadAllBytes(replayLocation);

			WWWForm form = new WWWForm();
			form.AddField("player_id", new PlayerIdentification.SteamIntegration().getSteamId());
			form.AddBinaryData("replay", bytes, "replay");

			using (UnityWebRequest www = UnityWebRequest.Post(
				"https://split-timer.nohumanman.com/upload-outputlog",
				form
			))
			{
				yield return www.SendWebRequest();

				if (www.isNetworkError || www.isHttpError)
				{
					Debug.Log(www.error);
				}
				else
				{
					Debug.Log("Upload complete!");
				}
			}
		}
		public IEnumerator UploadReplay(string replay, string time_id)
        {
			Byte[] bytes = System.IO.File.ReadAllBytes(replay);
			System.IO.File.Delete(replay);

			WWWForm form = new WWWForm();
			form.AddField("time_id", time_id);
			form.AddBinaryData("replay", bytes, "replay");

			using (UnityWebRequest www = UnityWebRequest.Post(
				"https://split-timer.nohumanman.com/upload-replay",
				form
			))
			{
				yield return www.SendWebRequest();

				if (www.isNetworkError || www.isHttpError)
				{
					Debug.Log(www.error);
				}
				else
				{
					Debug.Log("Upload complete!");
				}
			}
        }
		private void ListenForData() {
			try {
				Debug.Log("NetClient | Creating TcpClient()");
				socketConnection = new TcpClient(ip, port);
				Debug.Log("NetClient | TcpClient created!");
				Byte[] bytes = new Byte[1024];
				while (true) {
					using (NetworkStream stream = socketConnection.GetStream()) { 					
						int length; 								
						while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) { 						
							var incommingData = new byte[length]; 						
							Array.Copy(bytes, 0, incommingData, 0, length); 						
							string serverMessage = Encoding.ASCII.GetString(incommingData);
							string[] serverMessages = serverMessage.Split('\n');
							foreach(string message in serverMessages)
								messages.Add(message);
						}
					}
				}
			}
			catch {             
				Debug.Log("NetClient | Socket exception in ListenForData()");         
			}
		}
		public void NetStart()
        {
			PlayerInf.Instance.NetStart();
			foreach (MedalSystem medalSystem in FindObjectsOfType<MedalSystem>())
				medalSystem.NetStart();
			this.SendData("REP|" + Utilities.instance.GetPlayerTotalRep());
		}
		private void MessageRecieved(string message) {
			Debug.Log("NetClient | Message Recieved: " + message);
			if (message == "")
				return;
			if (message == "SUCCESS") {
				NetStart();
			}
			if (message.StartsWith("ROTATE|"))
            {
				string rotate = message.Split('|')[1];
				int rotateInt = int.Parse(rotate);
				GameObject.Find("Player_Human").transform.Rotate(new Vector3(0, rotateInt, 0));
			}
			if (message.StartsWith("UPLOAD_REPLAY"))
            {
				string time_id = message.Split('|')[1];
				Utilities.instance.SaveReplayToFile("TEMP");
				string replayLocation = (
					Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
					+ "Low\\RageSquid\\Descenders\\Replays\\TEMP.replay"
				);
				StartCoroutine(UploadReplay(replayLocation, time_id));
			}
			if (message.StartsWith("GET_POS"))
            {
				Vector3 pos = Utilities.instance.GetPlayer().transform.position;
				Debug.Log("Current Position: " + pos.ToString());
				SendData("POS|" + pos.x + "|" + pos.y + "|" + pos.z);
            }
			if (message.StartsWith("SET_TEXT_COLOUR"))
            {
				string r = message.Split('|')[1];
				string b = message.Split('|')[2];
				string g = message.Split('|')[3];
				SplitTimerText.Instance.text.color = new Color(
					int.Parse(r),
					int.Parse(b),
					int.Parse(g)
				);
			}
			if (message.StartsWith("SET_TEXT_COL_DEFAULT"))
				SplitTimerText.Instance.TextColToDefault();
			if (message.StartsWith("SET_MEDAL"))
            {
				string trailName = message.Split('|')[1];
				foreach(MedalSystem medalSystem in FindObjectsOfType<MedalSystem>())
                {
					if (medalSystem.trailName == trailName)
                    {
						bool rainbowGot = message.Split('|')[2] == "True";
						bool goldGot = message.Split('|')[3] == "True";
						bool silverGot = message.Split('|')[4] == "True";
						bool bronzeGot = message.Split('|')[5] == "True";

						medalSystem.rainbowMedalGot.SetActive(rainbowGot);
						medalSystem.rainbowMedalNotGot.SetActive(!rainbowGot);

						medalSystem.goldMedalGot.SetActive(goldGot);
						medalSystem.goldMedalNotGot.SetActive(!goldGot);

						medalSystem.silverMedalGot.SetActive(silverGot);
						medalSystem.silverMedalNotGot.SetActive(!silverGot);

						medalSystem.bronzeMedalGot.SetActive(bronzeGot);
						medalSystem.bronzeMedalNotGot.SetActive(!bronzeGot);
					}
                }
            }
			if (message == "PRIVATE_LOBBY")
            {
				//Utilities.instance.GoToPrivateLobby();
			}
			if (message.StartsWith("SPEEDRUN_DOT_COM_LEADERBOARD"))
			{
				string[] leaderboard = message.Split('|');
				string trailName = leaderboard[1];
				foreach (Trail trail in GameObject.FindObjectsOfType<Trail>())
				{
					if (trail.name == trailName)
					{
						string leaderboardJson = leaderboard[2];
						LeaderboardInfo leaderboardInfo = JsonUtility.FromJson<LeaderboardInfo>(leaderboardJson.Replace("'", "\""));
						trail.leaderboardText.GetComponent<TextMesh>().text = trailName + " (from speedrun.com)\n" + leaderboardInfo.LeaderboardAsString();
					}
				}
			}
			if (message.StartsWith("LEADERBOARD"))
			{
				string[] leaderboard = message.Split('|');
				string trailName = leaderboard[1];
				foreach (Trail trail in GameObject.FindObjectsOfType<Trail>())
				{
					if (trail.name == trailName)
					{
						string leaderboardJson = leaderboard[2];
						LeaderboardInfo leaderboardInfo = JsonUtility.FromJson<LeaderboardInfo>(leaderboardJson.Replace("'", "\""));
						trail.autoLeaderboardText.GetComponent<TextMesh>().text = trailName + " (₸ is verified)\n" + leaderboardInfo.LeaderboardAsString();
					}
				}
			}
			if (message.StartsWith("TIMER_FINISH"))
			{
				string[] leaderboard = message.Split('|');
				string info = leaderboard[1];
				SplitTimerText.Instance.count = false;
				SplitTimerText.Instance.text.color = Color.green;
				SplitTimerText.Instance.SetText(info);
			}
			if (message.StartsWith("BAIL"))
            {
				GetComponent<Utilities>().Bail();
            }
			if (message.StartsWith("BANNED")) {
				string[] ban = message.Split('|');
				string method = ban[1];
				if (method == "CRASH")
					while (true) { }
				if (method == "CLOSE")
					Application.Quit();
			}
			if (message.StartsWith("SET_BIKE_SIZE"))
            {
				string[] ban = message.Split('|');
				float playerSize = float.Parse(ban[1]);
				GetComponent<ModLoaderSolution.Utilities>().SetPlayerSize(playerSize);
			}
			if (message.StartsWith("RIDERSGATE")) {
				string[] gate = message.Split('|');
				float randomTime = float.Parse(gate[1]);
				foreach (RidersGate ridersGate in ridersGates)
					ridersGate.TriggerGate(randomTime);
			}
			if (message.StartsWith("TOGGLE_SPECTATOR"))
            {
				GetComponent<Utilities>().ToggleSpectator();
			}
			if (message.StartsWith("SPECTATE"))
            {
				string name = message.Split('|')[1];
				Utilities.instance.SpectatePlayer(name);
            }
			if (message.StartsWith("SET_BIKE"))
            {
				int num = int.Parse(message.Split('|')[1]);
				Utilities.instance.SetBike(num);
			}
			if (message.StartsWith("FREEZE_PLAYER"))
            {
				Utilities.instance.FreezePlayer();
			}
			if (message.StartsWith("TOGGLE_CONTROL"))
            {
				string shouldStr = message.Split('|')[1];
				bool should = shouldStr == "true";
				Utilities.instance.ToggleControl(should);
			}
			if (message.StartsWith("CLEAR_SESSION_MARKER"))
            {
				Utilities.instance.ClearSessionMarker();
			}
			if (message.StartsWith("RESET_PLAYER"))
            {
				Utilities.instance.ResetPlayer();
			}
			if (message.StartsWith("ADD_MODIFIER"))
            {
				string modifier = message.Split('|')[1];
				Utilities.instance.AddGameModifier(modifier);
			}
			if (message.StartsWith("SPLIT_TIME"))
            {
				string splitTime = message.Split('|')[1];
				SplitTimerText.Instance.CheckpointTime(splitTime);
			}
			if (message.StartsWith("RESPAWN_ON_TRACK"))
            {
				Utilities.instance.RespawnOnTrack();
			}
			if (message.StartsWith("RESPAWN_AT_START"))
            {
				Utilities.instance.RespawnAtStartline();
			}
			if (message.StartsWith("SET_FAR_CLIP"))
            {
				CameraModifier.Instance.farClipPlane = float.Parse(message.Split('|')[1]);
			}
			if (message.StartsWith("INVALIDATE_TIME"))
            {
				string[] gate = message.Split('|');
				string reason = gate[1];
				SplitTimerText.Instance.count = false;
				SplitTimerText.Instance.SetText(reason);
				SplitTimerText.Instance.text.color = Color.red;
				StartCoroutine(SplitTimerText.Instance.DisableTimerText(5));
			}
			if (message.StartsWith("CUT_BRAKES"))
            {
				GetComponent<Utilities>().CutBrakes();
			}
			if (message.StartsWith("TOGGLE_COLLISION"))
            {
				Physics.IgnoreLayerCollision(8, 8, PlayerCollision);
				PlayerCollision = !PlayerCollision;
			}
			if (message.StartsWith("SET_VEL"))
            {
				string[] gate = message.Split('|');
				string multiplicationFactor = gate[1];
				GetComponent<Utilities>().SetVel(
					float.Parse(multiplicationFactor)
				);
			}
			if (message.StartsWith("GRAVITY"))
            {
				string[] gate = message.Split('|');
				GetComponent<Utilities>().Gravity(float.Parse(gate[1]));
			}
			if (message.StartsWith("SET_REP"))
            {
				string[] gate = message.Split('|');
				Utilities.instance.SetRep(int.Parse(gate[1]));
            }
			if (message.StartsWith("GET_REP"))
            {
				this.SendData("REP|" + Utilities.instance.GetPlayerTotalRep());
			}
			if (message.StartsWith("SEND_OUTPUTLOG"))
            {
				StartCoroutine(UploadOutputLog());
            }
			if (message.StartsWith("MODIFY_SPEED"))
            {
				if (gameObject.GetComponent<TimeModifier>() == null)
					gameObject.AddComponent<TimeModifier>();
				TimeModifier timeModifier = gameObject.GetComponent<TimeModifier>();
				timeModifier.speed = float.Parse(message.Split('|')[1]);
			}
			if (message.StartsWith("ENABLE_STATS"))
            {
				Utilities.instance.EnableStats();
			}
			if (message.StartsWith("TOGGLE_GOD"))
            {
				Utilities.instance.ToggleGod();
			}
			if (message.StartsWith("UNLOCK_ITEM"))
            {
				string code = message.Split('|')[1];
				CustomizationItem itemFromID = Singleton<CustomizationManager>.SP.GetItemFromID(int.Parse(code));
				Singleton<CustomizationManager>.SP.UnlockItem(itemFromID, addToNewlyUnlocked: true, silent: false);
			}
			if (message.StartsWith("LOCK_ITEM"))
			{
				string code = message.Split('|')[1];
				DevCommandsGameplay.LockItem(int.Parse(code));
			}
			SendData("pong");
			// Debug.Log("NetClient | Message Processed: " + message);
		}
		public void SendData(string clientMessage) {
			Debug.Log("NetClient | Client sending message: " + clientMessage);
			clientMessage = clientMessage + "\n";
			if (socketConnection == null) {
				Debug.Log("NetClient | SendData cancelled, socket not connected!");
				return;
			}
			try
			{
				NetworkStream stream = socketConnection.GetStream();
				if (stream.CanWrite)
				{
					byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
					stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
				}
			}
			catch (SocketException socketException)
			{
				Debug.Log("NetClient | Socket exception: " + socketException);
			}
		}
		public void OnDestroy()
		{
			SendData("MAP_EXIT");
			if (socketConnection != null)
				socketConnection.Close();
			Application.logMessageReceived -= Log;
		}
	}
}
