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
        public static int Days_Before_Log_Delete = 5;
        public static Dictionary<int, string> corner1 = new Dictionary<int, string>();
        public static Dictionary<int, string> corner2 = new Dictionary<int, string>();
        public static Dictionary<int, string> entry = new Dictionary<int, string>();
        public static Dictionary<int, string> exit = new Dictionary<int, string>();
        public static Dictionary<int, string> response = new Dictionary<int, string>();
        public static Dictionary<int, bool> pve = new Dictionary<int, bool>();
        public static Dictionary<int, bool> nozombie = new Dictionary<int, bool>();
        public static Dictionary<int, DateTime> reminder = new Dictionary<int, DateTime>();
        public static Dictionary<int, string> reminderMsg = new Dictionary<int, string>();
        private static int _xMinCheck = 0, _yMinCheck = 0, _zMinCheck = 0, _xMaxCheck = 0, _yMaxCheck = 0, _zMaxCheck = 0;
        private const string file = "Zones.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        private static bool updateConfig = false;

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
            Players.Box.Clear();
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
                    Players.Box.Clear();
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
                        if (!_line.HasAttribute("reminderNotice"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because of missing reminderNotice attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        else
                        {
                            string _pve = _line.GetAttribute("PvE");
                            bool _result, _result2;
                            if (!bool.TryParse(_pve, out _result))
                            {
                                Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because improper true/false attribute: {0}.", subChild.OuterXml));
                                continue;
                            }
                            string _noZ = _line.GetAttribute("noZombie");
                            if (!bool.TryParse(_noZ, out _result2))
                            {
                                Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zones entry because improper true/false attribute: {0}.", subChild.OuterXml));
                                continue;
                            }
                            string[] box = { _line.GetAttribute("corner1"), _line.GetAttribute("corner2"), _line.GetAttribute("entryMessage"), _line.GetAttribute("exitMessage"),
                            _line.GetAttribute("response"), _line.GetAttribute("PvE"), _line.GetAttribute("noZombie"), _line.GetAttribute("reminderNotice") };
                            if (!Players.Box.Contains(box))
                            {
                                Players.Box.Add(box);
                            }
                        }
                    }
                }
            }
            if (updateConfig)
            {
                updateConfig = false;
                UpdateXml();
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
                if (Players.Box.Count > 0)
                {
                    for (int i = 0; i < Players.Box.Count; i++)
                    {
                        string[] _box = Players.Box[i];
                        sw.WriteLine(string.Format("        <zone corner1=\"{0}\" corner2=\"{1}\" entryMessage=\"{2}\" exitMessage=\"{3}\" response=\"{4}\" PvE=\"{5}\" noZombie=\"{6}\" reminderNotice=\"{7}\" />", _box[0], _box[1], _box[2], _box[3], _box[4], _box[5], _box[6], _box[7]));
                    }
                }
                else
                {
                    sw.WriteLine("        <zone corner1=\"-8000,-56,8000\" corner2=\"8000,200,0\" entryMessage=\"You are entering the Northern side\" exitMessage=\"You have exited the Northern Side\" response=\"\" PvE=\"false\" noZombie=\"false\" reminderNotice=\"You are still in the North\" />");
                    sw.WriteLine("        <zone corner1=\"-8000,-56,-1\" corner2=\"8000,200,-8000\" entryMessage=\"You are entering the Southern side\" exitMessage=\"You have exited the Southern Side\" response=\"\" PvE=\"false\" noZombie=\"false\" reminderNotice=\"You are still in the South\" />");
                    sw.WriteLine("        <zone corner1=\"-100,60,-90\" corner2=\"-140,70,-110\" entryMessage=\"You have entered the Market\" exitMessage=\"You have exited the Market\" response=\"say {PlayerName} has entered the market\" PvE=\"true\" noZombie=\"true\" reminderNotice=\"\" />");
                    sw.WriteLine("        <zone corner1=\"0,100,0\" corner2=\"25,105,25\" entryMessage=\"You have entered the Lobby\" exitMessage=\"You have exited the Lobby\" response=\"say {PlayerName} has entered the lobby\" PvE=\"true\" noZombie=\"true\" reminderNotice=\"You have been in the lobby for a long time...\" />");
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
            if (Players.ZoneExit.ContainsKey(_cInfoVictim.entityId) || Players.ZoneExit.ContainsKey(_cInfoKiller.entityId))
            {
                if (Players.ZonePvE.Contains(_cInfoVictim.entityId) && Players.ZonePvE.Contains(_cInfoKiller.entityId))
                {
                    string _phrase801;
                    if (!Phrases.Dict.TryGetValue(801, out _phrase801))
                    {
                        _phrase801 = "{Killer} has murdered you while you were in a protected zone.";
                    }
                    _phrase801 = _phrase801.Replace("{Killer}", _cInfoKiller.playerName);
                    _cInfoVictim.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase801), Config.Server_Response_Name, false, "ServerTools", false));
                    string _phrase802;
                    if (!Phrases.Dict.TryGetValue(802, out _phrase802))
                    {
                        _phrase802 = "You have murdered a player inside a protected zone. Their name was {Victim}";
                    }
                    _phrase802 = _phrase802.Replace("{Victim}", _cInfoVictim.playerName);
                    _cInfoKiller.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase802), Config.Server_Response_Name, false, "ServerTools", false));
                    Penalty(_cInfoKiller, _cInfoVictim);
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfoVictim.entityId];
                    int x = (int)_player.position.x;
                    int y = (int)_player.position.y;
                    int z = (int)_player.position.z;
                    string _sposition = x + "," + y + "," + z;
                    if (Players.Victim.ContainsKey(_cInfoVictim.entityId))
                    {
                        Players.Victim[_cInfoVictim.entityId] = _sposition;
                    }
                    else
                    {
                        Players.Victim.Add(_cInfoVictim.entityId, _sposition);
                    }
                }
                if (Players.ZonePvE.Contains(_cInfoVictim.entityId) & !Players.ZonePvE.Contains(_cInfoKiller.entityId))
                {
                    string _phrase801;
                    if (!Phrases.Dict.TryGetValue(801, out _phrase801))
                    {
                        _phrase801 = "{Killer} has murdered you while you were in a protected zone.";
                    }
                    _phrase801 = _phrase801.Replace("{Killer}", _cInfoKiller.playerName);
                    _cInfoVictim.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase801), Config.Server_Response_Name, false, "ServerTools", false));
                    string _phrase802;
                    if (!Phrases.Dict.TryGetValue(802, out _phrase802))
                    {
                        _phrase802 = "You have murdered a player inside a protected zone. Their name was {Victim}";
                    }
                    _phrase802 = _phrase802.Replace("{Victim}", _cInfoVictim.playerName);
                    _cInfoKiller.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase802), Config.Server_Response_Name, false, "ServerTools", false));
                    Penalty(_cInfoKiller, _cInfoVictim);
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfoVictim.entityId];
                    int x = (int)_player.position.x;
                    int y = (int)_player.position.y;
                    int z = (int)_player.position.z;
                    string _sposition = x + "," + y + "," + z;
                    if (Players.Victim.ContainsKey(_cInfoVictim.entityId))
                    {
                        Players.Victim[_cInfoVictim.entityId] = _sposition;
                    }
                    else
                    {
                        Players.Victim.Add(_cInfoVictim.entityId, _sposition);
                    }
                }
                if (!Players.ZonePvE.Contains(_cInfoVictim.entityId) & Players.ZonePvE.Contains(_cInfoKiller.entityId))
                {
                    string _phrase803;
                    if (!Phrases.Dict.TryGetValue(801, out _phrase803))
                    {
                        _phrase803 = "{Killer} has murdered you while they were in a protected zone.";
                    }
                    _phrase803 = _phrase803.Replace("{Killer}", _cInfoKiller.playerName);
                    _cInfoVictim.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase803), Config.Server_Response_Name, false, "ServerTools", false));
                    string _phrase802;
                    if (!Phrases.Dict.TryGetValue(802, out _phrase802))
                    {
                        _phrase802 = "You have murdered a player inside a protected zone. Their name was {Victim}";
                    }
                    _phrase802 = _phrase802.Replace("{Victim}", _cInfoVictim.playerName);
                    _cInfoKiller.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase802), Config.Server_Response_Name, false, "ServerTools", false));
                    Penalty(_cInfoKiller, _cInfoVictim);
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfoVictim.entityId];
                    int x = (int)_player.position.x;
                    int y = (int)_player.position.y;
                    int z = (int)_player.position.z;
                    string _sposition = x + "," + y + "," + z;
                    if (Players.Victim.ContainsKey(_cInfoVictim.entityId))
                    {
                        Players.Victim[_cInfoVictim.entityId] = _sposition;
                    }
                    else
                    {
                        Players.Victim.Add(_cInfoVictim.entityId, _sposition);
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
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            TimeSpan varTime = DateTime.Now - p.RespawnTime;
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
                            if (Players.Victim.TryGetValue(_cInfo.entityId, out _deathPos))
                            {
                                int x, y, z;
                                string[] _cords = _deathPos.Split(',');
                                int.TryParse(_cords[0], out x);
                                int.TryParse(_cords[1], out y);
                                int.TryParse(_cords[2], out z);
                                _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), false));
                                Players.Victim.Remove(_cInfo.entityId);
                            }
                        }
                        else
                        {
                            string _phrase811;
                            if (!Phrases.Dict.TryGetValue(606, out _phrase811))
                            {
                                _phrase811 = "{PlayerName} you can only use /return for four minutes after respawning. Time has expired.";
                            }
                            _phrase811 = _phrase811.Replace("{PlayerName}", _cInfo.playerName);
                            Players.Victim.Remove(_cInfo.entityId);
                        }
                    }
                }
            }
            if (!_donator)
            {
                if (_timepassed <= 2)
                {
                    string _deathPos;
                    if (Players.Victim.TryGetValue(_cInfo.entityId, out _deathPos))
                    {
                        int x, y, z;
                        string[] _cords = _deathPos.Split(',');
                        int.TryParse(_cords[0], out x);
                        int.TryParse(_cords[1], out y);
                        int.TryParse(_cords[2], out z);
                        _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), false));
                        Players.Victim.Remove(_cInfo.entityId);
                    }
                }
                else
                {
                    string _phrase606;
                    if (!Phrases.Dict.TryGetValue(606, out _phrase606))
                    {
                        _phrase606 = "{PlayerName} you can only use /return for two minutes after respawning. Time has expired.";
                    }
                    _phrase606 = _phrase606.Replace("{PlayerName}", _cInfo.playerName);
                    Players.Victim.Remove(_cInfo.entityId);
                }
            }
        }

        public static void Penalty(ClientInfo _cInfoKiller, ClientInfo _cInfoVictim)
        {
            if (Jail_Enabled)
            {
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} has been jailed for murder in a protected zone.[-]", Config.Chat_Response_Color, _cInfoKiller.playerName), Config.Server_Response_Name, false, "", false);
                SdtdConsole.Instance.ExecuteSync(string.Format("jail add {0} 120", _cInfoKiller.playerId), (ClientInfo)null);
                Players.Forgive[_cInfoVictim.entityId] = _cInfoKiller.entityId;
            }
            if (Kill_Enabled)
            {
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} has been executed for murder in a protected zone.[-]", Config.Chat_Response_Color, _cInfoKiller.playerName), Config.Server_Response_Name, false, "", false);
                SdtdConsole.Instance.ExecuteSync(string.Format("kill {0}", _cInfoKiller.playerId), (ClientInfo)null);
            }
            if (Kick_Enabled)
            {
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} has been kicked for murder in a protected zone.[-]", Config.Chat_Response_Color, _cInfoKiller.playerName), Config.Server_Response_Name, false, "", false);
                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for murder in a protected zone\"", _cInfoKiller.playerId), (ClientInfo)null);
            }
            if (Ban_Enabled)
            {
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} has been banned for murder in a protected zone.[-]", Config.Chat_Response_Color, _cInfoKiller.playerName), Config.Server_Response_Name, false, "", false);
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
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _response), Config.Server_Response_Name, false, "ServerTools", false);
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

        public static bool A(string[] _box, int _X, int _Y, int _Z)
        {
            int xMin, yMin, zMin, xMax, yMax, zMax;
            string[] _corner1 = _box[0].Split(',');
            int.TryParse(_corner1[0], out xMin);
            int.TryParse(_corner1[1], out yMin);
            int.TryParse(_corner1[2], out zMin);
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

        public static void Reminder()
        {
            foreach (KeyValuePair<int,DateTime> time in reminder)
            {
                ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForEntityId(time.Key);
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
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _msg), Config.Server_Response_Name, false, "ServerTools", false));
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
