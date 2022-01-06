using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;

namespace CustomTimer
{
    public class Boundary : ModBehaviour
    {
        [System.NonSerialized]
        public bool isInside = false;
        void OnTriggerStay(Collider other)
        {
            if (other.gameObject.transform.root.name == "Player_Human")
            {
                isInside = true;
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.transform.root.name == "Player_Human")
            {
                isInside = false;
            }
        }
    }
}