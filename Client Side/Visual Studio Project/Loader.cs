using UnityEngine;
using ModTool.Interface;

namespace DescendersSplitTimer
{
    public class Loader : ModBehaviour
    {
        public static GameObject gameObject;

        public static void Load()
        {
            Debug.Log("MODLOADER START");
            Utilities _u = GameObject.FindObjectOfType<Utilities>();
            if (_u != null)
            {
                Debug.Log("Mod was already loaded, unloading...");
                MonoBehaviour.Destroy(_u.gameObject);
            }
            Debug.Log("Finished Mod Check");
            gameObject = new GameObject();
            gameObject.name = "DescendersSplitTimerModLoaded";
            Debug.Log("GameObject Instantiated");
            gameObject.AddComponent<Utilities>();
            Debug.Log("Utilities added");
            gameObject.AddComponent<BikeSwitcher>();
            Debug.Log("BikeSwitcher added");
            gameObject.AddComponent<IceHandler>();
            Debug.Log("Ice Handler added");
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

