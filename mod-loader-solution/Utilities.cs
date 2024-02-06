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
using System.Diagnostics;
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
        public readonly Dictionary<string, string> seeds = new Dictionary<string, string>() {
                { "0", "Lobby" },
                {"1008" , "BikeOut v1"},
                {"1009" , "Mt. Rosie"},
                {"1010" , "Vuurberg"},
                {"1011" , "Cambria"},
                {"1012" , "STMP Line"},
                {"1013" , "BikeOut v2"},
                {"1014" , "Stoker"},
                {"1015" , "Dyfi"},
                {"1016" , "BikeOut v3"},
                {"1017" , "New Lexico"},
                {"1018" , "Alodalakes Bike Resort"},
                {"1019" , "Descenders Island"},
                {"1020" , "The Sanctuary"},
                {"1021" , "Mega Park"},
                {"1022" , "Kushmuck 4x Park"},
                {"1023" , "Jump City"},
                {"1024" , "BikeOut v4"},
                {"1025" , "Ido Bike park"},
                {"1026" , "Rose Ridge"},
                {"1027" , "Mt Slope"},
                {"1028" , "Drylands National Park"},
                {"1029" , "Dutchman's Rock"},
                {"1030" , "Island Cakewalk"},
                {"1031" , "Llangynog Freeride"},
                {"1032" , "Rival Falls"}
            };
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
        bool normalised = false;

        public void Awake()
        {
            NormaliseModSongs();
        }
        public void Update()
        {
            if (isFlying)
                SetVel(20f);
            if (!normalised && FindObjectOfType<GameData>() != null)
            {
                NormaliseModSongs();
                normalised = true;
            }
        }
        public Gesture[] gestures = new Gesture[0] {};
        public void GetGestures()
        {
            string gesturesField = "EL\u0080\u007f\u0084\u0080o";
            gestures = (Gesture[])typeof(Cyclist).GetField(gesturesField).GetValue(GetPlayer().GetComponent<Cyclist>());
        }
        public void ReleaseAllLimbsOnTrick()
        {
            string gesturesField = "EL\u0080\u007f\u0084\u0080o";
            gestures = (Gesture[])typeof(Cyclist).GetField(gesturesField).GetValue(GetPlayer().GetComponent<Cyclist>());
            foreach (Gesture x in gestures)
            {
                x.oneShotTrick = false;
                x.releaseLeftHand = true;
                x.releaseLeftFoot = true;
                x.releaseRightHand = true;
                x.releaseRightFoot = true;
            }
        }

        public void NormaliseModSongs()
        {
            // Function to put proper playlist instead of 'Descenders' song on mod maps
            
            /*
            GameData gData = FindObjectOfType<GameData>();
            FieldInfo playlists = typeof(GameData).GetField("yzY\u0083jnm"); // 
            MusicPlaylist[] musicPlaylists = (MusicPlaylist[])playlists.GetValue(gData);
            foreach (MusicPlaylist x in musicPlaylists)
            {
                Debug.Log("for " + x.world + ":");

                foreach (SongEntry ent in x.songs)
                    Debug.Log("    " + ent.name);
            }
            // add music playlist for None (Mod maps)
            List<MusicPlaylist> musicPlaylistList = musicPlaylists.ToList();
            MusicPlaylist playlistOfModMaps = musicPlaylistList[0];
            playlistOfModMaps.world = World.None;
            musicPlaylistList.Add(playlistOfModMaps);
            musicPlaylists = musicPlaylistList.ToArray();
            playlists.SetValue(gData, musicPlaylists);
            */



            // dirty the current session (to force a playlist re-load)

            /*FieldInfo currentSessionField = typeof(SessionManager).GetField("\u0083ESVMoz");
            AudioManager
            Session session = (Session)currentSessionField.GetValue(FindObjectOfType<SessionManager>());
            FieldInfo worldMapField = typeof(Session).GetField("WnRr]U`"); // world map
            //WorldMap worldMap = worldMapField.GetValue(session);
            worldMapField.SetValue(session, null);
            //WorldMap test = new WorldMap();
            //test.world = World.Hell;
            // session.worldMap = null; // set WorldMap to not null so 
            currentSessionField.SetValue(FindObjectOfType<SessionManager>(), session);*/

            /*
             * The check in AudioManager to reset the playlist is:
             * if (Singleton<SessionManager>.SP.GetWorld() != oldWorld) // (basically at least)
             * so we need to change the result of GetWorld() somehow without breaking anything.
             * 
             * 
             *  public World GetWorld()
                {
                    if (!SessionStarted())
                    {
                        return World.Overworld;
                    }

                    if (currentSession.worldMap != null)
                    {
                        return currentSession.worldMap.world;
                    }

                    if (currentSession.level != null)
                    {
                        return currentSession.level.world;
                    }

                    return World.None;
                }
             * } -> at the minute returns World.None
             */

            // dirty the current session (to force a playlist re-load)
            FieldInfo currentSessionField = typeof(SessionManager).GetField("\u0083ESVMoz"); // currentSession field
            object session = currentSessionField.GetValue(FindObjectOfType<SessionManager>()); // currentSession value
            FieldInfo worldMapField = session.GetType().GetField("WnRr]U`"); // worldMap
            WorldMap worldMap = new WorldMap(); // worldMap will be null, so we need to make one up
            worldMap.world = World.Glaciers; // set the world to glaciers (not World.None), this will play glaciers playlist
            Debug.Log("L6");
            worldMapField.SetValue(session, worldMap);
            Debug.Log("L7");
            // worldMap 
            Debug.Log(worldMap);
            FieldInfo worldWorldMapField = worldMap.GetType().GetField("world");
            Debug.Log("L6");
            worldWorldMapField.SetValue(worldMap, World.Forest);
            Debug.Log(session);

            //FieldInfo worldMapField = typeof(Session).GetField("WnRr]U`"); // worldMap field
            //WorldMap worldMap = (WorldMap)worldMapField.GetValue(session); // worldMap value
            // just debug to see if worldMap is none
            //Debug.Log("worldMap:" + worldMap);
            // set world on worldMap to none

            //worldMapField.SetValue(session, null);
            //WorldMap test = new WorldMap();
            //test.world = World.Hell;
            // session.worldMap = null; // set WorldMap to not null so 
            //currentSessionField.SetValue(FindObjectOfType<SessionManager>(), session);

        }
        public void Start()
        {
            instance = this;
        }
        AudioSource[] audioSources;
        public bool MapAudioActive = true;
        public void ToggleMapAudio()
        {
            if (audioSources == null)
                audioSources = FindObjectsOfType<AudioSource>();
            foreach (AudioSource audioSource in audioSources)
                audioSource.enabled = !audioSource.enabled;
            MapAudioActive = !MapAudioActive;
        }
        GameObject cyclist;
        public bool hasBailed()
        {
            if (cyclist == null)
                cyclist = GameObject.Find("Cyclist");
            if (cyclist != null)
                if (cyclist.transform.root.name == "Player_Human")
                    return false;
            return true;
        }
        public void SpawnAtCursor()
        {

        }
        // Note: VERY INNEFFICIENT
        public List<GameObject> GetNetworkedPlayers()
        {
            List<GameObject> networkedPlayers = new List<GameObject>();
            foreach (GameObject x in FindObjectsOfType<GameObject>()) {
                if (x.name == "Player_Networked")
                    networkedPlayers.Add(x);
            }
            return networkedPlayers;
        }
        public void RestartReplay()
        {
            Utilities.Log("Utilities | RestartReplay()");
            GameObject.Find("Player_Human").GetComponent<VehicleReplay>().SendMessage("StartRecord");
        }
        public void SaveReplayToFile(string path)
        {
            string replayObfuscatedName = "Ym}\u0084upr";
            Utilities.Log("Utilities | SaveReplayToFile('" + path + "')");
            Assembly a = Assembly.Load("Assembly-CSharp");
            Type replayType = a.GetType("l\u0080KRMtV");
            MethodInfo magicMethod = replayType.GetMethod("I\u0083tz]jk");
            Utilities.Log("Utilities | magicMethod found -" + magicMethod);
            Utilities.Log("Utilities | Vehicle Replay - " + GameObject.Find("Player_Human").GetComponent<VehicleReplay>());
            object replayClassObject = typeof(VehicleReplay)
                .GetField(replayObfuscatedName)
                .GetValue(GameObject.Find("Player_Human").GetComponent<VehicleReplay>());
            Utilities.Log("Utilities | replayClassObject found -" + replayClassObject);
            magicMethod.Invoke(replayClassObject, new object[] { path });
        }
        public float AngleFromGround()
        {
            return Vector3.Angle(GetPlayer().transform.up, Vector3.up);
        }
        public string GetUniqueID()
        {
            if (uniqueID != null)
                return uniqueID;
            return GetUniqueID("URM.txt");
        }
        public bool isFlying;
        bool PlayerCollision;
        public void TogglePlayerCollision()
        {
            Physics.IgnoreLayerCollision(8, 8, PlayerCollision);
            PlayerCollision = !PlayerCollision;
        }
        public void ToggleFly()
        {
            isFlying = !isFlying;
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
                    // Utilities.Log("ModLoaderSolution | Created " + file);
                }
                else
                {
                    id = File.ReadAllLines(userFile)[0];
                    id = id.Replace("\n", "");
                    // Utilities.Log("ModLoaderSolution | Found UserID " + id);
                }
            }
            catch (Exception e)
            {
                // Utilities.Log("ModLoaderSolution | Could not read user file:");
                // Utilities.Log(e);
            }

            uniqueID = id;
            return uniqueID;
        }
        public string GetCurrentMap()
        {
            SessionManager sessionManager = Singleton<SessionManager>.SP;
            return sessionManager.GetCurrentLevelFullSeed();
        }
        public static long ToUnixTime(DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date - epoch).TotalSeconds);
        }
        public void UpdateDiscordPresence(long startTimestamp = 0)
        {
            // don't do custom presence in freeplay
            if (!isBikePark() && !isMod() && !(GetCurrentMap() == "0"))
                return;
            DiscordManager dm = DiscordManager.SP;
            // dm.presence.details = dm.\u0084mfo\u007fzP.details
            DiscordRpc.RichPresence richpresence = (DiscordRpc.RichPresence)typeof(DiscordManager).GetField("\u0084mfo\u007fzP").GetValue(dm);
            DiscordRpc.RichPresence richpresenceCopy = richpresence;
            string current_map = GetCurrentMap().Split('-')[0];
            
            if (seeds.TryGetValue(current_map, out var seed))
                richpresence.details = "In " + seed;
            else
                richpresence.details = "In " + current_map;
            richpresence.largeImageText = "nohumanman's Descenders Modkit";
            richpresence.largeImageKey = "overworld";
            // teams to small image
            //richpresence.smallImageKey = "arboreal";
            //richpresence.smallImageText = "Team Arboreal";
            if (startTimestamp != 0)
                richpresence.startTimestamp = startTimestamp;

            typeof(DiscordManager).GetField("\u0084mfo\u007fzP").SetValue(dm, richpresence);
            // if different, update presence
            // NOTE: This will always update, because our assignment of rich presence
            // is overwritten immediately by descenders
            if (!richpresenceCopy.Equals(richpresence))
                DiscordRpc.UpdatePresence(ref richpresence);
            
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
            return currentMap.Contains("-");

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
            try
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
            catch (NullReferenceException)
            {
                return false;
            }
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
        public bool isInReplayMode()
        {
            if (GameObject.Find("State_ReplayBrowser") != null)
                return true;
            return false;
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
        //    Utilities.Log("Utilities: Loading Mod " + mod.name);
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
        //    //Utilities.Log("Utilities: loaded mod " + mod.name);
        //    //Utilities.Log("Utilities: loading scene");
        //    //ModScene modScene = mod.scenes.FirstOrDefault<ModScene>();
        //    //if (modScene != null)
        //    //{
        //    //    modScene.LoadAsync();
        //    //}
        //    //Utilities.Log("Utilities: loaded scene");

        //}

        //public void LoadMap(string seed)
        //{
        //    Utilities.Log("Utilities: Loading seed " + seed);
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
        //    //    Utilities.Log(underlyingValue);
        //    //}
        //    //sessionManager.StartNewSession(seed, global::GameMode.Sandbox);
        //}

        public void LogAllGameobjects()
        {
            string loc = "gameobjects.txt";
            string allobjs = "";
            foreach (GameObject x in FindObjectsOfType<GameObject>())
                allobjs += x.name + "\n";
            File.WriteAllText(loc, allobjs);
        }
        public bool isInPauseMenu()
        {
            string[] menus = { "UI_Pause", "UI_Options", "UI_OptionsGameplay", "UI_OptionsVideo", "UI_OptionsAudio", "UI_OptionsKeyBindings", "UI_OptionsLanguages" };
            foreach(string menu in menus)
                if (GameObject.Find(menu) != null)
                    return true;
            return false;
        }
        public struct CustomPlayerInf
        {
            public string playerName;
            public string steamID;
            public GameObject playerObject;
        }
        public static CustomPlayerInf FromPlayerInfo(PlayerInfo inf)
        {
            string json = JsonUtility.ToJson(inf);
            json = json.Replace("a^sXfY", "playerName");
            json = json.Replace("r~xs{n", "steamID");
            json = json.Replace("W\u0082oQHKm", "playerObject");
            CustomPlayerInf id = JsonUtility.FromJson<CustomPlayerInf>(json);
            return id;
        }
        public static GameObject GetPlayerFromId(string steam_id)
        {
            foreach (PlayerInfo inf in Singleton<PlayerManager>.SP.GetAllPlayers())
                if (FromPlayerInfo(inf).steamID == steam_id)
                    return FromPlayerInfo(inf).playerObject;
            return null;
        }
        public static PlayerInfoImpact GetPlayerInfoImpactFromId(string steam_id)
        {

            foreach (PlayerInfo inf in Singleton<PlayerManager>.SP.GetAllPlayers())
                if (FromPlayerInfo(inf).steamID == steam_id)
                    return (PlayerInfoImpact)inf;
            return null;
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
        public static void Log(GameObject obj)
        {
            Log(obj.ToString());
        }
        public static void Log(System.Exception exc)
        {
            Log(exc.ToString());
        }
        public static void Log(float exc)
        {
            Log(exc.ToString());
        }
        public static void Log(string log)
        {
            if (NetClient.DebugType == DebugType.RELEASE)
                return;
            MethodBase caller = new StackFrame(1, false).GetMethod();
            string prefix = caller.ReflectedType.FullName + "." + caller.Name;
            // we'll try to make the prefix length
            while (prefix.Length < 50)
                prefix = " " + prefix;
            Debug.Log(prefix + " - " + log);
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
        public string GetIdFromPlayerInfo(PlayerInfo pi)
        {
            return (string)pi.GetType().GetField("r~x\u007fs{n").GetValue(pi);
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
        public void SpectatePlayerCustom(string id)
        {
            GameObject player = GetPlayerFromId(id);
            if (player == null)
                return;
            FindObjectOfType<FollowCamSystem>().subject = player;
            FindObjectOfType<FollowCamSystem>().bother = true;
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
                Utilities.Log("Player not found: " + name);
        }
        public int GetBike()
        {
            return 0;
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
            Destroy(ui_BikeSelection);
            return;
            try
            {
                PlayerInfoImpact[] array = FindObjectsOfType<PlayerInfoImpact>();
                PlayerCustomization[] array2 = FindObjectsOfType<PlayerCustomization>();
                GameData gameData = FindObjectOfType<GameData>();
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
        public void SetPlayerSize(float scale)
        {
            GetPlayer().gameObject.transform.localScale = new Vector3(1, 1, 1) * scale;
        }
        public bool isInFreeCam()
        {
            return Camera.main.GetComponent<FreeCam>().enabled;
        }
        public bool isInBikeCamera()
        {
            return Camera.main.GetComponent<BikeCamera>().enabled;
        }
        public void DisableControlledCam()
        {
            Camera.main.GetComponent<BikeCamera>().enabled = false;
            Camera.main.GetComponent<FreeCam>().enabled = false;
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
        public void DisableAllBendGoals()
        {
            GameObject.Find("BendGoal_Left").SetActive(false);
            GameObject.Find("BendGoal_Right").SetActive(false);
        }
        public void ForceBail(Transform transform)
        {
            //GetCyclist(transform).SendMessage("Bail");
        }
        public void Bail()
        {
            GetPlayer().SendMessage("Bail");
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
            if (GetPlayer() != null)
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
                Utilities.Log(e);
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
        public void Gravity(float gravity)
        {
            Physics.gravity = Vector3.down * gravity;
        }
        public void SwitchSpectate()
        {
            MultiManager mm = MonoBehaviour.FindObjectOfType<MultiManager>();
            mm.SwitchToSpectate();
            Utilities.Log("utilities.SwitchSpectate");
        }
        public void ToggleSpectator()
        {
            MultiManager mm = MonoBehaviour.FindObjectOfType<MultiManager>();
            mm.ToggleSpectator();
            Utilities.Log("utilities.SwitchSpectate");
        }
        public void ClearSessionMarker()
        {
            PlayerInfoImpact pi = GetPlayerInfoImpact();
            pi.ClearSessionMarker();
        }
        public void ResetPlayer()
        {
            Utilities.Log("utilities.ResetPlayer");
            // activeModifiers uT`Xbuc
            PlayerInfoImpact pi = GetPlayerInfoImpact();
            List<GameModifier> activeModifiers = (List<GameModifier>)pi.GetType().GetField("uT`Xbuc").GetValue(pi);
            List<GameModifier> setModifiers = new List<GameModifier>();
            foreach (GameModifier gameModifier in activeModifiers)
            {
                Utilities.Log(gameModifier.name);
                if (!GameModifiers.gameModifiers.Contains(gameModifier.name))
                {
                    setModifiers.Add(gameModifier);
                }
            }
            pi.ResetPlayer();
            if (setModifiers.Count > 0)
            {
                Utilities.Log("Keeping " + setModifiers.Count + " modifiers");
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
            Utilities.Log("Utilities.RespawnOnTrack");
        }
        public void RespawnAtStartline()
        {
            PlayerInfoImpact pi = GetPlayerInfoImpact() ;
            pi.RespawnAtStartLine();
            Utilities.Log("Utilities.RespawnAtStartline");
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