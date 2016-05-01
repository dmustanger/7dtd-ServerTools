using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    public class InventoryCheck
    {
        public static bool IsEnabled = false;
        public static bool IsRunning = false;
        public static bool AnounceInvalidStack = false;
        public static bool BanPlayer = true;
        private static string file = "InvalidItems.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static SortedDictionary<string, string> dict = new SortedDictionary<string, string>();
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        public static void Load()
        {
            if (IsEnabled && !IsRunning)
            {
                LoadInvalidItemsXml();
                InitFileWatcher();
            }
        }

        public static void Unload()
        {
            fileWatcher.Dispose();
            IsRunning = false;
        }

        private static void LoadInvalidItemsXml()
        {
            if (!Utils.FileExists(filePath))
            {
                UpdateInvalidItemsXml();
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
            XmlNode _ICheckXml = xmlDoc.DocumentElement;
            foreach (XmlNode childNode in _ICheckXml.ChildNodes)
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
                        if (!dict.ContainsKey(_line.GetAttribute("itemName")))
                        {
                            dict.Add(_line.GetAttribute("itemName"), null);
                        }
                    }
                }
            }
        }

        private static void UpdateInvalidItemsXml()
        {
            fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<InvalidItems>");
                sw.WriteLine("    <Items>");
                if (dict.Count > 0)
                {
                    foreach (KeyValuePair<string, string> kvp in dict)
                    {
                        sw.WriteLine(string.Format("        <item itemName=\"{0}\" />", kvp.Key));
                    }
                }
                else
                {
                    sw.WriteLine(string.Format("        <item itemName=\"grass\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"dirt\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plainsGround\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"radiated\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"potassiumNitrate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"ironOre\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"clay\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"bedrock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sandStone\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"desertGround\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"asphalt\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"ice\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"fertileGrass\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"fertileFarmland\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"copperOre\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"burntForestGround\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"forestGround\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"coalOre\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrainFiller\" />"));
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
            LoadInvalidItemsXml();
        }

        public static void CheckInv(ClientInfo _cInfo, PlayerDataFile _playerDataFile)
        {
            if (_cInfo != null && !GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId))
            {
                for (int i = 0; i < _playerDataFile.inventory.Length; i++)
                {
                    ItemStack _intemStack = new ItemStack();
                    ItemValue _itemValue = new ItemValue();
                    _intemStack = _playerDataFile.inventory[i];
                    _itemValue = _intemStack.itemValue;
                    int _count = _playerDataFile.inventory[i].count;
                    if (_count > 0 && _itemValue != null && !_itemValue.Equals(ItemValue.None) && _cInfo != null)
                    {
                        int _maxAllowed = ItemClass.list[_itemValue.type].Stacknumber.Value;
                        string _name = ItemClass.list[_itemValue.type].GetItemName();
                        if (AnounceInvalidStack && _count > _maxAllowed)
                        {
                            string _phrase3 = "{PlayerName} you have a invalid item stack: {ItemName} {ItemCount}. Max per stack: {MaxPerStack}.";
                            if (!Phrases.Dict.TryGetValue(3, out _phrase3))
                            {
                                Log.Out("[SERVERTOOLS] Phrase 3 not found using default.");
                            }
                            _phrase3 = _phrase3.Replace("{PlayerName}", _cInfo.playerName);
                            _phrase3 = _phrase3.Replace("{ItemName}", _name);
                            _phrase3 = _phrase3.Replace("{ItemCount}", _count.ToString());
                            _phrase3 = _phrase3.Replace("{MaxPerStack}", _maxAllowed.ToString());
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("[FF8000]{0}[-]", _phrase3), "Server", false, "", false));
                            ChatLog.Log(_phrase3, "Server");
                        }
                        if (IsEnabled && dict.ContainsKey(_name))
                        {
                            if (BanPlayer)
                            {
                                string _phrase4 = "Cheat Detected: Auto banned {PlayerName} for having a invalid item: {ItemName}.";
                                if (!Phrases.Dict.TryGetValue(4, out _phrase4))
                                {
                                    Log.Out("[SERVERTOOLS] Phrase 4 not found using default.");
                                }
                                _phrase4 = _phrase4.Replace("{PlayerName}", _cInfo.playerName);
                                _phrase4 = _phrase4.Replace("{ItemName}", _name);
                                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0}[-]", _phrase4), "Server", false, "", false);
                                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 10 years \"Invalid Item {1}\"", _cInfo.entityId, _name), _cInfo);
                            }
                            else
                            {
                                string _phrase5 = "Cheat Detected: Auto kicked {PlayerName} for having a invalid item: {ItemName}.";
                                if (!Phrases.Dict.TryGetValue(5, out _phrase5))
                                {
                                    Log.Out("[SERVERTOOLS] Phrase 5 not found using default.");
                                }
                                _phrase5 = _phrase5.Replace("{PlayerName}", _cInfo.playerName);
                                _phrase5 = _phrase5.Replace("{ItemName}", _name);
                                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0}[-]", _phrase5), "Server", false, "", false);
                                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Invalid Item: {1}\"", _cInfo.entityId, _name), _cInfo);
                            }
                            break;
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
                        if (AnounceInvalidStack && _count > _maxAllowed)
                        {
                            string _phrase3 = "{PlayerName} you have a invalid item stack: {ItemName} {ItemCount}. Max per stack: {MaxPerStack}.";
                            if (!Phrases.Dict.TryGetValue(3, out _phrase3))
                            {
                                Log.Out("[SERVERTOOLS] Phrase 3 not found using default.");
                            }
                            _phrase3 = _phrase3.Replace("{PlayerName}", _cInfo.playerName);
                            _phrase3 = _phrase3.Replace("{ItemName}", _name);
                            _phrase3 = _phrase3.Replace("{ItemCount}", _count.ToString());
                            _phrase3 = _phrase3.Replace("{MaxPerStack}", _maxAllowed.ToString());
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("[FF8000]{0}[-]", _phrase3), "Server", false, "", false));
                            ChatLog.Log(_phrase3, "Server");
                        }
                        if (IsEnabled && dict.ContainsKey(_name))
                        {
                            if (BanPlayer)
                            {
                                string _phrase4 = "Cheat Detected: Auto banned {PlayerName} for having a invalid item: {ItemName}.";
                                if (!Phrases.Dict.TryGetValue(4, out _phrase4))
                                {
                                    Log.Out("[SERVERTOOLS] Phrase 4 not found using default.");
                                }
                                _phrase4 = _phrase4.Replace("{PlayerName}", _cInfo.playerName);
                                _phrase4 = _phrase4.Replace("{ItemName}", _name);
                                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0}[-]", _phrase4), "Server", false, "", false);
                                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 10 years \"Invalid Item {1}\"", _cInfo.entityId, _name), _cInfo);
                            }
                            else
                            {
                                string _phrase5 = "Cheat Detected: Auto kicked {PlayerName} for having a invalid item: {ItemName}.";
                                if (!Phrases.Dict.TryGetValue(5, out _phrase5))
                                {
                                    Log.Out("[SERVERTOOLS] Phrase 5 not found using default.");
                                }
                                _phrase5 = _phrase5.Replace("{PlayerName}", _cInfo.playerName);
                                _phrase5 = _phrase5.Replace("{ItemName}", _name);
                                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0}[-]", _phrase5), "Server", false, "", false);
                                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Invalid Item: {1}\"", _cInfo.entityId, _name), _cInfo);
                            }
                            break;
                        }
                    }
                }
            }
        }
    }
}