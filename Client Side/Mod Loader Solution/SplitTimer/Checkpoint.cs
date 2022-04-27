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
        void OnTriggerEnter(Collider other)
        {
            if (other.transform.name == "Bike" && other.transform.root.name == "Player_Human")
            {
                PlayerInfo.Instance.OnCheckpointEnter(trail.gameObject.name, checkpointType.ToString(), trail.checkpointList.Count, trail.clientTime);
            }
        }
    }
    public enum CheckpointType
    {
        Start, Finish, Intermediate
    }
}
