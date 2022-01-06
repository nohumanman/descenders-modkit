using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;
using UnityEngine.Networking;
using PlayerIdentification;

namespace CustomTimer
{

    public class Submit : ModBehaviour
    {
        private SteamIntegration steamIntegration = new SteamIntegration();
        public Identification id;
        [Tooltip("The name of the map's sheet")]
        public string mapName;
        [Tooltip("URL to send data to")]
        public string url;
        public void SubmitTime(string formattedTimeCount)
        {
            Debug.Log("Submitting Time");
            string submitTime = formattedTimeCount;
            Debug.Log(submitTime);
            string steamID = steamIntegration.getNameOfPlayer().steamID;
            string name = steamIntegration.getNameOfPlayer().playerName;
            Debug.Log(url + "?Request-Type=submitData&map=" + mapName + "&time=" + submitTime + "&name=" + name + "&steamID=" + steamID);
            StartCoroutine(getRequest(url + "?Request-Type=submitData&map=" + mapName + "&time=" + submitTime + "&name=" + name + "&steamID=" + steamID));
        }

        IEnumerator getRequest(string uri)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                yield return webRequest.SendWebRequest();
            }
        }
    }
}
