using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    public class Badwords
    {
        public static bool IsEnabled = false;
        public static bool IsRunning = false;
        public static bool Invalid_Name = false;
        private const string file = "BadWords.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        public static List<string> Words = new List<string>();
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
            Words.Clear();
            FileWatcher.Dispose();
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
                    Words.Clear();
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'BadWords.xml' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("Word"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Bad_Word_Filter entry because of missing a Word attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string _word = _line.GetAttribute("Word");
                        _word = _word.ToLower();
                        if (!Words.Contains(_word))
                        {
                            Words.Add(_word);
                        }
                    }
                }
            }
        }

        private static void UpdateXml()
        {
            FileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<BadWordFilter>");
                sw.WriteLine("    <BadWords>");
                if (Words.Count > 0)
                {
                    for (int i = 0; i < Words.Count; i++)
                    {
                        sw.WriteLine(string.Format("        <Bad Word=\"{0}\" />", Words[i]));
                    }
                }
                else
                {
                    sw.WriteLine("        <Bad Word=\"nigger\" />");
                    sw.WriteLine("        <Bad Word=\"n!gger\" />");
                    sw.WriteLine("        <Bad Word=\"ass\" />");
                    sw.WriteLine("        <Bad Word=\"cunt\" />");
                    sw.WriteLine("        <Bad Word=\"faggit\" />");
                    sw.WriteLine("        <Bad Word=\"trannysaurus\" />");
                    sw.WriteLine("        <Bad Word=\"cracker\" />");
                    sw.WriteLine("        <Bad Word=\"cr@cker\" />");
                    sw.WriteLine("        <Bad Word=\"fuck\" />");
                    sw.WriteLine("        <Bad Word=\"shit\" />");
                }
                sw.WriteLine("    </BadWords>");
                sw.WriteLine("</BadWordFilter>");
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