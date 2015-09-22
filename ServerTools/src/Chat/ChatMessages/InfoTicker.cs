using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;

namespace ServerTools
{
    public class InfoTicker
    {
        public static bool IsEnabled = false;
        public static int DelayBetweenMessages = 5;
        private static string _file = "InfoTicker.xml";
        private static string _filepath = string.Format("{0}/{1}", Config._configpath, _file);
        public static FileSystemWatcher _fileWatcher = new FileSystemWatcher(Config._configpath, _file);
        private static SortedDictionary<string, string> _message = new SortedDictionary<string, string>();
        public static Thread th;
        public static bool IsRunning = false;
        private static List<string> Messages
        {
            get { return new List<string>(_message.Keys); }
        }

        public static void Init()
        {
            if (IsEnabled)
            {
                if (!Utils.FileExists(_filepath))
                {
                    UpdateXml();
                }
                LoadMessages();
                InitFileWatcher();
                IsRunning = true;
                Start();
            }
        }

        private static void UpdateXml()
        {
            _fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(_filepath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<InfoTicketer>");
                sw.WriteLine("    <Motd>");
                sw.WriteLine(string.Format("        <Motd Message=\"{0}\" />", Motd._message));
                sw.WriteLine("    </Motd>");
                sw.WriteLine("    <Messages>");
                if (_message.Count > 0)
                {
                    foreach (string _message in Messages)
                    {
                        sw.WriteLine(string.Format("        <Message Message=\"{0}\" />", _message));
                    }
                }
                else
                {
                    sw.WriteLine("        <!-- <Message Message=\"Type /gimme once an hour for a free gift!\" /> -->");
                    sw.WriteLine("        <!-- <Message Message=\"Visit Yoursitehere for rules, custom recipes and forum discussions!\" /> -->");
                    sw.WriteLine("        <!-- <Message Message=\"Have a suggestion or complaint? Post on our forums and let us know  at Yoursitehere!\" /> -->");
                    sw.WriteLine("        <!-- <Message Message=\"Type /commands for a list of the chat commands.\" /> -->");
                }
                sw.WriteLine("    </Messages>");
                sw.WriteLine("</InfoTicketer>");
                sw.Flush();
                sw.Close();
            }
            _fileWatcher.EnableRaisingEvents = true;
        }

        private static void LoadMessages()
        {
            if (!Utils.FileExists(_filepath))
            {
                UpdateXml();
            }
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(_filepath);
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", _file, e.Message));
                return;
            }
            XmlNode _MessagesXml = xmlDoc.DocumentElement;
            foreach (XmlNode childNode in _MessagesXml.ChildNodes)
            {
                if (childNode.Name == "Motd")
                {
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Motd' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("Message"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Motd entry because of missing a Message attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        Motd._message = _line.GetAttribute("Message");
                    }
                }
                if (childNode.Name == "Messages")
                {
                    _message.Clear();
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
                        if (!_message.ContainsKey(_line.GetAttribute("Message")))
                        {
                            _message.Add(_line.GetAttribute("Message"), null);
                        }
                    }
                }
            }
        }

        private static void InitFileWatcher()
        {
            _fileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
            _fileWatcher.Created += new FileSystemEventHandler(OnFileChanged);
            _fileWatcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            _fileWatcher.EnableRaisingEvents = true;
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            if (!Utils.FileExists(_filepath))
            {
                UpdateXml();
            }
            LoadMessages();
        }

        public static void Start()
        {
            th = new Thread(new ThreadStart(StatusCheck));
            th.IsBackground = true;
            th.Start();
        }

        private static void StatusCheck()
        {
            while (IsEnabled)
            {
                int _playerCount = ConnectionManager.Instance.ClientCount();
                if (_playerCount > 0)
                {
                    List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                    ClientInfo _cInfo = _cInfoList.RandomObject();
                    string _message = Messages.RandomObject();
                    GameManager.Instance.GameMessageServer(_cInfo, string.Format("{0}{1}[-]", CustomCommands._chatcolor, _message), "Server");  
                }
                Thread.Sleep(60000 * DelayBetweenMessages);
            }
        }
    }
}