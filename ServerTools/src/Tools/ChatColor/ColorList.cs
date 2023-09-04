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
                    Log.Out(string.Format("Failed loading {0}: {1}", FilePath, e.Message));
                    return;
                }
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                Colors.Clear();
                if (childNodes != null && childNodes[0] != null && childNodes[0].OuterXml.Contains("Version") && childNodes[0].OuterXml.Contains(Config.Version))
                {
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        if (childNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement line = (XmlElement)childNodes[i];
                        if (!line.HasAttributes || !line.HasAttribute("Name") || !line.HasAttribute("Tags"))
                        {
                            continue;
                        }
                        string name = line.GetAttribute("Name").ToLower();
                        if (name == "")
                        {
                            continue;
                        }
                        string tags = line.GetAttribute("Tags").ToUpper();
                        if (!tags.Contains("[") || !tags.Contains("]"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ColorList.xml entry with missing brackets[] around the html color tag '{0}'", line.OuterXml));
                            continue;
                        }
                        if (!Colors.ContainsKey(name))
                        {
                            Colors.Add(name, tags);
                        }
                    }
                    ChatColor.LoadXml();
                    return;
                }
                else
                {
                    XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                    if (nodeList != null)
                    {
                        Timers.UpgradeColorListXml(nodeList);
                        //UpgradeXml(nodeList);
                        return;
                    }
                    File.Delete(FilePath);
                    UpdateXml();
                    return;
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
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- <Color Name=\"Red\" Tags=\"[FF0000]\" /> -->");
                    sw.WriteLine("    <!-- <Color Name=\"Rainbow\" Tags=\"[FF0000],[FF9933],[FFFF00],[009933],[0000CC],[9900CC],[FF33CC]\" /> -->");
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

        public static void UpgradeXml(XmlNodeList nodeList)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                File.Delete(FilePath);
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<Colors>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- <Color Name=\"Red\" Tags=\"[FF0000]\" /> -->");
                    sw.WriteLine("    <!-- <Color Name=\"Rainbow\" Tags=\"[FF0000],[FF9933],[FFFF00],[009933],[0000CC],[9900CC],[FF33CC]\" /> -->");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType == XmlNodeType.Comment)
                        {
                            if (!nodeList[i].OuterXml.Contains("<!-- <Color Name=\"Red\"") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Color Name=\"Rainbow\"") && !nodeList[i].OuterXml.Contains("<!-- <Color Name=\"\"") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Version"))
                            {
                                sw.WriteLine(nodeList[i].OuterXml);
                            }
                        }
                    }
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)nodeList[i];
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
