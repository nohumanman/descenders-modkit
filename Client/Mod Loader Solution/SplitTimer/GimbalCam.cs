using System;
using UnityEngine;
using ModLoaderSolution;
using System.Collections;
using System.Collections.Generic;

namespace SplitTimer
{
    public class GimbalCam : MonoBehaviour
    {
        public GameObject ExistingCamera;
        public bool ShouldLevel;
        public void Start()
        {
            StartCoroutine(UpdateCamera());
        }
        public void Update()
        {
            if (ExistingCamera == null)
                ExistingCamera = Camera.main.gameObject;
            if (Input.GetKey(KeyCode.G) && Input.GetKeyDown(KeyCode.C))
                ShouldLevel = !ShouldLevel;
        }
        public void LevelCamera()
        {
            if (ExistingCamera != null)
                ExistingCamera.transform.eulerAngles = new Vector3(
                    ExistingCamera.transform.eulerAngles.x,
                    ExistingCamera.transform.eulerAngles.y,
                    ExistingCamera.transform.eulerAngles.z * 0
                );
        }
        public IEnumerator UpdateCamera()
        {
            while (true)
            {
                if (ShouldLevel)
                    LevelCamera();
                yield return null;
            }
        }
    }
}
