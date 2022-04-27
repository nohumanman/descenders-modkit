using UnityEngine;
using ModTool.Interface;

namespace ModLoaderSolution
{
    public class Loader : ModBehaviour
    {
        public static GameObject gameObject;

        public static void Load()
        {
            Debug.Log("ModLoaderSolution | Load() function called.");
            Utilities _u = GameObject.FindObjectOfType<Utilities>();
            if (_u != null)
            {
                Debug.Log("ModLoaderSolution | Mod was already loaded, unloading...");
                MonoBehaviour.Destroy(_u.gameObject);
            }
            Debug.Log("ModLoaderSolution | Finished Mod Check");
            gameObject = new GameObject();
            Debug.Log("ModLoaderSolution | GameObject Instantiated");
            gameObject.AddComponent<Utilities>();
            Debug.Log("ModLoaderSolution | Utilities added");
            gameObject.AddComponent<SplitTimer.Initialisation>();
            Debug.Log("ModLoaderSolution | FreeReign added");
            // gameObject.AddComponent<NetClient>();
        }
        public static void Unload()
        {
            MonoBehaviour.Destroy(gameObject);
        }

        public static void _unload()
        {
            MonoBehaviour.Destroy(gameObject);
        }
    }
}

