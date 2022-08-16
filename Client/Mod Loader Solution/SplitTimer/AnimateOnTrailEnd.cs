using System;
using UnityEngine;
using System.Collections;
using ModLoaderSolution;

namespace SplitTimer
{
    public class AnimateOnTrailEnd : MonoBehaviour
    {
        public string trailName;
        public Animator animator;
        public GameObject[] ps4UI;
        public GameObject[] xboxUI;
        bool trailEnded;
        bool usingXbox;
        public void Start()
        {
            animator.Play("Hide");
        }
        public void TrailEnd()
        {
            trailEnded = true;
            Utilities.instance.ToggleControl(false);
            animator.Play("Show");
        }
        public void Update()
        {
            if (trailEnded)
            {
                foreach (string name in Input.GetJoystickNames())
                {
                    if (name == "Controller (Xbox One For Windows)")
                        usingXbox = true;
                }
                foreach(GameObject ui in ps4UI)
                    ui.SetActive(!usingXbox);
                foreach (GameObject ui in xboxUI)
                    ui.SetActive(usingXbox);
                bool hasPressedA = (
                    (Input.GetKeyDown("joystick button 1") && !usingXbox)
                    || (Input.GetKeyDown("joystick button 0") && usingXbox)
                    || Input.GetKeyDown(KeyCode.Return)
                );
                if (hasPressedA) {
                    animator.Play("Hide");
                    Utilities.instance.ToggleControl(true);
                    trailEnded = !trailEnded;
                }
            }
        }
    }
}
