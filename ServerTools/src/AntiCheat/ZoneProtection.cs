using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    class ZoneProtection
    {
        private static int timerInstanceCount = 0;
        public static bool IsEnabled = false;
        public static bool IsRunning = false;
        public static bool KillMurderer = false;
        public static bool JailEnabled = false;
        public static bool KickEnabled = false;
        public static bool BanEnabled = false;
        public static bool ZoneMessage = false;
        public static bool SetHome = false;
        private const string file = "ZoneProtection.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static SortedDictionary<string, string[]> Box = new SortedDictionary<string, string[]>();
        private static SortedDictionary<int, int> PlayerKills = new SortedDictionary<int, int>();
        private static SortedDictionary<int, int> Flag = new SortedDictionary<int, int>();
        public static SortedDictionary<int, Vector3> Victim = new SortedDictionary<int, Vector3>();
        public static SortedDictionary<int, int> Forgive = new SortedDictionary<int, int>();
        public static List<int> PvEFlag = new List<int>();
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        private static bool updateConfig = false;
        private static System.Timers.Timer t = new System.Timers.Timer();
        private static int _xMinCheck = 0;
        private static int _yMinCheck = 0;
        private static int _zMinCheck = 0;
        private static int _xMaxCheck = 0;
        private static int _yMaxCheck = 0;
        private static int _zMaxCheck = 0;

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
                        string _name = _line.GetAttribute("name");
                        string[] box = { _line.GetAttribute("corner1"), _line.GetAttribute("corner2") };
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
                        sw.WriteLine(string.Format("        <zone name=\"{0}\" corner1=\"{1}\" corner2=\"{2}\" />", kvpBox.Key, kvpBox.Value[0], kvpBox.Value[1]));
                    }
                }
                else
                {
                    sw.WriteLine("        <zone name=\"Market\" corner1=\"-100,60,-90\" corner2=\"-140,70,-110\" />");
                    sw.WriteLine("        <zone name=\"Lobby\" corner1=\"0,100,0\" corner2=\"25,105,25\" />");
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

        public static void ZoneProtectionTimerStart()
        {
            timerInstanceCount++;
            if (timerInstanceCount <= 1)
            {
                int d = 500;
                t.Interval = d;
                t.Start();
                t.Elapsed += new ElapsedEventHandler(KillCheck);
                t.Elapsed += new ElapsedEventHandler(ZoneCheck);
            }
        }

        public static void ZoneProtectionTimerStop()
        {
            t.Stop();
            PvEFlag.Clear();
        }

        public static void KillCheck(object sender, ElapsedEventArgs e)
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
                                    if (PvEFlag.Contains(_cInfoVictim.entityId) & PvEFlag.Contains(_cInfoKiller.entityId))
                                    {
                                        _cInfoVictim.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} has murdered you while you were in a protected zone.[-]", Config.ChatResponseColor, _cInfoKiller.playerName), "Server", false, "", false));
                                        _cInfoKiller.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You have murdered {1} while you were inside a protected zone.[-]", Config.ChatResponseColor, _cInfoVictim.playerName), "Server", false, "", false));
                                        Penalty(_cInfoKiller, _cInfoVictim);
                                        if (Victim.ContainsKey(_cInfoVictim.entityId))
                                        {
                                            Victim.Remove(_cInfoVictim.entityId);
                                        }
                                        Victim.Add(_cInfoVictim.entityId, _victim.position);                                                                                
                                    }
                                    if (PvEFlag.Contains(_cInfoVictim.entityId) & !PvEFlag.Contains(_cInfoKiller.entityId))
                                    {
                                        _cInfoVictim.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} has murdered you while you were in a protected zone.[-]", Config.ChatResponseColor, _cInfoKiller.playerName), "Server", false, "", false));
                                        _cInfoKiller.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You have murdered {1}. They were inside a protected zone.[-]", Config.ChatResponseColor, _cInfoVictim.playerName), "Server", false, "", false));
                                        Penalty(_cInfoKiller, _cInfoVictim);
                                        if (Victim.ContainsKey(_cInfoVictim.entityId))
                                        {
                                            Victim.Remove(_cInfoVictim.entityId);
                                        }
                                        Victim.Add(_cInfoVictim.entityId, _victim.position);
                                    }
                                    if (!PvEFlag.Contains(_cInfoVictim.entityId) & PvEFlag.Contains(_cInfoKiller.entityId))
                                    {
                                        _cInfoVictim.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} has murdered you while they were in a protected zone.[-]", Config.ChatResponseColor, _cInfoKiller.playerName), "Server", false, "", false));
                                        _cInfoKiller.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You have murdered {1} while you were inside a protected zone.[-]", Config.ChatResponseColor, _cInfoVictim.playerName), "Server", false, "", false));
                                        Penalty(_cInfoKiller, _cInfoVictim);
                                        if (Victim.ContainsKey(_cInfoVictim.entityId))
                                        {
                                            Victim.Remove(_cInfoVictim.entityId);
                                        }
                                        Victim.Add(_cInfoVictim.entityId, _victim.position);
                                        string _file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
                                        string _filepath = string.Format("{0}/DetectionLogs/{1}", API.GamePath, _file);
                                        using (StreamWriter sw = new StreamWriter(_filepath, true))
                                        {
                                            sw.WriteLine(string.Format("Detected {0}, Steam Id {1}, murdered {2}, Steam Id {3} in a protected zone.", _cInfoKiller.playerName, _cInfoKiller.steamId, _cInfoVictim.playerName, _cInfoVictim.steamId));
                                            sw.WriteLine();
                                            sw.Flush();
                                            sw.Close();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void ZoneCheck(object sender, ElapsedEventArgs e)
        {
            List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
            foreach (var _cInfo in _cInfoList)
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                Flag.Remove(_cInfo.entityId);
                if (!_player.IsDead())
                {
                    int _playerX = (int)_player.position.x;
                    int _playerY = (int)_player.position.y;
                    int _playerZ = (int)_player.position.z;

                    if (Box.Count > 0)
                    {
                        foreach (KeyValuePair<string, string[]> kvpCorners in Box)
                        {
                            float xMin;
                            float yMin;
                            float zMin;
                            string[] __corner1 = kvpCorners.Value[0].Split(',');
                            float.TryParse(__corner1[0], out xMin);
                            float.TryParse(__corner1[1], out yMin);
                            float.TryParse(__corner1[2], out zMin);
                            float xMax;
                            float yMax;
                            float zMax;
                            string[] __corner2 = kvpCorners.Value[1].Split(',');
                            float.TryParse(__corner2[0], out xMax);
                            float.TryParse(__corner2[1], out yMax);
                            float.TryParse(__corner2[2], out zMax);


                            if (xMin >= 0 & xMax > 0)
                            {
                                if (xMin < xMax)
                                {
                                    if (_playerX >= xMin)
                                    {
                                        _xMinCheck = 1;
                                    }
                                    else
                                    {
                                        _xMinCheck = 0;
                                    }
                                    if (_playerX <= xMax)
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
                                    if (_playerX <= xMin)
                                    {
                                        _xMinCheck = 1;
                                    }
                                    else
                                    {
                                        _xMinCheck = 0;
                                    }
                                    if (_playerX >= xMax)
                                    {
                                        _xMaxCheck = 1;
                                    }
                                    else
                                    {
                                        _xMaxCheck = 0;
                                    }
                                }
                            }
                            else if (xMin < 0 & xMax < 0)
                            {
                                if (xMin < xMax)
                                {
                                    if (_playerX >= xMin)
                                    {
                                        _xMinCheck = 1;
                                    }
                                    else
                                    {
                                        _xMinCheck = 0;
                                    }
                                    if (_playerX <= xMax)
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
                                    if (_playerX <= xMin)
                                    {
                                        _xMinCheck = 1;
                                    }
                                    else
                                    {
                                        _xMinCheck = 0;
                                    }
                                    if (_playerX >= xMax)
                                    {
                                        _xMaxCheck = 1;
                                    }
                                    else
                                    {
                                        _xMaxCheck = 0;
                                    }
                                }
                            }
                            else if (xMin < 0 & xMax > 0)
                            {
                                if (_playerX >= xMin)
                                {
                                    _xMinCheck = 1;
                                }
                                else
                                {
                                    _xMinCheck = 0;
                                }
                                if (_playerX <= xMax)
                                {
                                    _xMaxCheck = 1;
                                }
                                else
                                {
                                    _xMaxCheck = 0;
                                }
                            }
                            else if (xMin > 0 & xMax < 0)
                            {
                                if (_playerX <= xMin)
                                {
                                    _xMinCheck = 1;
                                }
                                else
                                {
                                    _xMinCheck = 0;
                                }
                                if (_playerX >= xMax)
                                {
                                    _xMaxCheck = 1;
                                }
                                else
                                {
                                    _xMaxCheck = 0;
                                }
                            }

                            if (yMin >= 0 & yMax > 0)
                            {
                                if (yMin < yMax)
                                {
                                    if (_playerY >= yMin)
                                    {
                                        _yMinCheck = 1;
                                    }
                                    else
                                    {
                                        _yMinCheck = 0;
                                    }
                                    if (_playerY <= yMax)
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
                                    if (_playerY <= yMin)
                                    {
                                        _yMinCheck = 1;
                                    }
                                    else
                                    {
                                        _yMinCheck = 0;
                                    }
                                    if (_playerY >= yMax)
                                    {
                                        _yMaxCheck = 1;
                                    }
                                    else
                                    {
                                        _yMaxCheck = 0;
                                    }
                                }
                            }
                            else if (yMin < 0 & yMax < 0)
                            {
                                if (yMin < yMax)
                                {
                                    if (_playerY >= yMin)
                                    {
                                        _yMinCheck = 1;
                                    }
                                    else
                                    {
                                        _yMinCheck = 0;
                                    }
                                    if (_playerY <= yMax)
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
                                    if (_playerY <= yMin)
                                    {
                                        _yMinCheck = 1;
                                    }
                                    else
                                    {
                                        _yMinCheck = 0;
                                    }
                                    if (_playerY >= yMax)
                                    {
                                        _yMaxCheck = 1;
                                    }
                                    else
                                    {
                                        _yMaxCheck = 0;
                                    }
                                }
                            }
                            else if (yMin < 0 & yMax >= 0)
                            {
                                if (_playerY >= yMin)
                                {
                                    _yMinCheck = 1;
                                }
                                else
                                {
                                    _yMinCheck = 0;
                                }
                                if (_playerY <= yMax)
                                {
                                    _yMaxCheck = 1;
                                }
                                else
                                {
                                    _yMaxCheck = 0;
                                }
                            }
                            else if (yMin > 0 & yMax < 0)
                            {
                                if (_playerY <= yMin)
                                {
                                    _yMinCheck = 1;
                                }
                                else
                                {
                                    _yMinCheck = 0;
                                }
                                if (_playerY >= yMax)
                                {
                                    _yMaxCheck = 1;
                                }
                                else
                                {
                                    _yMaxCheck = 0;
                                }
                            }

                            if (zMin >= 0 & zMax > 0)
                            {
                                if (zMin < zMax)
                                {
                                    if (_playerZ >= zMin)
                                    {
                                        _zMinCheck = 1;
                                    }
                                    else
                                    {
                                        _zMinCheck = 0;
                                    }
                                    if (_playerZ <= zMax)
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
                                    if (_playerZ <= zMin)
                                    {
                                        _zMinCheck = 1;
                                    }
                                    else
                                    {
                                        _zMinCheck = 0;
                                    }
                                    if (_playerZ >= zMax)
                                    {
                                        _zMaxCheck = 1;
                                    }
                                    else
                                    {
                                        _zMaxCheck = 0;
                                    }
                                }
                            }
                            else if (zMin < 0 & zMax < 0)
                            {
                                if (zMin < zMax)
                                {
                                    if (_playerZ >= zMin)
                                    {
                                        _zMinCheck = 1;
                                    }
                                    else
                                    {
                                        _zMinCheck = 0;
                                    }
                                    if (_playerZ <= zMax)
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
                                    if (_playerZ <= zMin)
                                    {
                                        _zMinCheck = 1;
                                    }
                                    else
                                    {
                                        _zMinCheck = 0;
                                    }
                                    if (_playerZ >= zMax)
                                    {
                                        _zMaxCheck = 1;
                                    }
                                    else
                                    {
                                        _zMaxCheck = 0;
                                    }
                                }
                            }
                            else if (zMin < 0 & zMax >= 0)
                            {
                                if (_playerZ >= zMin)
                                {
                                    _zMinCheck = 1;
                                }
                                else
                                {
                                    _zMinCheck = 0;
                                }
                                if (_playerZ <= zMax)
                                {
                                    _zMaxCheck = 1;
                                }
                                else
                                {
                                    _zMaxCheck = 0;
                                }
                            }
                            else if (zMin > 0 & zMax < 0)
                            {
                                if (_playerY <= zMin)
                                {
                                    _zMinCheck = 1;
                                }
                                else
                                {
                                    _zMinCheck = 0;
                                }
                                if (_playerY >= zMax)
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
                                if (!PvEFlag.Contains(_cInfo.entityId))
                                {
                                    if (ZoneMessage)
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You have entered {1}. Do not harm other players while in this zone.[-]", Config.ChatResponseColor, kvpCorners.Key), "Server", false, "", false));
                                    }
                                    PvEFlag.Add(_cInfo.entityId);
                                }
                            }
                            else
                            {
                                if (Flag.ContainsKey(_cInfo.entityId))
                                {
                                    int _flag = 0;
                                    if (Flag.TryGetValue(_cInfo.entityId, out _flag))
                                    {
                                        int _flag1 = _flag + 1;
                                        if (_flag1 == Box.Count & PvEFlag.Contains(_cInfo.entityId))
                                        {
                                            if (ZoneMessage)
                                            {
                                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You have exited the protected zone.[-]", Config.ChatResponseColor), "Server", false, "", false));
                                            }
                                            PvEFlag.Remove(_cInfo.entityId);
                                        }
                                        else
                                        {
                                            Flag.Remove(_cInfo.entityId);
                                            Flag.Add(_cInfo.entityId, _flag1);
                                        }
                                    }
                                }
                                else
                                {
                                    int _flag = 1;
                                    Flag.Add(_cInfo.entityId, _flag);
                                    if (Flag.TryGetValue(_cInfo.entityId, out _flag))
                                    {
                                        if (_flag == Box.Count & PvEFlag.Contains(_cInfo.entityId))
                                        {
                                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You have exited the protected zone.[-]", Config.ChatResponseColor), "Server", false, "", false));
                                            PvEFlag.Remove(_cInfo.entityId);
                                        }
                                    }
                                }
                            }
                        }
                    }
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
            if (JailEnabled)
            {
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} has been jailed for murder in a protected zone.[-]", Config.ChatResponseColor, _cInfoKiller.playerName), "Server", false, "", false);
                SdtdConsole.Instance.ExecuteSync(string.Format("jail add {0}", _cInfoKiller.playerId), (ClientInfo)null);
                if (Forgive.ContainsKey(_cInfoVictim.entityId))
                {
                    Forgive.Remove(_cInfoVictim.entityId);
                }
                Forgive.Add(_cInfoVictim.entityId, _cInfoKiller.entityId);
            }
            if (KillMurderer)
            {
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} has been executed for murder in a protected zone.[-]", Config.ChatResponseColor, _cInfoKiller.playerName), "Server", false, "", false);
                SdtdConsole.Instance.ExecuteSync(string.Format("kill {0}", _cInfoKiller.playerId), (ClientInfo)null);
            }
            if (KickEnabled)
            {
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} has been kicked for murder in a protected zone.[-]", Config.ChatResponseColor, _cInfoKiller.playerName), "Server", false, "", false);
                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for murder in a protected zone\"", _cInfoKiller.playerId), (ClientInfo)null);
            }
            if (BanEnabled)
            {
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} has been banned for murder in a protected zone.[-]", Config.ChatResponseColor, _cInfoKiller.playerName), "Server", false, "", false);
                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for murder in a protected zone\"", _cInfoKiller.playerId), (ClientInfo)null);
            }
        }
    }
}
