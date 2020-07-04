using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools.AntiCheat
{
    public class InvalidItems
    {
        public static bool IsEnabled = false, IsRunning = false, Announce_Invalid_Stack = false, Ban_Player = false, Check_Storage = false;
        public static int Admin_Level = 0, Days_Before_Log_Delete = 5;
        private static string file = "InvalidItems.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static List<string> dict = new List<string>();
        private static Dictionary<int, int> Flags = new Dictionary<int, int>();
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        private static string _file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string _filepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, _file);

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
            dict.Clear();
            fileWatcher.Dispose();
            IsRunning = false;
        }

        private static void LoadXml()
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
                if (childNode.Name == "Items")
                {
                    dict.Clear();
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Items' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("itemName"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Item entry because of missing 'itemName' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string _item = _line.GetAttribute("itemName");
                        ItemClass _class;
                        Block _block;
                        int _id;
                        if (int.TryParse(_item, out _id))
                        {
                            _class = ItemClass.GetForId(_id);
                            _block = Block.GetBlockByName(_item, true);
                        }
                        else
                        {
                            _class = ItemClass.GetItemClass(_item, true);
                            _block = Block.GetBlockByName(_item, true);
                        }
                        if (_class == null && _block == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Invalid item entry skipped. Item or block not found: {0}", _item));
                            continue;
                        }
                        if (!dict.Contains(_item))
                        {
                            dict.Add(_item);
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
                sw.WriteLine("<InvalidItems>");
                sw.WriteLine("    <Items>");
                if (dict.Count > 0)
                {
                    foreach (string _item in dict)
                    {
                        sw.WriteLine(string.Format("        <item itemName=\"{0}\" />", _item));
                    }
                }
                else
                {
                    sw.WriteLine(string.Format("        <item itemName=\"air\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrOrePotassiumNitrate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrOreIron\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrOreLead\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrBedrock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrDesertGround\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrIce\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrFertileDirt\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrFertileGrass\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrOreSilver\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrOreCoal\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrainFiller\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrOreGold\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrDestroyedWoodDebris\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrOreOilDeposit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrOreDiamond\" />"));
                }
                sw.WriteLine("    </Items>");
                sw.WriteLine("</InvalidItems>");
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

        public static void CheckInv(ClientInfo _cInfo, PlayerDataFile _playerDataFile)
        {
            try
            {
                if (_cInfo != null)
                {
                    GameManager.Instance.adminTools.GetAdmins().TryGetValue(_cInfo.playerId, out AdminToolsClientInfo Admin);
                    if (Admin.PermissionLevel > Admin_Level)
                    {
                        for (int i = 0; i < _playerDataFile.inventory.Length; i++)
                        {
                            ItemStack _itemStack = _playerDataFile.inventory[i];
                            ItemValue _itemValue = _itemStack.itemValue;
                            int _count = _playerDataFile.inventory[i].count;
                            if (_count > 0 && _itemValue != null && !_itemValue.Equals(ItemValue.None))
                            {
                                int _maxAllowed = ItemClass.list[_itemValue.type].Stacknumber.Value;
                                string _name = ItemClass.list[_itemValue.type].Name;
                                if (_count > _maxAllowed)
                                {
                                    MaxStack(_cInfo, _name, _count, _maxAllowed);
                                }
                                if (IsEnabled && dict.Contains(_name))
                                {
                                    if (Ban_Player)
                                    {
                                        Ban(_cInfo, _name);
                                    }
                                    else
                                    {
                                        if (Flags.ContainsKey(_cInfo.entityId))
                                        {
                                            int _value;
                                            if (Flags.TryGetValue(_cInfo.entityId, out _value))
                                            {
                                                if (_value == 2)
                                                {
                                                    Flag3(_cInfo, _name);
                                                }
                                                else
                                                {
                                                    Flag2(_cInfo, _name);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Flag1(_cInfo, _name);
                                        }
                                    }
                                    return;
                                }
                            }
                        }
                        for (int i = 0; i < _playerDataFile.bag.Length; i++)
                        {
                            ItemStack _itemStack = _playerDataFile.bag[i];
                            ItemValue _itemValue = _itemStack.itemValue;
                            int _count = _playerDataFile.bag[i].count;
                            if (_count > 0 && _itemValue != null && !_itemValue.Equals(ItemValue.None))
                            {
                                int _maxAllowed = ItemClass.list[_itemValue.type].Stacknumber.Value;
                                string _name = ItemClass.list[_itemValue.type].Name;
                                if (_count > _maxAllowed)
                                {
                                    MaxStack(_cInfo, _name, _count, _maxAllowed);
                                }
                                if (IsEnabled && dict.Contains(_name))
                                {
                                    if (Ban_Player)
                                    {
                                        Ban(_cInfo, _name);
                                    }
                                    else
                                    {
                                        if (Flags.ContainsKey(_cInfo.entityId))
                                        {
                                            int _value;
                                            if (Flags.TryGetValue(_cInfo.entityId, out _value))
                                            {
                                                if (_value == 2)
                                                {
                                                    Flag3(_cInfo, _name);
                                                }
                                                else
                                                {
                                                    Flag2(_cInfo, _name);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Flag1(_cInfo, _name);
                                        }
                                    }
                                    return;
                                }
                            }
                        }
                        for (int i = 0; i < _playerDataFile.equipment.GetSlotCount(); i++)
                        {
                            ItemValue _item = _playerDataFile.equipment.GetSlotItem(i);
                            if (_item != null && !_item.Equals(ItemValue.None))
                            {
                                int _maxAllowed = ItemClass.list[_item.type].Stacknumber.Value;
                                string _name = ItemClass.list[_item.type].Name;
                                if (IsEnabled && dict.Contains(_name))
                                {
                                    if (Ban_Player)
                                    {
                                        Ban(_cInfo, _name);
                                    }
                                    else
                                    {
                                        if (Flags.ContainsKey(_cInfo.entityId))
                                        {
                                            int _value;
                                            if (Flags.TryGetValue(_cInfo.entityId, out _value))
                                            {
                                                if (_value == 2)
                                                {
                                                    Flag3(_cInfo, _name);
                                                }
                                                else
                                                {
                                                    Flag2(_cInfo, _name);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Flag1(_cInfo, _name);
                                        }
                                    }
                                    return;
                                }
                            }
                        }
                        if (Flags.ContainsKey(_cInfo.entityId))
                        {
                            Flags.Remove(_cInfo.entityId);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in InventoryCheck.CheckInv: {0}.", e.Message));
            }
        }

        private static void MaxStack(ClientInfo _cInfo, string _name, int _count, int _maxAllowed)
        {
            string _phrase3;
            if (!Phrases.Dict.TryGetValue(3, out _phrase3))
            {
                _phrase3 = " you have a invalid item stack: {ItemName} {ItemCount}. Max per stack: {MaxPerStack}.";
            }
            _phrase3 = _phrase3.Replace("{ItemName}", _name);
            _phrase3 = _phrase3.Replace("{ItemCount}", _count.ToString());
            _phrase3 = _phrase3.Replace("{MaxPerStack}", _maxAllowed.ToString());
            ChatHook.ChatMessage(_cInfo, "[FF0000]" + _cInfo.playerName + _phrase3 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            using (StreamWriter sw = new StreamWriter(_filepath, true))
            {
                sw.WriteLine(string.Format("Detected {0}, Steam Id {1}, with invalid stack: {2} {3}. Warned the player.", _cInfo.playerName, _cInfo.playerId, _name, _count));
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
        }

        private static void Ban(ClientInfo _cInfo, string _name)
        {
            SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Invalid Item {1}\"", _cInfo.entityId, _name), (ClientInfo)null);
            string _phrase4;
            if (!Phrases.Dict.TryGetValue(4, out _phrase4))
            {
                _phrase4 = "Cheat Detected: Auto banned {PlayerName} for having a invalid item: {ItemName}.";
            }
            _phrase4 = _phrase4.Replace("{PlayerName}", _cInfo.playerName);
            _phrase4 = _phrase4.Replace("{ItemName}", _name);
            ChatHook.ChatMessage(_cInfo, "[FF0000]" + _phrase4 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
        }

        private static void Flag1(ClientInfo _cInfo, string _name)
        {
            Flags.Add(_cInfo.entityId, 1);
            string _phrase800;
            if (!Phrases.Dict.TryGetValue(800, out _phrase800))
            {
                _phrase800 = "Cheat Detected: {PlayerName} you are holding a invalid item: {ItemName}. You have 30 seconds to drop it.";
            }
            _phrase800 = _phrase800.Replace("{PlayerName}", _cInfo.playerName);
            _phrase800 = _phrase800.Replace("{ItemName}", _name);
            ChatHook.ChatMessage(_cInfo, "[FF0000]" + _phrase800 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
            using (StreamWriter sw = new StreamWriter(_filepath, true))
            {
                sw.WriteLine(string.Format("Detected {0}, Steam Id {1}, with invalid item: {2}. Warning was given to drop it.", _cInfo.playerName, _cInfo.playerId, _name));
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
        }

        private static void Flag2(ClientInfo _cInfo, string _name)
        {
            Flags[_cInfo.entityId] = 2;
            string _phrase799;
            if (!Phrases.Dict.TryGetValue(799, out _phrase799))
            {
                _phrase799 = "Cheat Detected: {PlayerName} you are holding a invalid item: {ItemName}. Final warning, drop it!";
            }
            _phrase799 = _phrase799.Replace("{PlayerName}", _cInfo.playerName);
            _phrase799 = _phrase799.Replace("{ItemName}", _name);
            ChatHook.ChatMessage(_cInfo, "[FF0000]" + _phrase799 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
            using (StreamWriter sw = new StreamWriter(_filepath, true))
            {
                sw.WriteLine(string.Format("Detected {0}, Steam Id {1}, with invalid item: {2}. Warning was given to drop it.", _cInfo.playerName, _cInfo.playerId, _name));
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
        }

        private static void Flag3(ClientInfo _cInfo, string _name)
        {
            string _phrase5;
            if (!Phrases.Dict.TryGetValue(5, out _phrase5))
            {
                _phrase5 = "Cheat Detected: Auto kicked {PlayerName} for having a invalid item: {ItemName}.";
            }
            _phrase5 = _phrase5.Replace("{PlayerName}", _cInfo.playerName);
            _phrase5 = _phrase5.Replace("{ItemName}", _name);
            ChatHook.ChatMessage(_cInfo, "[FF0000]" + _phrase5 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
            SdtdConsole.Instance.ExecuteSync(string.Format("ban {0} \"Invalid Item: {1}\"", _cInfo.entityId, _name), (ClientInfo)null);
            using (StreamWriter sw = new StreamWriter(_filepath, true))
            {
                sw.WriteLine(string.Format("Detected {0}, Steam Id {1}, with invalid item: {2}. Kicked the player.", _cInfo.playerName, _cInfo.playerId, _name));
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
            Flags.Remove(_cInfo.entityId);
        }

        public static void CheckStorage()
        {
            try
            {
                LinkedList<Chunk> chunkArray = new LinkedList<Chunk>();
                DictionaryList<Vector3i, TileEntity> tiles = new DictionaryList<Vector3i, TileEntity>();
                ChunkClusterList chunklist = GameManager.Instance.World.ChunkClusters;
                for (int i = 0; i < chunklist.Count; i++)
                {
                    ChunkCluster chunk = chunklist[i];
                    chunkArray = chunk.GetChunkArray();
                    foreach (Chunk _c in chunkArray)
                    {
                        tiles = _c.GetTileEntities();
                        foreach (TileEntity tile in tiles.dict.Values)
                        {
                            if (tile.GetTileEntityType().ToString().Equals("SecureLoot"))
                            {
                                TileEntitySecureLootContainer SecureLoot = (TileEntitySecureLootContainer)tile;
                                GameManager.Instance.adminTools.GetAdmins().TryGetValue(SecureLoot.GetOwner(), out AdminToolsClientInfo Admin);
                                if (Admin.PermissionLevel > Admin_Level)
                                {
                                    ItemStack[] items = SecureLoot.items;
                                    int slotNumber = 0;
                                    foreach (ItemStack item in items)
                                    {
                                        if (!item.IsEmpty())
                                        {
                                            string _itemName = ItemClass.list[item.itemValue.type].Name;
                                            if (dict.Contains(_itemName))
                                            {
                                                int _count = item.count;
                                                ItemStack itemStack = new ItemStack();
                                                SecureLoot.UpdateSlot(slotNumber, itemStack.Clone());
                                                Vector3i _chestPos = SecureLoot.localChunkPos;
                                                using (StreamWriter sw = new StreamWriter(_filepath, true))
                                                {
                                                    sw.WriteLine("[SERVERTOOLS] Removed {0} {1}, from a secure loot located at {2} {3} {4}, owned by {5}", item.count, _itemName, _chestPos.x, _chestPos.y, _chestPos.z, SecureLoot.GetOwner());
                                                    sw.WriteLine();
                                                    sw.Flush();
                                                    sw.Close();
                                                }
                                                Log.Out(string.Format("[SERVERTOOLS] Removed {0} {1}, from a secure loot located at {2} {3} {4}, owned by {5}", item.count, _itemName, _chestPos.x, _chestPos.y, _chestPos.z, SecureLoot.GetOwner()));
                                            }
                                        }
                                        slotNumber++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in InventoryCheck.ChestCheck: {0}.", e.Message));
            }
        }
    }
}