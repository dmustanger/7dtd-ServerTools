using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    class ColorList
    {
        public static Dictionary<string, string> Colors = new Dictionary<string, string>();

        private const string file = "ColorList.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        public static void Unload()
        {
            Colors.Clear();
            FileWatcher.Dispose();
        }

        public static void LoadXml()
        {
            try
            {
                if (!File.Exists(FilePath))
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
                    Log.Out(string.Format("{0} Failed loading {1}: {2}", DateTime.Now, FilePath, e.Message));
                    return;
                }
                bool upgrade = true;
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                if (childNodes != null && childNodes.Count > 0)
                {
                    Colors.Clear();
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        if (childNodes[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement _line = (XmlElement)childNodes[i];
                            if (_line.HasAttributes)
                            {
                                if (_line.HasAttribute("Version") && _line.GetAttribute("Version") == Config.Version)
                                {
                                    upgrade = false;
                                    continue;
                                }
                                else if (_line.HasAttribute("Name") && _line.HasAttribute("Tags"))
                                {
                                    if (string.IsNullOrWhiteSpace(_line.Attributes[0].Value) || string.IsNullOrWhiteSpace(_line.Attributes[1].Value))
                                    {
                                        Log.Out(string.Format("[SERVERTOOLS] Ignoring ColorList.xml entry: {0}", _line.OuterXml));
                                        continue;
                                    }
                                    else
                                    {
                                        string name = _line.GetAttribute("Name");
                                        string tags = _line.GetAttribute("Tags");
                                        if (!tags.Contains("[") || !tags.Contains("]"))
                                        {
                                            Log.Out(string.Format("[SERVERTOOLS] Ignoring ColorList.xml entry with missing brackets[] around the Tags: {0}", _line.OuterXml));
                                            continue;
                                        }
                                        if (!Colors.ContainsKey(name))
                                        {
                                            Colors.Add(name, tags);
                                        }
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
                Log.Out(string.Format("[SERVERTOOLS] Error in ColorList.LoadXml: {0}", e.Message));
            }
        }

        public static void UpdateXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<Colors>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("<!-- <Color Name=\"Red\" Tags=\"[FF0000]\" /> -->");
                    sw.WriteLine("<!-- <Color Name=\"Rainbow\" Tags=\"[FF0000],[FF9933],[FFFF00],[009933],[0000CC],[9900CC],[FF33CC]\" /> -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    if (Colors.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> kvp in Colors)
                        {
                            sw.WriteLine(string.Format("    <Color Name=\"{0}\" Tags=\"{1}\" />", kvp.Key, kvp.Value));
                        }
                    }
                    else
                    {
                        sw.WriteLine("    <!-- <Color Name=\"\" Tags=\"\" /> -->");
                    }
                    sw.WriteLine("</Colors>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ColorList.UpdateXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
        }

        private static void InitFileWatcher()
        {
            FileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.Created += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.EnableRaisingEvents = true;
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
                    sw.WriteLine("<Colors>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("<!-- <Color Name=\"Red\" Tags=\"[FF0000]\" /> -->");
                    sw.WriteLine("<!-- <Color Name=\"Rainbow\" Tags=\"[FF0000],[FF9933],[FFFF00],[009933],[0000CC],[9900CC],[FF33CC]\" /> -->");
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType == XmlNodeType.Comment && !_oldChildNodes[i].OuterXml.StartsWith("<!-- <Color Name=\"Red\"") &&
                            !_oldChildNodes[i].OuterXml.StartsWith("<!-- <Color Name=\"Rainbow\"") && !_oldChildNodes[i].OuterXml.StartsWith("    <!-- <Color Name=\"\""))
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
                            if (_line.HasAttributes && _line.Name == "Color")
                            {
                                _blank = false;
                                string _name = "", _tags = "";
                                DateTime _dateTime = DateTime.Now;
                                if (_line.HasAttribute("Name"))
                                {
                                    _name = _line.GetAttribute("Name");
                                }
                                if (_line.HasAttribute("Tags"))
                                {
                                    _tags = _line.GetAttribute("Tags");
                                }
                                sw.WriteLine(string.Format("    <Color Name=\"{0}\" Tags=\"{1}\" />", _name, _tags));
                            }
                        }
                    }
                    if (_blank)
                    {
                        sw.WriteLine("    <!-- <Color Name=\"\" Tags=\"\" /> -->");
                    }
                    sw.WriteLine("</Colors>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ChatColor.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}
