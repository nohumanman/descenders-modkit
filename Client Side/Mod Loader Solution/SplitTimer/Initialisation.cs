using System;
using UnityEngine;


namespace SplitTimer
{
    public class Initialisation : MonoBehaviour
    {
        public void Start()
        {
            object[] objects = FindObjectsOfType(typeof(GameObject));
            foreach (object obj in objects)
            {
                GameObject g = (GameObject)obj;
                string json = JsonUtility.ToJson(g.GetComponent("TimerInfo"));
                if (json != "" && json != null)
                {
                    Trail trail = g.AddComponent<Trail>();
                    JsonUtility.FromJsonOverwrite(json, trail);
                    trail.AddScripts();
                }
                string jsonMapInfo = JsonUtility.ToJson(g.GetComponent("JsonMapInfo"));
                if (jsonMapInfo != "" && jsonMapInfo != null)
                {
                    MapInfo mapInfo = this.gameObject.AddComponent<MapInfo>();
                    JsonUtility.FromJsonOverwrite(jsonMapInfo, mapInfo);
                }
                string timerText = JsonUtility.ToJson(g.GetComponent("TimerText"));
                if (timerText != "" && timerText != null)
                {
                    g.AddComponent<TimerText>();
                }
                string jsonRidersGate = JsonUtility.ToJson(g.GetComponent("JsonRidersGate"));
                if (jsonRidersGate != "" && jsonRidersGate != null)
                {
                    RidersGate x = g.AddComponent<RidersGate>();
                    JsonUtility.FromJsonOverwrite(jsonRidersGate, x);
                }
            }
            if (this.GetComponent<MapInfo>() == null)
                Debug.LogError("ERROR - No Map info found in scene!!");
            this.gameObject.AddComponent<PlayerInfo>();
            this.gameObject.AddComponent<NetClient>();
            this.gameObject.AddComponent<BikeSwitcher>();
        }
    }
}
