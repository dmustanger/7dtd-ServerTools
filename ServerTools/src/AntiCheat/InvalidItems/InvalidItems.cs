using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    public class InvalidItems
    {
        public static bool IsEnabled = false, IsRunning = false, Invalid_Stack = false, Ban_Player = false;
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
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                Dict.Clear();
                if (childNodes != null && (childNodes[0] != null && childNodes[0].OuterXml.Contains("Version") && childNodes[0].OuterXml.Contains(Config.Version)))
                {
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        if (childNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement line = (XmlElement)childNodes[i];
                        if (!line.HasAttributes || line.HasAttribute("Name"))
                        {
                            continue;
                        }
                        string item = line.GetAttribute("Name");
                        if (item == "")
                        {
                            continue;
                        }
                        ItemClass itemClass = ItemClass.GetItemClass(item, true);
                        if (itemClass == null)
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
                else
                {
                    XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                    if (nodeList != null)
                    {
                        Timers.UpgradeInvalidItemsXml(nodeList);
                        //UpgradeXml(nodeList);
                        return;
                    }
                    File.Delete(FilePath);
                    UpdateXml();
                    return;
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
                    sw.WriteLine("<InvalidItems>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- <Item Name=\"air\" /> -->");
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

        public static void CheckInv()
        {
            try
            {
                List<ClientInfo> clients = GeneralOperations.ClientList();
                if (clients == null || clients.Count < 1)
                {
                    return;
                }
                for (int i = 0; i < clients.Count; i++)
                {
                    ClientInfo cInfo = clients[i];
                    if (cInfo == null || cInfo.latestPlayerData == null)
                    {
                        continue;
                    }
                    if (GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfo.PlatformId) <= Admin_Level ||
                        GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfo.CrossplatformId) <= Admin_Level)
                    {
                        continue;
                    }
                    bool found = false;
                    for (int j = 0; j < cInfo.latestPlayerData.inventory.Length; j++)
                    {
                        ItemStack itemStack = cInfo.latestPlayerData.inventory[j];
                        if (itemStack != null)
                        {
                            ItemValue itemValue = itemStack.itemValue;
                            if (itemValue == null || itemValue.IsEmpty())
                            {
                                continue;
                            }
                            int count = cInfo.latestPlayerData.inventory[j].count;
                            if (count > 0 && itemValue != null && !itemValue.IsEmpty())
                            {
                                string name = itemValue.ItemClass.Name ?? itemValue.ItemClass.GetItemName();
                                if (Invalid_Stack)
                                {
                                    int maxAllowed = itemValue.ItemClass.Stacknumber.Value;
                                    if (count > maxAllowed)
                                    {
                                        MaxStack(cInfo, name, count, maxAllowed);
                                    }
                                }
                                if (Dict.Contains(name))
                                {
                                    if (Ban_Player)
                                    {
                                        Ban(cInfo, name);
                                    }
                                    else
                                    {
                                        if (Flags.ContainsKey(cInfo.entityId))
                                        {
                                            if (Flags.TryGetValue(cInfo.entityId, out int value))
                                            {
                                                if (value == 2)
                                                {
                                                    Flag3(cInfo, name);
                                                }
                                                else
                                                {
                                                    Flag2(cInfo, name);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Flag1(cInfo, name);
                                        }
                                    }
                                    found = true;
                                }
                            }
                        }
                    }
                    for (int j = 0; j < cInfo.latestPlayerData.bag.Length; j++)
                    {
                        ItemStack itemStack = cInfo.latestPlayerData.bag[j];
                        if (itemStack != null)
                        {
                            ItemValue itemValue = itemStack.itemValue;
                            if (itemValue == null || itemValue.IsEmpty())
                            {
                                continue;
                            }
                            int count = cInfo.latestPlayerData.bag[j].count;
                            if (count > 0 && itemValue != null && !itemValue.IsEmpty())
                            {
                                string name = itemValue.ItemClass.Name ?? itemValue.ItemClass.GetItemName();
                                if (Invalid_Stack)
                                {
                                    int maxAllowed = itemValue.ItemClass.Stacknumber.Value;
                                    if (count > maxAllowed)
                                    {
                                        MaxStack(cInfo, name, count, maxAllowed);
                                    }
                                }
                                if (Dict.Contains(name))
                                {
                                    if (Ban_Player)
                                    {
                                        Ban(cInfo, name);
                                    }
                                    else
                                    {
                                        if (Flags.ContainsKey(cInfo.entityId))
                                        {
                                            if (Flags.TryGetValue(cInfo.entityId, out int value))
                                            {
                                                if (value == 2)
                                                {
                                                    Flag3(cInfo, name);
                                                }
                                                else
                                                {
                                                    Flag2(cInfo, name);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Flag1(cInfo, name);
                                        }
                                    }
                                    found = true;
                                }
                            }
                        }
                    }
                    for (int j = 0; j < cInfo.latestPlayerData.equipment.GetSlotCount(); j++)
                    {
                        ItemValue itemValue = cInfo.latestPlayerData.equipment.GetSlotItem(j);
                        if (itemValue != null && !itemValue.IsEmpty())
                        {
                            string name = itemValue.ItemClass.Name ?? itemValue.ItemClass.GetItemName();
                            if (Dict.Contains(name))
                            {
                                if (Ban_Player)
                                {
                                    Ban(cInfo, name);
                                }
                                else
                                {
                                    if (Flags.ContainsKey(cInfo.entityId))
                                    {
                                        if (Flags.TryGetValue(cInfo.entityId, out int value))
                                        {
                                            if (value == 2)
                                            {
                                                Flag3(cInfo, name);
                                            }
                                            else
                                            {
                                                Flag2(cInfo, name);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Flag1(cInfo, name);
                                    }
                                }
                                found = true;
                            }
                        }
                    }
                    if (PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].Vault != null && 
                        PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].Vault.Length > 0)
                    {
                        ItemDataSerializable[] itemData = PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].Vault;
                        for (int j = 0; j < itemData.Length; j++)
                        {
                            if (Dict.Contains(itemData[j].name))
                            {
                                string name = itemData[j].name;
                                if (Ban_Player)
                                {
                                    Ban(cInfo, name);
                                }
                                else
                                {
                                    if (Flags.ContainsKey(cInfo.entityId))
                                    {
                                        if (Flags.TryGetValue(cInfo.entityId, out int value))
                                        {
                                            if (value == 2)
                                            {
                                                Flag3(cInfo, name);
                                            }
                                            else
                                            {
                                                Flag2(cInfo, name);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Flag1(cInfo, name);
                                    }
                                }
                                found = true;
                            }
                        }
                    }
                    if (!found && Flags.ContainsKey(cInfo.entityId))
                    {
                        Flags.Remove(cInfo.entityId);
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
                    sw.WriteLine(string.Format("Detected id '{0}' '{1}' named '{2}' with an invalid stack of '{3}' '{4}'. Player has been warned", _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, _count, _name));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
                Phrases.Dict.TryGetValue("InvalidItem1", out string phrase);
                phrase = phrase.Replace("{ItemName}", _name);
                phrase = phrase.Replace("{ItemCount}", _count.ToString());
                phrase = phrase.Replace("{MaxPerStack}", _maxAllowed.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"{1}\"", _cInfo.entityId, phrase), null);
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
                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.CrossplatformId.CombinedString, phrase), null);
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
                ChunkCluster cluster;
                DictionaryList<Vector3i, TileEntity> tileEntities = new DictionaryList<Vector3i, TileEntity>();
                ChunkClusterList chunklist = GameManager.Instance.World.ChunkClusters;
                if (chunklist == null || chunklist.Count < 1)
                {
                    return;
                }
                for (int i = 0; i < chunklist.Count; i++)
                {
                    cluster = chunklist[i];
                    if (cluster == null)
                    {
                        continue;
                    }
                    chunkArray = cluster.GetChunkArray();
                    if (chunkArray == null)
                    {
                        continue;
                    }
                    foreach (Chunk c in chunkArray)
                    {
                        if (c == null)
                        {
                            continue;
                        }
                        tileEntities = c.GetTileEntities();
                        if (tileEntities == null || tileEntities.Count < 1)
                        {
                            continue;
                        }
                        foreach (TileEntity tileEntity in tileEntities.dict.Values)
                        {
                            if (tileEntity == null)
                            {
                                continue;
                            }
                            if (tileEntity is TileEntitySecureLootContainer)
                            {
                                TileEntitySecureLootContainer SecureLoot = (TileEntitySecureLootContainer)tileEntity;
                                if (SecureLoot != null)
                                {
                                    PlatformUserIdentifierAbs platformUserIdentifierAbs = SecureLoot.GetOwner();
                                    if (platformUserIdentifierAbs != null && GameManager.Instance.adminTools.Users.GetUserPermissionLevel(platformUserIdentifierAbs) > Admin_Level)
                                    {
                                        ItemStack[] items = SecureLoot.items;
                                        if (items == null)
                                        {
                                            continue;
                                        }
                                        int slotNumber = 0;
                                        ItemStack itemStack;
                                        for (int j = 0; j < items.Length; j++)
                                        {
                                            itemStack = items[j];
                                            if (itemStack == null || itemStack.IsEmpty())
                                            {
                                                continue;
                                            }
                                            string itemName = itemStack.itemValue.ItemClass.Name ?? itemStack.itemValue.ItemClass.GetItemName();
                                            if (Dict.Contains(itemName))
                                            {
                                                SecureLoot.UpdateSlot(slotNumber, new ItemStack());
                                                tileEntity.SetModified();
                                                Vector3i chestPos = SecureLoot.localChunkPos;
                                                using (StreamWriter sw = new StreamWriter(DetectionFilepath, true, Encoding.UTF8))
                                                {
                                                    sw.WriteLine("[SERVERTOOLS] Removed '{0}' '{1}' from a secure loot located at '{2}' owned by '{3}'", itemStack.count, itemName, chestPos, SecureLoot.GetOwner().CombinedString);
                                                }
                                                Log.Out(string.Format("[SERVERTOOLS] Removed '{0}' '{1}' from a secure loot located at '{2}' owned by '{3}'", itemStack.count, itemName, chestPos, SecureLoot.GetOwner().CombinedString));
                                            }
                                            slotNumber++;
                                        }
                                    }
                                }
                            }
                            else if (tileEntity is TileEntitySecureLootContainerSigned)
                            {
                                TileEntitySecureLootContainerSigned SecureLoot = (TileEntitySecureLootContainerSigned)tileEntity;
                                if (SecureLoot != null)
                                {
                                    PlatformUserIdentifierAbs platformUserIdentifierAbs = SecureLoot.GetOwner();
                                    if (platformUserIdentifierAbs != null && GameManager.Instance.adminTools.Users.GetUserPermissionLevel(platformUserIdentifierAbs) > Admin_Level)
                                    {
                                        ItemStack[] items = SecureLoot.items;
                                        if (items == null)
                                        {
                                            continue;
                                        }
                                        int slotNumber = 0;
                                        ItemStack itemStack;
                                        for (int j = 0; j < items.Length; j++)
                                        {
                                            itemStack = items[j];
                                            if (itemStack == null || itemStack.IsEmpty())
                                            {
                                                continue;
                                            }
                                            string itemName = itemStack.itemValue.ItemClass.Name ?? itemStack.itemValue.ItemClass.GetItemName();
                                            if (Dict.Contains(itemName))
                                            {
                                                SecureLoot.UpdateSlot(slotNumber, itemStack.Clone());
                                                tileEntity.SetModified();
                                                Vector3i chestPos = SecureLoot.localChunkPos;
                                                using (StreamWriter sw = new StreamWriter(DetectionFilepath, true, Encoding.UTF8))
                                                {
                                                    sw.WriteLine("[SERVERTOOLS] Removed '{0}' '{1}' from a secure loot located at '{2}' owned by '{3}'", itemStack.count, itemName, chestPos, SecureLoot.GetOwner().CombinedString);
                                                }
                                                Log.Out(string.Format("[SERVERTOOLS] Removed '{0}' '{1}' from a secure loot located at '{2}' owned by '{3}'", itemStack.count, itemName, chestPos, SecureLoot.GetOwner().CombinedString));
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
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in InvalidItems.CheckStorage: {0}", e.Message));
            }
        }

        public static void UpgradeXml(XmlNodeList _nodeList)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                File.Delete(FilePath);
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<InvalidItems>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- <Item Name=\"air\" /> -->");
                    for (int i = 0; i < _nodeList.Count; i++)
                    {
                        if (_nodeList[i].NodeType == XmlNodeType.Comment)
                        {
                            if (!_nodeList[i].OuterXml.Contains("<!-- <Item Name=\"air") &&
                            !_nodeList[i].OuterXml.Contains("<!-- <Version") && !_nodeList[i].OuterXml.Contains("<Item Name=\"\""))
                            {
                                sw.WriteLine(_nodeList[i].OuterXml);
                            }
                        }
                    }
                    for (int i = 0; i < _nodeList.Count; i++)
                    {
                        if (_nodeList[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)_nodeList[i];
                            if (line.HasAttributes && line.Name == "Item")
                            {
                                string name = "";
                                if (line.HasAttribute("Name"))
                                {
                                    name = line.GetAttribute("Name");
                                }
                                sw.WriteLine("    <Item Name=\"{0}\" />", name);
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