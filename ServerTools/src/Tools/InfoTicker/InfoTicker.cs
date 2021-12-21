using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    public class InfoTicker
    {
        public static bool IsEnabled = false, IsRunning = false, Random = false;
        public static string Command_infoticker = "infoticker", Delay = "60";

        public static List<string> ExemptionList = new List<string>();

        private static Dictionary<string, string> Dict = new Dictionary<string, string>();
        private static List<string> MsgList = new List<string>();

        private const string file = "InfoTicker.xml";
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
            MsgList.Clear();
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
                    MsgList.Clear();
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
                                else if (line.HasAttribute("Message"))
                                {
                                    string _message = line.GetAttribute("Message");
                                    if (!Dict.ContainsKey(_message))
                                    {
                                        Dict.Add(_message, null);
                                    }
                                }
                            }
                        }
                    }
                    if (Dict.Count > 0)
                    {
                        BuildList();
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
                            Log.Out(string.Format("[SERVERTOOLS] The existing CommandList.xml was too old or misconfigured. File deleted and rebuilt for version {0}", Config.Version));
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
                    Log.Out(string.Format("[SERVERTOOLS] Error in InfoTicker.LoadXml: {0}", e.Message));
                }
            }
        }

        public static void BuildList()
        {
            MsgList = new List<string>(Dict.Keys);
        }

        private static void UpdateXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<InfoTicker>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("    <!-- Possible variables {EntityId}, {Id}, {EOS}, {PlayerName} -->");
                    sw.WriteLine("    <!-- <Ticker Message=\"Have a suggestion or complaint? Post on our forums or discord and let us know\" /> -->");
                    sw.WriteLine("    <!-- <Ticker Message=\"Type /commands for a list of the chat commands\" /> -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    if (Dict.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> kvp in Dict)
                        {
                            sw.WriteLine(string.Format("    <Ticker Message=\"{0}\" />", kvp.Key));
                        }
                    }
                    sw.WriteLine("</InfoTicker>");
                    sw.Flush();
                    sw.Close();
                }
                FileWatcher.EnableRaisingEvents = true;
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in InfoTicker.UpdateXml: {0}", e.Message));
            }
        }

        private static void InitFileWatcher()
        {
            FileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.Created += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.EnableRaisingEvents = true;
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
            if (EventSchedule.infoTicker != Delay)
            {
                EventSchedule.infoTicker = Delay;
                if (Delay.Contains(",") && Delay.Contains(":"))
                {
                    string[] times = Delay.Split(',');
                    for (int i = 0; i < times.Length; i++)
                    {
                        if (DateTime.TryParse(DateTime.Today.ToString("d") + " " + times[i] + ":00", out DateTime time))
                        {
                            if (DateTime.Now < time)
                            {
                                EventSchedule.Add("InfoTicker", time);
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
                                EventSchedule.Add("InfoTicker", time);
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
                            EventSchedule.Add("InfoTicker", time);
                        }
                        else if (DateTime.TryParse(DateTime.Today.AddDays(1).ToString("d") + " " + Delay + ":00", out DateTime secondaryTime))
                        {
                            EventSchedule.Add("InfoTicker", secondaryTime);
                        }
                    }
                }
                else
                {
                    if (int.TryParse(Delay, out int delay))
                    {
                        EventSchedule.Add("InfoTicker", DateTime.Now.AddMinutes(delay));
                    }
                    else
                    {
                        Log.Out("[SERVERTOOLS] Invalid Info_Ticker Delay detected. Use a single integer, 24h time or multiple 24h time entries");
                        Log.Out("[SERVERTOOLS] Example: 120 or 03:00 or 03:00, 06:00, 09:00");
                    }
                }
            }
        }

        public static void Exec()
        {
            try
            {
                if (MsgList.Count > 0 && ConnectionManager.Instance.ClientCount() > 0)
                {
                    if (Random)
                    {
                        MsgList.RandomizeList();
                        string message = MsgList[0];
                        List<ClientInfo> clientList = PersistentOperations.ClientList();
                        if (clientList != null)
                        {
                            for (int i = 0; i < clientList.Count; i++)
                            {
                                ClientInfo cInfo = clientList[i];
                                if (cInfo != null)
                                {
                                    if (!ExemptionList.Contains(cInfo.CrossplatformId.CombinedString))
                                    {
                                        message = message.Replace("{EntityId}", cInfo.entityId.ToString());
                                        message = message.Replace("{Id}", cInfo.PlatformId.CombinedString);
                                        message = message.Replace("{EOS}", cInfo.CrossplatformId.CombinedString);
                                        message = message.Replace("{PlayerName}", cInfo.playerName);
                                        ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + message + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                            }
                            MsgList.RemoveAt(0);
                            if (MsgList.Count == 0)
                            {
                                BuildList();
                            }
                        }
                    }
                    else
                    {
                        string message = MsgList[0];
                        List<ClientInfo> clientList = PersistentOperations.ClientList();
                        if (clientList != null)
                        {
                            for (int i = 0; i < clientList.Count; i++)
                            {
                                ClientInfo cInfo = clientList[i];
                                if (cInfo != null)
                                {
                                    if (!ExemptionList.Contains(cInfo.CrossplatformId.CombinedString))
                                    {
                                        message = message.Replace("{EntityId}", cInfo.entityId.ToString());
                                        message = message.Replace("{Id}", cInfo.PlatformId.CombinedString);
                                        message = message.Replace("{EOS}", cInfo.CrossplatformId.CombinedString);
                                        message = message.Replace("{PlayerName}", cInfo.playerName);
                                        ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + message + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                            }
                            MsgList.RemoveAt(0);
                            if (MsgList.Count == 0)
                            {
                                BuildList();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in InfoTicker.Exec: {0}", e.Message));
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
                    sw.WriteLine("<InfoTicker>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("    <!-- Possible variables {EntityId}, {Id}, {EOS}, {PlayerName} -->");
                    sw.WriteLine("    <!-- <Ticker Message=\"Have a suggestion or complaint? Post on our forums or discord and let us know\" /> -->");
                    sw.WriteLine("    <!-- <Ticker Message=\"Type /commands for a list of the chat commands\" /> -->");
                    for (int i = 0; i < OldNodeList.Count; i++)
                    {
                        if (OldNodeList[i].NodeType == XmlNodeType.Comment && !OldNodeList[i].OuterXml.Contains("<!-- Possible variables") &&
                            !OldNodeList[i].OuterXml.Contains("<!-- <Ticker Message=\"Have a suggestion") &&
                            !OldNodeList[i].OuterXml.Contains("<!-- <Ticker Message=\"Type /gimme") &&
                            !OldNodeList[i].OuterXml.Contains("<!-- <Ticker Message=\"Type /commands") && 
                            !OldNodeList[i].OuterXml.Contains("<!-- <Ticker Message=\"\""))
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
                            if (line.HasAttributes && line.Name == "Ticker")
                            {
                                string message = "";
                                if (line.HasAttribute("Message"))
                                {
                                    message = line.GetAttribute("Message");
                                }
                                sw.WriteLine(string.Format("    <Ticker Message=\"{0}\" />", message));
                            }
                        }
                    }
                    sw.WriteLine("</InfoTicker>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in InfoTicker.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}