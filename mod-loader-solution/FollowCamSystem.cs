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
        public float panSpeed = 20; // the speed the camera follows the player rotationally
        public float proximity; // the range a camera can see
        public bool shouldZoom; // whether the camera should zoom to correct FOV
    }
    public class FollowCamSystem : MonoBehaviour
    {
        public GameObject subject;
        private List<Cam> cameras = new List<Cam>() { };
        private Cam currentCam;
        private bool shouldSnap = false;
        private bool bother = true;
        public void LoadFromFile()
        {
            // credit ChatGPT
            string filePath = Path.Combine(Application.persistentDataPath, "cameras.txt");

            // Check if the file exists before attempting to read from it
            if (File.Exists(filePath))
            {
                cameras.Clear(); // Clear existing cameras data before loading

                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] values = line.Split(',');
                        if (values.Length == 3)
                        {
                            float x, y, z;
                            if (float.TryParse(values[0], out x) && float.TryParse(values[1], out y) && float.TryParse(values[2], out z))
                            {
                                Cam cam = new Cam();
                                cam.loc = new Vector3(x, y, z);
                                cameras.Add(cam);
                            }
                            else
                            {
                                Debug.LogWarning("Failed to parse Vector3 from file.");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Invalid line format in file.");
                        }
                    }
                }

                Debug.Log("cameras loaded from file.");
            }
            else
            {
                Debug.LogWarning("File does not exist or couldn't be found.");
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
                        string line = $"{cam.loc.x},{cam.loc.y},{cam.loc.z},{cam.proximity},{cam.panSpeed},{cam.shouldZoom}";
                        writer.WriteLine(line);
                    }
                }

                Debug.Log("cameras saved to file.");
            }
            else
            {
                Debug.LogWarning("No cameras data to save.");
            }
        }
        public void OnGUI()
        {
            int yPos = 20;
            int i = 0;
            List<string> title = new List<string>() { "X", "Y", "Z", "PanSpeed", "ShouldZoom", "Proximity"};
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
            xPos = 20;
            if (GUI.Button(new Rect(xPos, 20 + yPos, 240, 20), "Toggle Follow Cam"))
                bother = !bother;
            yPos += 22;
            if (GUI.Button(new Rect(xPos, 20 + yPos, 240, 20), "Look at me"))
                Camera.main.transform.LookAt(Utilities.instance.GetPlayer().transform);
            yPos += 22;
            if (GUI.Button(new Rect(xPos, 20 + yPos, 240, 20), "Add Camera"))
                cameras.Add(new Cam());
            yPos += 22;
            if (GUI.Button(new Rect(xPos, 20 + yPos, 240, 20), "Save to cameras.txt"))
                SaveToFile();
            yPos += 22;
            if (GUI.Button(new Rect(xPos, 20 + yPos, 240, 20), "Load from cameras.txt"))
                LoadFromFile();
            GUIStyle myButtonStyle2 = new GUIStyle(GUI.skin.button);
            myButtonStyle2.normal.textColor = Color.white;
            myButtonStyle2.normal.background = UserInterface.MakeTex(5, 5, new Color(0.2f, 0.06f, 0.12f));
            myButtonStyle2.fontSize = 30;
            GUI.Label(new Rect(Screen.width - 300, Screen.height - 80, 300, 80), Camera.main.transform.position.ToString(), myButtonStyle2);
        }
        public Cam GetBestCamera()
        {
            float closest = Mathf.Infinity;
            Cam closestCam = new Cam();
            float switchThreshold = 2f;
            foreach (Cam cam in cameras)
            {
                Vector3 vector3 = cam.loc;
                if (IsValid(vector3))
                {
                    float distanceToCam = Vector3.Distance(vector3, subject.transform.position);
                    
                    if (distanceToCam < closest - switchThreshold) // if this is the closest to our player
                    {
                        closest = distanceToCam; // set as closest
                        closestCam = cam;
                    }
                }
            }
            if (closestCam.loc == Vector3.zero)
                return null;
            return closestCam;
        }
        public void Update()
        {
            if (!bother)
                return;
            subject = Utilities.instance.GetPlayer();
            try
            {
                subject = Utilities.instance.GetNetworkedPlayers()[0];
            } catch (Exception e) { }
            Utilities.instance.DisableControlledCam();

            Cam bestCam = GetBestCamera();
            Vector3 bestLoc = bestCam.loc;
            if (bestLoc != currentCam.loc && bestLoc != Vector3.zero)
            {
                currentCam.loc = bestLoc;
                shouldSnap = true;
            }

            if (currentCam.loc != Vector3.zero)
            {
                foreach(Camera cam in FindObjectsOfType<Camera>())
                {
                    // set position of camera to current camera location
                    cam.transform.position = currentCam.loc;
                    // temporarily just set the rotation to look at player
                    //cam.transform.LookAt(subject.transform);
                    if (shouldSnap)
                    {
                        cam.transform.LookAt(subject.transform.position);
                    }
                    else
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(subject.transform.position - cam.transform.position);
                        cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, targetRotation, 13 * Time.deltaTime);
                    }

                    float distanceToSubject = Vector3.Distance(cam.transform.position, subject.transform.position);
                    const float someConstant = 5f;
                    // Calculate FOV based on distance (adjust this formula based on your needs)
                    float desiredFOV = Mathf.Atan(someConstant / distanceToSubject) * Mathf.Rad2Deg * 2f;

                    // Set the FOV within some defined range (e.g., between 20 and 100)
                    cam.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, desiredFOV, 13 * Time.deltaTime);//Mathf.Clamp(desiredFOV, 20, 100);


                }
                shouldSnap = false;
            }
        }
        public bool IsValid(Vector3 camPos, int proximity = 100)
        {
            if (subject == null)
                return false;
            if (Vector3.Distance(subject.transform.position, camPos) > proximity)
                return false;
            // if player is in line of sight of camera
            Vector3 directionToPlayer = subject.transform.position - camPos;
            RaycastHit hit;
            if (Physics.Raycast(camPos, directionToPlayer, out hit, proximity))
                if (hit.collider.gameObject.transform.root.gameObject == subject)  // root of collider is player_human
                    return true; // player is in line of view
            return false;
        }
    }
}