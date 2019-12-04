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
            Ban_Enabled = false, Zone_Message = false, Set_Home = false, No_Zombie = false;
        public static int Reminder_Delay = 20;
        public static string Command50 = "return";
        public static Dictionary<int, DateTime> reminder = new Dictionary<int, DateTime>();
        public static Dictionary<int, string> reminderMsg = new Dictionary<int, string>();
        public static Dictionary<int, string[]> zoneSetup1 = new Dictionary<int, string[]>();
        public static Dictionary<int, bool[]> zoneSetup2 = new Dictionary<int, bool[]>();
        public static Dictionary<int, string> Victim = new Dictionary<int, string>();
        public static Dictionary<int, int> Forgive = new Dictionary<int, int>();
        public static Dictionary<int, string> ZoneExit = new Dictionary<int, string>();
        public static List<int> ZonePvE = new List<int>();
        public static List<string[]> Box1 = new List<string[]>();
        public static List<bool[]> Box2 = new List<bool[]>();
        public static List<Rect> TestBox = new List<Rect>();
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
                    TestBox.Clear();
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
                    sw.WriteLine("        <zone corner1=\"-8000,0,8000\" corner2=\"8000,200,0\" entryMessage=\"You are entering the Northern side\" exitMessage=\"You have exited the Northern Side\" response=\"\" reminderNotice=\"You are still in the North\" circle=\"false\" PvE=\"false\" noZombie=\"false\" />");
                    sw.WriteLine("        <zone corner1=\"-8000,0,-1\" corner2=\"8000,200,-8000\" entryMessage=\"You are entering the Southern side\" exitMessage=\"You have exited the Southern Side\" response=\"whisper {PlayerName} you have entered the south side ^ ser {EntityId} 40 @ 4\" reminderNotice=\"You are still in the South\" circle=\"false\" PvE=\"false\" noZombie=\"false\" />");
                    sw.WriteLine("        <zone corner1=\"-100,0,-90\" corner2=\"40\" entryMessage=\"You have entered the Market\" exitMessage=\"You have exited the Market\" response=\"whisper {PlayerName} you have entered the market\" reminderNotice=\"\" circle=\"true\" PvE=\"true\" noZombie=\"true\" />");
                    sw.WriteLine("        <zone corner1=\"0,0,0\" corner2=\"25,105,25\" entryMessage=\"You have entered the Lobby\" exitMessage=\"You have exited the Lobby\" response=\"say {PlayerName} has entered the lobby\" reminderNotice=\"You have been in the lobby for a long time...\" circle=\"false\" PvE=\"true\" noZombie=\"true\" />");
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
            if (ZonePvE.Contains(_cInfoVictim.entityId) && ZonePvE.Contains(_cInfoKiller.entityId))
            {
                string _phrase801;
                if (!Phrases.Dict.TryGetValue(801, out _phrase801))
                {
                    _phrase801 = "{PlayerName} has murdered you while you were in a pve zone.";
                }
                _phrase801 = _phrase801.Replace("{PlayerName}", _cInfoKiller.playerName);
                ChatHook.ChatMessage(_cInfoVictim, LoadConfig.Chat_Response_Color + _phrase801 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                string _phrase802;
                if (!Phrases.Dict.TryGetValue(802, out _phrase802))
                {
                    _phrase802 = " you have murdered a player inside a pve zone. Their name was {PlayerName}";
                }
                _phrase802 = _phrase802.Replace("{PlayerName}", _cInfoVictim.playerName);
                ChatHook.ChatMessage(_cInfoKiller, LoadConfig.Chat_Response_Color + _phrase802 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
            }
            else if (ZonePvE.Contains(_cInfoVictim.entityId) & !ZonePvE.Contains(_cInfoKiller.entityId))
            {
                string _phrase801;
                if (!Phrases.Dict.TryGetValue(801, out _phrase801))
                {
                    _phrase801 = "{PlayerName} has murdered you while you were in a pve zone.";
                }
                _phrase801 = _phrase801.Replace("{PlayerName}", _cInfoKiller.playerName);
                ChatHook.ChatMessage(_cInfoVictim, LoadConfig.Chat_Response_Color + _phrase801 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                string _phrase802;
                if (!Phrases.Dict.TryGetValue(802, out _phrase802))
                {
                    _phrase802 = " you have murdered a player inside a pve zone. Their name was {PlayerName}";
                }
                _phrase802 = _phrase802.Replace("{PlayerName}", _cInfoVictim.playerName);
                ChatHook.ChatMessage(_cInfoKiller, LoadConfig.Chat_Response_Color + _phrase802 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                    _phrase802 = " you have murdered a player inside a pve zone. Their name was {PlayerName}.";
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
                    sw.WriteLine(string.Format("Detected {0}, Steam Id {1}, murdered {2}, Steam Id {3} in a pve zone.", _cInfoKiller.playerName, _cInfoKiller.steamId, _cInfoVictim.playerName, _cInfoVictim.steamId));
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
                int _X = (int)_player.position.x;
                int _Y = (int)_player.position.y;
                int _Z = (int)_player.position.z;
                for (int i = 0; i < Box1.Count; i++)
                {
                    string[] _box1 = Box1[i];
                    bool[] _box2 = Box2[i];
                    if (BoxCheck(_box1, _X, _Y, _Z, _box2))
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
                                    reminder[_player.entityId] = DateTime.Now;
                                    reminderMsg[_player.entityId] = _box1[5];
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
                            reminder.Add(_player.entityId, DateTime.Now);
                            reminderMsg.Add(_player.entityId, _box1[5]);
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
                    reminder.Remove(_player.entityId);
                    reminderMsg.Remove(_player.entityId);
                    if (ZonePvE.Contains(_player.entityId))
                    {
                        ZonePvE.Remove(_player.entityId);
                    }
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
                else if (_responseAdj.StartsWith("tele ") || _responseAdj.StartsWith("tp ") || _responseAdj.StartsWith("teleportplayer "))
                {
                    SdtdConsole.Instance.ExecuteSync(_responseAdj, null);
                    if (Zones.IsEnabled && Zones.ZoneExit.ContainsKey(_cInfo.entityId))
                    {
                        Zones.ZoneExit.Remove(_cInfo.entityId);
                    }
                }
                else
                {
                    SdtdConsole.Instance.ExecuteSync(_responseAdj, null);
                }
            }
        }

        public static bool BoxCheck(string[] _box, int _X, int _Y, int _Z, bool[] _box2)
        {
            string[] _corner1 = _box[0].Split(',');
            int xMin, yMin, zMin, xMax, yMax, zMax;
            int.TryParse(_corner1[0], out xMin);
            int.TryParse(_corner1[1], out yMin);
            int.TryParse(_corner1[2], out zMin);
            if (!_box2[0])
            {
                string[] _corner2 = _box[1].Split(',');
                int.TryParse(_corner2[0], out xMax);
                int.TryParse(_corner2[1], out yMax);
                int.TryParse(_corner2[2], out zMax);
                if (VectorCheck(xMin, yMin, zMin, xMax, yMax, zMax, _X, _Y, _Z))
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

        public static bool VectorCheck(int xMin, int yMin, int zMin, int xMax, int yMax, int zMax, int _X, int _Y, int _Z)
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
                                            if (VectorCheck(xMin, yMin, zMin, xMax, yMax, zMax, _X, _Y, _Z))
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
                Log.Out(string.Format("[SERVERTOOLS] Error in EntityCleanup.HostileCheck: {0}.", e));
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
                    if (_timepassed >= Reminder_Delay)
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
