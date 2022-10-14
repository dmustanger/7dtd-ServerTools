using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    public class ReservedSlots
    {
        public static bool IsEnabled = false, IsRunning = false, Reduced_Delay = false;
        public static int Session_Time = 30, Admin_Level = 0, Bonus_Exp = 0;

        public static Dictionary<string, DateTime> Dict = new Dictionary<string, DateTime>();
        public static Dictionary<string, string> Dict1 = new Dictionary<string, string>();
        public static Dictionary<ClientInfo, DateTime> Kicked = new Dictionary<ClientInfo, DateTime>();

        private static string file = "ReservedSlots.xml";
        private static string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
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
            Dict1.Clear();
            FileWatcher.Dispose();
            IsRunning = false;
        }

        private static void LoadXml()
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
                    Dict1.Clear();
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
                                else if (line.HasAttribute("Id") && line.HasAttribute("Name") && line.HasAttribute("Expires"))
                                {
                                    if (!DateTime.TryParse(line.GetAttribute("Expires"), out DateTime dt))
                                    {
                                        Log.Warning(string.Format("[SERVERTOOLS] Ignoring ReservedSlots.xml entry. Invalid (date) value for 'Expires' attribute: {0}", line.OuterXml));
                                        continue;
                                    }
                                    if (!Dict.ContainsKey(line.GetAttribute("Id")))
                                    {
                                        Dict.Add(line.GetAttribute("Id"), dt);
                                    }
                                    if (!Dict1.ContainsKey(line.GetAttribute("Id")))
                                    {
                                        Dict1.Add(line.GetAttribute("Id"), line.GetAttribute("Name"));
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
                            Log.Out(string.Format("[SERVERTOOLS] The existing ReservedSlots.xml was too old or misconfigured. File deleted and rebuilt for version {0}", Config.Version));
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
                    Log.Out(string.Format("[SERVERTOOLS] Error in ReservedSlots.LoadXml: {0}", e.Message));
                }
            }
        }

        public static void UpdateXml()
        {
            FileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<ReservedSlots>");
                sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                sw.WriteLine("    <!-- <Player Id=\"Steam_76561191234567891\" Name=\"Tron\" Expires=\"2050-10-29 10:30:00\" /> -->");
                sw.WriteLine("    <!-- <Player Id=\"EOS_0000a1b1c1dfe1feg1b1aaa1234aa123\" Name=\"Yoggi\" Expires=\"2050-01-11 07:30:00\" /> -->");
                sw.WriteLine();
                sw.WriteLine();
                if (Dict.Count > 0)
                {
                    foreach (KeyValuePair<string, DateTime> kvp in Dict)
                    {
                        Dict1.TryGetValue(kvp.Key, out string _name);
                        sw.WriteLine(string.Format("    <Player Id=\"{0}\" Name=\"{1}\" Expires=\"{2}\" />", kvp.Key, _name, kvp.Value.ToString()));
                    }
                }
                else
                {
                    sw.WriteLine(string.Format("    <!-- <Player Id=\"\" Name=\"\" Expires=\"\" /> -->"));
                }
                sw.WriteLine("</ReservedSlots>");
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

        public static bool ReservedCheck(PlatformUserIdentifierAbs _platformId, PlatformUserIdentifierAbs _crossplatformId)
        {
            if (Dict.ContainsKey(_platformId.CombinedString))
            {
                Dict.TryGetValue(_platformId.CombinedString, out DateTime dt);
                if (DateTime.Now < dt)
                {
                    return true;
                }
            }
            if (Dict.ContainsKey(_crossplatformId.CombinedString))
            {
                Dict.TryGetValue(_crossplatformId.CombinedString, out DateTime dt);
                if (DateTime.Now < dt)
                {
                    return true;
                }
            }
            return false;
        }

        public static void ReservedStatus(ClientInfo _cInfo, PlatformUserIdentifierAbs _platformId, PlatformUserIdentifierAbs _crossplatformId)
        {
            if (Dict.ContainsKey(_platformId.CombinedString))
            {
                Dict.TryGetValue(_platformId.CombinedString, out DateTime dt);
                if (DateTime.Now < dt)
                {
                    Phrases.Dict.TryGetValue("Reserved4", out string phrase);
                    phrase = phrase.Replace("{DateTime}", dt.ToString());
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    Phrases.Dict.TryGetValue("Reserved5", out string phrase);
                    phrase = phrase.Replace("{DateTime}", dt.ToString());
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                return;
            }
            if (Dict.ContainsKey(_crossplatformId.CombinedString))
            {
                Dict.TryGetValue(_crossplatformId.CombinedString, out DateTime dt);
                if (DateTime.Now < dt)
                {
                    Phrases.Dict.TryGetValue("Reserved4", out string phrase);
                    phrase = phrase.Replace("{DateTime}", dt.ToString());
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    Phrases.Dict.TryGetValue("Reserved5", out string phrase);
                    phrase = phrase.Replace("{DateTime}", dt.ToString());
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                return;
            }
            Phrases.Dict.TryGetValue("Reserved6", out string phrase1);
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
        }

        public static bool AdminCheck(ClientInfo _cInfo, PlatformUserIdentifierAbs _platformId, PlatformUserIdentifierAbs _crossplatformId)
        {
            if (GameManager.Instance.adminTools.GetUserPermissionLevel(_platformId) <= Admin_Level || 
                GameManager.Instance.adminTools.GetUserPermissionLevel(_crossplatformId) <= Admin_Level)
            {
                return true;
            }
            return false;
        }

        public static bool FullServer(ClientInfo _cInfo, PlatformUserIdentifierAbs _platformId, PlatformUserIdentifierAbs _crossplatformId)
        {
            try
            {
                List<ClientInfo> reserved = new List<ClientInfo>();
                List<ClientInfo> normal = new List<ClientInfo>();
                List<ClientInfo> clientList = GeneralFunction.ClientList();
                if (clientList != null)
                {
                    if (AdminCheck(_cInfo, _platformId, _crossplatformId))//admin is joining
                    {
                        for (int i = 0; i < clientList.Count; i++)
                        {
                            ClientInfo cInfo2 = clientList[i];
                            if (cInfo2 != null && cInfo2.PlatformId != null && cInfo2.CrossplatformId != null && cInfo2.entityId != _cInfo.entityId)
                            {
                                if (!AdminCheck(cInfo2, cInfo2.PlatformId, cInfo2.CrossplatformId))//not admin
                                {
                                    if (ReservedCheck(cInfo2.PlatformId, cInfo2.CrossplatformId))//reserved player
                                    {
                                        reserved.Add(cInfo2);
                                    }
                                    else
                                    {
                                        normal.Add(cInfo2);
                                    }
                                }
                            }
                        }
                    }
                    else if (ReservedCheck(_platformId, _crossplatformId))//reserved player is joining
                    {
                        for (int i = 0; i < clientList.Count; i++)
                        {
                            ClientInfo cInfo2 = clientList[i];
                            if (cInfo2 != null && cInfo2.PlatformId != null && cInfo2.CrossplatformId != null && cInfo2.entityId != _cInfo.entityId)
                            {
                                if (!AdminCheck(cInfo2, cInfo2.PlatformId, cInfo2.CrossplatformId) && !ReservedCheck(cInfo2.PlatformId, cInfo2.CrossplatformId))
                                {
                                    normal.Add(cInfo2);
                                }
                            }
                        }
                    }
                    if (normal.Count > 0)
                    {
                        if (normal.Count > 1)
                        {
                            normal.RandomizeList();
                        }
                        Phrases.Dict.TryGetValue("Reserved1", out string phrase);
                        phrase = phrase.Replace("{ServerResponseName}", Config.Server_Response_Name);
                        SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", normal[0].CrossplatformId.CombinedString, phrase), null);
                        return true;
                    }
                    else if (reserved.Count > 0)
                    {
                        if (reserved.Count > 1)
                        {
                            reserved.RandomizeList();
                        }
                        Phrases.Dict.TryGetValue("Reserved1", out string phrase);
                        phrase = phrase.Replace("{ServerResponseName}", Config.Server_Response_Name);
                        SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", reserved[0].CrossplatformId.CombinedString, phrase), null);
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ReservedSlots.FullServer: {0}", e.Message));
            }
            return false;
        }

        private static void UpgradeXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<ReservedSlots>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("    <!-- <Player Id=\"Steam_76561191234567891\" Name=\"Tron\" Expires=\"2050-10-29 10:30:00\" /> -->");
                    sw.WriteLine("    <!-- <Player Id=\"EOS_0000a1b1c1dfe1feg1b1aaa1234aa123\" Name=\"Yoggi\" Expires=\"2050-01-11 07:30:00\" /> -->");
                    for (int i = 0; i < OldNodeList.Count; i++)
                    {
                        if (OldNodeList[i].NodeType == XmlNodeType.Comment && !OldNodeList[i].OuterXml.Contains("<!-- <Player Id=\"\"") &&
                            !OldNodeList[i].OuterXml.Contains("<!-- <Player Id=\"Steam_76561191234567891\"") && 
                            !OldNodeList[i].OuterXml.Contains("<!-- <Player Id=\"EOS_0000a1b1c1dfe1feg1b1aaa1234aa123\""))
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
                                string id = "", name = "", expires = "";
                                if (line.HasAttribute("SteamId"))
                                {
                                    id = line.GetAttribute("SteamId");
                                    if (!id.Contains("Steam_"))
                                    {
                                        id.Insert(0, "Steam_");
                                    }
                                }
                                else if (line.HasAttribute("Id"))
                                {
                                    id = line.GetAttribute("Id");
                                }
                                if (line.HasAttribute("Name"))
                                {
                                    name = line.GetAttribute("Name");
                                }
                                if (line.HasAttribute("Expires"))
                                {
                                    expires = line.GetAttribute("Expires");
                                }
                                sw.WriteLine(string.Format("    <Player Id=\"{0}\" Name=\"{1}\" Expires=\"{2}\" />", id, name, expires));
                            }
                        }
                    }
                    sw.WriteLine("</ReservedSlots>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ReservedSlots.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}