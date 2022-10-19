using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ModLoaderSolution;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;

namespace SplitTimer
{
    public struct PlayerInfoForTeleport
    {
        public string Name;
        public GameObject Player;
    }
    public class UserInterface : MonoBehaviour {
        bool isActive;
        bool hasBeenActive = false;
        public PlayerInfoImpact[] players;

        bool __COMMANDS__ = false;
        bool __CHECKPOINTS__ = false;
        bool __PLAYERS__ = false;
        bool __STATS__ = false;
        bool __QoL__ = false;
        void OnGUI()
        {
            if (isActive)
            {
                hasBeenActive = true;
                GUIStyle myButtonStyle = new GUIStyle(GUI.skin.button);
                GUI.skin.font = Font.CreateDynamicFontFromOSFont("Rockwell", 12);
                if (GUI.Button(new Rect(10, 10, 150, 25), " \\/ COMMANDS \\/", myButtonStyle))
                    __COMMANDS__ = !__COMMANDS__;
                if (__COMMANDS__)
                {
                    if (GUI.Button(new Rect(10, 45, 150, 25), "Bail()", myButtonStyle))
                        Utilities.instance.Bail();
                    if (GUI.Button(new Rect(10, 70, 150, 25), "ApplyStupidModifiers()", myButtonStyle))
                        FindObjectOfType<StatsModification>().ApplyStupidModifiers();
                    if (GUI.Button(new Rect(10, 95, 150, 25), "ToggleFly()", myButtonStyle))
                        Utilities.instance.ToggleFly();
                    if (GUI.Button(new Rect(10, 120, 150, 25), "Dissapear()", myButtonStyle))
                        Utilities.instance.ToggleSpectator();
                    if (GUI.Button(new Rect(10, 145, 150, 25), "CutBrakes()", myButtonStyle))
                        Utilities.instance.CutBrakes();
                    if (GUI.Button(new Rect(10, 170, 150, 25), "TogglePlayerCollision()", myButtonStyle))
                        Utilities.instance.TogglePlayerCollision();
                    if (GUI.Button(new Rect(10, 195, 150, 25), "Gravity(4.4f)", myButtonStyle))
                        Utilities.instance.Gravity(4.4f);
                    if (GUI.Button(new Rect(10, 220, 150, 25), "Gravity(9.81f)"))
                        Utilities.instance.Gravity(9.81f);
                    if (GUI.Button(new Rect(10, 245, 150, 25), "Gravity(16f)"))
                        Utilities.instance.Gravity(16f);
                    if (GUI.Button(new Rect(10, 270, 150, 25), "Gravity(20f)"))
                        Utilities.instance.Gravity(20f);
                    if (GUI.Button(new Rect(10, 295, 150, 25), "ToggleGod()"))
                        Utilities.instance.ToggleGod();
                    if (GUI.Button(new Rect(10, 320, 150, 25), "EnableStats()"))
                        Utilities.instance.EnableStats();
                    if (GUI.Button(new Rect(10, 345, 150, 25), "SpawnAtCursor()"))
                        Utilities.instance.SpawnAtCursor();
                    if (GUI.Button(new Rect(10, 370, 150, 25), "ToggleCustomCam()") && FindObjectOfType<MovableCam>() != null)
                        FindObjectOfType<MovableCam>().ToggleCustomCam();
                }
                int yPos = 10;
                if (GUI.Button(new Rect(160, yPos, 150, 25), "\\/ CHECKPOINTS  \\/"))
                    __CHECKPOINTS__ = !__CHECKPOINTS__;
                yPos += 40;
                if (__CHECKPOINTS__)
                {
                    foreach (GameObject point in Utilities.instance.GetCheckpointObjects())
                    {
                        if (GUI.Button(new Rect(160, yPos, 150, 25), point.name))
                            Utilities.instance.GetPlayer().transform.position = point.transform.position;
                        yPos += 25;
                    }
                }
                yPos = 10;
                if (GUI.Button(new Rect(310, yPos, 150, 25), "\\/ Players \\/"))
                    __PLAYERS__ = !__PLAYERS__;
                yPos += 40;
                if (__PLAYERS__)
                {
                    foreach (PlayerInfoImpact x in players)
                    {
                        if (GUI.Button(new Rect(310, yPos, 150, 25), (string)typeof(PlayerInfoImpact).GetField("a^sXfY").GetValue(x)))
                        {
                            Transform bikeTransform = ((GameObject)typeof(PlayerInfoImpact).GetField("W\u0082oQHKm").GetValue(x)).transform;
                            GameObject.Find("Player_Human").transform.position = bikeTransform.position;
                            GameObject.Find("Player_Human").transform.rotation = bikeTransform.rotation;
                        }
                        yPos += 25;
                    }
                }
                yPos = 10;
                if (GUI.Button(new Rect(460, yPos, 220, 25), " \\/ STATS \\/"))
                    __STATS__ = !__STATS__;
                yPos += 40;
                if (__STATS__)
                {
                    if (GUI.Button(new Rect(460, yPos, 110, 25), "Reset"))
                        FindObjectOfType<StatsModification>().ResetStats();
                    if (GUI.Button(new Rect(570, yPos, 55, 25), "Save"))
                        FindObjectOfType<StatsModification>().SaveStats();
                    if (GUI.Button(new Rect(625, yPos, 55, 25), "Load"))
                        FindObjectOfType<StatsModification>().LoadStats();
                    yPos += 30;
                    GUI.Box(new Rect(460, yPos, 220, FindObjectOfType<StatsModification>().stats.Count * 25), "");
                    foreach (Stat stat in FindObjectOfType<StatsModification>().stats)
                    {
                        GUI.Label(new Rect(460, yPos, 180, 25), "  " + stat.Name + ":");
                        string temp = (GUI.TextArea(new Rect(640, yPos, 40, 25), stat.currentVal) + "\n").Split('\n')[0];
                        try
                        {
                            float.Parse(temp);
                            stat.currentVal = temp;
                        }
                        catch { }
                        yPos += 25;
                    }
                }
                yPos = 10;
                if (GUI.Button(new Rect(680, yPos, 150, 25), " \\/ Quality of Life \\/"))
                    __QoL__ = !__QoL__;
                yPos += 25;
                if (__QoL__)
                {
                    if (GUI.Button(new Rect(680, yPos, 150, 25), "Toggle Map Audio"))
                        Utilities.instance.ToggleMapAudio();
                }
            }
        }
        void GetAllPlayers()
        {
            List<PlayerInfoImpact> playersList = new List<PlayerInfoImpact>();
            foreach (PlayerInfoImpact playerBehaviour in FindObjectsOfType<PlayerInfoImpact>())
                playersList.Add(playerBehaviour);
            players = playersList.ToArray();
        }
        public void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.I))
            {
                GetAllPlayers();
                isActive = !isActive;
                Cursor.visible = isActive;
            }
            if (hasBeenActive)
            {
                foreach (Checkpoint x in FindObjectsOfType<Checkpoint>())
                    x.doesWork = false;
            }
            if (Input.GetKey(KeyCode.P) && Input.GetKey(KeyCode.Alpha0) && Input.GetKeyDown(KeyCode.V))
                foreach (Checkpoint x in FindObjectsOfType<Checkpoint>())
                    x.doesWork = true;
        }
    }
}
