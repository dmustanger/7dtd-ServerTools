using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    class ProtectedZones
    {
        public static bool IsEnabled = false, IsRunning = false;
        public static int Admin_Level = 0;

        public static List<int[]> ProtectedList = new List<int[]>();
        public static List<int> Players = new List<int>();

        private static Vector3 playerPosition;
        private static int[] protectedZoneOne = new int[5];
        private static int[] protectedZoneTwo = new int[5];

        private const string file = "ProtectedZones.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        public static void Unload()
        {
            ProtectedList.Clear();
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
                ProtectedList.Clear();
                if (childNodes != null && childNodes[0] != null && childNodes[0].OuterXml.Contains("Version") && childNodes[0].OuterXml.Contains(Config.Version))
                {
                    List<int[]> oldProtections = ProtectedList;
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        if (childNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement line = (XmlElement)childNodes[i];
                        if (!line.HasAttributes || !line.HasAttribute("Corner1") || !line.HasAttribute("Corner2") || !line.HasAttribute("Active"))
                        {
                            continue;
                        }
                        string corner1 = line.GetAttribute("Corner1");
                        string corner2 = line.GetAttribute("Corner2");
                        if (corner1 == "" || corner2 == "")
                        {
                            continue;
                        }
                        if (!corner1.Contains(","))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ProtectedZones.xml entry. Invalid form missing comma in Corner1 attribute: {0}", line.OuterXml));
                            continue;
                        }
                        if (!corner2.Contains(","))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ProtectedZones.xml entry. Invalid form missing comma in Corner2 attribute: {0}", line.OuterXml));
                            continue;
                        }
                        if (!bool.TryParse(line.GetAttribute("Active"), out bool active))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ProtectedZones.xml entry. Invalid True/False for Active attribute: {0}", line.OuterXml));
                            continue;
                        }
                        string[] corner1Split = corner1.Split(',');
                        string[] corner2Split = corner2.Split(',');
                        int.TryParse(corner1Split[0], out int corner1_x);
                        int.TryParse(corner1Split[1], out int corner1_z);
                        int.TryParse(corner2Split[0], out int corner2_x);
                        int.TryParse(corner2Split[1], out int corner2_z);
                        int[] vectors = new int[5];
                        vectors[0] = ((corner1_x <= corner2_x) ? corner1_x : corner2_x) - 2;
                        vectors[1] = ((corner1_z <= corner2_z) ? corner1_z : corner2_z) - 2;
                        vectors[2] = ((corner1_x <= corner2_x) ? corner2_x : corner1_x) +2;
                        vectors[3] = ((corner1_z <= corner2_z) ? corner2_z : corner1_z) +2;
                        vectors[4] = 1;
                        if (!ProtectedList.Contains(vectors))
                        {
                            ProtectedList.Add(vectors);
                        }
                    }
                }
                else
                {
                    XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                    if (nodeList != null)
                    {
                        Timers.UpgradeProtectedZonesXml(nodeList);
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
                    Log.Out("[SERVERTOOLS] Error in ProtectedZones.LoadXml: {0}", e.Message);
                }
            }
        }

        public static void UpdateXml()
        {
            FileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
            {
                sw.WriteLine("<Protected>");
                sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                sw.WriteLine("    <!-- <Protected Corner1=\"-30,-20\" Corner2=\"10,50\" Active=\"True\" /> -->");
                if (ProtectedList.Count > 0)
                {
                    for (int i = 0; i < ProtectedList.Count; i++)
                    {
                        if (ProtectedList[i][4] == 1)
                        {
                            sw.WriteLine(string.Format("    <Protected Corner1=\"{0},{1}\" Corner2=\"{2},{3}\" Active=\"True\" />", ProtectedList[i][0], ProtectedList[i][1], ProtectedList[i][2], ProtectedList[i][3]));
                        }
                        else
                        {
                            sw.WriteLine(string.Format("    <Protected Corner1=\"{0},{1}\" Corner2=\"{2},{3}\" Active=\"False\" />", ProtectedList[i][0], ProtectedList[i][1], ProtectedList[i][2], ProtectedList[i][3]));
                        }
                    }
                }
                sw.WriteLine("</Protected>");
                sw.Flush();
                sw.Close();
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

        public static bool IsProtected(Vector3i _position)
        {
            for (int i = 0; i < ProtectedList.Count; i++)
            {
                protectedZoneOne = ProtectedList[i];
                if (_position.x >= protectedZoneOne[0] && _position.x <= protectedZoneOne[2] && _position.z >= protectedZoneOne[1] && _position.z <= protectedZoneOne[3])
                {
                    return true;
                }
            }
            return false;
        }

        public static void InsideProtectedZone(ClientInfo _cInfo, EntityPlayer _player)
        {
            playerPosition = _player.serverPos.ToVector3() / 32;
            for (int i = 0; i < ProtectedList.Count; i++)
            {
                protectedZoneTwo = ProtectedList[i];
                if (playerPosition.x >= protectedZoneTwo[0] && playerPosition.x <= protectedZoneTwo[2] && playerPosition.z >= protectedZoneTwo[1] && playerPosition.z <= protectedZoneTwo[3])
                {
                    if (!Players.Contains(_cInfo.entityId))
                    {
                        Players.Add(_cInfo.entityId);
                        SdtdConsole.Instance.ExecuteSync(string.Format("buffplayer {0} block_protection", _cInfo.CrossplatformId.CombinedString), null);
                    }
                    return;
                }
            }
            if (Players.Contains(_cInfo.entityId))
            {
                Players.Remove(_cInfo.entityId);
                SdtdConsole.Instance.ExecuteSync(string.Format("debuffplayer {0} block_protection", _cInfo.CrossplatformId.CombinedString), null);
            }
        }

        public static void ClearSingleChunkProtection(ClientInfo _cInfo)
        {
            ThreadManager.AddSingleTask(delegate (ThreadManager.TaskInfo _taskInfo)
            {
                try
                {
                    EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
                    if (player != null)
                    {
                        Vector3 position = player.serverPos.ToVector3() / 32;
                        Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos((int)position.x, 1, (int)position.z);
                        Bounds bounds = chunk.GetAABB();
                        for (int i = (int)bounds.min.x; i < (int)bounds.max.x; i++)
                        {
                            for (int j = (int)bounds.min.z; j < (int)bounds.max.z; j++)
                            {
                                for (int k = 0; k < ProtectedList.Count; k++)
                                {
                                    if (i >= ProtectedList[k][0] && i <= ProtectedList[k][2] && j >= ProtectedList[k][1] && j <= ProtectedList[k][3])
                                    {
                                        ProtectedList.Remove(ProtectedList[k]);
                                        Log.Out("[SERVERTOOLS] Removed protection of a zone via console command for '{0}' named '{1}' @ '{2}'", _cInfo.playerName, _cInfo.CrossplatformId.CombinedString, position);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Out("[SERVERTOOLS] Error in ProtectedZones.ClearSingleChunkProtection: {0}", e.Message);
                }
            });
        }

        public static void ClearSurroundingChunkProtection(ClientInfo _cInfo)
        {
            try
            {
                EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
                if (player != null && ProtectedList.Count > 0)
                {
                    int[][] zones = ProtectedList.ToArray();
                    Vector3i position = new Vector3i(player.serverPos.ToVector3() / 32f);
                    List<Chunk> chunkList = GeneralOperations.GetSurroundingChunks(position);
                    Bounds bounds;
                    for (int i = 0; i < chunkList.Count; i++)
                    {
                        bounds = chunkList[i].GetAABB();
                        for (int j = (int)bounds.min.x; j < (int)bounds.max.x; j++)
                        {
                            for (int k = (int)bounds.min.z; k < (int)bounds.max.z; k++)
                            {
                                for (int l = 0; l < zones.Length; l++)
                                {
                                    if (ProtectedList.Contains(zones[l]) && i >= zones[l][0] && i <= zones[l][2] && j >= zones[l][1] && j <= zones[l][3])
                                    {
                                        ProtectedList.Remove(zones[l]);
                                        Log.Out("[SERVERTOOLS] Removed protection of a zone via console command for '{0}' named '{1}' @ '{2}'", _cInfo.playerName, _cInfo.CrossplatformId.CombinedString, position);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in ProtectedZones.ClearNineChunkProtection: {0}", e.Message);
            }
        }

        public static void UpgradeXml(XmlNodeList nodeList)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<Protected>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- <Protected Corner1=\"-30,-20\" Corner2=\"10,50\" Active=\"True\" /> -->");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType == XmlNodeType.Comment)
                        {
                            if (!nodeList[i].OuterXml.Contains("<!-- <Protected Corner1=\"-30,-20\"") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Protected Corner1=\"\"") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Version") && !nodeList[i].OuterXml.Contains("<!-- Do not forget"))
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
                            if (line.HasAttributes && line.Name == "Protected")
                            {
                                string corner1 = "", corner2 = "", active = "";
                                if (line.HasAttribute("Corner1"))
                                {
                                    corner1 = line.GetAttribute("Corner1");
                                }
                                if (line.HasAttribute("Corner2"))
                                {
                                    corner2 = line.GetAttribute("Corner2");
                                }
                                if (line.HasAttribute("Active"))
                                {
                                    active = line.GetAttribute("Active");
                                }
                                sw.WriteLine(string.Format("    <Protected Corner1=\"{0}\" Corner2=\"{1}\" Active=\"{2}\" />", corner1, corner2, active));
                            }
                        }
                    }
                    sw.WriteLine("</Protected>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ProtectedZones.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}
