using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    public class CustomCommands
    {
        public static bool IsEnabled = false;
        public static bool IsRunning = false;
        public static int _timepassed = 0;       
        public static SortedDictionary<string, string> Dict = new SortedDictionary<string, string>();
        public static SortedDictionary<string, int[]> Dict1 = new SortedDictionary<string, int[]>();
        private const string file = "CustomChatCommands.xml";
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
            if (IsRunning && !IsEnabled)
            {
                fileWatcher.Dispose();
                IsRunning = false;
            }
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
                if (childNode.Name == "Commands")
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
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Commands' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("Trigger"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Commands entry because of missing a Trigger attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Response"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Commands entry because of missing a Response attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        int _delay = 0;
                        if (_line.HasAttribute("DelayBetweenUses"))
                        {
                            if (!int.TryParse(_line.GetAttribute("DelayBetweenUses"), out _delay))
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Using default value of 0 for DelayBetweenUses for command entry {1} because of invalid (non-numeric) value: {0}", subChild.OuterXml, _line.GetAttribute("Trigger")));
                            }
                        }
                        int _number = 1;
                        if (_line.HasAttribute("Number"))
                        {
                            if (!int.TryParse(_line.GetAttribute("Number"), out _number))
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Using default value of 1 for Number for command entry {1} because of invalid (non-numeric) value: {0}", subChild.OuterXml, _line.GetAttribute("Trigger")));
                            }
                        }
                        string _trigger = _line.GetAttribute("Trigger");
                        if (!Dict.ContainsKey(_trigger))
                        {
                            string _response = _line.GetAttribute("Response");
                            Dict.Add(_trigger, _response);
                        }
                        if (!Dict1.ContainsKey(_trigger))
                        {
                            int[] _c = { _delay, _number };
                            Dict1.Add(_trigger, _c);
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
                sw.WriteLine("<CustomCommands>");
                sw.WriteLine("    <Commands>");
                sw.WriteLine("        <!-- possible variables {EntityId} {SteamId} {PlayerName} -->");
                if (Dict.Count > 0)
                {
                    foreach (KeyValuePair<string, string> kvp in Dict)
                    {
                        foreach (KeyValuePair<string, int[]> kvp1 in Dict1)
                        {
                            if (kvp.Key == kvp1.Key)
                            {
                                sw.WriteLine(string.Format("        <Command Number=\"{0}\" Trigger=\"{1}\" Response=\"{2}\" DelayBetweenUses=\"{3}\" />", kvp1.Value[1], kvp.Key, kvp.Value, kvp1.Value[0]));
                            }
                        }
                    }
                }
                else
                {
                    sw.WriteLine("        <Command Number=\"1\" Trigger=\"help\" Response=\"say &quot;[00FF00]Type /commands for a list of chat commands.[-]&quot;\" DelayBetweenUses=\"0\" />");
                    sw.WriteLine("        <Command Number=\"2\" Trigger=\"info\" Response=\"say &quot;[00FF00]Type /commands for a list of chat commands.[-]&quot;\" DelayBetweenUses=\"0\" />");
                    sw.WriteLine("        <Command Number=\"3\" Trigger=\"rules\" Response=\"say &quot;[00FF00]Visit YourSiteHere to see the rules.[-]&quot;\" DelayBetweenUses=\"0\" />");
                    sw.WriteLine("        <Command Number=\"4\" Trigger=\"website\" Response =\"say &quot;[00FF00]Visit YourSiteHere.[-]&quot;\" DelayBetweenUses=\"0\" />");
                    sw.WriteLine("        <Command Number=\"5\" Trigger=\"teamspeak\" Response=\"say &quot;[00FF00]The Teamspeak3 info is YourInfoHere.[-]&quot;\" DelayBetweenUses=\"0\" />");
                    sw.WriteLine("        <Command Number=\"6\" Trigger=\"market\" Response=\"tele {EntityId} 0 -1 0;\" DelayBetweenUses=\"60\" />");
                    sw.WriteLine("        <Command Number=\"7\" Trigger=\"test2\" Response=\"say &quot;Your command here&quot;\" DelayBetweenUses=\"10\" />");
                    sw.WriteLine("        <Command Number=\"8\" Trigger=\"test3\" Response=\"say &quot;Your command here&quot;\" DelayBetweenUses=\"20\" />");
                    sw.WriteLine("        <Command Number=\"9\" Trigger=\"test4\" Response=\"say &quot;Your command here&quot;\" DelayBetweenUses=\"30\" />");
                    sw.WriteLine("        <Command Number=\"10\" Trigger=\"test5\" Response=\"say &quot;Your command here&quot;\" DelayBetweenUses=\"40\" />");
                }
                sw.WriteLine("    </Commands>");
                sw.WriteLine("</CustomCommands>");
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

        public static string GetChatCommands(ClientInfo _cInfo)
        {
            string _commands = string.Format("{0}Commands are:", Config.ChatResponseColor);
            if (Gimme.IsEnabled)
            {
                _commands = string.Format("{0} /gimme", _commands);
            }
            if (TeleportHome.IsEnabled)
            {
                _commands = string.Format("{0} /sethome /home /delhome", _commands);
            }
            if (KillMe.IsEnabled)
            {
                _commands = string.Format("{0} /killme", _commands);
            }
            if (Day7.IsEnabled)
            {
                _commands = string.Format("{0} /day7", _commands);
            }
            if (Bloodmoon.IsEnabled)
            {
                _commands = string.Format("{0} /bloodmoon", _commands);
            }
            if (IsEnabled)
            {
                _commands = string.Format("{0} /pm /re", _commands);
            }
            if (ClanManager.IsEnabled)
            {
                _commands = string.Format("{0} /clancommands", _commands);
            }
            if (FirstClaimBlock.IsEnabled)
            {
                _commands = string.Format("{0} /claim", _commands);
            }
            if (Animals.IsEnabled)
            {
                _commands = string.Format("{0} /trackanimal /track", _commands);
            }
            if (VoteReward.IsEnabled)
            {
                _commands = string.Format("{0} /reward", _commands);
            }
            if (ChatHook.DonatorNameColoring)
            {
                _commands = string.Format("{0} /doncolor", _commands);
            }
            if (ChatHook.ReservedCheck)
            {
                _commands = string.Format("{0} /reserved", _commands);
            }
            if (AutoShutdown.IsEnabled)
            {
                _commands = string.Format("{0} /shutdown", _commands);
            }
            if (AdminList.IsEnabled)
            {
                _commands = string.Format("{0} /admin", _commands);
            }
            if (Travel.IsEnabled)
            {
                _commands = string.Format("{0} /travel", _commands);
            }
            if (ChatHook.SpecialPlayerNameColoring && ChatHook.SpecialPlayers.Contains(_cInfo.playerId))
            {
                _commands = string.Format("{0} /spcolor", _commands);
            }
            if (TeleportHome.IsEnabled & ReservedSlots.IsEnabled & ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
            {
                _commands = string.Format("{0} /sethome2 /home2 /delhome2", _commands);
            }
            if (WeatherVote.IsEnabled)
            {
                _commands = string.Format("{0} /weather", _commands);
            }
            if (WeatherVote.VoteOpen)
            {
                _commands = string.Format("{0} /sun /rain /snow /fog /wind", _commands);
            }
            if (AdminChat.IsEnabled && GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId))
            {
                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                if (Admin.PermissionLevel <= ChatHook.AdminLevel)
                {
                    _commands = string.Format("{0} @admins", _commands);
                    string[] _command = { "say" };
                    if (GameManager.Instance.adminTools.CommandAllowedFor(_command, _cInfo.playerId))
                    {
                        _commands = string.Format("{0} @all", _commands);
                    }
                    string[] _command1 = { "jail" };
                    if (GameManager.Instance.adminTools.CommandAllowedFor(_command1, _cInfo.playerId))
                    {
                        if (Jail.IsEnabled)
                        {
                            _commands = string.Format("{0} /jail", _commands);
                        }
                    }
                    string[] _command2 = { "mute" };
                    if (GameManager.Instance.adminTools.CommandAllowedFor(_command2, _cInfo.playerId))
                    {
                        _commands = string.Format("{0} /mute", _commands);
                    }
                }
            }
            if (Dict.Count > 0)
            {
                foreach (KeyValuePair<string, string> kvp in Dict)
                {
                    string _c = kvp.Key;
                    _commands = string.Format("{0} /{1}", _commands, _c);
                }
            }
            if (_commands.EndsWith("Commands are:"))
            {
                _commands = string.Format("{0}Sorry, there are no custom chat commands.", Config.ChatResponseColor);
            }
            _commands = string.Format("{0}[-]", _commands);
            return _commands;
        }

        public static void CheckCustomDelay(ClientInfo _cInfo, string _message, string _playerName, bool _announce)
        {
            int[] _c;
            if (Dict1.TryGetValue(_message, out _c))
            {
                if (_c[0] < 1)
                {
                    CommandResponse(_cInfo, _message, _playerName, _announce, _c);
                }
                else
                {
                    Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                    if (p == null || p.CustomCommand1 == null || p.CustomCommand2 == null || p.CustomCommand3 == null || p.CustomCommand4 == null || p.CustomCommand5 == null || p.CustomCommand6 == null || p.CustomCommand7 == null || p.CustomCommand8 == null || p.CustomCommand9 == null || p.CustomCommand10 == null)
                    {
                        CommandResponse(_cInfo, _message, _playerName, _announce, _c);
                    }
                    else
                    {
                        if (_c[1] == 1)
                        {
                            TimeSpan varTime = DateTime.Now - p.CustomCommand1;
                            double fractionalMinutes = varTime.TotalMinutes;
                            int _timepassed = (int)fractionalMinutes;
                        }
                        if (_c[1] == 2)
                        {
                            TimeSpan varTime = DateTime.Now - p.CustomCommand2;
                            double fractionalMinutes = varTime.TotalMinutes;
                            int _timepassed = (int)fractionalMinutes;
                        }
                        if (_c[1] == 3)
                        {
                            TimeSpan varTime = DateTime.Now - p.CustomCommand3;
                            double fractionalMinutes = varTime.TotalMinutes;
                            int _timepassed = (int)fractionalMinutes;
                        }
                        if (_c[1] == 4)
                        {
                            TimeSpan varTime = DateTime.Now - p.CustomCommand4;
                            double fractionalMinutes = varTime.TotalMinutes;
                            int _timepassed = (int)fractionalMinutes;
                        }
                        if (_c[1] == 5)
                        {
                            TimeSpan varTime = DateTime.Now - p.CustomCommand5;
                            double fractionalMinutes = varTime.TotalMinutes;
                            int _timepassed = (int)fractionalMinutes;
                        }
                        if (_c[1] == 6)
                        {
                            TimeSpan varTime = DateTime.Now - p.CustomCommand6;
                            double fractionalMinutes = varTime.TotalMinutes;
                            int _timepassed = (int)fractionalMinutes;
                        }
                        if (_c[1] == 7)
                        {
                            TimeSpan varTime = DateTime.Now - p.CustomCommand7;
                            double fractionalMinutes = varTime.TotalMinutes;
                            int _timepassed = (int)fractionalMinutes;
                        }
                        if (_c[1] == 8)
                        {
                            TimeSpan varTime = DateTime.Now - p.CustomCommand8;
                            double fractionalMinutes = varTime.TotalMinutes;
                            int _timepassed = (int)fractionalMinutes;
                        }
                        if (_c[1] == 9)
                        {
                            TimeSpan varTime = DateTime.Now - p.CustomCommand9;
                            double fractionalMinutes = varTime.TotalMinutes;
                            int _timepassed = (int)fractionalMinutes;
                        }
                        if (_c[1] == 10)
                        {
                            TimeSpan varTime = DateTime.Now - p.CustomCommand10;
                            double fractionalMinutes = varTime.TotalMinutes;
                            int _timepassed = (int)fractionalMinutes;
                        }
                        if (_timepassed > _c[0])
                        {
                            CommandResponse(_cInfo, _message, _playerName, _announce, _c);
                        }
                        else
                        {
                            int _timeleft = _c[0] - _timepassed;
                            string _phrase615;
                            if (!Phrases.Dict.TryGetValue(615, out _phrase615))
                            {
                                _phrase615 = "{PlayerName} you can only use {Command} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                            }
                            _phrase615 = _phrase615.Replace("{Command}", _message);
                            _phrase615 = _phrase615.Replace("{PlayerName}", _playerName);
                            _phrase615 = _phrase615.Replace("{DelayBetweenUses}", _c[0].ToString());
                            _phrase615 = _phrase615.Replace("{TimeRemaining}", _timeleft.ToString());
                            if (_announce)
                            {
                                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.ChatResponseColor, _phrase615), "Server", false, "", false);
                            }
                            else
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.ChatResponseColor, _phrase615), "Server", false, "", false));
                            }
                        }
                    }
                }
            }
        }

        public static string CommandResponse(ClientInfo _cInfo, string _message, string _playerName, bool _announce, int[] _c)
        {
            string _r;
            if (Dict.TryGetValue(_message, out _r))
            {
                string _response = _r;
                _response = _response.Replace("{EntityId}", _cInfo.entityId.ToString());
                _response = _response.Replace("{SteamId}", _cInfo.playerId);
                _response = _response.Replace("{PlayerName}", _playerName);
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("!{0}", _message), _playerName, false, "ServerTools", true);
                }
                if (_response.StartsWith("say "))
                {
                    if (_announce)
                    {
                        SdtdConsole.Instance.ExecuteSync(_response, _cInfo);
                    }
                    else
                    {
                        _response = _response.Replace("say ", "");
                        _response = _response.Replace("\"", "");
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format(_response), "Server", false, "ServerTools", false));
                    }
                }
                else
                {
                    SdtdConsole.Instance.ExecuteSync(_response, _cInfo);
                }

                int _delay = _c[0]; int _number = _c[1];
                if (_c[1] == 1)
                {
                    PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand1 = DateTime.Now;
                    PersistentContainer.Instance.Save();
                }
                if (_c[1] == 2)
                {
                    PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand2 = DateTime.Now;
                    PersistentContainer.Instance.Save();
                }
                if (_c[1] == 3)
                {
                    PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand3 = DateTime.Now;
                    PersistentContainer.Instance.Save();
                }
                if (_c[1] == 4)
                {
                    PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand4 = DateTime.Now;
                    PersistentContainer.Instance.Save();
                }
                if (_c[1] == 5)
                {
                    PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand5 = DateTime.Now;
                    PersistentContainer.Instance.Save();
                }
                if (_c[1] == 6)
                {
                    PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand6 = DateTime.Now;
                    PersistentContainer.Instance.Save();
                }
                if (_c[1] == 7)
                {
                    PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand7 = DateTime.Now;
                    PersistentContainer.Instance.Save();
                }
                if (_c[1] == 8)
                {
                    PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand8 = DateTime.Now;
                    PersistentContainer.Instance.Save();
                }
                if (_c[1] == 9)
                {
                    PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand9 = DateTime.Now;
                    PersistentContainer.Instance.Save();
                }
                if (_c[1] == 10)
                {
                    PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand10 = DateTime.Now;
                    PersistentContainer.Instance.Save();
                }
            }
            return "";
        }
    }
}