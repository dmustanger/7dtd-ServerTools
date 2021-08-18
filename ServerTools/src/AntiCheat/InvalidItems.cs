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

        private static readonly List<string> Dict = new List<string>();
        private static readonly Dictionary<int, int> Flags = new Dictionary<int, int>();
        private static readonly string file = "InvalidItems.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static readonly string DetectionFile = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static readonly string DetectionFilepath = string.Format("{0}/Logs/DetectionLogs/{1}", API.ConfigPath, DetectionFile);
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
            Dict.Clear();
            FileWatcher.Dispose();
            IsRunning = false;
        }

        private static void LoadXml()
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
                    Dict.Clear();
                    bool upgrade = true;
                    for (int i = 0; i < _childNodes.Count; i++)
                    {
                        if (_childNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement _line = (XmlElement)_childNodes[i];
                        if (_line.HasAttributes)
                        {
                            if (_line.HasAttribute("Version") && _line.GetAttribute("Version") == Config.Version)
                            {
                                upgrade = false;
                                continue;
                            }
                            else if (_line.HasAttribute("Name"))
                            {
                                string _item = _line.GetAttribute("Name");
                                ItemClass _class = ItemClass.GetItemClass(_item, true);
                                if (_class == null)
                                {
                                    Log.Out(string.Format("[SERVERTOOLS] Invalid InvalidItems.xml entry. Item or block not found: {0}", _item));
                                    continue;
                                }
                                if (!Dict.Contains(_item))
                                {
                                    Dict.Add(_item);
                                }
                            }
                        }
                    }
                    if (upgrade)
                    {
                        UpgradeXml(_childNodes);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in InvalidItems.LoadXml: {0}", e.Message));
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
                    sw.WriteLine("<InvalidItems>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine();
                    sw.WriteLine();
                    if (Dict.Count > 0)
                    {
                        foreach (string _item in Dict)
                        {
                            sw.WriteLine(string.Format("    <Item Name=\"{0}\" />", _item));
                        }
                    }
                    else
                    {
                        sw.WriteLine(string.Format("    <Item Name=\"air\" />"));
                        sw.WriteLine(string.Format("    <Item Name=\"terrOrePotassiumNitrate\" />"));
                        sw.WriteLine(string.Format("    <Item Name=\"terrOreIron\" />"));
                        sw.WriteLine(string.Format("    <Item Name=\"terrOreLead\" />"));
                        sw.WriteLine(string.Format("    <Item Name=\"terrBedrock\" />"));
                        sw.WriteLine(string.Format("    <Item Name=\"terrDesertGround\" />"));
                        sw.WriteLine(string.Format("    <Item Name=\"terrIce\" />"));
                        sw.WriteLine(string.Format("    <Item Name=\"terrFertileDirt\" />"));
                        sw.WriteLine(string.Format("    <Item Name=\"terrFertileGrass\" />"));
                        sw.WriteLine(string.Format("    <Item Name=\"terrOreSilver\" />"));
                        sw.WriteLine(string.Format("    <Item Name=\"terrOreCoal\" />"));
                        sw.WriteLine(string.Format("    <Item Name=\"terrainFiller\" />"));
                        sw.WriteLine(string.Format("    <Item Name=\"terrOreGold\" />"));
                        sw.WriteLine(string.Format("    <Item Name=\"terrDestroyedWoodDebris\" />"));
                        sw.WriteLine(string.Format("    <Item Name=\"terrOreOilDeposit\" />"));
                        sw.WriteLine(string.Format("    <Item Name=\"terrOreDiamond\" />"));
                    }
                    sw.WriteLine("</InvalidItems>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in InvalidItems.UpdateXml: {0}", e.Message));
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
                                if (IsEnabled && Dict.Contains(_name))
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
                                if (IsEnabled && Dict.Contains(_name))
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
                                    if (Dict.Contains(_name))
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
                Log.Out(string.Format("[SERVERTOOLS] Error in InvalidItems.CheckInv: {0}", e.Message));
            }
        }

        private static void MaxStack(ClientInfo _cInfo, string _name, int _count, int _maxAllowed)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(DetectionFilepath, true, Encoding.UTF8))
                {
                    sw.WriteLine(string.Format("Detected \"{0}\", Steam Id {1}, with invalid stack: {2} {3}. Warned the player.", _cInfo.playerName, _cInfo.playerId, _name, _count));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
                Phrases.Dict.TryGetValue("InvalidItem1", out string _phrase1);
                _phrase1 = _phrase1.Replace("{ItemName}", _name);
                _phrase1 = _phrase1.Replace("{ItemCount}", _count.ToString());
                _phrase1 = _phrase1.Replace("{MaxPerStack}", _maxAllowed.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in InvalidItems.MaxStack: {0}", e.Message));
            }
        }

        private static void Ban(ClientInfo _cInfo, string _name)
        {
            try
            {
                Phrases.Dict.TryGetValue("InvalidItem4", out string _phrase);
                _phrase = _phrase.Replace("{ItemName}", _name);
                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.entityId, _phrase), null);
                Phrases.Dict.TryGetValue("InvalidItem2", out _phrase);
                _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
                _phrase = _phrase.Replace("{ItemName}", _name);
                ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in InvalidItems.Ban: {0}", e.Message));
            }
        }

        private static void Flag1(ClientInfo _cInfo, string _name)
        {
            try
            {
                Flags.Add(_cInfo.entityId, 1);
                using (StreamWriter sw = new StreamWriter(DetectionFilepath, true, Encoding.UTF8))
                {
                    sw.WriteLine(string.Format("Detected \"{0}\", Steam id {1}, with invalid item: {2}. Warning was given to drop it.", _cInfo.playerName, _cInfo.playerId, _name));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
                Phrases.Dict.TryGetValue("InvalidItem5", out string _phrase);
                _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
                _phrase = _phrase.Replace("{ItemName}", _name);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in InvalidItems.Flag1: {0}", e.Message));
            }
        }

        private static void Flag2(ClientInfo _cInfo, string _name)
        {
            try
            {
                Flags[_cInfo.entityId] = 2;
                using (StreamWriter sw = new StreamWriter(DetectionFilepath, true, Encoding.UTF8))
                {
                    sw.WriteLine(string.Format("Detected \"{0}\", Steam id {1}, with invalid item: {2}. Final warning was given to drop it.", _cInfo.playerName, _cInfo.playerId, _name));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
                Phrases.Dict.TryGetValue("InvalidItem6", out string _phrase);
                _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
                _phrase = _phrase.Replace("{ItemName}", _name);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in InvalidItems.Flag2: {0}", e.Message));
            }
        }

        private static void Flag3(ClientInfo _cInfo, string _name)
        {
            try
            {
                Phrases.Dict.TryGetValue("InvalidItem4", out string _phrase4);
                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.entityId, _phrase4), null);
                using (StreamWriter sw = new StreamWriter(DetectionFilepath, true, Encoding.UTF8))
                {
                    sw.WriteLine(string.Format("Detected \"{0}\", Steam id {1}, with invalid item: {2}. Kicked the player.", _cInfo.playerName, _cInfo.playerId, _name));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
                Flags.Remove(_cInfo.entityId);
                Phrases.Dict.TryGetValue("InvalidItem3", out string _phrase);
                _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
                _phrase = _phrase.Replace("{ItemName}", _name);
                ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in InvalidItems.Flag3: {0}", e.Message));
            }
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
                                            if (Dict.Contains(_itemName))
                                            {
                                                ItemStack itemStack = new ItemStack();
                                                SecureLoot.UpdateSlot(slotNumber, itemStack.Clone());
                                                _tile.SetModified();
                                                Vector3i _chestPos = SecureLoot.localChunkPos;
                                                using (StreamWriter sw = new StreamWriter(DetectionFilepath, true, Encoding.UTF8))
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
                Log.Out(string.Format("[SERVERTOOLS] Error in InvalidItems.CheckStorage: {0}", e.Message));
            }
        }

        private static void UpgradeXml(XmlNodeList _oldChildNodes)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                File.Delete(FilePath);
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<InvalidItems>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine();
                    sw.WriteLine();
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement _line = (XmlElement)_oldChildNodes[i];
                        if (_line.HasAttributes && _line.OuterXml.Contains("Item"))
                        {
                            string _name = "";
                            if (_line.HasAttribute("Name"))
                            {
                                _name = _line.GetAttribute("Name");
                            }
                            sw.WriteLine(string.Format("    <Item Name=\"{0}\" />", _name));
                        }
                    }
                    sw.WriteLine("</InvalidItems>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in InvalidItems.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}