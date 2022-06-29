using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ModLoaderSolution;
using SplitTimer;

namespace CustomTeleporter
{
    public class CustomTeleporter : MonoBehaviour
    {
        IEnumerator coroutine;
        GameObject PlayerHuman;
        Vector3 RespawnPos;
        Vector3 RespawnSpeed;
        Quaternion RespawnRot;
        bool respawning = false;
        bool usingXbox = false;
        public void Start()
        {
            StartCoroutine(CheckForFastTrack());
        }
        public void FixedUpdate()
        {
            if (PlayerHuman == null)
                PlayerHuman = GameObject.Find("Player_Human");
            else
            {
                foreach(string name in Input.GetJoystickNames()){
                    if (name == "Controller (Xbox One For Windows)")
                        usingXbox = true;
                }
                if (((Input.GetKeyDown("joystick button 2") && !usingXbox) || (Input.GetKeyDown("joystick button 1") && usingXbox) || Input.GetKeyDown(KeyCode.R)) && Utilities.instance.hasBailed() && !respawning)
                {
                    if (coroutine != null)
                        StopCoroutine(coroutine);
                    coroutine = Respawn();
                    StartCoroutine(coroutine);
                }
                if (!Utilities.instance.hasBailed() && !Utilities.instance.isInAir() && Utilities.instance.AngleFromGround() < 50)
                {
                    RespawnPos = PlayerHuman.transform.position;
                    RespawnRot = PlayerHuman.transform.rotation;
                    RespawnSpeed = Utilities.instance.GetPlayer().GetComponent<Rigidbody>().velocity;
                }
            }
        }
        IEnumerator CheckForFastTrack()
        {
            while (true)
            {
                if (respawning)
                {
                    if ((Input.GetKeyDown("joystick button 1") && !usingXbox) || (Input.GetKeyDown("joystick button 0") && usingXbox) || Input.GetKeyDown(KeyCode.Return))
                    {
                        Debug.Log("Fast tracked!!! Speed back to one!");
                        TimeModifier.Instance.speed = 1f;
                        respawning = false;
                        StopCoroutine(coroutine);
                    }
                    if (Input.GetKeyDown("joystick button 5") || Input.GetKeyDown(KeyCode.Backslash))
                    {
                        Utilities.instance.GetPlayer().SendMessage("SetVelocity", Vector3.zero);
                    }
                }
                yield return null;
            }
        }
        IEnumerator Respawn()
        {
            Debug.Log("Respawning!");
            respawning = true;
            yield return new WaitForSeconds(0.01f);
            PlayerHuman.transform.position = RespawnPos;
            PlayerHuman.transform.rotation = RespawnRot;
            Camera.main.SendMessage("SnapCamera", true);
            Camera.main.SendMessage("SnapCamera", false);
            Camera.main.GetComponent<BikeCamera>().SnapCamera();
            Camera.main.GetComponent<BikeCamera>().SnapCamera(false);
            GameObject.FindObjectOfType<BikeCamera>().SetReverse(true);
            GameObject.FindObjectOfType<BikeCamera>().SetReverse(false);
            Utilities.instance.GetPlayer().SendMessage("SetVelocity", RespawnSpeed);
            TimeModifier.Instance.speed = 0.05f;
            yield return new WaitForSecondsRealtime(3);
            TimeModifier.Instance.speed = 1f;
            respawning = false;
            Debug.Log("Respawned!");
        }
    }
}