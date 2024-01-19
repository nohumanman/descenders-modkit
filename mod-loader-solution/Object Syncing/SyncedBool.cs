using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace ModLoaderSolution.Object_Syncing {
    /**
     * Class to sync the attributes of the object that is attached with other players
    */
    public class SyncedBool : MonoBehaviour
    {
        bool syncedBool = false;
        public void SetBool(bool newBool, bool isServer)
        {
            if (isServer)
                syncedBool = newBool;
            else
            {
                if (syncedBool == newBool)
                    return;
                syncedBool = newBool;
                // get all networked players in lobby as csv
                PlayerInfo[] allPlayers = Utilities.instance.GetAllPlayers();
                string allPlayerNames = "";
                foreach (PlayerInfo player in allPlayers)
                    allPlayerNames += Utilities.FromPlayerInfo(player).playerName + ",";
                // send updated player names to lobby
                NetClient.Instance.SendData("SYNC|LOBBY_UPDATE|" + allPlayerNames); // e.g. SYNC|LOBBY_UPDATE|nohumanman,BBB171,antgrass,
                // send new bool to server
                NetClient.Instance.SendData("SYNC|SET_BOOL|" + newBool.ToString() + "|" + name); // e.g. SYNC|SET_BOOL|True|Cube (3)
            }
        }
        public void SetBool(bool newBool)
        {
            SetBool(newBool, false);
        }

        public void ResyncBool()
        {
            // request bool resync from server
            NetClient.Instance.SendData("SYNC|REQUEST_BOOL|" + name); // e.g. SYNC|REQUEST_BOOL|Cube (3)
        }
        public void Start()
        {
            // resync bool on first join
            ResyncBool();
        }

        bool wasInPauseMenu = false;
        public void Update()
        {
            if (wasInPauseMenu && !Utilities.instance.isInPauseMenu())
                ResyncBool(); // WHEN UNPAUSED WE MUST RESYNC BOOL
        }
    }
}