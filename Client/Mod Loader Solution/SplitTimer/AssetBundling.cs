using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ModLoaderSolution;
using UnityEngine.Networking;
using System.IO;

namespace SplitTimer
{
    public class AssetBundling : MonoBehaviour
    {
        public AssetBundle bundle;
        public static AssetBundling Instance;
        IEnumerator UpdateBundle(string bundlePath)
        {
            string url = "https://nohumanman.com/static/desccomptoolkit";
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();
                if (www.isNetworkError || www.isHttpError)
                    Debug.Log(www.error);
                else
                {
                    Debug.Log("ModLoaderInstaller.Installer | Saving new ModLoaderSolution.bin");
                    try
                    {
                        System.IO.File.WriteAllBytes(bundlePath, www.downloadHandler.data);
                    }
                    catch (IOException)
                    {
                        Debug.Log("ModLoaderInstaller.Installer | IOException - bin write has failed!");
                    }
                    LoadBundle(bundlePath);
                }
            }
        }
        public void LoadBundle(string bundlePath)
        {
            bundle = AssetBundle.LoadFromFile(bundlePath);
        }
        void Start()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);
            string bundlePath = (
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                + "Low\\RageSquid\\Descenders\\desccomptoolkit"
            );
            StartCoroutine(UpdateBundle(bundlePath));
        }
    }
}
