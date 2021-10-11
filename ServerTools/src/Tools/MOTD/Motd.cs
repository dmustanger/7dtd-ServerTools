using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    public class Motd
    {
        public static bool IsEnabled = false, IsRunning = false, Show_On_Respawn = false;
        public static List<string> Dict = new List<string>();

        private const string file = "Motd.xml";
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
                            if (line.HasAttribute("Message"))
                            {
                                string message = line.GetAttribute("Message");
                                if (!Dict.Contains(message))
                                {
                                    Dict.Add(message);
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
                    sw.WriteLine("<Motds>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("<!-- Possible variables {EntityId} {SteamId} {PlayerName} -->");
                    sw.WriteLine("<!-- <Server Message=\"Welcome to the server\" /> -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    if (Dict.Count > 0)
                    {
                        foreach (string _message in Dict)
                        {
                            sw.WriteLine(string.Format("    <Server Message=\"{0}\" />", _message));
                        }
                    }
                    else
                    {
                        sw.WriteLine("    <!-- <Server Message=\"\" /> -->");
                    }
                    sw.WriteLine("</Motds>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Motd.UpdateXml: {0}", e.Message));
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

        public static void Send(ClientInfo _cInfo)
        {
            try
            {
                if (Dict.Count > 0)
                {
                    foreach (string _message in Dict)
                    {
                        string _motd = _message;
                        _motd = _motd.Replace("{EntityId}", _cInfo.entityId.ToString());
                        _motd = _motd.Replace("{SteamId}", _cInfo.playerId);
                        _motd = _motd.Replace("{PlayerName}", _cInfo.playerName);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _motd + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Motd.Send: {0}", e.Message));
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
                    sw.WriteLine("<Motds>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("<!-- Possible variables {EntityId} {SteamId} {PlayerName} -->");
                    sw.WriteLine("<!-- <Server Message=\"Welcome to the server\" /> -->");
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType == XmlNodeType.Comment && !_oldChildNodes[i].OuterXml.StartsWith("<!-- Possible variables") &&
                            !_oldChildNodes[i].OuterXml.StartsWith("<!-- <Server Message=\"Welcome to the server\"") && !_oldChildNodes[i].OuterXml.StartsWith("    <!-- <Server Message=\"\""))
                        {
                            sw.WriteLine(_oldChildNodes[i].OuterXml);
                        }
                    }
                    sw.WriteLine();
                    sw.WriteLine();
                    bool _blank = true;
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement _line = (XmlElement)_oldChildNodes[i];
                        if (_line.HasAttributes && _line.Name == "Server")
                        {
                            _blank = false;
                            string _message = "";
                            if (_line.HasAttribute("Message"))
                            {
                                _message = _line.GetAttribute("Message");
                            }
                            sw.WriteLine(string.Format("    <Server Message=\"{0}\" />", _message));
                        }
                    }
                    if (_blank)
                    {
                        sw.WriteLine("    <!-- <Server Message=\"\" /> -->");
                    }
                    sw.WriteLine("</Motds>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Motd.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}