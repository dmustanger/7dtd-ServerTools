using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    public class ReservedSlots
    {
        public static bool IsEnabled = false, IsRunning = false, Reduced_Delay = false, Admin_Slot = false;
        public static int Session_Time = 30, Admin_Level = 0;
        public static string Command69 = "reserved";
        public static Dictionary<string, DateTime> Dict = new Dictionary<string, DateTime>();
        public static Dictionary<string, string> Dict1 = new Dictionary<string, string>();
        public static Dictionary<string, DateTime> Kicked = new Dictionary<string, DateTime>();
        private static string file = "ReservedSlots.xml", filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        private static int _longestTime = 0;
        private static ClientInfo _playerToKick1 = null, _playerToKick2 = null;
        private static bool _operating = false;

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
            Dict.Clear();
            Dict1.Clear();
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
                    Dict.Clear();
                    Dict1.Clear();
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
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ReservedSlots entry because of missing 'SteamId' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Name"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ReservedSlots entry because of missing 'Name' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Expires"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ReservedSlots entry because of missing 'Expires' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        DateTime _dt;
                        if (!DateTime.TryParse(_line.GetAttribute("Expires"), out _dt))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ReservedSlots entry because of invalid (date) value for 'Expires' attribute: {0}", subChild.OuterXml));
                            continue;
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
                    sw.WriteLine(string.Format("        <Player SteamId=\"76561191234567891\" Name=\"foobar.\" Expires=\"10/29/2050 7:30:00 AM\" />"));
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

        public static void OpenSlot()
        {
            if (!_operating)
            {
                try
                {
                    _operating = true;
                    List<string> _sessionList = new List<string>(PlayerOperations.Session.Keys);
                    for (int i = 0; i < _sessionList.Count; i++)
                    {
                        string _player = _sessionList[i];
                        ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForPlayerId(_player);
                        if (_cInfo != null)
                        {
                            if (AdminCheck(_cInfo.playerId))
                            {
                                continue;
                            }
                            else if (DonorCheck(_cInfo.playerId))
                            {
                                DateTime _dt;
                                Dict.TryGetValue(_cInfo.playerId, out _dt);
                                if (DateTime.Now < _dt)
                                {
                                    continue;
                                }
                            }
                            Time(_cInfo);
                        }
                    }
                    if (_playerToKick1 != null)
                    {
                        Kick(_playerToKick1);
                    }
                    else if (_playerToKick2 != null)
                    {
                        Kick(_playerToKick2);
                    }
                    _longestTime = 0;
                    _playerToKick1 = null;
                    _playerToKick2 = null;
                    _operating = false;
                }
                catch (Exception e)
                {
                    _operating = false;
                    Log.Out(string.Format("[SERVERTOOLS] Error in ReservedSlots.OpenSlot: {0}.", e.Message));
                }
            }
        }

        private static void Time(ClientInfo _cInfo)
        {
            DateTime _dateTime;
            PlayerOperations.Session.TryGetValue(_cInfo.playerId, out _dateTime);
            TimeSpan varTime = DateTime.Now - _dateTime;
            double fractionalMinutes = varTime.TotalMinutes;
            int _timepassed = (int)fractionalMinutes;
            if (_timepassed > _longestTime)
            {
                _longestTime = _timepassed;
                _playerToKick2 = _cInfo;
                if (_timepassed >= Session_Time)
                {
                    _playerToKick1 = _cInfo;
                }
            }
        }

        private static void Kick(ClientInfo _cInfo)
        {
            if (_cInfo != null)
            {
                if (Session_Time > 0)
                {
                    Kicked.Add(_cInfo.playerId, DateTime.Now);
                }
                string _phrase20;
                if (!Phrases.Dict.TryGetValue(20, out _phrase20))
                {
                    _phrase20 = "Sorry {PlayerName} server is at max capacity and this slot is reserved.";
                }
                _phrase20 = _phrase20.Replace("{PlayerName}", _cInfo.playerName);
                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.playerId, _phrase20), (ClientInfo)null);
            }
        }

        public static bool DonorCheck(string _id)
        {
            if (Dict.ContainsKey(_id))
            {
                DateTime _dt;
                Dict.TryGetValue(_id, out _dt);
                if (DateTime.Now < _dt)
                {
                    return true;
                }
            }
            return false;
        }

        public static void ReservedStatus(ClientInfo _cInfo)
        {
            if (Dict.ContainsKey(_cInfo.playerId))
            {
                DateTime _dt;
                if (Dict.TryGetValue(_cInfo.playerId, out _dt))
                {
                    if (DateTime.Now < _dt)
                    {
                        string _response = ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " your reserved status expires on {DateTime}.[-]";
                        _response = _response.Replace("{DateTime}", _dt.ToString());
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _response + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        string _response = ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " your reserved status has expired on {DateTime}.[-]";
                        _response = _response.Replace("{DateTime}", _dt.ToString());
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _response + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you have not donated. Expiration date unavailable.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static bool AdminCheck(string _id)
        {
            AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_id);
            if (Admin.PermissionLevel <= Admin_Level)
            {
                return true;
            }
            return false;
        }
    }
}