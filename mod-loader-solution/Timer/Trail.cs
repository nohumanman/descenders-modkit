using System;
using UnityEngine;
using System.Collections.Generic;
using ModLoaderSolution;

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
        public float clientTime = 0f;
        public void Start()
        {
            Utilities.Log("Found Trail '" + name + "'");
            AddScripts();
            autoLeaderboardText.GetComponent<TextMesh>().text = "Not connected to server.";
        }
        public void AddScripts()
        {
            if (boundaries != null && startCheckpoint != null && endCheckpoint != null)
            {
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
        public void Update()
        {
            if (Utilities.instance.isInReplayMode())
                return;
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
        }
        bool InAllBoundaries()
        {
            foreach(GameObject bound in boundaryList)
                if (bound.GetComponent<Boundary>().inBoundary)
                    return false;
            return true;
        }
    }
}
