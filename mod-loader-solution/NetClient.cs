using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Security.Cryptography;

namespace ModLoaderSolution
{
	public enum DebugType
    {
		DEVELOPER, DEBUG, RELEASE
    }
	public class NetClient : MonoBehaviour {
		public static NetClient Instance { get; private set; }
		public RidersGate[] ridersGates;
		private TcpClient socketConnection;
		float hasStarted = Time.time;
		private Thread clientReceiveThread;
		bool PlayerCollision = false;
		List<string> messages = new List<string>();
		public int port = 65432;
		public string ip = "86.26.185.112";
		static string version = "0.3.01";
		static bool quietUpdate = false;
		static string patchNotes = "- Errors caused by descenders update should be resolved\n- Framerate should be improved\n- Ragesquid should push new fix sometime 09-08\n\n\nYours,\n- nohumanman"; // that which has changed since the last version.
		public static DebugType debugState = DebugType.RELEASE;
        void Awake(){
			Utilities.LogMethodCallStart();
			if (debugState == DebugType.DEVELOPER)
				ip = "localhost";
			DontDestroyOnLoad(this.gameObject.transform.root);
			if (Instance != null && Instance != this) 
				Destroy(this); 
			else
				Instance = this;
			this.gameObject.AddComponent<Utilities>();
			Utilities.Log("Version number " + version);
			Application.logMessageReceived += Log;
			Utilities.LogMethodCallEnd();
		}
		public static string GetVersion()
        {
			if (debugState == DebugType.DEVELOPER)
				return version + "-dev";
			else if (debugState == DebugType.DEBUG)
				return version + "-debug";
			else
				return version;
        }
		void Start () {
            Utilities.LogMethodCallStart();
			Utilities.Log("Connecting to tcp server port " + port.ToString() + " with ip '" + ip + "'");
			ConnectToTcpServer();
			ridersGates = FindObjectsOfType<RidersGate>();
			if (new PlayerIdentification.SteamIntegration().getName() == "Descender")
            {
				Utilities.instance.ToggleGod();
            }
			Utilities.LogMethodCallEnd();
		}
		public bool IsConnected()
        {
			return socketConnection != null && socketConnection.Connected;
		}
		bool poppedUp = false;
		void Update()
        {
            Utilities.LogMethodCallStart();
			if (!poppedUp && Utilities.GetPlayer() != null)
            {
				string lastVersion = "";
				try
				{
					lastVersion = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low\\RageSquid\\Descenders\\last_version.txt");
				}
				catch(Exception){
				}
				if (lastVersion != NetClient.version && !quietUpdate)
                {
					try
					{
						Utilities.instance.PopUp("Modkit patch notes " + NetClient.version, NetClient.patchNotes);
						poppedUp = true;
					}
					catch (Exception) { }
				}
				File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low\\RageSquid\\Descenders\\last_version.txt", NetClient.version);
			}
			if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.P))
				Application.OpenURL("https://modkit.nohumanman.com/trails");
			if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.C))
            {
				UserInterface.Instance.SpecialNotif("Collisions enabled: " + (!PlayerCollision).ToString());
				Physics.IgnoreLayerCollision(8, 8, PlayerCollision);
				PlayerCollision = !PlayerCollision;
			}
			if (Time.time - hasStarted > 10 && (socketConnection == null || !socketConnection.Connected))
            {
				Utilities.Log("Disconnected! Reconnecting now...");
                // SplitTimerText.Instance.count = false;
                SplitTimerText.Instance.text.color = Color.red;
				SplitTimerText.Instance.checkpointTime = "";
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
					catch (Exception){ }
				}
				messages.Clear();
			}
			catch (InvalidOperationException){}
			Utilities.LogMethodCallEnd();
		}
		private void ConnectToTcpServer () {
			Utilities.LogMethodCallStart();
			Utilities.Log("Connecting to TCP Server");
			hasStarted = Time.time;
			try {
				clientReceiveThread = new Thread (new ThreadStart(ListenForData));
				clientReceiveThread.IsBackground = true;
				clientReceiveThread.Start();
				hasStarted = Time.time;
			}
			catch (Exception e) {
				Utilities.Log("On client connect exception " + e); 		
			}
			Utilities.LogMethodCallEnd();
		}
		public void Log(string logString, string stackTrace, LogType type)
		{
			//SendData("LOG_LINE", logString);
		}
		public void Log(string logString)
        {
			//SendData("LOG_LINE", logString);
        }
		public IEnumerator UploadOutputLog()
		{
			Utilities.LogMethodCallStart();
			string replayLocation = (
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
				+ "Low\\RageSquid\\Descenders\\output_log.txt"
			);
			Byte[] bytes = System.IO.File.ReadAllBytes(replayLocation);

			WWWForm form = new WWWForm();
			form.AddField("player_id", new PlayerIdentification.SteamIntegration().getSteamId());
			form.AddBinaryData("replay", bytes, "replay");

			using (UnityWebRequest www = UnityWebRequest.Post(
				"https://modkit.nohumanman.com/upload-outputlog",
				form
			))
			{
				yield return www.SendWebRequest();

				if (www.isNetworkError || www.isHttpError)
				{
					Utilities.Log(www.error);
				}
				else
				{
					Utilities.Log("Upload complete!");
				}
			}
			Utilities.LogMethodCallEnd();
		}
		public IEnumerator UploadReplay(string replay, string time_id)
        {
			Utilities.LogMethodCallStart();
			Byte[] bytes = System.IO.File.ReadAllBytes(replay);
			System.IO.File.Delete(replay);

			WWWForm form = new WWWForm();
			form.AddField("time_id", time_id);
			form.AddBinaryData("replay", bytes, "replay");

			using (UnityWebRequest www = UnityWebRequest.Post(
				"https://modkit.nohumanman.com/upload-replay",
				form
			))
			{
				yield return www.SendWebRequest();

				if (www.isNetworkError || www.isHttpError)
				{
					Utilities.Log(www.error);
				}
				else
				{
					Utilities.Log("Upload complete!");
				}
			}
			Utilities.LogMethodCallEnd();
		}
		private void ListenForData() {
			Utilities.LogMethodCallStart();
			try {
				Utilities.Log("Creating TcpClient()");
				socketConnection = new TcpClient(ip, port);
				Utilities.Log("TcpClient created!");
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
				Utilities.Log("Socket exception in ListenForData()");         
			}
			Utilities.LogMethodCallEnd();
		}
		public void NetStart()
        {
			Utilities.LogMethodCallStart();
			PlayerManagement.Instance.NetStart();
			foreach (MedalSystem medalSystem in FindObjectsOfType<MedalSystem>())
				medalSystem.NetStart();
			this.SendData("REP", Utilities.instance.GetPlayerTotalRep());
			foreach (Boundary b in FindObjectsOfType<Boundary>())
				b.ForceUpdate(); // force tell the server what boundaries we are in.
			Utilities.LogMethodCallEnd();
		}
		private void MessageRecieved(string message) {
			Utilities.LogMethodCallStart();
			if (message == "")
				return;
			Utilities.Log("Message Recieved: " + message);
			if (message == "SUCCESS") {
				NetStart();
			}
            if (message.StartsWith("RECEIVED"))
            {
                // not waiting for a response anymore
                hashesAwaiting.Remove(message.Split('|')[1]);
            }
			if (message.StartsWith("ROTATE|"))
            {
				string rotate = message.Split('|')[1];
				int rotateInt = int.Parse(rotate);
				Utilities.GetPlayer().transform.Rotate(new Vector3(0, rotateInt, 0));
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
				Vector3 pos = Utilities.GetPlayer().transform.position;
				Utilities.Log("Current Position: " + pos.ToString());
				SendData("POS", pos.x, pos.y, pos.z);
            }
			
			if (message.StartsWith("CHAT_MESSAGE"))
            {
				string user = message.Split('|')[1];
				string map = message.Split('|')[2];
				string mess = message.Split('|')[3];
				Chat.instance.GetMessage(user, map, mess);
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
						trail.autoLeaderboardText.GetComponent<TextMesh>().text = trailName + "\n" + leaderboardInfo.LeaderboardAsString();
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
					if (!Utilities.instance.isInPauseMenu()) // not in pause menu
						ridersGate.TriggerGate(randomTime);
			}
			if (message.StartsWith("LOG_GAMEOBJECTS"))
			{
				foreach (GameObject go in FindObjectsOfType<GameObject>())
					Utilities.Log(go);
			}
			if (message.StartsWith("TOGGLE_SPECTATOR"))
            {
				GetComponent<Utilities>().ToggleSpectator();
			}
			if (message.StartsWith("SPECTATE"))
            {
				string id = message.Split('|')[1];
				Utilities.instance.SpectatePlayerCustom(id);
            }
			if (message.StartsWith("SET_BIKE"))
            {
				string new_bike = message.Split('|')[1];
				string steam_id;
				try { steam_id = message.Split('|')[2]; }
                catch{ steam_id = (new PlayerIdentification.SteamIntegration().id).ToString(); }
				FindObjectOfType<BikeSwitcher>().ToBike(new_bike, steam_id);
			}
			if (message.StartsWith("GET_IDS"))
            {
				foreach (global::PlayerInfo inf in Singleton<PlayerManager>.SP.GetAllPlayers())
                    Utilities.Log(Utilities.FromPlayerInfo(inf).steamID);
			}
			if (message.StartsWith("FREEZE_PLAYER"))
            {
				Utilities.instance.FreezePlayer();
			}
			if (message.StartsWith("POPUP"))
			{
				string title = message.Split('|')[1];
				string body = message.Split('|')[2];
				Utilities.instance.PopUp(title, body);
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
			if (message.StartsWith("NON_MODKIT_TRAIL"))
            {
				string url = message.Split('|')[1];
				bool proceed = true;
				foreach (Trail trail in FindObjectsOfType<Trail>())
					if (trail.url == url)
						proceed = false;
				if (proceed)
                {
					GameObject trailParent = GameObject.CreatePrimitive(PrimitiveType.Cube);
					Trail tr = trailParent.AddComponent<Trail>();
					tr.LoadFromUrl("https://modkit.nohumanman.com/static/trails/" + url);
				}
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
				StopCoroutine("DisableTimerText");
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
				this.SendData("REP", Utilities.instance.GetPlayerTotalRep());
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
			_SendData("pong", "");
			Utilities.LogMethodCallEnd();
		}
		IEnumerator SendDataDelayed(string clientMessage, float time)
        {
			yield return new WaitForSeconds(time);
			SendData(clientMessage);
        }
		public string clean(string mess)
        {
			foreach (char c in mess)
			{
				if (c < 32 || c > 126)
					mess = mess.Replace(c.ToString(), "?");
			}
			return mess;
		}
        static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        public List<string> hashesAwaiting = new List<string>();
        IEnumerator WaitForHash(string hashToWaitFor, string message)
        {
            yield return new WaitForSeconds(10f);
            if (hashesAwaiting.Contains(hashToWaitFor))
            {
                _SendData(message, hashToWaitFor); // we failed, so we will try again.
                hashesAwaiting.Remove(hashToWaitFor); // and forget about it
            }
        }
        public void SendData(params object[] data)
        {
			string clientMessage = "";
			foreach (object arg in data) 
				clientMessage += clean(arg.ToString()) + "|";
            clientMessage += Time.time + "|"; // so no hashes are the same
            string hash = ComputeSha256Hash(clientMessage);
            clientMessage += hash;
            hashesAwaiting.Add(hash);
            StartCoroutine(WaitForHash(hash, clientMessage));
            _SendData(clientMessage, hash);
        }
		void _SendData(string clientMessage, string hash) {
			Utilities.LogMethodCallStart();
			// Utilities.Log("Client sending message: " + clientMessage);
			// clean clientMessage
			if (!clientMessage.EndsWith("\n"))
				clientMessage = clientMessage + "\n";
			if (socketConnection == null || !socketConnection.Connected)
			{
				StartCoroutine(SendDataDelayed(clientMessage, 1)); // wait a second
                if (hash != "")
                    hashesAwaiting.Remove(hash); // not going to get a response from this one
				return;
            }
            try
            {
                NetworkStream stream = socketConnection.GetStream();
                if (stream.CanWrite)
                {
                    byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
                    stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
                    stream.Flush(); // Ensure data is flushed immediately (prevent truncation)
                }
            }
            catch (SocketException socketException)
            {
                Utilities.Log("Socket exception: " + socketException);
            }
			Utilities.LogMethodCallEnd();
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
