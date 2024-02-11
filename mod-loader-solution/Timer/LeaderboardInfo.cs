using System;
using UnityEngine;
using System.Collections.Generic;

namespace ModLoaderSolution
{
    [System.Serializable]
    public class LeaderboardInfo
    {
        public string[] name;
        public int[] place;
        public float[] time;
        public float[] pen;
        public string[] verified;
        int nameMaxLen = 15;
        string MakeLengthOf(string text, int lengthAim)
        {
            if (text.Length < lengthAim)
            {
                text += " ";
                return MakeLengthOf(text, lengthAim);
            }
            return text;
        }
        string TruncateText(string text, int maxLen)
        {
            if (text.Length > maxLen)
                return text.Substring(0, maxLen-3) + "...";
            return text;
        }
        public string LeaderboardAsString()
        {
            if (pen == null)
            {
                pen = new float[name.Length];
                verified = new string[name.Length];
            }
            if (name == null || name.Length == 0)
                return "";
            string leaderboardString = "";
            int maxNameLength = 0;
            for (int i = 0; i < name.Length && i < 10; i++)
                if (name[i].Length > maxNameLength)
                    maxNameLength = name[i].Length;
            if (maxNameLength > nameMaxLen)
                maxNameLength = nameMaxLen;
            for (int i = 0; i < name.Length && i < 10; i++)
            {
                string placeNum = place[i].ToString();
                if (placeNum.Length == 1)
                    placeNum = " " + placeNum;
                leaderboardString += placeNum + ". " + MakeLengthOf(TruncateText(name[i], nameMaxLen), maxNameLength) + " - "+ FormatTime(time[i]) + " \n";
            }
            //Utilities.Log("'" + leaderboardString + "'");
            return leaderboardString;
        }
        private string FormatTime(float time)
        {
            int intTime = (int)time;
            int minutes = intTime / 60;
            int seconds = intTime % 60;
            float fraction = time * 1000;
            fraction = (fraction % 1000);
            if (fraction == 1000)
            {
                fraction = 0;
                seconds += 1;
            }
            string timeText = System.String.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, fraction);
            return timeText;
        }
    }
}
