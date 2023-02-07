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
            Utilities _u = FindObjectOfType<Utilities>();
            if (_u != null)
            {
                Debug.Log("ModLoaderSolution | Mod was already loaded, unloading...");
                MonoBehaviour.Destroy(_u.gameObject);
            }
            Debug.Log("ModLoaderSolution | Finished Mod Check");
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

