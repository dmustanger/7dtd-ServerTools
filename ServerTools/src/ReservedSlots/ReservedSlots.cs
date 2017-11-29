using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    public class ReservedSlots
    {
        public static bool IsEnabled = false;        
        public static bool IsRunning = false;
        public static bool DonatorNameColoring = false;
        public static SortedDictionary<string, DateTime> Dict = new SortedDictionary<string, DateTime>();
        public static SortedDictionary<string, string> Dict1 = new SortedDictionary<string, string>();
        private static string file = "ReservedSlots.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        

        public static void Load()
        {
            if (IsEnabled && !IsRunning || DonatorNameColoring && !IsRunning)
            {
                LoadXml();
                InitFileWatcher();
            }
        }

        public static void Unload()
        {
            Dict.Clear();
            fileWatcher.Dispose();
            IsRunning = false;
            DonatorNameColoring = false;
        }

        private static void LoadXml()
        {
            bool _update = false;
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
                if (childNode.Name == "Players")
                {
                    Dict.Clear();
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Players' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("SteamId"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player entry because of missing 'SteamId' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Name"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player entry because of missing 'Name' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Expires"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player entry because of missing 'Expires' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        DateTime _dt;
                        if (_line.GetAttribute("Expires") == "")
                        {
                            _dt = DateTime.Parse("10/29/2050 7:30:00 AM");
                            _update = true;
                        }
                        else
                        {
                            if (!DateTime.TryParse(_line.GetAttribute("Expires"), out _dt))
                            {
                                Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player entry because of invalid (date) value for 'Expires' attribute: {0}", subChild.OuterXml));
                                continue;
                            }
                            
                        }
                        if (!Dict.ContainsKey(_line.GetAttribute("SteamId")))
                        {
                            Dict.Add(_line.GetAttribute("SteamId"), _dt);
                        }
                        if (!Dict1.ContainsKey(_line.GetAttribute("SteamId")))
                        {
                            Dict1.Add(_line.GetAttribute("SteamId"), _line.GetAttribute("Name"));
                        }
                    }
                }
            }
            if (_update)
            {
                UpdateXml();
            }
        }

        public static void UpdateXml()
        {
            fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<ReservedSlots>");
                sw.WriteLine("    <Players>");
                if (Dict.Count > 0)
                {
                    foreach (KeyValuePair<string, DateTime> kvp in Dict)
                    {
                        string _name = "";
                        Dict1.TryGetValue(kvp.Key, out _name);
                        sw.WriteLine(string.Format("        <Player SteamId=\"{0}\" Name=\"{1}\" Expires=\"{2}\" />", kvp.Key, _name, kvp.Value.ToString()));
                    }
                }
                else
                {
                    sw.WriteLine(string.Format("        <!-- Player SteamId=\"123456\" Name=\"foobar.\" Expires=\"10/29/2050 7:30:00 AM\" / -->"));
                }
                sw.WriteLine("    </Players>");
                sw.WriteLine("</ReservedSlots>");
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
            DonatorNameColoring = true;
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            if (!Utils.FileExists(filePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        public static void CheckReservedSlot(ClientInfo _cInfo)
        {
            if (IsEnabled)
            {
                int _playerCount = ConnectionManager.Instance.ClientCount();
                if (_playerCount == API.MaxPlayers)
                {
                    if (!Dict.ContainsKey(_cInfo.playerId))
                    {
                        string _phrase20;
                        if (!Phrases.Dict.TryGetValue(20, out _phrase20))
                        {
                            _phrase20 = "Sorry {PlayerName} this slot is reserved.";
                        }
                        _phrase20 = _phrase20.Replace("{PlayerName}", _cInfo.playerName);
                        SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.playerId, _phrase20), _cInfo);
                    }
                    else
                    {
                        DateTime _dt;
                        Dict.TryGetValue(_cInfo.playerId, out _dt);
                        if (DateTime.Now > _dt)
                        {
                            string _phrase21;
                            if (!Phrases.Dict.TryGetValue(21, out _phrase21))
                            {
                                _phrase21 = "Sorry {PlayerName} your reserved slot has expired.";
                            }
                            _phrase21 = _phrase21.Replace("{PlayerName}", _cInfo.playerName);
                            SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.playerId, _phrase21), _cInfo);
                        }
                        else
                        {
                            ClientInfo _playerToKick = null;
                            uint _itemsCrafted = 1999999999;
                            float _distanceWalked = 9999999999.0f;
                            int _level = 1000;
                            List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                            foreach (ClientInfo _cInfo1 in _cInfoList)
                            {
                                if (!Dict.ContainsKey(_cInfo1.playerId) && !GameManager.Instance.adminTools.IsAdmin(_cInfo1.playerId))
                                {
                                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo1.entityId];
                                    if (_player.Level <= _level)
                                    {
                                        if (_player.Level == _level)
                                        {
                                            if (_player.totalItemsCrafted <= _itemsCrafted)
                                            {
                                                if (_player.totalItemsCrafted == _itemsCrafted)
                                                {
                                                    if (_player.distanceWalked < _distanceWalked)
                                                    {
                                                        _distanceWalked = _player.distanceWalked;
                                                        _playerToKick = _cInfo1;
                                                    }
                                                }
                                                else
                                                {
                                                    _itemsCrafted = _player.totalItemsCrafted;
                                                    _playerToKick = _cInfo1;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            _level = _player.Level;
                                            _playerToKick = _cInfo1;
                                        }
                                    }
                                }
                            }
                            if (_playerToKick != null)
                            {
                                string _phrase20;
                                if (!Phrases.Dict.TryGetValue(20, out _phrase20))
                                {
                                    _phrase20 = "Sorry {PlayerName} this slot is reserved.";
                                }
                                _phrase20 = _phrase20.Replace("{PlayerName}", _playerToKick.playerName);
                                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _playerToKick.playerId, _phrase20), _playerToKick);
                            }
                        }
                    }
                }
            }
        }
    }
}