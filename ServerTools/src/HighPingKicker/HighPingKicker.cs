using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml;

namespace ServerTools
{
    public class HighPingKicker
    {
        public static bool IsEnabled = false;
        public static int Max_Ping = 250;
        public static int Samples_Needed = 0;

        //remove the next 2 lines a few months after everyone has a chance to update the mod
        private const string file = "HighPingImmunity.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        //----------------------------------------------------------------------------------

        private static Dictionary<string, int> samples = new Dictionary<string, int>();

        //remove Load() a few months after everyone has a chance to update the mod
        public static void Load()
        {
            if (IsEnabled)
            {
                Loadxml();
            }
        }
        //------------------------------------------------------------------------------

        //remove Loadxml() a few months after everyone has a chance to update the mod
        private static void Loadxml()
        {
            if (File.Exists(filePath))
            {
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
                            string _id = SQL.EscapeString(_line.GetAttribute("SteamId"));
                            string _sql = string.Format("SELECT * FROM Players WHERE steamid = '{0}'", _id);
                            DataTable _result = SQL.TQuery(_sql);
                            if (_result.Rows.Count > 0)
                            {
                                _sql = string.Format("UPDATE Players SET pingimmunity = 'true' WHERE steamid = '{0}'", _id);
                            }
                            else
                            {
                                _sql = string.Format("INSERT INTO Players (steamid, pingimmunity) VALUES ('{0}', 'true')", _id);
                            }
                            _result.Dispose();
                            SQL.FastQuery(_sql);
                        }
                    }
                }
                File.Delete(filePath);
            }
        }
        //----------------------------------------------------------------------------------------------

        public static void CheckPing(ClientInfo _cInfo)
        {
            string _sql = string.Format("SELECT pingimmunity FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            bool _isHighPingImmune = false;
            if (_result.Rows.Count > 0)
            {
                bool.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _isHighPingImmune); 
            }
            _result.Dispose();
            if (_cInfo.ping > Max_Ping && !_isHighPingImmune && !GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId))
            {
                if (Samples_Needed < 1)
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
                            if (_savedsamples < Samples_Needed)
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
                _phrase2 = "Auto Kicked: Ping too high. ({PlayerPing}) Max ping is {MaxPing}.";
            }
            _phrase1 = _phrase1.Replace("{PlayerName}", _cInfo.playerName);
            _phrase1 = _phrase1.Replace("{PlayerPing}", _cInfo.ping.ToString());
            _phrase1 = _phrase1.Replace("{MaxPing}", Max_Ping.ToString());
            _phrase2 = _phrase2.Replace("{PlayerPing}", _cInfo.ping.ToString());
            _phrase2 = _phrase2.Replace("{MaxPing}", Max_Ping.ToString());
            Log.Out(string.Format("[SERVERTOOLS] {0}", _phrase1));
            ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase1 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
            SdtdConsole.Instance.ExecuteSync(string.Format("Kick {0} \"{1}\"", _cInfo.entityId, _phrase2), (ClientInfo)null);
        }
    }
}