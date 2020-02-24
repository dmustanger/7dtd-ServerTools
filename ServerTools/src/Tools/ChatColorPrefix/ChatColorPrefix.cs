using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    class ChatColorPrefix
    {
        public static bool IsEnabled = false, IsRunning = false;
        private const string file = "ChatColorPrefix.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        public static Dictionary<string, string[]> dict = new Dictionary<string, string[]>();
        public static Dictionary<string, DateTime> dict1 = new Dictionary<string, DateTime>();
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);

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
                dict.Clear();
                dict1.Clear();
                fileWatcher.Dispose();
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
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'ColorPrefix' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("SteamId"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ColorPrefix entry because of missing SteamId attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Name"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ColorPrefix entry because of missing Name attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Group"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ColorPrefix entry because of missing Group attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Expires"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ColorPrefix entry because of missing Expires attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!DateTime.TryParse(_line.GetAttribute("Expires"), out DateTime _dt))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ColorPrefix entry because of invalid (date) value for 'Expires' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string _steamId = _line.GetAttribute("SteamId");
                        string _name = _line.GetAttribute("Name");
                        string _group = _line.GetAttribute("Group");
                        string _prefix = _line.GetAttribute("Prefix");
                        string _color = _line.GetAttribute("Color");
                        if (_prefix == "**")
                        {
                            _prefix = "";
                        }
                        if (_color == "**")
                        {
                            _color = "";
                        }
                        if (!dict.ContainsKey(_steamId))
                        {
                            string[] _c = new string[] { _name, _group, _prefix, _color };
                            dict.Add(_steamId, _c);
                            dict1.Add(_steamId, _dt);
                        }
                    }
                }
            }
        }

        public static void UpdateXml()
        {
            fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<ColorPrefixes>");
                sw.WriteLine("    <ColorPrefix>");
                if (dict.Count > 0)
                {
                    foreach (KeyValuePair<string, string[]> kvp in dict)
                    {
                        DateTime _dt;
                        dict1.TryGetValue(kvp.Key, out _dt);
                        sw.WriteLine(string.Format("        <Player SteamId=\"{0}\" Name=\"{1}\" Group=\"{2}\" Prefix=\"{3}\" Color=\"{4}\" Expires=\"{5}\" />", kvp.Key, kvp.Value[0], kvp.Value[1], kvp.Value[2], kvp.Value[3], _dt));
                    }
                }
                else
                {
                    sw.WriteLine("        <Player SteamId=\"12345678901\" Name=\"bob\" Group=\"admin1\" Prefix=\"(Captain)\" Color=\"[FF0000]\" Expires=\"10/29/2050 7:30:00 AM\" />");
                    sw.WriteLine("        <Player SteamId=\"12345678901\" Name=\"beth\" Group=\"admin2\" Prefix=\"(Admiral)\" Color=\"[008000]\" Expires=\"10/29/3000 7:30:00 AM\" />");
                    sw.WriteLine("        <Player SteamId=\"12345678901\" Name=\"bev\" Group=\"mod1\" Prefix=\"(Marine)\" Color=\"[FF9933]\" Expires=\"10/29/2150 7:30:00 AM\" />");
                    sw.WriteLine("        <Player SteamId=\"12345678901\" Name=\"betty\" Group=\"donor1\" Prefix=\"(Don)\" Color=\"[009000]\" Expires=\"04/24/2021 7:30:00 AM\" />");
                    sw.WriteLine("        <Player SteamId=\"12345678901\" Name=\"bart\" Group=\"donor2\" Prefix=\"(Don)\" Color=\"[FF66CC]\" Expires=\"11/22/2018 7:30:00 AM\" />");
                    sw.WriteLine("        <Player SteamId=\"12345678901\" Name=\"ben\" Group=\"donor3\" Prefix=\"(Don)\" Color=\"[E9C918]\" Expires=\"02/03/2020 7:30:00 AM\" />");
                    sw.WriteLine("        <Player SteamId=\"12345678901\" Name=\"billy\" Group=\"user1\" Prefix=\"(Pimp)\" Color=\"[ADAD85]\" Expires=\"10/29/2050 7:30:00 AM\" />");
                    sw.WriteLine("        <Player SteamId=\"12345678901\" Name=\"bernard\" Group=\"user2\" Prefix=\"(OG)\" Color=\"[993366]\" Expires=\"12/05/2019 7:30:00 AM\" />");
                    sw.WriteLine("        <Player SteamId=\"12345678901\" Name=\"bee man\" Group=\"user3\" Prefix=\"**\" Color=\"**\" Expires=\"05/11/2020 7:30:00 AM\" />");
                }
                sw.WriteLine("    </ColorPrefix>");
                sw.WriteLine("</ColorPrefixes>");
                sw.Flush();
                sw.Close();
            }
            fileWatcher.EnableRaisingEvents = true;
        }

        private static void InitFileWatcher()
        {
            fileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
            fileWatcher.Created += new FileSystemEventHandler(OnFileChanged);
            fileWatcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            fileWatcher.EnableRaisingEvents = true;
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
