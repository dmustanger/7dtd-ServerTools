using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    public class CustomCommands
    {
        public static bool IsEnabled = false;
        public static bool IsRunning = false;
        public static string ChatColor = "[00FF00]";
        public static SortedDictionary<string, string[]> Dict = new SortedDictionary<string, string[]>();
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
                        if (!_line.HasAttribute("Command"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Commands entry because of missing a Command attribute: {0}", subChild.OuterXml));
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
                                Log.Out(string.Format("[SERVERTOOLS] Using default value of 0 for DelayBetweenUses for command entry {1} because of invalid (non-numeric) value: {0}", subChild.OuterXml, _line.GetAttribute("Command")));
                            }
                        }
                        string _command = _line.GetAttribute("Command");
                        _command = _command.ToLower();
                        if (!Dict.ContainsKey(_command))
                        {
                            string[] _response = new string[] { _line.GetAttribute("Response"), _delay.ToString() };
                            Dict.Add(_command, _response);
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
                    foreach (KeyValuePair<string, string[]> kvp in Dict)
                    {
                        sw.WriteLine(string.Format("        <Command Command=\"{0}\" Response=\"{1}\" DelayBetweenUses=\"{2}\" />", kvp.Key, kvp.Value[0], kvp.Value[1]));
                    }
                }
                else
                {
                    sw.WriteLine("        <Command Command=\"help\" Response=\"say &quot;[00FF00]Type /commands for a list of chat commands.[-]&quot;\" DelayBetweenUses=\"0\" />");
                    sw.WriteLine("        <Command Command=\"info\" Response=\"say &quot;[00FF00]Type /commands for a list of chat commands.[-]&quot;\" DelayBetweenUses=\"0\" />");
                    sw.WriteLine("        <Command Command=\"rules\" Response=\"say &quot;[00FF00]Visit YourSiteHere to see the rules.[-]&quot;\" DelayBetweenUses=\"0\" />");
                    sw.WriteLine("        <Command Command=\"website\" Response =\"say &quot;[00FF00]Visit YourSiteHere.[-]&quot;\" DelayBetweenUses=\"0\" />");
                    sw.WriteLine("        <Command Command=\"teamspeak\" Response=\"say &quot;[00FF00]The Teamspeak3 info is YourInfoHere.[-]&quot;\" DelayBetweenUses=\"0\" />");
                    sw.WriteLine("        <Command Command=\"kickme\" Response=\"kick {EntityId} &quot;You said kick me&quot;\" DelayBetweenUses=\"60\" />");
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
            string _commands = string.Format("{0}Commands are:", ChatColor);
            if (Animals.IsEnabled)
            {
                _commands = string.Format("{0} /gimme", _commands);
            }
            if (Gimme.IsEnabled)
            {
                _commands = string.Format("{0} /gimme", _commands);
            }
            if (TeleportHome.IsEnabled)
            {
                _commands = string.Format("{0} /sethome /home", _commands);
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
            if (AdminChat.IsEnabled && GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId))
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
            if (Dict.Count > 0)
            {
                foreach (KeyValuePair<string, string[]> kvp in Dict)
                {
                    string _c = kvp.Key;
                    _commands = string.Format("{0} /{1}", _commands, _c);
                }
            }
            if (_commands.EndsWith("Commands are:") )
            {
                _commands = string.Format("{0}Sorry, there are no custom chat commands.", ChatColor);
            }
            _commands = string.Format("{0}[-]", _commands);
            return _commands;
        }
    }
}