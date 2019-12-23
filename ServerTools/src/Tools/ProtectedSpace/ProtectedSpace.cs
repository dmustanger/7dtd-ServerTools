using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    class ProtectedSpace
    {
        public static bool IsEnabled = false, IsRunning = false;
        public static List<string[]> ProtectedList = new List<string[]>();
        public static Dictionary<int, string[]> Vectors = new Dictionary<int, string[]>();
        private const string file = "ProtectedSpace.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);

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
                ProtectedList.Clear();
                fileWatcher.Dispose();
                IsRunning = false;
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
                if (childNode.Name == "spaces")
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
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'spaces' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("xMin"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ProtectedSpace entry because of missing xMin attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("zMin"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ProtectedSpace entry because of missing zMin attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("xMax"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ProtectedSpace entry because of missing xMax attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("zMax"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring ProtectedSpace entry because of missing zMax attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string[] _vectors = { _line.GetAttribute("xMin"), _line.GetAttribute("zMin"), _line.GetAttribute("xMax"), _line.GetAttribute("zMax") };
                        if (!ProtectedList.Contains(_vectors))
                        {
                            ProtectedList.Add(_vectors);
                        }
                    }
                }
            }
        }

        private static void UpdateXml()
        {
            fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<Protected>");
                sw.WriteLine("    <spaces>");
                if (ProtectedList.Count > 0)
                {
                    for (int i = 0; i < ProtectedList.Count; i++)
                    {
                        string[] _vectors = ProtectedList[i];
                        sw.WriteLine(string.Format("        <space xMin=\"{0}\" zMin=\"{1}\" xMax=\"{2}\" zMax=\"{3}\" />", _vectors[0], _vectors[1], _vectors[2], _vectors[3]));
                    }
                }
                else
                {
                    sw.WriteLine("        <!-- <space xMin=\"-30\" zMin=\"-20\" xMax=\"10\" zMax=\"50\" /> -->");
                    sw.WriteLine("        <!-- <space xMin=\"-800\" zMin=\"75\" xMax=\"-300\" zMax=\"100\" /> -->");
                    sw.WriteLine("        <!-- <space xMin=\"-50\" zMin=\"-600\" xMax=\"-5\" zMax=\"-550\" /> -->");
                }
                sw.WriteLine("    </spaces>");
                sw.WriteLine("</Protected>");
                sw.Flush();
                sw.Close();
            }
            fileWatcher.EnableRaisingEvents = true;
        }

        private static void InitFileWatcher()
        {
            fileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
            fileWatcher.Created += new FileSystemEventHandler(OnFileChanged);
            fileWatcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            fileWatcher.EnableRaisingEvents = true;
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

        public static bool Exec(Vector3i _position)
        {
            if (ProtectedList.Count > 0)
            {
                for (int i = 0; i < ProtectedList.Count; i++)
                {
                    string[] _vectors = ProtectedList[i];
                    int _xMin, _zMin, _xMax, _zMax;
                    int.TryParse(_vectors[0], out _xMin);
                    int.TryParse(_vectors[1], out _zMin);
                    int.TryParse(_vectors[2], out _xMax);
                    int.TryParse(_vectors[3], out _zMax);
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
            if (Lobby.IsEnabled && Lobby.Protected && Lobby.Lobby_Position != "0,0,0")
            {
                if (Lobby.ProtectedSpace((int)_worldPos.x, (int)_worldPos.z))
                {
                    return false;
                }
            }
            if (Market.IsEnabled && Market.Protected && Market.Market_Position != "0,0,0")
            {
                if (Market.ProtectedSpace((int)_worldPos.x, (int)_worldPos.z))
                {
                    return false;
                }
            }
            if (Zones.IsEnabled)
            {
                if (Zones.Protected(new Vector3i(_worldPos)))
                {
                    return false;
                }
            }
            if (ProtectedSpace.IsEnabled)
            {
                if (ProtectedSpace.Exec(new Vector3i(_worldPos)))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool Protected(Vector3i _worldPos)
        {
            if (Lobby.IsEnabled && Lobby.Protected && Lobby.Lobby_Position != "0,0,0")
            {
                if (Lobby.ProtectedSpace((int)_worldPos.x, (int)_worldPos.z))
                {
                    return true;
                }
            }
            if (Market.IsEnabled && Market.Protected && Market.Market_Position != "0,0,0")
            {
                if (Market.ProtectedSpace((int)_worldPos.x, (int)_worldPos.z))
                {
                    return true;
                }
            }
            if (Zones.IsEnabled)
            {
                if (Zones.Protected(_worldPos))
                {
                    return true;
                }
            }
            if (ProtectedSpace.IsEnabled)
            {
                if (ProtectedSpace.Exec(_worldPos))
                {
                    return true;
                }
            }
            return false;
        }

        public static void Add(int xMin, int zMin, int xMax, int zMax)
        {
            List<Chunk> _chunkList = new List<Chunk>();
            for (int i = xMin; i <= xMax; i++)
            {
                for (int j = 1; i <= 200; j++)
                {
                    for (int k = zMin; k <= zMax; k++)
                    {
                        if (GameManager.Instance.World.IsChunkAreaLoaded(i, j, k))
                        {
                            Chunk _chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(i, j, k);
                            if (!_chunkList.Contains(_chunk))
                            {
                                _chunkList.Add(_chunk);
                            }
                            Bounds bounds = _chunk.GetAABB();
                            _chunk.SetTraderArea(i - ((int)bounds.min.x), k - ((int)bounds.min.z), true);
                        }
                        else
                        {

                            SdtdConsole.Instance.Output(string.Format("{0}x,{1}z has not been loaded. Protection was skipped for this area", i, k));
                            continue;
                        }
                    }
                }
            }
            for (int i = 0; i <= _chunkList.Count; i++)
            {
                Chunk _chunk = _chunkList[i];
                List<ClientInfo> _clientList = PersistentOperations.ClientsWithin200Blocks(_chunk.GetWorldPos().x, _chunk.GetWorldPos().z);
                if (_clientList != null)
                {
                    for (int j = 0; j < _clientList.Count; j++)
                    {
                        ClientInfo _cInfo = _clientList[j];
                        //_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChunkRemove>().Setup(_chunk.Key));
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(_chunk, true));
                    }
                }
            }
        }
    }
}
