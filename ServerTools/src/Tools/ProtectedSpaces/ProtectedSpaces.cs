using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    class ProtectedSpaces
    {
        public static bool IsEnabled = false, IsRunning = false;
        public static List<string[]> ProtectedList = new List<string[]>();
        public static Dictionary<int, string[]> Vectors = new Dictionary<int, string[]>();
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
                UnloadProtection();
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
                    ProtectedList.Clear();
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
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ProtectedSpace entry because of missing XMin attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("ZMin"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ProtectedSpace entry because of missing ZMin attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("XMax"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ProtectedSpace entry because of missing XMax attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("ZMax"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ProtectedSpace entry because of missing ZMax attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string xMin = _line.GetAttribute("XMin");
                        string zMin = _line.GetAttribute("ZMin");
                        string xMax = _line.GetAttribute("XMax");
                        string zMax = _line.GetAttribute("ZMax");
                        int _xMin, _zMin, _xMax, _zMax;
                        if (!int.TryParse(xMin, out _xMin))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ProtectedSpace entry because of invalid _XMin integer: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(zMin, out _zMin))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ProtectedSpace entry because of invalid _ZMin integer: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(xMax, out _xMax))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ProtectedSpace entry because of invalid _XMax integer: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(zMax, out _zMax))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ProtectedSpace entry because of invalid _ZMax integer: {0}", subChild.OuterXml));
                            continue;
                        }
                        int _xMinAlt = _xMin, _zMinAlt = _zMin, _xMaxAlt = _xMax, _zMaxAlt = _zMax;
                        if (_xMin > _xMax)
                        {
                            _xMinAlt = _xMax;
                            _xMaxAlt = _xMin;
                        }
                        if (_zMin > _zMax)
                        {
                            _zMinAlt = _zMax;
                            _zMaxAlt = _zMin;
                        }
                        List<Chunk> _chunkList = new List<Chunk>();
                        string[] _vector = { _xMinAlt.ToString(), _zMinAlt.ToString(), _xMaxAlt.ToString(), _zMaxAlt.ToString() };
                        if (!ProtectedList.Contains(_vector))
                        {
                            ProtectedList.Add(_vector);
                        }
                        for (int i = _xMinAlt; i <= _xMaxAlt; i++)
                        {
                            for (int j = _zMinAlt; j <= _zMaxAlt; j++)
                            {
                                if (GameManager.Instance.World.IsChunkAreaLoaded(i, 1, j))
                                {
                                    Chunk _chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(i, 1, j);
                                    if (!_chunkList.Contains(_chunk))
                                    {
                                        _chunkList.Add(_chunk);
                                    }
                                    Bounds bounds = _chunk.GetAABB();
                                    int _x = i - (int)bounds.min.x, _z = j - (int)bounds.min.z;
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
                            for (int k = 0; k < _chunkList.Count; k++)
                            {
                                Chunk _chunk = _chunkList[k];
                                List<ClientInfo> _clientList = PersistentOperations.ClientList();
                                if (_clientList != null && _clientList.Count > 0)
                                {
                                    for (int l = 0; l < _clientList.Count; l++)
                                    {
                                        ClientInfo _cInfo2 = _clientList[l];
                                        if (_cInfo2 != null)
                                        {
                                            _cInfo2.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(_chunk, true));
                                        }
                                    }
                                }
                            }
                        }
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
                if (ProtectedList.Count > 0)
                {
                    for (int i = 0; i < ProtectedList.Count; i++)
                    {
                        string[] _vectors = ProtectedList[i];
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

        public static bool IsProtectedSpace(Vector3i _position)
        {
            if (ProtectedList.Count > 0)
            {
                for (int i = 0; i < ProtectedList.Count; i++)
                {
                    string[] _vectors = ProtectedList[i];
                    int _xMin = int.Parse(_vectors[0]), _zMin = int.Parse(_vectors[1]), _xMax = int.Parse(_vectors[2]), _zMax = int.Parse(_vectors[3]);
                    if (VectorBox(_xMin, _zMin, _xMax, _zMax, _position.x, _position.z))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool VectorBox(float xMin, float zMin, float xMax, float zMax, float _X, float _Z)
        {
            if (xMin >= 0 && xMax >= 0)
            {
                if (xMin < xMax)
                {
                    if (_X < xMin || _X > xMax)
                    {
                        return false;
                    }
                }
                else
                {
                    if (_X > xMin || _X < xMax)
                    {
                        return false;
                    }
                }
            }
            else if (xMin <= 0 && xMax <= 0)
            {
                if (xMin < xMax)
                {
                    if (_X < xMin || _X > xMax)
                    {
                        return false;
                    }
                }
                else
                {
                    if (_X > xMin || _X < xMax)
                    {
                        return false;
                    }
                }
            }
            else if (xMin <= 0 && xMax >= 0)
            {
                if (_X < xMin || _X > xMax)
                {
                    return false;
                }
            }
            else if (xMin >= 0 && xMax <= 0)
            {
                if (_X > xMin || _X < xMax)
                {
                    return false;
                }
            }
            if (zMin >= 0 && zMax >= 0)
            {
                if (zMin < zMax)
                {
                    if (_Z < zMin || _Z > zMax)
                    {
                        return false;
                    }
                }
                else
                {
                    if (_Z > zMin || _Z < zMax)
                    {
                        return false;
                    }
                }
            }
            else if (zMin <= 0 && zMax <= 0)
            {
                if (zMin < zMax)
                {
                    if (_Z < zMin || _Z > zMax)
                    {
                        return false;
                    }
                }
                else
                {
                    if (_Z > zMin || _Z < zMax)
                    {
                        return false;
                    }
                }
            }
            else if (zMin <= 0 && zMax >= 0)
            {
                if (_Z < zMin || _Z > zMax)
                {
                    return false;
                }
            }
            else if (zMin >= 0 && zMax <= 0)
            {
                if (_Z > zMin || _Z < zMax)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool VectorCircle(float xMin, float zMin, float _X, float _Z, int _radius)
        {
            if ((xMin - _X) * (xMin - _X) + (zMin - _Z) * (zMin - _Z) <= _radius * _radius)
            {
                return true;
            }
            return false;
        }

        public static bool AllowExplosion(Vector3 _worldPos)
        {
            if (Zones.IsEnabled)
            {
                if (Zones.Protected(new Vector3i(_worldPos)))
                {
                    return false;
                }
            }
            if (ProtectedSpaces.IsEnabled)
            {
                if (ProtectedSpaces.IsProtectedSpace(new Vector3i(_worldPos)))
                {
                    return false;
                }
            }
            return true;
        }

        public static void Add(string[] _vectors)
        {
            int _xMin = int.Parse(_vectors[0]), _zMin = int.Parse(_vectors[1]), _xMax = int.Parse(_vectors[2]), _zMax = int.Parse(_vectors[3]);
            int _xMinAlt = _xMin, _zMinAlt = _zMin, _xMaxAlt = _xMax, _zMaxAlt = _zMax;
            if (_xMin > _xMax)
            {
                _xMinAlt = _xMax;
                _xMaxAlt = _xMin;
            }
            if (_zMin > _zMax)
            {
                _zMinAlt = _zMax;
                _zMaxAlt = _zMin;
            }
            List<Chunk> _chunkList = new List<Chunk>();
            string[] _vectorsAlt = { _xMinAlt.ToString(), _zMinAlt.ToString(), _xMaxAlt.ToString(), _zMaxAlt.ToString() };
            ProtectedList.Add(_vectorsAlt);
            for (int i = _xMinAlt; i <= _xMaxAlt; i++)
            {
                for (int j = _zMinAlt; j <= _zMaxAlt; j++)
                {
                    if (GameManager.Instance.World.IsChunkAreaLoaded(i, 1, j))
                    {
                        Chunk _chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(i, 1, j);
                        if (!_chunkList.Contains(_chunk))
                        {
                            _chunkList.Add(_chunk);
                        }
                        Bounds bounds = _chunk.GetAABB();
                        int _x = i - (int)bounds.min.x, _z = j - (int)bounds.min.z;
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
                for (int k = 0; k < _chunkList.Count; k++)
                {
                    Chunk _chunk = _chunkList[k];
                    List<ClientInfo> _clientList = PersistentOperations.ClientList();
                    if (_clientList != null && _clientList.Count > 0)
                    {
                        for (int l = 0; l < _clientList.Count; l++)
                        {
                            ClientInfo _cInfo2 = _clientList[l];
                            if (_cInfo2 != null)
                            {
                                _cInfo2.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(_chunk, true));
                            }
                        }
                    }
                }
            }
            ProtectedSpaces.UpdateXml();
        }

        public static void Remove(string[] _vectors)
        {
            int _xMin = int.Parse(_vectors[0]), _zMin = int.Parse(_vectors[1]), _xMax = int.Parse(_vectors[2]), _zMax = int.Parse(_vectors[3]);
            int _xMinAlt = _xMin, _zMinAlt = _zMin, _xMaxAlt = _xMax, _zMaxAlt = _zMax;
            if (_xMin > _xMax)
            {
                _xMinAlt = _xMax;
                _xMaxAlt = _xMin;
            }
            if (_zMin > _zMax)
            {
                _zMinAlt = _zMax;
                _zMaxAlt = _zMin;
            }
            List<Chunk> _chunkList = new List<Chunk>();
            string[] _vectorsAlt = { _xMinAlt.ToString(), _zMinAlt.ToString(), _xMaxAlt.ToString(), _zMaxAlt.ToString() };
            if (ProtectedList.Contains(_vectorsAlt))
            {
                ProtectedList.Remove(_vectorsAlt);
                for (int i = _xMinAlt; i <= _xMaxAlt; i++)
                {
                    for (int j = _zMinAlt; j <= _zMaxAlt; j++)
                    {
                        if (GameManager.Instance.World.IsChunkAreaLoaded(i, 1, j))
                        {
                            Chunk _chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(i, 1, j);
                            if (!_chunkList.Contains(_chunk))
                            {
                                _chunkList.Add(_chunk);
                            }
                            Bounds bounds = _chunk.GetAABB();
                            int _x = i - (int)bounds.min.x, _z = j - (int)bounds.min.z;
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
                    for (int k = 0; k < _chunkList.Count; k++)
                    {
                        Chunk _chunk = _chunkList[k];
                        List<ClientInfo> _clientList = PersistentOperations.ClientList();
                        if (_clientList != null && _clientList.Count > 0)
                        {
                            for (int l = 0; l < _clientList.Count; l++)
                            {
                                ClientInfo _cInfo2 = _clientList[l];
                                if (_cInfo2 != null)
                                {
                                    _cInfo2.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(_chunk, true));
                                }
                            }
                        }
                    }
                }
                ProtectedSpaces.UpdateXml();
            }
        }



        public static void UnloadProtection()
        {
            List<string[]> _protectedList = ProtectedSpaces.ProtectedList;
            if (_protectedList.Count > 0)
            {
                List<Chunk> _chunkList = new List<Chunk>();
                for (int i = 0; i < _protectedList.Count; i++)
                {
                    string[] _vector = _protectedList[i];
                    int _xMin = int.Parse(_vector[0]), _zMin = int.Parse(_vector[1]), _xMax = int.Parse(_vector[2]), _zMax = int.Parse(_vector[3]);
                    int _xMinAlt = _xMin, _zMinAlt = _zMin, _xMaxAlt = _xMax, _zMaxAlt = _zMax;
                    if (_xMin > _xMax)
                    {
                        _xMinAlt = _xMax;
                        _xMaxAlt = _xMin;
                    }
                    if (_zMin > _zMax)
                    {
                        _zMinAlt = _zMax;
                        _zMaxAlt = _zMin;
                    }
                    for (int j = _xMinAlt; j <= _xMaxAlt; j++)
                    {
                        for (int k = _zMinAlt; k <= _zMaxAlt; k++)
                        {
                            if (GameManager.Instance.World.IsChunkAreaLoaded(j, 1, k))
                            {
                                Chunk _chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(j, 1, k);
                                if (!_chunkList.Contains(_chunk))
                                {
                                    _chunkList.Add(_chunk);
                                }
                                Bounds bounds = _chunk.GetAABB();
                                int _x = i - (int)bounds.min.x, _z = j - (int)bounds.min.z;
                                _chunk.SetTraderArea(_x, _z, false);
                            }
                        }
                    }
                    _protectedList.Remove(_vector);
                }
                ProtectedSpaces.ProtectedList = _protectedList;
                if (_chunkList.Count > 0)
                {
                    for (int l = 0; l < _chunkList.Count; l++)
                    {
                        Chunk _chunk = _chunkList[l];
                        List<ClientInfo> _clientList = PersistentOperations.ClientList();
                        if (_clientList != null && _clientList.Count > 0)
                        {
                            for (int m = 0; m < _clientList.Count; m++)
                            {
                                ClientInfo _cInfo2 = _clientList[m];
                                if (_cInfo2 != null)
                                {
                                    _cInfo2.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(_chunk, true));
                                }
                            }
                        }
                    }
                }
                ProtectedSpaces.UpdateXml();
            }
        }
    }
}
