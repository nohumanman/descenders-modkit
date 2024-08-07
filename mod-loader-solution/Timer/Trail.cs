using System;
using UnityEngine;
using System.Collections.Generic;
using ModLoaderSolution;
using System.IO;
using System.Collections;

namespace ModLoaderSolution
{
    public class Trail : MonoBehaviour
    {
        public GameObject boundaries;
        public GameObject startCheckpoint;
        public GameObject endCheckpoint;
        public GameObject leaderboardText;
        public GameObject autoLeaderboardText;
        public List<GameObject> boundaryList = new List<GameObject>();
        public List<GameObject> checkpointList = new List<GameObject>();
        public string url;
        public bool splitsAreCheckpoints = false;
        public float clientTime = 0f;
        public float lastBoundaryExit = -1f;
        public bool hidden = false;
        public void Start()
        {
            Utilities.LogMethodCallStart();
            Utilities.Log("Found Trail '" + name + "'");
            AddScripts();
            if (autoLeaderboardText != null)
                autoLeaderboardText.GetComponent<TextMesh>().text = "";
            Utilities.LogMethodCallEnd();
        }
        bool scriptsAdded = false;
        public void AddScripts()
        {
            Utilities.LogMethodCallStart();
            if (scriptsAdded) {
                Debug.Log("Scripts already added");
                return;
            }
            if (boundaries != null && startCheckpoint != null && endCheckpoint != null && !scriptsAdded)
            {
                scriptsAdded = true;
                foreach (Transform boundary in boundaries.transform)
                {
                    GameObject boundaryObj = boundary.gameObject;
                    Boundary boun = boundaryObj.AddComponent<Boundary>();
                    boun.trail = this;
                    boundaryList.Add(boundaryObj);
                }
                Utilities.Log("'" + this.name + "' has " + boundaryList.Count.ToString() + " boundaries.");
                foreach (Transform checkpoint in startCheckpoint.transform.parent)
                {
                    GameObject checkpointObj = checkpoint.gameObject;
                    if (checkpointObj.GetComponent<Checkpoint>() != null)
                        Debug.LogError("ALREADY HAS CHECKPOINT!");
                    else
                    {
                        Checkpoint check = checkpointObj.AddComponent<Checkpoint>();
                        check.trail = this;
                        if (checkpointObj == startCheckpoint)
                            check.checkpointType = CheckpointType.Start;
                        else if (checkpointObj == endCheckpoint)
                            check.checkpointType = CheckpointType.Finish;
                        else
                  
                  
                  
                  check.checkpointType = CheckpointType.Intermediate;
                        checkpointList.Add(checkpointObj);
                    }
                }
            }
            else
            {
                Debug.LogError("AddScripts not possible!");
            }
            Utilities.LogMethodCallEnd();
        }
        public void Update()
        {
            Utilities.LogMethodCallStart();
            if (Utilities.instance.isInReplayMode())
                return;
            // if Time.time - lastBoundryExit is greater than 15
            if ((Time.time - lastBoundaryExit) > 15 && lastBoundaryExit != -1)
            {
                if (SplitTimerText.Instance.currentTrail == this)
                {
                    SplitTimerText.Instance.hidden = true;
                }
            }
            // if select pressed, blow things up
            bool usingXbox = false;
            foreach (string name in Input.GetJoystickNames())
                if (name == "Controller (Xbox One For Windows)")
                    usingXbox = true;
            if (Input.GetKeyDown("joystick button 6") && usingXbox)
            {
                SplitTimerText.Instance.count = false;
                SplitTimerText.Instance.SetText("Restarted Fully");
                SplitTimerText.Instance.text.color = Color.red;
                StartCoroutine(SplitTimerText.Instance.DisableTimerText(5));
            }
            clientTime += Time.deltaTime;
            Utilities.LogMethodCallEnd();
        }
        public bool InAnyBoundaries()
        {
            if (boundaryList.Count == 0)
                return true;
            foreach(GameObject bound in boundaryList)
                if (bound.GetComponent<Boundary>().inBoundary)
                    return true;
            return false;
        }
        public void LoadFromUrl(string csvUrl)
        {
            this.url = csvUrl;
            StartCoroutine(LoadFromUrlCoro(csvUrl));
        }
        public void LoadFromCSV(string csvDir)
        {
            if (boundaries != null || startCheckpoint != null || endCheckpoint != null)
                return;
            // read csv to string
            if (!File.Exists(csvDir))
                return;
            string textContents = File.ReadAllText(csvDir);
            LoadFromTxt(textContents);
        }
        IEnumerator LoadFromUrlCoro(string url)
        {
            WWW w = new WWW(url);
            yield return w;
            LoadFromTxt(w.text);
        }
        /*
         * Destroys every checkpoint in the scene. When ignoreSplits is true, it will
         * destroy checkpoints even if they're splits of another trail.
        */
        public void DestroyCheckpoints(bool ignoreSplits = false)
        {
            foreach(GameObject cp in GameObject.FindGameObjectsWithTag("Checkpoint"))
            {
                // if checkpoint is not of a trail, destroy it.
                if (ignoreSplits || cp.GetComponent<ModLoaderSolution.Checkpoint>() == null)
                    Destroy(cp);
            }
        }
        /*
         * This method isn't how I'd like to do it (I'd want to use YAML), but
         * given we're compiling into a game without our own dependencies, we
         * can't really afford to be picky. If it works it works.
        */
        public void LoadFromTxt(string textContents)
        {
            string[] lines = textContents.Split('\n');
            List<string[]> csvContents = new List<string[]>();
            boundaries = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            boundaries.name = "Boundaries";
            boundaries.transform.SetParent(this.transform);
            // make a huge boundary
            /*GameObject bigBoundary = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bigBoundary.transform.localScale = new Vector3(100000, 100000, 100000);
            bigBoundary.transform.SetParent(boundaries.transform);
            bigBoundary.GetComponent<MeshRenderer>().enabled = false;
            bigBoundary.GetComponent<BoxCollider>().isTrigger = true;*/
            GameObject checkpoints = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            checkpoints.name = "Checkpoints";
            checkpoints.transform.SetParent(transform);
            foreach (string line in lines)
                csvContents.Add(line.Split(','));
            // bam, loaded, so now read it into ourself
            foreach (string[] line in csvContents)
            {
                if (line.Length == 2)
                {
                    line[1] = line[1].Replace("\n", "");
                }
                if (line[0] == "trail_name")
                {
                    // check if the same trail_name exists
                    string trail_name = line[1].Replace("\n", "");
                    this.gameObject.name = trail_name;
                    this.name = trail_name;
                    foreach (Trail tr in FindObjectsOfType<Trail>())
                        if (tr.name == this.gameObject.name && tr != this)
                            Destroy(this.gameObject);
                }
                else if (line[0] == "splitsAreCheckpoints")
                {
                    if (line[1] == "true"){
                        splitsAreCheckpoints = true;
                    }
                }
                else if (line[0] == "murderOtherSplits") {
                    if (line[1] == "true")
                    {
                        DestroyCheckpoints();
                    }
                }
                else if (line[0].StartsWith("CP"))
                {
                    // instantiate new checkpoint
                    GameObject CP = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    CP.transform.position = new Vector3(float.Parse(line[1]), float.Parse(line[2]), float.Parse(line[3]));
                    CP.name = line[0];
                    CP.transform.SetPositionAndRotation(
                        new Vector3(float.Parse(line[1]), float.Parse(line[2]), float.Parse(line[3])),
                        Quaternion.Euler(float.Parse(line[4]), float.Parse(line[5]), float.Parse(line[6]))
                    );
                    CP.transform.localScale = new Vector3(float.Parse(line[7]), float.Parse(line[8]), float.Parse(line[9]));
                    CP.GetComponent<BoxCollider>().isTrigger = true;

                    CP.transform.SetParent(checkpoints.transform, true);
                    if (line[0].StartsWith("CPS"))
                    {
                        startCheckpoint = CP;
                    }
                    if (line[0].StartsWith("CPE"))
                    {
                        endCheckpoint = CP;
                    }
                }
            }
            AddScripts();
        }
    }
}
