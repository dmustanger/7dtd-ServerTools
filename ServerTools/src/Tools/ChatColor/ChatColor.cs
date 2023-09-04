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

        private static char[] Symbols = { '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '?', '/', '\\' };
        private const string file = "ChatColor.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        public static void Unload()
        {
            Players.Clear();
            ExpireDate.Clear();
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
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                Players.Clear();
                ExpireDate.Clear();
                if (childNodes != null && childNodes[0] != null && childNodes[0].OuterXml.Contains("Version") && childNodes[0].OuterXml.Contains(Config.Version))
                {
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        if (childNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement line = (XmlElement)childNodes[i];
                        if (!line.HasAttributes)
                        {
                            continue;
                        }
                        if (line.HasAttribute("Id") && line.HasAttribute("Name") && line.HasAttribute("NameColor") &&
                              line.HasAttribute("Prefix") && line.HasAttribute("PrefixColor") && line.HasAttribute("Expires"))
                        {
                            string id = line.GetAttribute("Id");
                            if (id == "")
                            {
                                continue;
                            }
                            string name = line.GetAttribute("Name");
                            string nameColor = line.GetAttribute("NameColor").ToLower();
                            string prefix = line.GetAttribute("Prefix");
                            string prefixColor = line.GetAttribute("PrefixColor").ToLower();
                            if (!DateTime.TryParse(line.GetAttribute("Expires"), out DateTime dt))
                            {
                                Log.Warning(string.Format("[SERVERTOOLS] Ignoring ChatColor.xml entry. Invalid (date) value for 'Expires' attribute: {0}", line.OuterXml));
                                continue;
                            }
                            if (ColorList.Colors.Count > 0 && ColorList.Colors.ContainsKey(nameColor))
                            {
                                ColorList.Colors.TryGetValue(nameColor, out nameColor);
                            }
                            if (ColorList.Colors.Count > 0 && ColorList.Colors.ContainsKey(prefixColor))
                            {
                                ColorList.Colors.TryGetValue(prefixColor, out prefixColor);
                            }
                            if (!Players.ContainsKey(id))
                            {
                                string[] c = new string[] { name, nameColor, prefix, prefixColor };
                                Players.Add(id, c);
                                ExpireDate.Add(id, dt);
                            }
                        }
                    }
                    return;
                }
                else
                {
                    XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                    if (nodeList != null)
                    {
                        Timers.UpgradeChatColorXml(nodeList);
                        //UpgradeXml(nodeList);
                        return;
                    }
                    File.Delete(FilePath);
                    UpdateXml();
                    return;
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
                    Log.Out(string.Format("[SERVERTOOLS] Error in ChatColor.LoadXml: {0}", e.Message));
                }
            }
        }

        public static void UpdateXml()
        {
            FileWatcher.EnableRaisingEvents = false;

            using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<ChatColor>");
                sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                sw.WriteLine("    <!-- NameColor and PrefixColor can come from the ColorList.xml -->");
                sw.WriteLine("    <!-- <Player Id=\"Steam_12345678901234567\" Name=\"bob\" NameColor=\"[FF0000]\" Prefix=\"(Captain)\" PrefixColor=\"Red\" Expires=\"2050-01-11 07:30:00\" /> -->");
                if (Players.Count > 0)
                {
                    foreach (KeyValuePair<string, string[]> kvp in Players)
                    {
                        ExpireDate.TryGetValue(kvp.Key, out DateTime expiry);
                        sw.WriteLine(string.Format("    <Player Id=\"{0}\" Name=\"{1}\" NameColor=\"{2}\" Prefix=\"{3}\" PrefixColor=\"{4}\" Expires=\"{5}\" />", kvp.Key, kvp.Value[0], kvp.Value[1], kvp.Value[2], kvp.Value[3], expiry));
                    }
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
            if (!File.Exists(FilePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        private static bool Expired(DateTime _expiry)
        {
            try
            {
                if (DateTime.Now > _expiry)
                {
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
                if (ExpireDate.TryGetValue(_cInfo.PlatformId.CombinedString, out DateTime expiry))
                {
                    if (!Expired(expiry))
                    {
                        Players.TryGetValue(_cInfo.PlatformId.CombinedString, out string[] colorTags);
                        Phrases.Dict.TryGetValue("ChatColor2", out string phrase);
                        phrase = phrase.Replace("{NameTags}", colorTags[1]);
                        phrase = phrase.Replace("{PrefixTags}", colorTags[3]);
                        phrase = phrase.Replace("{DateTime}", expiry.ToString());
                        phrase = phrase.Replace("[", "");
                        phrase = phrase.Replace("]", "");
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else if (ExpireDate.TryGetValue(_cInfo.CrossplatformId.CombinedString, out expiry))
                {
                    if (!Expired(expiry))
                    {
                        Players.TryGetValue(_cInfo.CrossplatformId.CombinedString, out string[] colorTags);
                        Phrases.Dict.TryGetValue("ChatColor2", out string phrase);
                        phrase = phrase.Replace("{NameTags}", colorTags[1]);
                        phrase = phrase.Replace("{PrefixTags}", colorTags[3]);
                        phrase = phrase.Replace("{DateTime}", expiry.ToString());
                        phrase = phrase.Replace("[", "");
                        phrase = phrase.Replace("]", "");
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ChatColor.ShowColorAndExpiry: {0}", e.Message));
            }
        }

        public static void SetPrefixColor(ClientInfo _cInfo, string _messageLowerCase)
        {
            try
            {
                DateTime expiry = DateTime.Now;
                if (ExpireDate.ContainsKey(_cInfo.PlatformId.CombinedString))
                {
                    ExpireDate.TryGetValue(_cInfo.PlatformId.CombinedString, out expiry);
                }
                else if (ExpireDate.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                {
                    ExpireDate.TryGetValue(_cInfo.CrossplatformId.CombinedString, out expiry);
                }
                if (!Expired(expiry))
                {
                    DateTime lastPrefixColorChange = DateTime.Now;
                    if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastPrefixColorChange != null)
                    {
                        lastPrefixColorChange = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastPrefixColorChange;
                    }
                    TimeSpan varTime = DateTime.Now - lastPrefixColorChange;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int timepassed = (int)fractionalMinutes;
                    if (timepassed >= 5)
                    {
                        Phrases.Dict.TryGetValue("ChatColor7", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    if (_messageLowerCase.Length >= 8 && _messageLowerCase[0] == '[' && _messageLowerCase[7] == ']')
                    {
                        string[] colorTags = null;
                        if (Players.ContainsKey(_cInfo.PlatformId.CombinedString))
                        {
                            Players[_cInfo.PlatformId.CombinedString][3] = _messageLowerCase;
                        }
                        else if (Players.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                        {
                            Players[_cInfo.CrossplatformId.CombinedString][3] = _messageLowerCase;
                        }
                        UpdateXml();
                        PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastPrefixColorChange = DateTime.Now;
                        PersistentContainer.DataChange = true;
                        Phrases.Dict.TryGetValue("ChatColor4", out string phrase);
                        phrase = phrase.Replace("{Tags}", colorTags[3]);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("ChatColor3", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                DateTime expiry = DateTime.Now;
                if (ExpireDate.ContainsKey(_cInfo.PlatformId.CombinedString))
                {
                    ExpireDate.TryGetValue(_cInfo.PlatformId.CombinedString, out expiry);
                }
                else if (ExpireDate.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                {
                    ExpireDate.TryGetValue(_cInfo.CrossplatformId.CombinedString, out expiry);
                }
                if (!Expired(expiry))
                {
                    if (ColorList.Colors.Count > 0)
                    {
                        string[] colorTags = null;
                        if (Players.ContainsKey(_cInfo.PlatformId.CombinedString))
                        {
                            Players.TryGetValue(_cInfo.PlatformId.CombinedString, out colorTags);
                            KeyValuePair<string, string>[] Colors = ColorList.Colors.ToArray();
                            for (int i = 0; i < Colors.Length; i++)
                            {
                                if (Colors[i].Value == colorTags[3])
                                {
                                    if (Colors.Length > i + 1)
                                    {
                                        colorTags[3] = Colors[i + 1].Value;
                                        Players[_cInfo.PlatformId.CombinedString] = colorTags;
                                    }
                                }
                            }
                            colorTags[3] = Colors[0].Value;
                            Players[_cInfo.PlatformId.CombinedString] = colorTags;
                        }
                        else if (Players.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                        {
                            Players.TryGetValue(_cInfo.CrossplatformId.CombinedString, out colorTags);
                            KeyValuePair<string, string>[] Colors = ColorList.Colors.ToArray();
                            for (int i = 0; i < Colors.Length; i++)
                            {
                                if (Colors[i].Value == colorTags[3])
                                {
                                    if (Colors.Length > i + 1)
                                    {
                                        colorTags[3] = Colors[i + 1].Value;
                                        Players[_cInfo.CrossplatformId.CombinedString] = colorTags;
                                    }
                                }
                            }
                            colorTags[3] = Colors[0].Value;
                            Players[_cInfo.CrossplatformId.CombinedString] = colorTags;
                        }
                        UpdateXml();
                        Phrases.Dict.TryGetValue("ChatColor4", out string phrase1);
                        phrase1 = phrase1.Replace("{Tags}", colorTags[3]);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("ChatColor6", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                DateTime expiry = DateTime.Now;
                if (ExpireDate.ContainsKey(_cInfo.PlatformId.CombinedString))
                {
                    ExpireDate.TryGetValue(_cInfo.PlatformId.CombinedString, out expiry);
                }
                else if (ExpireDate.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                {
                    ExpireDate.TryGetValue(_cInfo.CrossplatformId.CombinedString, out expiry);
                }
                if (!Expired(expiry))
                {
                    DateTime lastNameColorChange = DateTime.Now;
                    if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastNameColorChange != null)
                    {
                        lastNameColorChange = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastNameColorChange;
                    }
                    TimeSpan varTime = DateTime.Now - lastNameColorChange;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int timepassed = (int)fractionalMinutes;
                    if (timepassed >= 5)
                    {
                        Phrases.Dict.TryGetValue("ChatColor7", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    if (messageLowerCase.Length >= 8 && messageLowerCase[0] == '[' && messageLowerCase[7] == ']')
                    {
                        string[] colorTags = null;
                        if (Players.ContainsKey(_cInfo.PlatformId.CombinedString))
                        {
                            Players.TryGetValue(_cInfo.PlatformId.CombinedString, out colorTags);
                            colorTags[1] = messageLowerCase;
                            Players[_cInfo.PlatformId.CombinedString] = colorTags;
                        }
                        else if (Players.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                        {
                            Players.TryGetValue(_cInfo.CrossplatformId.CombinedString, out colorTags);
                            colorTags[1] = messageLowerCase;
                            Players[_cInfo.CrossplatformId.CombinedString] = colorTags;
                        }
                        UpdateXml();
                        PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastNameColorChange = DateTime.Now;
                        PersistentContainer.DataChange = true;
                        Phrases.Dict.TryGetValue("ChatColor5", out string phrase);
                        phrase = phrase.Replace("{Tags}", colorTags[1]);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                DateTime expiry = DateTime.Now;
                if (ExpireDate.ContainsKey(_cInfo.PlatformId.CombinedString))
                {
                    ExpireDate.TryGetValue(_cInfo.PlatformId.CombinedString, out expiry);
                }
                else if (ExpireDate.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                {
                    ExpireDate.TryGetValue(_cInfo.CrossplatformId.CombinedString, out expiry);
                }
                if (!Expired(expiry))
                {
                    if (ColorList.Colors.Count > 0)
                    {
                        string[] colorTags = null;
                        if (Players.ContainsKey(_cInfo.PlatformId.CombinedString))
                        {
                            Players.TryGetValue(_cInfo.PlatformId.CombinedString, out colorTags);
                            Phrases.Dict.TryGetValue("ChatColor5", out string phrase);
                            KeyValuePair<string, string>[] Colors = ColorList.Colors.ToArray();
                            for (int i = 0; i < Colors.Length; i++)
                            {
                                if (Colors[i].Value == colorTags[1])
                                {
                                    if (Colors.Length > i + 1)
                                    {
                                        colorTags[1] = Colors[i + 1].Value;
                                        Players[_cInfo.PlatformId.CombinedString] = colorTags;
                                    }
                                }
                            }
                            colorTags[1] = Colors[0].Value;
                            Players[_cInfo.PlatformId.CombinedString] = colorTags;
                        }
                        else if (Players.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                        {
                            Players.TryGetValue(_cInfo.CrossplatformId.CombinedString, out colorTags);
                            Phrases.Dict.TryGetValue("ChatColor5", out string phrase);
                            KeyValuePair<string, string>[] Colors = ColorList.Colors.ToArray();
                            for (int i = 0; i < Colors.Length; i++)
                            {
                                if (Colors[i].Value == colorTags[1])
                                {
                                    if (Colors.Length > i + 1)
                                    {
                                        colorTags[1] = Colors[i + 1].Value;
                                        Players[_cInfo.CrossplatformId.CombinedString] = colorTags;
                                    }
                                }
                            }
                            colorTags[1] = Colors[0].Value;
                            Players[_cInfo.CrossplatformId.CombinedString] = colorTags;
                        }
                        UpdateXml();
                        Phrases.Dict.TryGetValue("ChatColor5", out string phrase1);
                        phrase1 = phrase1.Replace("{Tags}", colorTags[1]);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("ChatColor6", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ChatColor.RotateNameColor: {0}", e.Message));
            }
        }

        public static string ApplyNameColor(ClientInfo _cInfo, EChatType _chatType, string _name)
        {
            if (_name.Length < 1 || _name.Length > 7 && _name[0] == '[' && _name[7] == ']')
            {
                return _name;
            }

            string nameColor = "";
            string prefix = "";
            string prefixColor = "";
            DateTime dt = new DateTime();
            if (ExpireDate.ContainsKey(_cInfo.PlatformId.CombinedString))
            {
                ExpireDate.TryGetValue(_cInfo.PlatformId.CombinedString, out dt);
            }
            else if (ExpireDate.ContainsKey(_cInfo.CrossplatformId.CombinedString))
            {
                ExpireDate.TryGetValue(_cInfo.CrossplatformId.CombinedString, out dt);
            }
            if (DateTime.Now < dt)
            {
                string[] chatColorData;
                Players.TryGetValue(_cInfo.PlatformId.CombinedString, out chatColorData);
                if (chatColorData == null)
                {
                    Players.TryGetValue(_cInfo.CrossplatformId.CombinedString, out chatColorData);
                }
                nameColor = chatColorData[1];
                prefix = chatColorData[2];
                prefixColor = chatColorData[3];
            }
            else if (ChatHook.Normal_Player_Color_Prefix)
            {
                if (ChatHook.Normal_Player_Name_Color != "")
                {
                    nameColor = ChatHook.Normal_Player_Name_Color;
                }
                if (ChatHook.Normal_Player_Prefix != "")
                {
                    prefix = ChatHook.Normal_Player_Prefix;
                }
                if (ChatHook.Normal_Player_Prefix_Color != "")
                {
                    prefixColor = ChatHook.Normal_Player_Prefix_Color;
                }
            }
            if (ClanManager.IsEnabled && PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanName != null &&
                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanName != "")
            {
                prefix = prefix.Insert(prefix.Length, PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanName);
            }
            if (_chatType == EChatType.Friends)
            {
                prefix = prefix.Insert(0, "(Friends)");
                if (ChatHook.Friend_Chat_Color.StartsWith("[") && ChatHook.Friend_Chat_Color.EndsWith("]"))
                {
                    prefix = prefix.Insert(0, ChatHook.Friend_Chat_Color);
                }
                prefix = prefix.Insert(prefix.Length, "[-]");
            }
            else if (_chatType == EChatType.Party)
            {
                prefix = prefix.Insert(0, "(Party)");
                if (ChatHook.Party_Chat_Color.StartsWith("[") && ChatHook.Party_Chat_Color.EndsWith("]"))
                {
                    prefix = prefix.Insert(0, ChatHook.Party_Chat_Color);
                }
                prefix = prefix.Insert(prefix.Length, "[-]");
            }
            if (prefix != "" && prefixColor != "")
            {
                if (prefixColor.Contains(","))
                {
                    int colorIndex = 0;
                    string[] colors = prefixColor.Split(',');
                    for (int i = 0; i < prefix.Length; i += 9)
                    {
                        prefix = prefix.Insert(i, colors[colorIndex]);
                        colorIndex++;
                        if (colorIndex == colors.Length)
                        {
                            colorIndex = 0;
                        }
                    }
                    prefix = prefix.Insert(prefix.Length, "[ffffff]");
                }
                else
                {
                    prefix = prefix.Insert(0, prefixColor);
                    prefix = prefix.Insert(prefix.Length, "[-]");
                }
            }
            if (nameColor != "" && nameColor != "")
            {
                if (nameColor.Contains(","))
                {
                    int count = 0, colorIndex = 0;
                    string[] colors = nameColor.Split(',');
                    for (int i = 0; i < _name.Length; i += 9)
                    {
                        _name = _name.Insert(i, colors[colorIndex]);
                        count++;
                        colorIndex++;
                        if (colorIndex == colors.Length)
                        {
                            colorIndex = 0;
                        }
                    }
                    _name = _name.Insert(_name.Length, "[ffffff]");
                }
                else
                {
                    _name = _name.Insert(0, nameColor);
                    _name = _name.Insert(_name.Length, "[-]");
                }
            }
            if (prefix != "")
            {
                _name = string.Format("{0} {1}", prefix, _name);
            }
            return _name;
        }

        public static string ApplyMessageColor(string _msg)
        {
            if (ChatHook.Message_Color.StartsWith("[") && ChatHook.Message_Color.EndsWith("]"))
            {
                _msg = _msg.Insert(0, ChatHook.Message_Color);
                _msg = _msg.Insert(_msg.Length, "[-]");
            }
            return _msg;
        }
        
        public static void UpgradeXml(XmlNodeList nodeList)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                File.Delete(FilePath);
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<ChatColor>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- NameColor and PrefixColor can come from the ColorList.xml -->");
                    sw.WriteLine("    <!-- <Player Id=\"Steam_12345678901234567\" Name=\"bob\" NameColor=\"[FF0000]\" Prefix=\"(Captain)\" PrefixColor=\"Red\" Expires=\"2050-01-11 07:30:00\" /> -->");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType == XmlNodeType.Comment)
                        {
                            if (nodeList[i].NodeType == XmlNodeType.Comment)
                            {
                                if (!nodeList[i].OuterXml.Contains("NameColor and") &&
                                !nodeList[i].OuterXml.Contains("<Player Id=\"Steam_12345678901234567") && !nodeList[i].OuterXml.Contains("Do not forget") &&
                                !nodeList[i].OuterXml.Contains("<Version") && !nodeList[i].OuterXml.Contains("<Player Id=\"\""))
                                {
                                    sw.WriteLine(nodeList[i].OuterXml);
                                }
                            }
                        }
                    }
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)nodeList[i];
                            if (line.HasAttributes && line.Name == "Player")
                            {
                                string id = "", name = "", nameColor = "", prefix = "", prefixColor = "";
                                DateTime dateTime = DateTime.Now;
                                if (line.HasAttribute("Id"))
                                {
                                    id = line.GetAttribute("Id");
                                }
                                if (line.HasAttribute("Name"))
                                {
                                    name = line.GetAttribute("Name");
                                }
                                if (line.HasAttribute("Prefix"))
                                {
                                    prefix = line.GetAttribute("Prefix");
                                }
                                if (line.HasAttribute("NameColor"))
                                {
                                    nameColor = line.GetAttribute("NameColor");
                                }
                                if (line.HasAttribute("PrefixColor"))
                                {
                                    prefixColor = line.GetAttribute("PrefixColor");
                                }
                                if (line.HasAttribute("Expires"))
                                {
                                    DateTime.TryParse(line.GetAttribute("Expires"), out dateTime);
                                }
                                sw.WriteLine(string.Format("    <Player Id=\"{0}\" Name=\"{1}\" NameColor=\"{2}\" Prefix=\"{3}\" PrefixColor=\"{4}\" Expires=\"{5}\" />", id, name, nameColor, prefix, prefixColor, dateTime));
                            }
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
