﻿using System;
using UnityEngine;
using ModLoaderSolution;


namespace ModLoaderSolution
{
    public class TimeModifier : MonoBehaviour
    {
        public static TimeModifier Instance { get; private set; }
        public float speed = 1f;
        public bool bother = false;
        void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(this);
            else
                Instance = this;
        }
        void Update()
        {
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Y))
            {
                UserInterface.Instance.SpecialNotif("Slow-Mo on joystick: " + (!bother).ToString());
                bother = !bother;
            }
            if (!bother)
            {
                speed = 1f;
                return;
            }

            if (Input.GetKeyDown("joystick button 8"))
                speed = 0.5f;
            if (Input.GetKeyUp("joystick button 8"))
                speed = 1f;
            if (Utilities.instance.isInReplayMode())
                return;
            if (!Utilities.instance.isInPauseMenu())
                Time.timeScale = speed;
            else
                Time.timeScale = 0f;
        }
    }
}
