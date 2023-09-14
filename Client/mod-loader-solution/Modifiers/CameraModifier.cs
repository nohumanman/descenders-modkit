using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ModLoaderSolution;
using System;

namespace ModLoaderSolution
{
    public class CameraModifier : MonoBehaviour
	{
		public float farClipPlane = -1;
		public float nearClipPlane = -1;
		public static CameraModifier Instance { get; private set; }
		void Awake()
		{
			if (Instance != null && Instance != this)
				Destroy(this);
			else
				Instance = this;
		}
		public void Update()
        {
			foreach(Camera x in FindObjectsOfType<Camera>())
            {
				if (farClipPlane >= 0)
					x.farClipPlane = farClipPlane;
				if (nearClipPlane >= 0)
					x.nearClipPlane = nearClipPlane;
			}			
		}
    }
}