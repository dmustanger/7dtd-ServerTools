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
        public static int DelayBetweenUses = 60;
        private static SortedDictionary<string, string> _savedHomes = new SortedDictionary<string, string>();
        private static SortedDictionary<string, DateTime> _lastused = new SortedDictionary<string, DateTime>();
        private static string _file = "SavedHomesData.xml";
        private static string _filepath = string.Format("{0}/{1}", Config._datapath, _file);

        public static void Init()
        {
            LoadSavedHomesXml();
        }

        public static void LoadSavedHomesXml()
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
            XmlNode _ConfigXml = xmlDoc.DocumentElement;
            _savedHomes.Clear();
            _lastused.Clear();
            foreach (XmlNode childNode in _ConfigXml.ChildNodes)
            {
                if (childNode.Name == "Homes")
                {
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Homes' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("steamId"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Home entry because of missing 'steamId' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("pos"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Home entry because of missing 'pos' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("lastused"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Home entry because of missing 'lastused' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        _savedHomes.Add(_line.GetAttribute("steamId"), _line.GetAttribute("pos"));
                        DateTime dt;
                        if (!DateTime.TryParse(_line.GetAttribute("lastused"), out dt))
                        { 
                            continue;
                        }
                        _lastused.Add(_line.GetAttribute("steamId"), dt);
                    }
                }
            }
        }

        private static void UpdateXml()
        {
            if (!Directory.Exists(Config._datapath))
            {
                Directory.CreateDirectory(Config._datapath);
            }
            using (StreamWriter sw = new StreamWriter(_filepath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<SavedHomes>");
                sw.WriteLine("    <Homes>");
                foreach (KeyValuePair<string, string> kvp in _savedHomes)
                {
                    DateTime _datetime;
                    if (_lastused.TryGetValue(kvp.Key, out _datetime))
                    {
                        sw.WriteLine(string.Format("        <Home steamId=\"{0}\" pos=\"{1}\" lastused=\"{2}\" />", kvp.Key, kvp.Value, _datetime));
                    }
                    else
                    {
                        sw.WriteLine(string.Format("        <Home steamId=\"{0}\" pos=\"{1}\" lastused=\"\" />", kvp.Key, kvp.Value));
                    }
                }
                sw.WriteLine("    </Homes>");
                sw.WriteLine("</SavedHomes>");
                sw.Flush();
                sw.Close();
            }
        }

        public static void SetHome(ClientInfo _cInfo)
        {
            if (_savedHomes.ContainsKey(_cInfo.playerId))
            {
                string _phrase9 = "You already have a home set.";
                if (Phrases._Phrases.TryGetValue(9, out _phrase9))
                {
                    _phrase9 = _phrase9.Replace("{0}", _cInfo.playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase9 , CustomCommands._chatcolor), "Server"));
            }
            else
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                Vector3 _position = _player.GetPosition();
                string x = _position.x.ToString();
                string y = _position.y.ToString();
                string z = _position.z.ToString();
                string _sposition = x + "," + y + "," + z;
                _savedHomes.Add(_cInfo.playerId, _sposition);
                string _phrase10 = "Your home has been saved.";
                if (Phrases._Phrases.TryGetValue(10, out _phrase10))
                {
                    _phrase10 = _phrase10.Replace("{0}", _cInfo.playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase10, CustomCommands._chatcolor), "Server"));
                UpdateXml();
            }
        }

        public static void DelHome(ClientInfo _cInfo)
        {
            if (!_savedHomes.ContainsKey(_cInfo.playerId))
            {
                string _phrase11 = "You do not have a home saved.";
                if (Phrases._Phrases.TryGetValue(11, out _phrase11))
                {
                    _phrase11 = _phrase11.Replace("{0}", _cInfo.playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase11, CustomCommands._chatcolor), "Server"));
            }
            else
            {
                _savedHomes.Remove(_cInfo.playerId);
                string _phrase12 = "Your home has been removed.";
                if (Phrases._Phrases.TryGetValue(12, out _phrase12))
                {
                    _phrase12 = _phrase12.Replace("{0}", _cInfo.playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase12, CustomCommands._chatcolor), "Server"));
                UpdateXml();
            }
        }

        public static void TeleHome(ClientInfo _cInfo)
        {
            string _position;
            if (!_savedHomes.TryGetValue(_cInfo.playerId, out _position))
            {
                string _phrase11 = "You do not have a home saved.";
                if (Phrases._Phrases.TryGetValue(11, out _phrase11))
                {
                    _phrase11 = _phrase11.Replace("{0}", _cInfo.playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase11, CustomCommands._chatcolor), "Server"));
            }
            else
            {
                float x;
                float y;
                float z;
                string[] _cords = _position.Split(',');
                float.TryParse(_cords[0], out x);
                float.TryParse(_cords[1], out y);
                float.TryParse(_cords[2], out z);
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                _player.position.x = x;
                _player.position.y = y;
                _player.position.z = z;
                NetPackageEntityTeleport pkg = new NetPackageEntityTeleport(_player);
                DateTime _datetime;
                if (DelayBetweenUses > 0 && _lastused.TryGetValue(_cInfo.playerId, out _datetime))
                {
                    int _passedtime = time.GetMinutes(_datetime);
                    if (_passedtime > DelayBetweenUses)
                    {
                        _lastused.Remove(_cInfo.playerId);
                        _cInfo.SendPackage(pkg);
                        _lastused.Add(_cInfo.playerId, DateTime.Now);
                        UpdateXml();
                    }
                    else
                    {
                        int _timeleft = DelayBetweenUses - _passedtime;
                        string _phrase13 = "{0} you can only use /home once every {1} minutes. Time remaining: {2} minutes.";
                        if (Phrases._Phrases.TryGetValue(13, out _phrase13))
                        {
                            _phrase13 = _phrase13.Replace("{0}", _cInfo.playerName);
                            _phrase13 = _phrase13.Replace("{1}", DelayBetweenUses.ToString());
                            _phrase13 = _phrase13.Replace("{2}", _timeleft.ToString());
                        }
                        _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase13, CustomCommands._chatcolor), "Server"));
                    }
                }
                else
                {
                    _cInfo.SendPackage(pkg);
                    if(_lastused.ContainsKey(_cInfo.playerId))
                    {
                        _lastused.Remove(_cInfo.playerId);
                    }
                    _lastused.Add(_cInfo.playerId, DateTime.Now);
                    UpdateXml();
                }
            }
        }
    }
}