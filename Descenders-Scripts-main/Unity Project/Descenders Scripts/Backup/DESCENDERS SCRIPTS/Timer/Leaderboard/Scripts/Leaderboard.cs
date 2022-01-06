using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;
using UnityEngine.Networking;


// Version 1.0.1
namespace CustomLeaderboard {
    public class Leaderboard : ModBehaviour {
        [Tooltip("This is the name of your map, it will be displayed on the leaderboard")]
        public string map;
        [Tooltip("This is the passcode of your map, please do not forget this. If you get this incorrect, you will not be able to submit new scores.")]
        public string url  = "";
        [Tooltip("The GameObject that contains text to modify")]
        public GameObject text;

        bool requestSent = false;
        void Start()
        {
            RefreshLeaderboard();
        }

        public void RefreshLeaderboard()
        {
            requestSent = false;
            Debug.Log((url + "?Request-Type=getData&map=" + map));
            StartCoroutine(GetRequest(url + "?Request-Type=getData&map=" + map));
        }

        IEnumerator GetRequest(string uri)
        {
            string result = null;
            WWWForm data = new WWWForm();

            if (!requestSent){
                using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
                {
                    // Request and wait for the desired page.
                    yield return webRequest.SendWebRequest();
                    if (webRequest.downloadHandler.isDone)
                    {
                        Debug.Log(webRequest.downloadHandler.text);
                        result = webRequest.downloadHandler.text;
                    }
                }
            }
            if (result != null)
            {
                LeaderboardData jsonobject = JsonUtility.FromJson<LeaderboardData>(result);
                string final_results = "";
                for (int i = 0; (jsonobject.name.Count > i); i++)
                {
                    if (i != 0){
                        final_results += (i).ToString() + ". " + jsonobject.name[i] + " - " + jsonobject.time[i] + " - Times Played: " + jsonobject.timesPlayed[i] + "\n";
                    }
                    
                }
                text.GetComponent<TextMesh>().text = map + "\n" + final_results;
            }
        }

        class LeaderboardData
        {
            public List<string> name;
            public List<string> steamId;
            public List<string> time;
            public List<string> timesPlayed;
        }

    }
}
