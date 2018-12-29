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
                        if (!_line.HasAttribute("steamId"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ColorPrefix entry because of missing steamId attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("name"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ColorPrefix entry because of missing name attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("group"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ColorPrefix entry because of missing group attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("expires"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ColorPrefix entry because of missing expires attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        DateTime _dt;
                        if (!DateTime.TryParse(_line.GetAttribute("expires"), out _dt))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ColorPrefix entry because of invalid (date) value for 'expires' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string _steamId = _line.GetAttribute("steamId");
                        string _name = _line.GetAttribute("name");
                        string _group = _line.GetAttribute("group");
                        string _prefix = _line.GetAttribute("prefix");
                        string _color = _line.GetAttribute("color");
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
                sw.WriteLine("<Chat>");
                sw.WriteLine("    <ColorPrefix>");
                if (dict.Count > 0)
                {
                    foreach (KeyValuePair<string, string[]> kvp in dict)
                    {
                        DateTime _dt;
                        dict1.TryGetValue(kvp.Key, out _dt);
                        sw.WriteLine(string.Format("        <player steamId=\"{0}\" name=\"{1}\" group=\"{2}\" prefix=\"{3}\" color=\"{4}\" expires=\"{5}\" />", kvp.Key, kvp.Value[0], kvp.Value[1], kvp.Value[2], kvp.Value[3], _dt));
                    }
                }
                else
                {
                    sw.WriteLine("        <player steamId=\"12345678901\" name=\"bob\" group=\"admin1\" prefix=\"(Captain)\" color=\"[FF0000]\" expires=\"10/29/2050 7:30:00 AM\" />");
                    sw.WriteLine("        <player steamId=\"12345678901\" name=\"beth\" group=\"admin2\" prefix=\"(Admiral)\" color=\"[008000]\" expires=\"10/29/3000 7:30:00 AM\" />");
                    sw.WriteLine("        <player steamId=\"12345678901\" name=\"bev\" group=\"mod1\" prefix=\"(Marine)\" color=\"[FF9933]\" expires=\"10/29/2150 7:30:00 AM\" />");
                    sw.WriteLine("        <player steamId=\"12345678901\" name=\"betty\" group=\"donor1\" prefix=\"(Don)\" color=\"[009000]\" expires=\"04/24/2021 7:30:00 AM\" />");
                    sw.WriteLine("        <player steamId=\"12345678901\" name=\"bart\" group=\"donor2\" prefix=\"(Don)\" color=\"[FF66CC]\" expires=\"11/22/2018 7:30:00 AM\" />");
                    sw.WriteLine("        <player steamId=\"12345678901\" name=\"ben\" group=\"donor3\" prefix=\"(Don)\" color=\"[E9C918]\" expires=\"02/03/2020 7:30:00 AM\" />");
                    sw.WriteLine("        <player steamId=\"12345678901\" name=\"billy\" group=\"user1\" prefix=\"(Pimp)\" color=\"[ADAD85]\" expires=\"10/29/2050 7:30:00 AM\" />");
                    sw.WriteLine("        <player steamId=\"12345678901\" name=\"bernard\" group=\"user2\" prefix=\"(OG)\" color=\"[993366]\" expires=\"12/05/2019 7:30:00 AM\" />");
                    sw.WriteLine("        <player steamId=\"12345678901\" name=\"bee man\" group=\"user3\" prefix=\"**\" color=\"**\" expires=\"05/11/2020 7:30:00 AM\" />");
                }
                sw.WriteLine("    </ColorPrefix>");
                sw.WriteLine("</Chat>");
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
