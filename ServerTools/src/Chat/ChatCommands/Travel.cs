using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    class Travel
    {
        public static bool IsEnabled = false;
        public static bool IsRunning = false;
        public static int DelayBetweenUses = 60;
        private const string file = "TravelLocations.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static SortedDictionary<string, string[]> Box = new SortedDictionary<string, string[]>();
        private static SortedDictionary<string, string> Destination = new SortedDictionary<string, string>();
        private static List<int> Flag = new List<int>();
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        private static bool updateConfig = false;
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
                if (childNode.Name == "Travel")
                {
                    Box.Clear();
                    Destination.Clear();
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'travel' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("name"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Travel entry because of missing name attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("corner1"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Travel entry because of missing corner1 attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("corner2"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Travel entry because of missing corner2 attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("destination"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Travel entry because of missing destination attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string _name = _line.GetAttribute("name");
                        string[] box = { _line.GetAttribute("corner1"), _line.GetAttribute("corner2"), _line.GetAttribute("destination") };

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
                sw.WriteLine("<TravelLocations>");
                sw.WriteLine("    <Travel>");
                if (Box.Count > 0)
                {
                    foreach (KeyValuePair<string, string[]> kvpBox in Box)
                    {
                        sw.WriteLine(string.Format("        <location name=\"{0}\" corner1=\"{1}\" corner2=\"{2}\" destination=\"{3}\"  />", kvpBox.Key, kvpBox.Value[0], kvpBox.Value[1], kvpBox.Value[2]));
                    }
                }
                else
                {
                    sw.WriteLine("        <location name=\"zone1\" corner1=\"0,100,0\" corner2=\"10,100,10\" destination=\"-100,-1,-100\" />");
                    sw.WriteLine("        <location name=\"zone2\" corner1=\"-1,100,-1\" corner2=\"-10,100,-10\" destination=\"100,-1,100\" />");
                }
                sw.WriteLine("    </Travel>");
                sw.WriteLine("</TravelLocations>");
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

        public static void Check(ClientInfo _cInfo, bool _announce, string _playerName)
        {
            if (DelayBetweenUses < 1)
            {
                Tele(_cInfo, _announce);
            }
            else
            {
                Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                if (p == null || p.LastTravel == null)
                {
                    Tele(_cInfo, _announce);
                }
                else
                {
                    TimeSpan varTime = DateTime.Now - p.LastTravel;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int _timepassed = (int)fractionalMinutes;
                    if (_timepassed > DelayBetweenUses)
                    {
                        Tele(_cInfo, _announce);
                    }
                    else
                    {
                        int _timeleft = DelayBetweenUses - _timepassed;
                        string _phrase605;
                        if (!Phrases.Dict.TryGetValue(605, out _phrase605))
                        {
                            _phrase605 = "{PlayerName} you can only use /travel once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                        }
                        _phrase605 = _phrase605.Replace("{PlayerName}", _playerName);
                        _phrase605 = _phrase605.Replace("{DelayBetweenUses}", DelayBetweenUses.ToString());
                        _phrase605 = _phrase605.Replace("{TimeRemaining}", _timeleft.ToString());
                        if (_announce)
                        {
                            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.ChatResponseColor, _phrase605), "Server", false, "", false);
                        }
                        else
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.ChatResponseColor, _phrase605), "Server", false, "", false));
                        }
                    }
                }
            }
        }

        public static void Tele(ClientInfo _cInfo, bool _announce)
        {
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            if (_player.entityId == _cInfo.entityId)
            {
                Flag.Clear();

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
                        string[] _xyzCorner1 = kvpCorners.Value[0].Split(',');
                        float.TryParse(_xyzCorner1[0], out xMin);
                        float.TryParse(_xyzCorner1[1], out yMin);
                        float.TryParse(_xyzCorner1[2], out zMin);
                        float xMax;
                        float yMax;
                        float zMax;
                        string[] _xyzCorner2 = kvpCorners.Value[1].Split(',');
                        float.TryParse(_xyzCorner2[0], out xMax);
                        float.TryParse(_xyzCorner2[1], out yMax);
                        float.TryParse(_xyzCorner2[2], out zMax);
                        float xDest;
                        float yDest;
                        float zDest;
                        string[] _xyzDestCords = kvpCorners.Value[2].Split(',');
                        float.TryParse(_xyzDestCords[0], out xDest);
                        float.TryParse(_xyzDestCords[1], out yDest);
                        float.TryParse(_xyzDestCords[2], out zDest);
                        int _xDest = (int)xDest;
                        int _yDest = (int)yDest;
                        int _zDest = (int)zDest;

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
                        else if (xMin < 0 & xMax >= 0)
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
                            Vector3 _dest = new Vector3(_xDest, _yDest, _zDest);
                            _cInfo.SendPackage(new NetPackageTeleportPlayer(_dest));
                            PersistentContainer.Instance.Players[_cInfo.playerId, true].LastTravel = DateTime.Now;
                            PersistentContainer.Instance.Save();
                            string _phrase603;
                            if (!Phrases.Dict.TryGetValue(603, out _phrase603))
                            {
                                _phrase603 = "You have traveled to";
                            }
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} {2}.[-]", Config.ChatResponseColor, _phrase603, kvpCorners.Key), "Server", false, "", false));
                            continue;
                        }
                        else
                        {
                            Flag.Add(_cInfo.entityId + 1);
                            if (Flag.Count == Box.Count)
                            {
                                string _phrase604;
                                if (!Phrases.Dict.TryGetValue(604, out _phrase604))
                                {
                                    _phrase604 = "You are not in a travel location.";
                                }
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.ChatResponseColor, _phrase604), "Server", false, "", false));
                            }
                        }
                    }
                }
            }
        }
    }
}