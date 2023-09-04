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
        public static List<string> MsgList = new List<string>();

        private static string EventDelay = "";
        private static DateTime time = new DateTime();
        private static Dictionary<string, string> Dict = new Dictionary<string, string>();

        private const string file = "InfoTicker.xml";
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
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                Dict.Clear();
                MsgList.Clear();
                if (childNodes != null && (childNodes[0] != null && childNodes[0].OuterXml.Contains("Version") && childNodes[0].OuterXml.Contains(Config.Version)))
                {
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        if (childNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement line = (XmlElement)childNodes[i];
                        if (!line.HasAttributes || !line.HasAttribute("Message"))
                        {
                            continue;
                        }
                        string message = line.GetAttribute("Message");
                        if (message != "" && !Dict.ContainsKey(message))
                        {
                            Dict.Add(message, null);
                        }
                    }
                    if (Dict.Count > 0)
                    {
                        BuildList();
                    }
                }
                else
                {
                    XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                    if (nodeList != null)
                    {
                        Timers.UpgradeInfoTickerXml(nodeList);
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
                    Log.Out("[SERVERTOOLS] Error in InfoTicker.LoadXml: {0}", e.Message);
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
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- Possible variables {EntityId}, {Id}, {EOS}, {PlayerName} -->");
                    sw.WriteLine("    <!-- <Ticker Message=\"Have a suggestion or complaint? Post on our forums or discord and let us know\" /> -->");
                    sw.WriteLine("    <!-- <Ticker Message=\"Type /commands for a list of the chat commands\" /> -->");
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
                Log.Out("[SERVERTOOLS] Error in InfoTicker.UpdateXml: {0}", e.Message);
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

        public static void SetDelay(bool _loading)
        {
            if (EventDelay != Delay || _loading)
            {
                EventSchedule.Expired.Add("InfoTicker");
                EventDelay = Delay;
                if (Delay.Contains(",") && Delay.Contains(":"))
                {
                    string[] times = Delay.Split(',');
                    for (int i = 0; i < times.Length; i++)
                    {
                        string[] timeSplit1 = times[i].Split(':');
                        int.TryParse(timeSplit1[0], out int hours1);
                        int.TryParse(timeSplit1[1], out int minutes1);
                        time = DateTime.Today.AddHours(hours1).AddMinutes(minutes1);
                        if (DateTime.Now < time)
                        {
                            EventSchedule.AddToSchedule("InfoTicker", time);
                            return;
                        }
                    }
                    string[] timeSplit2 = times[0].Split(':');
                    int.TryParse(timeSplit2[0], out int hours2);
                    int.TryParse(timeSplit2[1], out int minutes2);
                    time = DateTime.Today.AddDays(1).AddHours(hours2).AddMinutes(minutes2);
                    EventSchedule.AddToSchedule("InfoTicker", time);
                    return;
                }
                else if (Delay.Contains(":"))
                {
                    string[] timeSplit2 = Delay.Split(':');
                    int.TryParse(timeSplit2[0], out int hours2);
                    int.TryParse(timeSplit2[1], out int minutes2);
                    time = DateTime.Today.AddHours(hours2).AddMinutes(minutes2);
                    if (DateTime.Now < time)
                    {
                        EventSchedule.AddToSchedule("InfoTicker", time);
                    }
                    else
                    {
                        time = DateTime.Today.AddDays(1).AddHours(hours2).AddMinutes(minutes2);
                        EventSchedule.AddToSchedule("InfoTicker", time);
                    }
                    return;
                }
                else
                {
                    if (int.TryParse(Delay, out int delay))
                    {
                        time = DateTime.Now.AddMinutes(delay);
                        EventSchedule.AddToSchedule("InfoTicker", time);
                    }
                    else
                    {
                        Log.Out("[SERVERTOOLS] Invalid Info_Ticker Delay detected. Use a single integer, 24h time or multiple 24h time entries");
                        Log.Out("[SERVERTOOLS] Example: 120 or 03:00 or 03:00, 06:00, 09:00");
                    }
                    return;
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
                        ClientInfo cInfo;
                        List<ClientInfo> clientList = GeneralOperations.ClientList();
                        if (clientList != null)
                        {
                            for (int i = 0; i < clientList.Count; i++)
                            {
                                cInfo = clientList[i];
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
                        }
                    }
                    else
                    {
                        string message = MsgList[0];
                        ClientInfo cInfo;
                        List<ClientInfo> clientList = GeneralOperations.ClientList();
                        if (clientList != null)
                        {
                            for (int i = 0; i < clientList.Count; i++)
                            {
                                cInfo = clientList[i];
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
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in InfoTicker.Exec: {0}", e.Message);
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
                    sw.WriteLine("<InfoTicker>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- Possible variables {EntityId}, {Id}, {EOS}, {PlayerName} -->");
                    sw.WriteLine("    <!-- <Ticker Message=\"Have a suggestion or complaint? Post on our forums or discord and let us know\" /> -->");
                    sw.WriteLine("    <!-- <Ticker Message=\"Type /commands for a list of the chat commands\" /> -->");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType == XmlNodeType.Comment)
                        {
                            if (!nodeList[i].OuterXml.Contains("<!-- Possible variables") && !nodeList[i].OuterXml.Contains("<!-- <Ticker Message=\"Have a suggestion") &&
                            !nodeList[i].OuterXml.Contains("<Ticker Message=\"\"") && !nodeList[i].OuterXml.Contains("<!-- <Ticker Message=\"Type /commands") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Version") && !nodeList[i].OuterXml.Contains("<!-- Do not forget"))
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
                Log.Out("[SERVERTOOLS] Error in InfoTicker.UpgradeXml: {0}", e.Message);
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}