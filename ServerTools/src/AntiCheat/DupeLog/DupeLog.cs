using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    class DupeLog
    {
        public static bool IsEnabled = false, IsRunning = false;

        public static List<string> Dict = new List<string>();

        private static readonly Dictionary<int, ItemStack[]> Bag = new Dictionary<int, ItemStack[]>();
        private static readonly Dictionary<int, ItemStack[]> Inventory = new Dictionary<int, ItemStack[]>();
        private static readonly Dictionary<int, int> Crafted = new Dictionary<int, int>();

        private const string file = "DuplicateItemExemption.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static readonly string DupeFile = string.Format("DupeLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static readonly string DupeFilepath = string.Format("{0}/Logs/DupeLogs/{1}", API.ConfigPath, DupeFile);
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        private static XmlNodeList OldNodeList;

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        public static void Unload()
        {

            Dict.Clear();
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
                bool upgrade = true;
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                if (childNodes != null)
                {
                    Dict.Clear();
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
                                else if (line.HasAttribute("Name"))
                                {
                                    string name = line.GetAttribute("Name");
                                    if (!Dict.Contains(name))
                                    {
                                        Dict.Add(name);
                                    }
                                }
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
                            Log.Out(string.Format("[SERVERTOOLS] The existing DuplicateItemExemption.xml was too old or misconfigured. File deleted and rebuilt for version {0}", Config.Version));
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
                    Log.Out(string.Format("[SERVERTOOLS] Error in DupeLog.LoadXml: {0}", e.Message));
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
                    sw.WriteLine("<DuplicateItemExemption>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("    <!-- <Item Name=\"\" /> -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    if (Dict.Count > 0)
                    {
                        for (int i = 0; i < Dict.Count; i++)
                        {
                            string _name = Dict[i];
                            sw.WriteLine(string.Format("    <Item Name=\"{0}\" />", _name));
                        }
                    }
                    sw.WriteLine("</DuplicateItemExemption>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in DupeLog.UpdateXml: {0}", e.Message));
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

        public static void Exec(ClientInfo _cInfo, PlayerDataFile _playerDataFile)
        {
            try
            {
                if (_cInfo != null && _playerDataFile != null && _playerDataFile.bag != null && _playerDataFile.inventory != null)
                {
                    ItemStack[] _bag = _playerDataFile.bag;
                    ItemStack[] _inventory = _playerDataFile.inventory;

                    if (Crafted.TryGetValue(_cInfo.entityId, out int _craftCount))
                    {
                        if ((int)_playerDataFile.totalItemsCrafted != _craftCount)
                        {
                            Crafted[_cInfo.entityId] = (int)_playerDataFile.totalItemsCrafted;
                            Bag[_cInfo.entityId] = _bag;
                            Inventory[_cInfo.entityId] = _inventory;
                            return;
                        }
                    }
                    else
                    {
                        Crafted.Add(_cInfo.entityId, (int)_playerDataFile.totalItemsCrafted);
                        Bag.Add(_cInfo.entityId, _bag);
                        Inventory.Add(_cInfo.entityId, _inventory);
                        return;
                    }

                    List<int> BagSlot = new List<int>();
                    List<int> InvSlot = new List<int>();
                    ItemStack _bagStackOld, _bagStackNew, _invStackOld, _invStackNew, _CompareBagOld, _CompareBagNew,
                    _CompareInvOld, _CompareInvNew;

                    Bag.TryGetValue(_cInfo.entityId, out ItemStack[]  _bagStacks);
                    {
                        Inventory.TryGetValue(_cInfo.entityId, out ItemStack[] _invStacks);
                        {
                            int _bagSize = _bag.Length;
                            int _invSize = _inventory.Length;
                            for (int i = 0; i < _bagSize; i++)
                            {
                                _bagStackOld = _bagStacks[i];
                                _bagStackNew = _bag[i];
                                bool BagNext = true;
                                int _oldTotal = 0, _newTotal = 0, _newCount;
                                if (!_bagStackNew.Equals(_bagStackOld) && !_bagStackNew.IsEmpty())
                                {
                                    string _name = _bagStackNew.itemValue.ItemClass.Name;
                                    if (!Dict.Contains(_name))
                                    {
                                        for (int j = 0; j < _bagSize; j++)
                                        {
                                            _CompareBagOld = _bagStacks[j];
                                            if (!_CompareBagOld.IsEmpty() && _name == _CompareBagOld.itemValue.ItemClass.Name)
                                            {
                                                _newCount = _oldTotal + _CompareBagOld.count;
                                                _oldTotal = _newCount;
                                            }
                                        }
                                        for (int j = 0; j < _invSize; j++)
                                        {
                                            _CompareInvOld = _invStacks[j];
                                            if (!_CompareInvOld.IsEmpty() && _name == _CompareInvOld.itemValue.ItemClass.Name)
                                            {
                                                _newCount = _oldTotal + _CompareInvOld.count;
                                                _oldTotal = _newCount;
                                            }
                                        }

                                        for (int j = 0; j < _bagSize; j++)
                                        {
                                            _CompareBagNew = _bag[j];
                                            if (!_CompareBagNew.IsEmpty() && _name == _CompareBagNew.itemValue.ItemClass.Name)
                                            {
                                                _newCount = _newTotal + _CompareBagNew.count;
                                                _newTotal = _newCount;
                                            }
                                        }
                                        for (int j = 0; j < _invSize; j++)
                                        {
                                            _CompareInvNew = _inventory[j];
                                            if (!_CompareInvNew.IsEmpty() && _name == _CompareInvNew.itemValue.ItemClass.Name)
                                            {
                                                _newCount = _newTotal + _CompareInvNew.count;
                                                _newTotal = _newCount;
                                            }
                                        }
                                        if (_oldTotal == _newTotal)
                                        {
                                            BagNext = false;
                                        }
                                        if (BagNext)
                                        {
                                            if (_bagStackNew.count == 1)
                                            {
                                                int _counter1 = 0, _counter2 = 0;
                                                for (int j = 0; j < _bagSize; j++)
                                                {
                                                    _CompareBagNew = _bag[j];
                                                    if (!_CompareBagNew.IsEmpty() && _name == _CompareBagNew.itemValue.ItemClass.Name && _CompareBagNew.count == 1)
                                                    {
                                                        _counter1++;
                                                    }
                                                }
                                                for (int j = 0; j < _invSize; j++)
                                                {
                                                    _CompareInvNew = _inventory[j];
                                                    if (!_CompareInvNew.IsEmpty() && _name == _CompareInvNew.itemValue.ItemClass.Name && _CompareInvNew.count == 1)
                                                    {
                                                        _counter2++;
                                                    }
                                                }
                                                if (_counter1 + _counter2 > 1)
                                                {
                                                    for (int j = 0; j < _bagSize; j++)
                                                    {
                                                        _CompareBagOld = _bagStacks[j];
                                                        if (!_CompareBagOld.IsEmpty() && _name == _CompareBagOld.itemValue.ItemClass.Name && _CompareBagOld.count >= _counter1)
                                                        {
                                                            BagNext = false;
                                                        }
                                                    }
                                                    if (BagNext)
                                                    {
                                                        for (int j = 0; j < _invSize; j++)
                                                        {
                                                            _CompareInvOld = _invStacks[j];
                                                            if (!_CompareInvOld.IsEmpty() && _name == _CompareInvOld.itemValue.ItemClass.Name && _CompareInvOld.count >= _counter1)
                                                            {
                                                                BagNext = false;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        if (BagNext)
                                        {
                                            if (_bagStackNew.itemValue.HasQuality)
                                            {
                                                for (int j = 0; j < _bagSize; j++)
                                                {
                                                    _CompareBagNew = _bag[j];
                                                    if (!_CompareBagNew.IsEmpty() && _bagStackNew.Equals(_CompareBagNew) && i != j && !BagSlot.Contains(j))
                                                    {
                                                        BagSlot.Add(i);
                                                        EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                                        PersistentPlayerData _persistentPlayerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_player.entityId);
                                                        EnumLandClaimOwner _owner = GameManager.Instance.World.GetLandClaimOwner(new Vector3i(_player.position.x, _player.position.y, _player.position.z), _persistentPlayerData);
                                                        if (_owner == EnumLandClaimOwner.Self || _owner == EnumLandClaimOwner.Ally)
                                                        {
                                                            using (StreamWriter sw = new StreamWriter(DupeFilepath, true, Encoding.UTF8))
                                                            {
                                                                sw.WriteLine(string.Format("{0}: {1} {2} has added {3} with quality {4} to their bag inside their own or ally claimed space at {5} {6} {7}.", DateTime.Now, _cInfo.PlatformId.ReadablePlatformUserIdentifier, _cInfo.playerName, _name, _bagStackNew.itemValue.Quality, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                                sw.WriteLine();
                                                                sw.Flush();
                                                                sw.Close();
                                                            }
                                                        }
                                                        else
                                                        {
                                                            using (StreamWriter sw = new StreamWriter(DupeFilepath, true, Encoding.UTF8))
                                                            {
                                                                sw.WriteLine(string.Format("{0}: {1} {2} has added {3} with quality {4} to their bag at {5} {6} {7}.", DateTime.Now, _cInfo.PlatformId.ReadablePlatformUserIdentifier, _cInfo.playerName, _name, _bagStackNew.itemValue.Quality, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                                sw.WriteLine();
                                                                sw.Flush();
                                                                sw.Close();
                                                            }
                                                        }
                                                    }
                                                }
                                                for (int j = 0; j < _invSize; j++)
                                                {
                                                    _CompareInvNew = _inventory[j];
                                                    if (!_CompareInvNew.IsEmpty() && _bagStackNew.Equals(_CompareInvNew) && !InvSlot.Contains(j))
                                                    {
                                                        InvSlot.Add(i);
                                                        EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                                        PersistentPlayerData _persistentPlayerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_player.entityId);
                                                        EnumLandClaimOwner _owner = GameManager.Instance.World.GetLandClaimOwner(new Vector3i(_player.position.x, _player.position.y, _player.position.z), _persistentPlayerData);
                                                        if (_owner == EnumLandClaimOwner.Self || _owner == EnumLandClaimOwner.Ally)
                                                        {
                                                            using (StreamWriter sw = new StreamWriter(DupeFilepath, true, Encoding.UTF8))
                                                            {
                                                                sw.WriteLine(string.Format("{0}: {1} {2} has added {3} with quality {4} to their inventory inside their own or ally claimed space at {5} {6} {7}.", DateTime.Now, _cInfo.PlatformId.ReadablePlatformUserIdentifier, _cInfo.playerName, _name, _bagStackNew.itemValue.Quality, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                                sw.WriteLine();
                                                                sw.Flush();
                                                                sw.Close();
                                                            }
                                                        }
                                                        else
                                                        {
                                                            using (StreamWriter sw = new StreamWriter(DupeFilepath, true, Encoding.UTF8))
                                                            {
                                                                sw.WriteLine(string.Format("{0}: {1} {2} has added {3} with quality {4} to their inventory at {5} {6} {7}.", DateTime.Now, _cInfo.PlatformId.ReadablePlatformUserIdentifier, _cInfo.playerName, _name, _bagStackNew.itemValue.Quality, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                                sw.WriteLine();
                                                                sw.Flush();
                                                                sw.Close();
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (_bagStackNew.count > 1)
                                                {
                                                    for (int j = 0; j < _bagSize; j++)
                                                    {
                                                        _CompareBagNew = _bag[j];
                                                        if (!_CompareBagNew.IsEmpty() && _bagStackNew.Equals(_CompareBagNew) && i != j && !BagSlot.Contains(j))
                                                        {
                                                            BagSlot.Add(i);
                                                            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                                            PersistentPlayerData _persistentPlayerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_player.entityId);
                                                            EnumLandClaimOwner _owner = GameManager.Instance.World.GetLandClaimOwner(new Vector3i(_player.position.x, _player.position.y, _player.position.z), _persistentPlayerData);
                                                            if (_owner == EnumLandClaimOwner.Self || _owner == EnumLandClaimOwner.Ally)
                                                            {
                                                                using (StreamWriter sw = new StreamWriter(DupeFilepath, true, Encoding.UTF8))
                                                                {
                                                                    sw.WriteLine(string.Format("{0}: {1} {2} has added {3} {4} to their bag, identical to another stack, inside their own or ally claimed space at {5} {6} {7}.", DateTime.Now, _cInfo.PlatformId.ReadablePlatformUserIdentifier, _cInfo.playerName, _bagStackNew.count, _name, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                                    sw.WriteLine();
                                                                    sw.Flush();
                                                                    sw.Close();
                                                                }
                                                            }
                                                            else
                                                            {
                                                                using (StreamWriter sw = new StreamWriter(DupeFilepath, true, Encoding.UTF8))
                                                                {
                                                                    sw.WriteLine(string.Format("{0}: {1} {2} has added {3} {4} to their bag, identical to another stack at {5} {6} {7}.", DateTime.Now, _cInfo.PlatformId.ReadablePlatformUserIdentifier, _cInfo.playerName, _bagStackNew.count, _name, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                                    sw.WriteLine();
                                                                    sw.Flush();
                                                                    sw.Close();
                                                                }
                                                            }
                                                        }
                                                    }
                                                    for (int j = 0; j < _invSize; j++)
                                                    {
                                                        _CompareInvNew = _inventory[j];
                                                        if (!_CompareInvNew.IsEmpty() && _bagStackNew.Equals(_CompareInvNew) && !InvSlot.Contains(j))
                                                        {
                                                            InvSlot.Add(i);
                                                            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                                            PersistentPlayerData _persistentPlayerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_player.entityId);
                                                            EnumLandClaimOwner _owner = GameManager.Instance.World.GetLandClaimOwner(new Vector3i(_player.position.x, _player.position.y, _player.position.z), _persistentPlayerData);
                                                            if (_owner == EnumLandClaimOwner.Self || _owner == EnumLandClaimOwner.Ally)
                                                            {
                                                                using (StreamWriter sw = new StreamWriter(DupeFilepath, true, Encoding.UTF8))
                                                                {
                                                                    sw.WriteLine(string.Format("{0}: {1} {2} has added {3} {4} to their bag, identical to another stack, inside their own or ally claimed space at {5} {6} {7}.", DateTime.Now, _cInfo.PlatformId.ReadablePlatformUserIdentifier, _cInfo.playerName, _bagStackNew.count, _name, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                                    sw.WriteLine();
                                                                    sw.Flush();
                                                                    sw.Close();
                                                                }
                                                            }
                                                            else
                                                            {
                                                                using (StreamWriter sw = new StreamWriter(DupeFilepath, true, Encoding.UTF8))
                                                                {
                                                                    sw.WriteLine(string.Format("{0}: {1} {2} has added {3} {4} to their bag, identical to another stack at {5} {6} {7}.", DateTime.Now, _cInfo.PlatformId.ReadablePlatformUserIdentifier, _cInfo.playerName, _bagStackNew.count, _name, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                                    sw.WriteLine();
                                                                    sw.Flush();
                                                                    sw.Close();
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            for (int i = 0; i < _invSize; i++)
                            {
                                bool InvNext = true;
                                _invStackOld = _invStacks[i];
                                _invStackNew = _inventory[i];
                                if (!_invStackNew.Equals(_invStackOld))
                                {
                                    if (!_invStackNew.IsEmpty())
                                    {
                                        string _invName = _invStackNew.itemValue.ItemClass.Name;
                                        if (!Dict.Contains(_invName))
                                        {
                                            int _oldTotal = 0, _newTotal = 0, _newCount;
                                            for (int j = 0; j < _invSize; j++)
                                            {
                                                _CompareInvOld = _invStacks[j];
                                                if (!_CompareInvOld.IsEmpty() && _invName == _CompareInvOld.itemValue.ItemClass.Name)
                                                {
                                                    _newCount = _oldTotal + _CompareInvOld.count;
                                                    _oldTotal = _newCount;
                                                }
                                            }
                                            for (int j = 0; j < _bagSize; j++)
                                            {
                                                _CompareBagOld = _bagStacks[j];
                                                if (!_CompareBagOld.IsEmpty() && _invName == _CompareBagOld.itemValue.ItemClass.Name)
                                                {
                                                    _newCount = _oldTotal + _CompareBagOld.count;
                                                    _oldTotal = _newCount;
                                                }
                                            }
                                            for (int j = 0; j < _invSize; j++)
                                            {
                                                _CompareInvNew = _inventory[j];
                                                if (!_CompareInvNew.IsEmpty() && _invName == _CompareInvNew.itemValue.ItemClass.Name)
                                                {
                                                    _newCount = _newTotal + _CompareInvNew.count;
                                                    _newTotal = _newCount;
                                                }
                                            }
                                            for (int j = 0; j < _bagSize; j++)
                                            {
                                                _CompareBagNew = _bag[j];
                                                if (!_CompareBagNew.IsEmpty() && _invName == _CompareBagNew.itemValue.ItemClass.Name)
                                                {
                                                    _newCount = _newTotal + _CompareBagNew.count;
                                                    _newTotal = _newCount;
                                                }
                                            }
                                            if (_oldTotal == _newTotal)
                                            {
                                                InvNext = false;
                                            }
                                            if (InvNext)
                                            {
                                                if (_invStackNew.count == 1)
                                                {
                                                    int _counter1 = 0, _counter2 = 0;
                                                    for (int j = 0; j < _invSize; j++)
                                                    {
                                                        _CompareInvNew = _inventory[j];
                                                        if (!_CompareInvNew.IsEmpty() && _invName == _CompareInvNew.itemValue.ItemClass.Name && _CompareInvNew.count == 1)
                                                        {
                                                            _counter1++;
                                                        }
                                                    }
                                                    for (int j = 0; j < _bagSize; j++)
                                                    {
                                                        _CompareBagNew = _bag[j];
                                                        if (!_CompareBagNew.IsEmpty() && _invName == _CompareBagNew.itemValue.ItemClass.Name && _CompareBagNew.count == 1)
                                                        {
                                                            _counter2++;
                                                        }
                                                    }
                                                    if (_counter1 + _counter2 > 1)
                                                    {
                                                        for (int j = 0; j < _invSize; j++)
                                                        {
                                                            _CompareInvOld = _invStacks[j];
                                                            if (!_CompareInvOld.IsEmpty() && _invName == _CompareInvOld.itemValue.ItemClass.Name && _CompareInvOld.count >= _counter1)
                                                            {
                                                                InvNext = false;
                                                            }
                                                        }
                                                        if (InvNext)
                                                        {
                                                            for (int j = 0; j < _bagSize; j++)
                                                            {
                                                                _CompareBagOld = _bagStacks[j];
                                                                if (!_CompareBagOld.IsEmpty() && _invName == _CompareBagOld.itemValue.ItemClass.Name && _CompareBagOld.count >= _counter1)
                                                                {
                                                                    InvNext = false;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            if (InvNext)
                                            {
                                                if (_invStackNew.itemValue.HasQuality)
                                                {
                                                    for (int j = 0; j < _invSize; j++)
                                                    {
                                                        _CompareInvNew = _inventory[j];
                                                        if (!_CompareInvNew.IsEmpty() && _invStackNew.Equals(_CompareInvNew) && i != j && !InvSlot.Contains(j))
                                                        {
                                                            InvSlot.Add(i);
                                                            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                                            PersistentPlayerData _persistentPlayerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_player.entityId);
                                                            EnumLandClaimOwner _owner = GameManager.Instance.World.GetLandClaimOwner(new Vector3i(_player.position.x, _player.position.y, _player.position.z), _persistentPlayerData);
                                                            if (_owner == EnumLandClaimOwner.Self || _owner == EnumLandClaimOwner.Ally)
                                                            {
                                                                using (StreamWriter sw = new StreamWriter(DupeFilepath, true, Encoding.UTF8))
                                                                {
                                                                    sw.WriteLine(string.Format("{0}: {1} {2} has added {3} with quality {4} to their inventory inside their own or ally claimed space at {5} {6} {7}.", DateTime.Now, _cInfo.PlatformId.ReadablePlatformUserIdentifier, _cInfo.playerName, _invName, _invStackNew.itemValue.Quality, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                                    sw.WriteLine();
                                                                    sw.Flush();
                                                                    sw.Close();
                                                                }
                                                            }
                                                            else
                                                            {
                                                                using (StreamWriter sw = new StreamWriter(DupeFilepath, true, Encoding.UTF8))
                                                                {
                                                                    sw.WriteLine(string.Format("{0}: {1} {2} has added {3} with quality {4} to their inventory, standing at {5} {6} {7}.", DateTime.Now, _cInfo.PlatformId.ReadablePlatformUserIdentifier, _cInfo.playerName, _invName, _invStackNew.itemValue.Quality, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                                    sw.WriteLine();
                                                                    sw.Flush();
                                                                    sw.Close();
                                                                }
                                                            }
                                                        }
                                                    }
                                                    for (int j = 0; j < _bagSize; j++)
                                                    {
                                                        _CompareBagNew = _bag[j];
                                                        if (!_CompareBagNew.IsEmpty() && _invStackNew.Equals(_CompareBagNew) && !InvSlot.Contains(j))
                                                        {
                                                            InvSlot.Add(i);
                                                            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                                            PersistentPlayerData _persistentPlayerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_player.entityId);
                                                            EnumLandClaimOwner _owner = GameManager.Instance.World.GetLandClaimOwner(new Vector3i(_player.position.x, _player.position.y, _player.position.z), _persistentPlayerData);
                                                            if (_owner == EnumLandClaimOwner.Self || _owner == EnumLandClaimOwner.Ally)
                                                            {
                                                                using (StreamWriter sw = new StreamWriter(DupeFilepath, true, Encoding.UTF8))
                                                                {
                                                                    sw.WriteLine(string.Format("{0}: {1} {2} has added {3} with quality {4} to their inventory inside their own or ally claimed space at {5} {6} {7}.", DateTime.Now, _cInfo.PlatformId.ReadablePlatformUserIdentifier, _cInfo.playerName, _invName, _invStackNew.itemValue.Quality, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                                    sw.WriteLine();
                                                                    sw.Flush();
                                                                    sw.Close();
                                                                }
                                                            }
                                                            else
                                                            {
                                                                using (StreamWriter sw = new StreamWriter(DupeFilepath, true, Encoding.UTF8))
                                                                {
                                                                    sw.WriteLine(string.Format("{0}: {1} {2} has added {3} with quality {4} to their inventory, standing at {5} {6} {7}.", DateTime.Now, _cInfo.PlatformId.ReadablePlatformUserIdentifier, _cInfo.playerName, _invName, _invStackNew.itemValue.Quality, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                                    sw.WriteLine();
                                                                    sw.Flush();
                                                                    sw.Close();
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (_invStackNew.count > 1)
                                                    {
                                                        for (int j = 0; j < _invSize; j++)
                                                        {
                                                            _CompareInvNew = _inventory[j];
                                                            if (!_CompareInvNew.IsEmpty() && _invStackNew.Equals(_CompareInvNew) && i != j && !InvSlot.Contains(j))
                                                            {
                                                                InvSlot.Add(i);
                                                                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                                                PersistentPlayerData _persistentPlayerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_player.entityId);
                                                                EnumLandClaimOwner _owner = GameManager.Instance.World.GetLandClaimOwner(new Vector3i(_player.position.x, _player.position.y, _player.position.z), _persistentPlayerData);
                                                                if (_owner == EnumLandClaimOwner.Self || _owner == EnumLandClaimOwner.Ally)
                                                                {
                                                                    using (StreamWriter sw = new StreamWriter(DupeFilepath, true, Encoding.UTF8))
                                                                    {
                                                                        sw.WriteLine(string.Format("{0}: {1} {2} has added {3} {4} to their inventory, identical to another stack, inside their own or ally claimed space at {5} {6} {7}.", DateTime.Now, _cInfo.PlatformId.ReadablePlatformUserIdentifier, _cInfo.playerName, _invStackNew.count, _invName, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                                        sw.WriteLine();
                                                                        sw.Flush();
                                                                        sw.Close();
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    using (StreamWriter sw = new StreamWriter(DupeFilepath, true, Encoding.UTF8))
                                                                    {
                                                                        sw.WriteLine(string.Format("{0}: {1} {2} has added {3} {4} to their inventory, identical to another stack, standing at {5} {6} {7}.", DateTime.Now, _cInfo.PlatformId.ReadablePlatformUserIdentifier, _cInfo.playerName, _invStackNew.count, _invName, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                                        sw.WriteLine();
                                                                        sw.Flush();
                                                                        sw.Close();
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        for (int j = 0; j < _bagSize; j++)
                                                        {
                                                            _CompareBagNew = _bag[j];
                                                            if (!_CompareBagNew.IsEmpty() && _invStackNew.Equals(_CompareBagNew) && !InvSlot.Contains(j))
                                                            {
                                                                InvSlot.Add(i);
                                                                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                                                PersistentPlayerData _persistentPlayerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_player.entityId);
                                                                EnumLandClaimOwner _owner = GameManager.Instance.World.GetLandClaimOwner(new Vector3i(_player.position.x, _player.position.y, _player.position.z), _persistentPlayerData);
                                                                if (_owner == EnumLandClaimOwner.Self || _owner == EnumLandClaimOwner.Ally)
                                                                {
                                                                    using (StreamWriter sw = new StreamWriter(DupeFilepath, true, Encoding.UTF8))
                                                                    {
                                                                        sw.WriteLine(string.Format("{0}: {1} {2} has added {3} {4} to their inventory, identical to another stack, inside their own or ally claimed space at {5} {6} {7}.", DateTime.Now, _cInfo.PlatformId.ReadablePlatformUserIdentifier, _cInfo.playerName, _invStackNew.count, _invName, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                                        sw.WriteLine();
                                                                        sw.Flush();
                                                                        sw.Close();
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    using (StreamWriter sw = new StreamWriter(DupeFilepath, true, Encoding.UTF8))
                                                                    {
                                                                        sw.WriteLine(string.Format("{0}: {1} {2} has added {3} {4} to their inventory, identical to another stack, standing at {5} {6} {7}.", DateTime.Now, _cInfo.PlatformId.ReadablePlatformUserIdentifier, _cInfo.playerName, _invStackNew.count, _invName, (int)_player.position.x, (int)_player.position.y, (int)_player.position.z));
                                                                        sw.WriteLine();
                                                                        sw.Flush();
                                                                        sw.Close();
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            Bag[_cInfo.entityId] = _bag;
                            Inventory[_cInfo.entityId] = _inventory;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in DupeLog.Exec: {0}", e.Message));
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
                    sw.WriteLine("<DuplicateItemExemption>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("    <!-- <Item Name=\"\" /> -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    if (OldNodeList != null)
                    {
                        for (int i = 0; i < OldNodeList.Count; i++)
                        {
                            if (OldNodeList[i].NodeType == XmlNodeType.Comment && !OldNodeList[i].OuterXml.Contains("<!-- <Item Name=\"\""))
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
                                if (line.HasAttributes && (line.Name == "Duplicate" || line.Name == "Item"))
                                {
                                    string name = "";
                                    if (line.HasAttribute("Name"))
                                    {
                                        name = line.GetAttribute("Name");
                                    }
                                    sw.WriteLine(string.Format("    <Item Name=\"{0}\" />", name));
                                }
                            }
                        }
                    }
                    sw.WriteLine("</DuplicateItemExemption>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in DupeLog.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}
