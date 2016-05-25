using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    public class KillMe
    {
        public static bool IsEnabled = false;
        public static bool IsRunning = false;
        public static int DelayBetweenUses = 60;
        private static SortedDictionary<string, DateTime> dict = new SortedDictionary<string, DateTime>();
        private static string file = "KillMeData.xml";
        private static string filepath = string.Format("{0}/{1}", API.DataPath, file);

        private static List<string> list
        {
            get { return new List<string>(dict.Keys); }
        }

        public static void Load()
        {
            LoadXml();
            IsRunning = true;
        }

        public static void Unload()
        {
            dict.Clear();
            IsRunning = false;
        }

        public static void CheckPlayer(ClientInfo _cInfo, bool _announce)
        {
            if (DelayBetweenUses < 1)
            {
                KillPlayer(_cInfo);
            }
            else
            {
                if (!dict.ContainsKey(_cInfo.playerId))
                {
                    KillPlayer(_cInfo);
                }
                else
                {
                    DateTime _datetime;
                    if (dict.TryGetValue(_cInfo.playerId, out _datetime))
                    {
                        TimeSpan varTime = DateTime.Now - _datetime;
                        double fractionalMinutes = varTime.TotalMinutes;
                        int _timepassed = (int)fractionalMinutes;
                        if (_timepassed > DelayBetweenUses)
                        {
                            KillPlayer(_cInfo);
                        }
                        else
                        {
                            int _timeremaining = DelayBetweenUses - _timepassed;
                            string _phrase8;
                            if (!Phrases.Dict.TryGetValue(8, out _phrase8))
                            {
                                _phrase8 = "{PlayerName} you can only use /killme once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                            }
                            _phrase8 = _phrase8.Replace("{PlayerName}", _cInfo.playerName);
                            _phrase8 = _phrase8.Replace("{DelayBetweenUses}", DelayBetweenUses.ToString());
                            _phrase8 = _phrase8.Replace("{TimeRemaining}", _timeremaining.ToString());
                            if (_announce)
                            {
                                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _phrase8), "Server", false, "", false);
                            }
                            else
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _phrase8), "Server", false, "", false));
                            }
                        }
                    }
                }
            }
        }

        private static void KillPlayer(ClientInfo _cInfo)
        {
            SdtdConsole.Instance.ExecuteSync(string.Format("kill {0}", _cInfo.entityId), _cInfo);
            if (dict.ContainsKey(_cInfo.playerId))
            {
                dict.Remove(_cInfo.playerId);
            }
            dict.Add(_cInfo.playerId, DateTime.Now);
            UpdateXml();
        }

        private static void UpdateXml()
        {
            using (StreamWriter sw = new StreamWriter(filepath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<Killme>");
                sw.WriteLine("    <Players>");
                foreach (string _id in list)
                {
                    DateTime _datetime;
                    if (dict.TryGetValue(_id, out _datetime))
                    {
                        TimeSpan varTime = DateTime.Now - _datetime;
                        double fractionalMinutes = varTime.TotalMinutes;
                        int _timepassed = (int)fractionalMinutes;
                        if (_timepassed > DelayBetweenUses)
                        {
                            dict.Remove(_id);
                        }
                        else
                        {
                            sw.WriteLine(string.Format("        <Player SteamId=\"{0}\" LastUsed=\"{1}\" />", _id, _datetime));
                        }
                    }
                }
                sw.WriteLine("    </Players>");
                sw.WriteLine("</Killme>");
                sw.Flush();
                sw.Close();
            }
        }

        private static void LoadXml()
        {
            if (!Utils.FileExists(filepath))
            {
                return;
            }
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(filepath);
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
                    dict.Clear();
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'players' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("SteamId"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring players entry because of missing 'SteamId' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        DateTime _datetime;
                        if (!DateTime.TryParse(_line.GetAttribute("LastUsed"), out _datetime))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring player entry because of invalid (date) value for 'LastUsed' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!dict.ContainsKey(_line.GetAttribute("SteamId")))
                        {
                            dict.Add(_line.GetAttribute("SteamId"), _datetime);
                        }
                    }
                }
            }
        }
    }
}