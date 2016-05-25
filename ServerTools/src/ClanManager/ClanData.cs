using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    public class ClanData
    {
        public static SortedDictionary<string, string> Cdict = new SortedDictionary<string, string>();
        private static string clandatafile = "ClansData.xml";
        private static string clandatafilepath = string.Format("{0}/{1}", API.DataPath, clandatafile);
        private static SortedDictionary<string, string> odict = new SortedDictionary<string, string>();
        private static string officerdatafile = "OfficersData.xml";
        private static string officerdatafilepath = string.Format("{0}/{1}", API.DataPath, officerdatafile);
        public static SortedDictionary<string, string> Pdict = new SortedDictionary<string, string>();
        private static string playerdatafile = "PlayersData.xml";
        private static string playerdatafilepath = string.Format("{0}/{1}", API.DataPath, playerdatafile);
        public static SortedDictionary<string, string> Idict = new SortedDictionary<string, string>();
        private static string invitedatafile = "InvitesData.xml";
        private static string invitedatafilepath = string.Format("{0}/{1}", API.DataPath, invitedatafile);

        public static List<string> ClansList
        {
            get { return new List<string>(Cdict.Keys); }
        }

        public static List<string> OwnersList
        {
            get { return new List<string>(Cdict.Values); }
        }

        public static List<string> OfficersList
        {
            get { return new List<string>(odict.Keys); }
        }

        public static List<string> PlayersList
        {
            get { return new List<string>(Pdict.Keys); }
        }

        public static List<string> InvitesList
        {
            get { return new List<string>(Idict.Keys); }
        }

        public static void Init()
        {
            if (ClanManager.IsEnabled)
            {
                LoadClanData();
                LoadOfficerData();
                LoadPlayerData();
                LoadInviteData();
            }
        }

        private static void LoadClanData()
        {
            if (!Utils.FileExists(clandatafilepath))
            {
                return;
            }
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(clandatafilepath);
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", clandatafile, e.Message));
                return;
            }
            XmlNode _ClandataXml = xmlDoc.DocumentElement;
            foreach (XmlNode childNode in _ClandataXml.ChildNodes)
            {
                if (childNode.Name == "Clans")
                {
                    Cdict.Clear();
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
                        if (!Cdict.ContainsKey(_line.GetAttribute("clan")))
                        {
                            Cdict.Add(_line.GetAttribute("clan"), _line.GetAttribute("steamId"));
                        }
                    }
                }
            }
        }

        private static void UpdateClanData()
        {
            using (StreamWriter sw = new StreamWriter(clandatafilepath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<ClanData>");
                sw.WriteLine("    <Clans>");
                foreach (KeyValuePair<string, string> kvp in Cdict)
                {
                    sw.WriteLine(string.Format("        <Clan clan=\"{0}\" steamId=\"{1}\" />", kvp.Key, kvp.Value));
                }
                sw.WriteLine("    </Clans>");
                sw.WriteLine("</ClanData>");
                sw.Flush();
                sw.Close();
            }
        }

        private static void LoadOfficerData()
        {
            if (!Utils.FileExists(officerdatafilepath))
            {
                return;
            }
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(officerdatafilepath);
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", officerdatafile, e.Message));
                return;
            }
            XmlNode _ClandataXml = xmlDoc.DocumentElement;
            foreach (XmlNode childNode in _ClandataXml.ChildNodes)
            {
                if (childNode.Name == "Officers")
                {
                    odict.Clear();
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
                        if (!odict.ContainsKey(_line.GetAttribute("steamId")))
                        {
                            odict.Add(_line.GetAttribute("steamId"), _line.GetAttribute("clan"));
                        }
                    }
                }
            }
        }

        private static void UpdateOfficerData()
        {
            using (StreamWriter sw = new StreamWriter(officerdatafilepath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<ClanData>");
                sw.WriteLine("    <Officers>");
                foreach (KeyValuePair<string, string> kvp in odict)
                {
                    sw.WriteLine(string.Format("        <Officer steamId=\"{0}\" clan=\"{1}\" />", kvp.Key, kvp.Value));
                }
                sw.WriteLine("    </Officers>");
                sw.WriteLine("</ClanData>");
                sw.Flush();
                sw.Close();
            }
        }

        private static void LoadPlayerData()
        {
            if (!Utils.FileExists(playerdatafilepath))
            {
                return;
            }
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(playerdatafilepath);
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", playerdatafile, e.Message));
                return;
            }
            XmlNode _ClandataXml = xmlDoc.DocumentElement;
            foreach (XmlNode childNode in _ClandataXml.ChildNodes)
            {
                if (childNode.Name == "Players")
                {
                    Pdict.Clear();
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
                        if (!Pdict.ContainsKey(_line.GetAttribute("steamId")))
                        {
                            Pdict.Add(_line.GetAttribute("steamId"), _line.GetAttribute("clan"));
                        }
                    }
                }
            }
        }

        private static void UpdatePlayerData()
        {
            using (StreamWriter sw = new StreamWriter(playerdatafilepath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<ClanData>");
                sw.WriteLine("    <Players>");
                foreach (KeyValuePair<string, string> kvp in Pdict)
                {
                    sw.WriteLine(string.Format("        <Officer steamId=\"{0}\" clan=\"{1}\" />", kvp.Key, kvp.Value));
                }
                sw.WriteLine("    </Players>");
                sw.WriteLine("</ClanData>");
                sw.Flush();
                sw.Close();
            }
        }

        private static void LoadInviteData()
        {
            if (!Utils.FileExists(invitedatafilepath))
            {
                return;
            }
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(invitedatafilepath);
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", invitedatafile, e.Message));
                return;
            }
            XmlNode _ClandataXml = xmlDoc.DocumentElement;
            foreach (XmlNode childNode in _ClandataXml.ChildNodes)
            {
                if (childNode.Name == "Invites")
                {
                    Idict.Clear();
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
                        if (!Idict.ContainsKey(_line.GetAttribute("steamId")))
                        {
                            Idict.Add(_line.GetAttribute("steamId"), _line.GetAttribute("clan"));
                        }
                    }
                }
            }
        }

        public static void UpdateInviteData()
        {
            using (StreamWriter sw = new StreamWriter(invitedatafilepath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<ClanData>");
                sw.WriteLine("    <Invites>");
                foreach (KeyValuePair<string, string> kvp in Idict)
                {
                    sw.WriteLine(string.Format("        <Invite steamId=\"{0}\" clan=\"{1}\" />", kvp.Key, kvp.Value));
                }
                sw.WriteLine("    </Invites>");
                sw.WriteLine("</ClanData>");
                sw.Flush();
                sw.Close();
            }
        }

        public static void AddClan(string _clanName, string _steamId)
        {
            if (Idict.ContainsKey(_steamId))
            {
                Idict.Remove(_steamId);
                UpdateInviteData();
            }
            Cdict.Add(_clanName, _steamId);
            odict.Add(_steamId, _clanName);
            Pdict.Add(_steamId, _clanName);
            UpdateClanData();
            UpdatePlayerData();
            UpdateOfficerData();
        }

        public static void RemoveClan(string _clanName, string _steamId)
        {
            Cdict.Remove(_clanName);
            odict.Remove(_steamId);
            foreach (string _player in PlayersList)
            {
                string _cName;
                if (Pdict.TryGetValue(_player, out _cName))
                {
                    if (_cName == _clanName)
                    {
                        Pdict.Remove(_player);
                    }
                }
            }
            foreach (string _invite in InvitesList)
            {
                string _cName;
                if (Idict.TryGetValue(_invite, out _cName))
                {
                    if (_cName == _clanName)
                    {
                        Idict.Remove(_invite);
                    }
                }
            }
            foreach (string _officer in OfficersList)
            {
                string _cName;
                if (odict.TryGetValue(_officer, out _cName))
                {
                    if (_cName == _clanName)
                    {
                        odict.Remove(_officer);
                    }
                }
            }
            UpdateClanData();
            UpdateOfficerData();
            UpdatePlayerData();
            UpdateInviteData();
        }

        public static void AddMember(string _clanName, string _steamId)
        {
            Idict.Remove(_steamId);
            Pdict.Add(_steamId, _clanName);
            UpdateInviteData();
            UpdatePlayerData();
        }

        public static void RemoveMember(string _steamId)
        {
            Pdict.Remove(_steamId);
            if (odict.ContainsKey(_steamId))
            {
                odict.Remove(_steamId);
            }
            UpdatePlayerData();
        }

        public static void PromoteMember(string _steamId, string _clanName)
        {
            if (!odict.ContainsKey(_steamId))
            {
                odict.Add(_steamId, _clanName);
                UpdateOfficerData();
            }
        }

        public static void DemoteMember(string _steamId)
        {
        }

        public static void InviteMember(string _steamId, string _clanName)
        {
            if (!Idict.ContainsKey(_steamId))
            {
                Idict.Add(_steamId, _clanName);
                UpdateInviteData();
            }
        }
    }
}
