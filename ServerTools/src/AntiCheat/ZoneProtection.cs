using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    class ZoneProtection
    {
        public static bool IsEnabled = false, IsRunning = false, Kill_Enabled = false, Jail_Enabled = false, Kick_Enabled = false, Ban_Enabled = false, Zone_Message = false, Set_Home = false;
        public static int Days_Before_Log_Delete = 5;      
        private const string file = "ZoneProtection.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static SortedDictionary<string, string[]> Box = new SortedDictionary<string, string[]>();
        private static SortedDictionary<int, int> PlayerKills = new SortedDictionary<int, int>();
        private static SortedDictionary<int, int> Flag = new SortedDictionary<int, int>();
        public static SortedDictionary<int, Vector3> Victim = new SortedDictionary<int, Vector3>();
        public static SortedDictionary<int, int> Forgive = new SortedDictionary<int, int>();
        public static SortedDictionary<int, string> PvEFlag = new SortedDictionary<int, string>();
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        private static bool updateConfig = false;
        private static int _xMinCheck = 0, _yMinCheck = 0, _zMinCheck = 0, _xMaxCheck = 0, _yMaxCheck = 0, _zMaxCheck = 0;

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
            Box.Clear();
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
                    Box.Clear();
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
                        if (!_line.HasAttribute("name"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone Protection entry because of missing name attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("corner1"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone Protection entry because of missing corner1 attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("corner2"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone Protection entry because of missing corner2 attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("entryMessage"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone Protection entry because of missing entryMessage attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("exitMessage"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Zone Protection entry because of missing exitMessage attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string _name = _line.GetAttribute("name");
                        string[] box = { _line.GetAttribute("corner1"), _line.GetAttribute("corner2"), _line.GetAttribute("entryMessage"), _line.GetAttribute("exitMessage") };
                        if (!Box.ContainsKey(_name))
                        {
                            Box.Add(_name, box);
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

        private static void UpdateXml()
        {
            fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<ZoneProtection>");
                sw.WriteLine("    <Zone>");
                if (Box.Count > 0)
                {
                    foreach (KeyValuePair<string, string[]> kvpBox in Box)
                    {
                        sw.WriteLine(string.Format("        <zone name=\"{0}\" corner1=\"{1}\" corner2=\"{2}\" entryMessage=\"{3}\" exitMessage=\"{4}\" />", kvpBox.Key, kvpBox.Value[0], kvpBox.Value[1], kvpBox.Value[2], kvpBox.Value[3]));
                    }
                }
                else
                {
                    sw.WriteLine("        <zone name=\"Market\" corner1=\"-100,60,-90\" corner2=\"-140,70,-110\" entryMessage=\"You are now entering the Market\" exitMessage=\"You are exiting the Market\" />");
                    sw.WriteLine("        <zone name=\"Lobby\" corner1=\"0,100,0\" corner2=\"25,105,25\" entryMessage=\"You are now entering the Lobby\" exitMessage=\"You are exiting the Lobby\" />");
                }
                sw.WriteLine("    </Zone>");
                sw.WriteLine("</ZoneProtection>");
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

        public static void Check()
        {
            List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
            foreach (var _cInfoKiller in _cInfoList)
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfoKiller.entityId];
                int _playerKills = _player.KilledPlayers;
                if (!PlayerKills.ContainsKey(_cInfoKiller.entityId))
                {
                    PlayerKills.Add(_cInfoKiller.entityId, _playerKills);
                }
                else
                {
                    int _kills;
                    if (PlayerKills.TryGetValue(_cInfoKiller.entityId, out _kills))
                    {
                        if (_playerKills > _kills)
                        {
                            PlayerKills.Remove(_cInfoKiller.entityId);
                            PlayerKills.Add(_cInfoKiller.entityId, _playerKills);
                            var _victim = _player.GetDamagedTarget();
                            if (_victim != null && _cInfoKiller.entityId != _victim.entityId)
                            {
                                ClientInfo _cInfoVictim = ConnectionManager.Instance.GetClientInfoForEntityId(_victim.entityId);
                                if (_cInfoVictim != null)
                                {
                                    if (PvEFlag.ContainsKey(_cInfoVictim.entityId) & PvEFlag.ContainsKey(_cInfoKiller.entityId))
                                    {
                                        string _phrase801;
                                        if (!Phrases.Dict.TryGetValue(801, out _phrase801))
                                        {
                                            _phrase801 = "{Killer} has murdered you while you were in a protected zone.";
                                        }
                                        _phrase801 = _phrase801.Replace("{Killer}", _cInfoKiller.playerName);
                                        _cInfoVictim.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase801), "Server", false, "", false));
                                        string _phrase802;
                                        if (!Phrases.Dict.TryGetValue(802, out _phrase802))
                                        {
                                            _phrase802 = "You have murdered a player inside a protected zone. Their name was {Victim}";
                                        }
                                        _phrase802 = _phrase802.Replace("{Victim}", _cInfoVictim.playerName);
                                        _cInfoKiller.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase802), "Server", false, "", false));
                                        Penalty(_cInfoKiller, _cInfoVictim);
                                        if (Victim.ContainsKey(_cInfoVictim.entityId))
                                        {
                                            Victim.Remove(_cInfoVictim.entityId);
                                        }
                                        Victim.Add(_cInfoVictim.entityId, _victim.position);
                                        continue;                                                                             
                                    }
                                    if (PvEFlag.ContainsKey(_cInfoVictim.entityId) & !PvEFlag.ContainsKey(_cInfoKiller.entityId))
                                    {
                                        string _phrase801;
                                        if (!Phrases.Dict.TryGetValue(801, out _phrase801))
                                        {
                                            _phrase801 = "{Killer} has murdered you while you were in a protected zone.";
                                        }
                                        _phrase801 = _phrase801.Replace("{Killer}", _cInfoKiller.playerName);
                                        _cInfoVictim.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase801), "Server", false, "", false));
                                        string _phrase802;
                                        if (!Phrases.Dict.TryGetValue(802, out _phrase802))
                                        {
                                            _phrase802 = "You have murdered a player inside a protected zone. Their name was {Victim}";
                                        }
                                        _phrase802 = _phrase802.Replace("{Victim}", _cInfoVictim.playerName);
                                        _cInfoKiller.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase802), "Server", false, "", false));
                                        Penalty(_cInfoKiller, _cInfoVictim);
                                        if (Victim.ContainsKey(_cInfoVictim.entityId))
                                        {
                                            Victim.Remove(_cInfoVictim.entityId);
                                        }
                                        Victim.Add(_cInfoVictim.entityId, _victim.position);
                                        continue;
                                    }
                                    if (!PvEFlag.ContainsKey(_cInfoVictim.entityId) & PvEFlag.ContainsKey(_cInfoKiller.entityId))
                                    {
                                        string _phrase803;
                                        if (!Phrases.Dict.TryGetValue(801, out _phrase803))
                                        {
                                            _phrase803 = "{Killer} has murdered you while they were in a protected zone.";
                                        }
                                        _phrase803 = _phrase803.Replace("{Killer}", _cInfoKiller.playerName);
                                        _cInfoVictim.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase803), "Server", false, "", false));
                                        string _phrase802;
                                        if (!Phrases.Dict.TryGetValue(802, out _phrase802))
                                        {
                                            _phrase802 = "You have murdered a player inside a protected zone. Their name was {Victim}";
                                        }
                                        _phrase802 = _phrase802.Replace("{Victim}", _cInfoVictim.playerName);
                                        _cInfoKiller.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase802), "Server", false, "", false));
                                        Penalty(_cInfoKiller, _cInfoVictim);
                                        if (Victim.ContainsKey(_cInfoVictim.entityId))
                                        {
                                            Victim.Remove(_cInfoVictim.entityId);
                                        }
                                        Victim.Add(_cInfoVictim.entityId, _victim.position);
                                        string _file1 = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
                                        string _filepath1 = string.Format("{0}/DetectionLogs/{1}", API.GamePath, _file1);
                                        using (StreamWriter sw = new StreamWriter(_filepath1, true))
                                        {
                                            sw.WriteLine(string.Format("Detected {0}, Steam Id {1}, murdered {2}, Steam Id {3} in a protected zone.", _cInfoKiller.playerName, _cInfoKiller.steamId, _cInfoVictim.playerName, _cInfoVictim.steamId));
                                            sw.WriteLine();
                                            sw.Flush();
                                            sw.Close();
                                        }
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                }
                if (!_player.IsDead())
                {
                    int _X = (int)_player.position.x;
                    int _Y = (int)_player.position.y;
                    int _Z = (int)_player.position.z;
                    if (Box.Count > 0)
                    {
                        Flag.Remove(_cInfoKiller.entityId);
                        foreach (KeyValuePair<string, string[]> kvpCorners in Box)
                        {
                            float xMin;
                            float yMin;
                            float zMin;
                            string[] _corner1 = kvpCorners.Value[0].Split(',');
                            float.TryParse(_corner1[0], out xMin);
                            float.TryParse(_corner1[1], out yMin);
                            float.TryParse(_corner1[2], out zMin);
                            float xMax;
                            float yMax;
                            float zMax;
                            string[] _corner2 = kvpCorners.Value[1].Split(',');
                            float.TryParse(_corner2[0], out xMax);
                            float.TryParse(_corner2[1], out yMax);
                            float.TryParse(_corner2[2], out zMax);
                            if (xMin >= 0 & xMax >= 0)
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
                            else if (xMin <= 0 & xMax <= 0)
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
                            else if (xMin <= 0 & xMax >= 0)
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
                            else if (xMin >= 0 & xMax <= 0)
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

                            if (yMin >= 0 & yMax >= 0)
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
                            else if (yMin <= 0 & yMax <= 0)
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
                            else if (yMin <= 0 & yMax >= 0)
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
                            else if (yMin >= 0 & yMax <= 0)
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

                            if (zMin >= 0 & zMax >= 0)
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
                            else if (zMin <= 0 & zMax <= 0)
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
                            else if (zMin <= 0 & zMax >= 0)
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
                            else if (zMin >= 0 & zMax <= 0)
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
                            if (_xMinCheck == 1 & _yMinCheck == 1 & _zMinCheck == 1 & _xMaxCheck == 1 & _yMaxCheck == 1 & _zMaxCheck == 1)
                            {
                                if (!PvEFlag.ContainsKey(_cInfoKiller.entityId))
                                {
                                    if (Zone_Message)
                                    {
                                        _cInfoKiller.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, kvpCorners.Value[2]), "Server", false, "", false));
                                    }
                                    PvEFlag.Add(_cInfoKiller.entityId, kvpCorners.Value[3]);
                                }
                                else
                                {
                                    string _msg;
                                    if (PvEFlag.TryGetValue(_cInfoKiller.entityId, out _msg))
                                    {
                                        if (_msg != kvpCorners.Value[2])
                                        {
                                            PvEFlag.Remove(_cInfoKiller.entityId);
                                            PvEFlag.Add(_cInfoKiller.entityId, kvpCorners.Value[3]);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (Flag.ContainsKey(_cInfoKiller.entityId))
                                {
                                    int _flag = 0;
                                    if (Flag.TryGetValue(_cInfoKiller.entityId, out _flag))
                                    {
                                        int _flag1 = _flag + 1;
                                        if (_flag1 == Box.Count & PvEFlag.ContainsKey(_cInfoKiller.entityId))
                                        {
                                            if (Zone_Message)
                                            {
                                                string _msg;
                                                if (PvEFlag.TryGetValue(_cInfoKiller.entityId, out _msg))
                                                {
                                                    _cInfoKiller.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _msg), "Server", false, "", false));
                                                }
                                            }
                                            PvEFlag.Remove(_cInfoKiller.entityId);
                                        }
                                        else
                                        {
                                            Flag.Remove(_cInfoKiller.entityId);
                                            Flag.Add(_cInfoKiller.entityId, _flag1);
                                        }
                                    }
                                }
                                else
                                {
                                    int _flag = 1;
                                    Flag.Add(_cInfoKiller.entityId, _flag);
                                    if (Flag.TryGetValue(_cInfoKiller.entityId, out _flag))
                                    {
                                        if (_flag == Box.Count & PvEFlag.ContainsKey(_cInfoKiller.entityId))
                                        {
                                            if (Zone_Message)
                                            {
                                                string _msg;
                                                if (PvEFlag.TryGetValue(_cInfoKiller.entityId, out _msg))
                                                {
                                                    _cInfoKiller.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _msg), "Server", false, "", false));
                                                }
                                            }
                                            PvEFlag.Remove(_cInfoKiller.entityId);
                                        }
                                    }
                                }
                            }
                        }
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
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            TimeSpan varTime = DateTime.Now - p.RespawnTime;
            double fractionalMinutes = varTime.TotalMinutes;
            int _timepassed = (int)fractionalMinutes;
            if (_timepassed < 2)
            {
                Vector3 _deathPos;
                if (Victim.TryGetValue(_cInfo.entityId, out _deathPos))
                {
                    _cInfo.SendPackage(new NetPackageTeleportPlayer(_deathPos));
                    Victim.Remove(_cInfo.entityId);
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
                Victim.Remove(_cInfo.entityId);
            }
        }

        public static void Penalty(ClientInfo _cInfoKiller, ClientInfo _cInfoVictim)
        {
            if (Jail_Enabled)
            {
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} has been jailed for murder in a protected zone.[-]", Config.Chat_Response_Color, _cInfoKiller.playerName), "Server", false, "", false);
                SdtdConsole.Instance.ExecuteSync(string.Format("jail add {0}", _cInfoKiller.playerId), (ClientInfo)null);
                if (Forgive.ContainsKey(_cInfoVictim.entityId))
                {
                    Forgive.Remove(_cInfoVictim.entityId);
                }
                Forgive.Add(_cInfoVictim.entityId, _cInfoKiller.entityId);
            }
            if (Kill_Enabled)
            {
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} has been executed for murder in a protected zone.[-]", Config.Chat_Response_Color, _cInfoKiller.playerName), "Server", false, "", false);
                SdtdConsole.Instance.ExecuteSync(string.Format("kill {0}", _cInfoKiller.playerId), (ClientInfo)null);
            }
            if (Kick_Enabled)
            {
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} has been kicked for murder in a protected zone.[-]", Config.Chat_Response_Color, _cInfoKiller.playerName), "Server", false, "", false);
                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for murder in a protected zone\"", _cInfoKiller.playerId), (ClientInfo)null);
            }
            if (Ban_Enabled)
            {
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} has been banned for murder in a protected zone.[-]", Config.Chat_Response_Color, _cInfoKiller.playerName), "Server", false, "", false);
                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for murder in a protected zone\"", _cInfoKiller.playerId), (ClientInfo)null);
            }
        }
    }
}
