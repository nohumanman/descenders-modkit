using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ModLoaderSolution;
using System;

namespace ModLoaderSolution
{
    public class CustomTeleporter : MonoBehaviour
    {
        IEnumerator coroutine;
        GameObject PlayerHuman;
        Vector3 RespawnPos;
        Vector3 RespawnSpeed;
        Quaternion RespawnRot;
        public float lastTimeRespawnSet;
        float periodOfCheckpointSet = 2;
        public GameObject enableOnRespawnXbox;
        public GameObject enableOnRespawnPs4;
        bool respawning = false;
        bool usingXbox = false;
        bool bailedPreviously = false;
        bool shouldUtilise = true;
        public void Start()
        {
            StartCoroutine(bailedPrev());
            lastTimeRespawnSet = Time.time;
        }
        public void Update()
        {

            if (Input.GetKey(KeyCode.N) && Input.GetKeyDown(KeyCode.D))
                shouldUtilise = !shouldUtilise;

                foreach (string name in Input.GetJoystickNames())
                {
                    if (name == "Controller (Xbox One For Windows)")
                        usingXbox = true;
                }
                if (enableOnRespawnPs4 != null && !usingXbox)
                    enableOnRespawnPs4.SetActive(respawning);
                if (enableOnRespawnXbox != null && usingXbox)
                    enableOnRespawnXbox.SetActive(respawning);
                bool hasPressedB = (
                    (Input.GetKeyDown("joystick button 2") && !usingXbox)
                    || (Input.GetKeyDown("joystick button 1") && usingXbox)
                    || Input.GetKeyDown(KeyCode.R)
                );
                bool hasPressedA = (
                    (Input.GetKeyDown("joystick button 1") && !usingXbox)
                    || (Input.GetKeyDown("joystick button 0") && usingXbox)
                    || Input.GetKeyDown(KeyCode.Return)
                );
            if (shouldUtilise)
            {
                if (respawning)
                {
                    if (hasPressedA || hasPressedB)
                    {
                        if (coroutine != null)
                            StopCoroutine(coroutine);
                        TimeModifier.Instance.speed = 1f;
                        respawning = false;
                    }
                    if (Input.GetKeyDown("joystick button 5") || Input.GetKeyDown(KeyCode.Backslash))
                        Utilities.GetPlayer().SendMessage("SetVelocity", Vector3.zero);
                }
                if (hasPressedB && bailedPreviously && !respawning)
                {
                    if (coroutine != null)
                        StopCoroutine(coroutine);
                    coroutine = Respawn();
                    StartCoroutine(coroutine);
                }
            }
        }
        IEnumerator bailedPrev()
        {
            while (true)
            {
                bailedPreviously = Utilities.instance.hasBailed();
                yield return new WaitForSecondsRealtime(0.2f);
            }
        }
        public void FixedUpdate()
        {
            bool shouldSetRespawn = (
                !Utilities.instance.hasBailed()
                && !Utilities.instance.isInAir()
                && Utilities.instance.AngleFromGround() < 50
                && !respawning
            );
            if (PlayerHuman == null)
                PlayerHuman = Utilities.GetPlayer();
            else
            {
                if (shouldSetRespawn && Time.time-lastTimeRespawnSet>periodOfCheckpointSet)
                {
                    RespawnPos = PlayerHuman.transform.position;
                    RespawnRot = PlayerHuman.transform.rotation;
                    RespawnSpeed = Utilities.GetPlayer().GetComponent<Rigidbody>().velocity;
                    lastTimeRespawnSet = Time.time;
                }
            }
        }
        IEnumerator Respawn()
        {
            Utilities.Log("Respawning!");
            respawning = true;
            yield return new WaitForSeconds(0.01f);
            PlayerHuman.transform.position = RespawnPos;
            PlayerHuman.transform.rotation = RespawnRot;
            Utilities.GetPlayer().SendMessage("SetVelocity", RespawnSpeed);
            yield return new WaitForEndOfFrame();
            FindObjectOfType<BikeCamera>().SetTarget(Utilities.instance.GetPlayerInfoImpact(), true);
            TimeModifier.Instance.speed = 0.05f;
            yield return new WaitForSecondsRealtime(5);
            TimeModifier.Instance.speed = 1f;
            respawning = false;
            Utilities.Log("Respawned!");
        }
    }
}