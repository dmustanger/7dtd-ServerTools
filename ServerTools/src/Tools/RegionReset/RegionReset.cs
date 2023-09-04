using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    public class RegionReset
    {
        public static bool IsEnabled = false, IsRunning = false;
        public static string Icon = "ui_game_symbol_brick";

        public static Dictionary<string, string> Regions = new Dictionary<string, string>();
        public static List<int[]> RegionBounds = new List<int[]>();
        public static List<int> RegionPlayer = new List<int>();
        
        private const string file = "RegionReset.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        
        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }
        
        public static void Unload()
        {
            Regions.Clear();
            RegionBounds.Clear();
            DisablePlayerBuff();
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
                    Log.Error("[SERVERTOOLS] Failed loading {0}: {1}", file, e.Message);
                    return;
                }
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                Regions.Clear();
                RegionBounds.Clear();
                if (childNodes != null && childNodes[0] != null && childNodes[0].OuterXml.Contains("Version") && childNodes[0].OuterXml.Contains(Config.Version))
                {
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        if (childNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement line = (XmlElement)childNodes[i];
                        if (!line.HasAttributes || !line.HasAttribute("Name") || !line.HasAttribute("Time"))
                        {
                            continue;
                        }
                        string name = line.GetAttribute("Name");
                        string time = line.GetAttribute("Time").ToLower();
                        if (name == "" || time == "")
                        {
                            continue;
                        }
                        name = name.Replace("r.", "");
                        name = name.Replace(".7rg", "");
                        if (!name.Contains("."))
                        {
                            Log.Out("[SERVERTOOLS] Region reset name is invalid: {0}", name);
                            continue;
                        }
                        if (!time.Contains("day") && !time.Contains("week") && !time.Contains("month"))
                        {
                            Log.Out("[SERVERTOOLS] Region reset time is invalid: {0}", time);
                            continue;
                        }
                        string[] nameSplit = name.Split('.');
                        if (!int.TryParse(nameSplit[0], out int value1))
                        {
                            Log.Out("[SERVERTOOLS] Region reset name is invalid: {0}", name);
                            continue;
                        }
                        if (!int.TryParse(nameSplit[1], out int value2))
                        {
                            Log.Out("[SERVERTOOLS] Region reset name is invalid: {0}", name);
                            continue;
                        }
                        int minX = 0, minZ = 0, maxX = 0, maxZ = 0;
                        if (value1 < 0)
                        {
                            minX = (value1 + 1) * 512;
                            maxX = minX - 512;
                        }
                        else
                        {
                            minX = value1 * 512;
                            maxX = minX + 512;
                        }

                        if (value2 < 0)
                        {
                            minZ = (value2 + 1) * 512;
                            maxZ = minZ - 512;
                        }
                        else
                        {
                            minZ = value2 * 512;
                            maxZ = minZ + 512;
                        }
                        if (!Regions.ContainsKey(name))
                        {
                            Regions.Add(name, time);
                            int[] bounds = new int[6];
                            bounds[0] = (minX <= maxX) ? minX : maxX;
                            bounds[1] = 0;
                            bounds[2] = (minZ <= maxZ) ? minZ : maxZ;
                            bounds[3] = (maxX >= minX) ? maxX : minX;
                            bounds[4] = 300;
                            bounds[5] = (maxZ >= minZ) ? maxZ : minZ;
                            RegionBounds.Add(bounds);
                        }
                    }
                }
                else
                {
                    XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                    if (nodeList != null)
                    {
                        Timers.UpgradeRegionResetXml(nodeList);
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
                    Log.Out("[SERVERTOOLS] Error in RegionReset.LoadXml: {0}", e.Message);
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
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- Possible time: day, week, month -->");
                    sw.WriteLine("    <!-- <Region Name=\"r.0.0.7rg\" Time=\"day\" /> -->");
                    sw.WriteLine("    <!-- <Region Name=\"r.-1.-1.7rg\" Time=\"week\" /> -->");
                    if (Regions.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> kvp in Regions)
                        {
                            sw.WriteLine("    <Region Name=\"r.{0}.7rg\" Time=\"{1}\" />", kvp.Key, kvp.Value);
                        }
                    }
                    sw.WriteLine("</RegionReset>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in RegionReset.UpdateXml: {0}", e.Message);
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
        
        public static void IsResetRegion(ClientInfo _cInfo, EntityPlayer _player)
        {
            if (RegionBounds.Count < 1)
            {
                return;
            }
            int[] bounds;
            for (int i = 0; i < RegionBounds.Count; i++)
            {
                bounds = RegionBounds[i];
                if (_player.position.x >= bounds[0] && _player.position.y >= bounds[1] && _player.position.z >= bounds[2] &&
                    _player.position.x <= bounds[3] && _player.position.y <= bounds[4] && _player.position.z <= bounds[5])
                {
                    if (!RegionPlayer.Contains(_player.entityId))
                    {
                        RegionPlayer.Add(_player.entityId);
                        SdtdConsole.Instance.ExecuteSync(string.Format("buffplayer {0} {1}", _cInfo.CrossplatformId.CombinedString, "region_reset"), null);
                    }
                    return;
                }
            }
            if (RegionPlayer.Contains(_player.entityId))
            {
                RegionPlayer.Remove(_player.entityId);
                SdtdConsole.Instance.ExecuteSync(string.Format("debuffplayer {0} {1}", _cInfo.CrossplatformId.CombinedString, "region_reset"), null);
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
                            SdtdConsole.Instance.ExecuteSync(string.Format("rr {0} {1}", region.Key[0], region.Key[1]), null);
                            if (region.Value == "day")
                            {
                                PersistentContainer.Instance.RegionReset.Add(region.Key, DateTime.Now.AddDays(1));
                            }
                            else if (region.Value == "week")
                            {
                                PersistentContainer.Instance.RegionReset.Add(region.Key, DateTime.Now.AddDays(7));
                            }
                            else if (region.Value == "month")
                            {
                                PersistentContainer.Instance.RegionReset.Add(region.Key, DateTime.Now.AddMonths(1));
                            }
                            PersistentContainer.DataChange = true;
                        }
                        else
                        {
                            PersistentContainer.Instance.RegionReset.TryGetValue(region.Key, out DateTime lastReset);
                            if (region.Value == "day" && DateTime.Now >= lastReset)
                            {
                                SdtdConsole.Instance.ExecuteSync(string.Format("rr {0} {1}", region.Key[0], region.Key[1]), null);
                                PersistentContainer.Instance.RegionReset[region.Key] = DateTime.Now.AddDays(1);
                                PersistentContainer.DataChange = true;
                            }
                            else if (region.Value == "week" && DateTime.Now >= lastReset)
                            {
                                SdtdConsole.Instance.ExecuteSync(string.Format("rr {0} {1}", region.Key[0], region.Key[1]), null);
                                PersistentContainer.Instance.RegionReset[region.Key] = DateTime.Now.AddDays(7);
                                PersistentContainer.DataChange = true;
                            }
                            else if (region.Value == "month" && DateTime.Now >= lastReset)
                            {
                                SdtdConsole.Instance.ExecuteSync(string.Format("rr {0} {1}", region.Key[0], region.Key[1]), null);
                                PersistentContainer.Instance.RegionReset[region.Key] = DateTime.Now.AddMonths(1);
                                PersistentContainer.DataChange = true;
                            }
                        }
                        count += 1;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in RegionReset.Exec: {0}", e.Message);
            }
        }

        public static void DisablePlayerBuff()
        {
            if (RegionPlayer.Count > 0)
            {
                for (int i = 0; i < RegionPlayer.Count; i++)
                {
                    ClientInfo cInfo = GeneralOperations.GetClientInfoFromEntityId(RegionPlayer[i]);
                    if (cInfo != null)
                    {
                        SdtdConsole.Instance.ExecuteSync(string.Format("debuffplayer {0} {1}", cInfo.CrossplatformId.CombinedString, "region_reset"), null);
                    }            
                }
            }
        }

        public static void SetIcon(string _iconName)
        {
            if (Icon != _iconName)
            {
                Icon = _iconName;
                GeneralOperations.SetBuffs();
            }
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
                    sw.WriteLine("<RegionReset>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- Possible time: day, week, month -->");
                    sw.WriteLine("    <!-- <Region Name=\"r.0.0.7rg\" Time=\"day\" /> -->");
                    sw.WriteLine("    <!-- <Region Name=\"r.-1.-1.7rg\" Time=\"week\" /> -->");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType == XmlNodeType.Comment)
                      {
                        if (!nodeList[i].OuterXml.Contains("<!-- <Region Name=\"r.0.0.7rg\"") && !nodeList[i].OuterXml.Contains("<!-- Do not forget") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Region Name=\"r.-1.-1.7rg\"") &&
                            !nodeList[i].OuterXml.Contains("<!-- Possible time") && !nodeList[i].OuterXml.Contains("<Region Name=\"\"") &&
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
                            if (line.HasAttributes && line.Name == "Region")
                            {
                                string name = "", time = "";
                                if (line.HasAttribute("Name"))
                                {
                                    name = line.GetAttribute("Name");
                                }
                                string[] nameSplit = name.Split('.');
                                int.TryParse(nameSplit[0], out int value1);
                                int.TryParse(nameSplit[1], out int value2);
                                if (line.HasAttribute("Time"))
                                {
                                    time = line.GetAttribute("Time");
                                }
                                sw.WriteLine("    <Region Name=\"r.{0}.{1}.7rg\" Time=\"{2}\" />", value1, value2, time);
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
                Log.Out("[SERVERTOOLS] Error in RegionReset.UpgradeXml: {0}", e.Message);
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}
