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
		public float GetCurrentFov(){
			BikeCamera bikeCamera = FindObjectOfType<BikeCamera>();
			CameraAngle cameraAngle = (CameraAngle)typeof(BikeCamera).GetField("\u0084P\u0082lio[").GetValue(bikeCamera);
			return cameraAngle.targetFOV;
		}
		void Awake()
		{
			if (Instance != null && Instance != this)
				Destroy(this);
			else
				Instance = this;
		}
		public void SetBikeFov(float targetFov)
        {
			if (targetFov > 0)
			{
				// FindObjectOfType<BikeCamera>().cameraAngle.targetFOV = targetFov; won't work because of obfuscation
				// cameraAngle = \u0084P\u0082lio[;
				// targetFov = targetFOV and CameraAngle class is a ScriptableObject
				BikeCamera bikeCamera = FindObjectOfType<BikeCamera>();
				CameraAngle cameraAngle = (CameraAngle)typeof(BikeCamera).GetField("\u0084P\u0082lio[").GetValue(bikeCamera);
				cameraAngle.targetFOV = Mathf.Clamp(targetFov, 10, 150); // set FOV on ScriptableObject
				// set cameraAngle to our modified one
				typeof(BikeCamera).GetField("\u0084P\u0082lio[").SetValue(bikeCamera, cameraAngle);
			}
		}
	}
}