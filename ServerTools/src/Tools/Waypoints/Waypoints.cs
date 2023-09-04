using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    class Waypoints
    {
        public static bool IsEnabled = false, IsRunning = false, Player_Check = false, Zombie_Check = false, Vehicle = false, Public_Waypoints = false, No_POI = false;
        public static int Delay_Between_Uses = 0, Max_Waypoints = 2, Reserved_Max_Waypoints = 4, Command_Cost = 0;
        public static string Command_go_way = "go way", Command_waypoint = "waypoint", Command_way = "way", Command_wp = "wp",
            Command_fwaypoint = "fwaypoint", Command_fway = "fway", Command_fwp = "fwp",
            Command_setwaypoint = "setwaypoint", Command_waypoint_save = "waypoint save", Command_way_save = "way save", Command_ws = "ws",
            Command_waypoint_delete = "waypoint delete", Command_way_delete = "way delete", Command_wd = "wd";

        public static Dictionary<int, DateTime> Invite = new Dictionary<int, DateTime>();
        public static Dictionary<int, string> FriendPosition = new Dictionary<int, string>();
        public static Dictionary<string, string[]> Dict = new Dictionary<string, string[]>();

        private const string file = "Waypoints.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        public static void Unload()
        {
            Dict.Clear();
            FileWatcher.Dispose();
            IsRunning = false;
        }

        public static void LoadXml()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    UpdateXml();
                }
                XmlDocument xmlDoc = new XmlDocument();
                try
                {
                    xmlDoc.Load(FilePath);
                }
                catch (XmlException e)
                {
                    Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", file, e.Message));
                    return;
                }
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                Dict.Clear();
                if (childNodes != null && (childNodes[0] != null && childNodes[0].OuterXml.Contains("Version") && childNodes[0].OuterXml.Contains(Config.Version)))
                {
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        if (childNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement line = (XmlElement)childNodes[i];
                        if (!line.HasAttributes || !line.HasAttribute("Name") || !line.HasAttribute("Position") || !line.HasAttribute("Cost"))
                        {
                            continue;
                        }
                        string name = line.GetAttribute("Name");
                        if (name == "")
                        {
                            continue;
                        }
                        string position = line.GetAttribute("Position");
                        string cost = line.GetAttribute("Cost");
                        if (!int.TryParse(cost, out int value))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring Waypoints.xml entry. Invalid (non-numeric) value for 'Cost' attribute: {0}", line.OuterXml));
                            continue;
                        }
                        if (!position.Contains(","))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring Waypoints.xml entry. Invalid value for 'Position' attribute: {0}", line.OuterXml));
                            continue;
                        }
                        string[] waypoint = { position, cost };
                        if (!Dict.ContainsKey(name))
                        {
                            Dict.Add(name, waypoint);
                        }
                    }
                }
                else
                {
                    XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                    if (nodeList != null)
                    {
                        Timers.UpgradeWaypointsXml(nodeList);
                        //UpgradeXml(nodeList);
                        return;
                    }
                    File.Delete(FilePath);
                    UpdateXml();
                    return;
                }
            }
            catch (Exception e)
            {
                if (e.Message == "Specified cast is not valid.")
                {
                    File.Delete(FilePath);
                    UpdateXml();
                }
                else
                {
                    Log.Out(string.Format("[SERVERTOOLS] Error in Waypoints.LoadXml: {0}", e.Message));
                }
            }
        }

        public static void UpdateXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<PublicWaypoints>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- <Waypoint Name=\"Example\" Position=\"-500,20,500\" Cost=\"150\" /> -->");
                    if (Dict.Count > 0)
                    {
                        foreach (KeyValuePair<string, string[]> kvp in Dict)
                        {
                            sw.WriteLine(string.Format("    <Waypoint Name=\"{0}\" Position=\"{1}\" Cost=\"{2}\" />", kvp.Key, kvp.Value[0], kvp.Value[1]));
                        }
                    }
                    sw.WriteLine("</PublicWaypoints>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Waypoints.UpdateXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
        }

        private static void InitFileWatcher()
        {
            FileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.Created += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.EnableRaisingEvents = true;
            IsRunning = true;
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            if (!File.Exists(FilePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        public static void List(ClientInfo _cInfo)
        {
            try
            {
                if (ReservedSlots.IsEnabled)
                {
                    if (ReservedSlots.Dict.ContainsKey(_cInfo.PlatformId.CombinedString) || ReservedSlots.Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                    {
                        if (ReservedSlots.Dict.TryGetValue(_cInfo.PlatformId.CombinedString, out DateTime dt))
                        {
                            if (DateTime.Now < dt)
                            {
                                ListResult(_cInfo, Reserved_Max_Waypoints);
                                return;
                            }
                        }
                        else if (ReservedSlots.Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out dt))
                        {
                            if (DateTime.Now < dt)
                            {
                                ListResult(_cInfo, Reserved_Max_Waypoints);
                                return;
                            }
                        }
                    }
                }
                ListResult(_cInfo, Max_Waypoints);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Waypoints.List: {0}", e.Message));
            }
        }

        public static void ListResult(ClientInfo _cInfo, int _waypointLimit)
        {
            try
            {
                Dictionary<string, string> waypoints = new Dictionary<string, string>();
                if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Waypoints != null)
                {
                    waypoints = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Waypoints;
                }
                if (waypoints.Count > 0)
                {
                    int count = _waypointLimit + PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].WaypointSpots;
                    var waypointList = waypoints.ToArray();
                    for (int i = 0; i < waypointList.Length; i++)
                    {
                        if (i <= count)
                        {
                            Phrases.Dict.TryGetValue("Waypoints12", out string phrase);
                            phrase = phrase.Replace("{Name}", waypointList[i].Key);
                            phrase = phrase.Replace("{Position}", waypointList[i].Value);
                            phrase = phrase.Replace("{Cost}", Command_Cost.ToString());
                            phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    if (Public_Waypoints && Dict.Count > 0)
                    {
                        var waypoint = Dict.ToArray();
                        for (int i = 0; i < waypoint.Length; i++)
                        {
                            Phrases.Dict.TryGetValue("Waypoints12", out string phrase);
                            phrase = phrase.Replace("{Name}", waypoint[i].Key);
                            phrase = phrase.Replace("{Position}", waypoint[i].Value[0]);
                            phrase = phrase.Replace("{Cost}", waypoint[i].Value[1]);
                            phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
                else if (Public_Waypoints && Dict.Count > 0)
                {
                    var waypoint = Dict.ToArray();
                    for (int i = 0; i < waypoint.Length; i++)
                    {
                        Phrases.Dict.TryGetValue("Waypoints12", out string phrase);
                        phrase = phrase.Replace("{Name}", waypoint[i].Key);
                        phrase = phrase.Replace("{Position}", waypoint[i].Value[0]);
                        phrase = phrase.Replace("{Cost}", waypoint[i].Value[1]);
                        phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Waypoints19", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Waypoints.ListResult: {0}", e.Message));
            }
        }

        public static void TeleDelay(ClientInfo _cInfo, string _waypoint, bool _friends)
        {
            try
            {
                if (!Event.Teams.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                {
                    if (Delay_Between_Uses < 1)
                    {
                        if ((PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Waypoints != null && 
                            PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Waypoints.ContainsKey(_waypoint)) ||
                            Dict.ContainsKey(_waypoint))
                        {
                            Checks(_cInfo, _waypoint, _friends);
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("Waypoints9", out string _phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastWaypoint != null)
                        {
                            DateTime lastWaypoint = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastWaypoint;
                            TimeSpan varTime = DateTime.Now - lastWaypoint;
                            double fractionalMinutes = varTime.TotalMinutes;
                            int timepassed = (int)fractionalMinutes;
                            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.PlatformId.CombinedString) || ReservedSlots.Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                                {
                                    if (ReservedSlots.Dict.TryGetValue(_cInfo.PlatformId.CombinedString, out DateTime dt))
                                    {
                                        if (DateTime.Now < dt)
                                        {
                                            int delay = Delay_Between_Uses / 2;
                                            Time(_cInfo, _waypoint, timepassed, delay, _friends);
                                            return;
                                        }
                                    }
                                    else if (ReservedSlots.Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out dt))
                                    {
                                        if (DateTime.Now < dt)
                                        {
                                            int delay = Delay_Between_Uses / 2;
                                            Time(_cInfo, _waypoint, timepassed, delay, _friends);
                                            return;
                                        }
                                    }
                                }
                            }
                            if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ReducedDelay)
                            {
                                int delay = Delay_Between_Uses / 2;
                                Time(_cInfo, _waypoint, timepassed, delay, _friends);
                                return;
                            }
                            Time(_cInfo, _waypoint, timepassed, Delay_Between_Uses, _friends);
                        }
                        else
                        {
                            if ((PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Waypoints != null && PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Waypoints.ContainsKey(_waypoint)) ||
                            Dict.ContainsKey(_waypoint))
                            {
                                Checks(_cInfo, _waypoint, false);
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("Waypoints9", out string phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Waypoints13", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Waypoints.TeleDelay: {0}", e.Message));
            }
        }

        private static void Time(ClientInfo _cInfo, string _waypoint, int _timepassed, int _delay, bool _friends)
        {
            try
            {
                if (_timepassed >= _delay)
                {
                    Checks(_cInfo, _waypoint, _friends);
                }
                else
                {
                    int timeleft = _delay - _timepassed;
                    Phrases.Dict.TryGetValue("Waypoints1", out string phrase);
                    phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    phrase = phrase.Replace("{DelayBetweenUses}", _delay.ToString());
                    phrase = phrase.Replace("{Value}", timeleft.ToString());
                    phrase = phrase.Replace("{Command_waypoint}", Command_waypoint);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Waypoints.Time: {0}", e.Message));
            }
        }

        private static void Checks(ClientInfo _cInfo, string _waypoint, bool _friends)
        {
            try
            {
                EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
                if (player != null)
                {
                    if (Vehicle)
                    {
                        Entity attachedEntity = player.AttachedToEntity;
                        if (attachedEntity != null && attachedEntity is EntityVehicle)
                        {
                            Phrases.Dict.TryGetValue("Teleport3", out string phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return;
                        }
                    }
                    if (Player_Check)
                    {
                        if (Teleportation.PCheck(_cInfo, player))
                        {
                            return;
                        }
                    }
                    if (Zombie_Check)
                    {
                        if (Teleportation.ZCheck(_cInfo, player))
                        {
                            return;
                        }
                    }
                    if (No_POI)
                    {
                        if (GameManager.Instance.World.IsPositionWithinPOI(player.position, 3))
                        {
                            Phrases.Dict.TryGetValue("Waypoints6", out string phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return;
                        }
                    }
                    Vector3 position = player.GetPosition();
                    int x = (int)position.x;
                    int y = (int)position.y;
                    int z = (int)position.z;
                    Vector3i vec3i = new Vector3i(x, y, z);
                    if (Dict.ContainsKey(_waypoint))
                    {
                        Dict.TryGetValue(_waypoint, out string[] waypointData);
                        int.TryParse(waypointData[1], out int cost);
                        CommandCost(_cInfo, _waypoint, position, _friends, cost);
                    }
                    else
                    {
                        CommandCost(_cInfo, _waypoint, position, _friends, Command_Cost);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Waypoints.Checks: {0}", e.Message));
            }
        }       

        private static void CommandCost(ClientInfo _cInfo, string _waypoint, Vector3 _position, bool _friends, int _cost)
        {
            try
            {
                int currency = 0, bankCurrency = 0, cost = Command_Cost;
                if (Wallet.IsEnabled)
                {
                    currency = Wallet.GetCurrency(_cInfo.CrossplatformId.CombinedString);
                }
                if (Bank.IsEnabled && Bank.Direct_Payment)
                {
                    bankCurrency = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Bank;
                }
                if (currency + bankCurrency >= cost)
                {
                    if (currency > 0)
                    {
                        if (currency < cost)
                        {
                            Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, currency);
                            cost -= currency;
                            Bank.SubtractCurrencyFromBank(_cInfo.CrossplatformId.CombinedString, cost);
                        }
                        else
                        {
                            Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, cost);
                        }
                    }
                    else
                    {
                        Bank.SubtractCurrencyFromBank(_cInfo.CrossplatformId.CombinedString, cost);
                    }
                    Exec(_cInfo, _waypoint, _position, _friends, _cost);
                }
                else
                {
                    Phrases.Dict.TryGetValue("Waypoints14", out string phrase);
                    phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Waypoints.CommandCost: {0}", e.Message));
            }
        }

        private static void Exec(ClientInfo _cInfo, string _waypointName, Vector3 _position, bool _friends, int _cost)
        {
            try
            {
                string waypointPosition = "";
                if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Waypoints != null && 
                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Waypoints.ContainsKey(_waypointName))
                {
                    Dictionary<string, string> waypoints = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Waypoints;
                    waypoints.TryGetValue(_waypointName, out waypointPosition);
                }
                else if (Dict.ContainsKey(_waypointName))
                {
                    Dict.TryGetValue(_waypointName, out string[] waypointData);
                    waypointPosition = waypointData[0];
                }
                else
                {
                    Phrases.Dict.TryGetValue("Waypoints4", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
                string[] cords = waypointPosition.Split(',');
                int.TryParse(cords[0], out int x);
                int.TryParse(cords[1], out int y);
                int.TryParse(cords[2], out int z);
                if (GeneralOperations.ClaimedByWho(_cInfo.CrossplatformId, new Vector3i(_position)) == EnumLandClaimOwner.None)
                {
                    if (_friends)
                    {
                        FriendInvite(_cInfo, _position, waypointPosition);
                    }
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastWaypoint = DateTime.Now;
                    PersistentContainer.DataChange = true;
                    return;
                }
                else
                {
                    Phrases.Dict.TryGetValue("Waypoints2", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Waypoints.Exec: {0}", e.Message));
            }
        }

        public static void SaveClaimCheck(ClientInfo _cInfo, string _waypoint)
        {
            try
            {
                if (!Event.Teams.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                {
                    World world = GameManager.Instance.World;
                    EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
                    if (player != null)
                    {
                        EnumLandClaimOwner claimOwner;
                        Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos((int)player.position.x, (int)player.position.y, (int)player.position.z);
                        if (chunk == null)
                        {
                            claimOwner = EnumLandClaimOwner.None;
                        }
                        else
                        {
                            claimOwner = GeneralOperations.ClaimedByWho(_cInfo.CrossplatformId, new Vector3i(player.position));
                        }
                        if (claimOwner == EnumLandClaimOwner.None)
                        {
                            ReservedCheck(_cInfo, _waypoint);
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("Waypoints10", out string phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Waypoints13", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Waypoints.SaveClaimCheck: {0}", e.Message));
            }
        }

        private static void ReservedCheck(ClientInfo _cInfo, string _waypoint)
        {
            try
            {
                if (ReservedSlots.IsEnabled && ReservedSlots.Dict.ContainsKey(_cInfo.PlatformId.CombinedString) || ReservedSlots.Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                {
                    if (ReservedSlots.Dict.TryGetValue(_cInfo.PlatformId.CombinedString, out DateTime dt))
                    {
                        if (DateTime.Now < dt)
                        {
                            SavePoint(_cInfo, _waypoint, Reserved_Max_Waypoints);
                            return;
                        }
                    }
                    else if (ReservedSlots.Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out dt))
                    {
                        if (DateTime.Now < dt)
                        {
                            SavePoint(_cInfo, _waypoint, Reserved_Max_Waypoints);
                            return;
                        }
                    }
                }
                SavePoint(_cInfo, _waypoint, Max_Waypoints);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Waypoints.ReservedCheck: {0}", e.Message));
            }
        }

        private static void SavePoint(ClientInfo _cInfo, string _waypoint, int _waypointTotal)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_waypoint))
                {
                    Phrases.Dict.TryGetValue("Waypoints11", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
                if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Waypoints != null)
                {
                    if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Waypoints.Count < _waypointTotal + PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].WaypointSpots)
                    {
                        EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
                        if (player != null)
                        {
                            Vector3 position = player.GetPosition();
                            int x = (int)position.x;
                            int y = (int)position.y;
                            int z = (int)position.z;
                            string wposition = x + "," + y + "," + z;
                            if (!PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Waypoints.ContainsKey(_waypoint))
                            {
                                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Waypoints.Add(_waypoint, wposition);
                                PersistentContainer.DataChange = true;
                                Phrases.Dict.TryGetValue("Waypoints8", out string phrase);
                                phrase = phrase.Replace("{Name}", _waypoint);
                                phrase = phrase.Replace("{Position}", wposition);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("Waypoints15", out string phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Waypoints5", out string phrase);
                        phrase = phrase.Replace("{Value}", _waypointTotal.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
                    if (player != null)
                    {
                        Vector3 position = player.GetPosition();
                        int x = (int)position.x;
                        int y = (int)position.y;
                        int z = (int)position.z;
                        string wposition = x + "," + y + "," + z;
                        Dictionary<string, string> waypoints = new Dictionary<string, string>();
                        waypoints.Add(_waypoint, wposition);
                        PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Waypoints = waypoints;
                        PersistentContainer.DataChange = true;
                        Phrases.Dict.TryGetValue("Waypoints8", out string _phrase);
                        _phrase = _phrase.Replace("{Name}", _waypoint);
                        _phrase = _phrase.Replace("{Position}", wposition);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Waypoints.SavePoint: {0}", e.Message));
            }
        }

        public static void DelPoint(ClientInfo _cInfo, string _waypoint)
        {
            try
            {
                if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Waypoints != null && PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Waypoints.ContainsKey(_waypoint))
                {
                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Waypoints.Remove(_waypoint);
                    PersistentContainer.DataChange = true;
                    Phrases.Dict.TryGetValue("Waypoints7", out string phrase);
                    phrase = phrase.Replace("{Name}", _waypoint);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    Phrases.Dict.TryGetValue("Waypoints4", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Waypoints.DelPoint: {0}", e.Message));
            }
        }

        public static void FriendInvite(ClientInfo _cInfo, Vector3 _position, string _destination)
        {
            try
            {
                int x = (int)_position.x;
                int y = (int)_position.y;
                int z = (int)_position.z;
                EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
                if (player != null)
                {
                    List<ClientInfo> clientList = GeneralOperations.ClientList();
                    if (clientList != null)
                    {
                        for (int i = 0; i < clientList.Count; i++)
                        {
                            ClientInfo cInfo2 = clientList[i];
                            EntityPlayer player2 = GeneralOperations.GetEntityPlayer(cInfo2.entityId);
                            if (player2 != null)
                            {
                                if (player.IsFriendsWith(player2))
                                {
                                    if ((x - (int)player2.position.x) * (x - (int)player2.position.x) + (z - (int)player2.position.z) * (z - (int)player2.position.z) <= 10 * 10)
                                    {
                                        Phrases.Dict.TryGetValue("Waypoints16", out string phrase);
                                        phrase = phrase.Replace("{PlayerName}", _cInfo.playerName);
                                        phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                                        phrase = phrase.Replace("{Command_go_way}", Command_go_way);
                                        ChatHook.ChatMessage(cInfo2, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                        Phrases.Dict.TryGetValue("Waypoints17", out phrase);
                                        phrase = phrase.Replace("{PlayerName}", cInfo2.playerName);
                                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                        if (Invite.ContainsKey(cInfo2.entityId))
                                        {
                                            Invite.Remove(cInfo2.entityId);
                                            FriendPosition.Remove(cInfo2.entityId);
                                        }
                                        Invite.Add(cInfo2.entityId, DateTime.Now);
                                        FriendPosition.Add(cInfo2.entityId, _destination);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Waypoints.FriendInvite: {0}", e.Message));
            }
        }

        public static void FriendWaypoint(ClientInfo _cInfo)
        {
            try
            {
                Invite.TryGetValue(_cInfo.entityId, out DateTime dt);
                {
                    TimeSpan varTime = DateTime.Now - dt;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int timepassed = (int)fractionalMinutes;
                    if (timepassed <= 2)
                    {
                        FriendPosition.TryGetValue(_cInfo.entityId, out string pos);
                        {
                            string[] cords = pos.Split(',');
                            int.TryParse(cords[0], out int x);
                            int.TryParse(cords[1], out int y);
                            int.TryParse(cords[2], out int z);
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                            Invite.Remove(_cInfo.entityId);
                            FriendPosition.Remove(_cInfo.entityId);
                        }
                    }
                    else
                    {
                        Invite.Remove(_cInfo.entityId);
                        FriendPosition.Remove(_cInfo.entityId);
                        Phrases.Dict.TryGetValue("Waypoints18", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Waypoints.FriendWaypoint: {0}", e.Message));
            }
        }

        public static void UpgradeXml(XmlNodeList nodeList)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                File.Delete(FilePath);
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<PublicWaypoints>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- <Waypoint Name=\"Example\" Position=\"-500,20,500\" Cost=\"150\" /> -->");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType == XmlNodeType.Comment)
                        {
                            if (!nodeList[i].OuterXml.Contains("<!-- <Waypoint Name=\"Example") && !nodeList[i].OuterXml.Contains("<!-- Do not forget") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Version") && !nodeList[i].OuterXml.Contains("<Waypoint Name=\"\""))
                            {
                                sw.WriteLine(nodeList[i].OuterXml);
                            }
                        }
                    }
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)nodeList[i];
                            if (line.HasAttributes && line.Name == "Waypoint")
                            {
                                string name = "", position = "", cost = "";
                                if (line.HasAttribute("Name"))
                                {
                                    name = line.GetAttribute("Name");
                                }
                                if (line.HasAttribute("Position"))
                                {
                                    position = line.GetAttribute("Position");
                                }
                                if (line.HasAttribute("Cost"))
                                {
                                    cost = line.GetAttribute("Cost");
                                }
                                sw.WriteLine(string.Format("    <Waypoint Name=\"{0}\" Position=\"{1}\" Cost=\"{2}\" />", name, position, cost));
                            }
                        }
                    }
                    sw.WriteLine("</PublicWaypoints>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Waypoints.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}
