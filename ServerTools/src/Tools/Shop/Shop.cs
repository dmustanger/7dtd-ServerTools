using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    class Shop
    {
        public static bool IsEnabled = false, IsRunning = false, Inside_Market = false, Inside_Traders = false;
        public static int Delay_Between_Uses = 60;
        public static string Command57 = "shop", Command58 = "shop buy";
        private const string file = "Shop.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        public static SortedDictionary<int, string[]> Dict = new SortedDictionary<int, string[]>();
        public static SortedDictionary<int, int[]> Dict1 = new SortedDictionary<int, int[]>();
        public static List<string> Categories = new List<string>();
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
            if (!IsEnabled && IsRunning)
            {
                Dict.Clear();
                Dict1.Clear();
                Categories.Clear();
                FileWatcher.Dispose();
                IsRunning = false;
            }
        }

        public static void LoadXml()
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
                    Dict.Clear();
                    Dict1.Clear();
                    Categories.Clear();
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Shop' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("Item"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of missing Item attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Name"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of missing Name attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Count"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of missing Count attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Quality"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of missing Quality attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Price"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of missing Price attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Category"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of missing Category attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("Item"), out int _item))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring Shop entry because of invalid (non-numeric) value for 'Item' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("Count"), out int _count))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring Shop entry because of invalid (non-numeric) value for 'Count' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("Quality"), out int _quality))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring Shop entry because of invalid (non-numeric) value for 'Quality' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("Price"), out int _price))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring Shop entry because of invalid (non-numeric) value for 'Price' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string _name = _line.GetAttribute("Name");
                        string _secondaryname;
                        if (_line.HasAttribute("SecondaryName"))
                        {
                            _secondaryname = _line.GetAttribute("SecondaryName");
                        }
                        else
                        {
                            _secondaryname = _name;
                        }
                        ItemValue _itemValue = ItemClass.GetItem(_name, false);
                        if (_itemValue.type == ItemValue.None.type)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring Shop entry because item could not be found: {0}", _name));
                            continue;
                        }
                        if (_count > _itemValue.ItemClass.Stacknumber.Value)
                        {
                            _count = _itemValue.ItemClass.Stacknumber.Value;
                        }
                        string _category = _line.GetAttribute("Category").ToLower();
                        if (!Categories.Contains(_category))
                        {
                            Categories.Add(_category);
                        }
                        if (_quality < 1)
                        {
                            _quality = 1;
                        }
                        else if (_quality > 600)
                        {
                            _quality = 600;
                        }
                        if (!Dict.ContainsKey(_item))
                        {
                            string[] _strings = new string[] { _name, _secondaryname, _category };
                            Dict.Add(_item, _strings);
                            int[] _integers = new int[] { _count, _quality, _price };
                            Dict1.Add(_item, _integers);
                        }
                    }
                }
            }
        }

        public static void UpdateXml()
        {
            FileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<Shop>");
                sw.WriteLine("    <Items>");
                if (Dict.Count > 0)
                {
                    foreach (KeyValuePair<int, string[]> kvp in Dict)
                    {
                        int[] _values;
                        if (Dict1.TryGetValue(kvp.Key, out _values))
                        {
                            sw.WriteLine(string.Format("        <Shop Item=\"{0}\" Name=\"{1}\" SecondaryName=\"{2}\" Count=\"{3}\" Quality=\"{4}\" Price=\"{5}\" Category=\"{6}\" />", kvp.Key, kvp.Value[0], kvp.Value[1], _values[0], _values[1], _values[2], kvp.Value[2]));
                        }
                    }
                }
                else
                {
                    sw.WriteLine("        <Shop Item=\"1\" Name=\"drinkJarBoiledWater\" SecondaryName=\"Bottled Water\" Count=\"1\" Quality=\"1\" Price=\"20\" Category=\"Food\" />");
                    sw.WriteLine("        <Shop Item=\"2\" Name=\"drinkJarBeer\" SecondaryName=\"Beer\" Count=\"1\" Quality=\"1\" Price=\"50\" Category=\"food\" />");
                    sw.WriteLine("        <Shop Item=\"3\" Name=\"brickBlock\" SecondaryName=\"Wood\" Count=\"1\" Quality=\"1\" Price=\"50\" Category=\"mats\" />");
                    sw.WriteLine("        <Shop Item=\"4\" Name=\"woodFrameBlock\" SecondaryName=\"Wood Frame Block\" Count=\"1\" Quality=\"1\" Price=\"20\" Category=\"mats\" />");
                    sw.WriteLine("        <Shop Item=\"5\" Name=\"foodCanChicken\" SecondaryName=\"Can of Chicken\" Count=\"1\" Quality=\"1\" Price=\"50\" Category=\"food\" />");
                    sw.WriteLine("        <Shop Item=\"6\" Name=\"foodCanChili\" SecondaryName=\"Can of Chilli\" Count=\"1\" Quality=\"1\" Price=\"50\" Category=\"food\" />");
                    sw.WriteLine("        <Shop Item=\"7\" Name=\"foodCropCorn\" SecondaryName=\"Corn\" Count=\"5\" Quality=\"1\" Price=\"50\" Category=\"food\" />");
                    sw.WriteLine("        <Shop Item=\"8\" Name=\"foodCropPotato\" SecondaryName=\"Potato\" Count=\"5\" Quality=\"1\" Price=\"50\" Category=\"food\" />");
                    sw.WriteLine("        <Shop Item=\"9\" Name=\"medicalBandage\" SecondaryName=\"First Aid Bandage\" Count=\"1\" Quality=\"1\" Price=\"50\" Category=\"meds\" />");
                    sw.WriteLine("        <Shop Item=\"10\" Name=\"drugPainkillers\" SecondaryName=\"Pain Killers\" Count=\"5\" Quality=\"1\" Price=\"150\" Category=\"meds\" />");
                    sw.WriteLine("        <Shop Item=\"11\" Name=\"gunHandgunT1Pistol\" SecondaryName=\"Pistol\" Count=\"1\" Quality=\"100\" Price=\"500\" Category=\"weapon\" />");
                    sw.WriteLine("        <Shop Item=\"12\" Name=\"casinoCoin\" SecondaryName=\"casinoCoin\" Count=\"1\" Quality=\"1\" Price=\"1\" Category=\"extra\" />");
                }
                sw.WriteLine("    </Items>");
                sw.WriteLine("</Shop>");
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
            if (!Utils.FileExists(filePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        public static void PosCheck(ClientInfo _cInfo, string _categoryOrItem, int _form, int _count)
        {
            if (Dict.Count > 0)
            {
                EntityPlayer _player = PersistentOperations.GetEntityPlayer(_cInfo.playerId);
                if (_player != null)
                {
                    if (Inside_Market && Inside_Traders)
                    {
                        int x, y, z;
                        string[] _cords = Market.Market_Position.Split(',');
                        int.TryParse(_cords[0], out x);
                        int.TryParse(_cords[1], out y);
                        int.TryParse(_cords[2], out z);
                        if ((x - _player.position.x) * (x - _player.position.x) + (z - _player.position.z) * (z - _player.position.z) <= Market.Market_Size * Market.Market_Size && GameManager.Instance.World.IsWithinTraderArea(new Vector3i(_player.position.x, _player.position.y, _player.position.z)))
                        {
                            PosCheck2(_cInfo, _categoryOrItem, _form, _count);
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue(349, out string _phrase349);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase349 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else if (Inside_Market && !Inside_Traders)
                    {
                        int x, y, z;
                        string[] _cords = Market.Market_Position.Split(',');
                        int.TryParse(_cords[0], out x);
                        int.TryParse(_cords[1], out y);
                        int.TryParse(_cords[2], out z);
                        if ((x - _player.position.x) * (x - _player.position.x) + (z - _player.position.z) * (z - _player.position.z) <= Market.Market_Size * Market.Market_Size)
                        {
                            PosCheck2(_cInfo, _categoryOrItem, _form, _count);
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue(350, out string _phrase350);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase350 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else if (!Inside_Market && Inside_Traders)
                    {
                        World world = GameManager.Instance.World;
                        Vector3i playerPos = new Vector3i(_player.position.x, _player.position.y, _player.position.z);
                        if (world.IsWithinTraderArea(playerPos))
                        {
                            PosCheck2(_cInfo, _categoryOrItem, _form, _count);
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue(343, out string _phrase343);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase343 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else if (!Inside_Market && !Inside_Traders)
                    {
                        PosCheck2(_cInfo, _categoryOrItem, _form, _count);
                    }
                }
            }
            else
            {
                Phrases.Dict.TryGetValue(348, out string _phrase348);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase348 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void PosCheck2(ClientInfo _cInfo, string _categoryOrItem, int _form, int _count)
        {
            if (_form == 1)
            {
                ListCategories(_cInfo);
            }
            else if (_form == 2)
            {
                ShowCategory(_cInfo, _categoryOrItem);
            }
            else if (int.TryParse(_categoryOrItem, out int _id))
            {
                Walletcheck(_cInfo, _id, _count);
            }
        }

        public static void ListCategories(ClientInfo _cInfo)
        {
            Phrases.Dict.TryGetValue(341, out string _phrase341);
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase341 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            string _categories = "";
            if (Categories.Count > 1)
            {
                _categories = string.Join(", ", Categories.ToArray());
            }
            else if (Categories.Count == 1)
            {
                _categories = Categories[0];
            }
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _categories + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            Phrases.Dict.TryGetValue(342, out string _phrase342);
            _phrase342 = _phrase342.Replace("{CommandPrivate}", ChatHook.Chat_Command_Prefix1);
            _phrase342 = _phrase342.Replace("{Command57}", Command57);
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase342 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
        }

        public static void ShowCategory(ClientInfo _cInfo, string _category)
        {
            if (Categories.Contains(_category))
            {
                int _count = 0;
                for (int i = 0; i <= Dict.Count; i++)
                {
                    string[] _dictValues;
                    if (Dict.TryGetValue(i, out _dictValues))
                    {
                        if (_dictValues[2] == _category)
                        {
                            int[] _dict1Values;
                            if (Dict1.TryGetValue(i, out _dict1Values))
                            {
                                if (_dict1Values[1] > 1)
                                {
                                    _count++;
                                    Phrases.Dict.TryGetValue(351, out string _phrase351);
                                    _phrase351 = _phrase351.Replace("{Id}", i.ToString());
                                    _phrase351 = _phrase351.Replace("{Count}", _dict1Values[0].ToString());
                                    _phrase351 = _phrase351.Replace("{Item}", _dictValues[1]);
                                    _phrase351 = _phrase351.Replace("{Quality}", _dict1Values[1].ToString());
                                    _phrase351 = _phrase351.Replace("{Price}", _dict1Values[2].ToString());
                                    _phrase351 = _phrase351.Replace("{Name}", Wallet.Coin_Name);
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase351 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else
                                {
                                    _count++;
                                    Phrases.Dict.TryGetValue(352, out string _phrase352);
                                    _phrase352 = _phrase352.Replace("{Id}", i.ToString());
                                    _phrase352 = _phrase352.Replace("{Count}", _dict1Values[0].ToString());
                                    _phrase352 = _phrase352.Replace("{Item}", _dictValues[1]);
                                    _phrase352 = _phrase352.Replace("{Price}", _dict1Values[2].ToString());
                                    _phrase352 = _phrase352.Replace("{Name}", Wallet.Coin_Name);
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase352 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                        }
                    }
                }
                if (_count != 0)
                {
                    Phrases.Dict.TryGetValue(353, out string _phrase353);
                    _phrase353 = _phrase353.Replace("{CommandPrivate}", ChatHook.Chat_Command_Prefix1);
                    _phrase353 = _phrase353.Replace("{Command58}", Command58);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase353 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                Phrases.Dict.TryGetValue(354, out string _phrase354);
                _phrase354 = _phrase354.Replace("{CommandPrivate}", ChatHook.Chat_Command_Prefix1);
                _phrase354 = _phrase354.Replace("{Command57}", Command57);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase354 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void Walletcheck(ClientInfo _cInfo, int _item, int _count)
        {
            if (Dict.ContainsKey(_item))
            {
                if (Dict.TryGetValue(_item, out string[] _stringValues))
                {
                    if (Dict1.TryGetValue(_item, out int[] _integerValues))
                    {
                        int _currentCoins = Wallet.GetCurrentCoins(_cInfo.playerId);
                        int _newAmount = _integerValues[2] * _count;
                        if (_currentCoins >= _newAmount)
                        {
                            int _newCount = _integerValues[0] * _count;
                            ShopPurchase(_cInfo, _stringValues[0], _stringValues[1], _newCount, _integerValues[1], _newAmount);
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue(345, out string _phrase345);
                            _phrase345 = _phrase345.Replace("{CoinName}", Wallet.Coin_Name);
                            _phrase345 = _phrase345.Replace("{Value}", _currentCoins.ToString());
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase345 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
            }
            else
            {
                Phrases.Dict.TryGetValue(355, out string _phrase355);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase355 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void ShopPurchase(ClientInfo _cInfo, string _itemName, string _secondaryName, int _count, int _quality, int _price)
        {
            World world = GameManager.Instance.World;
            ItemValue _itemValue = new ItemValue(ItemClass.GetItem(_itemName).type, _quality, _quality, false, null);
            int _maxAllowed = _itemValue.ItemClass.Stacknumber.Value;
            if (_count <= _maxAllowed)
            {
                var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                {
                    entityClass = EntityClass.FromString("item"),
                    id = EntityFactory.nextEntityID++,
                    itemStack = new ItemStack(_itemValue, _count),
                    pos = world.Players.dict[_cInfo.entityId].position,
                    rot = new Vector3(20f, 0f, 20f),
                    lifetime = 60f,
                    belongsPlayerId = _cInfo.entityId
                });
                world.SpawnEntityInWorld(entityItem);
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
                world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                Wallet.SubtractCoinsFromWallet(_cInfo.playerId, _price);
                Log.Out(string.Format("Sold {0} to {1} {2} through the shop", _itemValue.ItemClass.Name, _cInfo.playerId, _cInfo.playerName));
                Phrases.Dict.TryGetValue(356, out string _phrase356);
                _phrase356 = _phrase356.Replace("{Count}", _count.ToString());
                if (_secondaryName != "")
                {
                    _phrase356 = _phrase356.Replace("{Item}", _secondaryName);
                }
                else
                {
                    _phrase356 = _phrase356.Replace("{Item}", _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.GetItemName());
                }
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase356 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                Phrases.Dict.TryGetValue(357, out string _phrase357);
                _phrase357 = _phrase357.Replace("{Value}", _maxAllowed.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase357 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}