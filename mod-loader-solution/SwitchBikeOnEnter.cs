using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ModLoaderSolution;

namespace ModLoaderSolution
{
    public class SwitchBikeOnEnter : MonoBehaviour
    {
		public string BikeToSwitchTo;
		void OnTriggerEnter(Collider col)
		{
			FindObjectOfType<BikeSwitcher>().ToBike(BikeToSwitchTo, (new PlayerIdentification.SteamIntegration().getSteamId()));
		}
	}
}