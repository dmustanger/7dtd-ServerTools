using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    class FamilyShareAccount
    {
        public static bool IsEnabled = false;
        public static bool IsRunning = false;
        public static int AdminLevel = 0;
        private const string file = "FamilyShareAccount.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static SortedDictionary<string, string> OmittedPlayers = new SortedDictionary<string, string>();
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
                if (childNode.Name == "familyShareAllowed")
                {
                    OmittedPlayers.Clear();
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'familyShareAllowed' section: {0}", subChild.OuterXml));
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
                        if (!OmittedPlayers.ContainsKey(_steamid))
                        {
                            OmittedPlayers.Add(_steamid, _line.GetAttribute("name"));
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
                sw.WriteLine("<FamilyShareAccount>");
                sw.WriteLine("    <familyShareAllowed>");
                sw.WriteLine("        <!-- <Player SteamId=\"76560000000000000\" name=\"ralph\" /> -->");
                sw.WriteLine("        <!-- <Player SteamId=\"76560580000000000\" name=\"secondaryAccount\" /> -->");
                sw.WriteLine("        <!-- <Player SteamId=\"76574740000000000\" name=\"\" /> -->");
                foreach (KeyValuePair<string, string> _key in OmittedPlayers)
                {
                    sw.WriteLine(string.Format("        <Player SteamId=\"{0}\" name=\"{1}\" />", _key.Key, _key.Value));
                }
                sw.WriteLine("    </familyShareAllowed>");
                sw.WriteLine("</FamilyShareAccount>");
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

        public static void AccCheck(ClientInfo _cInfo)
        {
            if (_cInfo.ownerId != _cInfo.playerId && !OmittedPlayers.ContainsKey(_cInfo.playerId))
            {
                GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId);
                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                if (Admin.PermissionLevel > AdminLevel)
                {
                    string _file = string.Format("PlayerLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
                    string _filepath = string.Format("{0}/PlayerLogs/{1}", API.GamePath, _file);
                    using (StreamWriter sw = new StreamWriter(_filepath, true))
                    {
                        sw.WriteLine(string.Format("{0} {1} ownerId {2} playerId {3} IP Address {4} connected with a family share account", DateTime.Now, _cInfo.playerName, _cInfo.ownerId, _cInfo.playerId));
                        sw.WriteLine();
                        sw.Flush();
                        sw.Close();
                    }
                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"You have been kicked for using a family share account. Purchase the game or contact an administrator for permission to join this server\"", _cInfo.playerId), (ClientInfo)null);
                }
            }
        }
    }
}
