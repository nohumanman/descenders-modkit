using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ModLoaderSolution;
using System;
using System.IO;


namespace ModLoaderSolution
{
    public class Cam
    {
        public Vector3 loc = Vector3.zero; // the location of the camera
        public float panSpeed = 13; // the speed the camera follows the player rotationally
        public float proximity = 100; // the range a camera can see
        public float zoomAmount = 5f; // the amount the camera should be zoomed to the player
        public bool shouldZoom = true; // whether the camera should zoom to correct FOV
    }
    public class FollowCamSystem : MonoBehaviour
    {
        public GameObject subject;
        private List<Cam> cameras = new List<Cam>() { };
        private Cam currentCam;
        private bool shouldSnap = false;
        public bool bother = false;
        bool edit = false;
        public List<Cam> GetCameras()
        {
            return cameras;
        }
        public void OnGUI()
        {
            Utilities.LogMethodCallStart();
            UserInterface.Instance.InitGUI();
            if (!edit)
                return;
            int yPos = 20;
            int i = 0;
            List<string> title = new List<string>() { "X", "Y", "Z", "PanSpeed", "ShouldZoom", "Proximity", "zoomAmount"};
            float xPos = 20;
            foreach (string key in title)
            {
                GUI.Label(new Rect(xPos, yPos, 60, 20), key);
                xPos += 65;
            }
            yPos += 20;
            foreach (Cam cam in cameras)
            {
                Vector3 vector3 = cam.loc;
                xPos = 20;
                cam.loc.x = float.Parse(GUI.TextField(new Rect(xPos, yPos, 60, 20), cam.loc.x.ToString())); xPos += 65;
                cam.loc.y = float.Parse(GUI.TextField(new Rect(xPos, yPos, 60, 20), cam.loc.y.ToString())); xPos += 65;
                cam.loc.z = float.Parse(GUI.TextField(new Rect(xPos, yPos, 60, 20), cam.loc.z.ToString())); xPos += 65;
                cam.panSpeed = float.Parse(GUI.TextField(new Rect(xPos, yPos, 60, 20), cam.panSpeed.ToString())); xPos += 65;
                cam.shouldZoom = GUI.Toggle(new Rect(xPos, yPos, 60, 20), cam.shouldZoom, "shouldZoom"); xPos += 65;
                cam.proximity = float.Parse(GUI.TextField(new Rect(xPos, yPos, 60, 20), cam.proximity.ToString())); xPos += 65;
                cam.zoomAmount = float.Parse(GUI.TextField(new Rect(xPos, yPos, 60, 20), cam.zoomAmount.ToString())); xPos += 65;
                
                if (GUI.Button(new Rect(xPos, yPos, 60, 20), "SET"))
                    cameras[i].loc = Camera.main.transform.position;
                xPos += 60;
                if (GUI.Button(new Rect(xPos, yPos, 60, 20), "GOTO"))
                    Camera.main.transform.position = vector3;
                xPos += 60;
                if (GUI.Button(new Rect(xPos, yPos, 60, 20), "DEL"))
                    cameras.RemoveAt(i);
                yPos += 22;
                i++;
            }
            xPos = 800;
            yPos = 22;
            if (GUI.Button(new Rect(xPos, 20 + yPos, 240, 20), "Toggle Follow Cam", UserInterface.Instance.customButton))
                bother = !bother;
            yPos += 22;
            if (GUI.Button(new Rect(xPos, 20 + yPos, 240, 20), "Look at me once", UserInterface.Instance.customButton))
                Camera.main.transform.LookAt(Utilities.GetPlayer().transform);
            yPos += 22;
            if (GUI.Button(new Rect(xPos, 20 + yPos, 240, 20), "Add Camera", UserInterface.Instance.customButton))
                cameras.Add(new Cam());
            yPos += 22;
            if (GUI.Button(new Rect(xPos, 20 + yPos, 240, 20), "Save to cameras.txt", UserInterface.Instance.customButton))
                SaveToFile();
            yPos += 22;
            if (GUI.Button(new Rect(xPos, 20 + yPos, 240, 20), "Load from cameras.txt", UserInterface.Instance.customButton))
                LoadFromFile();
            GUIStyle myButtonStyle2 = new GUIStyle(GUI.skin.button);
            myButtonStyle2.normal.textColor = Color.white;
            myButtonStyle2.normal.background = UserInterface.MakeTex(5, 5, new Color(0.2f, 0.06f, 0.12f));
            myButtonStyle2.fontSize = 30;
            GUI.Label(new Rect(Screen.width - 300, Screen.height - 80, 300, 80), Camera.main.transform.position.ToString(), myButtonStyle2);
            Utilities.LogMethodCallEnd();
        }
        public Cam GetBestCamera()
        {
            Utilities.LogMethodCallStart();
            if (cameras.Count == 0)
                return null;
            if (Utilities.instance.hasBailed(subject))
                return null;
            float closest = Mathf.Infinity;
            Cam closestCam = cameras[0];
            float switchThreshold = 2f;
            
            foreach (Cam cam in cameras)
            {
                Vector3 vector3 = cam.loc;
                if (IsValid(vector3, (int)cam.proximity))
                {
                    float distanceToCam = Vector3.Distance(vector3, subject.transform.position);
                    if (distanceToCam < closest - switchThreshold) // if this is the closest to our player
                    {
                        closest = distanceToCam; // set as closest
                        closestCam = cam;
                    }
                }
            }

            Utilities.LogMethodCallEnd();
            if (closestCam.loc == Vector3.zero)
                return null;
            return closestCam;
        }
        public void Update()
        {
            Utilities.LogMethodCallStart();
            if (Input.GetKeyDown(KeyCode.M) && Input.GetKey(KeyCode.C))
                edit = !edit;

            if (Input.GetKeyDown(KeyCode.O) && Input.GetKey(KeyCode.C))
            {
                bother = !bother;
                Utilities.instance.SetFreeCam();
            }

            if (Input.GetKey(KeyCode.LeftControl))
                Cursor.visible = true;

            if (!bother)
                return;

            Utilities.instance.DisableControlledCam();

            // if no subject, pan slightly #117
            if (subject == null)
            {
                foreach (Camera cam in FindObjectsOfType<Camera>())
                {
                    Vector3 currentRot = cam.transform.rotation.eulerAngles;

                    if (currentRot.y > 350)
                        currentRot.y = 0;
                    currentRot.y += 20;
                    Quaternion targetRotation = Quaternion.Euler(currentRot);
                    cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, targetRotation, 0.2f * Time.deltaTime);
                }
                return;
            }
            Cam bestCam = GetBestCamera();
            if (bestCam == null)
                foreach (Camera cam in FindObjectsOfType<Camera>()){
                    cam.transform.position = currentCam.loc; // set position of camera to current camera location
                    Quaternion targetRotation = Quaternion.LookRotation(subject.transform.position - cam.transform.position);
                    cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, targetRotation, 13 * Time.deltaTime);
                }
            Vector3 bestLoc = bestCam.loc;
            if (currentCam == null)
                currentCam = bestCam;
            if (bestCam != null && bestLoc != currentCam.loc && bestLoc != Vector3.zero)
            {
                currentCam = bestCam;
                shouldSnap = true;
            }
            if (currentCam.loc != Vector3.zero)
            {
                foreach(Camera cam in FindObjectsOfType<Camera>())
                {
                    cam.transform.position = currentCam.loc; // set position of camera to current camera location
                    if (shouldSnap)
                        cam.transform.LookAt(subject.transform.position);
                    else
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(subject.transform.position - cam.transform.position);
                        cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, targetRotation, 13 * Time.deltaTime);
                    }

