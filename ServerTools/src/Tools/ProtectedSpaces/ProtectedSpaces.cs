using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    class ProtectedSpaces
    {
        public static bool IsEnabled = false, IsRunning = false;
        public static List<string[]> Protected = new List<string[]>();
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
            List<string[]> _addProtection = new List<string[]>();
            List<string[]> _removeProtection = new List<string[]>();
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
                        int _xMin, _zMin, _xMax, _zMax;
                        if (!int.TryParse(xMin, out _xMin))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring protected space entry because of invalid _XMin integer: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(zMin, out _zMin))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring protected space entry because of invalid _ZMin integer: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(xMax, out _xMax))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring protected space entry because of invalid _XMax integer: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(zMax, out _zMax))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring protected space entry because of invalid _ZMax integer: {0}", subChild.OuterXml));
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
                        string[] _vectors = { _xMinAlt.ToString(), _zMinAlt.ToString(), _xMaxAlt.ToString(), _zMaxAlt.ToString() };
                        if (!Protected.Contains(_vectors))
                        {
                            Protected.Add(_vectors);
                            if (!_addProtection.Contains(_vectors))
                            {
                                _addProtection.Add(_vectors);
                            }
                        }
                    }
                }
            }
            AddProtection(_addProtection);
            if (PersistentContainer.Instance.ProtectedSpace != null && PersistentContainer.Instance.ProtectedSpace.Count > 0)
            {
                List<string[]> _protectedSpaces = PersistentContainer.Instance.ProtectedSpace;
                for (int i = 0; i < _protectedSpaces.Count; i++)
                {
                    if (!_addProtection.Contains(_protectedSpaces[i]))
                    {
                        _removeProtection.Add(_protectedSpaces[i]);
                    }
                }
            }
            RemoveProtection(_removeProtection);
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
                        string[] _vectors = Protected[i];
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

        public static void AddProtection(List<string[]> _vectors)
        {
            List<string[]> _protected = new List<string[]>();
            if (PersistentContainer.Instance.ProtectedSpace != null && PersistentContainer.Instance.ProtectedSpace.Count > 0)
            {
                _protected = PersistentContainer.Instance.ProtectedSpace;
            }
            List<Chunk> _chunkList = new List<Chunk>();
            for (int i = 0; i < _vectors.Count; i++)
            {
                if (_vectors[i].Length < 4)
                {
                    continue;
                }
                int _xMin = int.Parse(_vectors[i][0]), _zMin = int.Parse(_vectors[i][1]), _xMax = int.Parse(_vectors[i][2]), _zMax = int.Parse(_vectors[i][3]);
                int _xMinFix = _xMin, _zMinFix = _zMin, _xMaxFix = _xMax, _zMaxFix = _zMax;
                if (_xMin > _xMax)
                {
                    _xMinFix = _xMax;
                    _xMaxFix = _xMin;
                }
                if (_zMin > _zMax)
                {
                    _zMinFix = _zMax;
                    _zMaxFix = _zMin;
                }
                string[] _vectorsAlt = { _xMinFix.ToString(), _zMinFix.ToString(), _xMaxFix.ToString(), _zMaxFix.ToString() };
                if (!_protected.Contains(_vectorsAlt))
                {
                    _protected.Add(_vectorsAlt);
                    for (int j = _xMinFix; j <= _xMaxFix; j++)
                    {
                        for (int k = _zMinFix; k <= _zMaxFix; k++)
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
            PersistentContainer.Instance.ProtectedSpace = _protected;
        }

        public static void RemoveProtection(List<string[]> _vectors)
        {
            List<string[]> _protected = new List<string[]>();
            if (PersistentContainer.Instance.ProtectedSpace != null && PersistentContainer.Instance.ProtectedSpace.Count > 0)
            {
                _protected = PersistentContainer.Instance.ProtectedSpace;
            }
            List<Chunk> _chunkList = new List<Chunk>();
            for (int i = 0; i < _vectors.Count; i++)
            {
                if (_vectors[i].Length < 4)
                {
                    continue;
                }
                int _xMin = int.Parse(_vectors[i][0]), _zMin = int.Parse(_vectors[i][1]), _xMax = int.Parse(_vectors[i][2]), _zMax = int.Parse(_vectors[i][3]);
                int _xMinFix = _xMin, _zMinFix = _zMin, _xMaxFix = _xMax, _zMaxFix = _zMax;
                if (_xMin > _xMax)
                {
                    _xMinFix = _xMax;
                    _xMaxFix = _xMin;
                }
                if (_zMin > _zMax)
                {
                    _zMinFix = _zMax;
                    _zMaxFix = _zMin;
                }
                string[] _vectorsAlt = { _xMinFix.ToString(), _zMinFix.ToString(), _xMaxFix.ToString(), _zMaxFix.ToString() };
                if (_protected.Contains(_vectorsAlt))
                {
                    _protected.Remove(_vectorsAlt);
                    for (int j = _xMinFix; j <= _xMaxFix; j++)
                    {
                        for (int k = _zMinFix; k <= _zMaxFix; k++)
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
            PersistentContainer.Instance.ProtectedSpace = _protected;
        }

        public static void UnloadProtection()
        {
            List<string[]> _protected = new List<string[]>();
            if (PersistentContainer.Instance.ProtectedSpace != null && PersistentContainer.Instance.ProtectedSpace.Count > 0)
            {
                _protected = PersistentContainer.Instance.ProtectedSpace;
            }
            List<Chunk> _chunkList = new List<Chunk>();
            for (int i = 0; i < _protected.Count; i++)
            {
                int _xMin = int.Parse(_protected[i][0]), _zMin = int.Parse(_protected[i][1]), _xMax = int.Parse(_protected[i][2]), _zMax = int.Parse(_protected[i][3]);
                int _xMinFix = _xMin, _zMinFix = _zMin, _xMaxFix = _xMax, _zMaxFix = _zMax;
                if (_xMin > _xMax)
                {
                    _xMinFix = _xMax;
                    _xMaxFix = _xMin;
                }
                if (_zMin > _zMax)
                {
                    _zMinFix = _zMax;
                    _zMaxFix = _zMin;
                }
                for (int j = _xMinFix; j <= _xMaxFix; j++)
                {
                    for (int k = _zMinFix; k <= _zMaxFix; k++)
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
}
