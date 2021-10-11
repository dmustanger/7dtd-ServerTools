using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    public class Badwords
    {
        public static bool IsEnabled = false, IsRunning = false, Invalid_Name = false;
        public static List<string> Dict = new List<string>();

        private const string file = "BadWords.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        public static void Unload()
        {
            Dict.Clear();
            FileWatcher.Dispose();
            IsRunning = false;
        }

        private static void LoadXml()
        {
            try
            {
                if (!Utils.FileExists(FilePath))
                {
                    UpdateXml();
                }
                XmlDocument xmlDoc = new XmlDocument();
                try
                {
                    xmlDoc.Load(FilePath);
                }
                catch (XmlException e)
                {
                    Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", file, e.Message));
                    return;
                }
                bool upgrade = true;
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                if (childNodes != null && childNodes.Count > 0)
                {
                    Dict.Clear();
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        if (childNodes[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)childNodes[i];
                            if (line.HasAttributes)
                            {
                                if (line.HasAttribute("Version") && line.GetAttribute("Version") == Config.Version)
                                {
                                    upgrade = false;
                                    continue;
                                }
                                else if (line.HasAttribute("Word"))
                                {
                                    string word = line.GetAttribute("Word");
                                    word = word.ToLower();
                                    if (!Dict.Contains(word))
                                    {
                                        Dict.Add(word);
                                    }
                                }
                            }
                        }
                    }
                }
                if (childNodes != null && upgrade)
                {
                    UpgradeXml(childNodes);
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Badwords.LoadXml: {0}", e.Message));
            }
        }

        private static void UpdateXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<BadWordFilter>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("<!-- <Bad Word=\"cracker\" /> -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    if (Dict.Count > 0)
                    {
                        for (int i = 0; i < Dict.Count; i++)
                        {
                            sw.WriteLine(string.Format("    <Bad Word=\"{0}\" />", Dict[i]));
                        }
                    }
                    else
                    {
                        sw.WriteLine("    <!-- <Bad Word=\"\" /> -->");
                    }
                    sw.WriteLine("</BadWordFilter>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Badwords.UpdateXml: {0}", e.Message));
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
            if (!Utils.FileExists(FilePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        private static void UpgradeXml(XmlNodeList _oldChildNodes)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<BadWordFilter>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("<!-- <Bad Word=\"cracker\" /> -->");
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType == XmlNodeType.Comment && !_oldChildNodes[i].OuterXml.StartsWith("<!-- <Bad Word=\"cracker\"") &&
                            !_oldChildNodes[i].OuterXml.StartsWith("    <!-- <Bad Word=\"\""))
                        {
                            sw.WriteLine(_oldChildNodes[i].OuterXml);
                        }
                    }
                    sw.WriteLine();
                    sw.WriteLine();
                    bool _blank = true;
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement _line = (XmlElement)_oldChildNodes[i];
                            if (_line.HasAttributes && _line.Name == "Bad")
                            {
                                _blank = false;
                                string _word = "";
                                if (_line.HasAttribute("Word"))
                                {
                                    _word = _line.GetAttribute("Word");
                                }
                                sw.WriteLine(string.Format("    <Bad Word=\"{0}\" />", _word));
                            }
                        }
                    }
                    if (_blank)
                    {
                        sw.WriteLine("    <!-- <Bad Word=\"\" /> -->");
                    }
                    sw.WriteLine("</BadWordFilter>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Badwords.UpgradeXml: {0}", e.Message));

            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}