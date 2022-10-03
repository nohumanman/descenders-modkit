using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ModLoaderSolution;

namespace SplitTimer
{
    class StatsModification : MonoBehaviour
    {
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
        public void ModifyVehicle(string field, object value)
        {
            Utilities.instance.GetPlayer()
                .GetComponent<Vehicle>()
                .GetType().GetField(field)
                .SetValue(
                    Utilities.instance.GetPlayer().GetComponent<Vehicle>(), value
                );
        }

    }
}
