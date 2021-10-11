using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    class LoginNotice
    {
        public static bool IsEnabled = false, IsRunning = false;
        public static Dictionary<string, string> Dict = new Dictionary<string, string>();

        private const string file = "LoginNotice.xml";
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
            FileWatcher.Dispose();
            IsRunning = false;
        }

        public static void LoadXml()
        {
            if (!Utils.FileExists(FilePath))
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
                            else if (line.HasAttribute("Id") && line.HasAttribute("Message"))
                            {
                                string id = line.GetAttribute("Id");
                                string message = line.GetAttribute("Message");
                                if (!Dict.ContainsKey(id))
                                {
                                    Dict.Add(id, message);
                                }
                            }
                        }
                    }
                }
            }
            if (childNodes != null && upgrade)
            {
                UpgradeXml(childNodes);
                return;
            }
        }

        private static void UpdateXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<LoginNotice>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("<!-- <Player Id=\"76561191234567891\" Message=\"Time to kick ass and chew bubble gum\" /> -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    if (Dict.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> kvp in Dict)
                        {
                            if (Dict.TryGetValue(kvp.Key, out string _message))
                            {
                                sw.WriteLine(string.Format("    <Player Id=\"{0}\" Message=\"{1}\" />", kvp.Key, _message));
                            }
                        }
                    }
                    else
                    {
                        sw.WriteLine("    <!-- <Player Id=\"\" Message=\"\" /> -->");
                    }
                    sw.WriteLine("</LoginNotice>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in LoginNotice.UpdateXml: {0}", e.Message));
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
            if (!Utils.FileExists(FilePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        public static void PlayerNotice(ClientInfo _cInfo)
        {
            if (Dict.TryGetValue(_cInfo.playerId, out string _message))
            {
                _message = _message.Replace("{PlayerName}", _cInfo.playerName);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _message + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
            }
        }

        private static void UpgradeXml(XmlNodeList _oldChildNodes)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<LoginNotice>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("<!-- <Player Id=\"76561191234567891\" Message=\"Time to kick ass and chew bubble gum\" /> -->");
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType == XmlNodeType.Comment && !_oldChildNodes[i].OuterXml.StartsWith("<!-- <Player Id=\"76561191234567891\"") &&
                            !_oldChildNodes[i].OuterXml.StartsWith("    <!-- <Zone Name=\"\""))
                        {
                            sw.WriteLine(_oldChildNodes[i].OuterXml);
                        }
                    }
                    sw.WriteLine();
                    sw.WriteLine();
                    bool _blank = true;
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement _line = (XmlElement)_oldChildNodes[i];
                            if (_line.HasAttributes && _line.Name == "Player")
                            {
                                _blank = false;
                                string _id = "", _message = "";
                                if (_line.HasAttribute("Id"))
                                {
                                    _id = _line.GetAttribute("Id");
                                }
                                if (_line.HasAttribute("Message"))
                                {
                                    _message = _line.GetAttribute("Message");
                                }
                                sw.WriteLine(string.Format("    <Player Id=\"{0}\" Message=\"{1}\" />", _id, _message));
                            }
                        }
                    }
                    if (_blank)
                    {
                        sw.WriteLine("    <!-- <Player Id=\"\" Message=\"\" /> -->");
                    }
                    sw.WriteLine("</LoginNotice>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in LoginNotice.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}
