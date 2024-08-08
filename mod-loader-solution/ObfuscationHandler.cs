using System;
using UnityEngine;
using System.Collections.Generic;
using ModLoaderSolution;
using System.IO;
using System.Collections;
using static MeshCombineStudio.MeshCombiner;

namespace ModLoaderSolution
{
    public static class ObfuscationHandler
    {
        public static bool gameIsObfuscated = false;
        readonly static Dictionary<string, string> obfuscatedVals = new Dictionary<string, string>()
        {
            { "label_titleText", "f`r}tXQ" },
            { "label_bodyText", "oZtLHbT" },
            { "bikeType", "dzQf\u0082nw" },
            { "bikeTypes", "bx}n\u0080PQ" },
            { "playerCustomization", "hUP\u007fi\u0084d" },
            { "playerName", "a^sXf\u0083Y" },
            { "userID", "r~x\u007fs{n" },
            { "currentSession", "\u0083ESVMoz" },
            { "worldMap", "WnRr]U`" },
            { "replay", "Ym}\u0084upr" },
            { "Replay", "l\u0080KRMtV" },
            { "SaveReplay", "I\u0083tz]jk" },
            { "presence", "\u0084mfo\u007fzP" },
            { "gestures", "EL\u0080\u007f\u0084\u0080o" }
        };
        static bool everChecked = false;
        static bool isObfuscated = false;
        public static bool IsGameObfuscated()
        {
            if (!everChecked)
                isObfuscated = typeof(UI_PopUp_TextBoxSmall).GetField("f`r}tXQ") != null;
            return isObfuscated;
        }
        public static bool hasNotified = false;
        public static string GetObfuscated(string name)
        {
            // if the game is obfuscated return the obfuscated val
            if (IsGameObfuscated())
                return obfuscatedVals[name];
            // otherwise pog we can return the name we got given
            if (!hasNotified)
            {
                Debug.LogWarning("WARNING! THIS BUILD OF DESCENDERS IS NOT OBFUSCATED!");
                hasNotified = true;
            }
            return name;
        }
    }
}