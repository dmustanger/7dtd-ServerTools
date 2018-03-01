using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    public class Motd
    {
        public static bool IsEnabled = false;
        public static bool IsRunning = false;
        public static bool Show_On_Respawn = false;
        private const string file = "Motd.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        private static bool updateConfig = false;
        public static List<string> Message = new List<string>();

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
            fileWatcher.Dispose();
            IsRunning = false;
        }

        public static void LoadXml()
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
                if (childNode.Name == "Motd")
                {
                    Message.Clear();
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
                        if (!_line.HasAttribute("message"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring message entry because of missing name attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string _message = _line.GetAttribute("message");
                        if (!Message.Contains(_message))
                        {
                            Message.Add(_message);
                        }
                    }
                }
            }
            if (updateConfig)
            {
                updateConfig = false;
                UpdateXml();
            }
        }

        private static void UpdateXml()
        {
            fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<Motds>");
                sw.WriteLine("    <Motd>");
                if (Message.Count > 0)
                {
                    foreach (string _message in Message)
                    {
                        sw.WriteLine(string.Format("        <Motd message=\"{0}\" />", _message));
                    }
                }
                else
                {
                    sw.WriteLine("        <Motd message=\"Welcome to the server\" />");
                    sw.WriteLine("        <Motd message=\"The server restarts every 4 hours\" />");
                }
                sw.WriteLine("    </Motd>");
                sw.WriteLine("</Motds>");
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
            IsRunning = true;
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            if (!Utils.FileExists(filePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        public static void Send(ClientInfo _cInfo)
        {
            if (Message.Count > 0)
            {
                foreach (string _message in Message)
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _message), "Server", false, "", false));
                }
            }
        }
    }
}