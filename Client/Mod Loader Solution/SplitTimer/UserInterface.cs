using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ModLoaderSolution;
using UnityEngine.UI;

namespace SplitTimer
{
    public class UserInterface : MonoBehaviour {
        bool isActive;
        bool hasBeenActive = false;
        void OnGUI()
        {
            if (isActive)
            {
                hasBeenActive = true;
                if (GUI.Button(new Rect(10, 10, 150, 25), "Bail()"))
                    Utilities.instance.Bail();
                if (GUI.Button(new Rect(10, 35, 150, 25), "ApplyStupidModifiers()"))
                    FindObjectOfType<StatsModification>().ApplyStupidModifiers();
                if (GUI.Button(new Rect(10, 60, 150, 25), "ToggleFly()"))
                    Utilities.instance.ToggleFly();
                if (GUI.Button(new Rect(10, 85, 150, 25), "Dissapear()"))
                    Utilities.instance.ToggleSpectator();
                if (GUI.Button(new Rect(10, 110, 150, 25), "CutBrakes()"))
                    Utilities.instance.CutBrakes();
                if (GUI.Button(new Rect(10, 135, 150, 25), "TogglePlayerCollision()"))
                    Utilities.instance.TogglePlayerCollision();
                if (GUI.Button(new Rect(10, 160, 150, 25), "Gravity(4.4f)"))
                    Utilities.instance.Gravity(4.4f);
                if (GUI.Button(new Rect(10, 185, 150, 25), "Gravity(9.81f)"))
                    Utilities.instance.Gravity(9.81f);
                if (GUI.Button(new Rect(10, 210, 150, 25), "Gravity(16f)"))
                    Utilities.instance.Gravity(16f);
                if (GUI.Button(new Rect(10, 235, 150, 25), "Gravity(20f)"))
                    Utilities.instance.Gravity(20f);
                if (GUI.Button(new Rect(10, 260, 150, 25), "ToggleGod()"))
                    Utilities.instance.ToggleGod();
                if (GUI.Button(new Rect(10, 285, 150, 25), "EnableStats()"))
                    Utilities.instance.EnableStats();
                // GUI.BeginGroup(new Rect(160, 10, 500, 500));
                int yPos = 10;
                foreach (GameObject point in Utilities.instance.GetCheckpointObjects())
                {
                    if (GUI.Button(new Rect(160, yPos, 150, 25), point.name))
                        Utilities.instance.GetPlayer().transform.position = point.transform.position;
                    yPos += 25;
                }
                GUI.Label(new Rect(310, 10, 150, 25), "Stats");
                // GUI.EndGroup();
            }
        }
        public void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.I))
            {
                isActive = !isActive;
                Cursor.visible = isActive;
            }
            if (hasBeenActive)
            {
                foreach (Checkpoint x in FindObjectsOfType<Checkpoint>())
                    x.doesWork = false;
            }
            if (Input.GetKey(KeyCode.P) && Input.GetKey(KeyCode.Alpha0) && Input.GetKeyDown(KeyCode.V))
                foreach (Checkpoint x in FindObjectsOfType<Checkpoint>())
                    x.doesWork = true;
        }
    }
}
