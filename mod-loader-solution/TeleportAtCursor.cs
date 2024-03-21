using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ModLoaderSolution;
using System;

namespace ModLoaderSolution
{
    class TeleportAtCursor : MonoBehaviour
    {
        bool teleportAtNextMouseClick = false;
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
                if (teleportAtNextMouseClick || Input.GetKey(KeyCode.T))
                {
                    teleportAtNextMouseClick = false;

                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    Vector3 x = new Vector3(0, 4, 0);
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                        Utilities.GetPlayer().transform.position = hit.point + x;
                }
            
        }
        public void TeleportToggle()
        {
            teleportAtNextMouseClick = !teleportAtNextMouseClick;
        }
    }
}
