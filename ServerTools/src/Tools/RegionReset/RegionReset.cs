using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    public class RegionReset
    {
        public static bool IsEnabled = false, IsRunning = false;

        public static Dictionary<string, string> Regions = new Dictionary<string, string>();
        public static List<Bounds> RegionBounds = new List<Bounds>();
        public static List<int> RegionPlayer = new List<int>();

        private const string file = "RegionReset.xml";
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
            Regions.Clear();
            RegionBounds.Clear();
            RegionPlayer.Clear();
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
                bool upgrade = true;
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                if (childNodes != null)
                {
                    Regions.Clear();
                    RegionBounds.Clear();
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
                                else if (line.HasAttribute("Name") && line.HasAttribute("Time"))
                                {
                                    string name = line.GetAttribute("Name");
                                    name = name.Replace("r.", "");
                                    name = name.Replace(".7rg", "");
                                    if (!name.Contains("."))
                                    {
                                        Log.Out(string.Format("[SERVERTOOLS] Region reset name is invalid: {0}", name));
                                        continue;
                                    }
                                    string time = line.GetAttribute("Time").ToLower();
                                    if (!time.Contains("day") && !time.Contains("week") && !time.Contains("month"))
                                    {
                                        Log.Out(string.Format("[SERVERTOOLS] Region reset time is invalid: {0}", time));
                                        continue;
                                    }
                                    string[] nameSplit = name.Split('.');
                                    if (!int.TryParse(nameSplit[0], out int value1))
                                    {
                                        Log.Out(string.Format("[SERVERTOOLS] Region reset name is invalid: {0}", name));
                                        continue;
                                    }
                                    if (!int.TryParse(nameSplit[1], out int value2))
                                    {
                                        Log.Out(string.Format("[SERVERTOOLS] Region reset name is invalid: {0}", name));
                                        continue;
                                    }
                                    int minX = value1 * 512;
                                    int maxX;
                                    if (value1 < 0)
                                    {
                                        maxX = minX - 512;
                                    }
                                    else
                                    {
                                        maxX = minX + 512;
                                    }
                                    int minZ = value2 * 512;
                                    int maxZ;
                                    if (value2 < 0)
                                    {
                                        maxZ = minZ - 512;
                                    }
                                    else
                                    {
                                        maxZ = minZ + 512;
                                    }
                                    Bounds regionBounds = new Bounds();
                                    regionBounds.SetMinMax(new Vector3(minX, 0, minZ), new Vector3(maxX, 200, maxZ));
                                    if (!Regions.ContainsKey(name))
                                    {
                                        Regions.Add(name, time);
                                        RegionBounds.Add(regionBounds);
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
                            Log.Out(string.Format("[SERVERTOOLS] The existing RegionReset.xml was too old or misconfigured. File deleted and rebuilt for version {0}", Config.Version));
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
                    Log.Out(string.Format("[SERVERTOOLS] Error in RegionReset.LoadXml: {0}", e.Message));
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
                    sw.WriteLine("<RegionReset>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("    <!-- Possible time: day, week, month -->");
                    sw.WriteLine("    <!-- <Region Name=\"r.0.0.7rg\" Time=\"day\" /> -->");
                    sw.WriteLine("    <!-- <Region Name=\"r.-1.-1.7rg\" Time=\"week\" /> -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    if (Regions.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> kvp in Regions)
                        {
                            sw.WriteLine(string.Format("    <Region Name=\"{0}\" Time=\"{1}\" />", kvp.Key, kvp.Value));
                        }
                    }
                    sw.WriteLine("</RegionReset>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in RegionReset.UpdateXml: {0}", e.Message));
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

        public static void IsRegenRegion(ClientInfo _cInfo, EntityPlayer _player)
        {
            for (int i = 0; i < RegionBounds.Count; i++)
            {
                if (RegionBounds[i].Contains(_player.position))
                {
                    if (!RegionPlayer.Contains(_player.entityId))
                    {
                        RegionPlayer.Add(_player.entityId);
                        SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("buffplayer {0} {1}", _cInfo.CrossplatformId.CombinedString, "region_reset"), null);
                    }
                    return;
                }
            }
            if (RegionPlayer.Contains(_player.entityId))
            {
                RegionPlayer.Remove(_player.entityId);
                SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("debuffplayer {0} {1}", _cInfo.CrossplatformId.CombinedString, "region_reset"), null);
            }
        }
        
        public static void Exec()
        {
            try
            {
                if (Regions.Count > 0)
                {
                    int count = 0;
                    foreach (var region in Regions)
                    {
                        if (!PersistentContainer.Instance.RegionReset.ContainsKey(region.Key))
                        {
                            Bounds bounds = RegionBounds[count];
                            SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("chunkreset {0} {1} {2} {3}", bounds.min.x, bounds.min.z, bounds.max.x, bounds.max.z), null);
                            PersistentContainer.Instance.RegionReset.Add(region.Key, DateTime.Now);
                            PersistentContainer.DataChange = true;
                        }
                        else
                        {
                            PersistentContainer.Instance.RegionReset.TryGetValue(region.Key, out DateTime lastReset);
                            if (region.Value == "day" && DateTime.Now.AddDays(1) >= lastReset)
                            {
                                Bounds bounds = RegionBounds[count];
                                SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("chunkreset {0} {1} {2} {3}", bounds.min.x, bounds.min.z, bounds.max.x, bounds.max.z), null);
                                PersistentContainer.Instance.RegionReset[region.Key] = DateTime.Now;
                                PersistentContainer.DataChange = true;
                            }
                            else if (region.Value == "week" && DateTime.Now.AddDays(7) >= lastReset)
                            {
                                Bounds bounds = RegionBounds[count];
                                SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("chunkreset {0} {1} {2} {3}", bounds.min.x, bounds.min.z, bounds.max.x, bounds.max.z), null);
                                PersistentContainer.Instance.RegionReset[region.Key] = DateTime.Now;
                                PersistentContainer.DataChange = true;
                            }
                            else if (region.Value == "month" && DateTime.Now.AddMonths(1) >= lastReset)
                            {
                                Bounds bounds = RegionBounds[count];
                                SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("chunkreset {0} {1} {2} {3}", bounds.min.x, bounds.min.z, bounds.max.x, bounds.max.z), null);
                                PersistentContainer.Instance.RegionReset[region.Key] = DateTime.Now;
                                PersistentContainer.DataChange = true;
                            }
                        }
                        count += 1;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in RegionReset.Exec: {0}", e.Message));
            }
        }
        
        private static void UpgradeXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<RegionReset>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("    <!-- Possible time: day, week, month -->");
                    sw.WriteLine("    <!-- <Region Name=\"r.0.0.7rg\" Time=\"day\" /> -->");
                    sw.WriteLine("    <!-- <Region Name=\"r.-1.-1.7rg\" Time=\"week\" /> -->");
                    for (int i = 0; i < OldNodeList.Count; i++)
                    {
                        if (OldNodeList[i].NodeType == XmlNodeType.Comment && !OldNodeList[i].OuterXml.Contains("<!-- <Region Name=\"r.0.0.7rg\"") &&
                            !OldNodeList[i].OuterXml.Contains("<!-- <Region Name=\"r.-1.-1.7rg\"") && 
                            !OldNodeList[i].OuterXml.Contains("<!-- <Region Name=\"\"") &&
                            !OldNodeList[i].OuterXml.Contains("<!-- Possible time"))
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
                            if (line.HasAttributes && line.Name == "Region")
                            {
                                string name = "", time = "";
                                if (line.HasAttribute("Name"))
                                {
                                    name = line.GetAttribute("Name");
                                }
                                if (line.HasAttribute("Time"))
                                {
                                    time = line.GetAttribute("Time");
                                }
                                sw.WriteLine(string.Format("    <Region Name=\"{0}\" Time=\"{1}\" />", name, time));
                            }
                        }
                    }
                    sw.WriteLine("</RegionReset>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in RegionReset.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}
