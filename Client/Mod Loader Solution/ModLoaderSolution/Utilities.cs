using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.UI;
using ModTool.Interface;
using ModTool.Shared;
using ModTool;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using TMPro;
using InControl;

// All credit to h!gh voltage for this script (https://www.youtube.com/c/FdShighvoltage)

namespace ModLoaderSolution
{
    public class Utilities : MonoBehaviour
    {
        public static Utilities instance;
        public string uniqueID;
        public Text LogUI;
        public int maxLines = 6;
        List<string> history = new List<string>();

        private Camera mainCamera;
        private float GameTime = 0f;
        public float gameTime
        {
            get
            {
                return GameTime;
            }
            private set
            {
                GameTime = value;
            }
        }

        public GameObject playerObject;
        string playerName = null;
        string playerTotalRep = null;
        public bool playerFirstSpawned = false;

        UI_MultiplayerNotifications multiplayerNotifications;
        RigidbodyConstraints constraints;


        //void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.Numlock))
        //    {
        //        GameObject obj = GetPlayer();
        //        test(obj.transform.position);
        //    }
        //}

        public void Start()
        {
            instance = this;
        }

        public string GetUniqueID()
        {
            if (uniqueID != null)
                return uniqueID;
            return GetUniqueID("URM.txt");
        }

        string GetUniqueID(string file)
        {
            string id = null;
            // read/save user id
            try
            {
                string userFile = Directory.GetCurrentDirectory() + "\\" + file;
                if (!File.Exists(userFile))
                {
                    id = Guid.NewGuid().ToString();
                    StreamWriter sw = new StreamWriter(userFile);
                    sw.WriteLine(id);
                    sw.Close();
                    Debug.Log("ModLoaderSolution | Created " + file);
                }
                else
                {
                    id = File.ReadAllLines(userFile)[0];
                    id = id.Replace("\n", "");
                    Debug.Log("ModLoaderSolution | Found UserID " + id);
                }
            }
            catch (Exception e)
            {
                Debug.Log("ModLoaderSolution | Could not read user file:");
                Debug.Log(e);
            }

            uniqueID = id;
            return uniqueID;
        }

        public string GetCurrentMap()
        {
            SessionManager sessionManager = Singleton<SessionManager>.SP;
            return sessionManager.GetCurrentLevelFullSeed();
        }

        public List<Mod> GetAllMods()
        {
            GameData gameData = FindObjectOfType<GameData>();
            //UcS\u0082\u0081DM mods
            List<Mod> mods = (List<Mod>)gameData.GetType().GetField("UcS\u0082\u0081DM").GetValue(gameData);
            return mods;
        }

        public bool isMod()
        {
            string currentMap = GetCurrentMap();
            List<Mod> mods = GetAllMods();
            foreach (Mod mod in mods)
            {
                if (mod.name + "-" + mod.modInfo.version == currentMap)
                    return true;
            }
            return false;
        }

        public bool isBikePark()
        {
            string currentMap = GetCurrentMap();
            //ZIq^s|j bikeParks
            //FqVmLOT bonusLevels
            GameData gameData = FindObjectOfType<GameData>();
            BonusLevelInfo[] bikeParks = (BonusLevelInfo[])gameData.GetType().GetField("ZIq^s|j").GetValue(gameData);
            BonusLevelInfo[] bonusLevels = (BonusLevelInfo[])gameData.GetType().GetField("FqVmLOT").GetValue(gameData);
            foreach (BonusLevelInfo level in bikeParks)
            {
                if (level.customSeed.ToString() == currentMap)
                    return true;
            }
            foreach (BonusLevelInfo level in bonusLevels)
            {
                if (level.customSeed.ToString() == currentMap)
                    return true;
            }
            return false;
        }

        public bool isSeed()
        {
            if (!isBikePark() && !isMod())
            {
                return true;
            }
            return false;
        }

