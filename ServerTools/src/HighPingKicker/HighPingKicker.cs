using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    public class HighPingKicker
    {
        public static bool IsEnabled = false;
        public static int MAXPING = 250;
        public static int SamplesNeeded = 0;
        private const string _file = "HighPingImmunity.xml";
        private static string _filepath = string.Format("{0}/{1}", Config._configpath, _file);
        public static FileSystemWatcher _fileWatcher = new FileSystemWatcher(Config._configpath, _file);
        public static SortedDictionary<string, string> _whiteListPlayers = new SortedDictionary<string, string>();
        private static Dictionary<string, int> _sameples = new Dictionary<string, int>();
        public static bool IsRunning = false;

        public static List<string> SteamId
        {
            get { return new List<string>(_whiteListPlayers.Keys); }
        }

        private static List<string> Samples
        {
            get { return new List<string>(_sameples.Keys); }
        }

        public static void Init()
        {
            if (IsEnabled && !IsRunning)
            {
                if (!Utils.FileExists(_filepath))
                {
                    UpdateXml();
                }
                Loadxml();
                InitFileWatcher();
                IsRunning = true;
            }
        }

        public static void UpdateXml()
        {
            _fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(_filepath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<HighPingKicker>");
                sw.WriteLine("    <immunePlayers>");
                sw.WriteLine("        <!-- <Player SteamId=\"76560000000000000\" name=\"foo\" /> -->");
                sw.WriteLine("        <!-- <Player SteamId=\"76560580000000000\" name=\"foobar\" /> -->");
                sw.WriteLine("        <!-- <Player SteamId=\"76574740000000000\" name=\"somename\" /> -->");
                foreach (string _ids in SteamId)
                {
                    string _name = null;
                    if (_whiteListPlayers.TryGetValue(_ids, out _name))
                    {
                        sw.WriteLine(string.Format("        <Player SteamId=\"{0}\" name=\"{1}\" />", _ids, _name));
                    }
                    else
                    {
                        sw.WriteLine(string.Format("        <Player SteamId=\"{0}\" name=\"\" />", _ids));
                    }
                }
                sw.WriteLine("    </immunePlayers>");
                sw.WriteLine("</HighPingKicker>");
                sw.Flush();
                sw.Close();
            }
            _fileWatcher.EnableRaisingEvents = true;
        }

        private static void Loadxml()
        {
            if (!Utils.FileExists(_filepath))
            {
                UpdateXml();
            }
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(_filepath);
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", _file, e.Message));
                return;
            }
            XmlNode _HighPingKicker = xmlDoc.DocumentElement;
            _whiteListPlayers.Clear();
            foreach (XmlNode childNode in _HighPingKicker.ChildNodes)
            {
                if (childNode.Name == "immunePlayers")
                {
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
                        if (!_whiteListPlayers.ContainsKey(_steamid))
                        {
                            _whiteListPlayers.Add(_steamid, _line.GetAttribute("name"));
                        }
                    }
                }
            }
        }

        private static void InitFileWatcher()
        {
            _fileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
            _fileWatcher.Created += new FileSystemEventHandler(OnFileChanged);
            _fileWatcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            _fileWatcher.EnableRaisingEvents = true;
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            if (!Utils.FileExists(_filepath))
            {
                UpdateXml();
            }
            Loadxml();
        }

        public static void CheckPing(ClientInfo _cInfo)
        {
            string _steamid = _cInfo.playerId;
            if (_cInfo.ping > MAXPING && !_whiteListPlayers.ContainsKey(_steamid) && !GameManager.Instance.adminTools.IsAdmin(_steamid))
            {
                if (SamplesNeeded < 1)
                {
                    KickPlayer(_cInfo);
                }
                else
                {
                    if (!Samples.Contains(_steamid))
                    {
                        _sameples.Add(_steamid, 1);
                    }
                    else
                    {
                        int _savedsamples = 0;
                        if (_sameples.TryGetValue(_steamid, out _savedsamples))
                        {
                            if (_savedsamples < SamplesNeeded)
                            {
                                _sameples.Remove(_steamid);
                                _sameples.Add(_steamid, _savedsamples + 1);
                            }
                            else
                            {
                                _sameples.Remove(_steamid);
                                KickPlayer(_cInfo);
                            }
                        }
                    }
                    
                }
            }
            else
            {
                if (Samples.Contains(_steamid))
                {
                    _sameples.Remove(_steamid);
                }
            }
        }

        private static void KickPlayer(ClientInfo _cInfo)
        {
            string _phrase1 = "Auto Kicking {PlayerName} for high ping. ({PlayerPing}) Maxping is {MaxPing}.";
            string _phrase2 = "Auto Kicked: Ping To High. ({PlayerPing}) Max Ping is {MaxPing}.";
            if (Phrases._Phrases.TryGetValue(1, out _phrase1) && Phrases._Phrases.TryGetValue(2, out _phrase2))
            {
                _phrase1 = _phrase1.Replace("{0}", _cInfo.playerName);
                _phrase1 = _phrase1.Replace("{1}", _cInfo.ping.ToString());
                _phrase1 = _phrase1.Replace("{2}", MAXPING.ToString());
                _phrase1 = _phrase1.Replace("{PlayerName}", _cInfo.playerName);
                _phrase1 = _phrase1.Replace("{PlayerPing}", _cInfo.ping.ToString());
                _phrase1 = _phrase1.Replace("{MaxPing}", MAXPING.ToString());
                _phrase2 = _phrase2.Replace("{0}", _cInfo.ping.ToString());
                _phrase2 = _phrase2.Replace("{1}", MAXPING.ToString());
                _phrase2 = _phrase2.Replace("{PlayerPing}", _cInfo.ping.ToString());
                _phrase2 = _phrase2.Replace("{MaxPing}", MAXPING.ToString());
            }
            Log.Out(string.Format("[SERVERTOOLS] {0}", _phrase1));
            GameManager.Instance.GameMessageServer(_cInfo, string.Format("[FF8000]{0}[-]", _phrase1), "Server");
            SdtdConsole.Instance.ExecuteSync(string.Format("Kick {0} \"{1}\"", _cInfo.entityId, _phrase2), _cInfo);
        }
    }
}