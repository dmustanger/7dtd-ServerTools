using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{

    class OutputLogBlocker
    {
        public static bool IsEnabled = false, IsRunning = false;

        public static List<string> Ommitted = new List<string>();

        private const string file = "OutputBlocker.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        public static void Unload()
        {
            Ommitted.Clear();
            FileWatcher.Dispose();
            IsRunning = false;
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
                    Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", file, e.Message));
                    return;
                }
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                Ommitted.Clear();
                if (childNodes != null && childNodes[0] != null && childNodes[0].OuterXml.Contains("Version") && childNodes[0].OuterXml.Contains(Config.Version))
                {
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        if (childNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement line = (XmlElement)childNodes[i];
                        if (!line.HasAttributes || !line.HasAttribute("Message"))
                        {
                            continue;
                        }
                        string message = line.GetAttribute("Message");
                        if (message == "")
                        {
                            continue;
                        }
                        if (!Ommitted.Contains(message))
                        {
                            Ommitted.Add(message);
                        }
                    }
                }
                else
                {
                    XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                    if (nodeList != null)
                    {
                        File.Delete(FilePath);
                        Timers.UpgradeOutputBlockerXml(nodeList);
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
                    Log.Out(string.Format("[SERVERTOOLS] Error in OutputLog.LoadXml: {0}", e.Message));
                }
            }
        }

        private static void UpdateXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<OutputLog>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Be very careful with your additions. Do not block entries you may need -->");
                    sw.WriteLine("    <!-- <Output Message=\"EntityBackpack\" /> -->");
                    sw.WriteLine("    <!-- <Output Message=\"Started thread\" /> -->");
                    sw.WriteLine("    <!-- <Output Message=\"Exited thread\" /> -->");
                    if (Ommitted.Count > 0)
                    {
                        for (int i = 0; i < Ommitted.Count; i++)
                        {
                            sw.WriteLine("    <Output Message=\"{0}\" />", Ommitted[i]);
                        }
                    }
                    sw.WriteLine("</OutputLog>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in OutputLog.UpdateXml: {0}", e.Message));
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
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<OutputLog>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Be very careful with your additions. Do not block entries you may need -->");
                    sw.WriteLine("    <!-- <Output Message=\"EntityBackpack\" /> -->");
                    sw.WriteLine("    <!-- <Output Message=\"Started thread\" /> -->");
                    sw.WriteLine("    <!-- <Output Message=\"Exited thread\" /> -->");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType == XmlNodeType.Comment)
                        {
                            if (!nodeList[i].OuterXml.Contains("Be very careful") && !nodeList[i].OuterXml.Contains("<Output Message=\"EntityBackpack\"") &&
                            !nodeList[i].OuterXml.Contains("<Output Message=\"Started thread\"") && !nodeList[i].OuterXml.Contains("<Output Message=\"Exited thread\"") &&
                            !nodeList[i].OuterXml.Contains("<Output Message=\"\""))
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
                            if (line.HasAttributes && line.Name == "Output")
                            {
                                string message = "";
                                if (line.HasAttribute("Message"))
                                {
                                    message = line.GetAttribute("Message");
                                }
                                sw.WriteLine(string.Format("    <Output Message=\"{0}\" />", message));
                            }
                        }
                    }
                    sw.WriteLine("</OutputLog>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in OutputLog.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}
