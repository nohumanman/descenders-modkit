using System;
using UnityEngine;


namespace SplitTimer
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
            Time.timeScale = speed;
        }
    }
}