        public BonusLevelInfo[] GetAllBikeParks()
        {
            GameData gameData = FindObjectOfType<GameData>();
            BonusLevelInfo[] bikeParks = (BonusLevelInfo[])gameData.GetType().GetField("ZIq^s|j").GetValue(gameData);
            return bikeParks;
        }

        public string GetBikeParkName(string seed)
        {
            if (seed == "1009")
                return "Mt. Rosie (1009)";
            else if (seed == "1017")
                return "New Lexico (1017)";

            GameData gameData = FindObjectOfType<GameData>();
            BonusLevelInfo[] bikeParks = (BonusLevelInfo[])gameData.GetType().GetField("ZIq^s|j").GetValue(gameData);
            BonusLevelInfo[] bonusLevels = (BonusLevelInfo[])gameData.GetType().GetField("FqVmLOT").GetValue(gameData);
            foreach (BonusLevelInfo level in bikeParks)
            {
                if (level.customSeed.ToString() == seed)
                    return level.name + " (" + seed + ")";
            }
            foreach (BonusLevelInfo level in bonusLevels)
            {
                if (level.customSeed.ToString() == seed)
                    return level.name + " (" + seed + ")";
            }
            return seed;
        }

        public StartLine GetStartLine()
        {
            try
            {
                PlayerInfoImpact player = GetPlayerInfoImpact();
                return player.GetStartLine();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public FinishLine GetFinishLine()
        {
            FinishLine finishLine = GameObject.FindObjectOfType<FinishLine>();
            return finishLine;
        }

        public GameObject GetStartLineObject()
        {
            return GameObject.FindGameObjectWithTag("Startline");
        }

        public GameObject GetFinishLineObject()
        {
            return GameObject.FindGameObjectWithTag("Finishline");
        }

        public Checkpoint[] GetCheckpoints()
        {
            Checkpoint[] checkpoints = GameObject.FindObjectsOfType<Checkpoint>();
            return checkpoints;
        }

        public GameObject[] GetCheckpointObjects()
        {
            GameObject[] checkpoints = GameObject.FindGameObjectsWithTag("Checkpoint");
            return checkpoints;
        }

        //public void LoadMap(Mod mod)
        //{
        //    Debug.Log("Utilities: Loading Mod " + mod.name);
        //    UI_FreerideWorkshop ui = Get_UI_FreerideWorkshop();
        //    ui.StartMod(mod);

        //    //DevCommandsGameplay.LoadLevelFromSeed("1001");
        //    //SessionManager sessionManager = Singleton<SessionManager>.SP;
        //    //sessionManager.StartNewSession(mod);

        //    //GameData gameData = FindObjectOfType<GameData>(); // loadedMod "Z{sz|~V"
        //    //Mod loadedMod = (Mod)gameData.GetType().GetField("Z{sz|~V").GetValue(gameData);
        //    //if (loadedMod != null)
        //    //{
        //    //    loadedMod.Unload();
        //    //}
        //    //mod.LoadAsync();
        //    //Debug.Log("Utilities: loaded mod " + mod.name);
        //    //Debug.Log("Utilities: loading scene");
        //    //ModScene modScene = mod.scenes.FirstOrDefault<ModScene>();
        //    //if (modScene != null)
        //    //{
        //    //    modScene.LoadAsync();
        //    //}
        //    //Debug.Log("Utilities: loaded scene");

        //}

        //public void LoadMap(string seed)
        //{
        //    Debug.Log("Utilities: Loading seed " + seed);
        //    SessionManager sessionManager = Singleton<SessionManager>.SP;
        //    //dZDR\u0081XK
        //    //object gameMode = sessionManager.GetType().GetField("dZDR\u0081XK").GetValue(sessionManager);
        //    //System.Type enumType = gameMode.GetType();
        //    //System.Type enumUnderlyingType = System.Enum.GetUnderlyingType(enumType);
        //    //System.Array enumValues = System.Enum.GetValues(enumType);

        //    //for (int i = 0; i < enumValues.Length; i++)
        //    //{
        //    //    object value = enumValues.GetValue(i);
        //    //    object underlyingValue = System.Convert.ChangeType(value, enumUnderlyingType);
        //    //    Debug.Log(underlyingValue);
        //    //}
        //    //sessionManager.StartNewSession(seed, global::GameMode.Sandbox);
        //}

        public bool isInPauseMenu()
        {
            GameObject pause_menu = GameObject.Find("UI_Pause");
            if (pause_menu != null)
            {
                //Log("isInPauseMenu true");
                return true;
            }
            //Log("isInPauseMenu false");
            return false;
        }

        public GameObject GetPlayer()
        {
            playerObject = Singleton<PlayerManager>.SP.GetPlayerObject();
            return playerObject;
        }

        public PlayerInfoImpact GetPlayerInfoImpact()
        {
            return Singleton<PlayerManager>.SP.GetPlayerImpact();
        }

        public bool GetPlayerFirstSpawned()
        {
            //Log("playerfirstspawned " + playerFirstSpawned);
            if (playerFirstSpawned)
                return true;

            playerObject = GetPlayer();
            if (playerObject != null)
                playerFirstSpawned = true;

            return playerFirstSpawned;
        }

        public List<string> GetPlayerLogTricks()
        {
            List<string> tricks = new List<string>();
            StuntLogLine[] sls = MonoBehaviour.FindObjectsOfType<StuntLogLine>();
            foreach(StuntLogLine sl in sls)
            {
                Component[] components = sl.gameObject.GetComponentsInChildren<Component>();
                foreach (Component c in components)
                {
                    try
                    {
                        Expression propertyExpr = Expression.Property(
                            Expression.Constant(c),
                            "text"
                        );
                        tricks.Add(Expression.Lambda<Func<string>>(propertyExpr).Compile()());
                        break;
                    }
                    catch (Exception){}
                }
            }
            return tricks;
        }

        public string GetPlayerTrick()
        {
            //---UI_RepPopup : label_repCashIn #Untagged
            //-- - UI_RepPopup : label_specialStatus #Untagged
            //--417 - UI_RepPopup : label_rep #Untagged TRICK REP COUNT
            //-- - UI_RepPopup : label_repMultiplier #Untagged
            //--HUGE AIR -UI_RepPopup : label_repSource #Untagged CURRENT TRICK
            //-- < sprite = 0 > 0 - UI_RepPopup : label_repTotal #Untagged
            //-- - UI_RepPopup : label_injuryRepHit #Untagged
            //-- - UI_RepPopup : label_specialStatus_Bail #Untagged
            //   label_repCashIn #Untagged TRICK REP COUNT
            //   label_specialStatus #Untagged TRICK LANDED
            //   label_rep #Untagged
            
            string trick = "";
            GameObject go = GameObject.Find("label_specialStatus");
            if (go != null)
            {
                Component[] components = go.GetComponentsInChildren<Component>();
                foreach (Component c in components)
                {
                    try
                    {
                        Expression propertyExpr = Expression.Property(
                            Expression.Constant(c),
                            "text"
                        );
                        trick = Expression.Lambda<Func<string>>(propertyExpr).Compile()();
                        break;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return trick;
        }

        public string GetPlayerCurrentTrick()
        {
            //---UI_RepPopup : label_repCashIn #Untagged
            //-- - UI_RepPopup : label_specialStatus #Untagged
            //--417 - UI_RepPopup : label_rep #Untagged TRICK REP COUNT
            //-- - UI_RepPopup : label_repMultiplier #Untagged
            //--HUGE AIR -UI_RepPopup : label_repSource #Untagged CURRENT TRICK
            //-- < sprite = 0 > 0 - UI_RepPopup : label_repTotal #Untagged
            //-- - UI_RepPopup : label_injuryRepHit #Untagged
            //-- - UI_RepPopup : label_specialStatus_Bail #Untagged
            //   label_repCashIn #Untagged TRICK REP COUNT
            //   label_specialStatus #Untagged TRICK LANDED
            //   label_rep #Untagged

            string trick = "";
            GameObject go = GameObject.Find("label_repSource");
            if (go != null)
            {
                Component[] components = go.GetComponentsInChildren<Component>();
                foreach (Component c in components)
                {
                    try
                    {
                        Expression propertyExpr = Expression.Property(
                            Expression.Constant(c),
                            "text"
                        );
                        trick = Expression.Lambda<Func<string>>(propertyExpr).Compile()();
                        break;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return trick;
        }
        public void SetRep(int rep)
        {
            DevCommandsBackEnd.Reputation = rep;
        }
        public string GetPlayerTotalRep()
        {
            if (playerTotalRep != null)
                return playerTotalRep;
            //  --0 - panel : label_speed #Untagged
            //--km / h - panel : label_units #Untagged
            string rep = "0";
            GameObject go = GameObject.Find("label_rep");
            if (go != null)
            {
                Component[] components = go.GetComponentsInChildren<Component>();
                foreach (Component c in components)
                {
                    try
                    {
                        Expression propertyExpr = Expression.Property(
                            Expression.Constant(c),
                            "text"
                        );
                        rep = Expression.Lambda<Func<string>>(propertyExpr).Compile()();
                        break;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            playerTotalRep = rep;
            return playerTotalRep;
        }

        public string GetPlayerNameJersey()
        {
            if (playerName != null)
                return playerName;

            string name = "Unknown";
            GameObject go = GameObject.Find("label_name");
            if (go != null)
            {
                Component[] components = go.GetComponentsInChildren<Component>();
                foreach (Component c in components)
                {
                    try
                    {
                        Expression propertyExpr = Expression.Property(
                            Expression.Constant(c),
                            "text"
                        );
                        name = Expression.Lambda<Func<string>>(propertyExpr).Compile()();
                        break;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            playerName = name.ToLower();
            return playerName;
        }

        public string GetPlayerName()
        {
            // playerName a^sXf\u0083Y;
            PlayerInfo player = Singleton<PlayerManager>.SP.GetPlayer();
            return (string)player.GetType().GetField("a^sXf\u0083Y").GetValue(player);
            //return player.playerName;
        }

        public string GetNameFromPlayerInfo(PlayerInfo pi)
        {
            return (string)pi.GetType().GetField("a^sXf\u0083Y").GetValue(pi);
            //return pi.playerName;
        }

        public List<string> GetSpectateTargets()
        {
            List<string> targets = new List<string>();
            string playerName = GetPlayerName();
            foreach (PlayerInfo pi in GetAllPlayers())
            {
                string name = GetNameFromPlayerInfo(pi);
                if (name != playerName)
                    targets.Add(name);
            }
            return targets;
        }

        public void SpectatePlayer(int id)
        {
            DevCommandsCamera.Spectate(id);
        }

        public void SpectatePlayer(string name)
        {
            PlayerInfo[] allPlayers = GetAllPlayers();
            PlayerInfo player = null;
            foreach (PlayerInfo pi in allPlayers)
            {
                if (GetNameFromPlayerInfo(pi).ToLower() == name.ToLower())
                {
                    player = pi;
                    break;
                }
            }
            if (player != null)
                SpectatePlayer(Array.IndexOf(allPlayers, player));
            else
                Debug.Log("Player not found: " + name);
        }
        public void SetBike(int bike)
        {
            UI_BikeSelection ui_BikeSelection = new UI_BikeSelection();
            try
            {
                ui_BikeSelection.HoverButtonBike(bike);
            }
            catch (Exception)
            {
            }
            UnityEngine.Object.Destroy(ui_BikeSelection);
            try
            {
                PlayerInfoImpact[] array = UnityEngine.Object.FindObjectsOfType<PlayerInfoImpact>();
                PlayerCustomization[] array2 = UnityEngine.Object.FindObjectsOfType<PlayerCustomization>();
                GameData gameData = UnityEngine.Object.FindObjectOfType<GameData>();
                BikeType[] array3 = (BikeType[])gameData.GetType().GetField("bx}n\u0080PQ").GetValue(gameData);
                foreach (PlayerInfoImpact playerInfoImpact in array)
                {
                    playerInfoImpact.GetType().GetField("dzQf\u0082nw").SetValue(playerInfoImpact, array3[bike]);
                }
                foreach (PlayerCustomization playerCustomization in array2)
                {
                    playerCustomization.RefreshBikeMesh();
                }
            }
            catch (Exception)
            {
            }
            //UI_BikeSelection bs = new UI_BikeSelection();
            //try
            //{
            //    bs.HoverButtonBike(bike);
            //}
            //catch (Exception){ }

            //Destroy(bs);

            //try
            //{
            //    PlayerInfoImpact[] allPlayers = FindObjectsOfType<PlayerInfoImpact>();
            //    PlayerCustomization[] allCPlayers = FindObjectsOfType<PlayerCustomization>();
            //    GameData gameData = FindObjectOfType<GameData>();
            //    BikeType[] bikeTypes = gameData.bikeTypes;

            //    foreach (PlayerInfoImpact player in allPlayers)
            //    {
            //        player.bikeType = bikeTypes[bike];
            //    }

            //    foreach (PlayerCustomization player in allCPlayers)
            //    {
            //        player.RefreshBikeMesh();
            //    }
            //}
            //catch (Exception) { }
        }

        public string GetBikeName(int bike)
        {
            string name = "Enduro";
            if (bike == 1)
                name = "Downhill";
            if (bike == 2)
                name = "Hardtail";
            return name;
        }

        public void SetCameraTarget(PlayerInfoImpact player, bool something=true)
        {
            Singleton<CameraManager>.SP.SetCameraTarget(player, something);
        }

        public PlayerInfo[] GetAllPlayers()
        {
            return Singleton<PlayerManager>.SP.GetAllPlayers();
        }

        public bool isInFreeCam()
        {
            return Camera.main.GetComponent<FreeCam>().enabled;
        }
        public bool isInBikeCamera()
        {
            return Camera.main.GetComponent<BikeCamera>().enabled;
        }

        public void SetFreeCam()
        {
            if (!isInFreeCam())
            {
                if (isInBikeCamera())
                    Camera.main.GetComponent<BikeCamera>().enabled = false;
                Camera.main.GetComponent<FreeCam>().enabled = true;
            }
        }

        public void SetBikeCamera()
        {
            if (!isInBikeCamera())
            {
                if (isInFreeCam())
                    Camera.main.GetComponent<FreeCam>().enabled = false;
                Camera.main.GetComponent<BikeCamera>().enabled = true;
            }
        }

        public void FreezePlayer()
        {
            GameObject player = GetPlayer();
            constraints = player.GetComponent<Rigidbody>().constraints;
            player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }

        public void UnfreezePlayer()
        {
            GameObject player = GetPlayer();
            player.GetComponent<Rigidbody>().constraints = constraints;
        }

        public void ForceBail(Transform transform)
        {
            //GetCyclist(transform).SendMessage("Bail");
        }

        UI_FreerideWorkshop Get_UI_FreerideWorkshop()
        {
            UI_FreerideWorkshop[] mps = Resources.FindObjectsOfTypeAll<UI_FreerideWorkshop>();
            if (mps.Length > 0)
                return mps[0];
            else
            {
                UI_FreerideWorkshop mp = Resources.Load("Prefabs/", typeof(UI_FreerideWorkshop)) as UI_FreerideWorkshop;
                return mp;
            }
        }

        UI_MultiplayerNotifications Get_UI_MultiplayerNotifications()
        {
            UI_MultiplayerNotifications[] mps = Resources.FindObjectsOfTypeAll<UI_MultiplayerNotifications>();
            if (mps.Length > 0)
                return mps[0];
            else
            {
                UI_MultiplayerNotifications mp = Resources.Load("Prefabs/", typeof(UI_MultiplayerNotifications)) as UI_MultiplayerNotifications;
                return mp;
            }
        }

        public void StartMPCountdown()
        {
            if (multiplayerNotifications == null)
            {
                multiplayerNotifications = Get_UI_MultiplayerNotifications();
            }
            multiplayerNotifications.StartCountdown();
        }

        public void ContinuousCenterMessage(string message)
        {
            multiplayerNotifications.SetCountdownText(message);
        }

        public void CenterMessage(string message)
        {
            if (multiplayerNotifications == null)
            {
                multiplayerNotifications = Get_UI_MultiplayerNotifications();
            }
            StopCoroutine("HideCenterMessage");
            multiplayerNotifications.SetCountdownText(message);
            StartCoroutine("HideCenterMessage");
            //JZigWYK   ??
        }

        public IEnumerator HideCenterMessage()
        {
            yield return new WaitForSeconds(5);
            multiplayerNotifications.SetCountdownText("");
        }

        public void HideCenterMessageImmediate()
        {
            if (multiplayerNotifications != null)
            {
                multiplayerNotifications.SetCountdownText("");
            }
        }

        public void ShowMessage(string message)
        {
            if (multiplayerNotifications == null)
            {
                multiplayerNotifications = Get_UI_MultiplayerNotifications();
            }
            multiplayerNotifications.ShowMessage(message);
        }

        public bool isInAir()
        {
            if (GetGroundFactor() < 0.9)
                return true;
            return false;
        }

        public float GetGroundFactor()
        {
            return GetPlayer().GetComponent<Cyclist>().onGroundFactor;
        }
        public bool isBailed()
        {
            //Y~IX\u0084YS
            Cyclist cyclist = GetPlayer().GetComponent<Cyclist>();
            //return !cyclist.onBike;
            bool bailed = true;
            try
            {
                Expression propertyExpr = Expression.Property(
                    Expression.Constant(cyclist),
                    "Y~IX\u0084YS"
                );
                bailed = Expression.Lambda<Func<bool>>(propertyExpr).Compile()();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            return !bailed;
        }

        public void ToggleControl(bool set)
        {
            VehicleController vc = GetPlayer().GetComponent<VehicleController>();
            vc.ToggleControl(set);
        }
        bool brakesCut = false;
        public void CutBrakes()
        {
            VehicleController vc = GetPlayer().GetComponent<VehicleController>();
            brakesCut = !brakesCut;
            vc.brakesCut = brakesCut;
        }

        public void SetVel(float multFactor)
        {
            Vector3 x = GetPlayer().transform.forward;
            x.y = 0;
            GetPlayer().SendMessage("SetVelocity", x * multFactor);
        }
        public void Gravity()
        {
            Physics.gravity = Vector3.down * 3;
        }
        public void SwitchSpectate()
        {
            MultiManager mm = MonoBehaviour.FindObjectOfType<MultiManager>();
            mm.SwitchToSpectate();
            Debug.Log("utilities.SwitchSpectate");
        }
        public void ToggleSpectator()
        {
            MultiManager mm = MonoBehaviour.FindObjectOfType<MultiManager>();
            mm.ToggleSpectator();
            Debug.Log("utilities.SwitchSpectate");
        }
        public void ClearSessionMarker()
        {
            PlayerInfoImpact pi = GetPlayerInfoImpact();
            pi.ClearSessionMarker();
        }
        public void ResetPlayer()
        {
            Debug.Log("utilities.ResetPlayer");
            // activeModifiers uT`Xbuc
            PlayerInfoImpact pi = GetPlayerInfoImpact();
            List<GameModifier> activeModifiers = (List<GameModifier>)pi.GetType().GetField("uT`Xbuc").GetValue(pi);
            List<GameModifier> setModifiers = new List<GameModifier>();
            foreach (GameModifier gameModifier in activeModifiers)
            {
                Debug.Log(gameModifier.name);
                if (!GameModifiers.gameModifiers.Contains(gameModifier.name))
                {
                    setModifiers.Add(gameModifier);
                }
            }
            pi.ResetPlayer();
            if (setModifiers.Count > 0)
            {
                Debug.Log("Keeping " + setModifiers.Count + " modifiers");
                pi.GetType().GetField("uT`Xbuc").SetValue(pi, setModifiers);
            }
        }

        public void AddGameModifier(string modifier)
        {
            PlayerInfoImpact pi = GetPlayerInfoImpact();
            pi.AddGameModifier(modifier);
        }

        public void AddGameModifier(string[] modifiers)
        {
            PlayerInfoImpact pi = GetPlayerInfoImpact();
            foreach (string modifier in modifiers)
                pi.AddGameModifier(modifier);
        }

        public void AddGameModifierAllPlayers(string[] modifiers)
        {
            PlayerInfoImpact[] allPlayers = FindObjectsOfType<PlayerInfoImpact>();
            foreach (PlayerInfoImpact player in allPlayers)
            {
                foreach (string modifier in modifiers)
                {
                    player.AddGameModifier(modifier);
                }
            }
        }

        public void RespawnOnTrack()
        {
            PlayerInfoImpact pi = GetPlayerInfoImpact();
            pi.RespawnOnTrack();
            Debug.Log("Utilities.RespawnOnTrack");
        }
        public void RespawnAtStartline()
        {
            PlayerInfoImpact pi = GetPlayerInfoImpact() ;
            pi.RespawnAtStartLine();
            Debug.Log("Utilities.RespawnAtStartline");
        }

        public void Log(string text)
        {
            Debug.Log(text);
            if (LogUI == null || !LogUI.gameObject.activeSelf)
                return;
            LogUI.text = "";
            history.Add(text);
            int length = history.Count;
            int max = length - maxLines;
            int counter = 0;
            foreach (var item in history)
            {
                counter++;
                if (max <= 0 || counter >= max)
                    LogUI.text = LogUI.text + item + "\n";
            }
            ShowLog();
            StopCoroutine("HideLog");
            StartCoroutine("HideLog");
        }

        public void ShowLog()
        {
            LogUI.gameObject.SetActive(true);
        }

        IEnumerator HideLog()
        {
            yield return new WaitForSeconds(5f);
            LogUI.gameObject.SetActive(false);
        }
        public bool bailEnabled = false;
        public void EnableStats()
        {
            PlayerInfoImpact pi = GetPlayerInfoImpact();
            DevTools.EnableAll();
            DevCommandsNetwork.ToggleInfo();
            DevCommandsPostFX.EnableAll();
        }
        public void ToggleGod()
        {
            DevCommandsGameplay.ToggleGodmode();
        }
    }


    public static class GameModifiers
    {
        public static string BunnyHop = "BUNNYHOP";
        public static string FakieBalance = "FAKIEBALANCE";
        public static string HeavyBailThreshold = "HEAVYBAILTHRESHOLD";
        public static string LandingImpact = "LANDINGIMPACT";
        public static string AirCorrection = "AIRCORRECTION";
        public static string OffroadFriction = "OFFROADFRICTION";
        public static string PumpStrength = "PUMPSTRENGTH";
        public static string SpeedWobbles = "SPEEDWOBBLES";
        public static string TweakSpeed = "TWEAKSPEED";
        public static string WheelieBalance = "WHEELIEBALANCE";
        public static string SpinSpeed = "SPINSPEED";

        public static string[] gameModifiers = new string[11]{
            BunnyHop, FakieBalance, HeavyBailThreshold,
            LandingImpact,AirCorrection,OffroadFriction,PumpStrength,
            SpeedWobbles,TweakSpeed,WheelieBalance,SpinSpeed
        };
  
        public static string[] gameModifiers2 = new string[7]{
            BunnyHop, AirCorrection, OffroadFriction, PumpStrength,
            SpeedWobbles, TweakSpeed, WheelieBalance
        };
    }

    public enum GameMode
    {
        None,
        StandardSession,
        GuestSession,
        WatchReplay,
        Spectate,
        Sandbox,
        Casual,
        StandardSessionPlus
    }
}