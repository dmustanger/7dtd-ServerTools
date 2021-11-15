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

        private static XmlNodeList OldNodeList;

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
                bool upgrade = true;
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                if (childNodes != null)
                {
                    Players.Clear();
                    ExpireDate.Clear();
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
                                else if (line.HasAttribute("SteamId") && line.HasAttribute("Name") && line.HasAttribute("NameColor") &&
                                      line.HasAttribute("Prefix") && line.HasAttribute("PrefixColor") && line.HasAttribute("Expires"))
                                {
                                    string steamId = line.GetAttribute("SteamId");
                                    string name = line.GetAttribute("Name");
                                    string nameColor = line.GetAttribute("NameColor");
                                    string prefix = line.GetAttribute("Prefix");
                                    string prefixColor = line.GetAttribute("PrefixColor");
                                    DateTime dt = DateTime.Parse(line.GetAttribute("Expires"));
                                    if (ColorList.Colors.Count > 0 && ColorList.Colors.ContainsKey(nameColor))
                                    {
                                        ColorList.Colors.TryGetValue(nameColor, out string colorArray);
                                        nameColor = colorArray;
                                    }
                                    if (ColorList.Colors.Count > 0 && ColorList.Colors.ContainsKey(prefixColor))
                                    {
                                        ColorList.Colors.TryGetValue(prefixColor, out string colorArray);
                                        prefixColor = colorArray;
                                    }
                                    if (!Players.ContainsKey(steamId))
                                    {
                                        string[] c = new string[] { name, nameColor, prefix, prefixColor };
                                        Players.Add(steamId, c);
                                        ExpireDate.Add(steamId, dt);
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
                            Utils.FileDelete(FilePath);
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
                                    Utils.FileDelete(FilePath);
                                    UpgradeXml();
                                    return;
                                }
                            }
                            Utils.FileDelete(FilePath);
                            UpdateXml();
                            Log.Out(string.Format("[SERVERTOOLS] The existing ChatColor.xml was too old or misconfigured. File deleted and rebuilt for version {0}", Config.Version));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message == "Specified cast is not valid.")
                {
                    Utils.FileDelete(FilePath);
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
                sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                sw.WriteLine("    <!-- NameColor and PrefixColor can come from the ColorList.xml -->");
                sw.WriteLine("    <!-- <Player SteamId=\"12345678901234567\" Name=\"bob\" NameColor=\"[FF0000]\" Prefix=\"(Captain)\" PrefixColor=\"Red\" Expires=\"10/29/2050 7:30:00 AM\" /> -->");
                sw.WriteLine();
                sw.WriteLine();
                if (Players.Count > 0)
                {
                    foreach (KeyValuePair<string, string[]> kvp in Players)
                    {
                        ExpireDate.TryGetValue(kvp.Key, out DateTime _expiry);
                        sw.WriteLine(string.Format("    <Player SteamId=\"{0}\" Name=\"{1}\" NameColor=\"{2}\" Prefix=\"{3}\" PrefixColor=\"{4}\" Expires=\"{5}\" />", kvp.Key, kvp.Value[0], kvp.Value[1], kvp.Value[2], kvp.Value[3], _expiry));
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
                ExpireDate.TryGetValue(_cInfo.playerId, out DateTime expiry);
                if (!Expired(_cInfo, expiry))
                {
                    Players.TryGetValue(_cInfo.playerId, out string[] colorTags);
                    Phrases.Dict.TryGetValue("ChatColor2", out string phrase);
                    phrase = phrase.Replace("{NameTags}", colorTags[1]);
                    phrase = phrase.Replace("{PrefixTags}", colorTags[3]);
                    phrase = phrase.Replace("{DateTime}", expiry.ToString());
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                ExpireDate.TryGetValue(_cInfo.playerId, out DateTime expiry);
                if (!Expired(_cInfo, expiry))
                {
                    DateTime lastPrefixColorChange = DateTime.Now;
                    if (PersistentContainer.Instance.Players[_cInfo.playerId].LastPrefixColorChange != null)
                    {
                        lastPrefixColorChange = PersistentContainer.Instance.Players[_cInfo.playerId].LastPrefixColorChange;
                    }
                    TimeSpan varTime = DateTime.Now - lastPrefixColorChange;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int _timepassed = (int)fractionalMinutes;
                    if (_timepassed >= 5)
                    {
                        Phrases.Dict.TryGetValue("ChatColor7", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    if (_messageLowerCase.Length >= 8 && _messageLowerCase[0] == '[' && _messageLowerCase[7] == ']')
                    {
                        Players.TryGetValue(_cInfo.playerId, out string[] colorTags);
                        colorTags[3] = _messageLowerCase;
                        Players[_cInfo.playerId] = colorTags;
                        UpdateXml();
                        PersistentContainer.Instance.Players[_cInfo.playerId].LastPrefixColorChange = DateTime.Now;
                        PersistentContainer.DataChange = true;
                        Phrases.Dict.TryGetValue("ChatColor4", out string _phrase);
                        _phrase = _phrase.Replace("{Tags}", colorTags[3]);
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
                ExpireDate.TryGetValue(_cInfo.playerId, out DateTime expiry);
                if (!Expired(_cInfo, expiry))
                {
                    if (ColorList.Colors.Count > 0)
                    {
                        Players.TryGetValue(_cInfo.playerId, out string[] colorTags);
                        
                        KeyValuePair<string, string>[] Colors = ColorList.Colors.ToArray();
                        for (int i = 0; i < Colors.Length; i++)
                        {
                            if (Colors[i].Value == colorTags[3])
                            {
                                if (Colors.Length > i + 1)
                                {
                                    colorTags[3] = Colors[i + 1].Value;
                                    Players[_cInfo.playerId] = colorTags;
                                    UpdateXml();
                                    Phrases.Dict.TryGetValue("ChatColor4", out string phrase);
                                    phrase = phrase.Replace("{Tags}", colorTags[3]);
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    return;
                                }
                            }
                        }
                        colorTags[3] = Colors[0].Value;
                        Players[_cInfo.playerId] = colorTags;
                        UpdateXml();
                        Phrases.Dict.TryGetValue("ChatColor4", out string phrase1);
                        phrase1 = phrase1.Replace("{Tags}", colorTags[3]);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                ExpireDate.TryGetValue(_cInfo.playerId, out DateTime expiry);
                if (!Expired(_cInfo, expiry))
                {
                    DateTime lastNameColorChange = DateTime.Now;
                    if (PersistentContainer.Instance.Players[_cInfo.playerId].LastNameColorChange != null)
                    {
                        lastNameColorChange = PersistentContainer.Instance.Players[_cInfo.playerId].LastNameColorChange;
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
                        Players.TryGetValue(_cInfo.playerId, out string[] colorTags);
                        colorTags[1] = messageLowerCase;
                        Players[_cInfo.playerId] = colorTags;
                        UpdateXml();
                        PersistentContainer.Instance.Players[_cInfo.playerId].LastNameColorChange = DateTime.Now;
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
                ExpireDate.TryGetValue(_cInfo.playerId, out DateTime expiry);
                if (!Expired(_cInfo, expiry))
                {
                    if (ColorList.Colors.Count > 0)
                    {
                        Players.TryGetValue(_cInfo.playerId, out string[] colorTags);
                        Phrases.Dict.TryGetValue("ChatColor5", out string phrase);
                        KeyValuePair<string, string>[] Colors = ColorList.Colors.ToArray();
                        for (int i = 0; i < Colors.Length; i++)
                        {
                            if (Colors[i].Value == colorTags[1])
                            {
                                if (Colors.Length > i + 1)
                                {
                                    colorTags[1] = Colors[i + 1].Value;
                                    Players[_cInfo.playerId] = colorTags;
                                    UpdateXml();
                                    phrase = phrase.Replace("{Tags}", colorTags[1]);
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    return;
                                }
                            }
                        }
                        colorTags[1] = Colors[0].Value;
                        Players[_cInfo.playerId] = colorTags;
                        UpdateXml();
                        Phrases.Dict.TryGetValue("ChatColor5", out string phrase1);
                        phrase = phrase.Replace("{Tags}", colorTags[1]);
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

        private static void UpgradeXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<ChatColor>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("    <!-- NameColor and PrefixColor can come from the ColorList.xml -->");
                    sw.WriteLine("    <!-- <Player SteamId=\"12345678901234567\" Name=\"bob\" NameColor=\"[FF0000]\" Prefix=\"(Captain)\" PrefixColor=\"Red\" Expires=\"10/29/2050 7:30:00 AM\" /> -->");
                    for (int i = 0; i < OldNodeList.Count; i++)
                    {
                        if (OldNodeList[i].NodeType == XmlNodeType.Comment && !OldNodeList[i].OuterXml.Contains("<!-- NameColor and") &&
                            !OldNodeList[i].OuterXml.Contains("<!-- <Player SteamId=\"12345678901234567\"") && !OldNodeList[i].OuterXml.Contains("<!-- <Player SteamId=\"\""))
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
                            if (line.HasAttributes && line.Name == "Player")
                            {
                                string steamId = "", name = "", nameColor = "", prefix = "", prefixColor = "";
                                DateTime dateTime = DateTime.Now;
                                if (line.HasAttribute("SteamId"))
                                {
                                    steamId = line.GetAttribute("SteamId");
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
                                sw.WriteLine(string.Format("    <Player SteamId=\"{0}\" Name=\"{1}\" NameColor=\"{2}\" Prefix=\"{3}\" PrefixColor=\"{4}\" Expires=\"{5}\" />", steamId, name, nameColor, prefix, prefixColor, dateTime));
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
