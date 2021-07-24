using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace ServerTools
{
    class ChatColorPrefix
    {
        public static bool IsEnabled = false, IsRunning = false, Rotate = false, Custom_Color = false;
        public static string Command_ccpr = "ccpr", Command_ccnr = "ccnr", Command_ccp = "ccp";
        private const string file = "ChatColorPrefix.xml";
        private static readonly string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        public static Dictionary<string, string[]> Players = new Dictionary<string, string[]>();
        public static Dictionary<string, DateTime> ExpireDate = new Dictionary<string, DateTime>();
        private static readonly FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);

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
                if (childNode.Name == "ColorPrefix")
                {
                    Players.Clear();
                    ExpireDate.Clear();
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'ChatColorPrefix' from ChatColorPrefix.xml: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("SteamId"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ChatColorPrefix entry because of missing SteamId attribute from ChatColorPrefix.xml: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Name"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ChatColorPrefix entry because of missing Name attribute from ChatColorPrefix.xml: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Group"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ChatColorPrefix entry because of missing Group attribute from ChatColorPrefix.xml: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Prefix"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ChatColorPrefix entry because of missing Prefix attribute from ChatColorPrefix.xml: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("NameColor"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ChatColorPrefix entry because of missing NameColor attribute from ChatColorPrefix.xml: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("PrefixColor"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ChatColorPrefix entry because of missing PrefixColor attribute from ChatColorPrefix.xml: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Expires"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ChatColorPrefix entry because of missing Expires attribute from ChatColorPrefix.xml: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!DateTime.TryParse(_line.GetAttribute("Expires"), out DateTime _dt))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ChatColorPrefix entry because of invalid (date) value for 'Expires' attribute from ChatColorPrefix.xml: {0}", subChild.OuterXml));
                            continue;
                        }

                        string _steamId = _line.GetAttribute("SteamId");
                        string _name = _line.GetAttribute("Name");
                        string _group = _line.GetAttribute("Group");
                        string _prefix = _line.GetAttribute("Prefix");
                        string _nameColor = _line.GetAttribute("NameColor");
                        string _prefixColor = _line.GetAttribute("PrefixColor");
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
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ChatColorPrefix.xml entry because of missing brackets[] or name matching the ColorList.xml for Name Color attribute: {0}", subChild.OuterXml));
                            continue;
                        }

                        if ((_prefixColor[0] != '[' || _prefixColor[7] != ']') && _prefixColor != "")
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ChatColorPrefix.xml entry because of missing brackets[] or name matching the ColorList.xml for Prefix Color attribute: {0}", subChild.OuterXml));
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
        }

        public static void UpdateXml()
        {
            FileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<ColorPrefixes>");
                sw.WriteLine("    <ColorPrefix>");
                sw.WriteLine("        <!-- PrefixColor and NameColor can come from the ColorList.xml -->");
                if (Players.Count > 0)
                {
                    foreach (KeyValuePair<string, string[]> kvp in Players)
                    {
                        ExpireDate.TryGetValue(kvp.Key, out DateTime _expiry);
                        sw.WriteLine(string.Format("        <Player SteamId=\"{0}\" Name=\"{1}\" Group=\"{2}\" Prefix=\"{3}\" NameColor=\"{4}\" PrefixColor=\"{5}\" Expires=\"{6}\" />", kvp.Key, kvp.Value[0], kvp.Value[1], kvp.Value[2], kvp.Value[3], kvp.Value[4], _expiry));
                    }
                }
                else
                {
                    sw.WriteLine("        <!-- <Player SteamId=\"12345678901\" Name=\"bob\" Group=\"admin1\" Prefix=\"(Captain)\" NameColor=\"[FF0000]\" PrefixColor=\"[FFFFFF]\" Expires=\"10/29/2050 7:30:00 AM\" />");
                    sw.WriteLine("        <Player SteamId=\"09876543210\" Name=\"beth\" Group=\"admin2\" Prefix=\"(Admiral)\" NameColor=\"[008000]\" PrefixColor=\"[FFFFFF]\" Expires=\"10/29/3000 7:30:00 AM\" />");
                    sw.WriteLine("        <Player SteamId=\"12345098765\" Name=\"bev\" Group=\"mod1\" Prefix=\"(Marine)\" NameColor=\"[FF9933]\" PrefixColor=\"[FFFFFF]\" Expires=\"10/29/2150 7:30:00 AM\" />");
                    sw.WriteLine("        <Player SteamId=\"54321567890\" Name=\"betty\" Group=\"donor1\" Prefix=\"(Don)\" NameColor=\"[009000]\" PrefixColor=\"[FFFFFF]\" Expires=\"04/24/2021 7:30:00 AM\" />");
                    sw.WriteLine("        <Player SteamId=\"56789054321\" Name=\"bart\" Group=\"donor2\" Prefix=\"(Don)\" NameColor=\"[FF66CC]\" PrefixColor=\"\" Expires=\"11/22/2018 7:30:00 AM\" />");
                    sw.WriteLine("        <Player SteamId=\"23456789098\" Name=\"ben\" Group=\"donor3\" Prefix=\"(Don)\" NameColor=\"\" PrefixColor=\"[FFFFFF]\" Expires=\"02/03/2020 7:30:00 AM\" />");
                    sw.WriteLine("        <Player SteamId=\"34567890987\" Name=\"billy\" Group=\"user1\" Prefix=\"(Pimp)\" NameColor=\"Red\" PrefixColor=\"Y-G\" Expires=\"10/29/2050 7:30:00 AM\" />");
                    sw.WriteLine("        <Player SteamId=\"45678909876\" Name=\"bernard\" Group=\"user2\" Prefix=\"(OG)\" NameColor=\"[993366],[CC6699]\" PrefixColor=\"[FFA500],[FF8C00],[FF7F50]\" Expires=\"12/05/2019 7:30:00 AM\" />");
                    sw.WriteLine("        <Player SteamId=\"56789098765\" Name=\"bee man\" Group=\"user3\" Prefix=\"(Buzz)\" NameColor=\"\" PrefixColor=\"\" Expires=\"05/11/2020 7:30:00 AM\" />");
                    sw.WriteLine("        <Player SteamId=\"67890987654\" Name=\"beatlejuice\" Group=\"user4\" Prefix=\"\" NameColor=\"Mustard\" PrefixColor=\"G-B\" Expires=\"07/15/2030 7:30:00 AM\" /> -->");
                }
                sw.WriteLine("    </ColorPrefix>");
                sw.WriteLine("</ColorPrefixes>");
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
            if (!Utils.FileExists(filePath))
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
                    Phrases.Dict.TryGetValue(931, out string _phrase931);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase931 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ChatColorPrefix.Expired: {0}", e.Message));
            }
            return false;
        }

        public static void ShowColorAndExpiry(ClientInfo _cInfo)
        {
            ExpireDate.TryGetValue(_cInfo.playerId, out DateTime _expiry);
            if (!Expired(_cInfo, _expiry))
            {
                Players.TryGetValue(_cInfo.playerId, out string[] _tags);
                string _prefixNoBracket = _tags[4].Replace("[", "");
                _prefixNoBracket = _prefixNoBracket.Replace("]", "");
                string _nameNoBracket = _tags[3].Replace("[", "");
                _nameNoBracket = _nameNoBracket.Replace("]", "");
                Phrases.Dict.TryGetValue(932, out string _phrase932);
                _phrase932 = _phrase932.Replace("{PrefixTags}", _prefixNoBracket);
                _phrase932 = _phrase932.Replace("{NameTags}", _nameNoBracket);
                _phrase932 = _phrase932.Replace("{DateTime}", _expiry.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase932 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void SetPrefixColor(ClientInfo _cInfo, string messageLowerCase)
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
                    Phrases.Dict.TryGetValue(937, out string _phrase937);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase937 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                    Phrases.Dict.TryGetValue(934, out string _phrase934);
                    _phrase934 = _phrase934.Replace("{Tags}", _prefixNoBracket);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase934 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    Phrases.Dict.TryGetValue(933, out string _phrase933);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase933 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void RotatePrefixColor(ClientInfo _cInfo)
        {
            ExpireDate.TryGetValue(_cInfo.playerId, out DateTime _expiry);
            if (!Expired(_cInfo, _expiry))
            {
                if (ColorList.Colors.Count > 0)
                {
                    Players.TryGetValue(_cInfo.playerId, out string[] _tags);
                    Phrases.Dict.TryGetValue(934, out string _phrase934);
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
                                _phrase934 = _phrase934.Replace("{Tags}", _prefixNoBracket);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase934 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                return;
                            }
                        }
                    }
                    _tags[4] = Colors[0].Value;
                    Players[_cInfo.playerId] = _tags;
                    UpdateXml();
                    _prefixNoBracket = Colors[0].Value.Replace("[", "");
                    _prefixNoBracket = _prefixNoBracket.Replace("]", "");
                    _phrase934 = _phrase934.Replace("{Tags}", _prefixNoBracket);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase934 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    Phrases.Dict.TryGetValue(936, out string _phrase936);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase936 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void SetNameColor(ClientInfo _cInfo, string messageLowerCase)
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
                    Phrases.Dict.TryGetValue(937, out string _phrase937);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase937 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                if (messageLowerCase.Length >= 8 && messageLowerCase[0] == '[' && messageLowerCase[7] == ']')
                {
                    Players.TryGetValue(_cInfo.playerId, out string[] _tags);
                    _tags[3] = messageLowerCase;
                    Players[_cInfo.playerId] = _tags;
                    UpdateXml();
                    PersistentContainer.Instance.Players[_cInfo.playerId].LastNameColorChange = DateTime.Now;
                    PersistentContainer.DataChange = true;

                    Phrases.Dict.TryGetValue(935, out string _phrase935);
                    _phrase935 = _phrase935.Replace("{Tags}", messageLowerCase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase935 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    Phrases.Dict.TryGetValue(933, out string _phrase933);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase933 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void RotateNameColor(ClientInfo _cInfo)
        {
            ExpireDate.TryGetValue(_cInfo.playerId, out DateTime _expiry);
            if (!Expired(_cInfo, _expiry))
            {
                if (ColorList.Colors.Count > 0)
                {
                    Players.TryGetValue(_cInfo.playerId, out string[] _tags);
                    Phrases.Dict.TryGetValue(935, out string _phrase935);
                    KeyValuePair<string, string>[] Colors = ColorList.Colors.ToArray();
                    for (int i = 0; i < Colors.Length; i++)
                    {
                        if (Colors[i].Value == _tags[3])
                        {
                            if (Colors.Length > i + 1)
                            {
                                Players[_cInfo.playerId][3] = Colors[i + 1].Value;
                                UpdateXml();
                                _phrase935 = _phrase935.Replace("{Tags}", Colors[i + 1].Value);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase935 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                return;
                            }
                        }
                    }
                    Players[_cInfo.playerId][3] = Colors[0].Value;
                    UpdateXml();
                    _phrase935 = _phrase935.Replace("{Tags}", Colors[0].Value);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase935 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    Phrases.Dict.TryGetValue(936, out string _phrase936);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase936 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }
    }
}
