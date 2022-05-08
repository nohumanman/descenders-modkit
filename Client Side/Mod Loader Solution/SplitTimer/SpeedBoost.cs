using System;
using UnityEngine;


namespace SplitTimer
{
    public class SpeedBoost : MonoBehaviour
    {
        GameObject Player;
        public bool speedEnabled = false;
        public float speedMultiplier = 5f;
        public void FixedUpdate()
        {
            if (speedEnabled)
            {
                if (Player == null)
                    Player = GameObject.Find("Player_Human");
                if (Player != null)
                {
                    Debug.Log(speedMultiplier);
                    Player.transform.root.GetComponent<Rigidbody>().velocity *= 1 + (Time.deltaTime * speedMultiplier);
                }
            }
        }
    }
}
