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
        public static int Max_Ping = 250;
        public static int Flags = 0;
        public static Dictionary<string, string> Dict = new Dictionary<string, string>();

        private const string file = "HighPingImmunity.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static Dictionary<string, int> FlagCounts = new Dictionary<string, int>();
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);

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
                Dict.Clear();
                FlagCounts.Clear();
                FileWatcher.Dispose();
                IsRunning = false;
            }
        }

        public static void LoadXml()
        {
            try
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
                XmlNodeList _childNodes = xmlDoc.DocumentElement.ChildNodes;
                if (_childNodes != null && _childNodes.Count > 0)
                {
                    Dict.Clear();
                    FlagCounts.Clear();
                    bool upgrade = true;
                    for (int i = 0; i < _childNodes.Count; i++)
                    {
                        if (_childNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement _line = (XmlElement)_childNodes[i];
                        if (_line.HasAttributes)
                        {
                            if (_line.HasAttribute("Version") && _line.GetAttribute("Version") == Config.Version)
                            {
                                upgrade = false;
                            }
                            else if (_line.HasAttribute("SteamId") && _line.HasAttribute("Name"))
                            {
                                string _id = _line.GetAttribute("SteamId");
                                string _name = _line.GetAttribute("Name");
                                if (!Dict.ContainsKey(_id))
                                {
                                    Dict.Add(_id, _name);
                                }
                            }
                        }
                    }
                    if (upgrade)
                    {
                        UpgradeXml(_childNodes);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in HighPingKicker.LoadXml: {0}", e.Message));
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
                    sw.WriteLine();
                    sw.WriteLine();
                    if (Dict.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> _c in Dict)
                        {
                            sw.WriteLine(string.Format("    <Player SteamId=\"{0}\" Name=\"{1}\" />", _c.Key, _c.Value));
                        }
                    }
                    else
                    {
                        sw.WriteLine("    <Player SteamId=\"\" Name=\"\" />");
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
            if (!Utils.FileExists(FilePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        public static void Exec(ClientInfo _cInfo)
        {
            try
            {
                if (Dict.ContainsKey(_cInfo.playerId) || GameManager.Instance.adminTools.IsAdmin(_cInfo))
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
                            if (!FlagCounts.ContainsKey(_cInfo.playerId))
                            {
                                FlagCounts.Add(_cInfo.playerId, 1);
                            }
                            else
                            {
                                FlagCounts.TryGetValue(_cInfo.playerId, out int _savedsamples);
                                {
                                    if (_savedsamples + 1 < Flags)
                                    {
                                        FlagCounts[_cInfo.playerId] = _savedsamples + 1;
                                    }
                                    else
                                    {
                                        FlagCounts.Remove(_cInfo.playerId);
                                        KickPlayer(_cInfo);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (FlagCounts.ContainsKey(_cInfo.playerId))
                        {
                            FlagCounts.Remove(_cInfo.playerId);
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
                Phrases.Dict.TryGetValue("HighPing1", out string _phrase1);
                _phrase1 = _phrase1.Replace("{PlayerName}", _cInfo.playerName);
                _phrase1 = _phrase1.Replace("{PlayerPing}", _cInfo.ping.ToString());
                _phrase1 = _phrase1.Replace("{MaxPing}", Max_Ping.ToString());
                ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                Phrases.Dict.TryGetValue("HighPing2", out string _phrase2);
                _phrase2 = _phrase2.Replace("{PlayerPing}", _cInfo.ping.ToString());
                _phrase2 = _phrase2.Replace("{MaxPing}", Max_Ping.ToString());
                SdtdConsole.Instance.ExecuteSync(string.Format("Kick {0} \"{1}\"", _cInfo.entityId, _phrase2), null);
                Log.Out(string.Format("[SERVERTOOLS] {0}", _phrase1));
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in HighPingKicker.KickPlayer: {0}", e.Message));
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
                    sw.WriteLine("<HighPing>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine();
                    sw.WriteLine();
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement _line = (XmlElement)_oldChildNodes[i];
                        if (_line.HasAttributes && _line.Name == "Player")
                        {
                            string _steamId = "", _name = "";
                            if (_line.HasAttribute("SteamId"))
                            {
                                _steamId = _line.GetAttribute("SteamId");
                            }
                            if (_line.HasAttribute("Name"))
                            {
                                _name = _line.GetAttribute("Name");
                            }
                            sw.WriteLine(string.Format("    <Player SteamId=\"{0}\" Name=\"{1}\" />", _steamId, _name));
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