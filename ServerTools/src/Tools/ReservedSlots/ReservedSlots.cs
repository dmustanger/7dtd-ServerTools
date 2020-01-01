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

        public static bool AdminCheck(string _steamId)
        {
            AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_steamId);
            if (Admin.PermissionLevel <= Admin_Level)
            {
                return true;
            }
            return false;
        }

        public static bool FullServer(string _playerId, string _playerName, string _compatibilityVersion)
        {
            try
            {
                ulong _num;
                if (!Steam.Masterserver.Server.GameServerInitialized || !GameManager.Instance.gameStateManager.IsGameStarted() || 
                    GameStats.GetInt(EnumGameStats.GameState) == 2 || string.IsNullOrEmpty(_playerName) ||
                    string.IsNullOrEmpty(_playerId) || string.IsNullOrEmpty(_playerId) || !ulong.TryParse(_playerId, out _num) || 
                    !string.Equals(Constants.cCompatibilityVersion, _compatibilityVersion, StringComparison.Ordinal))
                {
                    return true;
                }
                List<ClientInfo> list = PersistentOperations.ClientList();
                for (int i = 0; i < list.Count; i++)
                {
                    ClientInfo clientInfo = list[i];
                    if (clientInfo != null && clientInfo.playerId == _playerId)
                    {
                        return true;
                    }
                }
                ClientInfo _cInfoClientToKick = null;
                if (ReservedSlots.AdminCheck(_playerId))
                {
                    ClientInfo _cInfoReservedToKick = null;
                    int _clientSession = int.MinValue;
                    int _reservedSession = int.MinValue;
                    List<string> _sessionList = new List<string>(PersistentOperations.Session.Keys);
                    if (_sessionList == null)
                    {
                        return true;
                    }
                    for (int i = 0; i < _sessionList.Count; i++)
                    {
                        string _player = _sessionList[i];
                        ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForPlayerId(_player);
                        if (_cInfo2 != null && _playerId != _cInfo2.playerId)
                        {
                            if (ReservedSlots.AdminCheck(_cInfo2.playerId))
                            {
                                continue;
                            }
                            else if (ReservedSlots.ReservedCheck(_cInfo2.playerId))
                            {
                                DateTime _dateTime;
                                if (PersistentOperations.Session.TryGetValue(_cInfo2.playerId, out _dateTime))
                                {
                                    TimeSpan varTime = DateTime.Now - _dateTime;
                                    double fractionalMinutes = varTime.TotalMinutes;
                                    int _timepassed = (int)fractionalMinutes;
                                    if (_timepassed > _reservedSession)
                                    {
                                        _reservedSession = _timepassed;
                                        _cInfoReservedToKick = _cInfo2;
                                    }
                                }
                            }
                            else
                            {
                                DateTime _dateTime;
                                if (PersistentOperations.Session.TryGetValue(_cInfo2.playerId, out _dateTime))
                                {
                                    TimeSpan varTime = DateTime.Now - _dateTime;
                                    double fractionalMinutes = varTime.TotalMinutes;
                                    int _timepassed = (int)fractionalMinutes;
                                    if (_timepassed > _clientSession)
                                    {
                                        _clientSession = _timepassed;
                                        _cInfoClientToKick = _cInfo2;
                                    }
                                }
                            }
                        }
                    }
                    if (_cInfoClientToKick != null)
                    {
                        API.PlayerDisconnected(_cInfoClientToKick, true);
                        string _phrase20;
                        if (!Phrases.Dict.TryGetValue(20, out _phrase20))
                        {
                            _phrase20 = "{ServerResponseName}- The server is full. You were kicked by the reservation system to open a slot";
                        }
                        _phrase20 = _phrase20.Replace("{ServerResponseName}", LoadConfig.Server_Response_Name);
                        SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfoClientToKick.playerId, _phrase20), (ClientInfo)null);
                        EntityPlayer entityPlayer = (EntityPlayer)GameManager.Instance.World.GetEntity(_cInfoClientToKick.entityId);
                        if (entityPlayer != null)
                        {
                            if (_cInfoClientToKick.entityId != -1)
                            {
                                Log.Out("Player {0} disconnected after {1} minutes", new object[]
                            {
                                GameUtils.SafeStringFormat(entityPlayer.EntityName),
                                ((Time.timeSinceLevelLoad - entityPlayer.CreationTimeSinceLevelLoad) / 60f).ToCultureInvariantString("0.0")});
                            }
                            entityPlayer.OnEntityUnload();
                        }
                        GC.Collect();
                        MemoryPools.Cleanup();
                        PersistentPlayerData persistentPlayerData = PersistentOperations.GetPersistentPlayerDataFromSteamId(_cInfoClientToKick.playerId);
                        if (persistentPlayerData != null)
                        {
                            persistentPlayerData.LastLogin = DateTime.Now;
                            persistentPlayerData.EntityId = -1;
                        }
                        PersistentOperations.SavePersistentPlayerDataXML();
                        GameManager.Instance.World.aiDirector.RemoveEntity(entityPlayer);
                        GameManager.Instance.World.RemoveEntity(entityPlayer.entityId, EnumRemoveEntityReason.Unloaded);
                        ConnectionManager.Instance.Clients.Remove(_cInfoClientToKick);
                        return true;
                    }
                    else if (_cInfoReservedToKick != null)
                    {
                        API.PlayerDisconnected(_cInfoReservedToKick, true);
                        string _phrase20;
                        if (!Phrases.Dict.TryGetValue(20, out _phrase20))
                        {
                            _phrase20 = "{ServerResponseName}- The server is full. You were kicked by the reservation system to open a slot";
                        }
                        _phrase20 = _phrase20.Replace("{ServerResponseName}", LoadConfig.Server_Response_Name);
                        SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfoReservedToKick.playerId, _phrase20), (ClientInfo)null);
                        EntityPlayer entityPlayer = (EntityPlayer)GameManager.Instance.World.GetEntity(_cInfoReservedToKick.entityId);
                        if (entityPlayer != null)
                        {
                            if (_cInfoReservedToKick.entityId != -1)
                            {
                                Log.Out("Player {0} disconnected after {1} minutes", new object[]
                            {
                                GameUtils.SafeStringFormat(entityPlayer.EntityName),
                                ((Time.timeSinceLevelLoad - entityPlayer.CreationTimeSinceLevelLoad) / 60f).ToCultureInvariantString("0.0")});
                            }
                            entityPlayer.OnEntityUnload();
                        }
                        GC.Collect();
                        MemoryPools.Cleanup();
                        PersistentPlayerData persistentPlayerData = PersistentOperations.GetPersistentPlayerDataFromSteamId(_cInfoReservedToKick.playerId);
                        if (persistentPlayerData != null)
                        {
                            persistentPlayerData.LastLogin = DateTime.Now;
                            persistentPlayerData.EntityId = -1;
                        }
                        PersistentOperations.SavePersistentPlayerDataXML();
                        GameManager.Instance.World.aiDirector.RemoveEntity(entityPlayer);
                        GameManager.Instance.World.RemoveEntity(entityPlayer.entityId, EnumRemoveEntityReason.Unloaded);
                        ConnectionManager.Instance.Clients.Remove(_cInfoReservedToKick);
                        return true;
                    }
                }
                else if (ReservedSlots.ReservedCheck(_playerId))
                {
                    int _clientSession = int.MinValue;
                    List<string> _sessionList = new List<string>(PersistentOperations.Session.Keys);
                    if (_sessionList == null)
                    {
                        return true;
                    }
                    for (int i = 0; i < _sessionList.Count; i++)
                    {
                        string _player = _sessionList[i];
                        ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForPlayerId(_player);
                        if (_cInfo2 != null && _playerId != _cInfo2.playerId)
                        {
                            if (ReservedSlots.AdminCheck(_cInfo2.playerId))
                            {
                                continue;
                            }
                            else if (ReservedSlots.ReservedCheck(_cInfo2.playerId))
                            {
                                continue;
                            }
                            else
                            {
                                DateTime _dateTime;
                                if (PersistentOperations.Session.TryGetValue(_cInfo2.playerId, out _dateTime))
                                {
                                    TimeSpan varTime = DateTime.Now - _dateTime;
                                    double fractionalMinutes = varTime.TotalMinutes;
                                    int _timepassed = (int)fractionalMinutes;
                                    if (_timepassed > _clientSession)
                                    {
                                        _clientSession = _timepassed;
                                        _cInfoClientToKick = _cInfo2;
                                    }
                                }
                            }
                        }
                    }
                    if (_cInfoClientToKick != null)
                    {
                        API.PlayerDisconnected(_cInfoClientToKick, true);
                        string _phrase20;
                        if (!Phrases.Dict.TryGetValue(20, out _phrase20))
                        {
                            _phrase20 = "{ServerResponseName}- The server is full. You were kicked by the reservation system to open a slot";
                        }
                        _phrase20 = _phrase20.Replace("{ServerResponseName}", LoadConfig.Server_Response_Name);
                        SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfoClientToKick.playerId, _phrase20), (ClientInfo)null);
                        EntityPlayer entityPlayer = (EntityPlayer)GameManager.Instance.World.GetEntity(_cInfoClientToKick.entityId);
                        if (entityPlayer != null)
                        {
                            if (_cInfoClientToKick.entityId != -1)
                            {
                                Log.Out("Player {0} disconnected after {1} minutes", new object[]
                            {
                                GameUtils.SafeStringFormat(entityPlayer.EntityName),
                                ((Time.timeSinceLevelLoad - entityPlayer.CreationTimeSinceLevelLoad) / 60f).ToCultureInvariantString("0.0")});
                            }
                            entityPlayer.OnEntityUnload();
                        }
                        GC.Collect();
                        MemoryPools.Cleanup();
                        PersistentPlayerData persistentPlayerData = PersistentOperations.GetPersistentPlayerDataFromSteamId(_cInfoClientToKick.playerId);
                        if (persistentPlayerData != null)
                        {
                            persistentPlayerData.LastLogin = DateTime.Now;
                            persistentPlayerData.EntityId = -1;
                        }
                        PersistentOperations.SavePersistentPlayerDataXML();
                        GameManager.Instance.World.aiDirector.RemoveEntity(entityPlayer);
                        GameManager.Instance.World.RemoveEntity(entityPlayer.entityId, EnumRemoveEntityReason.Unloaded);
                        ConnectionManager.Instance.Clients.Remove(_cInfoClientToKick);
                        return true;
                    }
                }
                else if (ReservedSlots.Session_Time > 0)
                {
                    int _session = int.MinValue;
                    List<string> _sessionList = new List<string>(PersistentOperations.Session.Keys);
                    if (_sessionList == null)
                    {
                        return true;
                    }
                    for (int i = 0; i < _sessionList.Count; i++)
                    {
                        string _player = _sessionList[i];
                        ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForPlayerId(_player);
                        if (_cInfo2 != null && _playerId != _cInfo2.playerId)
                        {
                            if (ReservedSlots.AdminCheck(_cInfo2.playerId))
                            {
                                continue;
                            }
                            else if (ReservedSlots.ReservedCheck(_cInfo2.playerId))
                            {
                                continue;
                            }
                            else
                            {
                                DateTime _dateTime;
                                if (PersistentOperations.Session.TryGetValue(_cInfo2.playerId, out _dateTime))
                                {
                                    TimeSpan varTime = DateTime.Now - _dateTime;
                                    double fractionalMinutes = varTime.TotalMinutes;
                                    int _timepassed = (int)fractionalMinutes;
                                    if (_timepassed >= ReservedSlots.Session_Time && _timepassed > _session)
                                    {
                                        _session = _timepassed;
                                        _cInfoClientToKick = _cInfo2;
                                    }
                                }
                            }
                        }
                    }
                    if (_session > int.MinValue)
                    {
                        ReservedSlots.Kicked.Add(_cInfoClientToKick.playerId, DateTime.Now);
                        API.PlayerDisconnected(_cInfoClientToKick, true);
                        string _phrase20;
                        if (!Phrases.Dict.TryGetValue(20, out _phrase20))
                        {
                            _phrase20 = "{ServerResponseName}- The server is full. You were kicked by the reservation system to open a slot";
                        }
                        _phrase20 = _phrase20.Replace("{ServerResponseName}", LoadConfig.Server_Response_Name);
                        SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfoClientToKick.playerId, _phrase20), (ClientInfo)null);
                        EntityPlayer entityPlayer = (EntityPlayer)GameManager.Instance.World.GetEntity(_cInfoClientToKick.entityId);
                        if (entityPlayer != null)
                        {
                            if (_cInfoClientToKick.entityId != -1)
                            {
                                Log.Out("Player {0} disconnected after {1} minutes", new object[]
                            {
                                GameUtils.SafeStringFormat(entityPlayer.EntityName),
                                ((Time.timeSinceLevelLoad - entityPlayer.CreationTimeSinceLevelLoad) / 60f).ToCultureInvariantString("0.0")});
                            }
                            entityPlayer.OnEntityUnload();
                        }
                        GC.Collect();
                        MemoryPools.Cleanup();
                        PersistentPlayerData persistentPlayerData = PersistentOperations.GetPersistentPlayerDataFromSteamId(_cInfoClientToKick.playerId);
                        if (persistentPlayerData != null)
                        {
                            persistentPlayerData.LastLogin = DateTime.Now;
                            persistentPlayerData.EntityId = -1;
                        }
                        PersistentOperations.SavePersistentPlayerDataXML();
                        GameManager.Instance.World.aiDirector.RemoveEntity(entityPlayer);
                        GameManager.Instance.World.RemoveEntity(entityPlayer.entityId, EnumRemoveEntityReason.Unloaded);
                        ConnectionManager.Instance.Clients.Remove(_cInfoClientToKick);
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ReservedSlots.FullServer: {0}.", e.Message));
            }
            return true;
        }
    }
}