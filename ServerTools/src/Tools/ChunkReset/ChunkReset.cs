using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    class ChunkReset
    {
        public static bool IsEnabled = false, IsRunning = false;
        public static float[] Bounds = new float[6];
        public static string Icon = "ui_game_symbol_brick";

        public static Dictionary<string, string> Chunks = new Dictionary<string, string>();
        public static List<float[]> ChunkBounds = new List<float[]>();
        public static List<int> ChunkPlayer = new List<int>();
        
        private const string FileName = "ChunkReset.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, FileName);
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, FileName);
        
        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }
        
        public static void Unload()
        {
            Chunks.Clear();
            ChunkBounds.Clear();
            DisablePlayerBuff();
            ChunkPlayer.Clear();
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
                    Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", FileName, e.Message));
                    return;
                }
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                Chunks.Clear();
                if (childNodes != null && (childNodes[0] != null && childNodes[0].OuterXml.Contains("Version") && childNodes[0].OuterXml.Contains(Config.Version)))
                {
                    string saveGameRegionDir = GameIO.GetSaveGameRegionDir();
                    RegionFileManager regionFileManager = new RegionFileManager(saveGameRegionDir, saveGameRegionDir, 0, true);
                    List<long> chunkKeys = new List<long>();
                    XmlElement line;
                    Chunk chunk;
                    Bounds bounds;
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        if (childNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        line = (XmlElement)childNodes[i];
                        if (!line.HasAttributes || !line.HasAttribute("Position") || !line.HasAttribute("Time"))
                        {
                            continue;
                        }
                        string position = line.GetAttribute("Position");
                        if (position == "")
                        {
                            continue;
                        }
                        string time = line.GetAttribute("Time");
                        if (position == "0,0" || position == "")
                        {
                            continue;
                        }
                        if (!position.Contains(","))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Chunk reset position is invalid: {0}", position));
                            continue;
                        }
                        string[] positionSplit = position.Split(',');
                        if (!int.TryParse(positionSplit[0], out int x))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Chunk reset position x is invalid: {0}", positionSplit[0]));
                            continue;
                        }
                        if (!int.TryParse(positionSplit[1], out int z))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Chunk reset position z is invalid: {0}", positionSplit[1]));
                            continue;
                        }
                        int num = World.toChunkXZ(x);
                        int num2 = World.toChunkXZ(z);
                        long key = WorldChunkCache.MakeChunkKey(num, num2);
                        if (!chunkKeys.Contains(key))
                        {
                            chunkKeys.Add(key);
                            Chunks.Add(position, time);
                            if (!regionFileManager.ContainsChunkSync(key))
                            {
                                continue;
                            }
                            chunk = regionFileManager.GetChunkSync(key);
                            bounds = chunk.GetAABB();
                            Bounds[0] = (bounds.min.x <= bounds.max.x) ? bounds.min.x : bounds.max.x;
                            Bounds[1] = 0;
                            Bounds[2] = (bounds.min.z <= bounds.max.z) ? bounds.min.z : bounds.max.z;
                            Bounds[3] = (bounds.max.x >= bounds.min.x) ? bounds.max.x : bounds.min.x;
                            Bounds[4] = 300;
                            Bounds[5] = (bounds.max.z >= bounds.min.z) ? bounds.max.z : bounds.min.z;
                            ChunkBounds.Add(Bounds);
                        }
                    }
                    regionFileManager.Cleanup();
                }
                else
                {
                    XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                    if (nodeList != null)
                    {
                        Timers.UpgradeChunkResetXml(nodeList);
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
                    Log.Out(string.Format("[SERVERTOOLS] Error in ChunkReset.LoadXml: {0}", e.Message));
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
                    sw.WriteLine("<ChunkReset>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- A chunk is 16x16 blocks. Only one position is needed inside a chunk for reset -->");
                    sw.WriteLine("    <!-- Possible time: day, week, month -->");
                    sw.WriteLine("    <!-- <Chunk Position=\"0,0\" Time=\"week\" /> -->");
                    if (Chunks.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> kvp in Chunks)
                        {
                            sw.WriteLine(string.Format("    <Chunk Position=\"{0}\" Time=\"{1}\" />", kvp.Key, kvp.Value));
                        }
                    }
                    sw.WriteLine("</ChunkReset>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ChunkReset.UpdateXml: {0}", e.Message));
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
        
        public static void IsResetChunk(ClientInfo _cInfo, EntityPlayer _player)
        {
            for (int i = 0; i < ChunkBounds.Count; i++)
            {
                Bounds = ChunkBounds[i];
                if (_player.position.x >= Bounds[0] && _player.position.y >= Bounds[1] && _player.position.z >= Bounds[2] &&
                    _player.position.x <= Bounds[3] && _player.position.y <= Bounds[4] && _player.position.z <= Bounds[5])
                {
                    if (!ChunkPlayer.Contains(_player.entityId))
                    {
                        ChunkPlayer.Add(_player.entityId);
                        SdtdConsole.Instance.ExecuteSync(string.Format("buffplayer {0} {1}", _cInfo.CrossplatformId.CombinedString, "chunk_reset"), null);
                    }
                    return;
                }
            }
            if (ChunkPlayer.Contains(_player.entityId))
            {
                ChunkPlayer.Remove(_player.entityId);
                SdtdConsole.Instance.ExecuteSync(string.Format("debuffplayer {0} {1}", _cInfo.CrossplatformId.CombinedString, "chunk_reset"), null);
            }
        }
        
        public static void Exec()
        {
            try
            {
                if (Chunks.Count > 0)
                {
                    int count = 0;
                    foreach (var chunk in Chunks)
                    {
                        if (!PersistentContainer.Instance.ChunkReset.ContainsKey(chunk.Key))
                        {
                            Bounds = ChunkBounds[count];
                            SdtdConsole.Instance.ExecuteSync(string.Format("chunkreset {0} {1} {2} {3}", Bounds[0], Bounds[2], Bounds[3], Bounds[5]), null);
                            if (chunk.Value == "day")
                            {
                                PersistentContainer.Instance.RegionReset.Add(chunk.Key, DateTime.Now.AddDays(1));
                            }
                            else if (chunk.Value == "week")
                            {
                                PersistentContainer.Instance.RegionReset.Add(chunk.Key, DateTime.Now.AddDays(7));
                            }
                            else if (chunk.Value == "month")
                            {
                                PersistentContainer.Instance.RegionReset.Add(chunk.Key, DateTime.Now.AddMonths(1));
                            }
                            PersistentContainer.DataChange = true;
                        }
                        else
                        {
                            PersistentContainer.Instance.ChunkReset.TryGetValue(chunk.Key, out DateTime lastReset);
                            if (chunk.Value == "day" && DateTime.Now >= lastReset)
                            {
                                Bounds = ChunkBounds[count];
                                SdtdConsole.Instance.ExecuteSync(string.Format("chunkreset {0} {1} {2} {3}", Bounds[0], Bounds[2], Bounds[3], Bounds[5]), null);
                                PersistentContainer.Instance.ChunkReset[chunk.Key] = DateTime.Now;
                                PersistentContainer.DataChange = true;
                            }
                            else if (chunk.Value == "week" && DateTime.Now >= lastReset)
                            {
                                Bounds = ChunkBounds[count];
                                SdtdConsole.Instance.ExecuteSync(string.Format("chunkreset {0} {1} {2} {3}", Bounds[0], Bounds[2], Bounds[3], Bounds[5]), null);
                                PersistentContainer.Instance.ChunkReset[chunk.Key] = DateTime.Now;
                                PersistentContainer.DataChange = true;
                            }
                            else if (chunk.Value == "month" && DateTime.Now >= lastReset)
                            {
                                Bounds = ChunkBounds[count];
                                SdtdConsole.Instance.ExecuteSync(string.Format("chunkreset {0} {1} {2} {3}", Bounds[0], Bounds[2], Bounds[3], Bounds[5]), null);
                                PersistentContainer.Instance.ChunkReset[chunk.Key] = DateTime.Now;
                                PersistentContainer.DataChange = true;
                            }
                        }
                        count += 1;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ChunkReset.Exec: {0}", e.Message));
            }
        }

        public static void DisablePlayerBuff()
        {
            if (ChunkPlayer.Count > 0)
            {
                for (int i = 0; i < ChunkPlayer.Count; i++)
                {
                    ClientInfo cInfo = GeneralOperations.GetClientInfoFromEntityId(ChunkPlayer[i]);
                    if (cInfo != null)
                    {
                        SdtdConsole.Instance.ExecuteSync(string.Format("debuffplayer {0} {1}", cInfo.CrossplatformId.CombinedString, "chunk_reset"), null);
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
                    sw.WriteLine("<ChunkReset>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- A chunk is 16x16 blocks. Only one block is needed to be marked for the entire chunk to reset -->");
                    sw.WriteLine("    <!-- Possible time: day, week, month -->");
                    sw.WriteLine("    <!-- <Chunk Position=\"0,0\" Time=\"week\" /> -->");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType == XmlNodeType.Comment)
                      {
                        if (!nodeList[i].OuterXml.Contains("<!-- <Version") && !nodeList[i].OuterXml.Contains("<!-- Do not forget") &&
                            !nodeList[i].OuterXml.Contains("<!-- A chunk") && !nodeList[i].OuterXml.Contains("<!-- Possible time") &&
                            !nodeList[i].OuterXml.Contains("<Chunk Position=\"\""))
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
                            if (line.HasAttributes && line.Name == "Chunk")
                            {
                                string position = "", time = "";
                                if (line.HasAttribute("Position"))
                                {
                                    position = line.GetAttribute("Position");
                                }
                                if (line.HasAttribute("Time"))
                                {
                                    time = line.GetAttribute("Time");
                                }
                                sw.WriteLine(string.Format("    <Chunk Position=\"{0}\" Time=\"{1}\" />", position, time));
                            }
                        }
                    }
                    sw.WriteLine("</ChunkReset>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ChunkReset.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}
