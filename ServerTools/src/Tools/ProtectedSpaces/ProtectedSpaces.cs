using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    class ProtectedSpaces
    {
        public static bool IsEnabled = false, IsRunning = false;
        public static List<int[]> Protected = new List<int[]>();
        public static Dictionary<int, int[]> Vectors = new Dictionary<int, int[]>();
        private const string file = "ProtectedSpaces.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        public static void Load()
        {
            if (IsEnabled && !IsRunning)
            {
                LoadXml();
                InitFileWatcher();
            }
        }

        public static void Unload()
        {
            if (!IsEnabled && IsRunning)
            {
                FileWatcher.Dispose();
                IsRunning = false;
                RemoveAllProtection();
            }
        }

        public static void LoadXml()
        {
            if (!Utils.FileExists(filePath))
            {
                UpdateXml();
            }
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(filePath);
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", file, e.Message));
                return;
            }
            XmlNode _XmlNode = xmlDoc.DocumentElement;
            foreach (XmlNode childNode in _XmlNode.ChildNodes)
            {
                if (childNode.Name == "Spaces")
                {
                    Protected.Clear();
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Spaces' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("XMin"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring protected space entry because of missing XMin attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("ZMin"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring protected space entry because of missing ZMin attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("XMax"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring protected space entry because of missing XMax attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("ZMax"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring protected space entry because of missing ZMax attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string xMin = _line.GetAttribute("XMin");
                        string zMin = _line.GetAttribute("ZMin");
                        string xMax = _line.GetAttribute("XMax");
                        string zMax = _line.GetAttribute("ZMax");
                        if (!int.TryParse(xMin, out int _xMin))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring protected space entry because of invalid _XMin integer: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(zMin, out int _zMin))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring protected space entry because of invalid _ZMin integer: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(xMax, out int _xMax))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring protected space entry because of invalid _XMax integer: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(zMax, out int _zMax))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring protected space entry because of invalid _ZMax integer: {0}", subChild.OuterXml));
                            continue;
                        }
                        int[] _vectors = new int[3];
                        if (_xMin > _xMax)
                        {
                            _vectors[0] = _xMax;
                            _vectors[1] = _xMin;
                        }
                        else
                        {
                            _vectors[0] = _xMin;
                            _vectors[1] = _xMax;
                        }
                        if (_zMin > _zMax)
                        {
                            _vectors[2] = _zMax;
                            _vectors[3] = _zMin;
                        }
                        else
                        {
                            _vectors[2] = _zMin;
                            _vectors[3] = _zMax;
                        }
                        if (!Protected.Contains(_vectors))
                        {
                            Protected.Add(_vectors);
                            if (PersistentContainer.Instance.ProtectedSpace != null && PersistentContainer.Instance.ProtectedSpace.Count > 0)
                            {
                                if (!PersistentContainer.Instance.ProtectedSpace.Contains(_vectors))
                                {
                                    PersistentContainer.Instance.ProtectedSpace.Add(_vectors);
                                    AddProtection(_vectors);
                                }
                            }
                            else
                            {
                                List<int[]> _protect = new List<int[]>();
                                _protect.Add(_vectors);
                                PersistentContainer.Instance.ProtectedSpace = _protect;
                                AddProtection(_vectors);
                            }
                        }
                    }
                }
            }
            if (PersistentContainer.Instance.ProtectedSpace != null && PersistentContainer.Instance.ProtectedSpace.Count > 0)
            {
                List<int[]> _protected = PersistentContainer.Instance.ProtectedSpace;
                for (int i = 0; i < _protected.Count; i++)
                {
                    if (!Protected.Contains(_protected[i]))
                    {
                        PersistentContainer.Instance.ProtectedSpace.Remove(_protected[i]);
                        RemoveProtection(_protected[i]);
                    }
                }
            }
        }

        public static void UpdateXml()
        {
            FileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<Protected>");
                sw.WriteLine("    <Spaces>");
                if (Protected.Count > 0)
                {
                    for (int i = 0; i < Protected.Count; i++)
                    {
                        int[] _vectors = Protected[i];
                        sw.WriteLine(string.Format("        <Space XMin=\"{0}\" ZMin=\"{1}\" XMax=\"{2}\" ZMax=\"{3}\" />", _vectors[0], _vectors[1], _vectors[2], _vectors[3]));
                    }
                }
                else
                {
                    sw.WriteLine("        <!-- <Space XMin=\"-30\" ZMin=\"-20\" XMax=\"10\" ZMax=\"50\" /> -->");
                    sw.WriteLine("        <!-- <Space XMin=\"-800\" ZMin=\"75\" XMax=\"-300\" ZMax=\"100\" /> -->");
                    sw.WriteLine("        <!-- <Space XMin=\"-50\" ZMin=\"-600\" XMax=\"-5\" ZMax=\"-550\" /> -->");
                }
                sw.WriteLine("    </Spaces>");
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
            if (!Utils.FileExists(filePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        public static void AddProtection(int[] _vectors)
        {
            try
            {
                List<Chunk> _chunkList = new List<Chunk>();
                for (int j = _vectors[0]; j <= _vectors[1]; j++)
                {
                    for (int k = _vectors[2]; k <= _vectors[3]; k++)
                    {
                        if (GameManager.Instance.World.IsChunkAreaLoaded(j, 1, k))
                        {
                            Chunk _chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(j, 1, k);
                            if (!_chunkList.Contains(_chunk))
                            {
                                _chunkList.Add(_chunk);
                            }
                            Bounds bounds = _chunk.GetAABB();
                            int _x = j - (int)bounds.min.x, _z = k - (int)bounds.min.z;
                            _chunk.SetTraderArea(_x, _z, true);
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
                if (_chunkList.Count > 0)
                {
                    for (int i = 0; i < _chunkList.Count; i++)
                    {
                        Chunk _chunk = _chunkList[i];
                        List<ClientInfo> _clientList = PersistentOperations.ClientList();
                        if (_clientList != null && _clientList.Count > 0)
                        {
                            for (int j = 0; j < _clientList.Count; j++)
                            {
                                ClientInfo _cInfo2 = _clientList[j];
                                if (_cInfo2 != null)
                                {
                                    _cInfo2.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(_chunk, true));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ProtectedSpaces.AddProtection: {0}", e.Message));
            }
        }

        public static void RemoveProtection(int[] _vectors)
        {
            try
            {
                List<Chunk> _chunkList = new List<Chunk>();
                for (int j = _vectors[0]; j <= _vectors[1]; j++)
                {
                    for (int k = _vectors[2]; k <= _vectors[3]; k++)
                    {
                        if (GameManager.Instance.World.IsChunkAreaLoaded(j, 1, k))
                        {
                            Chunk _chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(j, 1, k);
                            if (!_chunkList.Contains(_chunk))
                            {
                                _chunkList.Add(_chunk);
                            }
                            Bounds bounds = _chunk.GetAABB();
                            int _x = j - (int)bounds.min.x, _z = k - (int)bounds.min.z;
                            _chunk.SetTraderArea(_x, _z, false);
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
                if (_chunkList.Count > 0)
                {
                    for (int i = 0; i < _chunkList.Count; i++)
                    {
                        Chunk _chunk = _chunkList[i];
                        List<ClientInfo> _clientList = PersistentOperations.ClientList();
                        if (_clientList != null && _clientList.Count > 0)
                        {
                            for (int j = 0; j < _clientList.Count; j++)
                            {
                                ClientInfo _cInfo2 = _clientList[j];
                                if (_cInfo2 != null)
                                {
                                    _cInfo2.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(_chunk, true));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ProtectedSpaces.RemoveProtection: {0}", e.Message));
            }
        }

        public static void RemoveAllProtection()
        {
            try
            {
                if (PersistentContainer.Instance.ProtectedSpace != null && PersistentContainer.Instance.ProtectedSpace.Count > 0)
                {
                    List<Chunk> _chunkList = new List<Chunk>();
                    List<int[]> _protected = PersistentContainer.Instance.ProtectedSpace;
                    for (int i = 0; i < _protected.Count; i++)
                    {
                        for (int j = _protected[i][0]; j <= _protected[i][1]; j++)
                        {
                            for (int k = _protected[i][2]; k <= _protected[i][3]; k++)
                            {
                                if (GameManager.Instance.World.IsChunkAreaLoaded(j, 1, k))
                                {
                                    Chunk _chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(j, 1, k);
                                    if (!_chunkList.Contains(_chunk))
                                    {
                                        _chunkList.Add(_chunk);
                                    }
                                    Bounds bounds = _chunk.GetAABB();
                                    int _x = j - (int)bounds.min.x, _z = k - (int)bounds.min.z;
                                    _chunk.SetTraderArea(_x, _z, false);
                                }
                            }
                        }
                    }
                    if (_chunkList.Count > 0)
                    {
                        for (int i = 0; i < _chunkList.Count; i++)
                        {
                            Chunk _chunk = _chunkList[i];
                            List<ClientInfo> _clientList = PersistentOperations.ClientList();
                            if (_clientList != null && _clientList.Count > 0)
                            {
                                for (int j = 0; j < _clientList.Count; j++)
                                {
                                    ClientInfo _cInfo2 = _clientList[j];
                                    if (_cInfo2 != null)
                                    {
                                        _cInfo2.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(_chunk, true));
                                    }
                                }
                            }
                        }
                    }
                    _protected.Clear();
                    PersistentContainer.Instance.ProtectedSpace = _protected;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ProtectedSpaces.RemoveAllProtection: {0}", e.Message));
            }
        }
    }
}
