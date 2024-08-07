using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModLoaderSolution
{
    class CustomDiscordManager : MonoBehaviour
    {
        public static CustomDiscordManager instance;
        public void Awake()
        {
            instance = this;
        }
        public IEnumerator ChangeMapPresence(string map_name)
        {
            long startTimestamp = Utilities.ToUnixTime(DateTime.UtcNow);
            while (Utilities.instance.GetCurrentMap() == map_name)
            {
                UpdateDiscordPresence(startTimestamp);
                yield return new WaitForSeconds(3f);
            }
        }
        public void UpdateDiscordPresence(long startTimestamp = 0)
        {
            // don't do custom presence in freeplay
            if (Utilities.instance.isInFreeplay())
                return;
            // get the discord managers
            DiscordManager discordManager = DiscordManager.SP;
            // dm.presence.details = dm.\u0084mfo\u007fzP.details
            DiscordRpc.RichPresence richpresence = (DiscordRpc.RichPresence)typeof(DiscordManager)
                .GetField(ObfuscationHandler.GetObfuscated("presence"))
                .GetValue(discordManager);
            DiscordRpc.RichPresence richpresenceCopy = richpresence;
            string current_map = Utilities.instance.GetCurrentMap().Split('-')[0];
            // add map we're in
            if (Utilities.instance.seeds.TryGetValue(current_map, out var seed))
                richpresence.details = "In " + seed;
            else
                richpresence.details = "In " + current_map;
            // give me some credit
            richpresence.largeImageText = "nohumanman's Descenders Modkit";
            // put the normal image on
            richpresence.largeImageKey = "overworld";
            // teams to small image
            //richpresence.smallImageKey = "arboreal";
            //richpresence.smallImageText = "Team Arboreal";
            if (startTimestamp != 0)
                richpresence.startTimestamp = startTimestamp;

            typeof(DiscordManager).GetField(ObfuscationHandler.GetObfuscated("presence")).SetValue(discordManager, richpresence);
            // if different, update presence
            // NOTE: This will always update, because our assignment of rich presence
            // is overwritten immediately by descenders
            if (!richpresenceCopy.Equals(richpresence))
                DiscordRpc.UpdatePresence(ref richpresence);
        }
    }
}
