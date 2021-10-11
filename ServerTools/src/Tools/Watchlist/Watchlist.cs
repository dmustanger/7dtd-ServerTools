using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    public class WatchList
    {
        public static bool IsEnabled = false, IsRunning = false;
        public static int Admin_Level = 0;
        public static string Delay = "5";
        public static SortedDictionary<string, string> Dict = new SortedDictionary<string, string>();

        private static string file = "Watchlist.xml";
        private static string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
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
                bool upgrade = true;
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                if (childNodes != null && childNodes.Count > 0)
                {
                    Dict.Clear();
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        if (childNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement line = (XmlElement)childNodes[i];
                        if (line.HasAttributes)
                        {
                            if (line.HasAttribute("Version") && line.GetAttribute("Version") == Config.Version)
                            {
                                upgrade = false;
                                continue;
                            }
                            else if (line.HasAttribute("SteamId") && line.HasAttribute("Reason"))
                            {
                                string steamdId = line.GetAttribute("SteamId");
                                string reason = line.GetAttribute("Reason");
                                if (!Dict.ContainsKey(line.GetAttribute("SteamId")))
                                {
                                    Dict.Add(steamdId, reason);
                                }
                            }
                        }
                    }
                }
                if (childNodes != null && upgrade)
                {
                    UpgradeXml(childNodes);
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Watchlist.LoadXml: {0}", e.Message));
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
                    sw.WriteLine("<Watchlist>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("<!-- Player SteamId=\"12345678909876543\" Reason=\"Suspected cheating\" / -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    if (Dict.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> kvp in Dict)
                        {
                            sw.WriteLine(string.Format("    <Player SteamId=\"{0}\" Reason=\"{1}\" />", kvp.Key, kvp.Value));
                        }
                    }
                    sw.WriteLine("</Watchlist>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Watchlist.UpdateXml: {0}", e.Message));
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

        public static void SetDelay()
        {
            if (EventSchedule.watchlist != Delay)
            {
                EventSchedule.watchlist = Delay;
                if (Delay.Contains(",") && Delay.Contains(":"))
                {
                    string[] times = Delay.Split(',');
                    for (int i = 0; i < times.Length; i++)
                    {
                        if (DateTime.TryParse(DateTime.Today.ToString("d") + " " + times[i] + ":00", out DateTime time))
                        {
                            if (DateTime.Now < time)
                            {
                                EventSchedule.Add("Shutdown", time);
                                return;
                            }
                        }
                    }
                    for (int i = 0; i < times.Length; i++)
                    {
                        if (DateTime.TryParse(DateTime.Today.AddDays(1).ToString("d") + " " + times[i] + ":00", out DateTime time))
                        {
                            if (DateTime.Now < time)
                            {
                                EventSchedule.Add("Shutdown", time);
                                return;
                            }
                        }
                    }
                }
                else if (Delay.Contains(":"))
                {
                    if (DateTime.TryParse(DateTime.Today.ToString("d") + " " + Delay + ":00", out DateTime time))
                    {
                        if (DateTime.Now < time)
                        {
                            EventSchedule.Add("Shutdown", time);
                        }
                        else if (DateTime.TryParse(DateTime.Today.AddDays(1).ToString("d") + " " + Delay + ":00", out DateTime secondaryTime))
                        {
                            EventSchedule.Add("Shutdown", secondaryTime);
                        }
                    }
                }
                else
                {
                    if (int.TryParse(Delay, out int delay))
                    {
                        EventSchedule.Add("Shutdown", DateTime.Now.AddMinutes(delay));
                    }
                    else
                    {
                        Log.Out("[SERVERTOOLS] Invalid Shutdown Time detected. Use a single integer, 24h time or multiple 24h time entries");
                        Log.Out("[SERVERTOOLS] Example: 120 or 03:00 or 03:00, 06:00, 09:00");
                    }
                }
            }
        }

        public static void Exec()
        {
            try
            {
                List<ClientInfo> _cInfoList = PersistentOperations.ClientList();
                for (int i = 0; i < _cInfoList.Count; i++)
                {
                    ClientInfo _cInfo = _cInfoList[i];
                    if (Dict.ContainsKey(_cInfo.playerId))
                    {
                        foreach (var _cInfo1 in _cInfoList)
                        {
                            if (GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo) <= Admin_Level)
                            {
                                Phrases.Dict.TryGetValue("Watchlist1", out string _phrase);
                                if (Dict.TryGetValue(_cInfo.playerId, out string _reason))
                                {
                                    _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
                                    _phrase = _phrase.Replace("{Reason}", _reason);
                                    ChatHook.ChatMessage(_cInfo1, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Watchlist.List: {0}", e.Message));
            }
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
                    sw.WriteLine("<Watchlist>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("<!-- Player SteamId=\"12345678909876543\" Reason=\"Suspected cheating.\" / -->");
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
                            string _steamId = "", _reason = "";
                            if (_line.HasAttribute("SteamId"))
                            {
                                _steamId = _line.GetAttribute("SteamId");
                            }
                            if (_line.HasAttribute("Reason"))
                            {
                                _reason = _line.GetAttribute("Reason");
                            }
                            sw.WriteLine(string.Format("    <Player SteamId=\"{0}\" Reason=\"{1}\" />", _steamId, _reason));
                        }
                    }
                    sw.WriteLine("</Watchlist>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Watchlist.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}