using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ModLoaderSolution;


namespace ModLoaderSolution
{
    public enum StatType
    {
        Vehicle, Wheel, Modifier
    }
    [Serializable]
    public class Stat
    {
        public string Name;
        public Type type;
        public StatType statType;
        public string ObfuscatedName;
        public string currentVal;
        public string prevVal;
        public string StartingValue;
        public int wheelNo;
        [SerializeField]
        public Stat(string aName, Type aType, string aObfuscatedName, StatType _statType, int wheelNum = -1)
        {
            Name = aName;
            type = aType;
            ObfuscatedName = aObfuscatedName;
            statType = _statType;
            wheelNo = wheelNum;
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
        string[] modifiers = new string[] { "BUNNYHOP", "FAKIEBALANCE", "HEAVYBAILTHRESHOLD", "LANDINGIMPACT", "AIRCORRECTION", "OFFROADFRICTION", "PUMPSTRENGTH", "SPEEDWOBBLES", "SPINSPEED", "TWEAKSPEED", "WHEELIEBALANCE" };
        void Start()
        {
            instance = this;
            stats.Add(new Stat("acceleration", typeof(float), "\u0084DUt\u0084vi", StatType.Vehicle));
            stats.Add(new Stat("startupAcceleration", typeof(float), "cPkCE^\u0081", StatType.Vehicle));
            stats.Add(new Stat("airFriction", typeof(float), "ei[frnu", StatType.Vehicle));
            stats.Add(new Stat("rollResistance", typeof(float), "~Sz\u0082Mf[", StatType.Vehicle));
            stats.Add(new Stat("maxSteering", typeof(float), "i[^xbRc", StatType.Vehicle));
            stats.Add(new Stat("steeringTime", typeof(float), "\u0083gpT]rZ", StatType.Vehicle));
            stats.Add(new Stat("steeringReductionForSpeed", typeof(float), "FeXucWp", StatType.Vehicle));
            stats.Add(new Stat("minSpeedForSteerReduction", typeof(float), "~L|\u007fVHv", StatType.Vehicle));
            stats.Add(new Stat("velocityLossInCorners", typeof(float), "EiKXn\u0080W", StatType.Vehicle));
            stats.Add(new Stat("steeringShake", typeof(float), "eVPs~\u0082h", StatType.Vehicle));
            stats.Add(new Stat("baseSlip", typeof(float), "gY\u007f\u0083VF\u0084", StatType.Vehicle));
            stats.Add(new Stat("speedToLoseGrip", typeof(float), "Z`sU}qp", StatType.Vehicle));
            stats.Add(new Stat("skiddingSlip", typeof(float), "[JmOrvR", StatType.Vehicle));
            stats.Add(new Stat("speedForFullSkid", typeof(float), "\u0084Se{\u0084[Q", StatType.Vehicle));
            stats.Add(new Stat("slipSmoothing", typeof(float), "yg^Qx|w", StatType.Vehicle));
            stats.Add(new Stat("scaledFrictionFactor", typeof(float), "rKH}Gur", StatType.Vehicle));
            stats.Add(new Stat("tweakingGripLoss", typeof(float), "z\u0082q|kdz", StatType.Vehicle));

            foreach(string modifier in modifiers)
                stats.Add(new Stat(modifier, typeof(float), modifier, StatType.Modifier));
            
            // wheels
            //stats.Add(new Stat("xL{gJGT", typeof(float), "xL{gJGT", StatType.Wheel, 0)); // default 0.5
            //stats.Add(new Stat("p~mkyX{", typeof(float), "p~mkyX{", StatType.Wheel, 0)); // default 50
            //stats.Add(new Stat("YrKDSPL", typeof(float), "YrKDSPL", StatType.Wheel, 0)); // default 5
            //stats.Add(new Stat("HqsqNkJ", typeof(float), "HqsqNkJ", StatType.Wheel, 0)); // default 0.5
            //stats.Add(new Stat("[z\u0082FoKi", typeof(float), "[z\u0082FoKi", StatType.Wheel, 0)); // default 0.1
            //stats.Add(new Stat("kISVipu", typeof(bool), "kISVipu", StatType.Wheel, 0)); // default true
            //stats.Add(new Stat("L\u0084fpN}[", typeof(bool), "L\u0084fpN}[", StatType.Wheel, 0)); // default true
            //stats.Add(new Stat("EI`qFti", typeof(bool), "EI`qFti", StatType.Wheel, 0)); // default true
            //stats.Add(new Stat("^X`UpQn", typeof(bool), "^X`UpQn", StatType.Wheel, 0)); // default true
            //stats.Add(new Stat("H\u007fywOFs", typeof(float), "H\u007fywOFs", StatType.Wheel, 0)); // default 0.5
            //stats.Add(new Stat("V\u007f}eqT\u0084", typeof(float), "V\u007f}eqT\u0084", StatType.Wheel)); // default 0.5
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
            if (Physics.gravity.y != -17.5f)
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
            Utilities.Log("StatsModification | Saving stats to '" + savePath + "'");
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
                Utilities.Log("StatsModification | LoadStats() Failed! Maybe no file is present?");
                return;
            }
        }
        public void ResetStats()
        {
            foreach (Stat stat in stats)
                stat.currentVal = stat.StartingValue;
        }
        public void DirtyStats()
        {
            // forces all stats to update themselves
            foreach (Stat stat in stats)
                stat.prevVal = null;
        }
        public void Update()
        {
            foreach (Stat stat in stats)
            {
                if (stat.currentVal == null && stat.prevVal == null)
                {
                    try
                    {
                        if (stat.statType == StatType.Vehicle)
                        {
                            stat.currentVal = GetVehicleMod(stat.ObfuscatedName).ToString();
                        }
                        else if (stat.statType == StatType.Modifier)
                        {
                            stat.currentVal = GetModMod(stat.ObfuscatedName).ToString();
                        }
                        else
                        {
                            stat.currentVal = GetWheelMod(stat.ObfuscatedName, stat.wheelNo).ToString();
                        }
                        
                        stat.StartingValue = stat.currentVal;
                        stat.prevVal = stat.currentVal;
                    } catch { }
                }
                if (stat.currentVal != stat.prevVal)
                {
                    if (!permitted && stat.currentVal != stat.StartingValue)
                        return;
                    try
                    {
                        if (stat.statType == StatType.Vehicle) {
                            if (ModifyVehicle(stat.ObfuscatedName, float.Parse(stat.currentVal)))
                            {
                                stat.prevVal = stat.currentVal;
                            }
                        }
                        else if (stat.statType == StatType.Modifier) {
                            if (ModifyMod(stat.ObfuscatedName, stat.currentVal))
                            {
                                stat.prevVal = stat.currentVal;
                            }
                        }

                    }
                    catch (Exception e){
                        Debug.Log(e);
                    }
                }
            }
        }
        public bool ModifyVehicle(string field, object value)
        {
            Utilities.GetPlayer()
                .GetComponent<Vehicle>()
                .GetType().GetField(field)
                .SetValue(
                    Utilities.GetPlayer().GetComponent<Vehicle>(), value
                );
            return true;
        }
        public bool ModifyWheel(string field, object value, int wheelNum)
        {
            Utilities.GetPlayer()
                .GetComponentsInChildren<Wheel>()[wheelNum]
                .GetType().GetField(field)
                .SetValue(
                    Utilities.GetPlayer().GetComponentsInChildren<Wheel>()[wheelNum], value
                );
            return true;
        }

