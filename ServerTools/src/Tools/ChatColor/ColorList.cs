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
                XmlNodeList _childNodes = xmlDoc.DocumentElement.ChildNodes;
                if (_childNodes != null && _childNodes.Count > 0)
                {
                    Colors.Clear();
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
                            else if (_line.HasAttribute("Name") && _line.HasAttribute("Tags"))
                            {
                                if (string.IsNullOrWhiteSpace(_line.Attributes[0].Value) || string.IsNullOrWhiteSpace(_line.Attributes[1].Value))
                                {
                                    Log.Out(string.Format("[SERVERTOOLS] Ignoring ChatColors.xml entry: {0}", _line.OuterXml));
                                    continue;
                                }
                                else
                                {
                                    string _name = _line.GetAttribute("Name");
                                    string _tags = _line.GetAttribute("Tags");
                                    if (!_tags.Contains("[") || !_tags.Contains("]"))
                                    {
                                        Log.Out(string.Format("[SERVERTOOLS] Ignoring ChatColors.xml entry with missing brackets[] around the Tags: {0}", _line.OuterXml));
                                        continue;
                                    }
                                    if (!Colors.ContainsKey(_name))
                                    {
                                        Colors.Add(_name, _tags);
                                    }
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
                        sw.WriteLine("  <Color Name=\"Red\" Tags=\"[FF0000]\" />");
                        sw.WriteLine("  <Color Name=\"Orange\" Tags=\"[FF9933]\" />");
                        sw.WriteLine("  <Color Name=\"Yellow\" Tags=\"[FFFF00]\" />");
                        sw.WriteLine("  <Color Name=\"Green\" Tags=\"[009933]\" />");
                        sw.WriteLine("  <Color Name=\"Blue\" Tags=\"[0000CC]\" />");
                        sw.WriteLine("  <Color Name=\"Purple\" Tags=\"[9900CC]\" />");
                        sw.WriteLine("  <Color Name=\"Pink\" Tags=\"[FF33CC]\" />");
                        sw.WriteLine("  <Color Name=\"Brown\" Tags=\"[4D0000]\" />");
                        sw.WriteLine("  <Color Name=\"Grey\" Tags=\"[696969]\" />");
                        sw.WriteLine("  <Color Name=\"Teal\" Tags=\"[33CCFF]\" />");
                        sw.WriteLine("  <Color Name=\"Mustard\" Tags=\"[CCCC00]\" />");
                        sw.WriteLine("  <Color Name=\"Forest\" Tags=\"[339933]\" />");
                        sw.WriteLine("  <Color Name=\"Aqua\" Tags=\"[3399FF]\" />");
                        sw.WriteLine("  <Color Name=\"Fire\" Tags=\"[F66302]\" />");
                        sw.WriteLine("  <Color Name=\"Rainbow\" Tags=\"[FF0000],[FF9933],[FFFF00],[009933],[0000CC],[9900CC],[FF33CC]\" />");
                        sw.WriteLine("  <Color Name=\"R-O\" Tags=\"[FF0000],[FF4300],[FF6100],[FF8700],[FFFF00],[009933]\" />");
                        sw.WriteLine("  <Color Name=\"R-Y\" Tags=\"[FF0000],[FF4300],[FF6100],[FF8700],[FFB000],[FFD500],[FFFF00]\" />");
                        sw.WriteLine("  <Color Name=\"Y-G\" Tags=\"[FFFF00],[AEEC00],[5FD600],[12BF00],[00AF11],[009933]\" />");
                        sw.WriteLine("  <Color Name=\"G-B\" Tags=\"[009933],[00A792],[0085B6],[0058C3],[0000CC]\" />");
                        sw.WriteLine("  <Color Name=\"W-R\" Tags=\"[FFFFFF],[FFC2C2],[FF8585],[FF4242],[FF0000]\" />");
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
                    sw.WriteLine();
                    sw.WriteLine();
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement _line = (XmlElement)_oldChildNodes[i];
                        if (_line.HasAttributes && _line.Name == "Color")
                        {
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
