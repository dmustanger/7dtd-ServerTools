using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    class LoginNotice
    {


        public static bool IsEnabled = false, IsRunning = false;
        private const string file = "LoginNotice.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        public static Dictionary<string, string> dict = new Dictionary<string, string>();
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        private static bool updateConfig = false;

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
                fileWatcher.Dispose();
                IsRunning = false;
            }
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
                if (childNode.Name == "Logins")
                {
                    dict.Clear();
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Login_Notice' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("id"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Login_Notice entry because of missing id attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("message"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Login_Notice entry because of missing message attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string _id = _line.GetAttribute("id");
                        string _message = _line.GetAttribute("message");
                        if (!dict.ContainsKey(_id))
                        {
                            dict.Add(_id, _message);
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
                sw.WriteLine("<LoginNotice>");
                sw.WriteLine("    <Logins>");
                if (dict.Count > 0)
                {
                    foreach (KeyValuePair<string, string> kvp in dict)
                    {
                        string _message;
                        if (dict.TryGetValue(kvp.Key, out _message))
                        {
                            sw.WriteLine(string.Format("        <login id=\"{0}\" message=\"{1}\" />", kvp.Key, _message));
                        }
                    }
                }
                else
                {
                    sw.WriteLine("        <login id=\"76561191234567891\" message=\"Time to kick ass and chew bubble gum\" />");
                    sw.WriteLine("        <login id=\"76561199876543210\" message=\"Head admin has arrived... shhh\" />");
                    sw.WriteLine("        <login id=\"76561191234509876\" message=\"Run for your lives, {PlayerName} has logged on\" />");
                }
                sw.WriteLine("    </Logins>");
                sw.WriteLine("</LoginNotice>");
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

        public static void PlayerNotice(ClientInfo _cInfo)
        {
            string _message;
            if (dict.TryGetValue(_cInfo.playerId, out _message))
            {
                _message = _message.Replace("{PlayerName}", _cInfo.playerName);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
            }
        }
    }
}
