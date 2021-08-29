using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    class Waypoints
    {
        public static bool IsEnabled = false, Player_Check = false, Zombie_Check = false, Vehicle = false, Public_Waypoints = false, IsRunning = false;
        public static int Delay_Between_Uses = 0, Max_Waypoints = 2, Reserved_Max_Waypoints = 4, Command_Cost = 0;
        public static string Command_go_way = "go way", Command_waypoint = "waypoint", Command_way = "way", Command_wp = "wp", Command_fwaypoint = "fwaypoint", Command_fway = "fway", Command_fwp = "fwp", 
            Command_waypoint_save = "waypoint save", Command_way_save = "way save", Command_ws = "ws", Command_waypoint_del = "waypoint del", Command_way_del = "way del", Command_wd = "wd";
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
                if (!Utils.FileExists(FilePath))
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
                XmlNodeList _childNodes = xmlDoc.DocumentElement.ChildNodes;
                if (_childNodes != null && _childNodes.Count > 0)
                {
                    Dict.Clear();
                    for (int i = 0; i < _childNodes.Count; i++)
                    {
                        if (_childNodes[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement _line = (XmlElement)_childNodes[i];
                            if (_line.HasAttributes)
                            {
                                if (_line.HasAttribute("Version") && _line.GetAttribute("Version") != Config.Version)
                                {
                                    UpgradeXml(_childNodes);
                                    return;
                                }
                                else if (_line.HasAttribute("Name") && _line.HasAttribute("Position") && _line.HasAttribute("Cost"))
                                {
                                    string _name = _line.GetAttribute("Name");
                                    string _position = _line.GetAttribute("Position");
                                    string _cost = _line.GetAttribute("Cost");
                                    if (!int.TryParse(_cost, out int _value))
                                    {
                                        Log.Out(string.Format("[SERVERTOOLS] Ignoring Waypoints.xml entry. Invalid (non-numeric) value for 'Cost' attribute: {0}", _line.OuterXml));
                                        continue;
                                    }
                                    if (!_position.Contains(","))
                                    {
                                        Log.Out(string.Format("[SERVERTOOLS] Ignoring Waypoints.xml entry. Invalid value for 'Position' attribute: {0}", _line.OuterXml));
                                        continue;
                                    }
                                    string[] _waypoint = { _position, _cost };
                                    if (!Dict.ContainsKey(_name))
                                    {
                                        Dict.Add(_name, _waypoint);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Waypoints.LoadXml: {0}", e.Message));
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
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("<!-- <Waypoint Name=\"Example\" Position=\"-500,20,500\" Cost=\"150\" /> -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    if (Dict.Count > 0)
                    {
                        foreach (KeyValuePair<string, string[]> kvp in Dict)
                        {
                            sw.WriteLine(string.Format("    <Waypoint Name=\"{0}\" Position=\"{1}\" Cost=\"{2}\" />", kvp.Key, kvp.Value[0], kvp.Value[1]));
                        }
                    }
                    else
                    {
                        sw.WriteLine("    <!-- <Waypoint Name=\"\" Position=\"\" Cost=\"\" /> -->");
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
            if (!Utils.FileExists(FilePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        private static void UpgradeXml(XmlNodeList _oldChildNodes)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<PublicWaypoints>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("<!-- <Waypoint Name=\"Example\" Position=\"-500,20,500\" Cost=\"150\" /> -->");
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType == XmlNodeType.Comment && !_oldChildNodes[i].OuterXml.StartsWith("<!-- <Waypoint Name=\"Example\"") && 
                            !_oldChildNodes[i].OuterXml.StartsWith("    <!-- <Waypoint Name=\"\""))
                        {
                            sw.WriteLine(_oldChildNodes[i].OuterXml);
                        }
                    }
                    sw.WriteLine();
                    sw.WriteLine();
                    bool _blank = true;
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement _line = (XmlElement)_oldChildNodes[i];
                        if (_line.HasAttributes && _line.Name == "Waypoint")
                        {
                            _blank = false;
                            string _name = "", _position = "", _cost = "";
                            if (_line.HasAttribute("Name"))
                            {
                                _name = _line.GetAttribute("Name");
                            }
                            if (_line.HasAttribute("Position"))
                            {
                                _position = _line.GetAttribute("Position");
                            }
                            if (_line.HasAttribute("Cost"))
                            {
                                _cost = _line.GetAttribute("Cost");
                            }
                            sw.WriteLine(string.Format("    <Waypoint Name=\"{0}\" Position=\"{1}\" Cost=\"{2}\" />", _name, _position, _cost));
                        }
                    }
                    if (_blank)
                    {
                        sw.WriteLine("    <!-- <Waypoint Name=\"\" Position=\"\" Cost=\"\" /> -->");
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

        public static void List(ClientInfo _cInfo)
        {
            try
            {
                if (ReservedSlots.IsEnabled && ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                {
                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out DateTime _dt);
                    if (DateTime.Now < _dt)
                    {
                        ListResult(_cInfo, Reserved_Max_Waypoints);
                        return;
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
                Dictionary<string, string> _waypoints = new Dictionary<string, string>();
                if (PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints != null)
                {
                    _waypoints = PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints;
                }
                
                int _count = 0;
                if (_waypoints.Count > 0)
                {
                    foreach (var _waypoint in _waypoints)
                    {
                        _count += 1;
                        if (_count <= _waypointLimit)
                        {
                            Phrases.Dict.TryGetValue("Waypoints12", out string _phrase);
                            _phrase = _phrase.Replace("{Name}", _waypoint.Key);
                            _phrase = _phrase.Replace("{Position}", _waypoint.Value);
                            _phrase = _phrase.Replace("{Cost}", Command_Cost.ToString());
                            _phrase = _phrase.Replace("{CoinName}", Wallet.Coin_Name);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    if (Public_Waypoints && PersistentContainer.Instance.Waypoints != null && PersistentContainer.Instance.Waypoints.Count > 0)
                    {
                        foreach (var _waypoint in _waypoints)
                        {
                            Phrases.Dict.TryGetValue("Waypoints12", out string _phrase);
                            _phrase = _phrase.Replace("{Name}", _waypoint.Key);
                            _phrase = _phrase.Replace("{Position}", _waypoint.Value);
                            _phrase = _phrase.Replace("{Cost}", Command_Cost.ToString());
                            _phrase = _phrase.Replace("{CoinName}", Wallet.Coin_Name);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
                else if (Public_Waypoints && PersistentContainer.Instance.Waypoints != null && PersistentContainer.Instance.Waypoints.Count > 0)
                {
                    foreach (var _waypoint in _waypoints)
                    {
                        Phrases.Dict.TryGetValue("Waypoints12", out string _phrase);
                        _phrase = _phrase.Replace("{Name}", _waypoint.Key);
                        _phrase = _phrase.Replace("{Position}", _waypoint.Value);
                        _phrase = _phrase.Replace("{Cost}", Command_Cost.ToString());
                        _phrase = _phrase.Replace("{CoinName}", Wallet.Coin_Name);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Waypoints19", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                if (!Event.Teams.ContainsKey(_cInfo.playerId))
                {
                    if (Delay_Between_Uses < 1)
                    {
                        if ((PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints != null && PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints.ContainsKey(_waypoint)) ||
                            (PersistentContainer.Instance.Waypoints != null && PersistentContainer.Instance.Waypoints.ContainsKey(_waypoint)))
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
                        if (PersistentContainer.Instance.Players[_cInfo.playerId].LastWaypoint != null)
                        {
                            DateTime _lastWaypoint = PersistentContainer.Instance.Players[_cInfo.playerId].LastWaypoint;
                            TimeSpan varTime = DateTime.Now - _lastWaypoint;
                            double fractionalMinutes = varTime.TotalMinutes;
                            int _timepassed = (int)fractionalMinutes;
                            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out DateTime _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        int _delay = Delay_Between_Uses / 2;
                                        Time(_cInfo, _waypoint, _timepassed, _delay, _friends);
                                        return;
                                    }
                                }
                            }
                            Time(_cInfo, _waypoint, _timepassed, Delay_Between_Uses, _friends);
                        }
                        else
                        {
                            if ((PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints != null && PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints.ContainsKey(_waypoint)) ||
                            (PersistentContainer.Instance.Waypoints != null && PersistentContainer.Instance.Waypoints.ContainsKey(_waypoint)))
                            {
                                Checks(_cInfo, _waypoint, false);
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("Waypoints9", out string _phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Waypoints13", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                    int _timeleft = _delay - _timepassed;
                    Phrases.Dict.TryGetValue("Waypoints1", out string _phrase);
                    _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    _phrase = _phrase.Replace("{DelayBetweenUses}", _delay.ToString());
                    _phrase = _phrase.Replace("{Value}", _timeleft.ToString());
                    _phrase = _phrase.Replace("{Command_waypoint}", Command_waypoint);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                EntityPlayer _player = PersistentOperations.GetEntityPlayer(_cInfo.playerId);
                if (_player != null)
                {
                    if (Vehicle)
                    {
                        Entity _attachedEntity = _player.AttachedToEntity;
                        if (_attachedEntity != null)
                        {
                            Phrases.Dict.TryGetValue("Teleport3", out string _phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return;
                        }
                    }
                    if (Player_Check)
                    {
                        if (Teleportation.PCheck(_cInfo, _player))
                        {
                            return;
                        }
                    }
                    if (Zombie_Check)
                    {
                        if (Teleportation.ZCheck(_cInfo, _player))
                        {
                            return;
                        }
                    }
                    Vector3 _position = _player.GetPosition();
                    int _x = (int)_position.x;
                    int _y = (int)_position.y;
                    int _z = (int)_position.z;
                    Vector3i _vec3i = new Vector3i(_x, _y, _z);
                    if (PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints != null && PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints.ContainsKey(_waypoint))
                    {
                        CommandCost(_cInfo, _waypoint, _position, _friends, Command_Cost);
                    }
                    else if (PersistentContainer.Instance.Waypoints != null && PersistentContainer.Instance.Waypoints.ContainsKey(_waypoint))
                    {
                        PersistentContainer.Instance.Waypoints.TryGetValue(_waypoint, out string[] _waypointData);
                        int.TryParse(_waypointData[2], out int _cost);
                        CommandCost(_cInfo, _waypoint, _position, _friends, _cost);
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
                if (Wallet.IsEnabled && _cost >= 1)
                {
                    if (Wallet.GetCurrentCoins(_cInfo.playerId) >= _cost)
                    {
                        Exec(_cInfo, _waypoint, _position, _friends, _cost);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Waypoints14", out string _phrase);
                        _phrase = _phrase.Replace("{CoinName}", Wallet.Coin_Name);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    Exec(_cInfo, _waypoint, _position, _friends, 0);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Waypoints.CommandCost: {0}", e.Message));
            }
        }

        private static void Exec(ClientInfo _cInfo, string _waypoint, Vector3 _position, bool _friends, int _cost)
        {
            try
            {
                if (PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints != null && PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints.ContainsKey(_waypoint))
                {
                    Dictionary<string, string> _waypoints = PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints;
                    _waypoints.TryGetValue(_waypoint, out string _waypointPos);
                    string[] _cords = _waypointPos.Split(',');
                    int.TryParse(_cords[0], out int _x);
                    int.TryParse(_cords[1], out int _y);
                    int.TryParse(_cords[2], out int _z);
                    if (_friends)
                    {
                        FriendInvite(_cInfo, _position, _waypointPos);
                    }
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(_x, _y, _z), null, false));
                    PersistentContainer.Instance.Players[_cInfo.playerId].LastWaypoint = DateTime.Now;
                    PersistentContainer.DataChange = true;
                    if (Wallet.IsEnabled && _cost >= 1)
                    {
                        Wallet.SubtractCoinsFromWallet(_cInfo.playerId, _cost);
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Waypoints4", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                if (!Event.Teams.ContainsKey(_cInfo.playerId))
                {
                    World world = GameManager.Instance.World;
                    EntityPlayer _player = world.Players.dict[_cInfo.entityId];
                    if (_player != null)
                    {
                        Vector3 _position = _player.GetPosition();
                        if (PersistentOperations.ClaimedByNone(_cInfo.playerId, new Vector3i(_position.x, _position.y, _position.z)))
                        {
                            ReservedCheck(_cInfo, _waypoint);
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("Waypoints10", out string _phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Waypoints13", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                if (ReservedSlots.IsEnabled && ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                {
                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out DateTime _dt);
                    if (DateTime.Now < _dt)
                    {
                        SavePoint(_cInfo, _waypoint, Reserved_Max_Waypoints);
                        return;
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
                if (PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints != null && PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints.Count > 0)
                {
                    if (PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints.Count < _waypointTotal)
                    {
                        EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                        if (_player != null)
                        {
                            Vector3 _position = _player.GetPosition();
                            int _x = (int)_position.x;
                            int _y = (int)_position.y;
                            int _z = (int)_position.z;
                            string _wposition = _x + "," + _y + "," + _z;
                            Dictionary<string, string> _waypoints = PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints;
                            if (!_waypoints.ContainsKey(_waypoint))
                            {
                                _waypoints.Add(_waypoint, _wposition);
                                PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints = _waypoints;
                                PersistentContainer.DataChange = true;
                                Phrases.Dict.TryGetValue("Waypoints8", out string _phrase);
                                _phrase = _phrase.Replace("{Name}", _waypoint);
                                _phrase = _phrase.Replace("{Position}", _wposition);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("Waypoints15", out string _phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Waypoints5", out string _phrase);
                        _phrase = _phrase.Replace("{Value}", _waypointTotal.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    if (_player != null)
                    {
                        Dictionary<string, string> _waypoints = new Dictionary<string, string>();
                        Vector3 _position = _player.GetPosition();
                        int _x = (int)_position.x;
                        int _y = (int)_position.y;
                        int _z = (int)_position.z;
                        string _wposition = _x + "," + _y + "," + _z;
                        _waypoints.Add(_waypoint, _wposition);
                        PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints = _waypoints;
                        PersistentContainer.DataChange = true;
                        Phrases.Dict.TryGetValue("Waypoints8", out string _phrase);
                        _phrase = _phrase.Replace("{Name}", _waypoint);
                        _phrase = _phrase.Replace("{Position}", _wposition);
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
                if (PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints != null && PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints.ContainsKey(_waypoint))
                {
                    Dictionary<string, string> _waypoints = PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints;
                    _waypoints.Remove(_waypoint);
                    PersistentContainer.Instance.Players[_cInfo.playerId].Waypoints = _waypoints;
                    PersistentContainer.DataChange = true;
                    Phrases.Dict.TryGetValue("Waypoints7", out string _phrase);
                    _phrase = _phrase.Replace("{Name}", _waypoint);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    Phrases.Dict.TryGetValue("Waypoints4", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                List<ClientInfo> _cInfoList = PersistentOperations.ClientList();
                for (int i = 0; i < _cInfoList.Count; i++)
                {
                    ClientInfo _cInfo2 = _cInfoList[i];
                    EntityPlayer _player2 = GameManager.Instance.World.Players.dict[_cInfo2.entityId];
                    if (_player2 != null)
                    {
                        if (_player.IsFriendsWith(_player2))
                        {
                            if ((x - (int)_player2.position.x) * (x - (int)_player2.position.x) + (z - (int)_player2.position.z) * (z - (int)_player2.position.z) <= 10 * 10)
                            {
                                Phrases.Dict.TryGetValue("Waypoints16", out string _phrase);
                                _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
                                _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                                _phrase = _phrase.Replace("{Command_go_way}", Command_go_way);
                                ChatHook.ChatMessage(_cInfo2, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                Phrases.Dict.TryGetValue("Waypoints17", out _phrase);
                                _phrase = _phrase.Replace("{PlayerName}", _cInfo2.playerName);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                if (Invite.ContainsKey(_cInfo2.entityId))
                                {
                                    Invite.Remove(_cInfo2.entityId);
                                    FriendPosition.Remove(_cInfo2.entityId);
                                }
                                Invite.Add(_cInfo2.entityId, DateTime.Now);
                                FriendPosition.Add(_cInfo2.entityId, _destination);
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
                Invite.TryGetValue(_cInfo.entityId, out DateTime _dt);
                {
                    TimeSpan varTime = DateTime.Now - _dt;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int _timepassed = (int)fractionalMinutes;
                    if (_timepassed <= 2)
                    {
                        FriendPosition.TryGetValue(_cInfo.entityId, out string _pos);
                        {
                            string[] _cords = _pos.Split(',');
                            int.TryParse(_cords[0], out int x);
                            int.TryParse(_cords[1], out int y);
                            int.TryParse(_cords[2], out int z);
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                            Invite.Remove(_cInfo.entityId);
                            FriendPosition.Remove(_cInfo.entityId);
                        }
                    }
                    else
                    {
                        Invite.Remove(_cInfo.entityId);
                        FriendPosition.Remove(_cInfo.entityId);
                        Phrases.Dict.TryGetValue("Waypoints18", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Waypoints.FriendWaypoint: {0}", e.Message));
            }
        }


    }
}
