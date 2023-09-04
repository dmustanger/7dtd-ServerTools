using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    public class HighPingKicker
    {
        public static bool IsEnabled = false, IsRunning = false;
        public static int Max_Ping = 250, Flags = 2;

        public static Dictionary<string, string> Dict = new Dictionary<string, string>();

        private const string file = "HighPingImmunity.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static Dictionary<int, int> Violations = new Dictionary<int, int>();
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        public static void Unload()
        {
            Dict.Clear();
            Violations.Clear();
            FileWatcher.Dispose();
            IsRunning = false;
        }

        public static void LoadXml()
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
                Violations.Clear();
                if (childNodes != null && (childNodes[0] != null && childNodes[0].OuterXml.Contains("Version") && childNodes[0].OuterXml.Contains(Config.Version)))
                {
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        if (childNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement line = (XmlElement)childNodes[i];
                        if (!line.HasAttributes || !line.HasAttribute("Id") || !line.HasAttribute("Name"))
                        {
                            continue;
                        }
                        string id = line.GetAttribute("Id");
                        if (id == "")
                        {
                            continue;
                        }
                        string name = line.GetAttribute("Name");
                        if (!Dict.ContainsKey(id))
                        {
                            Dict.Add(id, name);
                        }
                    }
                }
                else
                {
                    XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                    if (nodeList != null)
                    {
                        Timers.UpgradeHighPingKickerXml(nodeList);
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
                    Log.Out(string.Format("[SERVERTOOLS] Error in HighPingKicker.LoadXml: {0}", e.Message));
                }
            }
        }

        public static void UpdateXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<HighPing>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- <Player Id=\"Steam_12345678909876543\" Name=\"Example\" /> -->");
                    if (Dict.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> c in Dict)
                        {
                            sw.WriteLine("    <Player Id=\"{0}\" Name=\"{1}\" />", c.Key, c.Value);
                        }
                    }
                    sw.WriteLine("</HighPing>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in HighPingKicker.UpdateXml: {0}", e.Message));
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
            if (!File.Exists(FilePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        public static void Exec()
        {
            try
            {
                List<ClientInfo> clients = GeneralOperations.ClientList();
                if (clients == null || clients.Count == 0)
                {
                    return;
                }
                ClientInfo cInfo;
                for (int i = 0; i < clients.Count; i++)
                {
                    cInfo = clients[i];
                    if (cInfo != null && cInfo.loginDone)
                    {
                        if ((cInfo.PlatformId != null && Dict.ContainsKey(cInfo.PlatformId.CombinedString)) || 
                            (cInfo.CrossplatformId != null && Dict.ContainsKey(cInfo.CrossplatformId.CombinedString)))
                        {
                            continue;
                        }
                        else if (cInfo.ping >= Max_Ping)
                        {
                            if (Violations.ContainsKey(cInfo.entityId))
                            {
                                Violations[cInfo.entityId] += 1;
                            }
                            else
                            {
                                Violations.Add(cInfo.entityId, 1);
                            }
                            if (Violations[cInfo.entityId] >= Flags)
                            {
                                KickPlayer(cInfo, cInfo.ping);
                            }
                        }
                        else if (Violations.ContainsKey(cInfo.entityId))
                        {
                            Violations.Remove(cInfo.entityId);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in HighPingKicker.Exec: {0}", e.Message);
            }
        }

        private static void KickPlayer(ClientInfo _cInfo, int _ping)
        {
            try
            {
                Violations.Remove(_cInfo.entityId);
                Phrases.Dict.TryGetValue("HighPing1", out string phrase);
                phrase = phrase.Replace("{PlayerName}", _cInfo.playerName);
                phrase = phrase.Replace("{Value}", _ping.ToString());
                phrase = phrase.Replace("{MaxPing}", Max_Ping.ToString());
                ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                Log.Out("[SERVERTOOLS] Kicked player with id '{0}' '{1}' named '{2}' for high ping violation of '{3}'", _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, _ping);
                GeneralOperations.KickPlayer(_cInfo, phrase);
                return;
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in HighPingKicker.KickPlayer: {0}", e.Message);
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
                    sw.WriteLine("<HighPing>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- <Player Id=\"Steam_12345678909876543\" Name=\"Example\" /> -->");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType == XmlNodeType.Comment)
                        {
                            if (!nodeList[i].OuterXml.Contains("<!-- <Player Id=\"Steam_12345678909876543") && !nodeList[i].OuterXml.Contains("<!-- Do not forget") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Version") && !nodeList[i].OuterXml.Contains("<Player Id=\"\""))
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
                            if (line.HasAttributes && line.Name == "Player")
                            {
                                string id = "", name = "";
                                if (line.HasAttribute("Id"))
                                {
                                    id = line.GetAttribute("Id");

                                }
                                if (line.HasAttribute("Name"))
                                {
                                    name = line.GetAttribute("Name");
                                }
                                sw.WriteLine(string.Format("    <Player Id=\"{0}\" Name=\"{1}\" />", id, name));
                            }
                        }
                    }
                    sw.WriteLine("</HighPing>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in HighPingKicker.UpgradeXml: {0}", e.Message);
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}