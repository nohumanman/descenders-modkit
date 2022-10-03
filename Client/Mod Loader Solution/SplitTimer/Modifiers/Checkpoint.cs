using System;
using UnityEngine;
using ModLoaderSolution;

namespace SplitTimer
{
    public class Checkpoint : MonoBehaviour
    {
        public CheckpointType checkpointType;
        public Trail trail;
        public bool doesWork = true;
        public void Start()
        {
            // Debug.Log("Checkpoint | Checkpoint script added to " + this.gameObject.name);
        }
        void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.U))
                this.gameObject.GetComponent<MeshRenderer>().enabled = !this.gameObject.GetComponent<MeshRenderer>().enabled;
        }
        void OnTriggerEnter(Collider other)
        {
            if (other.transform.name == "Bike" && other.transform.root.name == "Player_Human" && doesWork)
            {
                PlayerInfo.Instance.OnCheckpointEnter(trail.gameObject.name, checkpointType.ToString(), trail.checkpointList.Count, SplitTimerText.Instance.time.ToString());
                if (this.checkpointType == CheckpointType.Start)
                {
                    NetClient.Instance.SendData("START_SPEED|" + PlayerInfo.Instance.speed);
                    SplitTimerText.Instance.RestartTimer();
                    //NetClient.Instance.gameObject.GetComponent<Utilities>().SetVel(5);
                }
                else if (this.checkpointType == CheckpointType.Finish)
                {
                    foreach(AnimateOnTrailEnd x in FindObjectsOfType<AnimateOnTrailEnd>())
                    {
                        if (x.trailName == trail.name)
                            x.TrailEnd();
                    }
                    // SplitTimerText.Instance.StopTimer();
                }
            }
        }
    }
    public enum CheckpointType
    {
        Start, Finish, Intermediate
    }
}
