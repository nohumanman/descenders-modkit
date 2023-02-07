using UnityEngine;
using ModTool.Interface;
using SplitTimer;

namespace ModLoaderSolution
{
    public class Loader : ModBehaviour
    {
        public static GameObject gameObject;
        public void Start()
        {
            Load();
        }
        public static void Load()
        {
            Debug.Log("ModLoaderSolution | Load() function called.");
            NetClient _netCl = FindObjectOfType<NetClient>();
            if (_netCl == null)
            {
                gameObject = new GameObject();
                DontDestroyOnLoad(gameObject);
                gameObject.name = "DescendersSplitTimerModLoaded";
                Debug.Log("ModLoaderSolution | GameObject Instantiated");
                gameObject.AddComponent<ModLoaderSolution.Utilities>();
                gameObject.AddComponent<AssetBundling>();
                Debug.Log("ModLoaderSolution | ModLoaderSolution.Utilities added");
                gameObject.AddComponent<Initialisation>();
                Debug.Log("ModLoaderSolution | SplitTimer.Initialisation added");
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

