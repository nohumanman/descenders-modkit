using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ModLoaderSolution;

namespace SplitTimer
{
    public class CustomAnim : MonoBehaviour
    {
        bool wasInAir = false;
        public void TriggerTrickAnim()
        {
            Cyclist _cyclist = FindObjectOfType<Cyclist>();
            Gesture normalGest = _cyclist.GetGesture(0);
            // prints "NoHander"
            Debug.Log(normalGest);
            // begins animation and then quits immediately after
            FindObjectOfType<Cyclist>().activeGesture = normalGest;
            // doesn't do a whole lot:
            // FindObjectOfType<Cyclist>().LoadGestureState(0, 0, 20);
        }
        public void Update()
        {
            if (Utilities.instance.isInAir() && !(Input.GetKey(KeyCode.R)))
                if (!wasInAir)
                {
                    TriggerTrickAnim();
                    wasInAir = true;
                }
            else
                wasInAir = false;
        }
    }
}
