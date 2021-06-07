using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    public class InvalidItems
    {
        public static bool IsEnabled = false, IsRunning = false, Invalid_Stack = false, Ban_Player = false, Check_Storage = false;
        public static int Admin_Level = 0, Days_Before_Log_Delete = 5;
        private static readonly string file = "InvalidItems.xml";
        private static readonly string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static readonly List<string> dict = new List<string>();
        private static readonly Dictionary<int, int> Flags = new Dictionary<int, int>();
        private static readonly FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        private static readonly string _file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static readonly string _filepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, _file);

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
                        if (!_line.HasAttribute("Name"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Item entry because of missing 'Name' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string _item = _line.GetAttribute("Name");
                        ItemClass _class;
                        Block _block;
                        if (int.TryParse(_item, out int _id))
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
            using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<InvalidItems>");
                sw.WriteLine("    <Items>");
                if (dict.Count > 0)
                {
                    foreach (string _item in dict)
                    {
                        sw.WriteLine(string.Format("        <Item Name=\"{0}\" />", _item));
                    }
                }
                else
                {
                    sw.WriteLine(string.Format("        <Item Name=\"air\" />"));
                    sw.WriteLine(string.Format("        <Item Name=\"terrOrePotassiumNitrate\" />"));
                    sw.WriteLine(string.Format("        <Item Name=\"terrOreIron\" />"));
                    sw.WriteLine(string.Format("        <Item Name=\"terrOreLead\" />"));
                    sw.WriteLine(string.Format("        <Item Name=\"terrBedrock\" />"));
                    sw.WriteLine(string.Format("        <Item Name=\"terrDesertGround\" />"));
                    sw.WriteLine(string.Format("        <Item Name=\"terrIce\" />"));
                    sw.WriteLine(string.Format("        <Item Name=\"terrFertileDirt\" />"));
                    sw.WriteLine(string.Format("        <Item Name=\"terrFertileGrass\" />"));
                    sw.WriteLine(string.Format("        <Item Name=\"terrOreSilver\" />"));
                    sw.WriteLine(string.Format("        <Item Name=\"terrOreCoal\" />"));
                    sw.WriteLine(string.Format("        <Item Name=\"terrainFiller\" />"));
                    sw.WriteLine(string.Format("        <Item Name=\"terrOreGold\" />"));
                    sw.WriteLine(string.Format("        <Item Name=\"terrDestroyedWoodDebris\" />"));
                    sw.WriteLine(string.Format("        <Item Name=\"terrOreOilDeposit\" />"));
                    sw.WriteLine(string.Format("        <Item Name=\"terrOreDiamond\" />"));
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
                    if (GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo) > Admin_Level)
                    {
                        for (int i = 0; i < _playerDataFile.inventory.Length; i++)
                        {
                            ItemStack _itemStack = _playerDataFile.inventory[i];
                            ItemValue _itemValue = _itemStack.itemValue;
                            int _count = _playerDataFile.inventory[i].count;
                            if (_count > 0 && _itemValue != null && !_itemValue.Equals(ItemValue.None))
                            {
                                string _name = ItemClass.list[_itemValue.type].Name;
                                if (Invalid_Stack)
                                {
                                    int _maxAllowed = ItemClass.list[_itemValue.type].Stacknumber.Value;
                                    if (_count > _maxAllowed)
                                    {
                                        MaxStack(_cInfo, _name, _count, _maxAllowed);
                                    }
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
                                            if (Flags.TryGetValue(_cInfo.entityId, out int _value))
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
                                string _name = ItemClass.list[_itemValue.type].Name;
                                if (Invalid_Stack)
                                {
                                    int _maxAllowed = ItemClass.list[_itemValue.type].Stacknumber.Value;
                                    if (_count > _maxAllowed)
                                    {
                                        MaxStack(_cInfo, _name, _count, _maxAllowed);
                                    }
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
                                            if (Flags.TryGetValue(_cInfo.entityId, out int _value))
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
                        if (IsEnabled)
                        {
                            for (int i = 0; i < _playerDataFile.equipment.GetSlotCount(); i++)
                            {
                                ItemValue _itemValue = _playerDataFile.equipment.GetSlotItem(i);
                                if (_itemValue != null && !_itemValue.Equals(ItemValue.None))
                                {
                                    string _name = ItemClass.list[_itemValue.type].Name;
                                    if (dict.Contains(_name))
                                    {
                                        if (Ban_Player)
                                        {
                                            Ban(_cInfo, _name);
                                        }
                                        else
                                        {
                                            if (Flags.ContainsKey(_cInfo.entityId))
                                            {
                                                if (Flags.TryGetValue(_cInfo.entityId, out int _value))
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
                Log.Out(string.Format("[SERVERTOOLS] Error in InventoryCheck.CheckInv: {0}", e.Message));
            }
        }

        private static void MaxStack(ClientInfo _cInfo, string _name, int _count, int _maxAllowed)
        {
            using (StreamWriter sw = new StreamWriter(_filepath, true, Encoding.UTF8))
            {
                sw.WriteLine(string.Format("Detected \"{0}\", Steam Id {1}, with invalid stack: {2} {3}. Warned the player.", _cInfo.playerName, _cInfo.playerId, _name, _count));
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
            Phrases.Dict.TryGetValue(11, out string _phrase11);
            _phrase11 = _phrase11.Replace("{ItemName}", _name);
            _phrase11 = _phrase11.Replace("{ItemCount}", _count.ToString());
            _phrase11 = _phrase11.Replace("{MaxPerStack}", _maxAllowed.ToString());
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase11 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
        }

        private static void Ban(ClientInfo _cInfo, string _name)
        {
            Phrases.Dict.TryGetValue(14, out string _phrase14);
            SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.entityId, _phrase14), null);
            Phrases.Dict.TryGetValue(12, out string _phrase12);
            _phrase12 = _phrase12.Replace("{PlayerName}", _cInfo.playerName);
            _phrase12 = _phrase12.Replace("{ItemName}", _name);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase12 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
        }

        private static void Flag1(ClientInfo _cInfo, string _name)
        {
            Flags.Add(_cInfo.entityId, 1);
            using (StreamWriter sw = new StreamWriter(_filepath, true, Encoding.UTF8))
            {
                sw.WriteLine(string.Format("Detected \"{0}\", Steam id {1}, with invalid item: {2}. Warning was given to drop it.", _cInfo.playerName, _cInfo.playerId, _name));
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
            Phrases.Dict.TryGetValue(15, out string _phrase15);
            _phrase15 = _phrase15.Replace("{PlayerName}", _cInfo.playerName);
            _phrase15 = _phrase15.Replace("{ItemName}", _name);
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase15 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
        }

        private static void Flag2(ClientInfo _cInfo, string _name)
        {
            Flags[_cInfo.entityId] = 2;
            using (StreamWriter sw = new StreamWriter(_filepath, true, Encoding.UTF8))
            {
                sw.WriteLine(string.Format("Detected \"{0}\", Steam id {1}, with invalid item: {2}. Final warning was given to drop it.", _cInfo.playerName, _cInfo.playerId, _name));
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
            Phrases.Dict.TryGetValue(16, out string _phrase16);
            _phrase16 = _phrase16.Replace("{PlayerName}", _cInfo.playerName);
            _phrase16 = _phrase16.Replace("{ItemName}", _name);
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase16 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
        }

        private static void Flag3(ClientInfo _cInfo, string _name)
        {
            Phrases.Dict.TryGetValue(14, out string _phrase14);
            SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.entityId, _phrase14), null);
            using (StreamWriter sw = new StreamWriter(_filepath, true, Encoding.UTF8))
            {
                sw.WriteLine(string.Format("Detected \"{0}\", Steam id {1}, with invalid item: {2}. Kicked the player.", _cInfo.playerName, _cInfo.playerId, _name));
                sw.WriteLine();
                sw.Flush();
                sw.Close();
            }
            Flags.Remove(_cInfo.entityId);
            Phrases.Dict.TryGetValue(13, out string _phrase13);
            _phrase13 = _phrase13.Replace("{PlayerName}", _cInfo.playerName);
            _phrase13 = _phrase13.Replace("{ItemName}", _name);
            ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase13 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
        }

        public static void CheckStorage()
        {
            try
            {
                LinkedList<Chunk> _chunkArray = new LinkedList<Chunk>();
                DictionaryList<Vector3i, TileEntity> _tiles = new DictionaryList<Vector3i, TileEntity>();
                ChunkClusterList _chunklist = GameManager.Instance.World.ChunkClusters;
                for (int i = 0; i < _chunklist.Count; i++)
                {
                    ChunkCluster _chunk = _chunklist[i];
                    _chunkArray = _chunk.GetChunkArray();
                    foreach (Chunk _c in _chunkArray)
                    {
                        _tiles = _c.GetTileEntities();
                        foreach (TileEntity _tile in _tiles.dict.Values)
                        {
                            if (_tile.GetTileEntityType().ToString().Equals("SecureLoot"))
                            {
                                TileEntitySecureLootContainer SecureLoot = (TileEntitySecureLootContainer)_tile;
                                if (GameManager.Instance.adminTools.GetUserPermissionLevel(SecureLoot.GetOwner()) > Admin_Level)
                                {
                                    ItemStack[] _items = SecureLoot.items;
                                    int slotNumber = 0;
                                    foreach (ItemStack _item in _items)
                                    {
                                        if (!_item.IsEmpty())
                                        {
                                            string _itemName = ItemClass.list[_item.itemValue.type].Name;
                                            if (dict.Contains(_itemName))
                                            {
                                                ItemStack itemStack = new ItemStack();
                                                SecureLoot.UpdateSlot(slotNumber, itemStack.Clone());
                                                _tile.SetModified();
                                                Vector3i _chestPos = SecureLoot.localChunkPos;
                                                using (StreamWriter sw = new StreamWriter(_filepath, true, Encoding.UTF8))
                                                {
                                                    sw.WriteLine("[SERVERTOOLS] Removed {0} {1}, from a secure loot located at {2} {3} {4}, owned by {5}", _item.count, _itemName, _chestPos.x, _chestPos.y, _chestPos.z, SecureLoot.GetOwner());
                                                    sw.WriteLine();
                                                    sw.Flush();
                                                    sw.Close();
                                                }
                                                Log.Out(string.Format("[SERVERTOOLS] Removed {0} {1}, from a secure loot located at {2} {3} {4}, owned by {5}", _item.count, _itemName, _chestPos.x, _chestPos.y, _chestPos.z, SecureLoot.GetOwner()));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in InventoryCheck.ChestCheck: {0}", e.Message));
            }
        }
    }
}