using System.Collections.Generic;
using System.IO;
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
        private static SortedDictionary<int, string[]> dict = new SortedDictionary<int, string[]>();
        private static SortedDictionary<int, int[]> dict1 = new SortedDictionary<int, int[]>();
        public static List<string> categories = new List<string>();
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        private static bool updateConfig = false;

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
                dict.Clear();
                dict1.Clear();
                categories.Clear();
                fileWatcher.Dispose();
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
                    dict.Clear();
                    dict1.Clear();
                    categories.Clear();
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
                        if (!_line.HasAttribute("SecondaryName"))
                        {
                            updateConfig = true;
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
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring Shop  entry because of invalid (non-numeric) value for 'Price' attribute: {0}", subChild.OuterXml));
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
                        ItemClass _class;
                        Block _block;
                        if (int.TryParse(_name, out int _id))
                        {
                            _class = ItemClass.GetForId(_id);
                            _block = Block.GetBlockByName(_name, false);
                        }
                        else
                        {
                            _class = ItemClass.GetItemClass(_name, false);
                            _block = Block.GetBlockByName(_name, false);
                        }
                        if (_class == null && _block == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Shop entry skipped. Item not found: {0}", _name));
                            continue;
                        }
                        string _category = _line.GetAttribute("Category").ToLower();
                        if (!categories.Contains(_category))
                        {
                            categories.Add(_category);
                        }
                        if (_quality < 1)
                        {
                            _quality = 1;
                        }
                        else if (_quality > 600)
                        {
                            _quality = 600;
                        }
                        if (!dict.ContainsKey(_item))
                        {
                            string[] _strings = new string[] { _name, _secondaryname, _category };
                            dict.Add(_item, _strings);
                            int[] _integers = new int[] { _count, _quality, _price };
                            dict1.Add(_item, _integers);
                        }
                    }
                }
            }
            if (updateConfig)
            {
                updateConfig = false;
                UpdateXml();
            }
        }

        private static void UpdateXml()
        {
            fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<Shop>");
                sw.WriteLine("    <Items>");
                if (dict.Count > 0)
                {
                    foreach (KeyValuePair<int, string[]> kvp in dict)
                    {
                        int[] _values;
                        if (dict1.TryGetValue(kvp.Key, out _values))
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

        public static void PosCheck(ClientInfo _cInfo, string _categoryOrItem, int _form, int _count)
        {
            if (dict.Count > 0)
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
                            string _phrase821;
                            if (!Phrases.Dict.TryGetValue(821, out _phrase821))
                            {
                                _phrase821 = "You are not inside a market or trader area. Find one and use this command again.";
                            }
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase821 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                            string _phrase564;
                            if (!Phrases.Dict.TryGetValue(564, out _phrase564))
                            {
                                _phrase564 = "You are outside the market. Get inside it and try again.";
                            }
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase564 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                            string _phrase619;
                            if (!Phrases.Dict.TryGetValue(619, out _phrase619))
                            {
                                _phrase619 = "You are not inside a trader area. Find a trader and use this command again.";
                            }
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase619 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                string _phrase624;
                if (!Phrases.Dict.TryGetValue(624, out _phrase624))
                {
                    _phrase624 = "The shop does not contain any items. Contact an administrator";
                }
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase624 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
            else
            {
                Walletcheck(_cInfo, _categoryOrItem, _count);
            }
        }

        public static void ListCategories(ClientInfo _cInfo)
        {
            string _phrase617;
            if (!Phrases.Dict.TryGetValue(617, out _phrase617))
            {
                _phrase617 = "The shop categories are:";
            }
            string _categories = "";
            if (categories.Count > 1)
            {
                _categories = string.Join(", ", categories.ToArray());
            }
            else if (categories.Count == 1)
            {
                _categories = categories[0];
            }
            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase617 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _categories + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            string _phrase618;
            if (!Phrases.Dict.TryGetValue(618, out _phrase618))
            {
                _phrase618 = "Type {CommandPrivate}{Command57} 'category' to view that list.";
            }
            _phrase618 = _phrase618.Replace("{CommandPrivate}", ChatHook.Command_Private);
            _phrase618 = _phrase618.Replace("{Command57}", Command57);
            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase618 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
        }

        public static void ShowCategory(ClientInfo _cInfo, string _category)
        {
            if (categories.Contains(_category))
            {
                int _count = 0;
                for (int i = 0; i <= dict.Count; i++)
                {
                    string[] _dictValues;
                    if (dict.TryGetValue(i, out _dictValues))
                    {
                        if (_dictValues[2] == _category)
                        {
                            int[] _dict1Values;
                            if (dict1.TryGetValue(i, out _dict1Values))
                            {
                                if (_dict1Values[1] > 1)
                                {
                                    _count++;
                                    string _message = "# {Id}: {Count} {Item} {Quality} quality for {Price} {Name}[-]";
                                    _message = _message.Replace("{Id}", i.ToString());
                                    _message = _message.Replace("{Count}", _dict1Values[0].ToString());
                                    _message = _message.Replace("{Item}", _dictValues[1]);
                                    _message = _message.Replace("{Quality}", _dict1Values[1].ToString());
                                    _message = _message.Replace("{Price}", _dict1Values[2].ToString());
                                    _message = _message.Replace("{Name}", Wallet.Coin_Name);
                                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else
                                {
                                    _count++;
                                    string _message = "# {Id}: {Count} {Item} for {Price} {Name}[-]";
                                    _message = _message.Replace("{Id}", i.ToString());
                                    _message = _message.Replace("{Count}", _dict1Values[0].ToString());
                                    _message = _message.Replace("{Item}", _dictValues[1]);
                                    _message = _message.Replace("{Price}", _dict1Values[2].ToString());
                                    _message = _message.Replace("{Name}", Wallet.Coin_Name);
                                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                        }
                    }
                }
                if (_count != 0)
                {
                    string _phrase823;
                    if (!Phrases.Dict.TryGetValue(823, out _phrase823))
                    {
                        _phrase823 = "Type {CommandPrivate}{Command58} # to purchase the shop item. You can add how many times you want to buy it with {CommandPrivate}{Command58} # #";
                    }
                    _phrase823 = _phrase823.Replace("{CommandPrivate}", ChatHook.Command_Private);
                    _phrase823 = _phrase823.Replace("{Command58}", Command58);
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase823 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                string _phrase822;
                if (!Phrases.Dict.TryGetValue(822, out _phrase822))
                {
                    _phrase822 = "This category is missing. Check {CommandPrivate}{Command57}.";
                }
                _phrase822 = _phrase822.Replace("{CommandPrivate}", ChatHook.Command_Private);
                _phrase822 = _phrase822.Replace("{Command57}", Command57);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase822 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void Walletcheck(ClientInfo _cInfo, string _item, int _count)
        {
            int _id;
            if (!int.TryParse(_item, out _id))
            {
                string _phrase620;
                if (!Phrases.Dict.TryGetValue(620, out _phrase620))
                {
                    _phrase620 = "The item or amount # you are trying to buy is not an integer. Please input {CommandPrivate}{Command58} 1 2 for example.";
                }
                _phrase620 = _phrase620.Replace("{CommandPrivate}", ChatHook.Command_Private);
                _phrase620 = _phrase620.Replace("{Command58}", Command58);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase620 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else if (dict.ContainsKey(_id))
            {
                string[] _stringValues;
                if (dict.TryGetValue(_id, out _stringValues))
                {
                    int[] _integerValues;
                    if (dict1.TryGetValue(_id, out _integerValues))
                    {
                        int _currentCoins = Wallet.GetCurrentCoins(_cInfo.playerId);
                        int _newAmount = _integerValues[2] * _count;
                        if (_currentCoins >= _newAmount)
                        {
                            int _newCount = _integerValues[0] * _count;
                            ShopPurchase(_cInfo, _stringValues[0], _stringValues[2], _newCount, _integerValues[1], _newAmount, _currentCoins);
                        }
                        else
                        {
                            string _phrase621;
                            if (!Phrases.Dict.TryGetValue(621, out _phrase621))
                            {
                                _phrase621 = "You do not have enough {Name}. Your wallet balance is {Value}.";
                            }
                            _phrase621 = _phrase621.Replace("{Name}", Wallet.Coin_Name);
                            _phrase621 = _phrase621.Replace("{Value}", _currentCoins.ToString());
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase621 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Item not found for shop purchase" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void ShopPurchase(ClientInfo _cInfo, string _itemName, string _secondaryName, int _count, int _quality, int _price, int currentCoins)
        {
            World world = GameManager.Instance.World;
            ItemValue _itemValue = new ItemValue(ItemClass.GetItem(_itemName).type, _quality, _quality, false, null, 1);
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
                world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Killed);
                Wallet.SubtractCoinsFromWallet(_cInfo.playerId, _price);
                Log.Out(string.Format("Sold {0} to {1}", _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.Name, _cInfo.playerName));
                string _message = "{Count} {Item} was purchased through the shop. If your bag is full, check the ground.";
                _message = _message.Replace("{Count}", _count.ToString());
                if (_secondaryName != "")
                {
                    _message = _message.Replace("{Item}", _secondaryName);
                }
                else
                {
                    _message = _message.Replace("{Item}", _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.Name);
                }
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                string _message = "You can only purchase a full stack worth at a time. The maximum stack size for this is {Max}.";
                _message = _message.Replace("{Max}", _maxAllowed.ToString());
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}