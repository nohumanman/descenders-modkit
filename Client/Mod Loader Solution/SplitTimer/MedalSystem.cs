using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SplitTimer
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
        public void NetStart()
        {
            NetClient.Instance.SendData("GET_MEDALS|" + trailName);
        }
    }
}
