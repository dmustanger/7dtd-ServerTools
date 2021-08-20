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
                XmlNodeList _childNodes = xmlDoc.DocumentElement.ChildNodes;
                if (_childNodes != null && _childNodes.Count > 0)
                {
                    Dict.Clear();
                    bool upgrade = true;
                    for (int i = 0; i < _childNodes.Count; i++)
                    {
                        if (_childNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement _line = (XmlElement)_childNodes[i];
                        if (_line.HasAttributes)
                        {
                            if (_line.HasAttribute("Version") && _line.GetAttribute("Version") == Config.Version)
                            {
                                upgrade = false;
                            }
                            else if (_line.HasAttribute("Word"))
                            {
                                string _word = _line.GetAttribute("Word");
                                _word = _word.ToLower();
                                if (!Dict.Contains(_word))
                                {
                                    Dict.Add(_word);
                                }
                            }
                        }
                    }
                    if (upgrade)
                    {
                        UpgradeXml(_childNodes);
                        return;
                    }
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
                        sw.WriteLine("    <Bad Word=\"nigger\" />");
                        sw.WriteLine("    <Bad Word=\"n!gger\" />");
                        sw.WriteLine("    <Bad Word=\"ass\" />");
                        sw.WriteLine("    <Bad Word=\"cunt\" />");
                        sw.WriteLine("    <Bad Word=\"trannysaurus\" />");
                        sw.WriteLine("    <Bad Word=\"cracker\" />");
                        sw.WriteLine("    <Bad Word=\"cr@cker\" />");
                        sw.WriteLine("    <Bad Word=\"fuck\" />");
                        sw.WriteLine("    <Bad Word=\"shit\" />");
                        sw.WriteLine("    <Bad Word=\"chink\" />");
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
                    sw.WriteLine();
                    sw.WriteLine();
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement _line = (XmlElement)_oldChildNodes[i];
                        if (_line.HasAttributes && _line.Name == "Bad")
                        {
                            string _word = "";
                            if (_line.HasAttribute("Word"))
                            {
                                _word = _line.GetAttribute("Word");
                            }
                            sw.WriteLine(string.Format("    <Bad Word=\"{0}\" />", _word));
                        }
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