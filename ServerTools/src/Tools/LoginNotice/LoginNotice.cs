using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    class LoginNotice
    {
        public static bool IsEnabled = false, IsRunning = false;

        public static Dictionary<string, string[]> Dict1 = new Dictionary<string, string[]>();
        public static Dictionary<string, DateTime> Dict2 = new Dictionary<string, DateTime>();

        private const string file = "LoginNotice.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        public static void Unload()
        {
            Dict1.Clear();
            Dict2.Clear();
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
                Dict1.Clear();
                Dict2.Clear();
                if (childNodes != null && (childNodes[0] != null && childNodes[0].OuterXml.Contains("Version") && childNodes[0].OuterXml.Contains(Config.Version)))
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
                        if (line.HasAttribute("Id") && line.HasAttribute("Name") && line.HasAttribute("Message") && line.HasAttribute("Expiry"))
                        {
                            string id = line.GetAttribute("Id");
                            string[] nameMessage = new string[2];
                            nameMessage[0] = line.GetAttribute("Name");
                            nameMessage[1] = line.GetAttribute("Message");
                            if (id == "")
                            {
                                continue;
                            }
                            if (!DateTime.TryParse(line.GetAttribute("Expiry"), out DateTime expiry))
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Ignoring LoginNotice.xml entry because of invalid (Date-Time) value for 'Expiry' attribute: {0}", line.OuterXml));
                                continue;
                            }
                            if (!Dict1.ContainsKey(id))
                            {
                                Dict1.Add(id, nameMessage);
                                Dict2.Add(id, expiry);
                            }
                        }
                    }
                }
                else
                {
                    XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                    if (nodeList != null)
                    {
                        Timers.UpgradeLoginNoticeXml(nodeList);
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
                    Log.Out(string.Format("[SERVERTOOLS] Error in LoginNotice.LoadXml: {0}", e.Message));
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
                    sw.WriteLine("<LoginNotice>");
                    sw.WriteLine(string.Format("    <!-- <Version=\"{0}\" /> -->", Config.Version));
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- <Player Id=\"Steam_76561191234567891\" Name=\"Macaroni\" Message=\"Time to kick ass and chew bubble gum\" Expiry=\"2050-01-11 07:30:00\" /> -->");
                    if (Dict1.Count > 0)
                    {
                        foreach (KeyValuePair<string, string[]> kvp in Dict1)
                        {
                            if (Dict1.TryGetValue(kvp.Key, out string[] nameMessage))
                            {
                                if (Dict2.TryGetValue(kvp.Key, out DateTime expiry))
                                {
                                    sw.WriteLine(string.Format("    <Player Id=\"{0}\" Name=\"{1}\" Message=\"{2}\" Expiry=\"{3}\" />", kvp.Key, nameMessage[0], nameMessage[1], expiry));
                                }
                            }
                        }
                    }
                    sw.WriteLine("</LoginNotice>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in LoginNotice.UpdateXml: {0}", e.Message));
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

        public static void LoginStatus(ClientInfo _cInfo)
        {
            if (Dict2.ContainsKey(_cInfo.PlatformId.CombinedString))
            {
                Dict2.TryGetValue(_cInfo.PlatformId.CombinedString, out DateTime dt);
                if (DateTime.Now < dt)
                {
                    Phrases.Dict.TryGetValue("LoginNotice1", out string phrase);
                    phrase = phrase.Replace("{DateTime}", dt.ToString());
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else if (Dict2.ContainsKey(_cInfo.CrossplatformId.CombinedString))
            {
                Dict2.TryGetValue(_cInfo.CrossplatformId.CombinedString, out DateTime dt);
                if (DateTime.Now < dt)
                {
                    Phrases.Dict.TryGetValue("LoginNotice1", out string phrase);
                    phrase = phrase.Replace("{DateTime}", dt.ToString());
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void PlayerNotice(ClientInfo _cInfo)
        {
            if (Dict2.ContainsKey(_cInfo.PlatformId.CombinedString))
            {
                Dict2.TryGetValue(_cInfo.PlatformId.CombinedString, out DateTime dt);
                if (DateTime.Now < dt)
                {
                    if (Dict1.TryGetValue(_cInfo.PlatformId.CombinedString, out string[] nameMessage))
                    {
                        nameMessage[1] = nameMessage[1].Replace("{PlayerName}", _cInfo.playerName);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + nameMessage[1] + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    }
                }
            }
            else if (Dict2.ContainsKey(_cInfo.CrossplatformId.CombinedString))
            {
                Dict2.TryGetValue(_cInfo.CrossplatformId.CombinedString, out DateTime dt);
                if (DateTime.Now < dt)
                {
                    if (Dict1.TryGetValue(_cInfo.CrossplatformId.CombinedString, out string[] nameMessage))
                    {
                        nameMessage[1] = nameMessage[1].Replace("{PlayerName}", _cInfo.playerName);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + nameMessage[1] + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    }
                }
            }
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
                    sw.WriteLine("<LoginNotice>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- <Player Id=\"Steam_76561191234567891\" Name=\"Macaroni\" Message=\"Time to kick ass and chew bubble gum\" Expiry=\"2050-01-11 07:30:00\" /> -->");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType == XmlNodeType.Comment)
                        {
                            if (!nodeList[i].OuterXml.Contains("<!-- <Player Id=\"Steam_76561191234567891\"") && !nodeList[i].OuterXml.Contains("<!-- Do not forget") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Version") && !nodeList[i].OuterXml.Contains("<Player Id=\"\""))
                            {
                                sw.WriteLine(nodeList[i].OuterXml);
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
                                string id = "", name = "", message = "", expiry = "";
                                if (line.HasAttribute("Id"))
                                {
                                    id = line.GetAttribute("Id");
                                }
                                if (line.HasAttribute("Name"))
                                {
                                    name = line.GetAttribute("Name");
                                }
                                if (line.HasAttribute("Message"))
                                {
                                    message = line.GetAttribute("Message");
                                }
                                if (line.HasAttribute("Expiry"))
                                {
                                    expiry = line.GetAttribute("Expiry");
                                }
                                sw.WriteLine(string.Format("    <Player Id=\"{0}\" Name=\"{1}\" Message=\"{2}\" Expiry=\"{3}\" />", id, name, message, expiry));
                            }
                        }
                    }
                    sw.WriteLine("</LoginNotice>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in LoginNotice.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}
