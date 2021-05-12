using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    class ChatColorPrefix
    {
        public static bool IsEnabled = false, IsRunning = false;
        private const string file = "ChatColorPrefix.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        public static Dictionary<string, string[]> Dict = new Dictionary<string, string[]>();
        public static Dictionary<string, DateTime> Dict1 = new Dictionary<string, DateTime>();
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
            if (!IsEnabled && IsRunning)
            {
                Dict.Clear();
                Dict1.Clear();
                FileWatcher.Dispose();
                IsRunning = false;
            }
        }

        public static void LoadXml()
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
                if (childNode.Name == "ColorPrefix")
                {
                    Dict.Clear();
                    Dict1.Clear();
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'ChatColorPrefix' from ChatColorPrefix.xml: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("SteamId"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ChatColorPrefix entry because of missing SteamId attribute from ChatColorPrefix.xml: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Name"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ChatColorPrefix entry because of missing Name attribute from ChatColorPrefix.xml: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Group"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ChatColorPrefix entry because of missing Group attribute from ChatColorPrefix.xml: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Prefix"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ChatColorPrefix entry because of missing Prefix attribute from ChatColorPrefix.xml: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("NameColor"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ChatColorPrefix entry because of missing NameColor attribute from ChatColorPrefix.xml: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("PrefixColor"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ChatColorPrefix entry because of missing PrefixColor attribute from ChatColorPrefix.xml: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Expires"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ChatColorPrefix entry because of missing Expires attribute from ChatColorPrefix.xml: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!DateTime.TryParse(_line.GetAttribute("Expires"), out DateTime _dt))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ChatColorPrefix entry because of invalid (date) value for 'Expires' attribute from ChatColorPrefix.xml: {0}", subChild.OuterXml));
                            continue;
                        }
                        string _steamId = _line.GetAttribute("SteamId");
                        string _name = _line.GetAttribute("Name");
                        string _group = _line.GetAttribute("Group");
                        string _prefix = _line.GetAttribute("Prefix");
                        string _nameColor = _line.GetAttribute("NameColor");
                        string _prefixColor = _line.GetAttribute("PrefixColor");
                        if (_prefix == "**")
                        {
                            _prefix = "";
                        }
                        if (_nameColor == "**")
                        {
                            _nameColor = "";
                        }
                        if ((!_nameColor.Contains("[") || !_nameColor.Contains("]")) && _nameColor != "")
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ChatColorPrefix entry because of missing [] for Name Color attribute from ChatColorPrefix.xml: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (_prefixColor == "**")
                        {
                            _prefixColor = "";
                        }
                        if ((!_prefixColor.Contains("[") || !_prefixColor.Contains("]")) && _prefixColor != "")
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ChatColorPrefix entry because of missing [] for Prefix Color attribute from ChatColorPrefix.xml: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!Dict.ContainsKey(_steamId))
                        {
                            string[] _c = new string[] { _name, _group, _prefix, _nameColor, _prefixColor };
                            Dict.Add(_steamId, _c);
                            Dict1.Add(_steamId, _dt);
                        }
                    }
                }
            }
        }

        public static void UpdateXml()
        {
            FileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<ColorPrefixes>");
                sw.WriteLine("    <ColorPrefix>");
                if (Dict.Count > 0)
                {
                    foreach (KeyValuePair<string, string[]> kvp in Dict)
                    {
                        DateTime _dt;
                        Dict1.TryGetValue(kvp.Key, out _dt);
                        sw.WriteLine(string.Format("        <Player SteamId=\"{0}\" Name=\"{1}\" Group=\"{2}\" Prefix=\"{3}\" NameColor=\"{4}\" PrefixColor=\"{5}\" Expires=\"{6}\" />", kvp.Key, kvp.Value[0], kvp.Value[1], kvp.Value[2], kvp.Value[3], kvp.Value[4], _dt));
                    }
                }
                else
                {
                    sw.WriteLine("        <Player SteamId=\"12345678901\" Name=\"bob\" Group=\"admin1\" Prefix=\"(Captain)\" NameColor=\"[FF0000]\" PrefixColor=\"[FFFFFF]\" Expires=\"10/29/2050 7:30:00 AM\" />");
                    sw.WriteLine("        <Player SteamId=\"12345678901\" Name=\"beth\" Group=\"admin2\" Prefix=\"(Admiral)\" NameColor=\"[008000]\" PrefixColor=\"[FFFFFF]\" Expires=\"10/29/3000 7:30:00 AM\" />");
                    sw.WriteLine("        <Player SteamId=\"12345678901\" Name=\"bev\" Group=\"mod1\" Prefix=\"(Marine)\" NameColor=\"[FF9933]\" PrefixColor=\"[FFFFFF]\" Expires=\"10/29/2150 7:30:00 AM\" />");
                    sw.WriteLine("        <Player SteamId=\"12345678901\" Name=\"betty\" Group=\"donor1\" Prefix=\"(Don)\" NameColor=\"[009000]\" PrefixColor=\"[FFFFFF]\" Expires=\"04/24/2021 7:30:00 AM\" />");
                    sw.WriteLine("        <Player SteamId=\"12345678901\" Name=\"bart\" Group=\"donor2\" Prefix=\"(Don)\" NameColor=\"[FF66CC]\" PrefixColor=\"[FFFFFF]\" Expires=\"11/22/2018 7:30:00 AM\" />");
                    sw.WriteLine("        <Player SteamId=\"12345678901\" Name=\"ben\" Group=\"donor3\" Prefix=\"(Don)\" NameColor=\"[E9C918]\" PrefixColor=\"[FFFFFF]\" Expires=\"02/03/2020 7:30:00 AM\" />");
                    sw.WriteLine("        <Player SteamId=\"12345678901\" Name=\"billy\" Group=\"user1\" Prefix=\"(Pimp)\" NameColor=\"[ADAD85]\" PrefixColor=\"[FFFFFF]\" Expires=\"10/29/2050 7:30:00 AM\" />");
                    sw.WriteLine("        <Player SteamId=\"12345678901\" Name=\"bernard\" Group=\"user2\" Prefix=\"(OG)\" NameColor=\"[993366],[CC6699]\" PrefixColor=\"[FFA500],[FF8C00],[FF7F50]\" Expires=\"12/05/2019 7:30:00 AM\" />");
                    sw.WriteLine("        <Player SteamId=\"12345678901\" Name=\"bee man\" Group=\"user3\" Prefix=\"**\" NameColor=\"**\" PrefixColor=\"**\" Expires=\"05/11/2020 7:30:00 AM\" />");
                    sw.WriteLine("        <Player SteamId=\"12345678901\" Name=\"beatlejuice\" Group=\"user4\" Prefix=\"\" NameColor=\"\" PrefixColor=\"\" Expires=\"07/15/2030 7:30:00 AM\" />");
                }
                sw.WriteLine("    </ColorPrefix>");
                sw.WriteLine("</ColorPrefixes>");
                sw.Flush();
                sw.Close();
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
            if (!Utils.FileExists(filePath))
            {
                UpdateXml();
            }
            LoadXml();
        }
    }
}
