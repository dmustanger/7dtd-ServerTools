using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    public class Badwords
    {
        public static bool IsEnabled = false;
        public static bool IsRunning = false;
        private const string file = "BadWords.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static SortedDictionary<string, string> dict = new SortedDictionary<string, string>();
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        public static List<string> List
        {
            get { return new List<string>(dict.Keys); }
        }

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
            dict.Clear();
            fileWatcher.Dispose();
            IsRunning = false;
        }

        private static void LoadXml()
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
                if (childNode.Name == "BadWords")
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
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'BadWords' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("Word"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring BadWord entry because of missing a Word attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string _word = _line.GetAttribute("Word");
                        _word = _word.ToLower();
                        if (!dict.ContainsKey(_word))
                        {
                            dict.Add(_word, null);
                        }
                    }
                }
            }
        }

        private static void UpdateXml()
        {
            fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<BadWordFilter>");
                sw.WriteLine("    <BadWords>");
                if (dict.Count > 0)
                {
                    foreach (string _word in List)
                    {
                        sw.WriteLine(string.Format("        <BadWord Word=\"{0}\" />", _word));
                    }
                }
                else
                {
                    sw.WriteLine("        <BadWord Word=\"nigger\" />");
                    sw.WriteLine("        <BadWord Word=\"n!gger\" />");
                    sw.WriteLine("        <BadWord Word=\"ass\" />");
                    sw.WriteLine("        <BadWord Word=\"cunt\" />");
                    sw.WriteLine("        <BadWord Word=\"faggit\" />");
                    sw.WriteLine("        <BadWord Word=\"piss\" />");
                    sw.WriteLine("        <BadWord Word=\"p!ss\" />");
                    sw.WriteLine("        <BadWord Word=\"fuck\" />");
                }
                sw.WriteLine("    </BadWords>");
                sw.WriteLine("</BadWordFilter>");
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
            LoadXml();
        }
    }
}