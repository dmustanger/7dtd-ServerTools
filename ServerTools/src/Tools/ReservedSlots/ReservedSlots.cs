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

        private const string file = "ReservedSlots.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);

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
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                Dict.Clear();
                Dict1.Clear();
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
                        if (line.HasAttribute("Id") && line.HasAttribute("Name") && line.HasAttribute("Expires"))
                        {
                            string id = line.GetAttribute("Id");
                            string name = line.GetAttribute("Name");
                            if (id == "")
                            {
                                continue;
                            }
                            if (!DateTime.TryParse(line.GetAttribute("Expires"), out DateTime dt))
                            {
                                Log.Warning(string.Format("[SERVERTOOLS] Ignoring ReservedSlots.xml entry. Invalid (date) value for 'Expires' attribute: {0}", line.OuterXml));
                                continue;
                            }
                            if (!Dict.ContainsKey(id))
                            {
                                Dict.Add(id, dt);
                                Dict1.Add(id, name);
                            }
                        }
                    }
                }
                else
                {
                    XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                    if (nodeList != null)
                    {
                        Timers.UpgradeReservedSlotsXml(nodeList);
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
                sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                sw.WriteLine("    <!-- <Player Id=\"Steam_76561191234567891\" Name=\"Tron\" Expires=\"2050-10-29 10:30:00\" /> -->");
                sw.WriteLine("    <!-- <Player Id=\"EOS_0000a1b1c1dfe1feg1b1aaa1234aa123\" Name=\"Yoggi\" Expires=\"2050-01-11 07:30:00\" /> -->");
                if (Dict.Count > 0)
                {
                    foreach (KeyValuePair<string, DateTime> kvp in Dict)
                    {
                        Dict1.TryGetValue(kvp.Key, out string _name);
                        sw.WriteLine(string.Format("    <Player Id=\"{0}\" Name=\"{1}\" Expires=\"{2}\" />", kvp.Key, _name, kvp.Value.ToString()));
                    }
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

        public static bool IsReserved(ClientInfo _cInfo)
        {
            if (Dict.ContainsKey(_cInfo.PlatformId.CombinedString))
            {
                Dict.TryGetValue(_cInfo.PlatformId.CombinedString, out DateTime dt);
                if (DateTime.Now < dt)
                {
                    return true;
                }
            }
            if (Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
            {
                Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out DateTime dt);
                if (DateTime.Now < dt)
                {
                    return true;
                }
            }
            return false;
        }

        public static void ReservedStatus(ClientInfo _cInfo)
        {
            if (Dict.ContainsKey(_cInfo.PlatformId.CombinedString))
            {
                Dict.TryGetValue(_cInfo.PlatformId.CombinedString, out DateTime dt);
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
            if (Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
            {
                Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out DateTime dt);
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

        public static bool IsAdmin(ClientInfo _cInfo)
        {
            if (GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_cInfo.PlatformId) <= Admin_Level || 
                GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_cInfo.CrossplatformId) <= Admin_Level)
            {
                return true;
            }
            return false;
        }

        public static bool FullServer(ClientInfo _cInfo)
        {
            try
            {
                if (_cInfo == null)
                {
                    return true;
                }
                List<ClientInfo> clientList = GeneralOperations.ClientList();
                if (clientList == null || clientList.Count == 0)
                {
                    return true;
                }
                if (IsAdmin(_cInfo))
                {
                    List<ClientInfo> reserved = new List<ClientInfo>();
                    List<ClientInfo> normal = new List<ClientInfo>();
                    for (int i = 0; i < clientList.Count; i++)
                    {
                        ClientInfo cInfo2 = clientList[i];
                        if (cInfo2 == null || !cInfo2.loginDone || cInfo2.PlatformId == null || cInfo2.CrossplatformId == null || cInfo2.entityId == _cInfo.entityId)
                        {
                            continue;
                        }
                        if (!IsAdmin(cInfo2))
                        {
                            if (IsReserved(cInfo2))
                            {
                                reserved.Add(cInfo2);
                            }
                            else
                            {
                                normal.Add(cInfo2);
                            }
                        }
                    }
                    if (normal.Count > 0)
                    {
                        normal.RandomizeList();
                        Phrases.Dict.TryGetValue("Reserved1", out string phrase1);
                        phrase1 = phrase1.Replace("{ServerResponseName}", Config.Server_Response_Name);
                        GeneralOperations.KickPlayer(normal[0], phrase1);
                        return true;
                    }
                    else if (reserved.Count > 0)
                    {
                        reserved.RandomizeList();
                        Phrases.Dict.TryGetValue("Reserved1", out string phrase2);
                        phrase2 = phrase2.Replace("{ServerResponseName}", Config.Server_Response_Name);
                        GeneralOperations.KickPlayer(normal[0], phrase2);
                        return true;
                    }
                }
                else if (IsReserved(_cInfo))
                {
                    List<ClientInfo> normal = new List<ClientInfo>();
                    for (int i = 0; i < clientList.Count; i++)
                    {
                        ClientInfo cInfo2 = clientList[i];
                        if (cInfo2 == null || !cInfo2.loginDone || cInfo2.PlatformId == null || cInfo2.CrossplatformId == null || cInfo2.entityId == _cInfo.entityId)
                        {
                            continue;
                        }
                        if (!IsAdmin(cInfo2) && !IsReserved(cInfo2))
                        {
                            normal.Add(cInfo2);
                        }
                    }
                    if (normal.Count > 0)
                    {
                        normal.RandomizeList();
                        Phrases.Dict.TryGetValue("Reserved1", out string phrase);
                        phrase = phrase.Replace("{ServerResponseName}", Config.Server_Response_Name);
                        GeneralOperations.KickPlayer(normal[0], phrase);
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

        public static void UpgradeXml(XmlNodeList nodeList)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                File.Delete(FilePath);
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<ReservedSlots>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- <Player Id=\"Steam_76561191234567891\" Name=\"Tron\" Expires=\"2050-10-29 10:30:00\" /> -->");
                    sw.WriteLine("    <!-- <Player Id=\"EOS_0000a1b1c1dfe1feg1b1aaa1234aa123\" Name=\"Yoggi\" Expires=\"2050-01-11 07:30:00\" /> -->");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType == XmlNodeType.Comment)
                        {
                            if (!nodeList[i].OuterXml.Contains("<!-- <Version") && !nodeList[i].OuterXml.Contains("<!-- Do not forget") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Player Id=\"Steam_76561191234567891") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Player Id=\"EOS_0000a1b1c1dfe1feg1b1aaa1234aa123") &&
                            !nodeList[i].OuterXml.Contains("<Player Id=\"\""))
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