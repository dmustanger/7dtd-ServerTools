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

        private static XmlNodeList OldNodeList;

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
                if (childNodes != null)
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
                if (upgrade)
                {
                    XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                    XmlNode node = nodeList[0];
                    XmlElement line = (XmlElement)nodeList[0];
                    if (line != null)
                    {
                        if (line.HasAttributes)
                        {
                            OldNodeList = nodeList;
                            File.Delete(FilePath);
                            UpgradeXml();
                            return;
                        }
                        else
                        {
                            nodeList = node.ChildNodes;
                            line = (XmlElement)nodeList[0];
                            if (line != null)
                            {
                                if (line.HasAttributes)
                                {
                                    OldNodeList = nodeList;
                                    File.Delete(FilePath);
                                    UpgradeXml();
                                    return;
                                }
                            }
                            File.Delete(FilePath);
                            UpdateXml();
                            Log.Out(string.Format("[SERVERTOOLS] The existing ColorList.xml was too old or misconfigured. File deleted and rebuilt for version {0}", Config.Version));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message == "Specified cast is not valid.")
                {
                    File.Delete(FilePath);
                    UpdateXml();
                }
                else
                {
                    Log.Out(string.Format("[SERVERTOOLS] Error in ColorList.LoadXml: {0}", e.Message));
                }
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
                    sw.WriteLine("    <!-- <Color Name=\"Red\" Tags=\"[FF0000]\" /> -->");
                    sw.WriteLine("    <!-- <Color Name=\"Rainbow\" Tags=\"[FF0000],[FF9933],[FFFF00],[009933],[0000CC],[9900CC],[FF33CC]\" /> -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    if (Colors.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> kvp in Colors)
                        {
                            sw.WriteLine(string.Format("    <Color Name=\"{0}\" Tags=\"{1}\" />", kvp.Key, kvp.Value));
                        }
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
            if (!File.Exists(FilePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        private static void UpgradeXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<Colors>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("    <!-- <Color Name=\"Red\" Tags=\"[FF0000]\" /> -->");
                    sw.WriteLine("    <!-- <Color Name=\"Rainbow\" Tags=\"[FF0000],[FF9933],[FFFF00],[009933],[0000CC],[9900CC],[FF33CC]\" /> -->");
                    for (int i = 0; i < OldNodeList.Count; i++)
                    {
                        if (OldNodeList[i].NodeType == XmlNodeType.Comment && !OldNodeList[i].OuterXml.Contains("<!-- <Color Name=\"Red\"") &&
                            !OldNodeList[i].OuterXml.Contains("<!-- <Color Name=\"Rainbow\"") && !OldNodeList[i].OuterXml.Contains("    <!-- <Color Name=\"\""))
                        {
                            sw.WriteLine(OldNodeList[i].OuterXml);
                        }
                    }
                    sw.WriteLine();
                    sw.WriteLine();
                    for (int i = 0; i < OldNodeList.Count; i++)
                    {
                        if (OldNodeList[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)OldNodeList[i];
                            if (line.HasAttributes && line.Name == "Color")
                            {
                                string name = "", tags = "";
                                DateTime dateTime = DateTime.Now;
                                if (line.HasAttribute("Name"))
                                {
                                    name = line.GetAttribute("Name");
                                }
                                if (line.HasAttribute("Tags"))
                                {
                                    tags = line.GetAttribute("Tags");
                                }
                                sw.WriteLine(string.Format("    <Color Name=\"{0}\" Tags=\"{1}\" />", name, tags));
                            }
                        }
                    }
                    sw.WriteLine("</Colors>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ColorList.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}
