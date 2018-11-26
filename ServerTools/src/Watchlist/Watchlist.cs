using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace ServerTools
{
    public class Watchlist
    {
        public static bool IsEnabled = false, IsRunning = false;
        public static int Admin_Level = 0;
        public static SortedDictionary<string, string> Dict = new SortedDictionary<string, string>();
        private static string file = "Watchlist.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
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
            Dict.Clear();
            fileWatcher.Dispose();
            IsRunning = false;
        }

        private static void LoadXml()
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
                if (childNode.Name == "Players")
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
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Players' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("SteamId"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player entry because of missing 'SteamId' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Reason"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player entry because of missing 'Reason' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!Dict.ContainsKey(_line.GetAttribute("SteamId")))
                        {
                            Dict.Add(_line.GetAttribute("SteamId"), _line.GetAttribute("Reason"));
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
                sw.WriteLine("<Watchlist>");
                sw.WriteLine("    <Players>");
                if (Dict.Count > 0)
                {
                    foreach (KeyValuePair<string, string> kvp in Dict)
                    {
                        sw.WriteLine(string.Format("        <Player SteamId=\"{0}\" Reason=\"{1}\" />", kvp.Key, kvp.Value));
                    }
                }
                else
                {
                    sw.WriteLine(string.Format("        <!-- Player SteamId=\"123456\" Reason=\"Suspected cheating.\" / -->"));
                }
                sw.WriteLine("    </Players>");
                sw.WriteLine("</Watchlist>");
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

        public static void CheckWatchlist()
        {
            List<ClientInfo> _cInfoList = ConnectionManager.Instance.Clients.List.ToList();
            for (int i = 0; i < _cInfoList.Count; i++)
            {
                ClientInfo _cInfo = _cInfoList[i];
                if (Dict.ContainsKey(_cInfo.playerId))
                {
                    foreach (var _cInfo1 in _cInfoList)
                    {
                        AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo1.playerId);
                        if (Admin.PermissionLevel <= Admin_Level)
                        {
                            string _phrase350;
                            if (!Phrases.Dict.TryGetValue(350, out _phrase350))
                            {
                                _phrase350 = "Player {PlayerName} is on the watchlist for {Reason}.";
                            }
                            string _reason = null;
                            if (Dict.TryGetValue(_cInfo.playerId, out _reason))
                            {
                                _phrase350 = _phrase350.Replace("{PlayerName}", _cInfo.playerName);
                                _phrase350 = _phrase350.Replace("{Reason}", _reason);
                                ChatHook.ChatMessage(_cInfo1, LoadConfig.Chat_Response_Color + _phrase350 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                }
            }
        }
    }
}