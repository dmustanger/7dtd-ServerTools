using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    public class InventoryCheck
    {
        public static bool AnounceInvalidStack = false;
        public static bool BanPlayer = true;
        public static bool IsEnabled = false;
        private static SortedDictionary<string, string> _invaliditems = new SortedDictionary<string, string>();
        private static string _file = "InvalidItems.xml";
        private static string _filepath = string.Format("{0}/{1}", Config._configpath, _file);
        public static FileSystemWatcher _fileWatcher = new FileSystemWatcher(Config._configpath, _file);
        public static bool IsRunning = false;
        private static List<string> InvalidItems
        {
            get { return new List<string>(_invaliditems.Keys); }
        }

        public static void Init()
        {
            if (IsEnabled && !IsRunning)
            {
                if (!Utils.FileExists(_filepath))
                {
                    UpdateInvalidItemsXml();
                }
                InitFileWatcher();
                IsRunning = true;
                LoadInvalidItemsXml();
            }
        }

        private static void LoadInvalidItemsXml()
        {
            if (!Utils.FileExists(_filepath))
            {
                UpdateInvalidItemsXml();
            }
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(_filepath);
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", _file, e.Message));
                return;
            }
            XmlNode _ICheckXml = xmlDoc.DocumentElement;
            _invaliditems.Clear();
            foreach (XmlNode childNode in _ICheckXml.ChildNodes)
            {
                if (childNode.Name == "Items")
                {
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
                        if(!_invaliditems.ContainsKey(_line.GetAttribute("itemName")))
                        {
                            _invaliditems.Add(_line.GetAttribute("itemName"), null);
                        }
                    }
                }
            }
        }

        private static void UpdateInvalidItemsXml()
        {
            _fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(_filepath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<InvalidItems>");
                sw.WriteLine("    <Items>");
                if (InvalidItems.Count > 0)
                {
                    foreach (KeyValuePair<string, string> kvp in _invaliditems)
                    {
                        sw.WriteLine(string.Format("        <item itemName=\"{0}\" />", kvp.Key));
                    }
                }
                else
                {
                    sw.WriteLine(string.Format("        <item itemName=\"concreteReinforced\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"bedrock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"desertGround\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tungstenOre\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"fertileGrass\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"gore\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"gore2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"gore3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"fertileFarmland\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"copperOre\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"burntForestGround\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"forestGround\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrainFiller\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"hardSod\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootWasteland\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootDesert\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootBurntForest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootPlains\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"animalGore\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"storageHealthInsecure\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"garageStorage\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"medicineCabinet\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"reinforcedWood\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"reinforcedWoodMetal\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"oven\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"porchLight01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"porchLight04\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"porchLight04Brass\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"ceilingLight01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"ceilingLight02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"ceilingLight02Brass\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"ceilingLight04\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"ceilingLight05\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"ceilingLight05Brass\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"wallLight01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"mailbox\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"chest01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"chest02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"coffin_top\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"coffin_bottom\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"trash_can01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"bin\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"suitcase\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"storageChest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"trashPile01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"trashPile02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"trashPile03\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"trashPile04\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"trashPile05\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"trashPile06\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"trashPile07\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"trashPile08\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"trashPile09\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"fridge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"gasPump\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"graveStone01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"graveStone02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"graveStone03\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"graveStone04\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"graveStone05\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"graveStone06\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"graveStone07\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"graveStone08\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"boardedWindows\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corpseLoot01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tubeDrywall\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"poleTop01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"streetLight01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"wallSafe\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corpseLoot02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"mailBox01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"mailBox02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"mailBox03\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"DeveloperChest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidWoodFrame\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"barrelSmokeTest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootMP\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelWall\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"munitionsBox\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"secureReinforcedDoorWooden\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"metalReinforcedDoorWooden\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"reinforcedTrunkPineTip\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"reinforcedScrapIronWall\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidScrapIronRampFrame\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"reinforcedScrapIronRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidScrapIronFrame\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"supplyCrateWorkingStiffs\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootCrateWorkingStiffs\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"supplyCrateShotgunMessiah\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootCrateShotgunMessiah\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelDoor1_v2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelDoor1_v3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodenHatch1_v2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodenHatch1_v3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"supplyCrateShamway\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootCrateShamway\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"mountainManStorageChest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"apacheArtifactChest\" />"));
                }
                sw.WriteLine("    </Items>");
                sw.WriteLine("</InvalidItems>");
                sw.Flush();
                sw.Close();
            }
            _fileWatcher.EnableRaisingEvents = true;
        }

        private static void InitFileWatcher()
        {
            _fileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
            _fileWatcher.Created += new FileSystemEventHandler(OnFileChanged);
            _fileWatcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            _fileWatcher.EnableRaisingEvents = true;
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            if (!Utils.FileExists(_filepath))
            {
                UpdateInvalidItemsXml();
            }
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
                        if (_count > _maxAllowed && AnounceInvalidStack)
                        {
                            string _phrase3 = "{PlayerName} you have a invalid item stack: {ItemName} {ItemCount}. Max per stack: {MaxPerStack}.";
                            if (Phrases._Phrases.TryGetValue(3, out _phrase3))
                            {
                                _phrase3 = _phrase3.Replace("{0}", _cInfo.playerName);
                                _phrase3 = _phrase3.Replace("{1}", _name);
                                _phrase3 = _phrase3.Replace("{2}", _count.ToString());
                                _phrase3 = _phrase3.Replace("{3}", _maxAllowed.ToString());
                                _phrase3 = _phrase3.Replace("{PlayerName}", _cInfo.playerName);
                                _phrase3 = _phrase3.Replace("{ItemName}", _name);
                                _phrase3 = _phrase3.Replace("{ItemCount}", _count.ToString());
                                _phrase3 = _phrase3.Replace("{MaxPerStack}", _maxAllowed.ToString());
                            }
                            _cInfo.SendPackage(new NetPackageGameMessage(string.Format("[FF8000]{0}[-]", _phrase3), "Server"));
                        }
                        if (_invaliditems.ContainsKey(_name))
                        {
                            if (BanPlayer)
                            {
                                string _phrase4 = "Cheat Detected: Auto banned {PlayerName} for having a invalid item: {ItemName}.";
                                if (Phrases._Phrases.TryGetValue(4, out _phrase4))
                                {
                                    _phrase4 = _phrase4.Replace("{0}", _cInfo.playerName);
                                    _phrase4 = _phrase4.Replace("{1}", _name);
                                    _phrase4 = _phrase4.Replace("{PlayerName}", _cInfo.playerName);
                                    _phrase4 = _phrase4.Replace("{ItemName}", _name);
                                }
                                GameManager.Instance.GameMessageServer(_cInfo, string.Format("[FF8000]{0}[-]", _phrase4), "Server");
                                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 10 years \"Invalid Item {1}\"", _cInfo.entityId, _name), _cInfo);
                            }
                            else
                            {
                                string _phrase5 = "Cheat Detected: Auto kicked {PlayerName} for having a invalid item: {ItemName}.";
                                if (Phrases._Phrases.TryGetValue(5, out _phrase5))
                                {
                                    _phrase5 = _phrase5.Replace("{0}", _cInfo.playerName);
                                    _phrase5 = _phrase5.Replace("{1}", _name);
                                    _phrase5 = _phrase5.Replace("{PlayerName}", _cInfo.playerName);
                                    _phrase5 = _phrase5.Replace("{ItemName}", _name);
                                }
                                GameManager.Instance.GameMessageServer(_cInfo, string.Format("[FF8000]{0}[-]", _phrase5), "Server");
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
                        if (_count > _maxAllowed && AnounceInvalidStack)
                        {
                            string _phrase3 = "{PlayerName} you have a invalid item stack: {ItemName} {ItemCount}. Max per stack: {MaxPerStack}.";
                            if (Phrases._Phrases.TryGetValue(3, out _phrase3))
                            {
                                _phrase3 = _phrase3.Replace("{0}", _cInfo.playerName);
                                _phrase3 = _phrase3.Replace("{1}", _name);
                                _phrase3 = _phrase3.Replace("{2}", _count.ToString());
                                _phrase3 = _phrase3.Replace("{3}", _maxAllowed.ToString());
                                _phrase3 = _phrase3.Replace("{PlayerName}", _cInfo.playerName);
                                _phrase3 = _phrase3.Replace("{ItemName}", _name);
                                _phrase3 = _phrase3.Replace("{ItemCount}", _count.ToString());
                                _phrase3 = _phrase3.Replace("{MaxPerStack}", _maxAllowed.ToString());
                            }
                            _cInfo.SendPackage(new NetPackageGameMessage(string.Format("[FF8000]{0}[-]", _phrase3), "Server"));
                        }
                        if (_invaliditems.ContainsKey(_name))
                        {
                            if (BanPlayer)
                            {
                                string _phrase4 = "Cheat Detected: Auto banned {PlayerName} for having a invalid item: {ItemName}.";
                                if (Phrases._Phrases.TryGetValue(4, out _phrase4))
                                {
                                    _phrase4 = _phrase4.Replace("{0}", _cInfo.playerName);
                                    _phrase4 = _phrase4.Replace("{1}", _name);
                                    _phrase4 = _phrase4.Replace("{PlayerName}", _cInfo.playerName);
                                    _phrase4 = _phrase4.Replace("{ItemName}", _name);
                                }
                                GameManager.Instance.GameMessageServer(_cInfo, string.Format("[FF8000]{0}[-]", _phrase4), "Server");
                                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 10 years \"Invalid Item {1}\"", _cInfo.entityId, _name), _cInfo);
                            }
                            else
                            {
                                string _phrase5 = "Cheat Detected: Auto kicked {PlayerName} for having a invalid item: {ItemName}.";
                                if (Phrases._Phrases.TryGetValue(5, out _phrase5))
                                {
                                    _phrase5 = _phrase5.Replace("{0}", _cInfo.playerName);
                                    _phrase5 = _phrase5.Replace("{1}", _name);
                                    _phrase5 = _phrase5.Replace("{PlayerName}", _cInfo.playerName);
                                    _phrase5 = _phrase5.Replace("{ItemName}", _name);
                                }
                                GameManager.Instance.GameMessageServer(_cInfo, string.Format("[FF8000]{0}[-]", _phrase5), "Server");
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