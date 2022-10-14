using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace ServerTools
{
    class LevelUp
    {
        public static bool IsEnabled = false, IsRunning = false, Xml_Only = false, Announce = true;

        public static Dictionary<int, int> PlayerLevels = new Dictionary<int, int>();

        private static Dictionary<int, string> Dict = new Dictionary<int, string>();

        private const string file = "LevelUp.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        private static XmlNodeList OldNodeList;

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        public static void Unload()
        {
            Dict.Clear();
            FileWatcher.Dispose();
            IsRunning = false;
        }

        public static void LoadXml()
        {
            try
            {
                if (!File.Exists(FilePath))
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
                bool upgrade = true;
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                if (childNodes != null)
                {
                    Dict.Clear();
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        if (childNodes[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)childNodes[i];
                            if (line.HasAttributes)
                            {
                                if (line.HasAttribute("Version") && line.GetAttribute("Version") == Config.Version)
                                {
                                    upgrade = false;
                                    continue;
                                }
                                else if (line.HasAttribute("Required") && line.HasAttribute("Command"))
                                {
                                    if (!int.TryParse(line.GetAttribute("Required"), out int level))
                                    {
                                        Log.Out(string.Format("[SERVERTOOLS] Ignoring LevelUp.xml entry because of invalid (non-numeric) value for 'Required' attribute: {0}", line.OuterXml));
                                        continue;
                                    }
                                    string command = line.GetAttribute("Command");
                                    if (!Dict.ContainsKey(level))
                                    {
                                        Dict.Add(level, command);
                                    }
                                }
                            }
                        }
                    }
                }
                if (upgrade)
                {
                    XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                    XmlNode node = nodeList[0];
                    XmlElement line = (XmlElement)nodeList[0];
                    if (line != null)
                    {
                        if (line.HasAttributes)
                        {
                            OldNodeList = nodeList;
                            File.Delete(FilePath);
                            UpgradeXml();
                            return;
                        }
                        else
                        {
                            nodeList = node.ChildNodes;
                            line = (XmlElement)nodeList[0];
                            if (line != null)
                            {
                                if (line.HasAttributes)
                                {
                                    OldNodeList = nodeList;
                                    File.Delete(FilePath);
                                    UpgradeXml();
                                    return;
                                }
                            }
                            File.Delete(FilePath);
                            UpdateXml();
                            Log.Out(string.Format("[SERVERTOOLS] The existing LevelUp.xml was too old or misconfigured. File deleted and rebuilt for version {0}", Config.Version));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message == "Specified cast is not valid.")
                {
                    File.Delete(FilePath);
                    UpdateXml();
                }
                else
                {
                    Log.Out(string.Format("[SERVERTOOLS] Error in LevelUp.LoadXml: {0}", e.Message));
                }
            }
        }

        private static void UpdateXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<Levels>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("    <!-- Command triggers console commands. Use ^ to separate multiple commands -->");
                    sw.WriteLine("    <!-- Possible variables for commands include whisper, global, {PlayerName}, {Id}, {EOS}, {PlayerId}, {Delay} -->");
                    sw.WriteLine("    <!-- <Level Required=\"300\" Command=\"global MAX LEVEL! Congratulations {PlayerName}!\" /> -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    if (Dict.Count > 0)
                    {
                        foreach (KeyValuePair<int, string> kvp in Dict)
                        {
                            sw.WriteLine(string.Format("    <Level Required=\"{0}\" Command=\"{1}\"  />", kvp.Key, kvp.Value));
                        }
                    }
                    sw.WriteLine("</Levels>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in LevelUp.UpdateXml: {0}", e.Message));
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
            if (!File.Exists(FilePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        public static void CheckLevel(ClientInfo _cInfo)
        {
            try
            {
                EntityPlayer player = GeneralFunction.GetEntityPlayer(_cInfo.entityId);
                if (player != null)
                {
                    if (PlayerLevels.ContainsKey(player.entityId))
                    {
                        PlayerLevels.TryGetValue(player.entityId, out int level);
                        if (player.Progression.Level > level)
                        {
                            PlayerLevels[player.entityId] = player.Progression.Level;
                            if (Xml_Only)
                            {
                                if (Dict.ContainsKey(player.Progression.Level))
                                {
                                    Dict.TryGetValue(player.Progression.Level, out string command);
                                    ProcessCommand(_cInfo, command);
                                    if (Announce)
                                    {
                                        Phrases.Dict.TryGetValue("LevelUp1", out string phrase);
                                        phrase = phrase.Replace("{PlayerName}", _cInfo.playerName);
                                        phrase = phrase.Replace("{Value}", player.Progression.Level.ToString());
                                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                    }
                                }
                            }
                            else
                            {
                                if (Dict.ContainsKey(player.Progression.Level))
                                {
                                    Dict.TryGetValue(player.Progression.Level, out string command);
                                    ProcessCommand(_cInfo, command);
                                }
                                if (Announce)
                                {
                                    Phrases.Dict.TryGetValue("LevelUp1", out string phrase);
                                    phrase = phrase.Replace("{PlayerName}", _cInfo.playerName);
                                    phrase = phrase.Replace("{Value}", player.Progression.Level.ToString());
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                                }
                            }
                        }
                    }
                    else
                    {
                        PlayerLevels.Add(player.entityId, player.Progression.Level);
                        if (player.Progression.Level == 1)
                        {
                            if (Dict.ContainsKey(player.Progression.Level))
                            {
                                Dict.TryGetValue(player.Progression.Level, out string command);
                                ProcessCommand(_cInfo, command);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in LevelUp.CheckLevel: {0}", e.Message));
            }
        }

        private static void ProcessCommand(ClientInfo _cInfo, string _command)
        {
            try
            {
                if (_command.Contains("^"))
                {
                    List<string> _commands = _command.Split('^').ToList();
                    for (int i = 0; i < _commands.Count; i++)
                    {
                        string _commandTrimmed = _commands[i].Trim();
                        if (_commandTrimmed.StartsWith("{Delay}"))
                        {
                            string[] _commandSplit = _commandTrimmed.Split(' ');
                            if (int.TryParse(_commandSplit[1], out int _time))
                            {
                                _commands.RemoveRange(0, i + 1);
                                Timers.Level_SingleUseTimer(_time, _cInfo.CrossplatformId.CombinedString, _commands);
                                return;
                            }
                            else
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Custom command error. Unable to commit delay with improper integer: {0}", _command));
                            }
                        }
                        else
                        {
                            Command(_cInfo, _commandTrimmed);
                        }
                    }
                }
                else
                {
                    Command(_cInfo, _command);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in LevelUp.ProcessCommand: {0}", e.Message));
            }
        }

        public static void LevelCommandDelayed(string _playerId, List<string> _commands)
        {
            try
            {
                ClientInfo cInfo = GeneralFunction.GetClientInfoFromNameOrId(_playerId);
                if (cInfo != null)
                {
                    for (int i = 0; i < _commands.Count; i++)
                    {
                        string _commandTrimmed = _commands[i].Trim();
                        if (_commandTrimmed.StartsWith("{Delay}"))
                        {
                            string[] _commandSplit = _commandTrimmed.Split(' ');
                            if (int.TryParse(_commandSplit[1], out int _time))
                            {
                                _commands.RemoveRange(0, i + 1);
                                Timers.Level_SingleUseTimer(_time, cInfo.CrossplatformId.CombinedString, _commands);
                                return;
                            }
                            else
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Custom command error. Unable to commit delay with improper integer: {0}", _commands));
                            }
                        }
                        else
                        {
                            Command(cInfo, _commandTrimmed);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in LevelUp.LevelCommandDelayed: {0}", e.Message));
            }
        }

        private static void Command(ClientInfo _cInfo, string _command)
        {
            try
            {
                _command = _command.Replace("{EntityId}", _cInfo.entityId.ToString());
                _command = _command.Replace("{Id}", _cInfo.PlatformId.CombinedString);
                _command = _command.Replace("{EOS}", _cInfo.CrossplatformId.CombinedString);
                _command = _command.Replace("{PlayerName}", _cInfo.playerName);
                if (_command.ToLower().StartsWith("global "))
                {
                    _command = _command.Replace("Global ", "");
                    _command = _command.Replace("global ", "");
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _command + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                }
                else if (_command.ToLower().StartsWith("whisper "))
                {
                    _command = _command.Replace("Whisper ", "");
                    _command = _command.Replace("whisper ", "");
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _command + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else if (_command.StartsWith("tele ") || _command.StartsWith("tp ") || _command.StartsWith("teleportplayer "))
                {
                    if (Zones.IsEnabled && Zones.ZonePlayer.ContainsKey(_cInfo.entityId))
                    {
                        Zones.ZonePlayer.Remove(_cInfo.entityId);
                    }
                    SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(_command, null);
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(_command, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in LevelUp.Command: {0}", e.Message));
            }
        }

        private static void UpgradeXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<Levels>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("    <!-- Command triggers console commands. Use ^ to separate multiple commands -->");
                    sw.WriteLine("    <!-- Possible variables for commands include whisper, global, {PlayerName}, {Id}, {EOS}, {PlayerId}, {Delay} -->");
                    sw.WriteLine("    <!-- <Level Required=\"300\" Command=\"global MAX LEVEL! Congratulations {PlayerName}!\" /> -->");
                    for (int i = 0; i < OldNodeList.Count; i++)
                    {
                        if (OldNodeList[i].NodeType == XmlNodeType.Comment && !OldNodeList[i].OuterXml.Contains("<!-- Command triggers console") &&
                            !OldNodeList[i].OuterXml.Contains("<!-- Possible variables") && !OldNodeList[i].OuterXml.Contains("<!-- <Level Required=\"300\"") &&
                            !OldNodeList[i].OuterXml.Contains("<!-- <Level Required=\"\""))
                        {
                            sw.WriteLine(OldNodeList[i].OuterXml);
                        }
                    }
                    sw.WriteLine();
                    sw.WriteLine();
                    for (int i = 0; i < OldNodeList.Count; i++)
                    {
                        if (OldNodeList[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)OldNodeList[i];
                            if (line.HasAttributes && line.Name == "Level")
                            {
                                string level = "", command = "";
                                if (line.HasAttribute("Required"))
                                {
                                    level = line.GetAttribute("Required");
                                }
                                if (line.HasAttribute("Command"))
                                {
                                    command = line.GetAttribute("Command");
                                }
                                sw.WriteLine(string.Format("    <Level Required=\"{0}\" Command=\"{1}\"  />", level, command));
                            }
                        }
                    }
                    sw.WriteLine("</Levels>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in LevelUp.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}
