using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace ServerTools
{
    class ChatColor
    {
        public static bool IsEnabled = false, IsRunning = false, Rotate = false, Custom_Color = false;
        public static string Command_ccpr = "ccpr", Command_ccnr = "ccnr", Command_ccc = "ccc";
        public static Dictionary<string, string[]> Players = new Dictionary<string, string[]>();
        public static Dictionary<string, DateTime> ExpireDate = new Dictionary<string, DateTime>();

        private const string file = "ChatColor.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
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
            if (!IsEnabled && IsRunning)
            {
                Players.Clear();
                ExpireDate.Clear();
                FileWatcher.Dispose();
                IsRunning = false;
            }
        }

        public static void LoadXml()
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
                    Players.Clear();
                    ExpireDate.Clear();
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
                            }
                            else if (_line.HasAttribute("SteamId") && _line.HasAttribute("Name") && _line.HasAttribute("Group") && _line.HasAttribute("Prefix") &&
                                 _line.HasAttribute("NameColor") && _line.HasAttribute("PrefixColor") && _line.HasAttribute("Expires"))
                            {
                                string _steamId = _line.GetAttribute("SteamId");
                                string _name = _line.GetAttribute("Name");
                                string _group = _line.GetAttribute("Group");
                                string _prefix = _line.GetAttribute("Prefix");
                                string _nameColor = _line.GetAttribute("NameColor");
                                string _prefixColor = _line.GetAttribute("PrefixColor");
                                DateTime _dt = DateTime.Parse(_line.GetAttribute("Expires"));
                                if (ColorList.Colors.Count > 0 && ColorList.Colors.ContainsKey(_nameColor))
                                {
                                    ColorList.Colors.TryGetValue(_nameColor, out string _colorArray);
                                    _nameColor = _colorArray;
                                }
                                if (ColorList.Colors.Count > 0 && ColorList.Colors.ContainsKey(_prefixColor))
                                {
                                    ColorList.Colors.TryGetValue(_prefixColor, out string _colorArray);
                                    _prefixColor = _colorArray;
                                }
                                if ((_nameColor[0] != '[' || _nameColor[7] != ']') && _nameColor != "")
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring ChatColor.xml entry because of missing brackets[] or name matching the ColorList.xml for NameColor attribute: {0}", _line.OuterXml));
                                    continue;
                                }

                                if ((_prefixColor[0] != '[' || _prefixColor[7] != ']') && _prefixColor != "")
                                {
                                    Log.Warning(string.Format("[SERVERTOOLS] Ignoring ChatColor.xml entry because of missing brackets[] or name matching the ColorList.xml for PrefixColor attribute: {0}", _line.OuterXml));
                                    continue;
                                }

                                if (!Players.ContainsKey(_steamId) && DateTime.Now < _dt)
                                {
                                    string[] _c = new string[] { _name, _group, _prefix, _nameColor, _prefixColor };
                                    Players.Add(_steamId, _c);
                                    ExpireDate.Add(_steamId, _dt);
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
                Log.Out(string.Format("[SERVERTOOLS] Error in ChatColor.LoadXml: {0}", e.Message));
            }
        }

        public static void UpdateXml()
        {
            FileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<ChatColor>");
                sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                sw.WriteLine("<!-- PrefixColor and NameColor can come from the ColorList.xml -->");
                sw.WriteLine();
                sw.WriteLine();
                if (Players.Count > 0)
                {
                    foreach (KeyValuePair<string, string[]> kvp in Players)
                    {
                        ExpireDate.TryGetValue(kvp.Key, out DateTime _expiry);
                        sw.WriteLine(string.Format("    <Player SteamId=\"{0}\" Name=\"{1}\" Group=\"{2}\" Prefix=\"{3}\" NameColor=\"{4}\" PrefixColor=\"{5}\" Expires=\"{6}\" />", kvp.Key, kvp.Value[0], kvp.Value[1], kvp.Value[2], kvp.Value[3], kvp.Value[4], _expiry));
                    }
                }
                else
                {
                    sw.WriteLine("    <!-- <Player SteamId=\"12345678901234567\" Name=\"bob\" Group=\"admin\" Prefix=\"(Captain)\" NameColor=\"[FF0000]\" PrefixColor=\"[FFFFFF]\" Expires=\"10/29/2050 7:30:00 AM\" />");
                }
                sw.WriteLine("</ChatColor>");
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

        private static bool Expired(ClientInfo _cInfo, DateTime _expiry)
        {
            try
            {
                if (DateTime.Now > _expiry)
                {
                    Players.Remove(_cInfo.playerId);
                    ExpireDate.Remove(_cInfo.playerId);
                    UpdateXml();
                    Phrases.Dict.TryGetValue("ChatColor1", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ChatColor.Expired: {0}", e.Message));
            }
            return false;
        }

        public static void ShowColorAndExpiry(ClientInfo _cInfo)
        {
            try
            {
                ExpireDate.TryGetValue(_cInfo.playerId, out DateTime _expiry);
                if (!Expired(_cInfo, _expiry))
                {
                    Players.TryGetValue(_cInfo.playerId, out string[] _tags);
                    string _prefixNoBracket = _tags[4].Replace("[", "");
                    _prefixNoBracket = _prefixNoBracket.Replace("]", "");
                    string _nameNoBracket = _tags[3].Replace("[", "");
                    _nameNoBracket = _nameNoBracket.Replace("]", "");
                    Phrases.Dict.TryGetValue("ChatColor2", out string _phrase);
                    _phrase = _phrase.Replace("{PrefixTags}", _prefixNoBracket);
                    _phrase = _phrase.Replace("{NameTags}", _nameNoBracket);
                    _phrase = _phrase.Replace("{DateTime}", _expiry.ToString());
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ChatColor.ShowColorAndExpiry: {0}", e.Message));
            }
        }

        public static void SetPrefixColor(ClientInfo _cInfo, string messageLowerCase)
        {
            try
            {
                ExpireDate.TryGetValue(_cInfo.playerId, out DateTime _expiry);
                if (!Expired(_cInfo, _expiry))
                {
                    DateTime _lastPrefixColorChange = DateTime.Now;
                    if (PersistentContainer.Instance.Players[_cInfo.playerId].LastPrefixColorChange != null)
                    {
                        _lastPrefixColorChange = PersistentContainer.Instance.Players[_cInfo.playerId].LastPrefixColorChange;
                    }
                    TimeSpan varTime = DateTime.Now - _lastPrefixColorChange;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int _timepassed = (int)fractionalMinutes;
                    if (_timepassed >= 5)
                    {
                        Phrases.Dict.TryGetValue("ChatColor7", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    if (messageLowerCase.Length >= 8 && messageLowerCase[0] == '[' && messageLowerCase[7] == ']')
                    {
                        Players.TryGetValue(_cInfo.playerId, out string[] _tags);
                        _tags[4] = messageLowerCase;
                        Players[_cInfo.playerId] = _tags;
                        UpdateXml();
                        PersistentContainer.Instance.Players[_cInfo.playerId].LastPrefixColorChange = DateTime.Now;
                        PersistentContainer.DataChange = true;
                        string _prefixNoBracket = messageLowerCase.Replace("[", "");
                        _prefixNoBracket = _prefixNoBracket.Replace("]", "");
                        Phrases.Dict.TryGetValue("ChatColor4", out string _phrase);
                        _phrase = _phrase.Replace("{Tags}", _prefixNoBracket);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("ChatColor3", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ChatColor.SetPrefixColor: {0}", e.Message));
            }
        }

        public static void RotatePrefixColor(ClientInfo _cInfo)
        {
            try
            {
                ExpireDate.TryGetValue(_cInfo.playerId, out DateTime _expiry);
                if (!Expired(_cInfo, _expiry))
                {
                    if (ColorList.Colors.Count > 0)
                    {
                        Players.TryGetValue(_cInfo.playerId, out string[] _tags);
                        Phrases.Dict.TryGetValue("ChatColor4", out string _phrase);
                        KeyValuePair<string, string>[] Colors = ColorList.Colors.ToArray();
                        string _prefixNoBracket = "";
                        for (int i = 0; i < Colors.Length; i++)
                        {
                            if (Colors[i].Value == _tags[4])
                            {
                                if (Colors.Length > i + 1)
                                {
                                    _tags[4] = Colors[i + 1].Value;
                                    Players[_cInfo.playerId] = _tags;
                                    UpdateXml();
                                    _prefixNoBracket = Colors[i + 1].Value.Replace("[", "");
                                    _prefixNoBracket = _prefixNoBracket.Replace("]", "");
                                    _phrase = _phrase.Replace("{Tags}", _prefixNoBracket);
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    return;
                                }
                            }
                        }
                        _tags[4] = Colors[0].Value;
                        Players[_cInfo.playerId] = _tags;
                        UpdateXml();
                        _prefixNoBracket = Colors[0].Value.Replace("[", "");
                        _prefixNoBracket = _prefixNoBracket.Replace("]", "");
                        _phrase = _phrase.Replace("{Tags}", _prefixNoBracket);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("ChatColor6", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ChatColor.RotatePrefixColor: {0}", e.Message));
            }
        }

        public static void SetNameColor(ClientInfo _cInfo, string messageLowerCase)
        {
            try
            {
                ExpireDate.TryGetValue(_cInfo.playerId, out DateTime _expiry);
                if (!Expired(_cInfo, _expiry))
                {
                    DateTime _lastNameColorChange = DateTime.Now;
                    if (PersistentContainer.Instance.Players[_cInfo.playerId].LastNameColorChange != null)
                    {
                        _lastNameColorChange = PersistentContainer.Instance.Players[_cInfo.playerId].LastNameColorChange;
                    }
                    TimeSpan varTime = DateTime.Now - _lastNameColorChange;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int _timepassed = (int)fractionalMinutes;
                    if (_timepassed >= 5)
                    {
                        Phrases.Dict.TryGetValue("ChatColor7", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    if (messageLowerCase.Length >= 8 && messageLowerCase[0] == '[' && messageLowerCase[7] == ']')
                    {
                        Players.TryGetValue(_cInfo.playerId, out string[] _tags);
                        _tags[3] = messageLowerCase;
                        Players[_cInfo.playerId] = _tags;
                        UpdateXml();
                        PersistentContainer.Instance.Players[_cInfo.playerId].LastNameColorChange = DateTime.Now;
                        PersistentContainer.DataChange = true;

                        Phrases.Dict.TryGetValue("ChatColor5", out string _phrase);
                        _phrase = _phrase.Replace("{Tags}", messageLowerCase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("ChatColor3", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ChatColor.SetNameColor: {0}", e.Message));
            }
        }

        public static void RotateNameColor(ClientInfo _cInfo)
        {
            try
            {
                ExpireDate.TryGetValue(_cInfo.playerId, out DateTime _expiry);
                if (!Expired(_cInfo, _expiry))
                {
                    if (ColorList.Colors.Count > 0)
                    {
                        Players.TryGetValue(_cInfo.playerId, out string[] _tags);
                        Phrases.Dict.TryGetValue("ChatColor5", out string _phrase);
                        KeyValuePair<string, string>[] Colors = ColorList.Colors.ToArray();
                        string _nameNoBracket = "";
                        for (int i = 0; i < Colors.Length; i++)
                        {
                            if (Colors[i].Value == _tags[3])
                            {
                                if (Colors.Length > i + 1)
                                {
                                    Players[_cInfo.playerId][3] = Colors[i + 1].Value;
                                    UpdateXml();
                                    _nameNoBracket = Colors[i + 1].Value.Replace("[", "");
                                    _nameNoBracket = _nameNoBracket.Replace("]", "");
                                    _phrase = _phrase.Replace("{Tags}", _nameNoBracket);
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    return;
                                }
                            }
                        }
                        Players[_cInfo.playerId][3] = Colors[0].Value;
                        UpdateXml();
                        _nameNoBracket = Colors[0].Value.Replace("[", "");
                        _nameNoBracket = _nameNoBracket.Replace("]", "");
                        _phrase = _phrase.Replace("{Tags}", _nameNoBracket);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("ChatColor6", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ChatColor.RotateNameColor: {0}", e.Message));
            }
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
                    sw.WriteLine("<ChatColor>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("<!-- PrefixColor and NameColor can come from the ColorList.xml -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement _line = (XmlElement)_oldChildNodes[i];
                        if (_line.HasAttributes)
                        {
                            string _steamId = "", _name = "", _group = "", _prefix = "", _nameColor = "", _prefixColor = "";
                            DateTime _dateTime = DateTime.Now;
                            if (_line.HasAttribute("SteamId"))
                            {
                                _steamId = _line.GetAttribute("SteamId");
                            }
                            if (_line.HasAttribute("Name"))
                            {
                                _name = _line.GetAttribute("Name");
                            }
                            if (_line.HasAttribute("Group"))
                            {
                                _group = _line.GetAttribute("Group");
                            }
                            if (_line.HasAttribute("Prefix"))
                            {
                                _prefix = _line.GetAttribute("Prefix");
                            }
                            if (_line.HasAttribute("NameColor"))
                            {
                                _nameColor = _line.GetAttribute("NameColor");
                            }
                            if (_line.HasAttribute("PrefixColor"))
                            {
                                _prefixColor = _line.GetAttribute("PrefixColor");
                            }
                            if (_line.HasAttribute("Expires"))
                            {
                                DateTime.TryParse(_line.GetAttribute("Expires"), out _dateTime);
                            }
                            sw.WriteLine(string.Format("    <Player SteamId=\"{0}\" Name=\"{1}\" Group=\"{2}\" Prefix=\"{3}\" NameColor=\"{4}\" PrefixColor=\"{5}\" Expires=\"{6}\" />", _steamId, _name, _group, _prefix, _nameColor, _prefixColor, _dateTime));
                        }
                    }
                    sw.WriteLine("</ChatColor>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ChatColor.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}
