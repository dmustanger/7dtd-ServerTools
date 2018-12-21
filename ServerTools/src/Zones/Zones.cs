using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    class Zones
    {
        public static bool IsEnabled = false, IsRunning = false, Kill_Enabled = false, Jail_Enabled = false, Kick_Enabled = false,
            Ban_Enabled = false, Zone_Message = false, Set_Home = false, No_Zombie = false;
        public static int Days_Before_Log_Delete = 5, Reminder_Delay = 20;
        
        public static Dictionary<int, DateTime> reminder = new Dictionary<int, DateTime>();
        public static Dictionary<int, string> reminderMsg = new Dictionary<int, string>();
        public static Dictionary<int, string[]> zoneSetup1 = new Dictionary<int, string[]>();
        public static Dictionary<int, bool[]> zoneSetup2 = new Dictionary<int, bool[]>();
        public static Dictionary<int, string> Victim = new Dictionary<int, string>();
        public static Dictionary<int, int> Forgive = new Dictionary<int, int>();
        public static Dictionary<int, string> ZoneExit = new Dictionary<int, string>();
        public static List<string[]> Box1 = new List<string[]>();
        public static List<bool[]> Box2 = new List<bool[]>();
        private static int _xMinCheck = 0, _yMinCheck = 0, _zMinCheck = 0, _xMaxCheck = 0, _yMaxCheck = 0, _zMaxCheck = 0;
        private const string file = "Zones.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        public static void Load()
        {
            if (IsEnabled && !IsRunning)
            {
                LoadXml();
                InitFileWatcher();
            }
        }

        public static void Unload()
        {
            Box1.Clear();
            Box2.Clear();
            fileWatcher.Dispose();
            IsRunning = false;
        }

        public static void LoadXml()
        {
            if (!Utils.FileExists(filePath))
            {
                UpdateXml();
            }
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(filePath);
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", file, e.Message));
                return;
            }
            XmlNode _XmlNode = xmlDoc.DocumentElement;
            foreach (XmlNode childNode in _XmlNode.ChildNodes)
            {
                if (childNode.Name == "Zone")
                {
                    Box1.Clear();
                    Box2.Clear();
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Zone' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("corner1"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because of missing corner1 attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("corner2"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because of missing corner2 attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("entryMessage"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because of missing entryMessage attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("exitMessage"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because of missing exitMessage attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("response"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because of missing response attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("reminderNotice"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because of missing reminderNotice attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("circle"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because of missing circle attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("PvE"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because of missing PvE attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("noZombie"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because of missing noZombie attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        else
                        {
                            string _circle = _line.GetAttribute("circle");
                            bool _result1, _result2, _result3; ;
                            if (!bool.TryParse(_circle, out _result1))
                            {
                                Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because improper true/false for circle attribute: {0}.", subChild.OuterXml));
                                continue;
                            }
                            string _pve = _line.GetAttribute("PvE");
                            if (!bool.TryParse(_pve, out _result2))
                            {
                                Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because improper true/false for PvE attribute: {0}.", subChild.OuterXml));
                                continue;
                            }
                            string _noZ = _line.GetAttribute("noZombie");
                            if (!bool.TryParse(_noZ, out _result3))
                            {
                                Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because improper true/false for noZombie attribute: {0}.", subChild.OuterXml));
                                continue;
                            }

                            string[] box1 = { _line.GetAttribute("corner1"), _line.GetAttribute("corner2"), _line.GetAttribute("entryMessage"), _line.GetAttribute("exitMessage"),
                            _line.GetAttribute("response"), _line.GetAttribute("reminderNotice") };
                            bool[] box2 = { _result1, _result2, _result3 };
                            if (!Box1.Contains(box1))
                            {
                                Box1.Add(box1);
                            }
                            if (!Box2.Contains(box2))
                            {
                                Box2.Add(box2);
                            }
                        }
                    }
                }
            }
        }

        public static void UpdateXml()
        {
            fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<Zones>");
                sw.WriteLine("    <Zone>");
                if (Box1.Count > 0)
                {
                    for (int i = 0; i < Box1.Count; i++)
                    {
                        string[] _box1 = Box1[i];
                        bool[] _box2 = Box2[i];
                        sw.WriteLine(string.Format("        <zone corner1=\"{0}\" corner2=\"{1}\" entryMessage=\"{2}\" exitMessage=\"{3}\" response=\"{4}\" reminderNotice=\"{5}\" circle=\"{6}\" PvE=\"{7}\" noZombie=\"{8}\" />", _box1[0], _box1[1], _box1[2], _box1[3], _box1[4], _box1[5], _box2[0], _box2[1], _box2[2]));
                    }
                }
                else
                {
                    sw.WriteLine("        <zone corner1=\"-8000,-56,8000\" corner2=\"8000,200,0\" entryMessage=\"You are entering the Northern side\" exitMessage=\"You have exited the Northern Side\" response=\"\" reminderNotice=\"You are still in the North\" circle=\"false\" PvE=\"false\" noZombie=\"false\" />");
                    sw.WriteLine("        <zone corner1=\"-8000,-56,-1\" corner2=\"8000,200,-8000\" entryMessage=\"You are entering the Southern side\" exitMessage=\"You have exited the Southern Side\" response=\"\" reminderNotice=\"You are still in the South\" circle=\"false\" PvE=\"false\" noZombie=\"false\" />");
                    sw.WriteLine("        <zone corner1=\"-100,60,-90\" corner2=\"40\" entryMessage=\"You have entered the Market\" exitMessage=\"You have exited the Market\" response=\"say {PlayerName} has entered the market\" reminderNotice=\"\" circle=\"true\" PvE=\"true\" noZombie=\"true\" />");
                    sw.WriteLine("        <zone corner1=\"0,100,0\" corner2=\"25,105,25\" entryMessage=\"You have entered the Lobby\" exitMessage=\"You have exited the Lobby\" response=\"say {PlayerName} has entered the lobby\" reminderNotice=\"You have been in the lobby for a long time...\" circle=\"false\" PvE=\"true\" noZombie=\"true\" />");
                }
                sw.WriteLine("    </Zone>");
                sw.WriteLine("</Zones>");
                sw.Flush();
                sw.Close();
            }
            fileWatcher.EnableRaisingEvents = true;
        }

        private static void InitFileWatcher()
        {
            fileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
            fileWatcher.Created += new FileSystemEventHandler(OnFileChanged);
            fileWatcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            fileWatcher.EnableRaisingEvents = true;
            IsRunning = true;
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            if (!Utils.FileExists(filePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        public static void Check(ClientInfo _cInfoKiller, ClientInfo _cInfoVictim)
        {
            if (ZoneExit.ContainsKey(_cInfoVictim.entityId) || ZoneExit.ContainsKey(_cInfoKiller.entityId))
            {
                if (Players.ZonePvE.Contains(_cInfoVictim.entityId) && Players.ZonePvE.Contains(_cInfoKiller.entityId))
                {
                    string _phrase801;
                    if (!Phrases.Dict.TryGetValue(801, out _phrase801))
                    {
                        _phrase801 = "{PlayerName} has murdered you while you were in a protected zone.";
                    }
                    _phrase801 = _phrase801.Replace("{PlayerName}", _cInfoKiller.playerName);
                    ChatHook.ChatMessage(_cInfoVictim, LoadConfig.Chat_Response_Color + _phrase801 + "[-]", _cInfoKiller.entityId, _cInfoKiller.playerName, EChatType.Whisper, null);
                    string _phrase802;
                    if (!Phrases.Dict.TryGetValue(802, out _phrase802))
                    {
                        _phrase802 = " you have murdered a player inside a protected zone. Their name was {PlayerName}";
                    }
                    _phrase802 = _phrase802.Replace("{PlayerName}", _cInfoVictim.playerName);
                    ChatHook.ChatMessage(_cInfoKiller, LoadConfig.Chat_Response_Color + _phrase802 + "[-]", _cInfoVictim.entityId, _cInfoVictim.playerName, EChatType.Whisper, null);
                    Penalty(_cInfoKiller, _cInfoVictim);
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfoVictim.entityId];
                    int x = (int)_player.position.x;
                    int y = (int)_player.position.y;
                    int z = (int)_player.position.z;
                    string _sposition = x + "," + y + "," + z;
                    if (Victim.ContainsKey(_cInfoVictim.entityId))
                    {
                        Victim[_cInfoVictim.entityId] = _sposition;
                    }
                    else
                    {
                        Victim.Add(_cInfoVictim.entityId, _sposition);
                    }
                }
                if (Players.ZonePvE.Contains(_cInfoVictim.entityId) & !Players.ZonePvE.Contains(_cInfoKiller.entityId))
                {
                    string _phrase801;
                    if (!Phrases.Dict.TryGetValue(801, out _phrase801))
                    {
                        _phrase801 = "{PlayerName} has murdered you while you were in a protected zone.";
                    }
                    _phrase801 = _phrase801.Replace("{PlayerName}", _cInfoKiller.playerName);
                    ChatHook.ChatMessage(_cInfoVictim, LoadConfig.Chat_Response_Color + _phrase801 + "[-]", _cInfoKiller.entityId, _cInfoKiller.playerName, EChatType.Whisper, null);
                    string _phrase802;
                    if (!Phrases.Dict.TryGetValue(802, out _phrase802))
                    {
                        _phrase802 = " you have murdered a player inside a protected zone. Their name was {PlayerName}";
                    }
                    _phrase802 = _phrase802.Replace("{PlayerName}", _cInfoVictim.playerName);
                    ChatHook.ChatMessage(_cInfoKiller, LoadConfig.Chat_Response_Color + _phrase802 + "[-]", _cInfoVictim.entityId, _cInfoVictim.playerName, EChatType.Whisper, null);
                    Penalty(_cInfoKiller, _cInfoVictim);
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfoVictim.entityId];
                    int x = (int)_player.position.x;
                    int y = (int)_player.position.y;
                    int z = (int)_player.position.z;
                    string _sposition = x + "," + y + "," + z;
                    if (Victim.ContainsKey(_cInfoVictim.entityId))
                    {
                        Victim[_cInfoVictim.entityId] = _sposition;
                    }
                    else
                    {
                        Victim.Add(_cInfoVictim.entityId, _sposition);
                    }
                }
                if (!Players.ZonePvE.Contains(_cInfoVictim.entityId) & Players.ZonePvE.Contains(_cInfoKiller.entityId))
                {
                    string _phrase803;
                    if (!Phrases.Dict.TryGetValue(801, out _phrase803))
                    {
                        _phrase803 = "{PlayerName} has murdered you while they were in a protected zone.";
                    }
                    _phrase803 = _phrase803.Replace("{PlayerName}", _cInfoKiller.playerName);
                    ChatHook.ChatMessage(_cInfoVictim, LoadConfig.Chat_Response_Color + _phrase803 + "[-]", _cInfoKiller.entityId, _cInfoKiller.playerName, EChatType.Whisper, null);
                    string _phrase802;
                    if (!Phrases.Dict.TryGetValue(802, out _phrase802))
                    {
                        _phrase802 = " you have murdered a player inside a protected zone. Their name was {PlayerName}";
                    }
                    _phrase802 = _phrase802.Replace("{PlayerName}", _cInfoVictim.playerName);
                    ChatHook.ChatMessage(_cInfoKiller, ChatHook.Player_Name_Color + _cInfoKiller.playerName + LoadConfig.Chat_Response_Color + _phrase802 + "[-]", _cInfoVictim.entityId, _cInfoVictim.playerName, EChatType.Whisper, null);
                    Penalty(_cInfoKiller, _cInfoVictim);
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfoVictim.entityId];
                    int x = (int)_player.position.x;
                    int y = (int)_player.position.y;
                    int z = (int)_player.position.z;
                    string _sposition = x + "," + y + "," + z;
                    if (Victim.ContainsKey(_cInfoVictim.entityId))
                    {
                        Victim[_cInfoVictim.entityId] = _sposition;
                    }
                    else
                    {
                        Victim.Add(_cInfoVictim.entityId, _sposition);
                    }
                    string _file1 = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
                    string _filepath1 = string.Format("{0}/DetectionLogs/{1}", API.GamePath, _file1);
                    using (StreamWriter sw = new StreamWriter(_filepath1, true))
                    {
                        sw.WriteLine(string.Format("Detected {0}, Steam Id {1}, murdered {2}, Steam Id {3} in a protected zone.", _cInfoKiller.playerName, _cInfoKiller.steamId, _cInfoVictim.playerName, _cInfoVictim.steamId));
                        sw.WriteLine();
                        sw.Flush();
                        sw.Close();
                    }
                }
            }
        }

        public static void DetectionLogsDir()
        {
            if (!Directory.Exists(API.GamePath + "/DetectionLogs"))
            {
                Directory.CreateDirectory(API.GamePath + "/DetectionLogs");
            }

            string[] files = Directory.GetFiles(API.GamePath + "/DetectionLogs");
            int _daysBeforeDeleted = (Days_Before_Log_Delete * -1);
            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                if (fi.CreationTime < DateTime.Now.AddDays(_daysBeforeDeleted))
                {
                    fi.Delete();
                }
            }
        }

        public static void ReturnToPosition(ClientInfo _cInfo)
        {
            bool _donator = false;
            string _sql = string.Format("SELECT respawnTime FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            DateTime _respawnTime;
            DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _respawnTime);
            _result.Dispose();
            TimeSpan varTime = DateTime.Now - _respawnTime;
            double fractionalMinutes = varTime.TotalMinutes;
            int _timepassed = (int)fractionalMinutes;
            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
            {
                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                {
                    DateTime _dt;
                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                    if (DateTime.Now < _dt)
                    {
                        _donator = true;
                        int _newDelay = 4;
                        if (_timepassed <= _newDelay)
                        {
                            string _deathPos;
                            if (Victim.TryGetValue(_cInfo.entityId, out _deathPos))
                            {
                                int x, y, z;
                                string[] _cords = _deathPos.Split(',');
                                int.TryParse(_cords[0], out x);
                                int.TryParse(_cords[1], out y);
                                int.TryParse(_cords[2], out z);
                                _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), null, false));
                                Victim.Remove(_cInfo.entityId);
                            }
                        }
                        else
                        {
                            string _phrase811;
                            if (!Phrases.Dict.TryGetValue(606, out _phrase811))
                            {
                                _phrase811 = " you can only use /return for four minutes after respawning. Time has expired.";
                            }
                            _phrase811 = _phrase811.Replace("{PlayerName}", _cInfo.playerName);
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase811 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            Victim.Remove(_cInfo.entityId);
                        }
                    }
                }
            }
            if (!_donator)
            {
                if (_timepassed <= 2)
                {
                    string _deathPos;
                    if (Victim.TryGetValue(_cInfo.entityId, out _deathPos))
                    {
                        int x, y, z;
                        string[] _cords = _deathPos.Split(',');
                        int.TryParse(_cords[0], out x);
                        int.TryParse(_cords[1], out y);
                        int.TryParse(_cords[2], out z);
                        _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), null, false));
                        Victim.Remove(_cInfo.entityId);
                    }
                }
                else
                {
                    string _phrase606;
                    if (!Phrases.Dict.TryGetValue(606, out _phrase606))
                    {
                        _phrase606 = " you can only use /return for two minutes after respawning. Time has expired.";
                    }
                    _phrase606 = _phrase606.Replace("{PlayerName}", _cInfo.playerName);
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase606 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    Victim.Remove(_cInfo.entityId);
                }
            }
        }

        public static void Penalty(ClientInfo _cInfoKiller, ClientInfo _cInfoVictim)
        {
            if (Jail_Enabled)
            {
                string _message = "[FF0000]{PlayerName} has been jailed for murder in a protected zone.";
                _message = _message.Replace("{PlayerName}", _cInfoKiller.playerName);
                ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                SdtdConsole.Instance.ExecuteSync(string.Format("jail add {0} 120", _cInfoKiller.playerId), (ClientInfo)null);
                Forgive[_cInfoVictim.entityId] = _cInfoKiller.entityId;
            }
            if (Kill_Enabled)
            {
                string _message = "[FF0000]{PlayerName} has been executed for murder in a protected zone.";
                _message = _message.Replace("{PlayerName}", _cInfoKiller.playerName);
                ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                SdtdConsole.Instance.ExecuteSync(string.Format("kill {0}", _cInfoKiller.playerId), (ClientInfo)null);
            }
            if (Kick_Enabled)
            {
                string _message = "[FF0000]{PlayerName} has been kicked for murder in a protected zone.";
                _message = _message.Replace("{PlayerName}", _cInfoKiller.playerName);
                ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for murder in a protected zone\"", _cInfoKiller.playerId), (ClientInfo)null);
            }
            if (Ban_Enabled)
            {
                string _message = "[FF0000]{PlayerName} has been banned for murder in a protected zone.";
                _message = _message.Replace("{PlayerName}", _cInfoKiller.playerName);
                ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for murder in a protected zone\"", _cInfoKiller.playerId), (ClientInfo)null);
            }
        }

        public static void Response(ClientInfo _cInfo, string _response)
        {
            _response = _response.Replace("{EntityId}", _cInfo.entityId.ToString());
            _response = _response.Replace("{SteamId}", _cInfo.playerId);
            _response = _response.Replace("{PlayerName}", _cInfo.playerName);
            if (_response.StartsWith("say "))
            {
                _response = _response.Replace("say ", "");
                ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _response + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
            }

            if (_response.StartsWith("tele ") || _response.StartsWith("tp ") || _response.StartsWith("teleportplayer "))
            {
                Players.NoFlight.Add(_cInfo.entityId);
                SdtdConsole.Instance.ExecuteSync(_response, _cInfo);
            }
            else
            {
                SdtdConsole.Instance.ExecuteSync(_response, _cInfo);
            }
        }

        public static bool BoxCheck(string[] _box, int _X, int _Y, int _Z, bool[] _box2)
        {
            string[] _corner1 = _box[0].Split(',');
            int xMin, yMin, zMin, xMax, yMax, zMax;;
            int.TryParse(_corner1[0], out xMin);
            int.TryParse(_corner1[1], out yMin);
            int.TryParse(_corner1[2], out zMin);
            if (!_box2[0])
            {
                string[] _corner2 = _box[1].Split(',');
                int.TryParse(_corner2[0], out xMax);
                int.TryParse(_corner2[1], out yMax);
                int.TryParse(_corner2[2], out zMax);
                if (xMin >= 0 && xMax >= 0)
                {
                    if (xMin < xMax)
                    {
                        if (_X >= xMin)
                        {
                            _xMinCheck = 1;
                        }
                        else
                        {
                            _xMinCheck = 0;
                        }
                        if (_X <= xMax)
                        {
                            _xMaxCheck = 1;
                        }
                        else
                        {
                            _xMaxCheck = 0;
                        }
                    }
                    else
                    {
                        if (_X <= xMin)
                        {
                            _xMinCheck = 1;
                        }
                        else
                        {
                            _xMinCheck = 0;
                        }
                        if (_X >= xMax)
                        {
                            _xMaxCheck = 1;
                        }
                        else
                        {
                            _xMaxCheck = 0;
                        }
                    }
                }
                else if (xMin <= 0 && xMax <= 0)
                {
                    if (xMin < xMax)
                    {
                        if (_X >= xMin)
                        {
                            _xMinCheck = 1;
                        }
                        else
                        {
                            _xMinCheck = 0;
                        }
                        if (_X <= xMax)
                        {
                            _xMaxCheck = 1;
                        }
                        else
                        {
                            _xMaxCheck = 0;
                        }
                    }
                    else
                    {
                        if (_X <= xMin)
                        {
                            _xMinCheck = 1;
                        }
                        else
                        {
                            _xMinCheck = 0;
                        }
                        if (_X >= xMax)
                        {
                            _xMaxCheck = 1;
                        }
                        else
                        {
                            _xMaxCheck = 0;
                        }
                    }
                }
                else if (xMin <= 0 && xMax >= 0)
                {
                    if (_X >= xMin)
                    {
                        _xMinCheck = 1;
                    }
                    else
                    {
                        _xMinCheck = 0;
                    }
                    if (_X <= xMax)
                    {
                        _xMaxCheck = 1;
                    }
                    else
                    {
                        _xMaxCheck = 0;
                    }
                }
                else if (xMin >= 0 && xMax <= 0)
                {
                    if (_X <= xMin)
                    {
                        _xMinCheck = 1;
                    }
                    else
                    {
                        _xMinCheck = 0;
                    }
                    if (_X >= xMax)
                    {
                        _xMaxCheck = 1;
                    }
                    else
                    {
                        _xMaxCheck = 0;
                    }
                }

                if (yMin >= 0 && yMax >= 0)
                {
                    if (yMin < yMax)
                    {
                        if (_Y >= yMin)
                        {
                            _yMinCheck = 1;
                        }
                        else
                        {
                            _yMinCheck = 0;
                        }
                        if (_Y <= yMax)
                        {
                            _yMaxCheck = 1;
                        }
                        else
                        {
                            _yMaxCheck = 0;
                        }
                    }
                    else
                    {
                        if (_Y <= yMin)
                        {
                            _yMinCheck = 1;
                        }
                        else
                        {
                            _yMinCheck = 0;
                        }
                        if (_Y >= yMax)
                        {
                            _yMaxCheck = 1;
                        }
                        else
                        {
                            _yMaxCheck = 0;
                        }
                    }
                }
                else if (yMin <= 0 && yMax <= 0)
                {
                    if (yMin < yMax)
                    {
                        if (_Y >= yMin)
                        {
                            _yMinCheck = 1;
                        }
                        else
                        {
                            _yMinCheck = 0;
                        }
                        if (_Y <= yMax)
                        {
                            _yMaxCheck = 1;
                        }
                        else
                        {
                            _yMaxCheck = 0;
                        }
                    }
                    else
                    {
                        if (_Y <= yMin)
                        {
                            _yMinCheck = 1;
                        }
                        else
                        {
                            _yMinCheck = 0;
                        }
                        if (_Y >= yMax)
                        {
                            _yMaxCheck = 1;
                        }
                        else
                        {
                            _yMaxCheck = 0;
                        }
                    }
                }
                else if (yMin <= 0 && yMax >= 0)
                {
                    if (_Y >= yMin)
                    {
                        _yMinCheck = 1;
                    }
                    else
                    {
                        _yMinCheck = 0;
                    }
                    if (_Y <= yMax)
                    {
                        _yMaxCheck = 1;
                    }
                    else
                    {
                        _yMaxCheck = 0;
                    }
                }
                else if (yMin >= 0 && yMax <= 0)
                {
                    if (_Y <= yMin)
                    {
                        _yMinCheck = 1;
                    }
                    else
                    {
                        _yMinCheck = 0;
                    }
                    if (_Y >= yMax)
                    {
                        _yMaxCheck = 1;
                    }
                    else
                    {
                        _yMaxCheck = 0;
                    }
                }

                if (zMin >= 0 && zMax >= 0)
                {
                    if (zMin < zMax)
                    {
                        if (_Z >= zMin)
                        {
                            _zMinCheck = 1;
                        }
                        else
                        {
                            _zMinCheck = 0;
                        }
                        if (_Z <= zMax)
                        {
                            _zMaxCheck = 1;
                        }
                        else
                        {
                            _zMaxCheck = 0;
                        }
                    }
                    else
                    {
                        if (_Z <= zMin)
                        {
                            _zMinCheck = 1;
                        }
                        else
                        {
                            _zMinCheck = 0;
                        }
                        if (_Z >= zMax)
                        {
                            _zMaxCheck = 1;
                        }
                        else
                        {
                            _zMaxCheck = 0;
                        }
                    }
                }
                else if (zMin <= 0 && zMax <= 0)
                {
                    if (zMin < zMax)
                    {
                        if (_Z >= zMin)
                        {
                            _zMinCheck = 1;
                        }
                        else
                        {
                            _zMinCheck = 0;
                        }
                        if (_Z <= zMax)
                        {
                            _zMaxCheck = 1;
                        }
                        else
                        {
                            _zMaxCheck = 0;
                        }
                    }
                    else
                    {
                        if (_Z <= zMin)
                        {
                            _zMinCheck = 1;
                        }
                        else
                        {
                            _zMinCheck = 0;
                        }
                        if (_Z >= zMax)
                        {
                            _zMaxCheck = 1;
                        }
                        else
                        {
                            _zMaxCheck = 0;
                        }
                    }
                }
                else if (zMin <= 0 && zMax >= 0)
                {
                    if (_Z >= zMin)
                    {
                        _zMinCheck = 1;
                    }
                    else
                    {
                        _zMinCheck = 0;
                    }
                    if (_Z <= zMax)
                    {
                        _zMaxCheck = 1;
                    }
                    else
                    {
                        _zMaxCheck = 0;
                    }
                }
                else if (zMin >= 0 && zMax <= 0)
                {
                    if (_Z <= zMin)
                    {
                        _zMinCheck = 1;
                    }
                    else
                    {
                        _zMinCheck = 0;
                    }
                    if (_Z >= zMax)
                    {
                        _zMaxCheck = 1;
                    }
                    else
                    {
                        _zMaxCheck = 0;
                    }
                }
                if (_xMinCheck == 1 && _yMinCheck == 1 && _zMinCheck == 1 && _xMaxCheck == 1 && _yMaxCheck == 1 && _zMaxCheck == 1)
                {
                    return true;
                }
                return false;
            }
            else
            {
                int _radius;
                if (int.TryParse(_box[1], out _radius))
                {
                    if ((xMin - _X) * (xMin - _X) + (zMin - _Z) * (zMin - _Z) <= _radius * _radius)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public static void Reminder()
        {
            foreach (KeyValuePair<int,DateTime> time in reminder)
            {
                ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForEntityId(time.Key);
                if (_cInfo != null)
                {
                    DateTime _dt = time.Value;
                    TimeSpan varTime = DateTime.Now - _dt;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int _timepassed = (int)fractionalMinutes;
                    if (_timepassed >= 15)
                    {
                        string _msg;
                        reminderMsg.TryGetValue(_cInfo.entityId, out _msg);
                        if (_msg != "")
                        {
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _msg + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            reminder[_cInfo.entityId] = DateTime.Now;
                        }
                    }
                }
                else
                {
                    reminder.Remove(time.Key);
                    reminderMsg.Remove(time.Key);
                }
            }
        }
    }
}
