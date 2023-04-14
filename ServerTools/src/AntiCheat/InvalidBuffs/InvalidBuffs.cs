using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    class InvalidBuffs
    {
        public static bool IsEnabled = false, IsRunning = false;

        public static List<string> InvalidBuffList = new List<string>();

        private static string detectionFile = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string detectionFilepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, detectionFile);
        private const string file = "InvalidBuffs.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        public static void Unload()
        {
            InvalidBuffList.Clear();
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
                InvalidBuffList.Clear();
                if (childNodes != null && childNodes[0] != null && childNodes[0].OuterXml.Contains("Version") && childNodes[0].OuterXml.Contains(Config.Version))
                {
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
                        if (line.HasAttribute("Buff"))
                        {
                            string buff = line.GetAttribute("Buff");

                            if (!InvalidBuffList.Contains(buff))
                            {
                                InvalidBuffList.Add(buff);
                            }
                        }
                    }
                }
                else
                {
                    XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                    if (nodeList != null)
                    {
                        File.Delete(FilePath);
                        UpgradeXml(nodeList);
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
                    Log.Out(string.Format("[SERVERTOOLS] Error in InvalidBuffs.LoadXml: {0}", e.Message));
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
                    sw.WriteLine("<InvalidBuffs>");
                    sw.WriteLine(string.Format("    <!-- <Version=\"{0}\" /> -->", Config.Version));
                    sw.WriteLine("    <!-- <Invalid Buff=\"godmode\" /> -->");
                    sw.WriteLine("    <!-- <Invalid Buff=\"twitch_immortal\" /> -->");
                    sw.WriteLine("    <!-- <Invalid Buff=\"twitch_tough1\" /> -->");
                    sw.WriteLine("    <!-- <Invalid Buff=\"buffVolatileAura\" /> -->");
                    sw.WriteLine("    <Invalid Buff=\"\" />");
                    if (InvalidBuffList.Count > 0)
                    {
                        for (int i = 0; i < InvalidBuffList.Count; i++)
                        {
                            sw.WriteLine(string.Format("    <Invalid Buff=\"{0}\" />", InvalidBuffList[i]));
                        }
                    }
                    sw.WriteLine("</InvalidBuffs>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in InvalidBuffs.UpdateXml: {0}", e.Message));
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

        public static void CheckForInvalidBuffs(ClientInfo _cInfo, EntityPlayer _player)
        {
            if (InvalidBuffList.Count < 1)
            {
                return;
            }
            for (int i = 0; i < InvalidBuffList.Count; i++)
            {
                if (_player.Buffs.HasBuff(InvalidBuffList[i]))
                {
                    SdtdConsole.Instance.ExecuteSync(string.Format("debuffPlayer {0} {1}", _cInfo.CrossplatformId.CombinedString, InvalidBuffList[i]), null);
                    
                    using (StreamWriter sw = new StreamWriter(detectionFilepath, true, Encoding.UTF8))
                    {
                        sw.WriteLine(string.Format("Detected id '{0}' '{1}' named '{2}' using '{3}' buff @ '{4}'", _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, InvalidBuffList[i], _player.position));
                        sw.WriteLine();
                        sw.Flush();
                        sw.Close();
                    }
                    Log.Warning(string.Format("[SERVERTOOLS] Detected id '{0}' '{1}' named '{2}' using '{3}' buff @ '{4}'", _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, InvalidBuffList[i], _player.position));
                }
            }
        }

        private static void UpgradeXml(XmlNodeList nodeList)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<InvalidBuffs>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- <Invalid Buff=\"godmode\" /> -->");
                    sw.WriteLine("    <!-- <Invalid Buff=\"twitch_immortal\" /> -->");
                    sw.WriteLine("    <!-- <Invalid Buff=\"twitch_tough1\" /> -->");
                    sw.WriteLine("    <!-- <Invalid Buff=\"buffVolatileAura\" /> -->");
                    sw.WriteLine("    <Invalid Buff=\"\" />");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType == XmlNodeType.Comment && !nodeList[i].OuterXml.Contains("<!-- <Invalid Buff=\"godmode\"") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Invalid Buff=\"twitch_immortal\"") && !nodeList[i].OuterXml.Contains("<!-- <Invalid Buff=\"twitch_tough1\"") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Invalid Buff=\"buffVolatileAura\"") && !nodeList[i].OuterXml.Contains("<!-- <Invalid Buff=\"\"") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Version"))
                        {
                            sw.WriteLine(nodeList[i].OuterXml);
                        }
                    }
                    sw.WriteLine("    <Invalid Buff=\"\" />");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)nodeList[i];
                            if (line.HasAttributes && line.Name == "Invalid")
                            {
                                string buff = "";
                                if (line.HasAttribute("Buff"))
                                {
                                    buff = line.GetAttribute("Buff");
                                }
                                sw.WriteLine(string.Format("    <Invalid Buff=\"{0}\" />", buff));
                            }
                        }
                    }
                    sw.WriteLine("</InvalidBuffs>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in InvalidBuffs.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}
