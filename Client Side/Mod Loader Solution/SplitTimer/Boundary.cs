using System;
using UnityEngine;


namespace SplitTimer
{
    public class Boundary : MonoBehaviour
    {
        public Trail trail;
        public void Start()
        {
            Debug.Log("Boundary added to " + this.gameObject.name);
        }
        void OnTriggerEnter(Collider other)
        {
            if (other.transform.name == "Bike" && other.transform.root.name == "Player_Human")
            {
                PlayerInfo.Instance.OnBoundryEnter(
                    trail.name,
                    this.gameObject.GetHashCode().ToString()
                );
            }
        }
        void OnTriggerExit(Collider other)
        {
            if (other.transform.name == "Bike" && other.transform.root.name == "Player_Human")
            {
                PlayerInfo.Instance.OnBoundryExit(
                    trail.name,
                    this.gameObject.GetHashCode().ToString()
                );
            }
        }
    }
}
