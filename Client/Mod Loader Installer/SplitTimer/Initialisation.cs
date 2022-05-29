using System;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.UI;
using ModTool.Interface;
using ModTool.Shared;
using ModTool;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using TMPro;
using InControl;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SplitTimer
{
    public class Initialisation : MonoBehaviour
    {
        public string modName = "4x Dobrany";
        IEnumerator Test()
        {
            string url = "https://nohumanman.com/static/ModLoaderSolution.bin";
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();
                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    Debug.Log("Here!!!!!!!!!!!");
                    string modioPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low\\RageSquid\\Descenders\\modio-202\\_installedMods";
                    string[] installedMods = Directory.GetDirectories(modioPath);
                    foreach (string installedModFolder in installedMods)
                    {
                        string modDrirectory = Directory.GetDirectories(installedModFolder)[0];
                        Debug.Log(installedModFolder);
                        Debug.Log(modDrirectory);
                        string[] x = modDrirectory.Split('\\');
                        string fileModName = x[x.Length-1];
                        if (fileModName == modName)
                        {
                            Debug.Log("Mod Found!");
                            System.IO.File.WriteAllBytes(modDrirectory + "\\ModLoaderSolution.bin", www.downloadHandler.data);
                        }
                    }
                }
            }
        }
        public void Start()
        {
            StartCoroutine(Test());
        }
    }
}
