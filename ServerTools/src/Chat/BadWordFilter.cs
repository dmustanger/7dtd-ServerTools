using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    public class Badwords
    {
        public static bool IsEnabled = false;
        private static SortedDictionary<string, string> _badWords = new SortedDictionary<string, string>();
        private static string _file = "BadWords.xml";
        private static string _filepath = string.Format("{0}/{1}", Config._configpath, _file);
        public static FileSystemWatcher _fileWatcher = new FileSystemWatcher(Config._configpath, _file);
        public static bool IsRunning = false;

        public static List<string> BadWordslist
        {
            get { return new List<string>(_badWords.Keys); }
        }

        public static void Init()
        {
            if (IsEnabled && !IsRunning)
            {
                if (!Utils.FileExists(_filepath))
                {
                    UpdateXml();
                }
                LoadBadWords();
                InitFileWatcher();
                IsRunning = true;
            }
        }

        private static void UpdateXml()
        {
            _fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(_filepath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<BadWordFilter>");
                sw.WriteLine("    <BadWords>");
                if (BadWordslist.Count > 0)
                {
                    foreach (string _word in BadWordslist)
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
            _fileWatcher.EnableRaisingEvents = true;
        }

        private static void InitFileWatcher()
        {
            _fileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
            _fileWatcher.Created += new FileSystemEventHandler(OnFileChanged);
            _fileWatcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            _fileWatcher.EnableRaisingEvents = true;
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            if (!Utils.FileExists(_filepath))
            {
                UpdateXml();
            }
            LoadBadWords();
        }

        private static void LoadBadWords()
        {
            if (!Utils.FileExists(_filepath))
            {
                UpdateXml();
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
            XmlNode _BadWordsXml = xmlDoc.DocumentElement;
            _badWords.Clear();
            foreach (XmlNode childNode in _BadWordsXml.ChildNodes)
            {
                if (childNode.Name == "BadWords")
                {
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
                        if (!_badWords.ContainsKey(_word))
                        {
                            _badWords.Add(_word, null);
                        }
                    }
                }
            }
        }
    }
}