using System;
using UnityEngine;
using System.Collections.Generic;


namespace SplitTimer
{
    [System.Serializable]
    public class LeaderboardInfo
    {
        public string[] name;
        public int[] place;
        public float[] time;
        public string LeaderboardAsString()
        {
            if (name == null || name.Length == 0)
                return "";
            string leaderboardString = "";
            for (int i = 0; i < name.Length && i < 10; i++)
            {
                leaderboardString += place[i] + ". " + name[i] + " - " + FormatTime(time[i]) + "\n";
            }
            return leaderboardString;
        }
        private string FormatTime(float time)
        {
            int intTime = (int)time;
            int minutes = intTime / 60;
            int seconds = intTime % 60;
            float fraction = time * 1000;
            fraction = (fraction % 1000);
            string timeText = System.String.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, fraction);
            return timeText;
        }
    }
}
