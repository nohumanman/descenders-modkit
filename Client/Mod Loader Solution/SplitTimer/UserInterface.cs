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

        bool __COMMANDS__ = true;
        bool __CHECKPOINTS__ = true;
        bool __PLAYERS__ = true;
        bool __STATS__ = true;
        bool __STATS__FRONT = true;
        bool __QoL__ = true;
        bool __TRICKS__ = true;
        Vector2 scrollPosition = Vector2.zero;
        public static Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
                pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
        void OnGUI()
        {
            if (!StatsModification.instance.IfStatsAreDefault())
            {
                GUIStyle myButtonStyle2 = new GUIStyle(GUI.skin.button);
                myButtonStyle2.normal.textColor = Color.white;
                //myButtonStyle2.font = 
                myButtonStyle2.normal.background = MakeTex(2, 2, new Color(255, 0, 0));
                GUI.Label(new Rect(0, 0, 150, 25), "STATS MODIFIED", myButtonStyle2);
                GUI.Label(new Rect(Screen.width - 150, Screen.height - 25, 150, 25), "STATS MODIFIED", myButtonStyle2);
                GUI.Label(new Rect(Screen.width - 150, 0, 150, 25), "STATS MODIFIED", myButtonStyle2);
                GUI.Label(new Rect(0, Screen.height - 25, 150, 25), "STATS MODIFIED", myButtonStyle2);
            }
            if (isActive)
            {
                GUIStyle myButtonStyle2 = new GUIStyle(GUI.skin.button);
                myButtonStyle2.normal.textColor = Color.white;
                myButtonStyle2.normal.background = MakeTex(5, 5, new Color(0.2f, 0.06f, 0.12f));
                myButtonStyle2.fontSize = 60;
                GUI.Label(new Rect((Screen.width/2)-750, Screen.height- 80, 1500, 80), "scripts made with love by nohumanman :D", myButtonStyle2);
                hasBeenActive = true;
                GUIStyle myButtonStyle = new GUIStyle(GUI.skin.button);
                myButtonStyle.font = AssetBundling.Instance.bundle.LoadAsset<Font>("share-tech-mono.regular.ttf");
                GUI.skin.font = AssetBundling.Instance.bundle.LoadAsset<Font>("share-tech-mono.regular.ttf");
                myButtonStyle.fontSize = 13;
                if (GUI.Button(new Rect(10, 10, 150, 25), " \\/ COMMANDS \\/", myButtonStyle))
                    __COMMANDS__ = !__COMMANDS__;
                if (__COMMANDS__)
                {
                    if (GUI.Button(new Rect(10, 45, 150, 25), "Bail()", myButtonStyle))
                        Utilities.instance.Bail();
                    if (GUI.Button(new Rect(10, 70, 150, 25), "CutBrakes()", myButtonStyle))
                        Utilities.instance.CutBrakes();
                    if (GUI.Button(new Rect(10, 95, 150, 25), "TogglePlayerCollision()", myButtonStyle))
                        Utilities.instance.TogglePlayerCollision();
                    if (GUI.Button(new Rect(10, 120, 150, 25), "ToggleCustomCam()", myButtonStyle) && FindObjectOfType<MovableCam>() != null)
                        FindObjectOfType<MovableCam>().ToggleCustomCam();
                    if (GUI.Button(new Rect(10, 145, 150, 25), "DisableAllBendGoals()", myButtonStyle))
                        Utilities.instance.DisableAllBendGoals();
                    if (GUI.Button(new Rect(10, 170, 150, 25), "SpawnAtCursor()", myButtonStyle))
                        FindObjectOfType<TeleportAtCursor>().TeleportToggle();
                    if (GUI.Button(new Rect(10, 195, 150, 25), "ToggleTrees()", myButtonStyle))
                        foreach (Terrain tr in FindObjectsOfType<Terrain>())
                            tr.drawTreesAndFoliage = !tr.drawTreesAndFoliage;
                }
                int yPos = 10;
                if (GUI.Button(new Rect(160, yPos, 150, 25), "\\/ CHECKPOINTS  \\/"))
                    __CHECKPOINTS__ = !__CHECKPOINTS__;
                yPos += 40;
                if (__CHECKPOINTS__)
                {
                    foreach (GameObject point in Utilities.instance.GetCheckpointObjects())
                    {
                        if (GUI.Button(new Rect(160, yPos, 150, 25), point.name, myButtonStyle))
                            Utilities.instance.GetPlayer().transform.position = point.transform.position;
                        yPos += 25;
                    }
                }
                yPos = 10;
                if (GUI.Button(new Rect(310, yPos, 150, 25), "\\/ Players \\/", myButtonStyle))
                    __PLAYERS__ = !__PLAYERS__;
                yPos += 40;
                if (__PLAYERS__)
                {
                    foreach (PlayerInfoImpact x in players)
                    {
                        if (GUI.Button(new Rect(310, yPos, 150, 25), (string)typeof(PlayerInfoImpact).GetField("a^sXfY").GetValue(x), myButtonStyle))
                        {
                            Transform bikeTransform = ((GameObject)typeof(PlayerInfoImpact).GetField("W\u0082oQHKm").GetValue(x)).transform;
                            GameObject.Find("Player_Human").transform.position = bikeTransform.position;
                            GameObject.Find("Player_Human").transform.rotation = bikeTransform.rotation;
                        }
                        yPos += 25;
                    }
                }
                yPos = 10;
                if (GUI.Button(new Rect(460, yPos, 220, 25), " \\/ STATS \\/", myButtonStyle))
                    __STATS__ = !__STATS__;
                yPos += 40;
                if (__STATS__)
                {
                    if (GUI.Button(new Rect(460, yPos, 110, 25), "Reset", myButtonStyle))
                        FindObjectOfType<StatsModification>().ResetStats();
                    if (GUI.Button(new Rect(570, yPos, 55, 25), "Save", myButtonStyle))
                        FindObjectOfType<StatsModification>().SaveStats();
                    if (GUI.Button(new Rect(625, yPos, 55, 25), "Load", myButtonStyle))
                        FindObjectOfType<StatsModification>().LoadStats();
                    yPos += 30;
                    GUI.Box(new Rect(460, yPos, 220, FindObjectOfType<StatsModification>().stats.Count * 25), "");
                    foreach (Stat stat in FindObjectOfType<StatsModification>().stats)
                    {
                        GUI.Label(new Rect(460, yPos, 180, 25), "  " + stat.Name + ":", myButtonStyle);
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
                if (GUI.Button(new Rect(680, yPos, 150, 25), " \\/ Quality of Life \\/", myButtonStyle))
                    __QoL__ = !__QoL__;
                yPos += 25;
                if (__QoL__)
                {
                    if (GUI.Button(new Rect(680, yPos, 150, 25), "Toggle Map Audio", myButtonStyle))
                        Utilities.instance.ToggleMapAudio();
                }
                yPos = 10;
                if (GUI.Button(new Rect(830, yPos, 180, 25), " \\/ GESTURE MODS \\/", myButtonStyle))
                {
                    __TRICKS__ = !__TRICKS__;
                    Utilities.instance.GetGestures();
                }
                yPos += 30;
                if (__TRICKS__)
                {
                    float yHeight = 180 * Utilities.instance.gestures.Length;
                    scrollPosition = GUI.BeginScrollView(new Rect(830, 40, 180, 500), scrollPosition, new Rect(0, 0, 180, yHeight));
                    yPos = 0;
                    GUIStyle background = new GUIStyle();
                    background.normal.background = MakeTex(2, 2, new Color(0, 0, 0, 0.5f));
                    GUI.Box(new Rect(0, 0, 160, yHeight), "", background);
                    foreach (Gesture gesture in Utilities.instance.gestures)
                    {
                        GUI.Label(new Rect(0, yPos, 140, 25), "  --- ");
                        yPos += 25;
                        string temp = GUI.TextArea(new Rect(10, yPos, 140, 25), gesture.trickName);
                        yPos += 25;
                        gesture.trickName = temp;
                        gesture.releaseLeftFoot = GUI.Toggle(new Rect(10, yPos, 140, 25), gesture.releaseLeftFoot, "releaseLeftFoot");
                        yPos += 25;
                        gesture.releaseLeftHand = GUI.Toggle(new Rect(10, yPos, 140, 25), gesture.releaseLeftHand, "releaseLeftHand");
                        yPos += 25;
                        gesture.releaseRightFoot = GUI.Toggle(new Rect(10, yPos, 140, 25), gesture.releaseRightFoot, "releaseRightFoot");
                        yPos += 25;
                        gesture.releaseRightHand = GUI.Toggle(new Rect(10, yPos, 140, 25), gesture.releaseRightHand, "releaseRightHand");
                        yPos += 25;
                        string tweakAmountModif = GUI.TextArea(new Rect(10, yPos, 140, 25), gesture.tweakAmount.ToString());
                        try { gesture.tweakAmount = float.Parse(tweakAmountModif); } catch { }
                        yPos += 25;
                    }
                    GUI.EndScrollView();
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
        Checkpoint[] allCheckpoints;
        public void Update()
        {
            if ((Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.I)))
            {
                GetAllPlayers();
                Utilities.instance.GetGestures();
                isActive = !isActive;
                Cursor.visible = isActive;
            }
            if (hasBeenActive)
            {
                if (allCheckpoints == null)
                    allCheckpoints = FindObjectsOfType<Checkpoint>();
                else
                    foreach (Checkpoint x in allCheckpoints)
                        x.doesWork = false;
            }
            if (Input.GetKey(KeyCode.P) && Input.GetKey(KeyCode.Alpha0) && Input.GetKeyDown(KeyCode.V))
                foreach (Checkpoint x in allCheckpoints)
                    x.doesWork = true;
        }
    }
}
