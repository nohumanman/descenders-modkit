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
using UnityEngine.SceneManagement;
using InControl;

// All credit to h!gh voltage for this script (https://www.youtube.com/c/FdShighvoltage)

namespace ModLoaderSolution
{
    public class Utilities : MonoBehaviour
    {
        public static Utilities instance;

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
        public static GameObject playerObject;
        string playerName = null;
        string playerTotalRep = null;
        public bool playerFirstSpawned = false;

        UI_MultiplayerNotifications multiplayerNotifications;
        RigidbodyConstraints constraints;

        bool normalised = false;
        public void Start()
        {
            instance = this;
        }
        public void Awake()
        {
            //NormaliseModSongs();
        }
        
        public void Update()
        {
            if (isFlying)
                SetVel(20f);
            if (!normalised && Utilities.GameObjectFindObjectOfType<GameData>() != null)
            {
                //NormaliseModSongs();
                normalised = true;
            }
            if (Input.GetKey(KeyCode.D) && Input.GetKeyDown(KeyCode.Alpha9)){
                Utilities.Log("Call Stats:\n" + GetCallStats());
            }
            GameObject content = Utilities.GameObjectFind("Content");
            if (content != null)
            {
                List<Transform> idealOrder = new List<Transform>();
                // find the ideal order
                foreach (Transform modInfoButtonTransform in content.transform)
                {
                    GameObject modInfoButton = modInfoButtonTransform.gameObject;
                    if (modInfoButton.GetComponentInChildren<TextMeshProUGUI>().text != "OPEN MOD BROWSER")
                        idealOrder.Add(modInfoButtonTransform);
                }
                idealOrder = idealOrder.OrderBy(x => x.gameObject.GetComponentInChildren<TextMeshProUGUI>().text).ToList();
                int i = 1;
                foreach (Transform modInfoButton in idealOrder)
                {
                    modInfoButton.SetSiblingIndex(i);
                    i++;
                }
            }
        }
        public Gesture[] gestures = new Gesture[0] {};
        public void GetGestures()
        {
            gestures = (Gesture[])typeof(Cyclist).GetField(
                ObfuscationHandler.GetObfuscated("gestures")
            ).GetValue(GetPlayer().GetComponent<Cyclist>());
        }
        public void ReleaseAllLimbsOnTrick()
        {
            gestures = (Gesture[])typeof(Cyclist).GetField(
                ObfuscationHandler.GetObfuscated("gestures")
            ).GetValue(GetPlayer().GetComponent<Cyclist>());
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
            // dirty the current session (to force a playlist re-load)
            FieldInfo currentSessionField = typeof(SessionManager).GetField(ObfuscationHandler.GetObfuscated("currentSession")); // currentSession field
            object session = currentSessionField.GetValue(Utilities.GameObjectFindObjectOfType<SessionManager>()); // currentSession value
            FieldInfo worldMapField = session.GetType().GetField(ObfuscationHandler.GetObfuscated("worldMap")); // worldMap
            WorldMap worldMap = new WorldMap(); // worldMap will be null, so we need to make one up
            worldMap.world = World.Glaciers; // set the world to glaciers (not World.None), this will play glaciers playlist
            worldMapField.SetValue(session, worldMap);
            // worldMap 
            FieldInfo worldWorldMapField = worldMap.GetType().GetField("world");
            worldWorldMapField.SetValue(worldMap, World.Forest);
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
            // if player doesn't exist they can't bail.
            if (GetPlayer() == null) return false;
            // see if cyclist is connected to player
            if (cyclist == null)
                cyclist = Utilities.GameObjectFind("Cyclist");
            if (cyclist != null)
                if (cyclist.transform.root.name == "Player_Human")
                    return false;
            return true;
        }
        public bool hasBailed(GameObject networkPlayer)
        {
            // iterate through all children to find 'Cyclist'
            foreach (Transform child in networkPlayer.transform)
                if (child.name == "Cyclist")
                    return false; // cyclist means we haven't bailed
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
            Utilities.Log("RestartReplay()");
            Utilities.GameObjectFind("Player_Human").GetComponent<VehicleReplay>().SendMessage("StartRecord");
        }
        public void SaveReplayToFile(string path)
        {
            Utilities.Log("SaveReplayToFile('" + path + "')");
            Assembly a = Assembly.Load("Assembly-CSharp");
            Type replayType = a.GetType(ObfuscationHandler.GetObfuscated("Replay"));
            MethodInfo magicMethod = replayType.GetMethod(ObfuscationHandler.GetObfuscated("SaveReplay"));
            Utilities.Log("magicMethod found -" + magicMethod);
            Utilities.Log("Vehicle Replay - " + Utilities.GameObjectFind("Player_Human").GetComponent<VehicleReplay>());
            object replayClassObject = typeof(VehicleReplay)
                .GetField(ObfuscationHandler.GetObfuscated("replay"))
                .GetValue(Utilities.GetPlayer().GetComponent<VehicleReplay>());
            Utilities.Log("replayClassObject found -" + replayClassObject);
            magicMethod.Invoke(replayClassObject, new object[] { path });
        }
        public float AngleFromGround()
        {
            return Vector3.Angle(GetPlayer().transform.up, Vector3.up);
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
        public string GetCurrentMap()
        {
            SessionManager sessionManager = Singleton<SessionManager>.SP;
            string map = sessionManager.GetCurrentLevelFullSeed();
            if (map == null)
                throw new NullReferenceException("Map is none!");
            return map;
        }
        public static long ToUnixTime(DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date - epoch).TotalSeconds);
        }

