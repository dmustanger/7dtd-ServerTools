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
        public static List<string> AddProtection = new List<string>();
        public static List<string> RemoveProtection = new List<string>();
        public static Dictionary<int, int[]> Vectors = new Dictionary<int, int[]>();

        public static List<long> Keys = new List<long>();

        private const string file = "ProtectedZones.xml";
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
            ProtectedList.Clear();
            AddProtection.Clear();
            RemoveProtection.Clear();
            Vectors.Clear();
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
                if (PersistentContainer.Instance.ProtectedZones == null)
                {
                    PersistentContainer.Instance.ProtectedZones = new List<string>();
                    PersistentContainer.DataChange = true;
                }
                bool upgrade = true;
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                if (childNodes != null)
                {
                    List<string> keys = new List<string>();
                    ProtectedList.Clear();
                    AddProtection.Clear();
                    RemoveProtection.Clear();
                    List<Chunk> chunks = new List<Chunk>();
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
                                else if (line.HasAttribute("Corner1") && line.HasAttribute("Corner2") && line.HasAttribute("Active"))
                                {
                                    string corner1 = line.GetAttribute("Corner1");
                                    string corner2 = line.GetAttribute("Corner2");
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
                                    if (corner1_x < corner2_x)
                                    {
                                        vectors[0] = corner1_x;
                                        vectors[2] = corner2_x;
                                    }
                                    else
                                    {
                                        vectors[0] = corner2_x;
                                        vectors[2] = corner1_x;
                                    }
                                    if (corner1_z < corner2_z)
                                    {
                                        vectors[1] = corner1_z;
                                        vectors[3] = corner2_z;
                                    }
                                    else
                                    {
                                        vectors[1] = corner2_z;
                                        vectors[3] = corner1_z;
                                    }
                                    if (active)
                                    {
                                        vectors[4] = 1;
                                    }
                                    else
                                    {
                                        vectors[4] = 0;
                                    }
                                    if (!ProtectedList.Contains(vectors))
                                    {
                                        ProtectedList.Add(vectors);
                                        for (int j = vectors[0]; j <= vectors[2]; j++)
                                        {
                                            for (int k = vectors[1]; k <= vectors[3]; k++)
                                            {
                                                string vector = new Vector3i(j, 0, k).ToString();
                                                Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(j, 1, k);
                                                if (chunk != null)
                                                {
                                                    Bounds bounds = chunk.GetAABB();
                                                    int x = j - (int)bounds.min.x, z = k - (int)bounds.min.z;
                                                    if (active)
                                                    {
                                                        if (!chunk.IsTraderArea(x, z))
                                                        {
                                                            chunk.SetTraderArea(x, z, true);
                                                            if (!chunks.Contains(chunk))
                                                            {
                                                                chunks.Add(chunk);
                                                            }
                                                            if (!PersistentContainer.Instance.ProtectedZones.Contains(vector))
                                                            {
                                                                PersistentContainer.Instance.ProtectedZones.Add(vector);
                                                                PersistentContainer.DataChange = true;
                                                            }
                                                        }
                                                        if (!keys.Contains(vector))
                                                        {
                                                            keys.Add(vector);
                                                        }
                                                    }
                                                    else if (PersistentContainer.Instance.ProtectedZones.Contains(vector))
                                                    {
                                                        if (chunk.IsTraderArea(x, z))
                                                        {
                                                            chunk.SetTraderArea(x, z, false);
                                                            if (!chunks.Contains(chunk))
                                                            {
                                                                chunks.Add(chunk);
                                                            }
                                                        }
                                                        PersistentContainer.Instance.ProtectedZones.Remove(vector);
                                                        PersistentContainer.DataChange = true;
                                                    }
                                                }
                                                else if (active)
                                                {
                                                    if (!PersistentContainer.Instance.ProtectedZones.Contains(vector))
                                                    {
                                                        AddProtection.Add(vector);
                                                    }
                                                    if (!keys.Contains(vector))
                                                    {
                                                        keys.Add(vector);
                                                    }
                                                }
                                                else if (PersistentContainer.Instance.ProtectedZones.Contains(vector))
                                                {
                                                    RemoveProtection.Remove(vector);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (chunks.Count > 0)
                    {
                        List<ClientInfo> clientList = PersistentOperations.ClientList();
                        if (clientList != null)
                        {
                            for (int i = 0; i < clientList.Count; i++)
                            {
                                ClientInfo cInfo = clientList[i];
                                if (cInfo != null)
                                {
                                    for (int j = 0; j < chunks.Count; j++)
                                    {
                                        cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(chunks[j], true));
                                    }
                                }
                            }
                        }
                    }
                    if (PersistentContainer.Instance.ProtectedZones.Count > 0)
                    {
                        for (int i = 0; i < PersistentContainer.Instance.ProtectedZones.Count; i++)
                        {
                            if (!keys.Contains(PersistentContainer.Instance.ProtectedZones[i]))
                            {
                                RemoveProtection.Add(PersistentContainer.Instance.ProtectedZones[i]);
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
                            Log.Out(string.Format("[SERVERTOOLS] The existing ProtectedZones.xml was too old or misconfigured. File deleted and rebuilt for version {0}", Config.Version));
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
                    Log.Out(string.Format("[SERVERTOOLS] Error in ProtectedZones.LoadXml: {0}", e.Message));
                }
            }
        }

        public static void UpdateXml()
        {
            FileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<Protected>");
                sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                sw.WriteLine("    <!-- <Protected Corner1=\"-30,-20\" Corner2=\"10,50\" Active=\"True\" /> -->");
                sw.WriteLine();
                sw.WriteLine();
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

        public static void Add(Chunk _chunk, Vector3i _vector, string _vectorString)
        {
            AddProtection.Remove(_vectorString);
            Bounds bounds = _chunk.GetAABB();
            int x = _vector.x - (int)bounds.min.x, z = _vector.z - (int)bounds.min.z;
            _chunk.SetTraderArea(x, z, true);
            PersistentContainer.Instance.ProtectedZones.Add(_vectorString);
            PersistentContainer.DataChange = true;
            List<ClientInfo> clientList = PersistentOperations.ClientList();
            if (clientList != null)
            {
                for (int i = 0; i < clientList.Count; i++)
                {
                    ClientInfo cInfo = clientList[i];
                    if (cInfo != null)
                    {
                        cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(_chunk, true));
                    }
                }
            }
        }

        public static void Remove(Chunk _chunk, Vector3i _vector, string _vectorString)
        {
            RemoveProtection.Remove(_vectorString);
            Bounds bounds = _chunk.GetAABB();
            int x = _vector.x - (int)bounds.min.x, z = _vector.z - (int)bounds.min.z;
            _chunk.SetTraderArea(x, z, false);
            PersistentContainer.Instance.ProtectedZones.Remove(_vectorString);
            PersistentContainer.DataChange = true;
            List<ClientInfo> clientList = PersistentOperations.ClientList();
            if (clientList != null)
            {
                for (int i = 0; i < clientList.Count; i++)
                {
                    ClientInfo cInfo = clientList[i];
                    if (cInfo != null)
                    {
                        cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(_chunk, true));
                    }
                }
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
                    sw.WriteLine("<Protected>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("    <!-- <Protected Corner1=\"-30,-20\" Corner2=\"10,50\" Active=\"True\" /> -->");
                    for (int i = 0; i < OldNodeList.Count; i++)
                    {
                        if (OldNodeList[i].NodeType == XmlNodeType.Comment && !OldNodeList[i].OuterXml.Contains("<!-- <Protected Corner1=\"-30,-20\"") && 
                            !OldNodeList[i].OuterXml.Contains("<!-- <Protected Corner1=\"\""))
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
