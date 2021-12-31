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

        private static void LoadXml()
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
                            XmlElement _line = (XmlElement)childNodes[i];
                            if (_line.HasAttributes)
                            {
                                if (_line.HasAttribute("Version") && _line.GetAttribute("Version") == Config.Version)
                                {
                                    upgrade = false;
                                    continue;
                                }
                                else if (_line.HasAttribute("Name"))
                                {
                                    string item = _line.GetAttribute("Name");
                                    ItemClass _class = ItemClass.GetItemClass(item, true);
                                    if (_class == null)
                                    {
                                        Log.Out(string.Format("[SERVERTOOLS] Invalid InvalidItems.xml entry. Item or block not found: {0}", item));
                                        continue;
                                    }
                                    if (!Dict.Contains(item))
                                    {
                                        Dict.Add(item);
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
                            Log.Out(string.Format("[SERVERTOOLS] The existing InvalidItems.xml was too old or misconfigured. File deleted and rebuilt for version {0}", Config.Version));
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
                    Log.Out(string.Format("[SERVERTOOLS] Error in InvalidItems.LoadXml: {0}", e.Message));
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
                    sw.WriteLine("<InvalidItems>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("    <!-- <Item Name=\"air\" /> -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    if (Dict.Count > 0)
                    {
                        foreach (string _item in Dict)
                        {
                            sw.WriteLine(string.Format("    <Item Name=\"{0}\" />", _item));
                        }
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
            if (!File.Exists(FilePath))
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
                    if (GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo.PlatformId) > Admin_Level ||
                        GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo.CrossplatformId) > Admin_Level)
                    {
                        for (int i = 0; i < _playerDataFile.inventory.Length; i++)
                        {
                            ItemStack itemStack = _playerDataFile.inventory[i];
                            ItemValue itemValue = itemStack.itemValue;
                            int count = _playerDataFile.inventory[i].count;
                            if (count > 0 && itemValue != null && !itemValue.Equals(ItemValue.None))
                            {
                                string name = ItemClass.list[itemValue.type].Name;
                                if (Invalid_Stack)
                                {
                                    int maxAllowed = ItemClass.list[itemValue.type].Stacknumber.Value;
                                    if (count > maxAllowed)
                                    {
                                        MaxStack(_cInfo, name, count, maxAllowed);
                                    }
                                }
                                if (IsEnabled && Dict.Contains(name))
                                {
                                    if (Ban_Player)
                                    {
                                        Ban(_cInfo, name);
                                    }
                                    else
                                    {
                                        if (Flags.ContainsKey(_cInfo.entityId))
                                        {
                                            if (Flags.TryGetValue(_cInfo.entityId, out int value))
                                            {
                                                if (value == 2)
                                                {
                                                    Flag3(_cInfo, name);
                                                }
                                                else
                                                {
                                                    Flag2(_cInfo, name);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Flag1(_cInfo, name);
                                        }
                                    }
                                    return;
                                }
                            }
                        }
                        for (int i = 0; i < _playerDataFile.bag.Length; i++)
                        {
                            ItemStack itemStack = _playerDataFile.bag[i];
                            ItemValue itemValue = itemStack.itemValue;
                            int count = _playerDataFile.bag[i].count;
                            if (count > 0 && itemValue != null && !itemValue.Equals(ItemValue.None))
                            {
                                string name = ItemClass.list[itemValue.type].Name;
                                if (Invalid_Stack)
                                {
                                    int maxAllowed = ItemClass.list[itemValue.type].Stacknumber.Value;
                                    if (count > maxAllowed)
                                    {
                                        MaxStack(_cInfo, name, count, maxAllowed);
                                    }
                                }
                                if (IsEnabled && Dict.Contains(name))
                                {
                                    if (Ban_Player)
                                    {
                                        Ban(_cInfo, name);
                                    }
                                    else
                                    {
                                        if (Flags.ContainsKey(_cInfo.entityId))
                                        {
                                            if (Flags.TryGetValue(_cInfo.entityId, out int value))
                                            {
                                                if (value == 2)
                                                {
                                                    Flag3(_cInfo, name);
                                                }
                                                else
                                                {
                                                    Flag2(_cInfo, name);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Flag1(_cInfo, name);
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
                                ItemValue itemValue = _playerDataFile.equipment.GetSlotItem(i);
                                if (itemValue != null && !itemValue.Equals(ItemValue.None))
                                {
                                    string name = ItemClass.list[itemValue.type].Name;
                                    if (Dict.Contains(name))
                                    {
                                        if (Ban_Player)
                                        {
                                            Ban(_cInfo, name);
                                        }
                                        else
                                        {
                                            if (Flags.ContainsKey(_cInfo.entityId))
                                            {
                                                if (Flags.TryGetValue(_cInfo.entityId, out int value))
                                                {
                                                    if (value == 2)
                                                    {
                                                        Flag3(_cInfo, name);
                                                    }
                                                    else
                                                    {
                                                        Flag2(_cInfo, name);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                Flag1(_cInfo, name);
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
                    sw.WriteLine(string.Format("Detected id '{0}' '{1}' named '{2}' with an invalid stack of '{3}' '{4}'. Player has been warned", _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, _cInfo.PlatformId.CombinedString, _count, _name));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
                Phrases.Dict.TryGetValue("InvalidItem1", out string phrase1);
                phrase1 = phrase1.Replace("{ItemName}", _name);
                phrase1 = phrase1.Replace("{ItemCount}", _count.ToString());
                phrase1 = phrase1.Replace("{MaxPerStack}", _maxAllowed.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                Phrases.Dict.TryGetValue("InvalidItem4", out string phrase);
                phrase = phrase.Replace("{ItemName}", _name);
                SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.entityId, phrase), null);
                Phrases.Dict.TryGetValue("InvalidItem2", out phrase);
                phrase = phrase.Replace("{PlayerName}", _cInfo.playerName);
                phrase = phrase.Replace("{ItemName}", _name);
                ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
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
                    sw.WriteLine(string.Format("Detected id '{0}' '{1}' named '{2}' with invalid item '{3}'. Warning has been given to drop it", _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, _name));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
                Phrases.Dict.TryGetValue("InvalidItem5", out string phrase);
                phrase = phrase.Replace("{PlayerName}", _cInfo.playerName);
                phrase = phrase.Replace("{ItemName}", _name);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                    sw.WriteLine(string.Format("Detected id '{0}' '{1}' named '{2}' with invalid item '{3}'. Final warning was given to drop it", _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, _name));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
                Phrases.Dict.TryGetValue("InvalidItem6", out string phrase);
                phrase = phrase.Replace("{PlayerName}", _cInfo.playerName);
                phrase = phrase.Replace("{ItemName}", _name);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                Phrases.Dict.TryGetValue("InvalidItem4", out string phrase);
                SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.entityId, phrase), null);
                using (StreamWriter sw = new StreamWriter(DetectionFilepath, true, Encoding.UTF8))
                {
                    sw.WriteLine(string.Format("Detected id '{0}' '{1}' named '{2}' with invalid item '{3}'. The player has been kicked", _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, _name));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
                Flags.Remove(_cInfo.entityId);
                Phrases.Dict.TryGetValue("InvalidItem3", out phrase);
                phrase = phrase.Replace("{PlayerName}", _cInfo.playerName);
                phrase = phrase.Replace("{ItemName}", _name);
                ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
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
                LinkedList<Chunk> chunkArray = null;
                DictionaryList<Vector3i, TileEntity> tiles = new DictionaryList<Vector3i, TileEntity>();
                ChunkClusterList chunklist = GameManager.Instance.World.ChunkClusters;
                for (int i = 0; i < chunklist.Count; i++)
                {
                    ChunkCluster chunk = chunklist[i];
                    chunkArray = chunk.GetChunkArray();
                    if (chunkArray != null)
                    {
                        foreach (Chunk c in chunkArray)
                        {
                            tiles = c.GetTileEntities();
                            if (tiles != null)
                            {
                                foreach (TileEntity tile in tiles.dict.Values)
                                {
                                    if (tile.GetTileEntityType().ToString().Equals("SecureLoot"))
                                    {
                                        TileEntitySecureLootContainer SecureLoot = (TileEntitySecureLootContainer)tile;
                                        if (GameManager.Instance.adminTools.GetUserPermissionLevel(SecureLoot.GetOwner()) > Admin_Level)
                                        {
                                            ItemStack[] items = SecureLoot.items;
                                            int slotNumber = 0;
                                            foreach (ItemStack item in items)
                                            {
                                                if (!item.IsEmpty())
                                                {
                                                    string itemName = ItemClass.list[item.itemValue.type].Name;
                                                    if (Dict.Contains(itemName))
                                                    {
                                                        ItemStack itemStack = new ItemStack();
                                                        SecureLoot.UpdateSlot(slotNumber, itemStack.Clone());
                                                        tile.SetModified();
                                                        Vector3i _chestPos = SecureLoot.localChunkPos;
                                                        using (StreamWriter sw = new StreamWriter(DetectionFilepath, true, Encoding.UTF8))
                                                        {
                                                            sw.WriteLine("[SERVERTOOLS] Removed '{0}' '{1}' from a secure loot located at '{2}' owned by '{3}'", item.count, itemName, _chestPos, SecureLoot.GetOwner().CombinedString);
                                                        }
                                                        Log.Out(string.Format("[SERVERTOOLS] Removed '{0}' '{1}' from a secure loot located at '{2}' owned by '{3}'", item.count, itemName, _chestPos, SecureLoot.GetOwner().CombinedString));
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
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in InvalidItems.CheckStorage: {0}", e.Message));
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
                    sw.WriteLine("<InvalidItems>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("    <!-- <Item Name=\"air\" /> -->");
                    for (int i = 0; i < OldNodeList.Count; i++)
                    {
                        if (OldNodeList[i].NodeType == XmlNodeType.Comment && !OldNodeList[i].OuterXml.Contains("<!-- <Item Name=\"air\""))
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
                            if (line.HasAttributes && line.Name == "Item")
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