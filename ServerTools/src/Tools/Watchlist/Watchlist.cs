using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    public class WatchList
    {
        public static bool IsEnabled = false, IsRunning = false;
        public static int Admin_Level = 0;
        public static string Delay = "5";

        public static SortedDictionary<string, string> Dict = new SortedDictionary<string, string>();

        private static string file = "WatchList.xml";
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
                if (childNodes != null && childNodes.Count > 0)
                {
                    Dict.Clear();
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        if (childNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement line = (XmlElement)childNodes[i];
                        if (line.HasAttributes)
                        {
                            if (line.HasAttribute("Version") && line.GetAttribute("Version") == Config.Version)
                            {
                                upgrade = false;
                                continue;
                            }
                            else if (line.HasAttribute("SteamId") && line.HasAttribute("Reason"))
                            {
                                string steamdId = line.GetAttribute("SteamId");
                                string reason = line.GetAttribute("Reason");
                                if (!Dict.ContainsKey(line.GetAttribute("SteamId")))
                                {
                                    Dict.Add(steamdId, reason);
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
                            Log.Out(string.Format("[SERVERTOOLS] The existing WatchList.xml was too old or misconfigured. File deleted and rebuilt for version {0}", Config.Version));
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
                    Log.Out(string.Format("[SERVERTOOLS] Error in WatchList.LoadXml: {0}", e.Message));
                }
            }
        }

        public static void UpdateXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<Watchlist>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("    <!-- Player Id=\"Steam_12345678909876543\" Reason=\"Suspected cheating\" / -->");
                    sw.WriteLine("    <!-- Player Id=\"EOS_1a3b5c7a9b1c3a5b7c9a1b3c5a7b9c1a3\" Reason=\"Cheaters R Assho\" / -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    if (Dict.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> kvp in Dict)
                        {
                            sw.WriteLine(string.Format("    <Player Id=\"{0}\" Reason=\"{1}\" />", kvp.Key, kvp.Value));
                        }
                    }
                    sw.WriteLine("</Watchlist>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WatchList.UpdateXml: {0}", e.Message));
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

        public static void SetDelay()
        {
            if (EventSchedule.watchList != Delay)
            {
                EventSchedule.watchList = Delay;
                if (Delay.Contains(",") && Delay.Contains(":"))
                {
                    string[] times = Delay.Split(',');
                    for (int i = 0; i < times.Length; i++)
                    {
                        if (DateTime.TryParse(DateTime.Today.ToString("d") + " " + times[i] + ":00", out DateTime time))
                        {
                            if (DateTime.Now < time)
                            {
                                EventSchedule.Add("WatchList", time);
                                return;
                            }
                        }
                    }
                    for (int i = 0; i < times.Length; i++)
                    {
                        if (DateTime.TryParse(DateTime.Today.AddDays(1).ToString("d") + " " + times[i] + ":00", out DateTime time))
                        {
                            if (DateTime.Now < time)
                            {
                                EventSchedule.Add("WatchList", time);
                                return;
                            }
                        }
                    }
                }
                else if (Delay.Contains(":"))
                {
                    if (DateTime.TryParse(DateTime.Today.ToString("d") + " " + Delay + ":00", out DateTime time))
                    {
                        if (DateTime.Now < time)
                        {
                            EventSchedule.Add("WatchList", time);
                        }
                        else if (DateTime.TryParse(DateTime.Today.AddDays(1).ToString("d") + " " + Delay + ":00", out DateTime secondaryTime))
                        {
                            EventSchedule.Add("WatchList", secondaryTime);
                        }
                    }
                }
                else
                {
                    if (int.TryParse(Delay, out int delay))
                    {
                        EventSchedule.Add("WatchList", DateTime.Now.AddMinutes(delay));
                    }
                    else
                    {
                        Log.Out("[SERVERTOOLS] Invalid Shutdown Time detected. Use a single integer, 24h time or multiple 24h time entries");
                        Log.Out("[SERVERTOOLS] Example: 120 or 03:00 or 03:00, 06:00, 09:00");
                    }
                }
            }
        }

        public static void Exec()
        {
            try
            {
                List<ClientInfo> clients = GeneralFunction.ClientList();
                if (clients != null && clients.Count > 0)
                {
                    List<ClientInfo> admin = GeneralFunction.ClientList();
                    List<ClientInfo> player = GeneralFunction.ClientList();
                    for (int i = 0; i < clients.Count; i++)
                    {
                        ClientInfo cInfo = clients[i];
                        if (cInfo != null)
                        {
                            if (GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.PlatformId) > Admin_Level &&
                                GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo.CrossplatformId) > Admin_Level)
                            {
                                if (Dict.ContainsKey(cInfo.PlatformId.ReadablePlatformUserIdentifier) ||
                                Dict.ContainsKey(cInfo.CrossplatformId.ReadablePlatformUserIdentifier))
                                {
                                    player.Add(cInfo);
                                }
                            }
                            else
                            {
                                admin.Add(cInfo);
                            }
                        }
                    }
                    if (admin.Count > 0 && player.Count > 0)
                    {
                        for (int i = 0; i < player.Count; i++)
                        {
                            Phrases.Dict.TryGetValue("Watchlist1", out string phrase);
                            if (Dict.TryGetValue(player[i].PlatformId.ReadablePlatformUserIdentifier, out string reason))
                            {
                                phrase = phrase.Replace("{PlayerName}", player[i].playerName);
                                phrase = phrase.Replace("{Reason}", reason);
                                for (int j = 0; j < admin.Count; j++)
                                {
                                    ChatHook.ChatMessage(admin[j], Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else if (Dict.TryGetValue(player[i].CrossplatformId.ReadablePlatformUserIdentifier, out reason))
                            {
                                phrase = phrase.Replace("{PlayerName}", player[i].playerName);
                                phrase = phrase.Replace("{Reason}", reason);
                                for (int j = 0; j < admin.Count; j++)
                                {
                                    ChatHook.ChatMessage(admin[j], Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WatchList.List: {0}", e.Message));
            }
        }

        private static void UpgradeXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                File.Delete(FilePath);
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<Watchlist>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("    <!-- Player Id=\"Steam_12345678909876543\" Reason=\"Suspected cheating.\" / -->");
                    sw.WriteLine("    <!-- Player Id=\"EOS_1a3b5c7a9b1c3a5b7c9a1b3c5a7b9c1a3\" Reason=\"Cheaters R Assho\" / -->");
                    for (int i = 0; i < OldNodeList.Count; i++)
                    {
                        if (OldNodeList[i].NodeType == XmlNodeType.Comment && !OldNodeList[i].OuterXml.Contains("<!-- Player Id=\"Steam_12345678909876543\"") &&
                            !OldNodeList[i].OuterXml.Contains("<!-- Player Id=\"EOS_1a3b5c7a9b1c3a5b7c9a1b3c5a7b9c1a3\""))
                        {
                            sw.WriteLine(OldNodeList[i].OuterXml);
                        }
                    }
                    sw.WriteLine();
                    sw.WriteLine();
                    for (int i = 0; i < OldNodeList.Count; i++)
                    {
                        if (OldNodeList[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement line = (XmlElement)OldNodeList[i];
                        if (line.HasAttributes && line.Name == "Player")
                        {
                            string id = "", reason = "";
                            if (line.HasAttribute("SteamId"))
                            {
                                id = line.GetAttribute("SteamId");
                            }
                            else if (line.HasAttribute("Id"))
                            {
                                id = line.GetAttribute("Id");
                                if (!id.Contains("Steam_"))
                                {
                                    id.Insert(0, "Steam_");
                                }
                            }
                            if (line.HasAttribute("Reason"))
                            {
                                reason = line.GetAttribute("Reason");
                            }
                            sw.WriteLine(string.Format("    <Player Id=\"{0}\" Reason=\"{1}\" />", id, reason));
                        }
                    }
                    sw.WriteLine("</Watchlist>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WatchList.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}