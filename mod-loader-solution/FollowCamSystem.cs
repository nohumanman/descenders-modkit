using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ModLoaderSolution;
using System;
using System.IO;


namespace ModLoaderSolution
{
    public class FollowCamSystem : MonoBehaviour
    {
        public GameObject subject;
        public List<Vector3> camLocations = new List<Vector3>() { };
        public Vector3 currentCamLoc = Vector3.zero;
        public void LoadFromFile()
        {
            // credit ChatGPT
            string filePath = Path.Combine(Application.persistentDataPath, "CamLocations.txt");

            // Check if the file exists before attempting to read from it
            if (File.Exists(filePath))
            {
                camLocations.Clear(); // Clear existing camLocations data before loading

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
                                Vector3 loadedVector = new Vector3(x, y, z);
                                camLocations.Add(loadedVector);
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

                Debug.Log("CamLocations loaded from file.");
            }
            else
            {
                Debug.LogWarning("File does not exist or couldn't be found.");
            }
        }

        public void SaveToFile()
        {
            // credit ChatGPT
            string filePath = Path.Combine(Application.persistentDataPath, "CamLocations.txt");

            // Ensure camLocations has data to save
            if (camLocations.Count > 0)
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    foreach (Vector3 location in camLocations)
                    {
                        string line = $"{location.x},{location.y},{location.z}";
                        writer.WriteLine(line);
                    }
                }

                Debug.Log("CamLocations saved to file.");
            }
            else
            {
                Debug.LogWarning("No camLocations data to save.");
            }
        }

        public void OnGUI()
        {
            int pos = 0;
            int i = 0;
            foreach (Vector3 vector3 in camLocations)
            {
                string x = GUI.TextField(new Rect(20, 20+pos, 60, 20), vector3.x.ToString());
                string y = GUI.TextField(new Rect(20+60, 20 + pos, 60, 20), vector3.y.ToString());
                string z = GUI.TextField(new Rect(20+60+60, 20 + pos, 60, 20), vector3.z.ToString());
                if (GUI.Button(new Rect(20 + 60 + 60 + 60, 20 + pos, 60, 20), "SET"))
                    camLocations[i] = Camera.main.transform.position;
                else
                    camLocations[i] = new Vector3(float.Parse(x), float.Parse(y), float.Parse(z));
                if (GUI.Button(new Rect(20 + 60 + 60 + 60 + 60, 20 + pos, 60, 20), "GOTO"))
                    Camera.main.transform.position = vector3;
                if (GUI.Button(new Rect(20 + 60 + 60 + 60 + 60 + 60, 20 + pos, 60, 20), "DEL"))
                    camLocations.RemoveAt(i);
                pos += 22;
                i++;
            }
            if (GUI.Button(new Rect(20 + 60 + 60 + 60, 20 + pos, 240, 20), "Toggle Follow Cam"))
                bother = !bother;
            pos += 22;
            if (GUI.Button(new Rect(20 + 60 + 60 + 60, 20 + pos, 240, 20), "Look at me"))
                Camera.main.transform.LookAt(Utilities.instance.GetPlayer().transform);
            pos += 22;
            if (GUI.Button(new Rect(20 + 60 + 60 + 60, 20 + pos, 240, 20), "Add Camera"))
                camLocations.Add(Vector3.zero);
            pos += 22;
            if (GUI.Button(new Rect(20 + 60 + 60 + 60, 20 + pos, 240, 20), "Save to CamLocations.txt"))
                SaveToFile();
            if (GUI.Button(new Rect(20 + 60 + 60 + 60, 20 + pos, 240, 20), "Load from CamLocations.txt"))
                LoadFromFile();
            GUIStyle myButtonStyle2 = new GUIStyle(GUI.skin.button);
            myButtonStyle2.normal.textColor = Color.white;
            myButtonStyle2.normal.background = UserInterface.MakeTex(5, 5, new Color(0.2f, 0.06f, 0.12f));
            myButtonStyle2.fontSize = 30;
            GUI.Label(new Rect(Screen.width - 300, Screen.height - 80, 300, 80), Camera.main.transform.position.ToString(), myButtonStyle2);
        }
        bool shouldSnap = false;
        bool bother = true;
        public Vector3 GetBestCamera()
        {
            float closest = Mathf.Infinity;
            Vector3 closestCam = Vector3.zero;
            foreach (Vector3 vector3 in camLocations)
                if (IsValid(vector3))
                {
                    float distanceToCam = Vector3.Distance(vector3, subject.transform.position);
                    if (distanceToCam < closest) // if this is the closest to our player
                    {
                        closest = distanceToCam; // set as closest
                        closestCam = vector3;
                    }
                }
            return closestCam;
        }
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                Utilities.instance.GetNetworkedPlayers();
                foreach(Vector3 v in camLocations)
                {
                    GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    GameObject objCenter = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    MeshRenderer r = obj.GetComponent<MeshRenderer>();
                    obj.GetComponent<SphereCollider>().enabled = false;
                    Material mat = r.material;
                    mat.SetFloat("_Mode", 2);
                    Color color = Color.blue;
                    color.a = 0.1f;
                    mat.color = color;

                    // shit from the google
                    // Update material properties for changes to take effect
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.DisableKeyword("_ALPHATEST_ON");
                    mat.EnableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

                    objCenter.transform.position = v;

                    obj.transform.position = v;
                    obj.transform.localScale = Vector3.one * 100;
                }
            }
            if (!bother)
                return;
            subject = Utilities.instance.GetPlayer();
            subject = Utilities.instance.GetNetworkedPlayers()[0];
            Utilities.instance.DisableControlledCam();

            Vector3 bestLoc = GetBestCamera();
            if (bestLoc != currentCamLoc && bestLoc != Vector3.zero)
            {
                currentCamLoc = bestLoc;
                shouldSnap = true;
            }

            if (currentCamLoc != Vector3.zero)
            {
                foreach(Camera cam in FindObjectsOfType<Camera>())
                {
                    // set position of camera to current camera location
                    cam.transform.position = currentCamLoc;
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
                    cam.fieldOfView = 20; // TEMPORARY TO TEST
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