using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    public class HighPingKicker
    {
        public static bool IsEnabled = false;
        public static bool IsRunning = false;
        public static int MAXPING = 250;
        public static int SamplesNeeded = 0;
        public static SortedDictionary<string, string> Dict = new SortedDictionary<string, string>();
        private const string file = "HighPingImmunity.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        private static Dictionary<string, int> samples = new Dictionary<string, int>();

        public static void Load()
        {
            if (IsEnabled && !IsRunning)
            {
                Loadxml();
                InitFileWatcher();
            }
        }

        public static void Unload()
        {
            fileWatcher.Dispose();
            IsRunning = false;
        }

        private static void Loadxml()
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
                if (childNode.Name == "immunePlayers")
                {
                    Dict.Clear();
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'immunePlayers' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("SteamId"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring player entry because of missing 'steamid' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("name"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring player entry because of missing 'name' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string _steamid = _line.GetAttribute("SteamId");
                        if (!Dict.ContainsKey(_steamid))
                        {
                            Dict.Add(_steamid, _line.GetAttribute("name"));
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
                sw.WriteLine("<HighPingKicker>");
                sw.WriteLine("    <immunePlayers>");
                sw.WriteLine("        <!-- <Player SteamId=\"76560000000000000\" name=\"foo\" /> -->");
                sw.WriteLine("        <!-- <Player SteamId=\"76560580000000000\" name=\"foobar\" /> -->");
                sw.WriteLine("        <!-- <Player SteamId=\"76574740000000000\" name=\"\" /> -->");
                foreach (KeyValuePair<string, string> _key in Dict)
                {
                    sw.WriteLine(string.Format("        <Player SteamId=\"{0}\" name=\"{1}\" />", _key.Key, _key.Value));
                }
                sw.WriteLine("    </immunePlayers>");
                sw.WriteLine("</HighPingKicker>");
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
            Loadxml();
        }

        public static void CheckPing(ClientInfo _cInfo)
        {
            if (_cInfo.ping > MAXPING && !Dict.ContainsKey(_cInfo.playerId) && !GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId))
            {
                if (SamplesNeeded < 1)
                {
                    KickPlayer(_cInfo);
                }
                else
                {
                    if (!samples.ContainsKey(_cInfo.playerId))
                    {
                        samples.Add(_cInfo.playerId, 1);
                    }
                    else
                    {
                        int _savedsamples = 0;
                        if (samples.TryGetValue(_cInfo.playerId, out _savedsamples))
                        {
                            if (_savedsamples < SamplesNeeded)
                            {
                                samples.Remove(_cInfo.playerId);
                                samples.Add(_cInfo.playerId, _savedsamples + 1);
                            }
                            else
                            {
                                samples.Remove(_cInfo.playerId);
                                KickPlayer(_cInfo);
                            }
                        }
                    }
                }
            }
            else
            {
                if (samples.ContainsKey(_cInfo.playerId))
                {
                    samples.Remove(_cInfo.playerId);
                }
            }
        }

        private static void KickPlayer(ClientInfo _cInfo)
        {
            string _phrase1;
            string _phrase2;
            if (!Phrases.Dict.TryGetValue(1, out _phrase1))
            {
                _phrase1 = "Auto Kicking {PlayerName} for high ping. ({PlayerPing}) Maxping is {MaxPing}.";
            }
            if (!Phrases.Dict.TryGetValue(2, out _phrase2))
            {
                _phrase2 = "Auto Kicked: Ping To High. ({PlayerPing}) Max Ping is {MaxPing}.";
            }
            _phrase1 = _phrase1.Replace("{PlayerName}", _cInfo.playerName);
            _phrase1 = _phrase1.Replace("{PlayerPing}", _cInfo.ping.ToString());
            _phrase1 = _phrase1.Replace("{MaxPing}", MAXPING.ToString());
            _phrase2 = _phrase2.Replace("{PlayerPing}", _cInfo.ping.ToString());
            _phrase2 = _phrase2.Replace("{MaxPing}", MAXPING.ToString());
            Log.Out(string.Format("[SERVERTOOLS] {0}", _phrase1));
            GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0}[-]", _phrase1), "Server", false, "", false);
            SdtdConsole.Instance.ExecuteSync(string.Format("Kick {0} \"{1}\"", _cInfo.entityId, _phrase2), _cInfo);
        }
    }
}