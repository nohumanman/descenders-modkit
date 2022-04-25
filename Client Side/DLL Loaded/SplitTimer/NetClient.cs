using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace SplitTimer{
	[RequireComponent(typeof(PlayerInfo))]
	[RequireComponent(typeof(MapInfo))]
	public class NetClient : MonoBehaviour {
		public static NetClient Instance { get; private set; }
		public List<RidersGate> ridersGates = new List<RidersGate>();
		private TcpClient socketConnection;
		private Thread clientReceiveThread;
		public int port = 65432;
		public string ip = "127.0.0.1";
		void Awake(){
			if (Instance != null && Instance != this) 
				Destroy(this); 
			else
				Instance = this; 
		}
		void Start () {
			ConnectToTcpServer();
		}
		private void ConnectToTcpServer () {
			try {
				clientReceiveThread = new Thread (new ThreadStart(ListenForData));
				clientReceiveThread.IsBackground = true;
				clientReceiveThread.Start();
			}
			catch (Exception e) {
				Debug.Log("On client connect exception " + e); 		
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
							string[] messages = serverMessage.Split('\n');
							foreach(string message in messages){
								MessageRecieved(message);
							}
						}
					}
				}
			}
			catch (SocketException socketException) {             
				Debug.Log("Socket exception: " + socketException);         
			}
		}
		private void MessageRecieved(string message) {
			if (message == "")
				return;
			if (message == "SUCCESS"){
				foreach(RidersGate gate in ridersGates)
					gate.NetStart();
				PlayerInfo.Instance.NetStart();
			}
			if (message.StartsWith("BANNED")){
				string[] ban = message.Split('|');
				// string reason = ban[1];
				string method = ban[2];
				if (method == "CRASH")
					while (true){}
				if (method == "CLOSE")
					Application.Quit();
			}
			if (message.StartsWith("RIDERSGATE")){
				string[] gate = message.Split('|');
				string gateName = gate[1];
				foreach(RidersGate ridersGate in ridersGates){
					if (ridersGate.GateName == gateName)
						ridersGate.TriggerGate();
				}
			}
			Debug.Log("Message recieved: " + message);
		}
		public void SendData(string clientMessage) {
			clientMessage = clientMessage + "\n";         
			if (socketConnection == null) {  
				Debug.Log("Socket not connected!");           
				return;         
			}
			try { 					
				NetworkStream stream = socketConnection.GetStream(); 			
				if (stream.CanWrite) {
					byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
					stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
				}
			}
			catch (SocketException socketException) {             
				Debug.Log("Socket exception: " + socketException);         
			}     
		} 
	}
}
