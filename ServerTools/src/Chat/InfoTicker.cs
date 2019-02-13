using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace ServerTools
{
    public class InfoTicker
    {
        public static bool IsEnabled = false, IsRunning = false, Random = false;
        public static string Command104 = "infoticker";
        private const string file = "InfoTicker.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static Dictionary<string, string> dict = new Dictionary<string, string>();
        private static List<string> msgList = new List<string>();
        public static List<string> exemptionList = new List<string>();
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);

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
                dict.Clear();
                msgList.Clear();
                fileWatcher.Dispose();
                IsRunning = false;
            }
        }

        private static void LoadXml()
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
                if (childNode.Name == "Messages")
                {
                    dict.Clear();
                    msgList.Clear();
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Messages' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("Message"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Message entry because of missing a Message attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string _message = _line.GetAttribute("Message");
                        if (!dict.ContainsKey(_message))
                        {
                            dict.Add(_message, null);
                        }
                        else
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Message entry because this message already exists: {0}", subChild.OuterXml));
                            continue;
                        }
                    }
                }
            }
            if (dict.Count == 0)
            {
                Log.Warning("------------------------------------------------------------------------------------");
                Log.Warning("[SERVERTOOLS] Ignoring infoticker because no messages from your file could be added.");
                Log.Warning("------------------------------------------------------------------------------------");
            }
            else
            {
                BuildList();
            }
        }

        public static void BuildList()
        {
            if (dict.Count > 0)
            {
                msgList = new List<string>(dict.Keys);
            }
            else
            {
                Log.Warning("[SERVERTOOLS] Ignoring infoticker because no messages from your file could be added.");
            }
        }

        private static void UpdateXml()
        {
            fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<InfoTicketer>");
                sw.WriteLine("    <Messages>");
                sw.WriteLine("        <!-- possible variables {EntityId} {SteamId} {PlayerName}-->");
                if (dict.Count > 0)
                {
                    foreach (KeyValuePair<string, string> kvp in dict)
                    {
                        sw.WriteLine(string.Format("        <Ticker Message=\"{0}\" />", kvp.Key));
                    }
                }
                else
                {
                    sw.WriteLine("        <Ticker Message=\"Have a suggestion or complaint? Post on our forums or discord and let us know.\" />");
                    sw.WriteLine("        <!-- <Ticker Message=\"Type /gimme once an hour for a free gift!\" /> -->");
                    sw.WriteLine("        <!-- <Ticker Message=\"Typ /gimme, einmal pro Stunde für ein freies Geschenk!\" /> -->");
                    sw.WriteLine("        <!-- <Ticker Message=\"Visit Yoursitehere for rules, custom recipes and forum discussions!\" /> -->");
                    sw.WriteLine("        <!-- <Ticker Message=\"Besuchen Yoursitehere für Regelungen , kundenspezifische Rezepturen und Forumsdiskussionen!\" /> -->");
                    sw.WriteLine("        <!-- <Ticker Message=\"Type /commands for a list of the chat commands.\" /> -->");
                }
                sw.WriteLine("    </Messages>");
                sw.WriteLine("</InfoTicketer>");
                sw.Flush();
                sw.Close();
            }
            fileWatcher.EnableRaisingEvents = true;
        }

        private static void InitFileWatcher()
        {
            fileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
            fileWatcher.Created += new FileSystemEventHandler(OnFileChanged);
            fileWatcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            fileWatcher.EnableRaisingEvents = true;
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            if (!Utils.FileExists(filePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        public static void StatusCheck()
        {
            if (ConnectionManager.Instance.ClientCount() > 0)
            {
                if (Random)
                {
                    msgList.RandomizeList();
                    string _message = msgList[0];
                    List<ClientInfo> _cInfoList = ConnectionManager.Instance.Clients.List.ToList();
                    for (int i = 0; i < _cInfoList.Count; i++)
                    {
                        ClientInfo _cInfo = _cInfoList[i];
                        if (_cInfo != null)
                        {
                            if (!exemptionList.Contains(_cInfo.playerId))
                            {
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                    msgList.RemoveAt(0);
                    if (msgList.Count == 0)
                    {
                        BuildList();
                    }
                }
                else
                {
                    string _message = msgList[0];
                    List<ClientInfo> _cInfoList = ConnectionManager.Instance.Clients.List.ToList();
                    for (int i = 0; i < _cInfoList.Count; i++)
                    {
                        ClientInfo _cInfo = _cInfoList[i];
                        if (_cInfo != null)
                        {
                            if (!exemptionList.Contains(_cInfo.playerId))
                            {
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                    msgList.RemoveAt(0);
                    if (msgList.Count == 0)
                    {
                        BuildList();
                    }
                }
            }
        }
    }
}