using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ModLoaderSolution;

namespace ModLoaderSolution
{
	public class SloMoZone : MonoBehaviour
	{
        void OnTriggerEnter(Collider other)
        {
            if (other.transform.name == "Bike" && other.transform.root.name == "Player_Human")
            {
                TimeModifier.Instance.speed = 0.2f;
            }
        }
        void OnTriggerExit(Collider other)
        {
            if (other.transform.name == "Bike" && other.transform.root.name == "Player_Human")
            {
                TimeModifier.Instance.speed = 1f;
            }
        }
    }
}