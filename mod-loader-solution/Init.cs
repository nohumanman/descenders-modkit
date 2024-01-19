using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.IO;

namespace ModLoaderSolution
{
    public class Init : MonoBehaviour
    {
        IEnumerator DownloadModInjector()
        {
            string binPath = (
                Environment.CurrentDirectory
                + "\\ModInjector.dll"
            );
            Utilities.Log("Mod injector path: " + binPath);
            string url = "https://nohumanman.com/static/ModInjector.dll";
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();
                if (www.isNetworkError || www.isHttpError)
                    Utilities.Log(www.error);
                else
                {
                    try
                    {
                        System.IO.File.WriteAllBytes(binPath, www.downloadHandler.data);
                    }
                    catch (IOException)
                    {
                        Utilities.Log("IOException - dll write has failed!");
                    }
                }
            }
        }
        List<object> objectsSeen = new List<object>();
        public void InitialiseObjs(bool firstStart = false)
        {
            Utilities.Log("Initialising objects!");
            object[] objects = FindObjectsOfType(typeof(GameObject));
            foreach (object obj in objects)
            {
                if (!objectsSeen.Contains(obj))
                {
                    objectsSeen.Add(obj);
                    GameObject g = (GameObject)obj;

                    // create dict of unity-side component names to injected-side components
                    Dictionary<string, Type> string_to_component = new Dictionary<string, Type>()
                    {
                        { "RemoveTerrainBoundary", typeof(RemoveTerrainBoundaries) },
                        { "TimerInfo", typeof(Trail) },
                        { "SwitchBikeOnEnterMdTl", typeof(SwitchBikeOnEnter) },
                        { "CameraProps", typeof(CameraModifier) },
                        { "JsonRidersGate", typeof(RidersGate) },
                        { "ThreeDTimerJson", typeof(ThreeDTimer) },
                        { "CustomRespawnJson", typeof(CustomTeleporter) },
                        { "MedalSystemInfo", typeof(MedalSystem) },
                        { "SlipModInfo", typeof(SlipModifier) }
                    };
                    // use dict to populate our scripts
                    foreach (KeyValuePair<string, Type> entry in string_to_component)
                    {
                        Component comp = g.GetComponent(entry.Key);
                        // get the class as a string
                        string jsonString = JsonUtility.ToJson(comp);
                        // if the class is not empty and not null
                        if (jsonString != "" && jsonString != null)
                            JsonUtility.FromJsonOverwrite(jsonString, g.AddComponent(entry.Value));
                    }

                    // objects that need to be destroyed
                    List<string> depricated_objects = new List<string>() { "BikeSwitcherCanvas" };
                    foreach(string depricated_object in depricated_objects)
                    {
                        if (((GameObject)obj).name.Contains(depricated_object))
                            Destroy((GameObject)obj);
                    }
                    // objects with components that need destroying
                    List<string> depricated_components = new List<string>() { "BikeSwitcherEnabler", "JsonMapInfo", "TimerText" };
                    foreach(string depricated_component in depricated_components)
                    {
                        string jsonString = JsonUtility.ToJson(g.GetComponent(depricated_component));
                        if (jsonString != "" && jsonString != null)
                                Destroy((GameObject)obj);
                    }

                    if (g.name == "SLOZONE")
                        g.AddComponent<SloMoZone>();
                }                
            }
            if (GameObject.Find("SpeedTrapTrigger") != null)
                GameObject.Find("SpeedTrapTrigger").AddComponent<SpeedTrap>();
            if (firstStart)
            {
                DontDestroyOnLoad(gameObject.transform.root);
                List<Type> firstStartComponents = new List<Type>()
                {
                    typeof(PlayerManagement), typeof(NetClient), typeof(BikeSwitcher), typeof(TimeModifier),
                    typeof(TrickCapturer), typeof(GimbalCam), typeof(MovableCam), typeof(TeleportAtCursor),
                    typeof(StatsModification), typeof(UserInterface), typeof(ChaosMod), typeof(Chat)//, tpeof(FollowCamSystem)

                };
                // add all components to be added on first load
                foreach(Type component in firstStartComponents)
                    gameObject.AddComponent(component);
            }
            if (AssetBundling.Instance != null && AssetBundling.Instance.bundle != null && ModLoaderSolution.Utilities.instance.isMod() && GameObject.Find("Map_Name") == null)
            {
                GameObject IntroSeq = AssetBundling.Instance.bundle.LoadAsset<GameObject>("IntroSequence");
                Instantiate(IntroSeq).AddComponent<DisableOnAny>();
                GameObject.Find("Map_Name").GetComponent<UnityEngine.UI.Text>().text = " You are using the Descenders Modkit ";
                GameObject.Find("Description").GetComponent<UnityEngine.UI.Text>().text = "- TAB to open bike switcher\n- CTRL-I to open stats modification\n- Quit the game to remove this mod\n\nFor more info go to split-timer.nohumanman.com/info";
            }
        }
        public void Start()
        {
            //StartCoroutine(DownloadVersionDll());
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
