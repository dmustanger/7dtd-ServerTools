using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    class Zones
    {
        public static bool IsEnabled = false, IsRunning = false, Kill_Enabled = false, Jail_Enabled = false, Kick_Enabled = false,
            Ban_Enabled = false, Zone_Message = false, Set_Home = false;
        public static int Reminder_Delay = 20;
        public static string Command50 = "return";
        public static Dictionary<int, DateTime> Reminder = new Dictionary<int, DateTime>();
        public static Dictionary<int, string> ReminderMsg = new Dictionary<int, string>();
        public static Dictionary<int, string> Victim = new Dictionary<int, string>();
        public static Dictionary<int, int> Forgive = new Dictionary<int, int>();
        public static Dictionary<int, string> ZoneExit = new Dictionary<int, string>();
        public static List<int> ZonePvE = new List<int>();
        public static List<string[]> Box1 = new List<string[]>();
        public static List<bool[]> Box2 = new List<bool[]>();
        public static List<string[]> ProtectedList = new List<string[]>();
        private const string file = "Zones.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        public static Dictionary<int, string[]> zoneSetup1 = new Dictionary<int, string[]>();
        public static Dictionary<int, bool[]> zoneSetup2 = new Dictionary<int, bool[]>();

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
            if (!IsEnabled && IsRunning)
            {
                Box1.Clear();
                Box2.Clear();
                ReminderMsg.Clear();
                Reminder.Clear();
                ZoneExit.Clear();
                ZonePvE.Clear();
                fileWatcher.Dispose();
                IsRunning = false;
                UnloadProtection();
            }
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
                        if (!_line.HasAttribute("protected"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because of missing protected attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        else
                        {
                            bool _result1, _result2, _result3, _result4;
                            string _circle = _line.GetAttribute("circle");
                            if (!bool.TryParse(_circle, out _result1))
                            {
                                Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because improper True/False for circle attribute: {0}.", subChild.OuterXml));
                                continue;
                            }
                            string _pve = _line.GetAttribute("PvE");
                            if (!bool.TryParse(_pve, out _result2))
                            {
                                Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because improper True/False for PvE attribute: {0}.", subChild.OuterXml));
                                continue;
                            }
                            string _noZ = _line.GetAttribute("noZombie");
                            if (!bool.TryParse(_noZ, out _result3))
                            {
                                Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because improper True/False for noZombie attribute: {0}.", subChild.OuterXml));
                                continue;
                            }
                            string _protect = _line.GetAttribute("protected");
                            if (!bool.TryParse(_protect, out _result4))
                            {
                                Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because improper True/False for protected attribute: {0}.", subChild.OuterXml));
                                continue;
                            }
                            string[] box1 = { _line.GetAttribute("corner1"), _line.GetAttribute("corner2"), _line.GetAttribute("entryMessage"), _line.GetAttribute("exitMessage"),
                            _line.GetAttribute("response"), _line.GetAttribute("reminderNotice") };
                            bool[] box2 = { _result1, _result2, _result3, _result4 };
                            if (!Box1.Contains(box1))
                            {
                                Box1.Add(box1);
                                Box2.Add(box2);
                                if (_result4)
                                {
                                    if (!_result1)
                                    {
                                        string[] _corner1 = box1[0].Split(',');
                                        string[] _corner2 = box1[1].Split(',');
                                        string[] _vectors = { _corner1[0], _corner1[2], _corner2[0], _corner2[2] };
                                        Zones.AddProtection(_vectors);
                                    }
                                    else
                                    {
                                        //Add circle protection later
                                    }
                                }
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
                        sw.WriteLine(string.Format("        <zone corner1=\"{0}\" corner2=\"{1}\" circle=\"{2}\" entryMessage=\"{3}\" exitMessage=\"{4}\" response=\"{5}\" reminderNotice=\"{6}\" PvE=\"{7}\" noZombie=\"{8}\" protected=\"{9}\" />", _box1[0], _box1[1], _box2[0], _box1[2], _box1[3], _box1[4], _box1[5], _box2[1], _box2[2], _box2[3]));
                    }
                }
                else
                {
                    sw.WriteLine("        <!-- <zone corner1=\"-8000,0,8000\" corner2=\"8000,200,0\" circle=\"false\" entryMessage=\"You are entering the Northern side\" exitMessage=\"You have exited the Northern Side\" response=\"\" reminderNotice=\"You are still in the North\" PvE=\"false\" noZombie=\"false\" protected=\"false\" /> -->");
                    sw.WriteLine("        <!-- <zone corner1=\"-8000,0,-1\" corner2=\"8000,200,-8000\" circle=\"false\" entryMessage=\"You are entering the Southern side\" exitMessage=\"You have exited the Southern Side\" response=\"whisper {PlayerName} you have entered the south side ^ ser {EntityId} 40 @ 4\" reminderNotice=\"You are still in the South\" PvE=\"false\" noZombie=\"false\" protected=\"false\" /> -->");
                    sw.WriteLine("        <!-- <zone corner1=\"-100,0,-90\" corner2=\"40\" circle=\"true\" entryMessage=\"You have entered the Market\" exitMessage=\"You have exited the Market\" response=\"whisper {PlayerName} you have entered the market\" reminderNotice=\"\" PvE=\"true\" noZombie=\"true\" protected=\"true\" /> -->");
                    sw.WriteLine("        <!-- <zone corner1=\"0,0,0\" corner2=\"25,105,25\" circle=\"false\" entryMessage=\"You have entered the Lobby\" exitMessage=\"You have exited the Lobby\" response=\"global {PlayerName} has entered the lobby\" reminderNotice=\"You have been in the lobby for a long time...\" PvE=\"true\" noZombie=\"true\" protected=\"true\" /> -->");
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

        public static void Check(ClientInfo _cInfoVictim, ClientInfo _cInfoKiller)
        {
            if (ZonePvE.Contains(_cInfoVictim.entityId))
            {
                string _phrase801;
                if (!Phrases.Dict.TryGetValue(801, out _phrase801))
                {
                    _phrase801 = "{PlayerName} has murdered you while you were in a pve zone. Your death count was reduced by one.";
                }
                _phrase801 = _phrase801.Replace("{PlayerName}", _cInfoKiller.playerName);
                ChatHook.ChatMessage(_cInfoVictim, LoadConfig.Chat_Response_Color + _phrase801 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                string _phrase802;
                if (!Phrases.Dict.TryGetValue(802, out _phrase802))
                {
                    _phrase802 = " you have murdered a player while inside a pve zone. Their name was {PlayerName}. Your player kill count was reduced by one.";
                }
                _phrase802 = _phrase802.Replace("{PlayerName}", _cInfoVictim.playerName);
                ChatHook.ChatMessage(_cInfoKiller, LoadConfig.Chat_Response_Color + _phrase802 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                EntityPlayer _player = PersistentOperations.GetEntityPlayer(_cInfoVictim.playerId);
                if (_player != null)
                {
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
                Penalty(_cInfoKiller, _cInfoVictim);
            }
            else if (!ZonePvE.Contains(_cInfoVictim.entityId) && ZonePvE.Contains(_cInfoKiller.entityId))
            {
                string _phrase801;
                if (!Phrases.Dict.TryGetValue(801, out _phrase801))
                {
                    _phrase801 = "{PlayerName} has murdered you while they were in a pve zone.";
                }
                _phrase801 = _phrase801.Replace("{PlayerName}", _cInfoKiller.playerName);
                ChatHook.ChatMessage(_cInfoVictim, LoadConfig.Chat_Response_Color + _phrase801 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                string _phrase802;
                if (!Phrases.Dict.TryGetValue(802, out _phrase802))
                {
                    _phrase802 = " you have murdered a player while inside a pve zone. Their name was {PlayerName}.";
                }
                _phrase802 = _phrase802.Replace("{PlayerName}", _cInfoVictim.playerName);
                ChatHook.ChatMessage(_cInfoKiller, ChatHook.Player_Name_Color + _cInfoKiller.playerName + LoadConfig.Chat_Response_Color + _phrase802 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                Penalty(_cInfoKiller, _cInfoVictim);
                string _file1 = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
                string _filepath1 = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, _file1);
                using (StreamWriter sw = new StreamWriter(_filepath1, true))
                {
                    sw.WriteLine(string.Format("Detected {0} with steam id {1} has murdered {2} with steam id {3} while inside a pve zone.", _cInfoKiller.playerName, _cInfoKiller.steamId, _cInfoVictim.playerName, _cInfoVictim.steamId));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
            }
        }

        public static void ZoneCheck(ClientInfo _cInfo, EntityAlive _player)
        {
            if (Box1.Count > 0)
            {
                for (int i = 0; i < Box1.Count; i++)
                {
                    string[] _box1 = Box1[i];
                    bool[] _box2 = Box2[i];
                    if (BoxCheck(_box1, _player.position.x, _player.position.y, _player.position.z, _box2))
                    {
                        if (ZoneExit.ContainsKey(_player.entityId))
                        {
                            string _exitMsg;
                            if (ZoneExit.TryGetValue(_player.entityId, out _exitMsg))
                            {
                                if (_exitMsg != _box1[3])
                                {
                                    if (Zone_Message)
                                    {
                                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _box1[2] + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                    if (_box1[4] != "")
                                    {
                                        Response(_cInfo, _box1[4]);
                                    }
                                    ZoneExit[_player.entityId] = _box1[3];
                                    Reminder[_player.entityId] = DateTime.Now;
                                    ReminderMsg[_player.entityId] = _box1[5];
                                }
                            }
                        }
                        else
                        {
                            if (Zone_Message)
                            {
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _box1[2] + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            if (_box1[4] != "")
                            {
                                Response(_cInfo, _box1[4]);
                            }
                            ZoneExit.Add(_player.entityId, _box1[3]);
                            Reminder.Add(_player.entityId, DateTime.Now);
                            ReminderMsg.Add(_player.entityId, _box1[5]);
                        }
                        if (_box2[1])
                        {
                            if (!ZonePvE.Contains(_player.entityId))
                            {
                                ZonePvE.Add(_player.entityId);
                            }
                        }
                        else if (ZonePvE.Contains(_player.entityId))
                        {
                            ZonePvE.Remove(_player.entityId);
                        }
                        return;
                    }
                }
                if (ZoneExit.ContainsKey(_player.entityId))
                {
                    if (Zone_Message)
                    {
                        string _msg;
                        if (ZoneExit.TryGetValue(_player.entityId, out _msg))
                        {
                            if (_msg != "")
                            {
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _msg + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                    ZoneExit.Remove(_player.entityId);
                    Reminder.Remove(_player.entityId);
                    ReminderMsg.Remove(_player.entityId);
                    if (ZonePvE.Contains(_player.entityId))
                    {
                        ZonePvE.Remove(_player.entityId);
                    }
                }
            }
            if (Lobby.IsEnabled && Lobby.LobbyPlayers.Contains(_cInfo.entityId) && !Lobby.InsideLobby(_player.position.x, _player.position.z))
            {
                Lobby.LobbyPlayers.Remove(_cInfo.entityId);
                if (Lobby.Return)
                {
                    PersistentContainer.Instance.Players[_cInfo.playerId].LobbyReturnPos = "";
                    PersistentContainer.Instance.Save();
                    string _phrase556;
                    if (!Phrases.Dict.TryGetValue(556, out _phrase556))
                    {
                        _phrase556 = " you have left the lobby space. {PrivateCommand}{Command53} command is no longer available.";
                        _phrase556 = _phrase556.Replace("{PrivateCommand}", ChatHook.Command_Private);
                        _phrase556 = _phrase556.Replace("{Command53}", ChatHook.Command_Private);
                    }
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase556 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            if (Market.IsEnabled && Market.MarketPlayers.Contains(_cInfo.entityId) && !Market.InsideMarket(_player.position.x, _player.position.z))
            {
                Market.MarketPlayers.Remove(_cInfo.entityId);
                if (Market.Return)
                {
                    PersistentContainer.Instance.Players[_cInfo.playerId].MarketReturnPos = "";
                    PersistentContainer.Instance.Save();
                    string _phrase564;
                    if (!Phrases.Dict.TryGetValue(564, out _phrase564))
                    {
                        _phrase564 = " you have left the market space. {PrivateCommand}{Command51} command is no longer available.";
                        _phrase564 = _phrase564.Replace("{PrivateCommand}", ChatHook.Command_Private);
                        _phrase564 = _phrase564.Replace("{Command51}", ChatHook.Command_Private);
                    }
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase564 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void ReturnToPosition(ClientInfo _cInfo)
        {
            DateTime _respawnTime = PersistentContainer.Instance.Players[_cInfo.playerId].ZoneDeathTime;
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
                        Time(_cInfo, _timepassed, 6);
                        return;
                    }
                }
            }
            Time(_cInfo, _timepassed, 3);
        }

        public static void Time(ClientInfo _cInfo, int _timePassed, int _timeAllowed)
        {
            if (_timePassed <= _timeAllowed)
            {
                string _deathPos;
                if (Victim.TryGetValue(_cInfo.entityId, out _deathPos))
                {
                    int x, y, z;
                    string[] _cords = _deathPos.Split(',');
                    int.TryParse(_cords[0], out x);
                    int.TryParse(_cords[1], out y);
                    int.TryParse(_cords[2], out z);
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, y, z), null, false));
                    Victim.Remove(_cInfo.entityId);
                }
            }
            else
            {
                string _phrase811;
                if (!Phrases.Dict.TryGetValue(606, out _phrase811))
                {
                    _phrase811 = " you can only use {CommandPrivate}{Command50} for {Time} minutes after being killed in a pve zone. Time has expired.";
                }
                _phrase811 = _phrase811.Replace("{PlayerName}", _cInfo.playerName);
                _phrase811 = _phrase811.Replace("{CommandPrivate}", ChatHook.Command_Private);
                _phrase811 = _phrase811.Replace("{Command50}", Zones.Command50);
                _phrase811 = _phrase811.Replace("{Time}", _timeAllowed.ToString());
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase811 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                Victim.Remove(_cInfo.entityId);
            }
        }

        public static void Penalty(ClientInfo _cInfoKiller, ClientInfo _cInfoVictim)
        {
            try
            {
                EntityPlayer _victim = PersistentOperations.GetEntityPlayer(_cInfoVictim.playerId);
                if (_victim != null)
                {
                    int _deathCount = _victim.Died - 1;
                    _victim.Died = _deathCount;
                    _victim.bPlayerStatsChanged = true;
                    _cInfoVictim.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerStats>().Setup(_victim));
                }
                EntityPlayer _killer = PersistentOperations.GetEntityPlayer(_cInfoKiller.playerId);
                if (_killer != null)
                {
                    int _killCount = _victim.KilledPlayers - 1;
                    _killer.KilledPlayers = _killCount;
                    _killer.bPlayerStatsChanged = true;
                    _cInfoKiller.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerStats>().Setup(_killer));
                }
                if (Jail_Enabled)
                {
                    string _message = "[FF0000]{PlayerName} has been jailed for murder in a pve zone.";
                    _message = _message.Replace("{PlayerName}", _cInfoKiller.playerName);
                    ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                    SdtdConsole.Instance.ExecuteSync(string.Format("jail add {0} 120", _cInfoKiller.playerId), (ClientInfo)null);
                    if (!Forgive.ContainsKey(_cInfoVictim.entityId))
                    {
                        Forgive.Add(_cInfoVictim.entityId, _cInfoKiller.entityId);
                    }
                    else
                    {
                        Forgive[_cInfoVictim.entityId] = _cInfoKiller.entityId;
                    }
                }
                if (Kill_Enabled)
                {
                    string _message = "[FF0000]{PlayerName} has been executed for murder in a pve zone.";
                    _message = _message.Replace("{PlayerName}", _cInfoKiller.playerName);
                    ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                    SdtdConsole.Instance.ExecuteSync(string.Format("kill {0}", _cInfoKiller.playerId), (ClientInfo)null);
                }
                if (Kick_Enabled)
                {
                    string _message = "[FF0000]{PlayerName} has been kicked for murder in a pve zone.";
                    _message = _message.Replace("{PlayerName}", _cInfoKiller.playerName);
                    ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for murder in a pve zone\"", _cInfoKiller.playerId), (ClientInfo)null);
                }
                if (Ban_Enabled)
                {
                    string _message = "[FF0000]{PlayerName} has been banned for murder in a pve zone.";
                    _message = _message.Replace("{PlayerName}", _cInfoKiller.playerName);
                    ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                    SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for murder in a pve zone\"", _cInfoKiller.playerId), (ClientInfo)null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Zones.Penalty: {0}", e.Message));
            }
        }

        public static void Response(ClientInfo _cInfo, string _response)
        {
            string[] _responseSplit = _response.Split('^');
            for (int i = 0; i < _responseSplit.Length; i++)
            {
                string _responseAdj = _responseSplit[i].Trim();
                _responseAdj = _responseAdj.Replace("{EntityId}", _cInfo.entityId.ToString());
                _responseAdj = _responseAdj.Replace("{SteamId}", _cInfo.playerId);
                _responseAdj = _responseAdj.Replace("{PlayerName}", _cInfo.playerName);
                if (_responseAdj.ToLower().StartsWith("global "))
                {
                    _responseAdj = _responseAdj.Replace("Global ", "");
                    _responseAdj = _responseAdj.Replace("global ", "");
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _responseAdj + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
                else if (_responseAdj.ToLower().StartsWith("whisper "))
                {
                    _responseAdj = _responseAdj.Replace("Whisper ", "");
                    _responseAdj = _responseAdj.Replace("whisper ", "");
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _responseAdj + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    SdtdConsole.Instance.ExecuteSync(_responseAdj, null);
                }
            }
        }

        public static bool BoxCheck(string[] _box, float _X, float _Y, float _Z, bool[] _box2)
        {
            string[] _corner1 = _box[0].Split(',');
            float xMin, yMin, zMin, xMax, yMax, zMax;
            float.TryParse(_corner1[0], out xMin);
            float.TryParse(_corner1[1], out yMin);
            float.TryParse(_corner1[2], out zMin);
            if (!_box2[0])
            {
                string[] _corner2 = _box[1].Split(',');
                float.TryParse(_corner2[0], out xMax);
                float.TryParse(_corner2[1], out yMax);
                float.TryParse(_corner2[2], out zMax);
                if (VectorBox(xMin, yMin, zMin, xMax, yMax, zMax, _X, _Y, _Z))
                {
                    return true;
                }
                return false;
            }
            else
            {
                int _radius;
                int.TryParse(_box[1], out _radius);
                if (VectorCircle(xMin, zMin, _X, _Z, _radius))
                {
                    return true;
                }
                return false;
            }
        }

        public static bool VectorCircle(float xMin, float zMin, float _X, float _Z, int _radius)
        {
            if ((xMin - _X) * (xMin - _X) + (zMin - _Z) * (zMin - _Z) <= _radius * _radius)
            {
                return true;
            }
            return false;
        }

        public static bool VectorBox(float xMin, float yMin, float zMin, float xMax, float yMax, float zMax, float _X, float _Y, float _Z)
        {
            if (xMin >= 0 && xMax >= 0)
            {
                if (xMin < xMax)
                {
                    if (_X < xMin || _X > xMax)
                    {
                        return false;
                    }
                }
                else
                {
                    if (_X > xMin || _X < xMax)
                    {
                        return false;
                    }
                }
            }
            else if (xMin <= 0 && xMax <= 0)
            {
                if (xMin < xMax)
                {
                    if (_X < xMin || _X > xMax)
                    {
                        return false;
                    }
                }
                else
                {
                    if (_X > xMin || _X < xMax)
                    {
                        return false;
                    }
                }
            }
            else if (xMin <= 0 && xMax >= 0)
            {
                if (_X < xMin || _X > xMax)
                {
                    return false;
                }
            }
            else if (xMin >= 0 && xMax <= 0)
            {
                if (_X > xMin || _X < xMax)
                {
                    return false;
                }
            }
            if (yMin >= 0 && yMax >= 0)
            {
                if (yMin < yMax)
                {
                    if (_Y < yMin || _Y > yMax)
                    {
                        return false;
                    }
                }
                else
                {
                    if (_Y > yMin || _Y < yMax)
                    {
                        return false;
                    }
                }
            }
            else if (yMin <= 0 && yMax <= 0)
            {
                if (yMin < yMax)
                {
                    if (_Y < yMin || _Y > yMax)
                    {
                        return false;
                    }
                }
                else
                {
                    if (_Y > yMin || _Y < yMax)
                    {
                        return false;
                    }
                }
            }
            else if (yMin <= 0 && yMax >= 0)
            {
                if (_Y < yMin || _Y > yMax)
                {
                    return false;
                }
            }
            else if (yMin >= 0 && yMax <= 0)
            {
                if (_Y > yMin || _Y < yMax)
                {
                    return false;
                }
            }
            if (zMin >= 0 && zMax >= 0)
            {
                if (zMin < zMax)
                {
                    if (_Z < zMin || _Z > zMax)
                    {
                        return false;
                    }
                }
                else
                {
                    if (_Z > zMin || _Z < zMax)
                    {
                        return false;
                    }
                }
            }
            else if (zMin <= 0 && zMax <= 0)
            {
                if (zMin < zMax)
                {
                    if (_Z < zMin || _Z > zMax)
                    {
                        return false;
                    }
                }
                else
                {
                    if (_Z > zMin || _Z < zMax)
                    {
                        return false;
                    }
                }
            }
            else if (zMin <= 0 && zMax >= 0)
            {
                if (_Z < zMin || _Z > zMax)
                {
                    return false;
                }
            }
            else if (zMin >= 0 && zMax <= 0)
            {
                if (_Z > zMin || _Z < zMax)
                {
                    return false;
                }
            }
            return true;
        }

        public static void HostileCheck()
        {
            try
            {
                if (Zones.Box1.Count > 0)
                {
                    for (int i = 0; i < Zones.Box1.Count; i++)
                    {
                        string[] _box1 = Zones.Box1[i];
                        bool[] _box2 = Zones.Box2[i];
                        if (_box2[2])
                        {
                            List<Entity> Entities = GameManager.Instance.World.Entities.list;
                            for (int j = 0; j < Entities.Count; j++)
                            {
                                Entity _entity = Entities[j];
                                if (_entity != null && !_entity.IsClientControlled() && !_entity.IsDead())
                                {
                                    string _tags = _entity.EntityClass.Tags.ToString();
                                    if (_tags.Contains("zombie") || _tags.Contains("hostile"))
                                    {
                                        Vector3 _vec = _entity.position;
                                        int _X = (int)_entity.position.x;
                                        int _Y = (int)_entity.position.y;
                                        int _Z = (int)_entity.position.z;
                                        int xMin, yMin, zMin;
                                        string[] _corner1 = _box1[0].Split(',');
                                        int.TryParse(_corner1[0], out xMin);
                                        int.TryParse(_corner1[1], out yMin);
                                        int.TryParse(_corner1[2], out zMin);
                                        if (!_box2[0])
                                        {
                                            int xMax, yMax, zMax;
                                            string[] _corner2 = _box1[1].Split(',');
                                            int.TryParse(_corner2[0], out xMax);
                                            int.TryParse(_corner2[1], out yMax);
                                            int.TryParse(_corner2[2], out zMax);
                                            if (VectorBox(xMin, yMin, zMin, xMax, yMax, zMax, _X, _Y, _Z))
                                            {
                                                string _name = EntityClass.list[_entity.entityClass].entityClassName;
                                                GameManager.Instance.World.RemoveEntity(_entity.entityId, EnumRemoveEntityReason.Despawned);
                                                Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed {0} from protected zone @ {1} {2} {3}", _name, _X, _Y, _Z));
                                            }
                                        }
                                        else
                                        {
                                            int _radius;
                                            if (int.TryParse(_box1[1], out _radius))
                                            {
                                                if ((xMin - _X) * (xMin - _X) + (zMin - _Z) * (zMin - _Z) <= _radius * _radius)
                                                {
                                                    string _name = EntityClass.list[_entity.entityClass].entityClassName;
                                                    GameManager.Instance.World.RemoveEntity(_entity.entityId, EnumRemoveEntityReason.Despawned);
                                                    Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed {0} from protected zone @ {1} {2} {3}", _name, _X, _Y, _Z));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Zones.HostileCheck: {0}", e.Message));
            }
        }

        public static void ReminderExec()
        {
            try
            {
                foreach (KeyValuePair<int, DateTime> time in Reminder)
                {
                    ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForEntityId(time.Key);
                    if (_cInfo != null)
                    {
                        DateTime _dt = time.Value;
                        TimeSpan varTime = DateTime.Now - _dt;
                        double fractionalMinutes = varTime.TotalMinutes;
                        int _timepassed = (int)fractionalMinutes;
                        if (_timepassed >= Reminder_Delay)
                        {
                            string _msg;
                            ReminderMsg.TryGetValue(_cInfo.entityId, out _msg);
                            if (_msg != "")
                            {
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _msg + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                Reminder[_cInfo.entityId] = DateTime.Now;
                            }
                        }
                    }
                    else
                    {
                        Reminder.Remove(time.Key);
                        ReminderMsg.Remove(time.Key);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Zones.ReminderExec: {0}", e.Message));
            }
        }

        public static bool Protected(Vector3i _position)
        {
            try
            {
                if (Box1.Count > 0)
                {
                    for (int i = 0; i < Box1.Count; i++)
                    {
                        bool[] _box2 = Box2[i];
                        if (_box2[3])
                        {
                            string[] _box1 = Box1[i];
                            float _xMin, _zMin, _xMax, _zMax;
                            string[] _min = _box1[0].Split(',');
                            float.TryParse(_min[0], out _xMin);
                            float.TryParse(_min[2], out _zMin);
                            if (!_box2[0])
                            {
                                string[] _max = _box1[1].Split(',');
                                float.TryParse(_max[0], out _xMax);
                                float.TryParse(_max[2], out _zMax);
                                if (Zones.VectorBox(_xMin, _zMin, _xMax, _zMax, _position.x, _position.z))
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                int _radius;
                                int.TryParse(_box1[1], out _radius);
                                if (Zones.VectorCircle(_xMin, _zMin, _position.x, _position.z, _radius))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Zones.Protected: {0}", e.Message));
            }
            return false;
        }

        public static bool VectorBox(float xMin, float zMin, float xMax, float zMax, float _X, float _Z)
        {
            if (xMin >= 0 && xMax >= 0)
            {
                if (xMin < xMax)
                {
                    if (_X < xMin || _X > xMax)
                    {
                        return false;
                    }
                }
                else
                {
                    if (_X > xMin || _X < xMax)
                    {
                        return false;
                    }
                }
            }
            else if (xMin <= 0 && xMax <= 0)
            {
                if (xMin < xMax)
                {
                    if (_X < xMin || _X > xMax)
                    {
                        return false;
                    }
                }
                else
                {
                    if (_X > xMin || _X < xMax)
                    {
                        return false;
                    }
                }
            }
            else if (xMin <= 0 && xMax >= 0)
            {
                if (_X < xMin || _X > xMax)
                {
                    return false;
                }
            }
            else if (xMin >= 0 && xMax <= 0)
            {
                if (_X > xMin || _X < xMax)
                {
                    return false;
                }
            }
            if (zMin >= 0 && zMax >= 0)
            {
                if (zMin < zMax)
                {
                    if (_Z < zMin || _Z > zMax)
                    {
                        return false;
                    }
                }
                else
                {
                    if (_Z > zMin || _Z < zMax)
                    {
                        return false;
                    }
                }
            }
            else if (zMin <= 0 && zMax <= 0)
            {
                if (zMin < zMax)
                {
                    if (_Z < zMin || _Z > zMax)
                    {
                        return false;
                    }
                }
                else
                {
                    if (_Z > zMin || _Z < zMax)
                    {
                        return false;
                    }
                }
            }
            else if (zMin <= 0 && zMax >= 0)
            {
                if (_Z < zMin || _Z > zMax)
                {
                    return false;
                }
            }
            else if (zMin >= 0 && zMax <= 0)
            {
                if (_Z > zMin || _Z < zMax)
                {
                    return false;
                }
            }
            return true;
        }

        public static void AddProtection(string[] _vectors)
        {
            try
            {
                int _xMin = int.Parse(_vectors[0]), _zMin = int.Parse(_vectors[1]), _xMax = int.Parse(_vectors[2]), _zMax = int.Parse(_vectors[3]);
                int _xMinAlt = _xMin, _zMinAlt = _zMin, _xMaxAlt = _xMax, _zMaxAlt = _zMax;
                if (_xMin > _xMax)
                {
                    _xMinAlt = _xMax;
                    _xMaxAlt = _xMin;
                }
                if (_zMin > _zMax)
                {
                    _zMinAlt = _zMax;
                    _zMaxAlt = _zMin;
                }
                List<Chunk> _chunkList = new List<Chunk>();
                string[] _vectorsAlt = { _xMinAlt.ToString(), _zMinAlt.ToString(), _xMaxAlt.ToString(), _zMaxAlt.ToString() };
                if (!Zones.ProtectedList.Contains(_vectorsAlt))
                {
                    Zones.ProtectedList.Add(_vectorsAlt);
                    for (int i = _xMinAlt; i <= _xMaxAlt; i++)
                    {
                        for (int j = _zMinAlt; j <= _zMaxAlt; j++)
                        {
                            if (GameManager.Instance.World.IsChunkAreaLoaded(i, 1, j))
                            {
                                Chunk _chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(i, 1, j);
                                if (!_chunkList.Contains(_chunk))
                                {
                                    _chunkList.Add(_chunk);
                                }
                                Bounds bounds = _chunk.GetAABB();
                                int _x = i - (int)bounds.min.x, _z = j - (int)bounds.min.z;
                                _chunk.SetTraderArea(_x, _z, true);
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                    if (_chunkList.Count > 0)
                    {
                        for (int k = 0; k < _chunkList.Count; k++)
                        {
                            Chunk _chunk = _chunkList[k];
                            List<ClientInfo> _clientList = PersistentOperations.ClientList();
                            if (_clientList != null && _clientList.Count > 0)
                            {
                                for (int l = 0; l < _clientList.Count; l++)
                                {
                                    ClientInfo _cInfo2 = _clientList[l];
                                    if (_cInfo2 != null)
                                    {
                                        _cInfo2.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(_chunk, true));
                                    }
                                }
                            }
                        }
                    }
                    Zones.UpdateXml();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Zones.AddProtection: {0}", e.Message));
            }
        }

        public static void UnloadProtection()
        {
            try
            {
                List<string[]> _protectedList = Zones.ProtectedList;
                if (_protectedList.Count > 0)
                {
                    List<Chunk> _chunkList = new List<Chunk>();
                    for (int i = 0; i < _protectedList.Count; i++)
                    {
                        string[] _vector = _protectedList[i];
                        int _xMin = int.Parse(_vector[0]), _zMin = int.Parse(_vector[1]), _xMax = int.Parse(_vector[2]), _zMax = int.Parse(_vector[3]);
                        int _xMinAlt = _xMin, _zMinAlt = _zMin, _xMaxAlt = _xMax, _zMaxAlt = _zMax;
                        if (_xMin > _xMax)
                        {
                            _xMinAlt = _xMax;
                            _xMaxAlt = _xMin;
                        }
                        if (_zMin > _zMax)
                        {
                            _zMinAlt = _zMax;
                            _zMaxAlt = _zMin;
                        }
                        for (int j = _xMinAlt; j <= _xMaxAlt; j++)
                        {
                            for (int k = _zMinAlt; k <= _zMaxAlt; k++)
                            {
                                if (GameManager.Instance.World.IsChunkAreaLoaded(j, 1, k))
                                {
                                    Chunk _chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(j, 1, k);
                                    if (!_chunkList.Contains(_chunk))
                                    {
                                        _chunkList.Add(_chunk);
                                    }
                                    Bounds bounds = _chunk.GetAABB();
                                    int _x = i - (int)bounds.min.x, _z = j - (int)bounds.min.z;
                                    _chunk.SetTraderArea(_x, _z, false);
                                }
                            }
                        }
                        _protectedList.Remove(_vector);
                    }
                    Zones.ProtectedList = _protectedList;
                    if (_chunkList.Count > 0)
                    {
                        for (int l = 0; l < _chunkList.Count; l++)
                        {
                            Chunk _chunk = _chunkList[l];
                            List<ClientInfo> _clientList = PersistentOperations.ClientList();
                            if (_clientList != null && _clientList.Count > 0)
                            {
                                for (int m = 0; m < _clientList.Count; m++)
                                {
                                    ClientInfo _cInfo2 = _clientList[m];
                                    if (_cInfo2 != null)
                                    {
                                        _cInfo2.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(_chunk, true));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Zones.UnloadProtection: {0}", e.Message));
            }
        }
    }
}
