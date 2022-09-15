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
                    JsonUtility.FromJsonOverwrite(json, g.AddComponent<Trail>());
                string jsonMapInfo = JsonUtility.ToJson(g.GetComponent("JsonMapInfo"));
                if (jsonMapInfo != "" && jsonMapInfo != null)
                    JsonUtility.FromJsonOverwrite(jsonMapInfo, this.gameObject.AddComponent<MapInfo>());
                string timerText = JsonUtility.ToJson(g.GetComponent("TimerText"));
                if (timerText != "" && timerText != null)
                    g.AddComponent<SplitTimerText>();
                string cameraPropsText = JsonUtility.ToJson(g.GetComponent("CameraProps"));
                if (cameraPropsText != "" && cameraPropsText != null)
                    JsonUtility.FromJsonOverwrite(cameraPropsText, g.AddComponent<CameraModifier>());
                string jsonRidersGate = JsonUtility.ToJson(g.GetComponent("JsonRidersGate"));
                if (jsonRidersGate != "" && jsonRidersGate != null)
                    JsonUtility.FromJsonOverwrite(jsonRidersGate, g.AddComponent<RidersGate>());
                string jsonRespawn = JsonUtility.ToJson(g.GetComponent("CustomRespawnJson"));
                if (jsonRespawn != "" && jsonRespawn != null)
                    JsonUtility.FromJsonOverwrite(jsonRespawn, gameObject.AddComponent<CustomTeleporter>());
                string jsonMedalSystem = JsonUtility.ToJson(g.GetComponent("MedalSystemInfo"));
                if (jsonMedalSystem != "" && jsonMedalSystem != null)
                    JsonUtility.FromJsonOverwrite(jsonMedalSystem, gameObject.AddComponent<MedalSystem>());
                if (g.name == "SLOZONE")
                    g.AddComponent<SloMoZone>();
            }
            if (this.GetComponent<MapInfo>() == null)
                Debug.LogError("ERROR - No Map info found in scene!!");
            this.gameObject.AddComponent<PlayerInfo>();
            this.gameObject.AddComponent<NetClient>();
            this.gameObject.AddComponent<BikeSwitcher>();
            this.gameObject.AddComponent<TimeModifier>();
            this.gameObject.AddComponent<TrickCapturer>();
            this.gameObject.AddComponent<GimbalCam>();
            GameObject.Find("SpeedTrapTrigger").AddComponent<SpeedTrap>();
        }
    }
}
