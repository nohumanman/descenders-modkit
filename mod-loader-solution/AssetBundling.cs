using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ModLoaderSolution;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;

namespace ModLoaderSolution
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
                    Utilities.Log(www.error);
                else
                {
                    Utilities.Log("ModLoaderInstaller.Installer | Saving new ModLoaderSolution.bin");
                    try
                    {
                        System.IO.File.WriteAllBytes(bundlePath, www.downloadHandler.data);
                    }
                    catch (IOException)
                    {
                        Utilities.Log("ModLoaderInstaller.Installer | IOException - bin write has failed!");
                    }
                    LoadBundle(bundlePath);
                }
            }
        }
        public void LoadBundle(string bundlePath)
        {
            Utilities.LogMethodCallStart();
            bundle = AssetBundle.LoadFromFile(bundlePath);
            OnBundleLoad();
            Utilities.LogMethodCallEnd();
        }
        public void OnBundleLoad()
        {
            GameObject asst = bundle.LoadAsset<GameObject>("BikeSwitcherRadial");
            GameObject bikeSwitcherRadial = Instantiate(asst);
            DontDestroyOnLoad(bikeSwitcherRadial.transform.root);
            bikeSwitcherRadial.AddComponent<ObjEnabler>();
            foreach (Transform chil in bikeSwitcherRadial.transform.GetComponentsInChildren<Transform>())
                if (chil.parent == bikeSwitcherRadial.transform)
                    bikeSwitcherRadial.GetComponent<ObjEnabler>().obj = chil.gameObject;
            foreach (Button btn in bikeSwitcherRadial.GetComponent<ObjEnabler>().obj.GetComponentsInChildren<Button>())
            {
                //btn.gameObject.AddComponent<ButtonHack>();
                string bike = "";
                foreach (Transform chil in btn.transform)
                    bike = chil.GetComponent<Text>().text;
                btn.onClick.AddListener(() => { FindObjectOfType<BikeSwitcher>().ToBike(bike, (new PlayerIdentification.SteamIntegration()).getSteamId()); });
            }
            bikeSwitcherRadial.GetComponent<ObjEnabler>().obj.SetActive(false);

            GameObject TimerCanvasAsset = bundle.LoadAsset<GameObject>("TimerCanvas");
            GameObject TimerCanvas = Instantiate(TimerCanvasAsset);
            TimerCanvas.transform.position -= new Vector3(0, 1000);
            foreach(Transform child in TimerCanvas.transform)
            {
                foreach(Transform childs_child in child)
                {
                    if (childs_child.gameObject.name == "TimerText")
                        childs_child.gameObject.AddComponent<SplitTimerText>();
                    if (childs_child.gameObject.name == "TextShadow")
                        childs_child.gameObject.AddComponent<TimerCopier>();
                }
            }
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
