using System;
using UnityEngine;
using ModLoaderSolution;


namespace ModLoaderSolution
{
    public class TimeModifier : MonoBehaviour
    {
        public static TimeModifier Instance { get; private set; }
        public float speed = 1f;
        void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(this);
            else
                Instance = this;
        }
        void Update()
        {
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
