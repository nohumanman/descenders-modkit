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
		public static FovModifier Instance { get; private set; }
		public float targetFov = -1;
		public float fovOffset = 0;
		void Awake()
		{
			if (Instance != null && Instance != this)
				Destroy(this);
			else
				Instance = this;
		}
		public void Update()
		{
			foreach (Camera x in FindObjectsOfType<Camera>())
			{
				x.fieldOfView += fovOffset;
			}
		}
	}
}