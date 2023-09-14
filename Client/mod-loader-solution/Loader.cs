using UnityEngine;
using ModTool.Interface;
using SplitTimer;
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
        public static GameObject gameObject;
        public void Start()
        {
            Load();
        }
        private static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode)
        {
            NetClient _netCl = FindObjectOfType<NetClient>();
            if (_netCl == null)
            {
                gameObject = new GameObject();
                DontDestroyOnLoad(gameObject);
                gameObject.name = "DescendersSplitTimerModLoaded";
                Debug.Log("ModLoaderSolution | GameObject Instantiated");
                gameObject.AddComponent<Utilities>();
                gameObject.AddComponent<AssetBundling>();
                Debug.Log("ModLoaderSolution | ModLoaderSolution.Utilities added");
                gameObject.AddComponent<Initialisation>();
                Debug.Log("ModLoaderSolution | SplitTimer.Initialisation added");
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
                    gameObject = new GameObject();
                    DontDestroyOnLoad(gameObject);
                    gameObject.name = "DescendersSplitTimerModLoaded";
                    Debug.Log("ModLoaderSolution | GameObject Instantiated");
                    gameObject.AddComponent<Utilities>();
                    gameObject.AddComponent<AssetBundling>();
                    Debug.Log("ModLoaderSolution | ModLoaderSolution.Utilities added");
                    gameObject.AddComponent<Initialisation>();
                    Debug.Log("ModLoaderSolution | SplitTimer.Initialisation added");
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

