﻿using UnityEngine;
using System.Collections;

namespace SplitTimer
{
    public class GimbalCam : MonoBehaviour
    {
        public GameObject ExistingCamera;
        public bool ShouldLevel = false;
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
            {
                GameObject _player = GameObject.Find("Player_Human");
                if (_player != null)
                    transform.eulerAngles = _player.transform.eulerAngles;
            }
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