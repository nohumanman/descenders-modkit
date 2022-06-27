using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ModLoaderSolution;

namespace CustomTeleporter
{
    public class CustomTeleporter : MonoBehaviour
    {
        GameObject PlayerHuman;
        Vector3 RespawnPoint;
        Vector3 PreviousPos;
        Quaternion RespawnRot;
        bool respawning = false;
        public void FixedUpdate()
        {
            if (PlayerHuman == null)
                PlayerHuman = GameObject.Find("Player_Human");
            else
            {

                if (Utilities.instance.hasBailed())
                    StartCoroutine(Respawn());
                // if (Vector3.Distance(PreviousPos, PlayerHuman.transform.position) > 1)
                        
                if (!Utilities.instance.isInAir() && !Utilities.instance.hasBailed() && !respawning) {
                    RespawnPoint = PlayerHuman.transform.position;
                    RespawnRot = PlayerHuman.transform.rotation;
                }
                PreviousPos = PlayerHuman.transform.position;
            }
        }
        IEnumerator Respawn()
        {
            Debug.Log("Respawning!!!!!!!!!!!!!!");
            respawning = true;
            yield return new WaitForSeconds(0.01f);
            PlayerHuman.transform.position = RespawnPoint;
            PlayerHuman.transform.rotation = RespawnRot;
            // Camera.main.SendMessage("SnapCamera");
            yield return new WaitForEndOfFrame();
            Camera.main.GetComponent<BikeCamera>().SnapCamera(false);
            GameObject.FindObjectOfType<BikeCamera>().SnapCamera(false);
            GameObject.FindObjectOfType<BikeCamera>().SnapCamera(true);
            // GameObject.FindObjectOfType<BikeCamera>().AddCameraShake(500000);
            respawning = false;
        }
    }
}