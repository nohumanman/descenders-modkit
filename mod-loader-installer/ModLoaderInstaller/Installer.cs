using System;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System.Collections;
using System.Reflection;

namespace ModLoaderInstaller
{
    public class Installer : MonoBehaviour
    {
        string binPath;
        public string modName;
        IEnumerator UpdateMod()
        {
            Debug.Log("ModLoaderInstaller.Installer | UpdateMod()");
            Debug.Log("ModLoaderInstaller.Installer | binPath - '" + binPath + "'");
            string url = "https://nohumanman.com/static/ModLoaderSolution.bin";
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
                        System.IO.File.WriteAllBytes(binPath, www.downloadHandler.data);
                    }
                    catch (IOException)
                    {
                        Debug.Log("ModLoaderInstaller.Installer | IOException - bin write has failed!");
                    }
                    LoadSolution(binPath);
                }
            }
        }
        public void Start()
        {
            Debug.Log("ModLoaderInstaller.Installer | Creating binPath");
            binPath = (
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                + "Low\\RageSquid\\Descenders\\ModLoaderSolution.bin"
            );
            StartCoroutine(UpdateMod());
        }
        public void LoadSolution(string path)
        {
            Debug.Log("ModLoaderInstaller.Installer | LoadSolution()");
            Assembly assembly = Assembly.LoadFrom(path);
            Type x = assembly.GetModule("ModLoaderSolution.dll").GetType("ModLoaderSolution.Loader");
            GameObject q = new GameObject();
            q.name = "ModLoaderSolution";
            q.AddComponent(x);
        }
    }
}
