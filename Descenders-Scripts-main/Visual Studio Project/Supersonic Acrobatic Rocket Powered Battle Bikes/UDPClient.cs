using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;


namespace RocketLeagueMod
{
    class UDPClient: MonoBehaviour
    {
        public string current_version = "0.0.1";
        public Utilities utilities;

        Thread readThread;
        IPEndPoint IP;
        UdpClient client;

        public string ip = "86.137.143.195";
        public int port = 12000;

        bool threadStarted = false;
        public bool connectedToServer = false;

        public List<NetworkedGameObject> networkedGameObjects;

        public string lastReceivedPacket = "";

        public string uuid = "";

        void Start()
        {
            networkedGameObjects = new List<NetworkedGameObject>();
            utilities = Loader.gameObject.GetComponent<Utilities>();

            StartCoroutine(LateStart());
        }

        IEnumerator LateStart()
        {
            yield return new WaitForSeconds(6);
            DelayedStart();
        }

        void DelayedStart()
        {
            Debug.Log("Searching for Player Human's Name");

            // Set up Player_Human for networking.
            NetworkedGameObject humanNetworked = GameObject.Find("Player_Human").AddComponent<NetworkedGameObject>();
            Debug.Log("Added NetworkedGameObject");
            humanNetworked.IsHost = true;
            Debug.Log("Trying to find Player's name");
            string playerName = utilities.GetPlayerName();

            Debug.Log("Changing Unique Identifier");
            humanNetworked.UniqueIdentifier = playerName;
            networkedGameObjects.Add(humanNetworked);
            Debug.Log("Found local player, name is" + playerName);
            // end of Player_Human for networking.

            // set up ball for networking
            NetworkedGameObject ballNetworked = GameObject.Find("Ball").AddComponent<NetworkedGameObject>();
            if (playerName == "nohumanman")
                ballNetworked.IsHost = true;
            else
                ballNetworked.IsHost = false;
            Debug.Log("ballnetworked IsHost is " + ballNetworked.IsHost.ToString());
            ballNetworked.UniqueIdentifier = "MAINBALL";
            networkedGameObjects.Add(ballNetworked);
            // end of ball setup.

            // Set up all Player_Networked for networking.
            PlayerInfoImpact[] allPlayers = FindObjectsOfType<PlayerInfoImpact>();
            foreach (PlayerInfoImpact info in allPlayers)
            {
                Debug.Log("For this playerinfoimpact, the parent is " + info.gameObject.transform.root.gameObject.name);
                if (info.gameObject.transform.root.gameObject.name != "PROJECT_IMPACT")
                {
                    NetworkedGameObject x = info.gameObject.AddComponent<NetworkedGameObject>();
                    string name = utilities.GetNameFromPlayerInfo(info);
                    x.UniqueIdentifier = name;
                    x.IsHost = false;
                }
            }
            Debug.Log("ROCKET LEAGUE MOD LOADED!");

            StartCoroutine(UpdateEveryTenSeconds());
        }

        void UpdateNewNetworkedPlayers()
        {
            // Set up all Player_Networked for networking.
            PlayerInfoImpact[] allPlayers = FindObjectsOfType<PlayerInfoImpact>();
            foreach (PlayerInfoImpact info in allPlayers)
            {
                //Debug.Log("For this playerinfoimpact, the parent is " + info.gameObject.transform.root.gameObject.name);
                if (info.gameObject.transform.root.gameObject.name != "PROJECT_IMPACT" && info.gameObject.transform.root.GetComponent<NetworkedGameObject>() == null)
                    // if player is not the local player and player doesn't already have the networked gameobejct componetn attached.
                {
                    NetworkedGameObject x = info.gameObject.AddComponent<NetworkedGameObject>();
                    networkedGameObjects.Add(x);
                    string name = utilities.GetNameFromPlayerInfo(info);
                    x.UniqueIdentifier = name;
                    x.IsHost = false;
                }
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                ConnectToServer();
            }
        }

        void FixedUpdate()
        {
            GetNewMessage();
        }

        IEnumerator UpdateEveryTenSeconds()
        {
            while (true)
            {
                UpdateNewNetworkedPlayers();
                yield return new WaitForSeconds(10f);
            }
        }

        string GetNewMessage()
        {
            string lastMessage = getLatestPacket();
            if (lastMessage != "")
            {
                lastReceivedPacket = "";
                string[] messages = lastMessage.Split('\r');
                foreach (string message in messages)
                {
                    //Debug.Log(message);
                    string[] x = message.Split('|');

                    foreach (NetworkedGameObject networkedGameObject in networkedGameObjects)
                    {
                        if (networkedGameObject.UniqueIdentifier == x[0] && !networkedGameObject.IsHost)
                            networkedGameObject.UpdateValuesFromString(message);
                    }
                    return message;
                }
            }
            return null;
        }

        public void ConnectToServer()
        {
            if (connectedToServer)
                return;

            Debug.Log("Connecting to " + ip + ":" + port);
            IP = new IPEndPoint(IPAddress.Parse(ip), port);
            client = new UdpClient(ip, port);
            //client.Client.Blocking = false;
            //client.Client.ReceiveTimeout = 1000;
            // create thread for reading UDP messages
            threadStarted = true;
            readThread = new Thread(new ThreadStart(ReceiveData));
            readThread.IsBackground = true;
            readThread.Start();

            string name = utilities.GetPlayerName();
            name = name.Replace(",", "");
            name = name.Replace("\\", "");
            name = name.Replace("\t", "");
            StartCoroutine(SendData("LOGIN|" + name + "|VERSION|" + current_version));
        }

        public void DisconnectFromServer()
        {
            StartCoroutine(SendData("DISCONNECT"));
            connectedToServer = false;
            stopThread();
        }

        void OnDisable()
        {
            DisconnectFromServer();
        }

        public void OnServerConnected(string message)
        {
            uuid = message.Replace("\\Connected:", "");
            connectedToServer = true;
        }

        public void OnServerDisonnected()
        {
            connectedToServer = false;
            stopThread();
        }


        // Stop reading UDP messages
        public void stopThread()
        {
            if (!threadStarted)
                return;

            if (readThread.IsAlive)
                readThread.Abort();
            threadStarted = false;

            if (client != null)
                client.Close();
        }        

        public IEnumerator SendData(string message = "")
        {
            try
            {
                if (client != null)
                {
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    client.Send(data, data.Length);
                    //utilities.Log("SENT DATA \n" + message);
                }
            }
            catch (Exception e)
            {
                Debug.Log("Exception: " + e);
                if (connectedToServer)
                    OnServerDisonnected();
            }
            yield return null;
        }

        void ReceiveData()
        {
            while (threadStarted)
            {
                try
                {
                    // receive bytes
                    byte[] data = client.Receive(ref IP);
                    // encode UTF8-coded bytes to text format
                    string text = Encoding.UTF8.GetString(data);
                    // store new massage as latest message
                    lastReceivedPacket = text;
                    // update received messages
                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                }
            }
        }

        string getLatestPacket()
        {
            return lastReceivedPacket;
        }
    }
}
