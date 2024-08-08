using ModLoaderSolution;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerIdentification{
	public struct Identification
    {
        public string playerName;
        public string steamID;
    }
	public class SteamIntegration {
		public Identification id;

        public string getName(){
            return getPlayerId().playerName;
        }

        public string getSteamId(){
            return getPlayerId().steamID;
        }

        Identification getPlayerId()
        {
			// If in editor, don't make errors, just return a skewed value.
			if (Application.isEditor){
				Identification playerId =  new Identification();
				playerId.playerName = "Runtime Name";
				playerId.steamID = "1234567890";
				return playerId;
			}
            GameObject playerInfoHuman = ModLoaderSolution.Utilities.GameObjectFind("PlayerInfo_Human");
            if (playerInfoHuman == null)
            {
                return new Identification();
            }
       
            Component player_info_impact = playerInfoHuman.GetComponent("PlayerInfoImpact");
            string json = JsonUtility.ToJson(player_info_impact);

            json = json.Replace(ObfuscationHandler.GetObfuscated("playerName"), "playerName");
            json = json.Replace(ObfuscationHandler.GetObfuscated("userID"), "steamID");
            
            Identification id = JsonUtility.FromJson<Identification>(json);
            return id;
        }
	}
}

