using System.Collections;
using UnityEngine;
using ModTool.Interface;

namespace ModLoaderSolution
{
    public class MovableCam : ModBehaviour
    {
        public float posIncrement = 0.1f;
        public float rotIncrement = 5f;
        bool UseCustomCam = false;
        GameObject ExistingCamera;
        GameObject PlayerHuman;
        bool rotating;
        Transform prevParent;
        Vector3 DisplacementOfCam = Vector3.zero;
        Vector3 RotDisplacementOfCam = Vector3.zero;
        public void ToggleCustomCam()
        {
            if (ExistingCamera != null && PlayerHuman != null)
            {
                UseCustomCam = !UseCustomCam;
                if (UseCustomCam)
                {
                    prevParent = ExistingCamera.transform.parent;
                    ExistingCamera.transform.SetParent(PlayerHuman.transform);
                }
                else
                    ExistingCamera.transform.SetParent(prevParent);
            }
        }
        void Update()
        {
            if (UseCustomCam)
            {
                FindCam();
                FindPlayer();
                if (ExistingCamera != null && PlayerHuman != null)
                {

                    ExistingCamera.transform.localPosition = DisplacementOfCam;
                    ExistingCamera.transform.eulerAngles = PlayerHuman.transform.eulerAngles + RotDisplacementOfCam;
                }
            }
            if (Input.GetKeyDown(KeyCode.Tab))
                rotating = !rotating;
            if (Input.GetKey(KeyCode.Y) && Input.GetKey(KeyCode.Equals))
                if (!rotating)
                    DisplacementOfCam.y += posIncrement * Time.deltaTime;
                else
                    RotDisplacementOfCam.y += rotIncrement * Time.deltaTime;
            if (Input.GetKey(KeyCode.Y) && Input.GetKey(KeyCode.Minus))
                if (!rotating)
                    DisplacementOfCam.y -= posIncrement * Time.deltaTime;
                else
                    RotDisplacementOfCam.y -= rotIncrement * Time.deltaTime;
            if (Input.GetKey(KeyCode.X) && Input.GetKey(KeyCode.Equals))
                if (!rotating)
                    DisplacementOfCam.x += posIncrement * Time.deltaTime;
                else
                    RotDisplacementOfCam.x += rotIncrement * Time.deltaTime;
            if (Input.GetKey(KeyCode.X) && Input.GetKey(KeyCode.Minus))
                if (!rotating)
                    DisplacementOfCam.x -= posIncrement * Time.deltaTime;
                else
                    RotDisplacementOfCam.x -= rotIncrement * Time.deltaTime;
            if (Input.GetKey(KeyCode.Z) && Input.GetKey(KeyCode.Equals))
                if (!rotating)
                    DisplacementOfCam.z += posIncrement * Time.deltaTime;
                else
                    RotDisplacementOfCam.z += rotIncrement * Time.deltaTime;
            if (Input.GetKey(KeyCode.Z) && Input.GetKey(KeyCode.Minus))
                if (!rotating)
                    DisplacementOfCam.z -= posIncrement * Time.deltaTime;
                else
                    RotDisplacementOfCam.z -= rotIncrement * Time.deltaTime;
        }
        public void FindCam()
        {
            if (ExistingCamera == null && Camera.main != null)
                ExistingCamera = Camera.main.gameObject;
        }
        public void FindPlayer()
        {
            if (PlayerHuman == null && Utilities.GetPlayer() != null)
                PlayerHuman = Utilities.GetPlayer();
        }
    }
}