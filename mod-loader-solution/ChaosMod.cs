using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ModLoaderSolution;

namespace ModLoaderSolution
{
    public class ChaosMod : MonoBehaviour
    {
        bool isChaotic = false;
        AudioSource _audioSource;
        float startTime;
        public void StartChaos()
        {
            isChaotic = true;
            startTime = Time.time;
            AudioClip chaosBegin = AssetBundling.Instance.bundle.LoadAsset<AudioClip>("ChaosBegin.mp3");
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.PlayOneShot(chaosBegin);
            StartCoroutine(SpawnBannersOnPlayer(200));
        }
        void OnGUI()
        {
            if (!isChaotic)
                return;
            GUIStyle myButtonStyle2 = new GUIStyle(GUI.skin.button);
            myButtonStyle2.normal.textColor = Color.white;
            myButtonStyle2.normal.background = UserInterface.MakeTex(5, 5, new Color(0.2f, 0.06f, 0.12f));
            myButtonStyle2.fontSize = 60;
            GUI.Label(new Rect((Screen.width / 2) - 750, Screen.height - 160, 1500, 80), "scripts made with love by nohumanman :D", myButtonStyle2);
            GUI.Label(new Rect((Screen.width / 2) - 750, Screen.height - 80, 1500, 80), "btw you have " + Mathf.Round(20-(Time.time-startTime)) + " until the next chaos", myButtonStyle2);
        }
        IEnumerator SwitchThoseBikes()
        {
            string[] bikes = { "Coffee_Bike", "BMX", "Specialized_Demo", "Intense_M16", "Fish", "Canyon_Spectral" };
            while (true)
            {
                foreach (string bike in bikes)
                {
                    foreach (global::PlayerInfo inf in Singleton<PlayerManager>.SP.GetAllPlayers())
                        FindObjectOfType<BikeSwitcher>().ToBike(bike, Utilities.FromPlayerInfo(inf).steamID);
                    yield return new WaitForSeconds(5);
                }
            }            
        }
        IEnumerator SpawnCapsules()
        {
            GameObject Capsule_W_Rbody = AssetBundling.Instance.bundle.LoadAsset<GameObject>("Capsule_W_Rbody");
            while (true)
            {;
                GameObject bannerInstance = Instantiate(Capsule_W_Rbody);
                bannerInstance.transform.position = Utilities.GetPlayer().transform.position;
                bannerInstance.transform.rotation = Utilities.GetPlayer().transform.rotation;
                bannerInstance.transform.position += Utilities.GetPlayer().transform.forward.normalized * 10;
                yield return new WaitForSeconds(2f);
            }
        }
        IEnumerator SpawnBannersOnPlayer(int amountToSpawn)
        {
            int amountSpawned = 0;
            bool was1 = false;
            while (amountSpawned < amountToSpawn)
            {
                GameObject banner;
                if (was1)
                     banner = AssetBundling.Instance.bundle.LoadAsset<GameObject>("nohumanman_banner");
                else
                    banner = AssetBundling.Instance.bundle.LoadAsset<GameObject>("desc_comp_banner");
                GameObject bannerInstance = Instantiate(banner);
                bannerInstance.transform.position = Utilities.GetPlayer().transform.position;
                bannerInstance.transform.rotation = Utilities.GetPlayer().transform.rotation;
                was1 = !was1;
                amountSpawned++;
                yield return new WaitForSeconds(2f);
            }
        }
        public IEnumerator DoWeirdScaling()
        {
            float i = 1;
            while (true)
            {
                while (i < 1.5)
                {
                    GetComponent<Utilities>().SetPlayerSize(i);
                    i += 0.01f;
                    yield return new WaitForSeconds(0.1f);
                }
                while (i > 0.5)
                {
                    GetComponent<Utilities>().SetPlayerSize(i);
                    i -= 0.01f;
                    yield return new WaitForSeconds(0.1f);
                }
            }
            
        }
        int intensity = 0;
        public void Update()
        {
            if (isChaotic)
            {
                if (Time.time - startTime > 20)
                {
                    // new chaos aspect
                    
                    if (intensity == 0)
                        StartCoroutine(SpawnCapsules());
                    else if (intensity == 1)
                        StartCoroutine(SwitchThoseBikes());
                    else if (intensity == 2)
                        StartCoroutine(DoWeirdScaling());
                    else
                    {
                        intensity = 0;
                        foreach (Stat stat in StatsModification.instance.stats)
                            stat.currentVal = "20";
                    }
                    intensity++;
                    startTime = Time.time;
                }
            }
        }
    }
}