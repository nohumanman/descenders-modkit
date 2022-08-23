using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using ModLoaderSolution;

namespace SplitTimer
{
    public struct Location {
		public float timestamp;
		public Vector3 position;
	}

    public class RunChecker : MonoBehaviour
	{
		public Location[] locations;
		public bool gotAllLocations = false;
		public void Start()
        {
			StartCoroutine(MakeRun());
        }
		public IEnumerator MakeRun()
        {
            while (!gotAllLocations)
            {
				yield return null;
            }
			GameObject ourPlayer = GameObject.CreatePrimitive(PrimitiveType.Cube);
			int i = 0;
			foreach(Location location in locations)
            {
				ourPlayer.transform.position = location.position;
				yield return new WaitForSeconds(locations[i+1].timestamp-location.timestamp);
            }
			Destroy(ourPlayer);
			Destroy(gameObject);
		}
	}
}
