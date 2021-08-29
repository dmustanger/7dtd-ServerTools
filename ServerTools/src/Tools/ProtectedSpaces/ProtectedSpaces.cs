using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        public static void Unload()
        {
            Protected.Clear();
            Vectors.Clear();
            FileWatcher.Dispose();
            IsRunning = false;
        }

        public static void LoadXml()
        {
            try
            {
                if (!Utils.FileExists(FilePath))
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
                XmlNodeList _childNodes = xmlDoc.DocumentElement.ChildNodes;
                if (_childNodes != null && _childNodes.Count > 0)
                {
                    Protected.Clear();
                    Vectors.Clear();
                    for (int i = 0; i < _childNodes.Count; i++)
                    {
                        if (_childNodes[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement _line = (XmlElement)_childNodes[i];
                            if (_line.HasAttributes)
                            {
                                if (_line.HasAttribute("Version") && _line.GetAttribute("Version") != Config.Version)
                                {
                                    UpgradeXml(_childNodes);
                                    return;
                                }
                                else if (_line.HasAttribute("Corner1") && _line.HasAttribute("Corner2") && _line.HasAttribute("Active"))
                                {
                                    string corner1 = _line.GetAttribute("Corner1");
                                    string corner2 = _line.GetAttribute("Corner2");
                                    if (!corner1.Contains(","))
                                    {
                                        Log.Warning(string.Format("[SERVERTOOLS] Ignoring ProtectedSpaces.xml entry. Invalid form missing comma attribute: {0}", _line.OuterXml));
                                        continue;
                                    }
                                    if (!corner2.Contains(","))
                                    {
                                        Log.Warning(string.Format("[SERVERTOOLS] Ignoring ProtectedSpaces.xml entry. Invalid form missing comma attribute: {0}", _line.OuterXml));
                                        continue;
                                    }
                                    if (!bool.TryParse(_line.GetAttribute("Active"), out bool _isActive))
                                    {
                                        Log.Warning(string.Format("[SERVERTOOLS] Ignoring ProtectedSpaces.xml entry. Invalid True/False for Active attribute: {0}.", _line.OuterXml));
                                        continue;
                                    }
                                    string[] _corner1Split = corner1.Split(',');
                                    string[] _corner2Split = corner2.Split(',');
                                    int.TryParse(_corner1Split[0], out int _corner1_x);
                                    int.TryParse(_corner1Split[1], out int _corner1_z);
                                    int.TryParse(_corner2Split[0], out int _corner2_x);
                                    int.TryParse(_corner2Split[1], out int _corner2_z);
                                    int[] _vectors = new int[5];
                                    if (_corner1_x < _corner2_x)
                                    {
                                        _vectors[0] = _corner1_x;
                                        _vectors[2] = _corner2_x;
                                    }
                                    else
                                    {
                                        _vectors[0] = _corner2_x;
                                        _vectors[2] = _corner1_x;
                                    }
                                    if (_corner1_z < _corner2_z)
                                    {
                                        _vectors[1] = _corner1_z;
                                        _vectors[3] = _corner2_z;
                                    }
                                    else
                                    {
                                        _vectors[1] = _corner2_z;
                                        _vectors[3] = _corner1_z;
                                    }
                                    if (_isActive)
                                    {
                                        _vectors[4] = 1;
                                    }
                                    else
                                    {
                                        _vectors[4] = 0;
                                    }
                                    if (!Protected.Contains(_vectors))
                                    {
                                        Protected.Add(_vectors);
                                        if (_vectors[4] == 1)
                                        {
                                            AddProtection(_vectors);
                                        }
                                        else
                                        {
                                            RemoveProtection(_vectors);
                                        }
                                    }
                                    else if (_vectors[4] == 1)
                                    {
                                        AddProtection(_vectors);
                                    }
                                    else
                                    {
                                        RemoveProtection(_vectors);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ProtectedSpaces.LoadXml: {0}", e.Message));
            }
        }

        public static void UpdateXml()
        {
            FileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<Protected>");
                sw.WriteLine("<!-- <Protected Corner1=\"-30,-20\" Corner2=\"10,50\" Active=\"True\" /> -->");
                sw.WriteLine();
                sw.WriteLine();
                if (Protected.Count > 0)
                {
                    for (int i = 0; i < Protected.Count; i++)
                    {
                        if (Protected[i][4] == 1)
                        {
                            sw.WriteLine(string.Format("    <Protected Corner1=\"{0},{1}\" Corner2=\"{2},{3}\" Active=\"True\" />", Protected[i][0], Protected[i][1], Protected[i][2], Protected[i][3]));
                        }
                        else
                        {
                            sw.WriteLine(string.Format("    <Protected Corner1=\"{0},{1}\" Corner2=\"{2},{3}\" Active=\"False\" />", Protected[i][0], Protected[i][1], Protected[i][2], Protected[i][3]));
                        }
                    }
                }
                else
                {
                    sw.WriteLine("    <!-- <Protected Corner1=\"\" Corner2=\"\" Active=\"\" /> -->");
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
            if (!Utils.FileExists(FilePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        public static void AddProtection(int[] _vectors)
        {
            try
            {
                if (_vectors != null && _vectors.Length == 5)
                {
                    List<int[]> _loadedLocations = new List<int[]>();
                    List<int[]> _unloadedLocations = new List<int[]>();
                    for (int i = _vectors[0]; i <= _vectors[2]; i++)
                    {
                        for (int j = _vectors[1]; j <= _vectors[3]; j++)
                        {
                            if (!GameManager.Instance.World.IsWithinTraderArea(new Vector3i(i, 1, j)))
                            {
                                if (GameManager.Instance.World.IsChunkAreaLoaded(i, 1, j))
                                {

                                    _loadedLocations.Add(new int[] { i, j });
                                }
                                else
                                {
                                    _unloadedLocations.Add(new int[] { i, j });
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                    if (_loadedLocations.Count > 0)
                    {
                        List<Chunk> _chunkList = new List<Chunk>();
                        for (int i = 0; i < _loadedLocations.Count; i++)
                        {
                            Chunk _chunk = GameManager.Instance.World.GetChunkFromWorldPos(_loadedLocations[i][0], 1, _loadedLocations[i][1]) as Chunk;
                            if (!_chunkList.Contains(_chunk))
                            {
                                _chunkList.Add(_chunk);
                            }
                            Bounds bounds = _chunk.GetAABB();
                            int _x = _loadedLocations[i][0] - (int)bounds.min.x, _z = _loadedLocations[i][1] - (int)bounds.min.z;
                            _chunk.SetTraderArea(_x, _z, true);
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
                                        ClientInfo _cInfo = _clientList[j];
                                        if (_cInfo != null)
                                        {
                                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(_chunk, true));
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (_unloadedLocations.Count > 0)
                    {
                        for (int i = 0; i < _unloadedLocations.Count; i++)
                        {
                            if (GameManager.Instance.World.GetChunkSync(_unloadedLocations[i][0], _unloadedLocations[i][1]) is Chunk)
                            {
                                Chunk _chunk = GameManager.Instance.World.GetChunkSync(_unloadedLocations[i][0], 1, _unloadedLocations[i][1]) as Chunk;
                                Bounds bounds = _chunk.GetAABB();
                                int _x = _unloadedLocations[i][0] - (int)bounds.min.x, _z = _unloadedLocations[i][1] - (int)bounds.min.z;
                                _chunk.SetTraderArea(_x, _z, true);
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
                if (_vectors != null && _vectors.Length == 5)
                {
                    List<int[]> _loadedLocations = new List<int[]>();
                    List<int[]> _unloadedLocations = new List<int[]>();
                    for (int i = _vectors[0]; i <= _vectors[2]; i++)
                    {
                        for (int j = _vectors[1]; j <= _vectors[3]; j++)
                        {
                            if (GameManager.Instance.World.IsChunkAreaLoaded(i, 1, j))
                            {
                                _loadedLocations.Add(new int[] { i, j });
                            }
                            else
                            {
                                _unloadedLocations.Add(new int[] { i, j });
                            }
                        }
                    }
                    if (_loadedLocations.Count > 0)
                    {
                        List<Chunk> _chunkList = new List<Chunk>();
                        for (int i = 0; i < _loadedLocations.Count; i++)
                        {
                            Chunk _chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(_loadedLocations[i][0], 1, _loadedLocations[i][1]);
                            if (!_chunkList.Contains(_chunk))
                            {
                                _chunkList.Add(_chunk);
                            }
                            Bounds bounds = _chunk.GetAABB();
                            int _x = _loadedLocations[i][0] - (int)bounds.min.x, _z = _loadedLocations[i][1] - (int)bounds.min.z;
                            _chunk.SetTraderArea(_x, _z, false);
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
                                        ClientInfo _cInfo = _clientList[j];
                                        if (_cInfo != null)
                                        {
                                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(_chunk, true));
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (_unloadedLocations.Count > 0)
                    {
                        for (int i = 0; i < _unloadedLocations.Count; i++)
                        {
                            if (GameManager.Instance.World.GetChunkSync(_unloadedLocations[i][0], _unloadedLocations[i][1]) is Chunk)
                            {
                                Chunk _chunk = GameManager.Instance.World.GetChunkSync(_unloadedLocations[i][0], 1, _unloadedLocations[i][1]) as Chunk;
                                Bounds bounds = _chunk.GetAABB();
                                int _x = _unloadedLocations[i][0] - (int)bounds.min.x, _z = _unloadedLocations[i][1] - (int)bounds.min.z;
                                _chunk.SetTraderArea(_x, _z, false);
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

        private static void UpgradeXml(XmlNodeList _oldChildNodes)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<Protected>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("<!-- <Protected Corner1=\"-30,-20\" Corner2=\"10,50\" Active=\"True\" /> -->");
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType == XmlNodeType.Comment && !_oldChildNodes[i].OuterXml.StartsWith("<!-- <Protected Corner1=\"-30,-20\"") &&
                            !_oldChildNodes[i].OuterXml.StartsWith("    <!-- <Protected Corner1=\"\""))
                        {
                            sw.WriteLine(_oldChildNodes[i].OuterXml);
                        }
                    }
                    sw.WriteLine();
                    sw.WriteLine();
                    bool _blank = true;
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement _line = (XmlElement)_oldChildNodes[i];
                            if (_line.HasAttributes && _line.Name == "Protected")
                            {
                                _blank = false;
                                string _corner1 = "", _corner2 = "", _active = "";
                                if (_line.HasAttribute("Corner1"))
                                {
                                    _corner1 = _line.GetAttribute("Corner1");
                                }
                                if (_line.HasAttribute("Corner2"))
                                {
                                    _corner2 = _line.GetAttribute("Corner2");
                                }
                                if (_line.HasAttribute("Active"))
                                {
                                    _active = _line.GetAttribute("Active");
                                }
                                sw.WriteLine(string.Format("    <Protected Corner1=\"{0}\" Corner2=\"{1}\" Active=\"{2}\" />", _corner1, _corner2, _active));
                            }
                        }
                    }
                    if (_blank)
                    {
                        sw.WriteLine("    <!-- <Protected Corner1=\"\" Corner2=\"\" Active=\"\" /> -->");
                    }
                    sw.WriteLine("</Protected>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ProtectedSpaces.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}
