using System;
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
        private static Dictionary<string, int> FlagCounts = new Dictionary<string, int>();
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        private static XmlNodeList OldNodeList;

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        public static void Unload()
        {
            Dict.Clear();
            FlagCounts.Clear();
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
                bool upgrade = true;
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                if (childNodes != null)
                {
                    Dict.Clear();
                    FlagCounts.Clear();
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
                                else if (line.HasAttribute("SteamId") && line.HasAttribute("Name"))
                                {
                                    string id = line.GetAttribute("SteamId");
                                    string name = line.GetAttribute("Name");
                                    if (!Dict.ContainsKey(id))
                                    {
                                        Dict.Add(id, name);
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
                            OldNodeList = nodeList;
                            File.Delete(FilePath);
                            UpgradeXml();
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
                                    OldNodeList = nodeList;
                                    File.Delete(FilePath);
                                    UpgradeXml();
                                    return;
                                }
                            }
                            File.Delete(FilePath);
                            UpdateXml();
                            Log.Out(string.Format("[SERVERTOOLS] The existing HighPingImmunity.xml was too old or misconfigured. File deleted and rebuilt for version {0}", Config.Version));
                        }
                    }
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
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("    <!-- <Player Id=\"76561191234567890\" Name=\"Example\" /> -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    if (Dict.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> c in Dict)
                        {
                            sw.WriteLine(string.Format("    <Player Id=\"{0}\" Name=\"{1}\" />", c.Key, c.Value));
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

        public static void Exec(ClientInfo _cInfo)
        {
            try
            {
                if (Dict.ContainsKey(_cInfo.PlatformId.CombinedString) || Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString) || GameManager.Instance.adminTools.IsAdmin(_cInfo))
                {
                    return;
                }
                else
                {
                    if (_cInfo.ping >= Max_Ping)
                    {
                        if (Flags == 1)
                        {
                            KickPlayer(_cInfo);
                        }
                        else
                        {
                            if (!FlagCounts.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                            {
                                FlagCounts.Add(_cInfo.CrossplatformId.CombinedString, 1);
                            }
                            else
                            {
                                FlagCounts.TryGetValue(_cInfo.CrossplatformId.CombinedString, out int _savedsamples);
                                {
                                    if (_savedsamples + 1 < Flags)
                                    {
                                        FlagCounts[_cInfo.CrossplatformId.CombinedString] = _savedsamples + 1;
                                    }
                                    else
                                    {
                                        FlagCounts.Remove(_cInfo.CrossplatformId.CombinedString);
                                        KickPlayer(_cInfo);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (FlagCounts.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                        {
                            FlagCounts.Remove(_cInfo.CrossplatformId.CombinedString);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in HighPingKicker.Exec: {0}", e.Message));
            }
        }

        private static void KickPlayer(ClientInfo _cInfo)
        {
            try
            {
                Phrases.Dict.TryGetValue("HighPing1", out string phrase1);
                phrase1 = phrase1.Replace("{PlayerName}", _cInfo.playerName);
                phrase1 = phrase1.Replace("{PlayerPing}", _cInfo.ping.ToString());
                phrase1 = phrase1.Replace("{MaxPing}", Max_Ping.ToString());
                ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                Phrases.Dict.TryGetValue("HighPing2", out string phrase2);
                phrase2 = phrase2.Replace("{PlayerPing}", _cInfo.ping.ToString());
                phrase2 = phrase2.Replace("{MaxPing}", Max_Ping.ToString());
                SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("Kick {0} \"{1}\"", _cInfo.entityId, phrase2), null);
                Log.Out(string.Format("[SERVERTOOLS] Kicked player id {0} for high ping", _cInfo.CrossplatformId.CombinedString));
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in HighPingKicker.KickPlayer: {0}", e.Message));
            }
        }

        private static void UpgradeXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<HighPing>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("    <!-- <Player Id=\"76561191234567890\" Name=\"Example\" /> -->");
                    for (int i = 0; i < OldNodeList.Count; i++)
                    {
                        if (OldNodeList[i].NodeType == XmlNodeType.Comment && !OldNodeList[i].OuterXml.StartsWith("<!-- <Player Id=\"76561191234567890\"") &&
                            !OldNodeList[i].OuterXml.StartsWith("    <!-- <Player Id=\"\""))
                        {
                            sw.WriteLine(OldNodeList[i].OuterXml);
                        }
                    }
                    sw.WriteLine();
                    sw.WriteLine();
                    for (int i = 0; i < OldNodeList.Count; i++)
                    {
                        if (OldNodeList[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)OldNodeList[i];
                            if (line.HasAttributes && line.Name == "Player")
                            {
                                string id = "", name = "";
                                if (line.HasAttribute("SteamId"))
                                {
                                    id = line.GetAttribute("SteamId");
                                    if (id.Contains("_"))
                                    {
                                        id.Insert(0, "Steam_");
                                    }
                                }
                                else if (line.HasAttribute("Id"))
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
                Log.Out(string.Format("[SERVERTOOLS] Error in HighPingKicker.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}