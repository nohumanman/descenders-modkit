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
                string replaceBikeJson = JsonUtility.ToJson(g.GetComponent("ReplaceBike"));
                //if (replaceBikeJson != "" && replaceBikeJson != null)
                //    JsonUtility.FromJsonOverwrite(replaceBikeJson, g.AddComponent<ReplaceBikeAttempt>());

                string removeTerrainjson = JsonUtility.ToJson(g.GetComponent("RemoveTerrainBoundary"));
                if (removeTerrainjson != "" && removeTerrainjson != null)
                    JsonUtility.FromJsonOverwrite(removeTerrainjson, g.AddComponent<RemoveTerrainBoundaries>());
                string json = JsonUtility.ToJson(g.GetComponent("TimerInfo"));
                if (json != "" && json != null)
                    JsonUtility.FromJsonOverwrite(json, g.AddComponent<Trail>());
                string jsonMapInfo = JsonUtility.ToJson(g.GetComponent("JsonMapInfo"));
                if (jsonMapInfo != "" && jsonMapInfo != null)
                    JsonUtility.FromJsonOverwrite(jsonMapInfo, g.AddComponent<MapInfo>());
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
                    JsonUtility.FromJsonOverwrite(jsonRespawn, g.AddComponent<CustomTeleporter>());
                string jsonMedalSystem = JsonUtility.ToJson(g.GetComponent("MedalSystemInfo"));
                if (jsonMedalSystem != "" && jsonMedalSystem != null)
                    JsonUtility.FromJsonOverwrite(jsonMedalSystem, g.AddComponent<MedalSystem>());
                if (g.name == "SLOZONE")
                    g.AddComponent<SloMoZone>();
                string jsonSlipModInfo = JsonUtility.ToJson(g.GetComponent("SlipModInfo"));
                if (jsonSlipModInfo != "" && jsonSlipModInfo != null)
                    JsonUtility.FromJsonOverwrite(jsonSlipModInfo, g.AddComponent<SlipModifier>());
            }
            if (FindObjectOfType<MapInfo>() == null)
                Debug.LogError("ERROR - No Map info found in scene!!");
            gameObject.AddComponent<PlayerInf>();
            gameObject.AddComponent<NetClient>();
            gameObject.AddComponent<BikeSwitcher>();
            gameObject.AddComponent<TimeModifier>();
            gameObject.AddComponent<TrickCapturer>();
            gameObject.AddComponent<GimbalCam>();
            gameObject.AddComponent<MovableCam>();
            //gameObject.AddComponent<RainbowLight>();
            gameObject.AddComponent<StatsModification>();
            gameObject.AddComponent<UserInterface>();
            gameObject.AddComponent<Flags>();
            if (GameObject.Find("SpeedTrapTrigger") != null)
                GameObject.Find("SpeedTrapTrigger").AddComponent<SpeedTrap>();
        }
    }
}
