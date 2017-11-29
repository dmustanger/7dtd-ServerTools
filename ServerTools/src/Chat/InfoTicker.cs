using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;

namespace ServerTools
{
    public class InfoTicker
    {
        public static bool IsRunning = false;
        public static bool IsEnabled = false;
        public static bool Random = false;
        public static int DelayBetweenMessages = 5;
        private const string file = "InfoTicker.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static SortedDictionary<string, int> dict = new SortedDictionary<string, int>();
        private static List<string> msgList = new List<string>();
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        private static Thread th;


        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
            Start();
            IsRunning = true;
        }

        public static void Unload()
        {
            th.Abort();
            fileWatcher.Dispose();
            IsRunning = false;
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
                        if (!_line.HasAttribute("id"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Message entry because of missing 'id' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Message"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Message entry because of missing a Message attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        int _id;
                        if (!int.TryParse(_line.GetAttribute("id"), out _id))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Message entry because of invalid (non-numeric) value for 'id' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!dict.ContainsKey(_line.GetAttribute("Message")))
                        {
                            dict.Add(_line.GetAttribute("Message"), _id);
                        }
                        foreach (var msg in dict.Keys)
                        {
                            if (!msgList.Contains(msg))
                            {
                                msgList.Add(msg);
                            }  
                        }
                    }
                }
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
                if (dict.Count > 0)
                {
                    foreach (KeyValuePair<string, int> kvp in dict)
                    {
                        sw.WriteLine(string.Format("        <Message id=\"{0}\" Message=\"{1}\" />", kvp.Value, kvp.Key));
                    }
                }
                else
                {
                    sw.WriteLine("        <!-- <Message id=\"1\" Message=\"Type /gimme once an hour for a free gift!\" /> -->");
                    sw.WriteLine("        <!-- <Message id=\"1\" Message=\"Typ /gimme, einmal pro Stunde für ein freies Geschenk!\" /> -->");
                    sw.WriteLine("        <!-- <Message id=\"2\" Message=\"Visit Yoursitehere for rules, custom recipes and forum discussions!\" /> -->");
                    sw.WriteLine("        <!-- <Message id=\"2\" Message=\"Besuchen Yoursitehere für Regelungen , kundenspezifische Rezepturen und Forumsdiskussionen!\" /> -->");
                    sw.WriteLine("        <!-- <Message id=\"3\" Message=\"Have a suggestion or complaint? Post on our forums and let us know  at Yoursitehere!\" /> -->");
                    sw.WriteLine("        <!-- <Message id=\"4\" Message=\"Type /commands for a list of the chat commands.\" /> -->");
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

        private static void Start()
        {
            th = new Thread(new ThreadStart(StatusCheck));
            th.IsBackground = true;
            th.Start();
            Log.Out("[SERVERTOOLS] InfoTicker has started.");
        }

        private static void StatusCheck()
        {
            while (IsEnabled)
            {
                if (ConnectionManager.Instance.ClientCount() > 0)
                {
                    if (msgList.Count > 0)
                    {
                        List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                        if (Random)
                        {
                            ClientInfo _cInfo = _cInfoList.RandomObject();
                            msgList.RandomizeList();
                            var _message = msgList.First();
                            if (_message != null)
                            {
                                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _message), "Server", false, "", false);
                                msgList.RemoveAt(0);
                            }
                        }
                        else
                        { 
                            ClientInfo _cInfo = _cInfoList.RandomObject();
                            var _message = msgList.First();
                            if (_message != null)
                            {
                                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _message), "Server", false, "", false);
                                msgList.RemoveAt(0);
                            }
                        }
                    }
                    else
                    {
                        LoadXml();
                        if (msgList.Count > 0)
                        {
                            List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                            if (Random)
                            {
                                ClientInfo _cInfo = _cInfoList.RandomObject();
                                msgList.RandomizeList();
                                var _message = msgList.First();
                                if (_message != null)
                                {
                                    GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _message), "Server", false, "", false);
                                    msgList.RemoveAt(0);
                                }
                            }
                            else
                            {
                                ClientInfo _cInfo = _cInfoList.RandomObject();
                                var _message = msgList.First();
                                if (_message != null)
                                {
                                    GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _message), "Server", false, "", false);
                                    msgList.RemoveAt(0);
                                }
                            }
                        }
                    }
                }
                Thread.Sleep(60000 * DelayBetweenMessages);
            }
        }
    }
}