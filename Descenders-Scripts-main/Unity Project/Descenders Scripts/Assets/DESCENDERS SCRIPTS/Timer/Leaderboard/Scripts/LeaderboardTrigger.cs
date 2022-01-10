using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;

namespace CustomLeaderboard{
    public class LeaderboardTrigger : ModBehaviour{

        public GameObject leaderboardHandler;

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.root.name == "Player_Human")
            {
                leaderboardHandler.GetComponent<Leaderboard>().RefreshLeaderboard();
            }
        }
    }
}
