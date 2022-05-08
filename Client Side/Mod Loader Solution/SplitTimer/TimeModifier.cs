using System;
using UnityEngine;


namespace SplitTimer
{
    public class TimeModifier : MonoBehaviour
    {
        public float speed = 1f;
        void Update()
        {
            Time.timeScale = speed;
        }
    }
}
