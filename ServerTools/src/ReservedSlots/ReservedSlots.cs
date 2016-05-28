using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    public class ReservedSlots
    {
        public static bool IsEnabled = false;
        public static bool IsRunning = false;
        private static SortedDictionary<string, string> dict = new SortedDictionary<string, string>();
        private static string file = "ReservedSlots.xml";
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
            dict.Clear();
            fileWatcher.Dispose();
            IsRunning = false;
        }

        private static void LoadXml()
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
                if (childNode.Name == "Players")
                {
                    dict.Clear();
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
                        if (!dict.ContainsKey(_line.GetAttribute("SteamId")))
                        {
                            dict.Add(_line.GetAttribute("SteamId"), _line.GetAttribute("Name"));
                        }
                    }
                }
            }
        }

        private static void UpdateXml()
        {
            fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<ReservedSlots>");
                sw.WriteLine("    <Players>");
                if (dict.Count > 0)
                {
                    foreach (KeyValuePair<string, string> kvp in dict)
                    {
                        sw.WriteLine(string.Format("        <Player SteamId=\"{0}\" Name=\"{1}\" />", kvp.Key, kvp.Value));
                    }
                }
                else
                {
                    sw.WriteLine(string.Format("        <!-- Player SteamId=\"123456\" Name=\"foobar.\" / -->"));
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
            int _playerCount = ConnectionManager.Instance.ClientCount();
            if (_playerCount == API.MaxPlayers)
            {
                if (!dict.ContainsKey(_cInfo.playerId) && !GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId))
                {
                    string _phrase20;
                    if (!Phrases.Dict.TryGetValue(20, out _phrase20))
                    {
                        _phrase20 = "Sorry {PlayerName} this slot is reserved.";
                    }
                    _phrase20 = _phrase20.Replace("{PlayerName}", _cInfo.playerName);
                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.entityId, _phrase20), _cInfo);
                }
                else
                { 
                    ClientInfo _playerToKick = null;
                    uint _itemsCrafted = 1999999999;
                    float _distanceWalked = 9999999999.0f;
                    List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                    foreach (ClientInfo _cInfo1 in _cInfoList)
                    {
                        if (_cInfo.playerId != _cInfo1.playerId && !GameManager.Instance.adminTools.IsAdmin(_cInfo1.playerId))
                        {
                            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo1.entityId];
                            uint _totalItemsCrafted = _player.totalItemsCrafted;
                            if (_totalItemsCrafted <= _itemsCrafted)
                            {
                                if (_totalItemsCrafted == _itemsCrafted)
                                {
                                    float _totalDistanceWalked = _player.distanceWalked;
                                    if (_totalDistanceWalked < _distanceWalked)
                                    {
                                        _distanceWalked = _totalDistanceWalked;
                                        _playerToKick = _cInfo1;
                                    }
                                }
                                else
                                {
                                    _itemsCrafted = _totalItemsCrafted;
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
                        SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _playerToKick.entityId, _phrase20), _playerToKick);
                    }
                }
            }
        }
    }
}