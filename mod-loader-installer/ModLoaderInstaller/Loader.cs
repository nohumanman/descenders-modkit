using UnityEngine;
using ModTool.Interface;

namespace ModLoaderInstaller
{
    public class Loader : ModBehaviour
    {
        public static GameObject gameObject;
        public static void Load()
        {
            Debug.Log("ModLoaderInstaller.Loader | Load() function called.");
            gameObject = new GameObject();
            gameObject.name = "ModLoaderInstaller";
            Debug.Log("ModLoaderInstaller.Loader | GameObject Instantiated");
            gameObject.AddComponent<Installer>();
            Debug.Log("ModLoaderInstaller.Loader | Installer added");
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

