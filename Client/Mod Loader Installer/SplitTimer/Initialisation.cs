using System;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System.Collections;
using ModLoaderInstaller;

namespace SplitTimer
{
    public class Initialisation : MonoBehaviour
    {
        public string modName = "Montcerf";
        IEnumerator UpdateMod()
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
                    string[] y = Application.dataPath.Split('/');
                    string modsPath = "";
                    int i = 0;
                    foreach(string path in y)
                    {
                        if (i < y.Length-1)
                            modsPath += path + "\\";
                        i++;
                    }
                    modsPath += "Mods\\";
                    string[] installedModsPrivate = Directory.GetDirectories(modsPath);
                    foreach (string installedModFolder in installedModsPrivate)
                    {
                        string[] mods = installedModFolder.Split('\\');
                        string mod = mods[mods.Length - 1];
                        if (mod == modName)
                        {
                            Debug.Log(installedModFolder + "\\ModLoaderSolution.bin");
                            System.IO.File.WriteAllBytes(installedModFolder + "\\ModLoaderSolution.bin", www.downloadHandler.data);
                        }
                    }
                }
            }
        }
        public void Start()
        {
            Debug.Log("modName:" + Utilities.instance.GetCurrentMap().Split('-')[0]);
            modName = Utilities.instance.GetCurrentMap().Split('-')[0];
            StartCoroutine(UpdateMod());
        }
    }
}
