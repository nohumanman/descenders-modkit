using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ModLoaderSolution;


namespace SplitTimer
{
    struct StatStruct
    {
        public string Name;
    }
    [Serializable]
    public class Stat
    {
        public string Name;
        public Type type;
        public string ObfuscatedName;
        public string currentVal;
        public string prevVal;
        public string StartingValue;
        [SerializeField]
        public Stat(string aName, Type aType, string aObfuscatedName)
        {
            Name = aName;
            type = aType;
            ObfuscatedName = aObfuscatedName;
        }
    }
    public struct deez
    {
        public string nuts;
    }
    class StatsModification : MonoBehaviour
    {
        public bool permitted = true;
        public static StatsModification instance;
        string savePath = Environment.CurrentDirectory;
        public List<Stat> stats = new List<Stat>();
        void Start()
        {
            instance = this;
            stats.Add(new Stat("acceleration", typeof(float), "\u0084DUt\u0084vi"));
            stats.Add(new Stat("startupAcceleration", typeof(float), "cPkCE^\u0081"));
            stats.Add(new Stat("airFriction", typeof(float), "ei[frnu"));
            stats.Add(new Stat("rollResistance", typeof(float), "~Sz\u0082Mf["));
            stats.Add(new Stat("maxSteering", typeof(float), "i[^xbRc"));
            stats.Add(new Stat("steeringTime", typeof(float), "\u0083gpT]rZ"));
            stats.Add(new Stat("steeringReductionForSpeed", typeof(float), "FeXucWp"));
            stats.Add(new Stat("minSpeedForSteerReduction", typeof(float), "~L|\u007fVHv"));
            stats.Add(new Stat("velocityLossInCorners", typeof(float), "EiKXn\u0080W"));
            stats.Add(new Stat("steeringShake", typeof(float), "eVPs~\u0082h"));
            stats.Add(new Stat("baseSlip", typeof(float), "gY\u007f\u0083VF\u0084"));
            stats.Add(new Stat("speedToLoseGrip", typeof(float), "Z`sU}qp"));
            stats.Add(new Stat("skiddingSlip", typeof(float), "[JmOrvR"));
            stats.Add(new Stat("speedForFullSkid", typeof(float), "\u0084Se{\u0084[Q"));
            stats.Add(new Stat("slipSmoothing", typeof(float), "yg^Qx|w"));
            stats.Add(new Stat("scaledFrictionFactor", typeof(float), "rKH}Gur"));
            stats.Add(new Stat("tweakingGripLoss", typeof(float), "z\u0082q|kdz"));
        }
        [Serializable]
        public class This
        {
            public string[] obfuscatedNames;
            public string[] currentVals;
        }
        public bool IfStatsAreDefault()
        {
            foreach (Stat stat in stats)
                if (stat.currentVal != stat.StartingValue)
                    return false;
            return true;
        }
        public void SaveStats()
        {
            This x = new This();
            x.obfuscatedNames = new string[stats.Count];
            x.currentVals = new string[stats.Count];
            int i = 0;
            foreach (Stat stat in stats)
            {
                x.obfuscatedNames[i] = stat.ObfuscatedName;
                x.currentVals[i] = stat.currentVal;
                i++;
            }
            Debug.Log("StatsModification | Saving stats to '" + savePath + "'");
            System.IO.File.WriteAllText(savePath + "\\SavedStats.json", JsonUtility.ToJson(x, true));
        }
        public void LoadStats()
        {
            try
            {
                string textToLoad = System.IO.File.ReadAllText(savePath + "\\SavedStats.json");
                This x = (This)JsonUtility.FromJson(textToLoad, typeof(This));
                foreach(Stat stat in stats)
                {
                    int i = 0;
                    foreach(string val in x.obfuscatedNames)
                    {
                        if (val == stat.ObfuscatedName)
                            stat.currentVal = x.currentVals[i];
                        i++;
                    }
                }
            }
            catch {
                Debug.Log("StatsModification | LoadStats() Failed! Maybe no file is present?");
                return;
            }
        }
        public void ResetStats()
        {
            foreach (Stat stat in stats)
            {
                stat.currentVal = stat.StartingValue;
            }
        }
        public void Update()
        {
            foreach (Stat stat in stats)
            {
                if (stat.currentVal == null && stat.prevVal == null)
                {
                    try
                    {
                        stat.currentVal = GetVehicleMod(stat.ObfuscatedName).ToString();
                        stat.StartingValue = stat.currentVal;
                        stat.prevVal = stat.currentVal;
                    } catch { }
                }
                if (stat.currentVal != stat.prevVal)
                {
                    //if (stat.type == typeof(float))
                        ModifyVehicle(stat.ObfuscatedName, float.Parse(stat.currentVal));
                    //if (stat.type == typeof(int))
                    //    ModifyVehicle(stat.ObfuscatedName, int.Parse(stat.currentVal));
                    stat.prevVal = stat.currentVal;
                }
            }
        }
        public void ModifyVehicle(string field, object value)
        {
            Utilities.instance.GetPlayer()
                .GetComponent<Vehicle>()
                .GetType().GetField(field)
                .SetValue(
                    Utilities.instance.GetPlayer().GetComponent<Vehicle>(), value
                );
        }
        public object GetVehicleMod(string field)
        {
            return Utilities.instance.GetPlayer()
                .GetComponent<Vehicle>()
                .GetType().GetField(field).GetValue(
                Utilities.instance.GetPlayer().GetComponent<Vehicle>()
            );
        }
        public void ApplyStupidModifiers()
        {
            GameModifier[] gameModifiers = (GameModifier[])typeof(GameData).GetField("\u0081jU\u0080h\u0084c").GetValue(FindObjectOfType<GameData>());
            foreach (GameModifier mod in gameModifiers)
            {
                Debug.Log(mod.name);
                Debug.Log(mod.modifiers[0].percentageValue);
                if (mod.name == "LANDINGIMPACT" || mod.name == "BUNNYHOP" || mod.name == "PUMPSTRENGTH")
                    mod.modifiers[0].percentageValue = 50000000;
                //if (mod.name == "FAKIEBALANCE")
                //    mod.modifiers[0].percentageValue = -50000;
                PlayerInfoImpact pi = Utilities.instance.GetPlayerInfoImpact();
                pi.AddGameModifier(mod);
            }
        }
    }
}
