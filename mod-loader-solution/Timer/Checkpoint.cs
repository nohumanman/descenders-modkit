using System;
using UnityEngine;
using ModLoaderSolution;

namespace ModLoaderSolution
{
    public class Checkpoint : MonoBehaviour
    {
        public CheckpointType checkpointType;
        public Trail trail;
        public bool doesWork = true;
        public void Start()
        {
            // Utilities.Log("Checkpoint | Checkpoint script added to " + this.gameObject.name);
        }
        void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.U))
                this.gameObject.GetComponent<MeshRenderer>().enabled = !this.gameObject.GetComponent<MeshRenderer>().enabled;
        }
        void OnTriggerEnter(Collider other)
        {
            // check if our other.transform.name is Bike so we're actually looking at the bike not arm or something
 
            Utilities.Log("SplitTimer.Checkpoint | " + DateTime.Now.ToString("MM.dd.yyy HH:mm:ss.fff") + "-" + other.transform.name + " - checkpoint '" + this.name + "' Entered");
            if (other.transform.name == "Bike" && other.transform.root.name == "Player_Human")
            {
                Utilities.Log("SplitTimer.Checkpoint | " + DateTime.Now.ToString("MM.dd.yyy HH:mm:ss.fff") + " - checkpoint '" + this.name + "' Entered");
                if (Utilities.instance.isInReplayMode())
                    return;
                if (!doesWork)
                    return;
                if (!Utilities.instance.isInReplayMode())
                    PlayerManagement.Instance.OnCheckpointEnter(trail.gameObject.name, checkpointType.ToString(), trail.checkpointList.Count, SplitTimerText.Instance.time.ToString());
                
                if (this.checkpointType == CheckpointType.Start)
                {
                    Utilities.instance.RestartReplay();
                    NetClient.Instance.SendData("START_SPEED|" + PlayerManagement.Instance.speed);
                    SplitTimerText.Instance.RestartTimer();
                    //NetClient.Instance.gameObject.GetComponent<Utilities>().SetVel(5);
                }
                else if (this.checkpointType == CheckpointType.Finish)
                {
                    Utilities.instance.SaveReplayToFile("tmp");
                    SplitTimerText.Instance.StopTimer();
                }
            }
        }
    }
    public enum CheckpointType
    {
        Start, Finish, Intermediate
    }
}
