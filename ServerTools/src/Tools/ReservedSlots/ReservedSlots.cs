using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    public class ReservedSlots
    {
        public static bool IsEnabled = false, IsRunning = false, Operating = false, Reduced_Delay = false, Admin_Slot = false;
        public static int Session_Time = 30, Admin_Level = 0;
        public static string Command_reserved = "reserved";
        public static Dictionary<string, DateTime> Dict = new Dictionary<string, DateTime>();
        public static Dictionary<string, string> Dict1 = new Dictionary<string, string>();
        public static Dictionary<string, DateTime> Kicked = new Dictionary<string, DateTime>();

        private static string file = "ReservedSlots.xml";
        private static string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);

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
            FileWatcher.Dispose();
            IsRunning = false;
        }

        private static void LoadXml()
        {
            try
            {
                if (!Utils.FileExists(FilePath))
                {
                    UpdateXml();
                }
                XmlDocument xmlDoc = new XmlDocument();
                try
                {
                    xmlDoc.Load(FilePath);
                }
                catch (XmlException e)
                {
                    Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", file, e.Message));
                    return;
                }
                XmlNodeList _childNodes = xmlDoc.DocumentElement.ChildNodes;
                if (_childNodes != null && _childNodes.Count > 0)
                {
                    Dict.Clear();
                    Dict1.Clear();
                    bool upgrade = true;
                    for (int i = 0; i < _childNodes.Count; i++)
                    {
                        if (_childNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement _line = (XmlElement)_childNodes[i];
                        if (_line.HasAttributes)
                        {
                            if (_line.HasAttribute("Version") && _line.GetAttribute("Version") == Config.Version)
                            {
                                upgrade = false;
                                continue;
                            }
                            else if (_line.HasAttribute("SteamId") && _line.HasAttribute("Name") && _line.HasAttribute("Expires"))
                            {
                                if (!DateTime.TryParse(_line.GetAttribute("Expires"), out DateTime _dt))
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring ReservedSlots.xml entry. Invalid (date) value for 'Expires' attribute: {0}", _line.OuterXml));
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
                    if (upgrade)
                    {
                        UpgradeXml(_childNodes);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ReservedSlots.LoadXml: {0}", e.Message));
            }
        }

        public static void UpdateXml()
        {
            FileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<ReservedSlots>");
                sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                sw.WriteLine();
                sw.WriteLine();
                if (Dict.Count > 0)
                {
                    foreach (KeyValuePair<string, DateTime> kvp in Dict)
                    {
                        Dict1.TryGetValue(kvp.Key, out string _name);
                        sw.WriteLine(string.Format("    <Player SteamId=\"{0}\" Name=\"{1}\" Expires=\"{2}\" />", kvp.Key, _name, kvp.Value.ToString()));
                    }
                }
                else
                {
                    sw.WriteLine(string.Format("    <Player SteamId=\"\" Name=\"\" Expires=\"\" />"));
                }
                sw.WriteLine("</ReservedSlots>");
                sw.Flush();
                sw.Close();
            }
            FileWatcher.EnableRaisingEvents = true;
        }

        private static void InitFileWatcher()
        {
            FileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.Created += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.EnableRaisingEvents = true;
            IsRunning = true;
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            if (!Utils.FileExists(FilePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        public static bool ReservedCheck(string _id)
        {
            if (Dict.ContainsKey(_id))
            {
                Dict.TryGetValue(_id, out DateTime _dt);
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
                if (Dict.TryGetValue(_cInfo.playerId, out DateTime _dt))
                {
                    if (DateTime.Now < _dt)
                    {
                        Phrases.Dict.TryGetValue("Reserved4", out string _phrase4);
                        _phrase4 = _phrase4.Replace("{DateTime}", _dt.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase4 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Reserved5", out string _phrase5);
                        _phrase5 = _phrase5.Replace("{DateTime}", _dt.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase5 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                Phrases.Dict.TryGetValue("Reserved6", out string _phrase6);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase6 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static bool AdminCheck(string _steamId)
        {
            if (GameManager.Instance.adminTools.GetUserPermissionLevel(_steamId) <= Admin_Level)
            {
                return true;
            }
            return false;
        }

        public static bool FullServer(string _playerId)
        {
            try
            {
                List<string> _reservedKicks = new List<string>();
                List<string> _normalKicks = new List<string>();
                string _clientToKick = null;
                List<ClientInfo> _clientList = PersistentOperations.ClientList();
                if (AdminCheck(_playerId))//admin is joining
                {
                    if (_clientList != null && _clientList.Count > 0)
                    {
                        for (int i = 0; i < _clientList.Count; i++)
                        {
                            ClientInfo _cInfo2 = _clientList[i];
                            if (_cInfo2 != null && !string.IsNullOrEmpty(_cInfo2.playerId) && _cInfo2.playerId != _playerId)
                            {
                                if (!AdminCheck(_cInfo2.playerId))//not admin
                                {
                                    if (ReservedCheck(_cInfo2.playerId))//reserved player
                                    {
                                        _reservedKicks.Add(_cInfo2.playerId);
                                    }
                                    else
                                    {
                                        _normalKicks.Add(_cInfo2.playerId);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (ReservedCheck(_playerId))//reserved player is joining
                {
                    if (_clientList != null && _clientList.Count > 0)
                    {
                        for (int i = 0; i < _clientList.Count; i++)
                        {
                            ClientInfo _cInfo2 = _clientList[i];
                            if (_cInfo2 != null && !string.IsNullOrEmpty(_cInfo2.playerId) && _cInfo2.playerId != _playerId)
                            {
                                if (!AdminCheck(_cInfo2.playerId) && !ReservedCheck(_cInfo2.playerId))
                                {
                                    _normalKicks.Add(_cInfo2.playerId);
                                }
                            }
                        }
                    }
                }
                else//regular player is joining
                {
                    if (_clientList != null && _clientList.Count > 0)
                    {
                        for (int i = 0; i < _clientList.Count; i++)
                        {
                            ClientInfo _cInfo2 = _clientList[i];
                            if (_cInfo2 != null && !string.IsNullOrEmpty(_cInfo2.playerId) && _cInfo2.playerId != _playerId)
                            {
                                if (!AdminCheck(_cInfo2.playerId) && !ReservedCheck(_cInfo2.playerId))
                                {
                                    if (Session_Time > 0)
                                    {
                                        if (PersistentOperations.Session.TryGetValue(_cInfo2.playerId, out DateTime _dateTime))
                                        {
                                            TimeSpan varTime = DateTime.Now - _dateTime;
                                            double fractionalMinutes = varTime.TotalMinutes;
                                            int _timepassed = (int)fractionalMinutes;
                                            if (_timepassed >= Session_Time)
                                            {
                                                _normalKicks.Add(_cInfo2.playerId);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (_normalKicks != null && _normalKicks.Count > 0)
                {
                    _normalKicks.RandomizeList();
                    _clientToKick = _normalKicks[0];
                    if (Session_Time > 0)
                    {
                        Kicked.Add(_clientToKick, DateTime.Now);
                    }
                    Phrases.Dict.TryGetValue("Reserved1", out string _phrase1);
                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _clientToKick, _phrase1), null);
                    return true;
                }
                else if (_reservedKicks != null && _reservedKicks.Count > 0)
                {
                    _reservedKicks.RandomizeList();
                    _clientToKick = _reservedKicks[0];
                    Phrases.Dict.TryGetValue("Reserved1", out string _phrase1);
                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _clientToKick, _phrase1), null);
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ReservedSlots.FullServer: {0}", e.Message));
            }
            return false;
        }

        private static void UpgradeXml(XmlNodeList _oldChildNodes)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                File.Delete(FilePath);
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<ReservedSlots>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine();
                    sw.WriteLine();
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement _line = (XmlElement)_oldChildNodes[i];
                        if (_line.HasAttributes && _line.OuterXml.Contains("Player"))
                        {
                            string _steamId = "", _name = "", _expires = "";
                            if (_line.HasAttribute("SteamId"))
                            {
                                _steamId = _line.GetAttribute("SteamId");
                            }
                            if (_line.HasAttribute("Name"))
                            {
                                _name = _line.GetAttribute("Name");
                            }
                            if (_line.HasAttribute("Expires"))
                            {
                                _expires = _line.GetAttribute("Expires");
                            }
                            sw.WriteLine(string.Format("    <Player SteamId=\"{0}\" Name=\"{1}\" Expires=\"{2}\" />", _steamId, _name, _expires));
                        }
                    }
                    sw.WriteLine("</ReservedSlots>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ReservedSlots.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}