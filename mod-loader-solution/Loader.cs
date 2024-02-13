using UnityEngine;
using ModTool.Interface;
using System.IO;
using System.Windows.Forms;
using UnityEngine.Networking;
using System.Collections;
using System.Reflection;
using System.Threading;
using System;

namespace ModLoaderSolution
{
    public class Loader : ModBehaviour
    {
        public static GameObject loaderObj;
        public void Start()
        {
            Load();
        }
        private static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode)
        {
            NetClient _netCl = FindObjectOfType<NetClient>();
            if (_netCl == null)
            {
                loaderObj = new GameObject();
                DontDestroyOnLoad(loaderObj.transform.root);
                loaderObj.name = "DescendersSplitTimerModLoaded";
                Utilities.Log("GameObject Instantiated");
                loaderObj.AddComponent<Utilities>();
                loaderObj.AddComponent<AssetBundling>();
                Utilities.Log("ModLoaderSolution.Utilities added");
                loaderObj.AddComponent<Init>();
                Utilities.Log("SplitTimer.Initialisation added");
            }
        }

        public static void Load()
        {
            // ModLoaderSolution.bin already downloaded into LocalLow/RageSquid/Descenders
            // so load version.dll into same folder as .exe file
            if (UnityEngine.SceneManagement.SceneManager.sceneCount > 0)
            {
                NetClient _netCl = FindObjectOfType<NetClient>();
                if (_netCl == null)
                {
                    loaderObj = new GameObject();
                    DontDestroyOnLoad(loaderObj.transform.root);
                    loaderObj.name = "DescendersSplitTimerModLoaded";
                    Utilities.Log(" ModLoaderSolution has loaded");
                    loaderObj.AddComponent<Utilities>();
                    loaderObj.AddComponent<AssetBundling>();
                    Utilities.Log("ModLoaderSolution.Utilities added");
                    loaderObj.AddComponent<Init>();
                    Utilities.Log("SplitTimer.Initialisation added");
                }
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            }
        }
        public static void Unload()
        {
            //MonoBehaviour.Destroy(gameObject);
        }
        public static void _unload()
        {
            //MonoBehaviour.Destroy(gameObject);
        }
    }
}

