﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;

namespace PlayerIdentification{
	public struct Identification
    {
        public string playerName;
        public string steamID;
    }
	public class SteamIntegration : ModBehaviour {
		public Identification id;

        void Start()
        {
            id = getNameOfPlayer();
            Debug.Log(id.playerName);
            Debug.Log(id.steamID);
        }

        public Identification getNameOfPlayer()
        {
			// If in editor, don't make errors, just return a skewed value.
			if (Application.isEditor){
				Identification playerId =  new Identification();
				playerId.playerName = "Runtime Name";
				playerId.steamID = "1234567890";
				return playerId;
			}
            GameObject playerInfoHuman = GameObject.Find("PlayerInfo_Human");
            if (playerInfoHuman == null)
            {
                return new Identification();
            }
            Component player_info_impact = playerInfoHuman.GetComponent("PlayerInfoImpact");
            string json = JsonUtility.ToJson(player_info_impact);
            json = json.Replace("a^sXfY", "playerName");
            json = json.Replace("r~xs{n", "steamID");
            Identification id = JsonUtility.FromJson<Identification>(json);
            return id;
        }
	}
}

