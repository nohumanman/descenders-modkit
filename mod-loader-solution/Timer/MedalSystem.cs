using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ModLoaderSolution
{
	public class MedalSystem : MonoBehaviour
	{
        [Header("Rainbow Medal")]
        public GameObject rainbowMedalGot;
        public GameObject rainbowMedalNotGot;
        [Header("Gold Medal")]
        public GameObject goldMedalGot;
        public GameObject goldMedalNotGot;
        [Header("Silver Medal")]
        public GameObject silverMedalGot;
        public GameObject silverMedalNotGot;
        [Header("Bronze Medal")]
        public GameObject bronzeMedalGot;
        public GameObject bronzeMedalNotGot;
        [Header("Config")]
        public string trailName;
        void Start()
        {
            rainbowMedalGot.SetActive(false);
            rainbowMedalNotGot.SetActive(false);
            goldMedalGot.SetActive(false);
            goldMedalNotGot.SetActive(false);
            silverMedalGot.SetActive(false);
            silverMedalNotGot.SetActive(false);
            bronzeMedalGot.SetActive(false);
            bronzeMedalNotGot.SetActive(false);
        }
        public void NetStart()
        {
            Utilities.Log("MedalSystem | NetStart() called for '" + trailName + "' sending GET_MEDALS");
            NetClient.Instance.SendData("GET_MEDALS", trailName);
        }
    }
}
