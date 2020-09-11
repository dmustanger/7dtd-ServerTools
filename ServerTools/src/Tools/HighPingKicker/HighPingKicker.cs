using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    public class HighPingKicker
    {
        public static bool IsEnabled = false, IsRunning = false;
        public static int Max_Ping = 250;
        public static int Flags = 0;
        private const string file = "HighPingImmunity.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        public static Dictionary<string, string> Dict = new Dictionary<string, string>();
        private static Dictionary<string, int> FlagCounts = new Dictionary<string, int>();
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);

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
                if (childNode.Name == "Immune")
                {
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Immune' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("SteamId"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring player entry because of missing 'SteamId' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Name"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring player entry because of missing 'Name' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string _id = _line.GetAttribute("SteamId");
                        string _name = _line.GetAttribute("Name");
                        if (!Dict.ContainsKey(_id))
                        {
                            Dict.Add(_id, _name);
                        }
                    }
                }
            }
        }

        public static void UpdateXml()
        {
            fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<HighPing>");
                sw.WriteLine("    <Immune>");
                if (Dict.Count > 0)
                {
                    foreach (KeyValuePair<string, string> _c in Dict)
                    {
                        sw.WriteLine(string.Format("        <Player Name=\"{0}\" SteamId=\"{1}\" />", _c.Key, _c.Value));
                    }
                }
                else
                {
                    sw.WriteLine("        <Player Name=\"Cow\" SteamId=\"76560987654321234\" />");
                }
                sw.WriteLine("    </Immune>");
                sw.WriteLine("</HighPing>");
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

        public static void Exec(ClientInfo _cInfo)
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
                            int _savedsamples;
                            FlagCounts.TryGetValue(_cInfo.playerId, out _savedsamples);
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

        private static void KickPlayer(ClientInfo _cInfo)
        {
            Phrases.Dict.TryGetValue(1, out string _phrase1);
            _phrase1 = _phrase1.Replace("{PlayerName}", _cInfo.playerName);
            _phrase1 = _phrase1.Replace("{PlayerPing}", _cInfo.ping.ToString());
            _phrase1 = _phrase1.Replace("{MaxPing}", Max_Ping.ToString());
            ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase1 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
            Phrases.Dict.TryGetValue(2, out string _phrase2);
            _phrase2 = _phrase2.Replace("{PlayerPing}", _cInfo.ping.ToString());
            _phrase2 = _phrase2.Replace("{MaxPing}", Max_Ping.ToString());
            ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase2 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
            SdtdConsole.Instance.ExecuteSync(string.Format("Kick {0} \"{1}\"", _cInfo.entityId, _phrase2), (ClientInfo)null);
            Log.Out(string.Format("[SERVERTOOLS] {0}", _phrase1));
        }
    }
}