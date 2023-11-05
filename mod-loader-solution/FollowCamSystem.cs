using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ModLoaderSolution;
using System;

namespace ModLoaderSolution
{
    public class FollowCamSystem : MonoBehaviour
    {
        public List<Vector3> camLocations = new List<Vector3>() { new Vector3(5, 210, 525), new Vector3(41, 197, 507), new Vector3(57, 201, 511) };
        public Vector3 currentCamLoc = Vector3.zero;
        public void Update()
        {
            Utilities.instance.DisableControlledCam();
            float closest = Mathf.Infinity;
            bool anyValid = false;
            foreach (Vector3 vector3 in camLocations)
                if (IsValid(vector3)){ // if camera is valid
                    float distanceToCam = Vector3.Distance(vector3, Utilities.instance.GetPlayer().transform.position);
                    // if this is the closest to our player
                    if (distanceToCam < closest)
                    {
                        anyValid = true;
                        closest = distanceToCam; // set as closest
                        currentCamLoc = vector3; // set as the one
                    }
                }
            if (!anyValid)
                currentCamLoc = Utilities.instance.GetPlayer().transform.position;
            if (currentCamLoc != Vector3.zero)
            {
                foreach(Camera cam in FindObjectsOfType<Camera>())
                {
                    // set position of camera to current camera location
                    cam.transform.position = currentCamLoc;
                    // temporarily just set the rotation to look at player
                    cam.transform.LookAt(Utilities.instance.GetPlayer().transform);
                }
            }
        }
        public bool IsValid(Vector3 camPos, int proximity = 5000)
        {
            if (Utilities.instance.GetPlayer() == null)
                return false;
            if (Vector3.Distance(Utilities.instance.GetPlayer().transform.position, camPos) > proximity)
                return false;
            // if player is in line of sight of camera
            Vector3 directionToPlayer = Utilities.instance.GetPlayer().transform.position - camPos;
            RaycastHit hit;
            if (Physics.Raycast(camPos, directionToPlayer, out hit, proximity))
                if (hit.collider.gameObject.transform.root.gameObject == Utilities.instance.GetPlayer())  // root of collider is player_human
                    return true; // player is in line of view
            return false;
        }
    }
}