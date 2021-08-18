using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    //10.0.0.0 to 10.255.255.255

    //172.16.0.0 to 172.31.255.255

    //192.168.0.0 to 192.168.255.255


    class CredentialCheck
    {
        public static bool IsEnabled = false, IsRunning = false, Family_Share = false, Bad_Id = false, No_Internal = false;
        public static int Admin_Level = 0;
        public static SortedDictionary<string, string> Dict = new SortedDictionary<string, string>();

        private const string file = "FamilyShareAccount.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static readonly string DetectionFile = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static readonly string DetectionFilepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, DetectionFile);
        
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
            Dict.Clear();
            FileWatcher.Dispose();
            IsRunning = false;
        }

        private static void LoadXml()
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
                                continue;
                            }
                            else if (_line.HasAttribute("SteamId") && _line.HasAttribute("Name"))
                            {
                                string _steamid = _line.GetAttribute("SteamId");
                                string _name = _line.GetAttribute("SteamId");
                                if (!Dict.ContainsKey(_steamid))
                                {
                                    Dict.Add(_steamid, _name);
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
                Log.Out(string.Format("[SERVERTOOLS] Error in CredentialCheck.LoadXml: {0}", e.Message));
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
                    sw.WriteLine("<FamilyShareAccount>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine();
                    sw.WriteLine();
                    foreach (KeyValuePair<string, string> _key in Dict)
                    {
                        sw.WriteLine(string.Format("    <Player SteamId=\"{0}\" Name=\"{1}\" />", _key.Key, _key.Value));
                    }
                    sw.WriteLine("</FamilyShareAccount>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CredentialCheck.UpdateXml: {0}", e.Message));
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

        public static bool AccCheck(ClientInfo _cInfo)
        {
            try
            {
                if (_cInfo != null)
                {
                    if (GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo) > Admin_Level)
                    {
                        if (Family_Share && !Dict.ContainsKey(_cInfo.playerId))
                        {
                            if (_cInfo.ownerId != _cInfo.playerId)
                            {
                                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"You have been kicked for using a family share account. Purchase the game or contact an administrator for permission to join this server\"", _cInfo.playerId), null);
                                using (StreamWriter sw = new StreamWriter(DetectionFilepath, true, Encoding.UTF8))
                                {
                                    sw.WriteLine(string.Format("{0}: Player name {1} with ownerId {2} playerId {3} IP Address {4} connected with a family share account", DateTime.Now, _cInfo.playerName, _cInfo.ownerId, _cInfo.playerId, _cInfo.ip));
                                    sw.WriteLine();
                                    sw.Flush();
                                    sw.Close();
                                }
                                return false;
                            }
                        }
                        if (Bad_Id)
                        {
                            if (_cInfo.ownerId.Length != 17 || !_cInfo.ownerId.StartsWith("7656119") || _cInfo.playerId.Length != 17 || !_cInfo.playerId.StartsWith("7656119"))
                            {
                                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"You have been kicked for using an invalid Id\"", _cInfo.playerId), null);
                                using (StreamWriter sw = new StreamWriter(DetectionFilepath, true, Encoding.UTF8))
                                {
                                    sw.WriteLine(string.Format("{0}: Player name {1} with ownerId {2} playerId {3} IP Address {4} connected with an invalid Id", DateTime.Now, _cInfo.playerName, _cInfo.ownerId, _cInfo.playerId, _cInfo.ip));
                                    sw.WriteLine();
                                    sw.Flush();
                                    sw.Close();
                                }
                                return false;
                            }
                        }
                        if (No_Internal)
                        {
                            long _ipInteger = PersistentOperations.ConvertIPToLong(_cInfo.ip);
                            long.TryParse("10.0.0.0", out long _rangeStart1);
                            long.TryParse("10.255.255.255", out long _rangeEnd1);
                            long.TryParse("172.16.0.0", out long _rangeStart2);
                            long.TryParse("172.31.255.255", out long _rangeEnd2);
                            long.TryParse("192.168.0.0", out long _rangeStart3);
                            long.TryParse("192.168.255.255", out long _rangeEnd3);
                            if ((_ipInteger >= _rangeStart1 && _ipInteger <= _rangeEnd1) || (_ipInteger >= _rangeStart2 && _ipInteger <= _rangeEnd2) || (_ipInteger >= _rangeStart3 && _ipInteger <= _rangeEnd3))
                            {
                                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"You have been kicked for using an internal IP address\"", _cInfo.playerId), null);
                                using (StreamWriter sw = new StreamWriter(DetectionFilepath, true, Encoding.UTF8))
                                {
                                    sw.WriteLine(string.Format("{0}: Player name {1} with ownerId {2} playerId {3} IP Address {4} connected with an internal IP address", DateTime.Now, _cInfo.playerName, _cInfo.ownerId, _cInfo.playerId, _cInfo.ip));
                                    sw.WriteLine();
                                    sw.Flush();
                                    sw.Close();
                                    sw.Dispose();
                                }
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CredentialCheck.AccCheck: {0}", e.Message));
            }
            return true;
        }

        private static void UpgradeXml(XmlNodeList _oldChildNodes)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                File.Delete(FilePath);
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<FamilyShareAccount>");
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
                        if (_line.HasAttributes && _line.OuterXml.Contains("Player"))
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
                    sw.WriteLine("</FamilyShareAccount>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CredentialCheck.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}
