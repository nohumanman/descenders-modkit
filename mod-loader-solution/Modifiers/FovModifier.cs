using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ModLoaderSolution;
using System;

namespace ModLoaderSolution
{
	public class FovModifier : MonoBehaviour
	{
		public float targetFov = -1;
		public float fovOffset = 0;

		public void Update()
		{
			foreach (Camera x in FindObjectsOfType<Camera>())
			{
				x.fieldOfView += fovOffset;
			}
		}
	}
}