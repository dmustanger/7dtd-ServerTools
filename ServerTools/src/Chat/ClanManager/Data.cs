using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    public class ClanData
    {
        public static SortedDictionary<string, string> Owners = new SortedDictionary<string, string>();
        private static string _Ownerdatafile = "OwnersData.xml";
        private static string _Ownerdatafilepath = string.Format("{0}/{1}", Config._datapath, _Ownerdatafile);
        public static SortedDictionary<string, string> Players = new SortedDictionary<string, string>();
        private static string _Playerdatafile = "PlayersData.xml";
        private static string _Playerdatafilepath = string.Format("{0}/{1}", Config._datapath, _Playerdatafile);
        public static SortedDictionary<string, string> Clans = new SortedDictionary<string, string>();
        private static string _Clandatafile = "ClansData.xml";
        private static string _Clandatafilepath = string.Format("{0}/{1}", Config._datapath, _Clandatafile);
        public static SortedDictionary<string, string> Officers = new SortedDictionary<string, string>();
        private static string _Officerdatafile = "OfficersData.xml";
        private static string _Officerdatafilepath = string.Format("{0}/{1}", Config._datapath, _Officerdatafile);
        public static SortedDictionary<string, string> Invites = new SortedDictionary<string, string>();
        private static string _Invitedatafile = "InvitesData.xml";
        private static string _Invitedatafilepath = string.Format("{0}/{1}", Config._datapath, _Invitedatafile);

        public static List<string> ClansList
        {
            get { return new List<string>(Clans.Keys); }
        }

        public static List<string> OwnersList
        {
            get { return new List<string>(Owners.Keys); }
        }

        public static List<string> PlayersList
        {
            get { return new List<string>(Players.Keys); }
        }

        public static List<string> OfficersList
        {
            get { return new List<string>(Officers.Keys); }
        }

        public static List<string> InvitesList
        {
            get { return new List<string>(Invites.Keys); }
        }

        public static void Init()
        {
            if (ClanManager.IsEnabled)
            {
                LoadInviteData();
                LoadOfficerData();
                LoadClanData();
                LoadPlayerData();
                LoadOwnerData();
            }
        }

        private static void LoadInviteData()
        {
            if (!Utils.FileExists(_Invitedatafilepath))
            {
                return;
            }
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(_Invitedatafilepath);
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", _Invitedatafile, e.Message));
                return;
            }
            XmlNode _ClandataXml = xmlDoc.DocumentElement;
            Invites.Clear();
            foreach (XmlNode childNode in _ClandataXml.ChildNodes)
            {
                if (childNode.Name == "Invites")
                {
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Invites' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("steamId"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Invite entry because of missing a steamId attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("clan"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Invite entry because of missing a clan attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!Invites.ContainsKey(_line.GetAttribute("steamId")))
                        {
                            Invites.Add(_line.GetAttribute("steamId"), _line.GetAttribute("clan"));
                        }
                    }
                }
            }
        }

        public static void UpdateInviteData()
        {
            if (!Directory.Exists(Config._datapath))
            {
                Directory.CreateDirectory(Config._datapath);
            }
            using (StreamWriter sw = new StreamWriter(_Invitedatafilepath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<ClanData>");
                sw.WriteLine("    <Invites>");
                foreach (KeyValuePair<string, string> kvp in Invites)
                {
                    sw.WriteLine(string.Format("        <Invite steamId=\"{0}\" clan=\"{1}\" />", kvp.Key, kvp.Value));
                }
                sw.WriteLine("    </Invites>");
                sw.WriteLine("</ClanData>");
                sw.Flush();
                sw.Close();
            }
        }

        private static void LoadOfficerData()
        {
            if (!Utils.FileExists(_Officerdatafilepath))
            {
                return;
            }
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(_Officerdatafilepath);
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", _Officerdatafile, e.Message));
                return;
            }
            XmlNode _ClandataXml = xmlDoc.DocumentElement;
            Officers.Clear();
            foreach (XmlNode childNode in _ClandataXml.ChildNodes)
            {
                if (childNode.Name == "Officers")
                {
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Officers' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("steamId"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Officer entry because of missing a steamId attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("clan"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Officer entry because of missing a clan attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!Officers.ContainsKey(_line.GetAttribute("steamId")))
                        {
                            Officers.Add(_line.GetAttribute("steamId"), _line.GetAttribute("clan"));
                        }
                    }
                }
            }
        }

        public static void UpdateOfficerData()
        {
            if (!Directory.Exists(Config._datapath))
            {
                Directory.CreateDirectory(Config._datapath);
            }
            using (StreamWriter sw = new StreamWriter(_Officerdatafilepath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<ClanData>");
                sw.WriteLine("    <Officers>");
                foreach (KeyValuePair<string, string> kvp in Officers)
                {
                    sw.WriteLine(string.Format("        <Officer steamId=\"{0}\" clan=\"{1}\" />", kvp.Key, kvp.Value));
                }
                sw.WriteLine("    </Officers>");
                sw.WriteLine("</ClanData>");
                sw.Flush();
                sw.Close();
            }
        }

        private static void LoadClanData()
        {
            if (!Utils.FileExists(_Clandatafilepath))
            {
                return;
            }
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(_Clandatafilepath);
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", _Clandatafile, e.Message));
                return;
            }
            XmlNode _ClandataXml = xmlDoc.DocumentElement;
            Clans.Clear();
            foreach (XmlNode childNode in _ClandataXml.ChildNodes)
            {
                if (childNode.Name == "Clans")
                {
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Clans' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("steamId"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clan entry because of missing a steamId attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("clan"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Clan entry because of missing a clan attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!Clans.ContainsKey(_line.GetAttribute("steamId")))
                        {
                            Clans.Add(_line.GetAttribute("clan"), _line.GetAttribute("steamId"));
                        }
                    }
                }
            }
        }

        public static void UpdateClanData()
        {
            if (!Directory.Exists(Config._datapath))
            {
                Directory.CreateDirectory(Config._datapath);
            }
            using (StreamWriter sw = new StreamWriter(_Clandatafilepath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<ClanData>");
                sw.WriteLine("    <Clans>");
                foreach (KeyValuePair<string, string> kvp in Clans)
                {
                    sw.WriteLine(string.Format("        <Clan clan=\"{0}\" steamId=\"{1}\" />", kvp.Key, kvp.Value));
                }
                sw.WriteLine("    </Clans>");
                sw.WriteLine("</ClanData>");
                sw.Flush();
                sw.Close();
            }
        }

        private static void LoadPlayerData()
        {
            if (!Utils.FileExists(_Playerdatafilepath))
            {
                return;
            }
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(_Playerdatafilepath);
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", _Playerdatafile, e.Message));
                return;
            }
            XmlNode _ClandataXml = xmlDoc.DocumentElement;
            Players.Clear();
            foreach (XmlNode childNode in _ClandataXml.ChildNodes)
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
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Players' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("steamId"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player entry because of missing a steamId attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("clan"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Player entry because of missing a clan attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!Players.ContainsKey(_line.GetAttribute("steamId")))
                        {
                            Players.Add(_line.GetAttribute("steamId"), _line.GetAttribute("clan"));
                        }
                    }
                }
            }
        }

        public static void UpdatePlayerData()
        {
            if (!Directory.Exists(Config._datapath))
            {
                Directory.CreateDirectory(Config._datapath);
            }
            using (StreamWriter sw = new StreamWriter(_Playerdatafilepath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<ClanData>");
                sw.WriteLine("    <Players>");
                foreach (KeyValuePair<string, string> kvp in Players)
                {
                    sw.WriteLine(string.Format("        <Officer steamId=\"{0}\" clan=\"{1}\" />", kvp.Key, kvp.Value));
                }
                sw.WriteLine("    </Players>");
                sw.WriteLine("</ClanData>");
                sw.Flush();
                sw.Close();
            }
        }

        private static void LoadOwnerData()
        {
            if (!Utils.FileExists(_Ownerdatafilepath))
            {
                return;
            }
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(_Ownerdatafilepath);
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", _Ownerdatafile, e.Message));
                return;
            }
            XmlNode _ClandataXml = xmlDoc.DocumentElement;
            Owners.Clear();
            foreach (XmlNode childNode in _ClandataXml.ChildNodes)
            {
                if (childNode.Name == "Owners")
                {
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Owners' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("steamId"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Owner entry because of missing a steamId attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("clan"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Owner entry because of missing a clan attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!Owners.ContainsKey(_line.GetAttribute("steamId")))
                        {
                            Owners.Add(_line.GetAttribute("steamId"), _line.GetAttribute("clan"));
                        }
                    }
                }
            }
        }

        public static void UpdateOwnerData()
        {
            if (!Directory.Exists(Config._datapath))
            {
                Directory.CreateDirectory(Config._datapath);
            }
            using (StreamWriter sw = new StreamWriter(_Ownerdatafilepath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<ClanData>");
                sw.WriteLine("    <Owners>");
                foreach (KeyValuePair<string, string> kvp in Owners)
                {
                    sw.WriteLine(string.Format("        <Owner steamId=\"{0}\" clan=\"{1}\" />", kvp.Key, kvp.Value));
                }
                sw.WriteLine("    </Owners>");
                sw.WriteLine("</ClanData>");
                sw.Flush();
                sw.Close();
            }
        }

        public static void AddClan(string _clanName, string _steamId)
        {
            if (Invites.ContainsKey(_steamId))
            {
                Invites.Remove(_steamId);
                UpdateInviteData();
            }
            Owners.Add(_steamId, _clanName);
            Clans.Add(_clanName, _steamId);
            Players.Add(_steamId, _clanName);
            Officers.Add(_steamId, _clanName);
            UpdateClanData();
            UpdatePlayerData();
            UpdateOfficerData();
            UpdateOwnerData();
        }

        public static void RemoveClan(string _clanName, string _steamId)
        {
            Owners.Remove(_steamId);
            Clans.Remove(_clanName);
            foreach (string _player in PlayersList)
            {
                string _cName;
                if (Players.TryGetValue(_player, out _cName))
                {
                    if(_cName == _clanName)
                    {
                        Players.Remove(_player);
                    }
                }
            }
            foreach (string _invite in InvitesList)
            {
                string _cName;
                if (Invites.TryGetValue(_invite, out _cName))
                {
                    if (_cName == _clanName)
                    {
                        Invites.Remove(_invite);
                    }
                }
            }
            foreach (string _officer in OfficersList)
            {
                string _cName;
                if (Officers.TryGetValue(_officer, out _cName))
                {
                    if (_cName == _clanName)
                    {
                        Officers.Remove(_officer);
                    }
                }
            }
            UpdateInviteData();
            UpdateClanData();
            UpdatePlayerData();
            UpdateOfficerData();
            UpdateOwnerData();
        }

        public static void AddMember(string _clanName, string _steamId)
        {
            Invites.Remove(_steamId);
            Players.Add(_steamId, _clanName);
            UpdateInviteData();
            UpdatePlayerData();   
        }

        public static void RemoveMember(string _steamId)
        {
            Players.Remove(_steamId);
            if (Officers.ContainsKey(_steamId))
            {
                Officers.Remove(_steamId);
            }
        }
    }
}
