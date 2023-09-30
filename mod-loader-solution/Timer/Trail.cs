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
            Debug.Log("ModLoaderSolution.Trail | Found Trail '" + name + "'");
            AddScripts();
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
                Debug.Log("ModLoaderSolution.Trail | '" + this.name + "' has " + boundaryList.Count.ToString() + " boundaries.");
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
        int framesSinceBoundaryCheck;
        public void Update()
        {
            if (Utilities.instance.isInReplayMode())
                return;
            clientTime += Time.deltaTime;
            framesSinceBoundaryCheck += 1;
            if (framesSinceBoundaryCheck > 20){
                framesSinceBoundaryCheck = 0;
                //if (InAllBoundaries())
               //     
                //);
            }
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
