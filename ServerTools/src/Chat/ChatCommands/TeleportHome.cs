using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    public class TeleportHome
    {
        public static bool IsEnabled = false;
        public static bool IsRunning = false;
        public static int DelayBetweenUses = 60;
        private static SortedDictionary<string, string> dict = new SortedDictionary<string, string>();
        private static SortedDictionary<string, DateTime> dict1 = new SortedDictionary<string, DateTime>();
        private static string _file = "TeleportHomeData.xml";
        private static string _filepath = string.Format("{0}/{1}", API.DataPath, _file);

        public static void Load()
        {
            LoadXml();
            IsRunning = true;  
        }

        public static void Unload()
        {
            dict.Clear();
            dict1.Clear();
            IsRunning = false;
        }

        public static void SetHome(ClientInfo _cInfo)
        {
            if (!dict.ContainsKey(_cInfo.playerId))
            {
                HomeSet(_cInfo);
            }
            else
            {
                string _phrase9 = "{PlayerName} you already have a home set.";
                if (!Phrases.Dict.TryGetValue(9, out _phrase9))
                {
                    Log.Out("[SERVERTOOLS] Phrase 9 not found using default.");
                }
                _phrase9 = _phrase9.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase9, CustomCommands.ChatColor), "Server", false, "", false));
            }
        }

        private static void HomeSet(ClientInfo _cInfo)
        {
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            Vector3 _position = _player.GetPosition();
            string x = _position.x.ToString();
            string y = _position.y.ToString();
            string z = _position.z.ToString();
            string _sposition = x + "," + y + "," + z;
            dict.Add(_cInfo.playerId, _sposition);
            UpdateXml();
            string _phrase10 = "{PlayerName} your home has been saved.";
            if (!Phrases.Dict.TryGetValue(10, out _phrase10))
            {
                Log.Out("[SERVERTOOLS] Phrase 10 not found using default.");
            }
            _phrase10 = _phrase10.Replace("{PlayerName}", _cInfo.playerName);
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase10, CustomCommands.ChatColor), "Server", false, "", false));
        }

        public static void DelHome(ClientInfo _cInfo)
        {
            if (!dict.ContainsKey(_cInfo.playerId))
            {
                string _phrase11 = "{PlayerName} you do not have a home saved.";
                if (!Phrases.Dict.TryGetValue(11, out _phrase11))
                {
                    Log.Out("[SERVERTOOLS] Phrase 11 not found using default.");
                }
                _phrase11 = _phrase11.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase11, CustomCommands.ChatColor), "Server", false, "", false));
            }
            else
            {
                dict.Remove(_cInfo.playerId);
                if (dict1.ContainsKey(_cInfo.playerId))
                {
                    dict1.Remove(_cInfo.playerId);
                }
                UpdateXml();
                string _phrase12 = "{PlayerName} your home has been removed.";
                if (!Phrases.Dict.TryGetValue(12, out _phrase12))
                {
                    Log.Out("[SERVERTOOLS] Phrase 12 not found using default.");
                }
                _phrase12 = _phrase12.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase12, CustomCommands.ChatColor), "Server", false, "", false));
            }
        }

        public static void TeleHome(ClientInfo _cInfo)
        {
            if (!dict.ContainsKey(_cInfo.playerId))
            {
                string _phrase11 = "{PlayerName} you do not have a home saved.";
                if (!Phrases.Dict.TryGetValue(11, out _phrase11))
                {
                    Log.Out("[SERVERTOOLS] Phrase 11 not found using default.");
                }
                _phrase11 = _phrase11.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase11, CustomCommands.ChatColor), "Server", false, "", false));
            }
            else
            {
                string _cords = null;
                if (dict.TryGetValue(_cInfo.playerId, out _cords))
                {
                    if (DelayBetweenUses < 1)
                    {
                        Home(_cInfo, _cords);
                    }
                    else
                    {
                        if (!dict1.ContainsKey(_cInfo.playerId))
                        {
                            Home(_cInfo, _cords);
                        }
                        else
                        {
                            DateTime _datetime;
                            if (dict1.TryGetValue(_cInfo.playerId, out _datetime))
                            {
                                int _passedtime = GetMinutes(_datetime);
                                if (_passedtime < DelayBetweenUses)
                                {
                                    int _timeleft = DelayBetweenUses - _passedtime;
                                    string _phrase13 = "{PlayerName} you can only use /home once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                    if (!Phrases.Dict.TryGetValue(13, out _phrase13))
                                    {
                                        Log.Out("[SERVERTOOLS] Phrase 13 not found using default.");
                                    }
                                    _phrase13 = _phrase13.Replace("{PlayerName}", _cInfo.playerName);
                                    _phrase13 = _phrase13.Replace("{DelayBetweenUses}", DelayBetweenUses.ToString());
                                    _phrase13 = _phrase13.Replace("{TimeRemaining}", _timeleft.ToString());
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase13, CustomCommands.ChatColor), "Server", false, "", false));
                                }
                                else
                                {
                                    dict1.Remove(_cInfo.playerId);
                                    Home(_cInfo, _cords);
                                }
                            }
                        } 
                    }
                } 
            }
        }

        private static void Home(ClientInfo _cInfo, string _home)
        {
            float x;
            float y;
            float z;
            string[] _cords = _home.Split(',');
            float.TryParse(_cords[0], out x);
            float.TryParse(_cords[1], out y);
            float.TryParse(_cords[2], out z);
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            Vector3 destPos = new Vector3();
            destPos.x = x;
            destPos.y = -1;
            destPos.z = z;
            NetPackageTeleportPlayer pkg = new NetPackageTeleportPlayer(destPos);
            _cInfo.SendPackage(pkg);
            if (dict1.ContainsKey(_cInfo.playerId))
            {
                dict1.Remove(_cInfo.playerId);
            }
            dict1.Add(_cInfo.playerId, DateTime.Now);
            UpdateXml();
        }

        private static int GetMinutes(DateTime _datetime)
        {
            TimeSpan varTime = DateTime.Now - _datetime;
            double fractionalMinutes = varTime.TotalMinutes;
            int wholeMinutes = (int)fractionalMinutes;
            return wholeMinutes;
        }

        private static void LoadXml()
        {
            if (!Utils.FileExists(_filepath))
            {
                return;
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
            XmlNode _TeleportHomeXml = xmlDoc.DocumentElement;
            foreach (XmlNode childNode in _TeleportHomeXml.ChildNodes)
            {
                if (childNode.Name == "Homes")
                {
                    dict.Clear();
                    dict1.Clear();
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'homes' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("SteamId"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring homes entry because of missing 'SteamId' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Home"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring homes entry because of missing 'Home' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        DateTime _datetime;
                        if (!DateTime.TryParse(_line.GetAttribute("LastUsed"), out _datetime))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring home entry because of invalid (date) value for 'LastUsed' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string _steamid = _line.GetAttribute("SteamId");
                        if (!dict.ContainsKey(_steamid))
                        {
                            dict.Add(_steamid, _line.GetAttribute("Home"));
                        }
                        if (!dict1.ContainsKey(_steamid))
                        {
                            dict1.Add(_steamid, _datetime);
                        }
                    }
                }
            }
        }

        private static void UpdateXml()
        {
            using (StreamWriter sw = new StreamWriter(_filepath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<SavedHomes>");
                sw.WriteLine("    <Homes>");
                foreach (KeyValuePair<string, string> kvp in dict)
                {
                    DateTime _datetime;
                    if (dict1.TryGetValue(kvp.Key, out _datetime))
                    {
                        sw.WriteLine(string.Format("        <Home SteamId=\"{0}\" Home=\"{1}\" LastUsed=\"{2}\" />", kvp.Key, kvp.Value, _datetime));
                    }
                    else
                    {
                        sw.WriteLine(string.Format("        <Home SteamId=\"{0}\" Home=\"{1}\" LastUsed=\"\" />", kvp.Key, kvp.Value));
                    }
                }
                sw.WriteLine("    </Homes>");
                sw.WriteLine("</SavedHomes>");
                sw.Flush();
                sw.Close();
            }
        }
    }
}