                    float distanceToSubject = Vector3.Distance(cam.transform.position, subject.transform.position);
                    float someConstant = currentCam.zoomAmount;
                    // Calculate FOV based on distance (adjust this formula based on your needs)
                    float desiredFOV = Mathf.Atan(someConstant / distanceToSubject) * Mathf.Rad2Deg * 2f;
                    // Set the FOV within some defined range (e.g., between 20 and 100)
                    cam.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, desiredFOV, currentCam.panSpeed * Time.deltaTime);//Mathf.Clamp(desiredFOV, 20, 100);
                }
                shouldSnap = false;
            }
            Utilities.LogMethodCallEnd();
        }
        public bool IsValid(Vector3 camPos, int proximity = 100, float coneAngle = 45f)
        {
            Utilities.LogMethodCallStart();
            if (subject == null)
                return false;

            Vector3 directionToPlayer = subject.transform.position - camPos;

            // Use layer mask to filter colliders
            int layerMask = LayerMask.GetMask("Cyclist");

            // Check distance
            if (Vector3.Distance(subject.transform.position, camPos) > proximity)
                return false;

            // Overlap colliders with sphere using layer mask
            Collider[] colliders = Physics.OverlapSphere(camPos, proximity, layerMask);

            foreach (Collider collider in colliders)
            {
                // Check if the collider belongs to the subject or its children
                if (collider.gameObject.transform.root.gameObject == subject)
                {
                    // Check if the collider is within the cone of vision
                    Vector3 directionToCollider = collider.transform.position - camPos;
                    float angle = Vector3.Angle(directionToCollider, directionToPlayer);

                    if (angle < coneAngle)
                    {
                        RaycastHit hit;
                        if (Physics.Raycast(camPos, directionToCollider, out hit, proximity, layerMask))
                        {
                            if (hit.collider == collider)
                                return true;
                        }
                    }
                }
            }
            Utilities.LogMethodCallEnd();
            return false;
        }



        public void LoadFromFile()
        {
            string filePath = Path.Combine(Application.persistentDataPath, "cameras.txt");

            if (File.Exists(filePath))
            {
                cameras.Clear(); // Clear the existing data before loading from file

                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] values = line.Split(',');

                        if (values.Length == 7)
                        {
                            float x, y, z, proximity, panSpeed, zoomAmount;
                            bool shouldZoom;

                            if (float.TryParse(values[0], out x) &&
                                float.TryParse(values[1], out y) &&
                                float.TryParse(values[2], out z) &&
                                float.TryParse(values[3], out proximity) &&
                                float.TryParse(values[4], out panSpeed) &&
                                bool.TryParse(values[5], out shouldZoom) &&
                                float.TryParse(values[6], out zoomAmount))
                            {
                                Cam loadedCam = new Cam();
                                loadedCam.loc = new Vector3(x, y, z);
                                loadedCam.proximity = proximity;
                                loadedCam.panSpeed = panSpeed;
                                loadedCam.shouldZoom = shouldZoom;
                                loadedCam.zoomAmount = zoomAmount;
                                cameras.Add(loadedCam);
                            }
                            else
                            {
                                Debug.LogWarning("Failed to parse line: " + line);
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Invalid line format: " + line);
                        }
                    }
                }
                Debug.Log("Cameras loaded from file.");
            }
            else
            {
                Debug.LogWarning("File not found: " + filePath);
            }
        }

        public void SaveToFile()
        {
            // credit ChatGPT
            string filePath = Path.Combine(Application.persistentDataPath, "cameras.txt");

            // Ensure cameras has data to save
            if (cameras.Count > 0)
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    foreach (Cam cam in cameras)
                    {
                        string line = $"{cam.loc.x},{cam.loc.y},{cam.loc.z},{cam.proximity},{cam.panSpeed},{cam.shouldZoom},{cam.zoomAmount}";
                        writer.WriteLine(line);
                    }
                }
                Debug.Log("cameras saved to file.");
            }
            else { Debug.LogWarning("No cameras data to save."); }
        }

    }
}