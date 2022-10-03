using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ModLoaderSolution;

namespace SplitTimer
{
	public class BikeSwitcher : MonoBehaviour
	{
        public string oldBike;
        public int GetBike()
        {
            return 404;
        }
		public void ToEnduro()
        {
            gameObject.GetComponent<Utilities>().SetBike(0);
            PlayerInfo.Instance.OnBikeSwitch(oldBike, "enduro");
            oldBike = "enduro";
        }
		public void ToDownhill()
        {
            gameObject.GetComponent<Utilities>().SetBike(1);
            PlayerInfo.Instance.OnBikeSwitch(oldBike, "downhill");
            oldBike = "downhill";
        }
		public void ToHardtail()
        {
            gameObject.GetComponent<Utilities>().SetBike(2);
            PlayerInfo.Instance.OnBikeSwitch(oldBike, "hardtail");
            oldBike = "hardtail";
        }
	}
}