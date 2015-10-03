using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    public class KillMe
    {
        public static bool IsEnabled = false;
        public static int DelayBetweenUses = 60;
        private static string _file = "KillMeData.xml";
        private static string _filepath = string.Format("{0}/{1}", Config._datapath, _file);
        private static Dictionary<string, DateTime> _Players = new Dictionary<string, DateTime>();

        private static List<string> _PlayersList
        {
            get { return new List<string>(_Players.Keys); }
        }

        public static void Init()
        { 
            LoadKillmeXml();
        }

        private static void LoadKillmeXml()
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
            XmlNode _KillmeXml = xmlDoc.DocumentElement;
            _Players.Clear();
            foreach (XmlNode childNode in _KillmeXml.ChildNodes)
            {
                if (childNode.Name == "Players")
                {
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
                        _Players.Add(_line.GetAttribute("SteamId"), _datetime);
                    }
                }
            }
        }

        private static void UpdateKillmeXml()
        {
            if (!Directory.Exists(Config._datapath))
            {
                Directory.CreateDirectory(Config._datapath);
            }
            using (StreamWriter sw = new StreamWriter(_filepath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<Killme>");
                sw.WriteLine("    <Players>");
                foreach (string _sid in _PlayersList)
                {
                    DateTime _datetime;
                    if (_Players.TryGetValue(_sid, out _datetime))
                    {
                        int _timepassed = time.GetMinutes(_datetime);
                        if (_timepassed > DelayBetweenUses)
                        {
                            _Players.Remove(_sid);
                        }
                        else
                        {
                            sw.WriteLine(string.Format("        <Player SteamId=\"{0}\" LastUsed=\"{1}\" />", _sid, _datetime));
                        }
                    }
                }
                sw.WriteLine("    </Players>");
                sw.WriteLine("</Killme>");
                sw.Flush();
                sw.Close();
            }
        }

        public static void KillPlayer(ClientInfo _cInfo, bool _announce, string _message, string _playerName)
        {
            DateTime _datetime;
            if (DelayBetweenUses > 0 && _Players.TryGetValue(_cInfo.playerId, out _datetime))
            {
                int _timepassed = time.GetMinutes(_datetime);
                if (_timepassed < DelayBetweenUses)
                {
                    int _timeleft = DelayBetweenUses - _timepassed;
                    string _phrase8 = "{PlayerName} you can only use /killme once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                    if (Phrases._Phrases.TryGetValue(8, out _phrase8))
                    {
                        _phrase8 = _phrase8.Replace("{0}", _playerName);
                        _phrase8 = _phrase8.Replace("{1}", DelayBetweenUses.ToString());
                        _phrase8 = _phrase8.Replace("{2}", _timeleft.ToString());
                        _phrase8 = _phrase8.Replace("{PlayerName}", _playerName);
                        _phrase8 = _phrase8.Replace("{DelayBetweenUses}", DelayBetweenUses.ToString());
                        _phrase8 = _phrase8.Replace("{TimeRemaining}", _timeleft.ToString());
                    }
                    if (_announce)
                    {
                        GameManager.Instance.GameMessageServer(_cInfo, string.Format("{0}{1}[-]", CustomCommands._chatcolor, _phrase8), "Server");
                    }
                    else
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{0}{1}[-]", CustomCommands._chatcolor, _phrase8), "Server"));
                    }
                }
                else
                {
                    _Players.Remove(_cInfo.playerId);
                    SdtdConsole.Instance.ExecuteSync(string.Format("kill {0}", _cInfo.entityId), _cInfo);
                    _Players.Add(_cInfo.playerId, DateTime.Now);
                    UpdateKillmeXml();
                }
            }
            else
            {
                if (_PlayersList.Contains(_cInfo.playerId))
                {
                    _Players.Remove(_cInfo.playerId);
                }
                SdtdConsole.Instance.ExecuteSync(string.Format("kill {0}", _cInfo.entityId), _cInfo);
                _Players.Add(_cInfo.playerId, DateTime.Now);
                UpdateKillmeXml();
            }
        }
    }
}