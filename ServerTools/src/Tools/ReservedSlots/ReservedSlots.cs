using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    public class ReservedSlots
    {
        public static bool IsEnabled = false, IsRunning = false, Operating = false, Reduced_Delay = false, Admin_Slot = false;
        public static int Session_Time = 30, Admin_Level = 0;
        public static string Command69 = "reserved";
        public static Dictionary<string, DateTime> Dict = new Dictionary<string, DateTime>();
        public static Dictionary<string, string> Dict1 = new Dictionary<string, string>();
        public static Dictionary<string, DateTime> Kicked = new Dictionary<string, DateTime>();
        private static string file = "ReservedSlots.xml", filePath = string.Format("{0}/{1}", API.ConfigPath, file);
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

        public static bool ReservedCheck(string _id)
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
                        string _response = "Your reserved status expires on {DateTime}.[-]";
                        _response = _response.Replace("{DateTime}", _dt.ToString());
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _response + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        string _response = "Your reserved status has expired on {DateTime}.[-]";
                        _response = _response.Replace("{DateTime}", _dt.ToString());
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _response + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You are not on the reservation list. Expiration date unavailable.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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

        public static void FullServer(ClientInfo _cInfo)
        {
            try
            {
                List<ClientInfo> _reservedKicks = new List<ClientInfo>();
                List<ClientInfo> _normalKicks = new List<ClientInfo>();
                ClientInfo _cInfoClientToKick = null;
                List<ClientInfo> _clientList = PersistentOperations.ClientList();
                if (ReservedSlots.AdminCheck(_cInfo.playerId))//admin is joining
                {
                    if (_clientList != null && _clientList.Count > 0)
                    {
                        for (int i = 0; i < _clientList.Count; i++)
                        {
                            ClientInfo _cInfo2 = _clientList[i];
                            if (_cInfo2 != null && _cInfo2.playerId != _cInfo.playerId)
                            {
                                if (!ReservedSlots.AdminCheck(_cInfo2.playerId))//not admin
                                {
                                    if (ReservedSlots.ReservedCheck(_cInfo2.playerId))//reserved player
                                    {
                                        _reservedKicks.Add(_cInfo2);
                                    }
                                    else
                                    {
                                        _normalKicks.Add(_cInfo2);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (ReservedSlots.ReservedCheck(_cInfo.playerId))//reserved player is joining
                {
                    if (_clientList != null && _clientList.Count > 0)
                    {
                        for (int i = 0; i < _clientList.Count; i++)
                        {
                            ClientInfo _cInfo2 = _clientList[i];
                            if (_cInfo2 != null && _cInfo2.playerId != _cInfo.playerId)
                            {
                                if (!ReservedSlots.AdminCheck(_cInfo2.playerId) && !ReservedSlots.ReservedCheck(_cInfo2.playerId))
                                {
                                    _normalKicks.Add(_cInfo2);
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
                            if (_cInfo2 != null && _cInfo2.playerId != _cInfo.playerId)
                            {
                                if (!ReservedSlots.AdminCheck(_cInfo2.playerId) && !ReservedSlots.ReservedCheck(_cInfo2.playerId))
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
                                                _normalKicks.Add(_cInfo2);
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
                    _cInfoClientToKick = _normalKicks[0];
                    if (Session_Time > 0)
                    {
                        ReservedSlots.Kicked.Add(_cInfoClientToKick.playerId, DateTime.Now);
                    }
                    string _phrase20;
                    if (!Phrases.Dict.TryGetValue(20, out _phrase20))
                    {
                        _phrase20 = "{ServerResponseName}- The server is full. You were kicked by the reservation system to open a slot";
                    }
                    _phrase20 = _phrase20.Replace("{ServerResponseName}", LoadConfig.Server_Response_Name);
                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfoClientToKick.playerId, _phrase20), null);
                }
                else if (_reservedKicks != null && _reservedKicks.Count > 0)
                {
                    string _phrase20;
                    if (!Phrases.Dict.TryGetValue(20, out _phrase20))
                    {
                        _phrase20 = "{ServerResponseName}- The server is full. You were kicked by the reservation system to open a slot for an admin";
                    }
                    _phrase20 = _phrase20.Replace("{ServerResponseName}", LoadConfig.Server_Response_Name);
                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfoClientToKick.playerId, _phrase20), null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ReservedSlots.FullServer: {0}", e.Message));
            }
        }
    }
}