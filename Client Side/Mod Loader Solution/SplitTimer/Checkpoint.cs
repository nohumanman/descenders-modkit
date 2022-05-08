using System;
using UnityEngine;


namespace SplitTimer
{
    public class Checkpoint : MonoBehaviour
    {
        public CheckpointType checkpointType;
        public Trail trail;
        public void Start()
        {
            Debug.Log("Checkpoint script added to " + this.gameObject.name);
        }
        void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.U))
                this.gameObject.GetComponent<MeshRenderer>().enabled = !this.gameObject.GetComponent<MeshRenderer>().enabled;
        }
        void OnTriggerEnter(Collider other)
        {
            if (other.transform.name == "Bike" && other.transform.root.name == "Player_Human")
            {
                PlayerInfo.Instance.OnCheckpointEnter(trail.gameObject.name, checkpointType.ToString(), trail.checkpointList.Count);
                if (this.checkpointType == CheckpointType.Start)
                {
                    TimerText.Instance.RestartTimer();
                }
                if (this.checkpointType == CheckpointType.Finish)
                {
                    TimerText.Instance.StopTimer();
                }
            }
        }
    }
    public enum CheckpointType
    {
        Start, Finish, Intermediate
    }
}
