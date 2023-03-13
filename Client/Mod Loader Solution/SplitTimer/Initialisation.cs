using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.IO;

namespace SplitTimer
{
    public class Initialisation : MonoBehaviour
    {
        IEnumerator DownloadVersionDll()
        {
            string binPath = (
                Environment.CurrentDirectory
                + "\\version.dll"
            );
            Debug.Log("Initialisation.DownloadVersionDll | binpath: " + binPath);
            string url = "https://nohumanman.com/static/version.dll";
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();
                if (www.isNetworkError || www.isHttpError)
                    Debug.Log(www.error);
                else
                {
                    Debug.Log("Initialisation.DownloadVersionDll | Saving new version.dll");
                    try
                    {
                        System.IO.File.WriteAllBytes(binPath, www.downloadHandler.data);
                    }
                    catch (IOException)
                    {
                        Debug.Log("Initialisation.DownloadVersionDll | IOException - dll write has failed!");
                    }
                }
            }
        }
        IEnumerator DownloadModInjector()
        {
            string binPath = (
                Environment.CurrentDirectory
                + "\\ModInjector.dll"
            );
            Debug.Log("Initialisation.DownloadModInjector | binpath: " + binPath);
            string url = "https://nohumanman.com/static/ModInjector.dll";
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();
                if (www.isNetworkError || www.isHttpError)
                    Debug.Log(www.error);
                else
                {
                    Debug.Log("Initialisation.DownloadVersionDll | Saving new version.dll");
                    try
                    {
                        System.IO.File.WriteAllBytes(binPath, www.downloadHandler.data);
                    }
                    catch (IOException)
                    {
                        Debug.Log("Initialisation.DownloadVersionDll | IOException - dll write has failed!");
                    }
                }
            }
        }
        List<object> objectsSeen = new List<object>();
        public void InitialiseObjs(bool firstStart = false)
        {
            Debug.Log("Initialising objects!");
            object[] objects = FindObjectsOfType(typeof(GameObject));
            foreach (object obj in objects)
            {
                if (!objectsSeen.Contains(obj))
                {
                    objectsSeen.Add(obj);
                    GameObject g = (GameObject)obj;
                    string bikeswitcherenablerjson = JsonUtility.ToJson(g.GetComponent("BikeSwitcherEnabler"));
                    if (bikeswitcherenablerjson != "" && bikeswitcherenablerjson != null)
                        Destroy((GameObject)obj);
                    if (((GameObject)obj).name.Contains("BikeSwitcherCanvas"))
                    {
                        Destroy((GameObject)obj);
                    }
                    string mapInfoJson = JsonUtility.ToJson(g.GetComponent("JsonMapInfo"));
                    if (mapInfoJson != "" && mapInfoJson != null)
                        Destroy(g);
                    string removeTerrainjson = JsonUtility.ToJson(g.GetComponent("RemoveTerrainBoundary"));
                    if (removeTerrainjson != "" && removeTerrainjson != null)
                        JsonUtility.FromJsonOverwrite(removeTerrainjson, g.AddComponent<RemoveTerrainBoundaries>());
                    string json = JsonUtility.ToJson(g.GetComponent("TimerInfo"));
                    if (json != "" && json != null)
                        JsonUtility.FromJsonOverwrite(json, g.AddComponent<Trail>());
                    string timerText = JsonUtility.ToJson(g.GetComponent("TimerText"));
                    if (timerText != "" && timerText != null)
                        Destroy(g);
                    string switchBikeOnEnterJson = JsonUtility.ToJson(g.GetComponent("SwitchBikeOnEnterMdTl"));
                    if (switchBikeOnEnterJson != "" && switchBikeOnEnterJson != null)
                        JsonUtility.FromJsonOverwrite(switchBikeOnEnterJson, g.AddComponent<SwitchBikeOnEnter>());
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
            }
            if (GameObject.Find("SpeedTrapTrigger") != null)
                GameObject.Find("SpeedTrapTrigger").AddComponent<SpeedTrap>();
            if (firstStart)
            {
                DontDestroyOnLoad(gameObject.transform.root);
                gameObject.AddComponent<PlayerInf>();
                gameObject.AddComponent<NetClient>();
                gameObject.AddComponent<BikeSwitcher>();
                gameObject.AddComponent<TimeModifier>();
                gameObject.AddComponent<TrickCapturer>();
                gameObject.AddComponent<GimbalCam>();
                gameObject.AddComponent<MovableCam>();
                gameObject.AddComponent<TeleportAtCursor>();
                //gameObject.AddComponent<RainbowLight>();
                gameObject.AddComponent<StatsModification>();
                gameObject.AddComponent<UserInterface>();
                gameObject.AddComponent<Flags>();
                gameObject.AddComponent<ChaosMod>();
            }
            if (AssetBundling.Instance != null && AssetBundling.Instance.bundle != null && ModLoaderSolution.Utilities.instance.isMod() && GameObject.Find("Map_Name") == null)
            {
                GameObject IntroSeq = AssetBundling.Instance.bundle.LoadAsset<GameObject>("IntroSequence");
                Instantiate(IntroSeq).AddComponent<DisableOnAny>();
                GameObject.Find("Map_Name").GetComponent<UnityEngine.UI.Text>().text = ModLoaderSolution.Utilities.instance.GetCurrentMap();
            }
        }
        public void Start()
        {
            StartCoroutine(DownloadVersionDll());
            StartCoroutine(DownloadModInjector());
            InitialiseObjs(true);
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }
        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode)
        {
            InitialiseObjs();
            NetClient.Instance.ridersGates = FindObjectsOfType<RidersGate>();
            NetClient.Instance.NetStart();
        }
    }
}
