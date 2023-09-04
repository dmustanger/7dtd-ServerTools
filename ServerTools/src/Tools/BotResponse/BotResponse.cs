using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace ServerTools
{
    class BotResponse
    {
        public static bool IsEnabled = false, IsRunning = false;

        public static Dictionary<string, string[]> Dict = new Dictionary<string, string[]>();

        private const string file = "BotResponse.xml";
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
                    Log.Error("[SERVERTOOLS] Failed loading {0}: {1}", file, e.Message);
                    return;
                }
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                if (childNodes != null && childNodes[0] != null && childNodes[0].OuterXml.Contains("Version") && childNodes[0].OuterXml.Contains(Config.Version))
                {
                    Dict.Clear();
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        if (childNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement line = (XmlElement)childNodes[i];
                        if (!line.HasAttributes)
                        {
                            continue;
                        }
                        if (line.HasAttribute("Message") && line.HasAttribute("Response") && line.HasAttribute("Exact") && line.HasAttribute("Whisper"))
                        {
                            string message = line.GetAttribute("Message").ToLower();
                            if (message == "")
                            {
                                continue;
                            }
                            string response = line.GetAttribute("Response");
                            if (!bool.TryParse(line.GetAttribute("Exact"), out bool exact))
                            {
                                continue;
                            }
                            if (!bool.TryParse(line.GetAttribute("Whisper"), out bool whisper))
                            {
                                continue;
                            }
                            string[] values = { response, exact.ToString().ToLower(), whisper.ToString().ToLower() };
                            if (!Dict.ContainsKey(message))
                            {
                                Dict.Add(message, values);
                            }
                        }
                    }
                }
                else
                {
                    XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                    if (nodeList != null)
                    {
                        Timers.UpgradeBotResponseXml(nodeList);
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
                    Log.Out("[SERVERTOOLS] Error in BotResponse.LoadXml: {0}", e.Message);
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
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<BotResponse>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- <Chat Message=\"Any admin on\" Response=\"From the skies comes a bolt of lightning\" /> -->");
                    if (Dict.Count > 0)
                    {
                        foreach (KeyValuePair<string, string[]> kvp in Dict)
                        {
                            sw.WriteLine(string.Format("    <Chat Message=\"{0}\" Response=\"{1}\" Exact=\"{2}\" Whisper=\"{3}\" />", kvp.Key, kvp.Value[0], kvp.Value[1], kvp.Value[2]));
                        }
                    }
                    sw.WriteLine("</BotResponse>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in BotResponse.UpdateXml: {0}", e.Message);
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
                File.Delete(FilePath);
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<BotResponse>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- <Chat Message=\"Any admin on\" Response=\"From the skies comes a bolt of lightning\" /> -->");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType == XmlNodeType.Comment)
                        {
                            if (!nodeList[i].OuterXml.Contains("<!-- <Chat Message=\"Any admin on\"") && !nodeList[i].OuterXml.Contains("<!-- Do not forget") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Version") && !nodeList[i].OuterXml.Contains("<Chat Message=\"\""))
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
                            if (line.HasAttributes && line.Name == "Chat")
                            {
                                string message = "", response = "", exact = "", whisper = "";
                                if (line.HasAttribute("Message"))
                                {
                                    message = line.GetAttribute("Message");
                                }
                                if (line.HasAttribute("Response"))
                                {
                                    response = line.GetAttribute("Response");
                                }
                                if (line.HasAttribute("Exact"))
                                {
                                    exact = line.GetAttribute("Exact");
                                }
                                if (line.HasAttribute("Whisper"))
                                {
                                    whisper = line.GetAttribute("Whisper");
                                }
                                sw.WriteLine(string.Format("    <Chat Message=\"{0}\" Response=\"{1}\" Exact=\"{2}\" Whisper=\"{3}\" />", message, response, exact, whisper));
                            }
                        }
                    }
                    sw.WriteLine("</BotResponse>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in BotResponse.UpgradeXml: {0}", e.Message);
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}
