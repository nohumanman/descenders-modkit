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

namespace MyNamespace
{
    public class MyClass
    {
        public static int MyMethod(string pwzArgument)
        {
            MessageBox.Show("hello world");
            Thread thread1 = new Thread(MethodInOtherThread);
            thread1.Start();
            return 0;
        }
        public static void MethodInOtherThread()
        {
            for (int i = 0; i < 3; i++)
            {
                File.WriteAllText("C:\\Users\\point\\Desktop\\Csharplog.txt", "L");
                Console.WriteLine("Working thread...");
                Thread.Sleep(100);
                if (Input.GetKeyDown(KeyCode.G))
                    File.WriteAllText("C:\\Users\\point\\Desktop\\pressed g.txt", "L");
            }

        }
    }
}

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
            File.WriteAllText("C:\\Users\\point\\Desktop\\Csharplog.txt", "L");
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

