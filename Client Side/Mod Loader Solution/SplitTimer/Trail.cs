using System;
using UnityEngine;
using System.Collections.Generic;


namespace SplitTimer
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
        public void AddScripts()
        {
            if (boundaries != null && startCheckpoint != null && endCheckpoint != null)
            {
                foreach (Transform boundary in boundaries.transform)
                {
                    GameObject boundaryObj = boundary.gameObject;
                    Debug.Log("Trail '" + this.name + "' | Adding boundary to " + boundaryObj.name);
                    Boundary boun = boundaryObj.AddComponent<Boundary>();
                    boun.trail = this;
                    boundaryList.Add(boundaryObj);
                }
                foreach (Transform checkpoint in startCheckpoint.transform.parent)
                {
                    GameObject checkpointObj = checkpoint.gameObject;
                    Debug.Log("Trail '" + this.name + "' | Adding checkpoint to " + checkpointObj.name);
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
            clientTime += Time.deltaTime;
        }
    }
}
