using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    public class CustomCommands
    {
        public static bool IsEnabled = false;
        public static string _chatcolor = "[00FF00]";
        public static SortedDictionary<string, string> _customCommands = new SortedDictionary<string, string>();
        private static string _file = "CustomChatCommands.xml";
        private static string _filepath = string.Format("{0}/{1}", Config._configpath, _file);
        public static FileSystemWatcher _fileWatcher = new FileSystemWatcher(Config._configpath, _file);
        public static bool IsRunning = false;

        public static List<string> list
        {
            get { return new List<string>(_customCommands.Keys); }
        }

        public static void Init()
        {
            if (IsEnabled && !IsRunning)
            {
                if (!Utils.FileExists(_filepath))
                {
                    UpdateXml();
                }
                LoadCustomCommandsXml();
                InitFileWatcher();
                IsRunning = true;
            }
        }

        private static void LoadCustomCommandsXml()
        {
            if (!Utils.FileExists(_filepath))
            {
                UpdateXml();
            }
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(_filepath);
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", _file, e.Message));
                return;
            }
            XmlNode _CommandsXml = xmlDoc.DocumentElement;
            _customCommands.Clear();
            foreach (XmlNode childNode in _CommandsXml.ChildNodes)
            {
                if (childNode.Name == "Commands")
                {
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
                        if (!_customCommands.ContainsKey(_line.GetAttribute("Command")))
                        {
                            _customCommands.Add(_line.GetAttribute("Command"), _line.GetAttribute("Response"));
                        }
                    }
                }
            }
        }

        private static void UpdateXml()
        {
            _fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(_filepath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<CustomCommands>");
                sw.WriteLine("    <Commands>");
                if (_customCommands.Count > 0)
                {
                    foreach (KeyValuePair<string, string> kvp in _customCommands)
                    {
                        sw.WriteLine(string.Format("        <Command Command=\"{0}\" Response=\"{1}\" />", kvp.Key, kvp.Value));
                    }
                }
                else
                {
                    sw.WriteLine("        <!-- possible variables {EntityId} {SteamId} {PlayerName} -->");
                    sw.WriteLine("        <!-- <Command Command=\"rules\" Response=\"say &quot;[00FF00]Visit YourSiteHere to see the rules.[-]&quot;\" /> -->");
                    sw.WriteLine("        <!-- <Command Command=\"website\" Response =\"say &quot;[00FF00]Visit YourSiteHere.[-]&quot;\" /> -->");
                    sw.WriteLine("        <!-- <Command Command=\"teamspeak\" Response=\"say &quot;[00FF00]The Teamspeak3 info is YourInfoHere.[-]&quot;\" /> -->");
                    sw.WriteLine("        <!-- <Command Command=\"kickme\" Response=\"kick {EntityId} &quot;You said kick me&quot;\" /> -->");
                }
                sw.WriteLine("    </Commands>");
                sw.WriteLine("</CustomCommands>");
                sw.Flush();
                sw.Close();
            }
            _fileWatcher.EnableRaisingEvents = true;
        }

        private static void InitFileWatcher()
        {
            _fileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
            _fileWatcher.Created += new FileSystemEventHandler(OnFileChanged);
            _fileWatcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            _fileWatcher.EnableRaisingEvents = true;
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            if (!Utils.FileExists(_filepath))
            {
                UpdateXml();
            }
            LoadCustomCommandsXml();
        }

        public static string GetChatCommands(ClientInfo _cInfo)
        {
            string _commands = string.Format("{0}Commands are:", _chatcolor);
            if (Gimme.IsEnabled)
            {
                _commands = string.Format("{0} /gimme", _commands);
            }
            if (TeleportHome.IsEnabled)
            {
                _commands = string.Format("{0} /sethome /delhome /home", _commands);
            }
            if (KillMe.IsEnabled)
            {
                _commands = string.Format("{0} /killme", _commands);
            }
            if (Day7.IsEnabled)
            {
                _commands = string.Format("{0} /day7", _commands);
            }
            if (Whisper.IsEnabled)
            {
                _commands = string.Format("{0} /pm", _commands);
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
            }
            if (list.Count > 0)
            {
                foreach (string _command in list)
                {
                    _commands = string.Format("{0} /{1}", _commands, _command);
                }
            }
            _commands = string.Format("{0}[-]", _commands);
            return _commands;
        }
    }
}