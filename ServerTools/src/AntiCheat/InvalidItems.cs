using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    public class InventoryCheck
    {
        public static bool IsEnabled = false, IsRunning = false, Anounce_Invalid_Stack = false, Ban_Player = false;
        public static int Admin_Level = 0, Days_Before_Log_Delete = 5;
        private static string file = "InvalidItems.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static List<string> dict = new List<string>();
        private static Dictionary<int, int> playerflag = new Dictionary<int, int>();
        private static List<int> dropCheck = new List<int>();
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        private static string _file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static string _filepath = string.Format("{0}/ServerTools/Logs/DetectionLogs/{1}", API.GamePath, _file);

        public static void PlayerLogsDir()
        {
            if (!Directory.Exists(API.GamePath + "/ServerTools/Logs/DetectionLogs"))
            {
                Directory.CreateDirectory(API.GamePath + "/ServerTools/Logs/DetectionLogs");
            }

            string[] files = Directory.GetFiles(API.GamePath + "/ServerTools/Logs/DetectionLogs");
            int _daysBeforeDeleted = (Days_Before_Log_Delete * -1);
            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                if (fi.CreationTime <= DateTime.Now.AddDays(_daysBeforeDeleted))
                {
                    fi.Delete();
                }
            }
        }

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
                    sw.WriteLine(string.Format("        <item itemName=\"terrDirt\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrOrePotassiumNitrate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrOreIron\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrOreLead\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrBedrock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrDesertGround\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrIce\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrFertileDirt\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrFertileGrass\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrFertileFarmland\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrOreSilver\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrOreCoal\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrainFiller\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrOreGold\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrDestroyedWoodDebris\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrOreOilDeposit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrOreDiamond\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrFertileFarmland\" />"));
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
            if (_cInfo != null)
            {
                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                if (Admin.PermissionLevel > Admin_Level)
                {
                    int _bagClean = 0, _invClean = 0, _totalBagCount = 0, _totalInventoryCount = 0;
                    for (int i = 0; i < _playerDataFile.inventory.Length; i++)
                    {
                        ItemStack _itemStack = new ItemStack();
                        ItemValue _itemValue = new ItemValue();
                        _itemStack = _playerDataFile.inventory[i];
                        _itemValue = _itemStack.itemValue;
                        int _count = _playerDataFile.inventory[i].count;
                        if (_count > 0 && _itemValue != null && !_itemValue.Equals(ItemValue.None) && _cInfo != null)
                        {
                            int _maxAllowed = ItemClass.list[_itemValue.type].Stacknumber.Value;
                            string _name = ItemClass.list[_itemValue.type].GetItemName();
                            if (Anounce_Invalid_Stack && _count > _maxAllowed)
                            {
                                string _phrase3;
                                if (!Phrases.Dict.TryGetValue(3, out _phrase3))
                                {
                                    _phrase3 = "you have a invalid item stack: {ItemName} {ItemCount}. Max per stack: {MaxPerStack}.";
                                }
                                _phrase3 = _phrase3.Replace("{ItemName}", _name);
                                _phrase3 = _phrase3.Replace("{ItemCount}", _count.ToString());
                                _phrase3 = _phrase3.Replace("{MaxPerStack}", _maxAllowed.ToString());
                                ChatHook.ChatMessage(_cInfo, "[FF0000]" + _cInfo.playerName  + _phrase3 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                ChatLog.Log(_phrase3, LoadConfig.Server_Response_Name);
                            }
                            if (IsEnabled && dict.Contains(_name))
                            {
                                if (Ban_Player)
                                {
                                    string _phrase4;
                                    if (!Phrases.Dict.TryGetValue(4, out _phrase4))
                                    {
                                        _phrase4 = "Cheat Detected: Auto banned {PlayerName} for having a invalid item: {ItemName}.";
                                    }
                                    _phrase4 = _phrase4.Replace("{PlayerName}", _cInfo.playerName);
                                    _phrase4 = _phrase4.Replace("{ItemName}", _name);
                                    ChatHook.ChatMessage(_cInfo, "[FF0000]" + _cInfo.playerName  + _phrase4 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                    SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Invalid Item {1}\"", _cInfo.entityId, _name), (ClientInfo)null);
                                    using (StreamWriter sw = new StreamWriter(_filepath, true))
                                    {
                                        sw.WriteLine(string.Format("Detected {0}, Steam Id {1}, with invalid item: {2}. Banned the player.", _cInfo.playerName, _cInfo.playerId, _name));
                                        sw.WriteLine();
                                        sw.Flush();
                                        sw.Close();
                                    }
                                }
                                else
                                {
                                    if (playerflag.ContainsKey(_cInfo.entityId))
                                    {
                                        int _value;
                                        if (playerflag.TryGetValue(_cInfo.entityId, out _value))
                                        {
                                            if (_value == 2)
                                            {
                                                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Invalid Item: {1}\"", _cInfo.entityId, _name), (ClientInfo)null);
                                                string _phrase5;
                                                if (!Phrases.Dict.TryGetValue(5, out _phrase5))
                                                {
                                                    _phrase5 = "Cheat Detected: Auto kicked {PlayerName} for having a invalid item: {ItemName}.";
                                                }
                                                _phrase5 = _phrase5.Replace("{PlayerName}", _cInfo.playerName);
                                                _phrase5 = _phrase5.Replace("{ItemName}", _name);
                                                ChatHook.ChatMessage(_cInfo, "[FF0000]" + _cInfo.playerName  + _phrase5 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);                                                
                                                using (StreamWriter sw = new StreamWriter(_filepath, true))
                                                {
                                                    sw.WriteLine(string.Format("Detected {0}, Steam Id {1}, with invalid item: {2}. Kicked the player.", _cInfo.playerName, _cInfo.playerId, _name));
                                                    sw.WriteLine();
                                                    sw.Flush();
                                                    sw.Close();
                                                }
                                                playerflag.Remove(_cInfo.entityId);
                                            }
                                            else
                                            {
                                                playerflag.Remove(_cInfo.entityId);
                                                playerflag[_cInfo.entityId] = 2;
                                                string _phrase799;
                                                if (!Phrases.Dict.TryGetValue(799, out _phrase799))
                                                {
                                                    _phrase799 = "Cheat Detected: {PlayerName} you are holding a invalid item: {ItemName}. Final warning, drop it!";
                                                }
                                                _phrase799 = _phrase799.Replace("{PlayerName}", _cInfo.playerName);
                                                _phrase799 = _phrase799.Replace("{ItemName}", _name);
                                                ChatHook.ChatMessage(_cInfo, "[FF0000]" + _cInfo.playerName  + _phrase799 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        playerflag.Add(_cInfo.entityId, 1);
                                        string _phrase800;
                                        if (!Phrases.Dict.TryGetValue(800, out _phrase800))
                                        {
                                            _phrase800 = "Cheat Detected: {PlayerName} you are holding a invalid item: {ItemName}. You have 30 seconds to drop it.";
                                        }
                                        _phrase800 = _phrase800.Replace("{PlayerName}", _cInfo.playerName);
                                        _phrase800 = _phrase800.Replace("{ItemName}", _name);
                                        ChatHook.ChatMessage(_cInfo, "[FF0000]" + _cInfo.playerName  + _phrase800 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                    }
                                }
                            }
                            else if (IsEnabled)
                            {
                                _totalInventoryCount++;
                                if (_totalInventoryCount == _count)
                                {
                                    _totalInventoryCount = 0;
                                    _invClean = 1;
                                }
                            }
                        }
                    }
                    for (int i = 0; i < _playerDataFile.bag.Length; i++)
                    {                        
                        ItemStack _intemStack = new ItemStack();
                        ItemValue _itemValue = new ItemValue();
                        _intemStack = _playerDataFile.bag[i];
                        _itemValue = _intemStack.itemValue;
                        int _count = _playerDataFile.bag[i].count;
                        if (_count > 0 && _itemValue != null && !_itemValue.Equals(ItemValue.None) && _cInfo != null)
                        {
                            int _maxAllowed = ItemClass.list[_itemValue.type].Stacknumber.Value;
                            string _name = ItemClass.list[_itemValue.type].GetItemName();
                            if (Anounce_Invalid_Stack && _count > _maxAllowed)
                            {
                                string _phrase3;
                                if (!Phrases.Dict.TryGetValue(3, out _phrase3))
                                {
                                    _phrase3 = "you have a invalid item stack: {ItemName} {ItemCount}. Max per stack: {MaxPerStack}.";
                                }
                                _phrase3 = _phrase3.Replace("{ItemName}", _name);
                                _phrase3 = _phrase3.Replace("{ItemCount}", _count.ToString());
                                _phrase3 = _phrase3.Replace("{MaxPerStack}", _maxAllowed.ToString());
                                ChatLog.Log(_phrase3, LoadConfig.Server_Response_Name);
                                ChatHook.ChatMessage(_cInfo, "[FF0000]" + _cInfo.playerName  + _phrase3 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            if (IsEnabled && dict.Contains(_name))
                            {
                                if (Ban_Player)
                                {
                                    SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Invalid Item {1}\"", _cInfo.entityId, _name), (ClientInfo)null);
                                    string _phrase4;
                                    if (!Phrases.Dict.TryGetValue(4, out _phrase4))
                                    {
                                        _phrase4 = "Cheat Detected: Auto banned {PlayerName} for having a invalid item: {ItemName}.";
                                    }
                                    _phrase4 = _phrase4.Replace("{PlayerName}", _cInfo.playerName);
                                    _phrase4 = _phrase4.Replace("{ItemName}", _name);
                                    ChatHook.ChatMessage(_cInfo, "[FF0000]" + _cInfo.playerName  + _phrase4 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                                else
                                {
                                    if (playerflag.ContainsKey(_cInfo.entityId))
                                    {
                                        int _value;
                                        if (playerflag.TryGetValue(_cInfo.entityId, out _value))
                                        {
                                            if (_value == 2)
                                            {
                                                string _phrase5;
                                                if (!Phrases.Dict.TryGetValue(5, out _phrase5))
                                                {
                                                    _phrase5 = "Cheat Detected: Auto kicked {PlayerName} for having a invalid item: {ItemName}.";
                                                }
                                                _phrase5 = _phrase5.Replace("{PlayerName}", _cInfo.playerName);
                                                _phrase5 = _phrase5.Replace("{ItemName}", _name);
                                                ChatHook.ChatMessage(_cInfo, "[FF0000]" + _cInfo.playerName  + _phrase5 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Invalid Item: {1}\"", _cInfo.entityId, _name), (ClientInfo)null);
                                                using (StreamWriter sw = new StreamWriter(_filepath, true))
                                                {
                                                    sw.WriteLine(string.Format("Detected {0}, Steam Id {1}, with invalid item: {2}. Kicked the player.", _cInfo.playerName, _cInfo.playerId, _name));
                                                    sw.WriteLine();
                                                    sw.Flush();
                                                    sw.Close();
                                                }
                                                playerflag.Remove(_cInfo.entityId);
                                            }
                                            else
                                            {
                                                playerflag.Remove(_cInfo.entityId);
                                                playerflag[_cInfo.entityId] = 2;
                                                string _phrase799;
                                                if (!Phrases.Dict.TryGetValue(799, out _phrase799))
                                                {
                                                    _phrase799 = "Cheat Detected: {PlayerName} you are holding a invalid item: {ItemName}. Final warning, drop it!";
                                                }
                                                _phrase799 = _phrase799.Replace("{PlayerName}", _cInfo.playerName);
                                                _phrase799 = _phrase799.Replace("{ItemName}", _name);
                                                ChatHook.ChatMessage(_cInfo, "[FF0000]" + _cInfo.playerName  + _phrase799 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        playerflag.Add(_cInfo.entityId, 1);
                                        string _phrase800;
                                        if (!Phrases.Dict.TryGetValue(800, out _phrase800))
                                        {
                                            _phrase800 = "Cheat Detected: {PlayerName} you are holding a invalid item: {ItemName}. You have 30 seconds to drop it.";
                                        }
                                        _phrase800 = _phrase800.Replace("{PlayerName}", _cInfo.playerName);
                                        _phrase800 = _phrase800.Replace("{ItemName}", _name);
                                        ChatHook.ChatMessage(_cInfo, "[FF0000]" + _cInfo.playerName  + _phrase800 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                    }
                                }
                            }
                            else if (IsEnabled)
                            {
                                _totalBagCount++;
                                if (_totalBagCount == _count)
                                {
                                    _totalBagCount = 0;
                                    _bagClean = 1;
                                }
                            }
                        }
                    }
                    if (_bagClean == 1 && _invClean == 1)
                    {
                        _bagClean = 0;
                        _invClean = 0;
                        if (dropCheck.Contains(_cInfo.entityId))
                        {
                            dropCheck.Remove(_cInfo.entityId);
                        }
                    }
                }
            }
        }
    }
}