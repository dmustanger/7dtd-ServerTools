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
            if (childNodes != null)
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
            if (upgrade)
            {
                XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                XmlNode node = nodeList[0];
                XmlElement line = (XmlElement)nodeList[0];
                if (line != null)
                {
                    if (line.HasAttributes)
                    {
                        UpgradeXml(nodeList);
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
                                UpgradeXml(nodeList);
                                return;
                            }
                        }
                    }
                }
                UpgradeXml(null);
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
                    foreach (string message in Dict)
                    {
                        string motd = message;
                        motd = motd.Replace("{EntityId}", _cInfo.entityId.ToString());
                        motd = motd.Replace("{SteamId}", _cInfo.playerId);
                        motd = motd.Replace("{PlayerName}", _cInfo.playerName);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + motd + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                        if (_oldChildNodes[i].NodeType == XmlNodeType.Comment && !_oldChildNodes[i].OuterXml.Contains("<!-- Possible variables") &&
                            !_oldChildNodes[i].OuterXml.Contains("<!-- <Server Message=\"Welcome to the server\"") && !_oldChildNodes[i].OuterXml.Contains("    <!-- <Server Message=\"\""))
                        {
                            sw.WriteLine(_oldChildNodes[i].OuterXml);
                        }
                    }
                    sw.WriteLine();
                    sw.WriteLine();
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)_oldChildNodes[i];
                            if (line.HasAttributes && line.Name == "Server")
                            {
                                string message = "";
                                if (line.HasAttribute("Message"))
                                {
                                    message = line.GetAttribute("Message");
                                }
                                sw.WriteLine(string.Format("    <Server Message=\"{0}\" />", message));
                            }
                        }
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