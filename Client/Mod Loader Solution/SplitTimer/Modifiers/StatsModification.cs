using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ModLoaderSolution;

namespace SplitTimer
{
    public class Stat : ScriptableObject
    {
        public string Name;
        public Type type;
        public string ObfuscatedName;
        public string currentVal;
        public string prevVal;
        public Stat(string aName, Type aType, string aObfuscatedName, string aCurrentVal, string aPrevVal)
        {
            Name = aName;
            type = aType;
            ObfuscatedName = aObfuscatedName;
            currentVal = aCurrentVal;
            prevVal = aPrevVal;
        }
    }
    class StatsModification : MonoBehaviour
    {
        public List<Stat> stats = new List<Stat>();

        void Start()
        {
            stats.Add(new Stat("acceleration", typeof(float), "\u0084DUt\u0084vi", "1.3", "1.3"));
            stats.Add(new Stat("startupAcceleration", typeof(float), "cPkCE^\u0081", "1.5", "1.5"));
            stats.Add(new Stat("airFriction", typeof(float), "ei[frnu", "0.06", "0.06"));
            stats.Add(new Stat("rollResistance", typeof(float), "~Sz\u0082Mf[", "0.5", "0.5"));
            stats.Add(new Stat("maxSteering", typeof(float), "i[^xbRc", "5.0", "5.0"));
            stats.Add(new Stat("steeringTime", typeof(float), "\u0083gpT]rZ", "0.5", "0.5"));
            stats.Add(new Stat("steeringReductionForSpeed", typeof(float), "FeXucWp", "0.4", "0.4"));
            stats.Add(new Stat("minSpeedForSteerReduction", typeof(float), "~L|\u007fVHv", "2", "2"));
            stats.Add(new Stat("velocityLossInCorners", typeof(float), "EiKXn\u0080W", "1", "1"));
            stats.Add(new Stat("steeringShake", typeof(float), "eVPs~\u0082h", "", ""));
            stats.Add(new Stat("baseSlip", typeof(float), "gY\u007f\u0083VF\u0084", "25", "25"));
            stats.Add(new Stat("speedToLoseGrip", typeof(float), "Z`sU}qp", "2", "2"));
            stats.Add(new Stat("skiddingSlip", typeof(float), "[JmOrvR", "5", "5"));
            stats.Add(new Stat("speedForFullSkid", typeof(float), "\u0084Se{\u0084[Q", "10", "10"));
            stats.Add(new Stat("slipSmoothing", typeof(float), "yg^Qx|w", "5", "5"));
            stats.Add(new Stat("scaledFrictionFactor", typeof(float), "rKH}Gur", "0.5", "0.5"));
            stats.Add(new Stat("tweakingGripLoss", typeof(float), "z\u0082q|kdz", "0.5", "0.5"));
        }
        public void Update()
        {
            foreach (Stat stat in stats)
            {
                if (stat.currentVal != stat.prevVal)
                {
                    if (stat.type == typeof(float))
                        ModifyVehicle(stat.ObfuscatedName, float.Parse(stat.currentVal));
                    if (stat.type == typeof(int))
                        ModifyVehicle(stat.ObfuscatedName, int.Parse(stat.currentVal));
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
