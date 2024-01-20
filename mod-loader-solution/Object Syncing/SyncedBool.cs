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
        // the bool to sync
        bool syncedBool = false;
        public void UpdateLobby()
        {
            PlayerInfo[] allPlayers = Utilities.instance.GetAllPlayers();
            string allPlayerNames = "";
            foreach (PlayerInfo player in allPlayers)
                allPlayerNames += Utilities.FromPlayerInfo(player).steamID + ","; // add to our csv
            // send updated player names to lobby
            NetClient.Instance.SendData("SYNC|LOBBY_UPDATE|" + allPlayerNames); // e.g. SYNC|LOBBY_UPDATE|nohumanman,BBB171,antgrass,
        }
        public void SetBool(bool newBool, bool isServer)
        {
            if (isServer)
                syncedBool = newBool;
            else
            {
                if (syncedBool == newBool)
                    return;
                syncedBool = newBool;
                UpdateLobby();
                // send new bool to server
                NetClient.Instance.SendData("SYNC|SET_BOOL|" + newBool.ToString() + "|" + name); // e.g. SYNC|SET_BOOL|True|Cube (3)
            }
        }
        public void SetBool(bool newBool)
        {
            SetBool(newBool, false);
        }
        public bool GetBool()
        {
            return syncedBool;
        }
        public void ResyncBool()
        {
            // request bool resync from server
            UpdateLobby(); // update lobby so it knows who's bools we're requesting
            NetClient.Instance.SendData("SYNC|REQUEST_BOOL|" + name); // e.g. SYNC|REQUEST_BOOL|Cube (3)
        }
        public void Start()
        {
            // resync bool on net start
            NetClient.Instance.onNetStart += ResyncBool;
        }
        public void Update()
        {
            if (Input.GetKeyDown("joystick button 0") || Input.GetKeyDown(KeyCode.Escape))
                ResyncBool(); // WHEN UNPAUSED WE MUST RESYNC BOOL
        }
    }
}