        public List<Mod> GetAllMods()
        {
            GameData gameData = Utilities.GameObjectFindObjectOfType<GameData>();
            //UcS\u0082\u0081DM mods
            List<Mod> mods = (List<Mod>)gameData.GetType().GetField("UcS\u0082\u0081DM").GetValue(gameData);
            return mods;
        }
        public bool isInFreeplay()
        {
            if (!isBikePark() && !isMod() && !(GetCurrentMap() == "0"))
                return true;
            return false;
        }
        public bool isMod()
        {
            string currentMap = GetCurrentMap();
            return currentMap.Contains("-");
        }

        public bool isBikePark()
        {
            try
            {
                string currentMap = GetCurrentMap();
                //ZIq^s|j bikeParks
                //FqVmLOT bonusLevels
                GameData gameData = Utilities.GameObjectFindObjectOfType<GameData>();
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
            GameData gameData = Utilities.GameObjectFindObjectOfType<GameData>();
            BonusLevelInfo[] bikeParks = (BonusLevelInfo[])gameData.GetType().GetField("ZIq^s|j").GetValue(gameData);
            return bikeParks;
        }
        public bool isInReplayMode()
        {
            if (Utilities.GameObjectFind("State_ReplayBrowser") != null)
                return true;
            return false;
        }
        public string GetBikeParkName(string seed)
        {
            if (seed == "1009")
                return "Mt. Rosie (1009)";
            else if (seed == "1017")
                return "New Lexico (1017)";

            GameData gameData = Utilities.GameObjectFindObjectOfType<GameData>();
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
            FinishLine finishLine = Utilities.GameObjectFindObjectOfType<FinishLine>();
            return finishLine;
        }
        static Hashtable cachedObjectsOfType = new Hashtable();
        static Hashtable recentlySearchedTypes = new Hashtable();
        public static T GameObjectFindObjectOfType<T>() where T : Object
        {
            return (T)GameObjectFindObjectOfType(typeof(T));
        }
        public static Object GameObjectFindObjectOfType(Type type)
        {
            if (cachedObjectsOfType.ContainsKey(type))
                return (Object)cachedObjectsOfType[type];
            // we don't have the gameobject cached!
            if (recentlySearched.ContainsKey(type))
                if ((Time.time - (float)recentlySearched[type]) < 1)
                    return null; // we searched for it not long ago, so let's not bother now
            cachedObjects.RemoveAll(x => !x); // remove all null caches (in case any have been deleted)
            Object objectOfType = (Object)FindObjectOfType(type);
            if (objectOfType != null)
                cachedObjectsOfType.Add(type, objectOfType);
            else
            {
                // if null
                if (recentlySearchedTypes.ContainsKey(type))
                    recentlySearched[type] = Time.time; // we'll update our failed search
                else
                    recentlySearchedTypes.Add(type, Time.time); // we'll add our failed search
            }

            return objectOfType;
        }
        static List<Object> cachedObjects = new List<Object>();
        static Hashtable recentlySearched = new Hashtable();
        public static GameObject GameObjectFind(string gameobjectName)
        {
            foreach (GameObject go in cachedObjects)
            {
                if (go != null)
                {
                    if (go?.name == gameobjectName)
                    {
                        return go;
                    }
                }
            }
            // we don't have the gameobject cached!

            if (recentlySearched.ContainsKey(gameobjectName))
            {
                if ((Time.time - (float)recentlySearched[gameobjectName]) < 1)
                {
                    return null; // we searched for it not long ago, so let's not bother now
                }
            }
            cachedObjects.RemoveAll(x => !x);
            GameObject foundObject = GameObject.Find(gameobjectName);
            if (foundObject != null)
                cachedObjects.Add(foundObject);
            else
            {
                // if nul
                if (recentlySearched.ContainsKey(gameobjectName))
                    recentlySearched[gameobjectName] = Time.time; // we'll update our failed search
                else
                    recentlySearched.Add(gameobjectName, Time.time); // we'll add our failed search
            }
                
            return foundObject;
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
        public static void PrintHierarchy(Transform parent, string indent = "")
        {
            Debug.Log(indent + parent.name);

            // Loop through all children and recursively print their hierarchy
            foreach (Transform child in parent)
            {
                PrintHierarchy(child, indent + "  "); // Increase the indentation for children
            }
        }

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
            Utilities.LogMethodCallStart();
            string[] menus = { "UI_Pause", "UI_Options", "UI_OptionsGameplay", "UI_OptionsVideo", "UI_OptionsAudio", "UI_OptionsKeyBindings", "UI_OptionsLanguages" };
            foreach(string menu in menus)
                if (Utilities.GameObjectFind(menu) != null)
                {
                    Utilities.LogMethodCallEnd();
                    return true;
                }
            Utilities.LogMethodCallEnd();
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

        /// <summary>
        /// Expect this method to return null often
        /// </summary>
        public static GameObject GetPlayer()
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
            GameObject go = Utilities.GameObjectFind("label_specialStatus");
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
            if (NetClient.debugState == DebugType.DEBUG || NetClient.debugState == DebugType.DEVELOPER)
            {
                MethodBase caller = new StackFrame(1, false).GetMethod();
                string prefix = caller.ReflectedType.FullName + "." + caller.Name;
                Debug.Log(DateTime.Now.ToString("MM.dd.yyy HH:mm:ss.fff") + " - " + prefix + " - " + log);
            }
        }
        public bool ModIsLoading()
        {
            return SceneManager.GetActiveScene().isLoaded;
        }
        static bool logMethodCalls = false;
        static List<MethodBase> methods = new List<MethodBase>();
        static List<float> methodsTimeCalled = new List<float>();
        static Hashtable methodInfos = new Hashtable();
        public static void LogMethodCallStart()
        {
            if (!logMethodCalls)
                return;
            MethodBase caller = new StackFrame(1, false).GetMethod();
            methods.Add(caller);
            methodsTimeCalled.Add(Time.time);
        }
        public static void LogMethodCallEnd()
        {
            if (!logMethodCalls)
                return;
            MethodBase caller = new StackFrame(1, false).GetMethod();
            string prefix = caller.ReflectedType.FullName + "." + caller.Name;
            int i = methods.IndexOf(caller);
            Utilities.Log(DateTime.Now.ToString("MM.dd.yyy HH:mm:ss.fff") + "\t" + prefix + " took " + (Time.time-methodsTimeCalled[i]) + "s");
            try
            {
                float totalTimeSpent = (float)methodInfos[prefix];
                methodInfos[prefix] = (Time.time - methodsTimeCalled[i]) + totalTimeSpent;
            }
            catch (KeyNotFoundException) {
                methodInfos.Add(prefix, (Time.time - methodsTimeCalled[i]));
            }
            catch (NullReferenceException)
            {
                methodInfos.Add(prefix, (Time.time - methodsTimeCalled[i]));
            }
            methods.Remove(caller);
            methodsTimeCalled.RemoveAt(i);
        }
        public string GetCallStats()
        {
            string ret = "";
            foreach(string key in methodInfos.Keys)
                ret += key + " for " + methodInfos[key].ToString() + "s\n";
            return ret;
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
            GameObject go = Utilities.GameObjectFind("label_repSource");
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
            GameObject go = Utilities.GameObjectFind("label_rep");
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
            GameObject go = Utilities.GameObjectFind("label_name");
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
            PlayerInfo player = Singleton<PlayerManager>.SP.GetPlayer();
            return (string)player.GetType().GetField(ObfuscationHandler.GetObfuscated("playerName")).GetValue(player);
            //return player.playerName;
        }

        public string GetNameFromPlayerInfo(PlayerInfo pi)
        {
            return (string)pi.GetType().GetField(ObfuscationHandler.GetObfuscated("playerName")).GetValue(pi);
            //return pi.playerName;
        }
        public string GetIdFromPlayerInfo(PlayerInfo pi)
        {
            return (string)pi.GetType().GetField("userID").GetValue(pi);
            //return pi.userID;
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
            if (Utilities.GameObjectFindObjectOfType<FollowCamSystem>().GetCameras().Count == 0)
            {
                // if there are no cameras, do a custom spectate
                Utilities.GameObjectFindObjectOfType<FollowCamSystem>().subject = null;
                Utilities.GameObjectFindObjectOfType<FollowCamSystem>().bother = false;
                SpectatePlayerFromId(id);
            }
            else
            {
                Utilities.GameObjectFindObjectOfType<FollowCamSystem>().subject = player;
                Utilities.GameObjectFindObjectOfType<FollowCamSystem>().bother = true;
            }
        }
        public void SpectatePlayer(int index)
        {
            Utilities.Log("SpectatePlayer(int " + index.ToString() + ")");
            DevCommandsCamera.Spectate(index);
        }
        public void SpectatePlayerFromId(string id)
        {
            Utilities.Log("SpectatePlayerFromId(string " + id.ToString() + ")");
            PlayerInfo[] allPlayers = GetAllPlayers();
            PlayerInfo player = null;
            foreach (PlayerInfo pi in allPlayers)
            {
                if (GetIdFromPlayerInfo(pi).ToLower() == id.ToLower())
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
        public void SpectatePlayer(string name)
        {
            Utilities.Log("SpectatePlayer(string " + name.ToString() + ")");
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
            string oldBike = Utilities.GameObjectFindObjectOfType<BikeSwitcher>().oldBike;
            if (oldBike == "downhill")
                return 1;
            else if (oldBike == "hardtail")
                return 2;
            return 0;
        }
        public void SetBike(int steam_id, int bike)
        {
            PlayerInfoImpact[] array = FindObjectsOfType<PlayerInfoImpact>();
            PlayerCustomization[] array2 = FindObjectsOfType<PlayerCustomization>();
            GameData gameData = Utilities.GameObjectFindObjectOfType<GameData>();
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
        public void SetBike(int bike)
        {
            UI_BikeSelection ui_BikeSelection = new UI_BikeSelection();
            try
            {
                ui_BikeSelection.HoverButtonBike(bike);
            }
            catch (Exception){}
            Destroy(ui_BikeSelection);
        }
        public void SetBike(PlayerInfoImpact playerInfoImpact, int bike)
        {
            // bikeType = GameData.SP.bikeTypes[num];
            //dzQf\u0082nw = GameData.[~qsVD|.bx}n\u0080PQ[cVpqe^E];

            // playerInfoImpact.bikeType = Utilities.GameObjectFindObjectOfType<GameData>().bikeTypes[bike];
            BikeType[] bikeTypes = (BikeType[])typeof(GameData).GetField(ObfuscationHandler.GetObfuscated("bikeTypes")).GetValue(Utilities.GameObjectFindObjectOfType<GameData>());
            typeof(PlayerInfoImpact).GetField(ObfuscationHandler.GetObfuscated("bikeType")).SetValue(playerInfoImpact, bikeTypes[bike]);

            // playerCustomization = hUP\u007fi\u0084d
            PlayerCustomization playerCustomization = (PlayerCustomization)typeof(PlayerInfoImpact).GetField(ObfuscationHandler.GetObfuscated("playerCustomization")).GetValue(playerInfoImpact);
            playerCustomization.RefreshBikeMesh();
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
        public int GetBikeInt(string bike)
        {
            if (bike.ToLower() == "downhill")
                return 1;
            if (bike.ToLower() == "hardtail")
                return 2;
            return 0;
        }
        public void PopUp(string titleText, string bodyText)
        {
            int lineLimit = 12;
            int lines = 0;
            foreach(char c in bodyText)
                if (c == '\n')
                    lines++;
            if (lines > lineLimit)
                return;
            UI_PopUp_TextBoxSmall uI_PopUp_TextBoxSmall = Utilities.GameObjectFindObjectOfType<PermaGUI>().SpawnPopUp<UI_PopUp_TextBoxSmall>();
            // label_titleText = f`r}tXQ
            TMPro.TextMeshProUGUI label_titleText = (TMPro.TextMeshProUGUI)uI_PopUp_TextBoxSmall.GetType().GetField(ObfuscationHandler.GetObfuscated("label_titleText")).GetValue(uI_PopUp_TextBoxSmall);
            label_titleText.text = titleText;
            // label_bodyText = oZtLHbT
            TMPro.TextMeshProUGUI label_bodyText = (TMPro.TextMeshProUGUI)uI_PopUp_TextBoxSmall.GetType().GetField(ObfuscationHandler.GetObfuscated("label_bodyText")).GetValue(uI_PopUp_TextBoxSmall);
            label_bodyText.text = bodyText;
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
            Utilities.GameObjectFind("BendGoal_Left").SetActive(false);
            Utilities.GameObjectFind("BendGoal_Right").SetActive(false);
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
            MultiManager mm = FindObjectOfType<MultiManager>();
            mm.SwitchToSpectate();
            Utilities.Log("utilities.SwitchSpectate");
        }
        public void ToggleSpectator()
        {
            MultiManager mm = Utilities.GameObjectFindObjectOfType<MultiManager>();
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