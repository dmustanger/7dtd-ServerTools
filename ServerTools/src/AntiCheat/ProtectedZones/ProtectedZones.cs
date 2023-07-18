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

        public static List<int[]> ProtectedList = new List<int[]>();
        public static Dictionary<int, int[]> Vectors = new Dictionary<int, int[]>();

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
                if (childNodes != null && (childNodes[0] != null && childNodes[0].OuterXml.Contains("Version") && childNodes[0].OuterXml.Contains(Config.Version)))
                {
                    List<int[]> oldProtections = ProtectedList;
                    string saveGameRegionDir = GameIO.GetSaveGameRegionDir();
                    RegionFileManager regionFileManager = new RegionFileManager(saveGameRegionDir, saveGameRegionDir, 0, true);
                    List<Chunk> chunks = new List<Chunk>();
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
                        vectors[0] = (corner1_x <= corner2_x) ? corner1_x : corner2_x;
                        vectors[1] = (corner1_z <= corner2_z) ? corner1_z : corner2_z;
                        vectors[2] = (corner1_x <= corner2_x) ? corner2_x : corner1_x;
                        vectors[3] = (corner1_z <= corner2_z) ? corner2_z : corner1_z;
                        vectors[4] = 1;
                        if (!ProtectedList.Contains(vectors))
                        {
                            ProtectedList.Add(vectors);
                            for (int j = vectors[0]; j <= vectors[2]; j++)
                            {
                                for (int k = vectors[1]; k <= vectors[3]; k++)
                                {
                                    Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(j, 1, k);
                                    if (chunk != null)
                                    {
                                        Bounds bounds = chunk.GetAABB();
                                        int x = j - (int)bounds.min.x, z = k - (int)bounds.min.z;
                                        if (active)
                                        {
                                            if (chunk.IsTraderArea(x, z))
                                            {
                                                continue;
                                            }
                                            chunk.SetTraderArea(x, z, true);
                                            if (!chunks.Contains(chunk))
                                            {
                                                chunks.Add(chunk);
                                            }
                                        }
                                        else
                                        {
                                            if (!chunk.IsTraderArea(x, z))
                                            {
                                                continue;
                                            }
                                            chunk.SetTraderArea(x, z, false);
                                            if (!chunks.Contains(chunk))
                                            {
                                                chunks.Add(chunk);
                                            }
                                        }
                                        continue;
                                    }
                                    else
                                    {
                                        int num = World.toChunkXZ(j);
                                        int num2 = World.toChunkXZ(k);
                                        long key = WorldChunkCache.MakeChunkKey(num, num2);
                                        if (regionFileManager.ContainsChunkSync(key))
                                        {
                                            chunk = regionFileManager.GetChunkSync(key);
                                            if (chunk == null)
                                            {
                                                continue;
                                            }
                                            Bounds bounds = chunk.GetAABB();
                                            int x = j - (int)bounds.min.x, z = k - (int)bounds.min.z;
                                            if (active)
                                            {
                                                if (!chunk.IsTraderArea(x, z))
                                                {
                                                    chunk.SetTraderArea(x, z, true);
                                                }
                                            }
                                            else
                                            {
                                                if (chunk.IsTraderArea(x, z))
                                                {
                                                    chunk.SetTraderArea(x, z, false);
                                                }
                                            }
                                            GameManager.Instance.World.ChunkCache.AddChunkSync(chunk);
                                        }
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                    for (int i = 0; i < oldProtections.Count; i++)
                    {
                        if (ProtectedList.Contains(oldProtections[i]))
                        {
                            continue;
                        }
                        for (int j = oldProtections[i][0]; j <= oldProtections[i][2]; j++)
                        {
                            for (int k = oldProtections[i][1]; k <= oldProtections[i][3]; k++)
                            {
                                Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(j, 1, k);
                                if (chunk != null)
                                {
                                    Bounds bounds = chunk.GetAABB();
                                    int x = j - (int)bounds.min.x, z = k - (int)bounds.min.z;
                                    if (!chunk.IsTraderArea(x, z))
                                    {
                                        continue;
                                    }
                                    chunk.SetTraderArea(x, z, false);
                                    if (!chunks.Contains(chunk))
                                    {
                                        chunks.Add(chunk);
                                    }
                                    continue;
                                }
                                else
                                {
                                    int num = World.toChunkXZ(j);
                                    int num2 = World.toChunkXZ(k);
                                    long key = WorldChunkCache.MakeChunkKey(num, num2);
                                    if (regionFileManager.ContainsChunkSync(key))
                                    {
                                        chunk = regionFileManager.GetChunkSync(key);
                                        if (chunk == null)
                                        {
                                            continue;
                                        }
                                        Bounds bounds = chunk.GetAABB();
                                        int x = j - (int)bounds.min.x, z = k - (int)bounds.min.z;
                                        if (chunk.IsTraderArea(x, z))
                                        {
                                            chunk.SetTraderArea(x, z, false);
                                            GameManager.Instance.World.ChunkCache.AddChunkSync(chunk);
                                        }
                                    }
                                    continue;
                                }
                            }
                        }
                    }
                    if (chunks.Count > 0)
                    {
                        List<ClientInfo> clientList = GeneralOperations.ClientList();
                        if (clientList == null || clientList.Count < 1)
                        {
                            for (int i = 0; i < clientList.Count; i++)
                            {
                                ClientInfo cInfo = clientList[i];
                                if (cInfo == null)
                                {
                                    continue;
                                }
                                for (int j = 0; j < chunks.Count; j++)
                                {
                                    cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(chunks[j], true));
                                }
                            }
                        }
                    }
                    GameManager.Instance.World.ChunkCache.Save();
                    regionFileManager.MakePersistent(GameManager.Instance.World.ChunkCache, true);
                    regionFileManager.WaitSaveDone();
                    regionFileManager.Cleanup();
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
                    Log.Out(string.Format("[SERVERTOOLS] Error in ProtectedZones.LoadXml: {0}", e.Message));
                }
            }
        }

        public static void UpdateXml()
        {
            FileWatcher.EnableRaisingEvents = false;
            File.Delete(FilePath);
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

        public static void ClearSingleChunkProtection(ClientInfo _cInfo)
        {
            try
            {
                List<Chunk> chunkList = new List<Chunk>();
                EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
                if (player != null)
                {
                    Vector3 position = player.position;
                    int x = (int)position.x, z = (int)position.z;
                    if (GameManager.Instance.World.IsChunkAreaLoaded(x, 1, z))
                    {
                        Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(x, 1, z);
                        if (!chunkList.Contains(chunk))
                        {
                            chunkList.Add(chunk);
                        }
                        Bounds bounds = chunk.GetAABB();
                        for (int i = (int)bounds.min.x; i < (int)bounds.max.x; i++)
                        {
                            for (int j = (int)bounds.min.z; j < (int)bounds.max.z; j++)
                            {
                                x = i - (int)bounds.min.x;
                                z = j - (int)bounds.min.z;
                                chunk.SetTraderArea(x, z, false);
                            }
                        }
                    }
                }
                if (chunkList.Count > 0)
                {
                    for (int k = 0; k < chunkList.Count; k++)
                    {
                        Chunk chunk = chunkList[k];
                        List<ClientInfo> clientList = GeneralOperations.ClientList();
                        if (clientList != null)
                        {
                            for (int l = 0; l < clientList.Count; l++)
                            {
                                ClientInfo cInfo2 = clientList[l];
                                if (cInfo2 != null)
                                {
                                    cInfo2.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(chunk, true));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ProtectedZones.ClearChunkProtection: {0}", e.Message));
            }
        }

        public static void ClearNineChunkProtection(ClientInfo _cInfo)
        {
            try
            {
                List<Chunk> chunkList = new List<Chunk>();
                List<int[]> positionList = new List<int[]>();
                EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
                if (player != null)
                {
                    positionList.Add(new int[] { (int)player.position.x, (int)player.position.z });
                    positionList.Add(new int[] { (int)player.position.x + 16, (int)player.position.z });
                    positionList.Add(new int[] { (int)player.position.x + 16, (int)player.position.z - 16 });
                    positionList.Add(new int[] { (int)player.position.x, (int)player.position.z - 16 });
                    positionList.Add(new int[] { (int)player.position.x - 16, (int)player.position.z - 16 });
                    positionList.Add(new int[] { (int)player.position.x - 16, (int)player.position.z });
                    positionList.Add(new int[] { (int)player.position.x - 16, (int)player.position.z + 16 });
                    positionList.Add(new int[] { (int)player.position.x, (int)player.position.z + 16 });
                    positionList.Add(new int[] { (int)player.position.x + 16, (int)player.position.z + 16 });
                    for (int i = 0; i < positionList.Count; i++)
                    {
                        int x = positionList[i][0], z = positionList[i][1];
                        if (GameManager.Instance.World.IsChunkAreaLoaded(x, 1, z))
                        {
                            Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(x, 1, z);
                            if (!chunkList.Contains(chunk))
                            {
                                chunkList.Add(chunk);
                            }
                            Bounds bounds = chunk.GetAABB();
                            for (int j = (int)bounds.min.x; j < (int)bounds.max.x; j++)
                            {
                                for (int k = (int)bounds.min.z; k < (int)bounds.max.z; k++)
                                {
                                    x = j - (int)bounds.min.x;
                                    z = k - (int)bounds.min.z;
                                    chunk.SetTraderArea(x, z, false);
                                }
                            }
                        }
                    }
                }
                if (chunkList.Count > 0)
                {
                    for (int k = 0; k < chunkList.Count; k++)
                    {
                        Chunk chunk = chunkList[k];
                        List<ClientInfo> clientList = GeneralOperations.ClientList();
                        if (clientList != null)
                        {
                            for (int l = 0; l < clientList.Count; l++)
                            {
                                ClientInfo cInfo2 = clientList[l];
                                if (cInfo2 != null)
                                {
                                    cInfo2.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(chunk, true));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ProtectedZones.ClearNineChunkProtection: {0}", e.Message));
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