        public bool ModifyMod(string field, object value)
        {
            GameModifier[] gameModifiers = (GameModifier[])typeof(GameData).GetField("\u0081jU\u0080h\u0084c").GetValue(FindObjectOfType<GameData>());
            PlayerInfoImpact pi = Utilities.instance.GetPlayerInfoImpact();
            foreach (GameModifier gameModifier in gameModifiers)
            {
                if (gameModifier.name == field)
                {
                    gameModifier.modifiers[0].percentageValue = float.Parse((string)value);
                    pi.AddGameModifier(gameModifier);
                    return true;
                }
            }
            return false;
        }
        public object GetWheelMod(string field, int wheelNum)
        {
            return Utilities.GetPlayer()
                .GetComponentsInChildren<Wheel>()[wheelNum]
                .GetType().GetField(field).GetValue(
                Utilities.GetPlayer().GetComponentsInChildren<Wheel>()[wheelNum]
            );
        }
        public object GetVehicleMod(string field)
        {
            return Utilities.GetPlayer()
                .GetComponent<Vehicle>()
                .GetType().GetField(field).GetValue(
                Utilities.GetPlayer().GetComponent<Vehicle>()
            );
        }
        public object GetModMod(string field)
        {
            GameModifier[] gameModifiers = (GameModifier[])typeof(GameData).GetField("\u0081jU\u0080h\u0084c").GetValue(FindObjectOfType<GameData>());
            foreach (GameModifier gameModifier in gameModifiers)
                if (gameModifier.name == field)
                    return gameModifier.modifiers[0].percentageValue;
            return 0;
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
