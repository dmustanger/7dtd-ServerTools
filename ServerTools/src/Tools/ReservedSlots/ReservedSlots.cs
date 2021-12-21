using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    public class ReservedSlots
    {
        public static bool IsEnabled = false, IsRunning = false, Operating = false, Reduced_Delay = false, Admin_Slot = false, Bonus_Exp = false;
        public static int Session_Time = 30, Admin_Level = 0;
        public static string Command_reserved = "reserved";

        public static Dictionary<string, DateTime> Dict = new Dictionary<string, DateTime>();
        public static Dictionary<string, string> Dict1 = new Dictionary<string, string>();
        public static Dictionary<string, DateTime> Kicked = new Dictionary<string, DateTime>();

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
                sw.WriteLine("    <!-- <Player Id=\"Steam_76561191234567891\" Name=\"Tron\" Expires=\"10/29/2050 7:30:00 AM\" /> -->");
                sw.WriteLine("    <!-- <Player Id=\"EOS_0000a1b1c1dfe1feg1b1aaa1234aa123\" Name=\"Yoggi\" Expires=\"01/11/2150 7:30:00 AM\" /> -->");
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

        public static bool ReservedCheck(ClientInfo _cInfo)
        {
            if (Dict.ContainsKey(_cInfo.PlatformId.CombinedString))
            {
                Dict.TryGetValue(_cInfo.PlatformId.CombinedString, out DateTime dt);
                if (DateTime.Now < dt)
                {
                    return true;
                }
            }
            else if (Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
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
            if (Dict.ContainsKey(_cInfo.PlatformId.CombinedString) || Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
            {
                if (Dict.TryGetValue(_cInfo.PlatformId.CombinedString, out DateTime dt))
                {
                    if (DateTime.Now < dt)
                    {
                        Phrases.Dict.TryGetValue("Reserved4", out string phrase4);
                        phrase4 = phrase4.Replace("{DateTime}", dt.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase4 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Reserved5", out string phrase5);
                        phrase5 = phrase5.Replace("{DateTime}", dt.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase5 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else if (Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out dt))
                {
                    if (DateTime.Now < dt)
                    {
                        Phrases.Dict.TryGetValue("Reserved4", out string phrase4);
                        phrase4 = phrase4.Replace("{DateTime}", dt.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase4 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Reserved5", out string phrase5);
                        phrase5 = phrase5.Replace("{DateTime}", dt.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase5 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            else
            {
                Phrases.Dict.TryGetValue("Reserved6", out string phrase6);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase6 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static bool AdminCheck(ClientInfo _cInfo)
        {
            if (GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo.PlatformId) <= Admin_Level || GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo.CrossplatformId) <= Admin_Level)
            {
                return true;
            }
            return false;
        }

        public static bool FullServer(ClientInfo _cInfo)
        {
            try
            {
                List<string> reservedKicks = new List<string>();
                List<string> normalKicks = new List<string>();
                string clientToKick = null;
                List<ClientInfo> clientList = PersistentOperations.ClientList();
                if (clientList != null)
                {
                    if (AdminCheck(_cInfo))//admin is joining
                    {
                        for (int i = 0; i < clientList.Count; i++)
                        {
                            ClientInfo cInfo2 = clientList[i];
                            if (cInfo2 != null && cInfo2.CrossplatformId != null && cInfo2.entityId != _cInfo.entityId)
                            {
                                if (!AdminCheck(cInfo2))//not admin
                                {
                                    if (ReservedCheck(cInfo2))//reserved player
                                    {
                                        reservedKicks.Add(cInfo2.CrossplatformId.CombinedString);
                                    }
                                    else
                                    {
                                        normalKicks.Add(cInfo2.CrossplatformId.CombinedString);
                                    }
                                }
                            }
                        }
                    }
                    else if (ReservedCheck(_cInfo))//reserved player is joining
                    {
                        for (int i = 0; i < clientList.Count; i++)
                        {
                            ClientInfo cInfo2 = clientList[i];
                            if (cInfo2 != null && cInfo2.CrossplatformId != null && cInfo2.entityId != _cInfo.entityId)
                            {
                                if (!AdminCheck(cInfo2) && !ReservedCheck(cInfo2))
                                {
                                    normalKicks.Add(cInfo2.CrossplatformId.CombinedString);
                                }
                            }
                        }
                    }
                    else//regular player is joining
                    {
                        for (int i = 0; i < clientList.Count; i++)
                        {
                            ClientInfo cInfo2 = clientList[i];
                            if (cInfo2 != null && cInfo2.CrossplatformId != null && cInfo2.entityId != _cInfo.entityId)
                            {
                                if (!AdminCheck(cInfo2) && !ReservedCheck(cInfo2))
                                {
                                    if (Session_Time > 0)
                                    {
                                        if (PersistentOperations.Session.TryGetValue(cInfo2.CrossplatformId.CombinedString, out DateTime dateTime))
                                        {
                                            TimeSpan varTime = DateTime.Now - dateTime;
                                            double fractionalMinutes = varTime.TotalMinutes;
                                            int timepassed = (int)fractionalMinutes;
                                            if (timepassed >= Session_Time)
                                            {
                                                normalKicks.Add(cInfo2.CrossplatformId.CombinedString);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (normalKicks.Count > 0)
                    {
                        normalKicks.RandomizeList();
                        clientToKick = normalKicks[0];
                        if (Session_Time > 0)
                        {
                            Kicked.Add(clientToKick, DateTime.Now);
                        }
                        Phrases.Dict.TryGetValue("Reserved1", out string phrase1);
                        SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", clientToKick, phrase1), null);
                        return true;
                    }
                    else if (reservedKicks.Count > 0)
                    {
                        reservedKicks.RandomizeList();
                        clientToKick = reservedKicks[0];
                        Phrases.Dict.TryGetValue("Reserved1", out string phrase1);
                        SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", clientToKick, phrase1), null);
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
                    sw.WriteLine("    <!-- <Player Id=\"Steam_76561191234567891\" Name=\"Tron\" Expires=\"10/29/2050 7:30:00 AM\" /> -->");
                    sw.WriteLine("    <!-- <Player Id=\"EOS_0000a1b1c1dfe1feg1b1aaa1234aa123\" Name=\"Yoggi\" Expires=\"01/11/2150 7:30:00 AM\" /> -->");
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