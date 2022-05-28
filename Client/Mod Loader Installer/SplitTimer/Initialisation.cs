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
        public string modName = "Snowbird Island";
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
                    Debug.Log(Directory.GetCurrentDirectory());
                    Debug.Log(SceneManager.GetActiveScene().path);
                    Debug.Log(SceneManager.GetActiveScene().name);
                    string savePath = string.Format("{0}/{1}.bin", Directory.GetCurrentDirectory(), "\\Mods\\" + modName + "\\ModLoaderSolution");
                    System.IO.File.WriteAllBytes(savePath, www.downloadHandler.data);
                }
            }
        }
        public void Start()
        {
            StartCoroutine(Test());
        }
    }
}
