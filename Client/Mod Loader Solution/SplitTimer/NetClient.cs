using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using ModLoaderSolution;

namespace SplitTimer{
	[RequireComponent(typeof(PlayerInfo))]
	[RequireComponent(typeof(MapInfo))]
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
			if (Instance != null && Instance != this) 
				Destroy(this); 
			else
				Instance = this;
			this.gameObject.AddComponent<Utilities>();
		}
		void Start () {
			Debug.Log("NetClient | Connecting to tcp server port " + port.ToString() + " with ip " + ip);
			ConnectToTcpServer();
			ridersGates = GameObject.FindObjectsOfType<RidersGate>();
		}
		void Update()
        {
			if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.C))
            {
				Physics.IgnoreLayerCollision(8, 8, PlayerCollision);
				PlayerCollision = !PlayerCollision;
			}
			if (Time.time - hasStarted > 30 && !socketConnection.Connected)
            {
				Debug.Log("NetClient | Disconnected! Reconecting now...");
				SplitTimerText.Instance.text.color = Color.red;
				SplitTimerText.Instance.text.text = "Server Disconnected.\n";
				ConnectToTcpServer();
            }
			foreach (string message in messages)
            {
                try
                {
					MessageRecieved(message);
				}
				catch (Exception ex)
                {
					Debug.Log("NetClient | MessageRecieved failed with '" + ex + "'");
                }
            }
			messages.Clear();
		}
		private void ConnectToTcpServer () {
			Debug.Log("NetClient | Connecting to TCP Server...");
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
		private void ListenForData() {
			try {
				socketConnection = new TcpClient(ip, port);
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
							{
								messages.Add(message);
							}
						}
					}
				}
			}
			catch (SocketException socketException) {             
				Debug.Log("NetClient | Socket exception - " + socketException);         
			}
		}
		private void MessageRecieved(string message) {
			Debug.Log("NetClient | Message Recieved: " + message);
			if (message == "")
				return;
			if (message == "SUCCESS") {
				PlayerInfo.Instance.NetStart();
				this.SendData("REP|" + gameObject.GetComponent<Utilities>().GetPlayerTotalRep());
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
						trail.leaderboardText.GetComponent<TextMesh>().text = trailName + " - Speedrun.com\n" + leaderboardInfo.LeaderboardAsString();
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
						trail.autoLeaderboardText.GetComponent<TextMesh>().text = trailName + " - Automatic\n" + leaderboardInfo.LeaderboardAsString();
					}
				}
			}
			if (message.StartsWith("TIMER_FINISH"))
            {
				string[] leaderboard = message.Split('|');
				string info = leaderboard[1];
				SplitTimerText.Instance.text.color = Color.green;
				SplitTimerText.Instance.text.text = info;
			}
			if (message.StartsWith("BANNED")) {
				string[] ban = message.Split('|');
				string method = ban[1];
				if (method == "CRASH")
					while (true) { }
				if (method == "CLOSE")
					Application.Quit();
			}
			if (message.StartsWith("RIDERSGATE")) {
				string[] gate = message.Split('|');
				float randomTime = float.Parse(gate[1]);
				foreach (RidersGate ridersGate in ridersGates) {
					ridersGate.TriggerGate(randomTime);
				}
			}
			if (message.StartsWith("TOGGLE_SPECTATOR"))
            {
				GetComponent<Utilities>().ToggleSpectator();
			}
			if (message.StartsWith("SPECTATE"))
            {
				string name = message.Split('|')[1];
				gameObject.GetComponent<Utilities>().SpectatePlayer(name);
            }
			if (message.StartsWith("SET_BIKE"))
            {
				int num = int.Parse(message.Split('|')[1]);
				gameObject.GetComponent<Utilities>().SetBike(num);
			}
			if (message.StartsWith("FREEZE_PLAYER"))
            {
				gameObject.GetComponent<Utilities>().FreezePlayer();
			}
			if (message.StartsWith("TOGGLE_CONTROL"))
            {
				string shouldStr = message.Split('|')[1];
				bool should = shouldStr == "true";
				gameObject.GetComponent<Utilities>().ToggleControl(should);
			}
			if (message.StartsWith("CLEAR_SESSION_MARKER"))
            {
				gameObject.GetComponent<Utilities>().ClearSessionMarker();
			}
			if (message.StartsWith("RESET_PLAYER"))
            {
				gameObject.GetComponent<Utilities>().ResetPlayer();
			}
			if (message.StartsWith("ADD_MODIFIER"))
            {
				string modifier = message.Split('|')[1];
				gameObject.GetComponent<Utilities>().AddGameModifier(modifier);
			}
			if (message.StartsWith("SPLIT_TIME"))
            {
				string splitTime = message.Split('|')[1];
				SplitTimerText.Instance.CheckpointTime(splitTime);
			}
			if (message.StartsWith("RESPAWN_ON_TRACK"))
            {
				gameObject.GetComponent<Utilities>().RespawnOnTrack();
			}
			if (message.StartsWith("RESPAWN_AT_START"))
            {
				gameObject.GetComponent<Utilities>().RespawnAtStartline();
			}
			if (message.StartsWith("INVALIDATE_TIME"))
            {
				string[] gate = message.Split('|');
				string reason = gate[1];
				SplitTimerText.Instance.count = false;
				SplitTimerText.Instance.text.text = reason + "\n";
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
				GetComponent<Utilities>().Gravity();
			}
			if (message.StartsWith("SET_REP"))
            {
				string[] gate = message.Split('|');
				gameObject.GetComponent<Utilities>().SetRep(int.Parse(gate[1]));
            }
			if (message.StartsWith("GET_REP"))
            {
				this.SendData("REP|" + gameObject.GetComponent<Utilities>().GetPlayerTotalRep());
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
				gameObject.GetComponent<Utilities>().EnableStats();
			}
			if (message.StartsWith("TOGGLE_GOD"))
            {
				gameObject.GetComponent<Utilities>().ToggleGod();
			}
			SendData("pong");
			Debug.Log("NetClient | Message Processed: " + message);
		}
		public void SendData(string clientMessage) {
			clientMessage = clientMessage + "\n";
			Debug.Log("NetClient | Client sending message: " + clientMessage);
			if (socketConnection == null) {
				Debug.Log("NetClient | Socket not connected!");
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
			{
				socketConnection.Close();
			}
		}
	}
}
