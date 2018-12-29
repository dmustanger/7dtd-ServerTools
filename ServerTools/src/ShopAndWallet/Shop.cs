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
        private const string file = "Shop.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static SortedDictionary<int, string[]> dict = new SortedDictionary<int, string[]>();
        private static SortedDictionary<int, int[]> dict1 = new SortedDictionary<int, int[]>();
        private static List<string> categories = new List<string>();
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
                fileWatcher.Dispose();
                IsRunning = false;
                dict.Clear();
                dict1.Clear();
                categories.Clear();
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
                if (childNode.Name == "items")
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
                        if (!_line.HasAttribute("item"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of missing item attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("name"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of missing name attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("secondaryname"))
                        {
                            updateConfig = true;
                        }
                        if (!_line.HasAttribute("count"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of missing count attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("quality"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of missing quality attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("price"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of missing price attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("category"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Shop entry because of missing category attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        int _item = 1;
                        int _count = 1;
                        int _quality = 1;
                        int _price = 1;
                        if (!int.TryParse(_line.GetAttribute("item"), out _item))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring Shop entry because of invalid (non-numeric) value for 'item' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("count"), out _count))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring Shop entry because of invalid (non-numeric) value for 'count' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("quality"), out _quality))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring Shop entry because of invalid (non-numeric) value for 'quality' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("price"), out _price))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring Shop Item entry because of invalid (non-numeric) value for 'price' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (_quality > 6)
                        {
                            _quality = 1;
                        }
                        string _name = _line.GetAttribute("name");
                        string _secondaryname;
                        if (_line.HasAttribute("secondaryname"))
                        {
                            _secondaryname = _line.GetAttribute("secondaryname");
                        }
                        else
                        {
                            _secondaryname = _name;
                        }
                        ItemClass _class;
                        Block _block;
                        int _id;
                        if (int.TryParse(_name, out _id))
                        {
                            _class = ItemClass.GetForId(_id);
                            _block = Block.GetBlockByName(_name, true);
                        }
                        else
                        {
                            _class = ItemClass.GetItemClass(_name, true);
                            _block = Block.GetBlockByName(_name, true);
                        }
                        if (_class == null && _block == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Shop entry skipped. Item not found: {0}", _name));
                            continue;
                        }
                        string _category = _line.GetAttribute("category");
                        _category = _category.ToLower();
                        if (!categories.Contains(_category))
                        {
                            categories.Add(_category);
                        }
                        if (!dict.ContainsKey(_item))
                        {
                            string[] _n = new string[] { _name, _secondaryname, _category };
                            dict.Add(_item, _n);
                            int[] _c = new int[] { _count, _quality, _price };
                            dict1.Add(_item, _c);
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
                sw.WriteLine("    <items>");
                if (dict.Count > 0)
                {
                    foreach (KeyValuePair<int, string[]> kvp in dict)
                    {
                        int[] _values;
                        if (dict1.TryGetValue(kvp.Key, out _values))
                        {
                            sw.WriteLine(string.Format("        <shop item=\"{0}\" name=\"{1}\" secondaryname=\"{2}\" count=\"{3}\" quality=\"{4}\" price=\"{5}\" category=\"{6}\" />", kvp.Key, kvp.Value[0], kvp.Value[1], _values[0], _values[1], _values[2], kvp.Value[2]));
                        }
                    }
                }
                else
                {
                    sw.WriteLine("        <shop item=\"1\" name=\"drinkJarBoiledWater\" secondaryName=\"Bottled Water\" count=\"1\" quality=\"1\" price=\"20\" category=\"food\" />");
                    sw.WriteLine("        <shop item=\"2\" name=\"drinkJarBeer\" secondaryName=\"Beer\" count=\"1\" quality=\"1\" price=\"50\" category=\"food\" />");
                    sw.WriteLine("        <shop item=\"3\" name=\"brickBlock\" secondaryName=\"Wood\" count=\"1\" quality=\"1\" price=\"50\" category=\"mats\" />");
                    sw.WriteLine("        <shop item=\"4\" name=\"woodFrameBlock\" secondaryName=\"Wood Frame Block\" count=\"1\" quality=\"1\" price=\"20\" category=\"mats\" />");
                    sw.WriteLine("        <shop item=\"5\" name=\"foodCanChicken\" secondaryName=\"Can of Chicken\" count=\"1\" quality=\"1\" price=\"50\" category=\"food\" />");
                    sw.WriteLine("        <shop item=\"6\" name=\"foodCanChili\" secondaryName=\"Can of Chilli\" count=\"1\" quality=\"1\" price=\"50\" category=\"food\" />");
                    sw.WriteLine("        <shop item=\"7\" name=\"foodCropCorn\" secondaryName=\"Corn\" count=\"5\" quality=\"1\" price=\"50\" category=\"food\" />");
                    sw.WriteLine("        <shop item=\"8\" name=\"foodCropPotato\" secondaryName=\"Potato\" count=\"5\" quality=\"1\" price=\"50\" category=\"food\" />");
                    sw.WriteLine("        <shop item=\"9\" name=\"medicalBandage\" secondaryName=\"First Aid Bandage\" count=\"1\" quality=\"1\" price=\"50\" category=\"meds\" />");
                    sw.WriteLine("        <shop item=\"10\" name=\"drugPainkillers\" secondaryName=\"Pain Killers\" count=\"5\" quality=\"1\" price=\"150\" category=\"meds\" />");
                    sw.WriteLine("        <shop item=\"11\" name=\"gunPistol\" secondaryName=\"Pistol\" count=\"1\" quality=\"100\" price=\"500\" category=\"weapon\" />");
                    sw.WriteLine("        <shop item=\"12\" name=\"casinoCoin\" secondaryName=\"casinoCoin\" count=\"1\" quality=\"1\" price=\"1\" category=\"extra\" />");
                }
                sw.WriteLine("    </items>");
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

        public static void PosCheck(ClientInfo _cInfo, string _playerName, string _categoryOrItem, int _form)
        {
            if (dict.Count > 0)
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                if (Inside_Market && Inside_Traders)
                {
                    int x, y, z;
                    string[] _cords = SetMarket.Market_Position.Split(',');
                    int.TryParse(_cords[0], out x);
                    int.TryParse(_cords[1], out y);
                    int.TryParse(_cords[2], out z);
                    if ((x - _player.position.x) * (x - _player.position.x) + (z - _player.position.z) * (z - _player.position.z) <= MarketChat.Market_Size * MarketChat.Market_Size)
                    {
                        PosCheck2(_cInfo, _playerName, _categoryOrItem, _form);
                        return;
                    }
                    World world = GameManager.Instance.World;
                    Vector3i playerPos = new Vector3i((int)_player.position.x, (int)_player.position.y, (int)_player.position.z);
                    if (world.IsWithinTraderArea(playerPos))
                    {
                        PosCheck2(_cInfo, _playerName, _categoryOrItem, _form);
                    }
                    else
                    {
                        string _phrase821;
                        if (!Phrases.Dict.TryGetValue(821, out _phrase821))
                        {
                            _phrase821 = " you are not inside a market or trader area. Find one and use this command again.";
                        }
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase821 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else if (Inside_Market && !Inside_Traders)
                {
                    int x, y, z;
                    string[] _cords = SetMarket.Market_Position.Split(',');
                    int.TryParse(_cords[0], out x);
                    int.TryParse(_cords[1], out y);
                    int.TryParse(_cords[2], out z);
                    if ((x - _player.position.x) * (x - _player.position.x) + (z - _player.position.z) * (z - _player.position.z) <= MarketChat.Market_Size * MarketChat.Market_Size)
                    {
                        PosCheck2(_cInfo, _playerName, _categoryOrItem, _form);
                    }
                    else
                    {
                        string _phrase564;
                        if (!Phrases.Dict.TryGetValue(564, out _phrase564))
                        {
                            _phrase564 = " you are outside the market. Get inside it and try again.";
                        }
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase564 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else if (!Inside_Market && Inside_Traders)
                {
                    World world = GameManager.Instance.World;
                    Vector3i playerPos = new Vector3i((int)_player.position.x, (int)_player.position.y, (int)_player.position.z);
                    if (world.IsWithinTraderArea(playerPos))
                    {
                        PosCheck2(_cInfo, _playerName, _categoryOrItem, _form);
                    }
                    else
                    {
                        string _phrase619;
                        if (!Phrases.Dict.TryGetValue(619, out _phrase619))
                        {
                            _phrase619 = " you are not inside a trader area. Find a trader and use this command again.";
                        }
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase619 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else if (!Inside_Market && !Inside_Traders)
                {
                    PosCheck2(_cInfo, _playerName, _categoryOrItem, _form);
                }
            }
            else
            {
                string _phrase624;
                if (!Phrases.Dict.TryGetValue(624, out _phrase624))
                {
                    _phrase624 = " the shop does not contain any items. Contact an administrator";
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase624 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void PosCheck2(ClientInfo _cInfo, string _playerName, string _categoryOrItem, int _form)
        {
            if (_form == 1)
            {
                ListCategories(_cInfo, _playerName);
            }
            else if (_form == 2)
            {
                ShowCategory(_cInfo, _playerName, _categoryOrItem);
            }
            else
            {
                Walletcheck(_cInfo, _playerName, _categoryOrItem);
            }
        }

        public static void ListCategories(ClientInfo _cInfo, string _playerName)
        {
            string _phrase617;
            if (!Phrases.Dict.TryGetValue(617, out _phrase617))
            {
                _phrase617 = "The shop categories are:";
            }
            string _categories = string.Join(", ", categories.ToArray());
            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase617 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _categories + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            string _phrase618;
            if (!Phrases.Dict.TryGetValue(618, out _phrase618))
            {
                _phrase618 = "Type /shop 'category' to view that list.";
            }
            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase618 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
        }

        public static void ShowCategory(ClientInfo _cInfo, string _playerName, string _category)
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
                                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                        _phrase823 = " type /buy # to purchase the shop item. You can add how many times you want to buy it. /buy # #";
                    }
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase823 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                string _phrase822;
                if (!Phrases.Dict.TryGetValue(822, out _phrase822))
                {
                    _phrase822 = " this category is missing. Check /shop.";
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase822 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void Walletcheck(ClientInfo _cInfo, string _playerName, string _item)
        {
            int _id;
            if (_item.Contains(" "))
            {
                string[] _idAmount = _item.Split(' ');
                if (!int.TryParse(_idAmount[0], out _id))
                {
                    string _phrase620;
                    if (!Phrases.Dict.TryGetValue(620, out _phrase620))
                    {
                        _phrase620 = " the item or amount # you are trying to buy is not an integer. Please input /buy 1 2 for example.";
                    }
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase620 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    int _amount;
                    if (!int.TryParse(_idAmount[1], out _amount))
                    {
                        string _phrase620;
                        if (!Phrases.Dict.TryGetValue(620, out _phrase620))
                        {
                            _phrase620 = " the item or amount # you are trying to buy is not an integer. Please input /buy 1 2 for example.";
                        }
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase620 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        if (dict.ContainsKey(_id))
                        {
                            string[] _stringValues;
                            if (dict.TryGetValue(_id, out _stringValues))
                            {
                                int[] _integerValues;
                                if (dict1.TryGetValue(_id, out _integerValues))
                                {
                                    int _currentCoins = Wallet.GetcurrentCoins(_cInfo);
                                    int _newAmount = _integerValues[2] * _amount;
                                    if (_currentCoins >= _newAmount)
                                    {
                                        int _newCount = _integerValues[0] * _amount;
                                        ShopPurchase(_cInfo, _stringValues[0], _newCount, _integerValues[1], _newAmount, _playerName, _currentCoins);
                                    }
                                    else
                                    {
                                        string _phrase621;
                                        if (!Phrases.Dict.TryGetValue(621, out _phrase621))
                                        {
                                            _phrase621 = " you do not have enough {Name}. Your wallet balance is {Value}.";
                                        }
                                        _phrase621 = _phrase621.Replace("{Name}", Wallet.Coin_Name);
                                        _phrase621 = _phrase621.Replace("{Value}", _currentCoins.ToString());
                                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase621 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (int.TryParse(_item, out _id))
                {
                    if (dict.ContainsKey(_id))
                    {
                        string[] _stringValues;
                        if (dict.TryGetValue(_id, out _stringValues))
                        {
                            int[] _integerValues;
                            if (dict1.TryGetValue(_id, out _integerValues))
                            {
                                int _currentCoins = Wallet.GetcurrentCoins(_cInfo);
                                if (_currentCoins >= _integerValues[2])
                                {
                                    ShopPurchase(_cInfo, _stringValues[0], _integerValues[0], _integerValues[1], _integerValues[2], _playerName, _currentCoins);
                                }
                                else
                                {
                                    string _phrase621;
                                    if (!Phrases.Dict.TryGetValue(621, out _phrase621))
                                    {
                                        _phrase621 = " you do not have enough {Name}. Your wallet balance is {Value}.";
                                    }
                                    _phrase621 = _phrase621.Replace("{Name}", Wallet.Coin_Name);
                                    _phrase621 = _phrase621.Replace("{Value}", _currentCoins.ToString());
                                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase621 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                        }
                    }
                }
                else
                {
                    string _phrase622;
                    if (!Phrases.Dict.TryGetValue(622, out _phrase622))
                    {
                        _phrase622 = " there was no item # matching the shop. Check the shop category again.";
                    }
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase622 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void ShopPurchase(ClientInfo _cInfo, string _itemName, int _count, int _quality, int _price, string _playerName, int currentCoins)
        {
            World world = GameManager.Instance.World;
            ItemValue itemValue = new ItemValue(ItemClass.GetItem(_itemName).type, _quality, _quality, true);
            var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
            {
                entityClass = EntityClass.FromString("item"),
                id = EntityFactory.nextEntityID++,
                itemStack = new ItemStack(itemValue, _count),
                pos = world.Players.dict[_cInfo.entityId].position,
                rot = new Vector3(20f, 0f, 20f),
                lifetime = 60f,
                belongsPlayerId = _cInfo.entityId
            });
            world.SpawnEntityInWorld(entityItem);
            _cInfo.SendPackage(new NetPackageEntityCollect(entityItem.entityId, _cInfo.entityId));
            world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Killed);
            SdtdConsole.Instance.Output(string.Format("Sold {0} to {1}.", itemValue.ItemClass.GetLocalizedItemName() ?? itemValue.ItemClass.Name, _cInfo.playerName));
            string _message = "{Count} {Item} was purchased through the shop. If your bag is full, check the ground.";
            _message = _message.Replace("{Count}", _count.ToString());
            _message = _message.Replace("{Item}", itemValue.ItemClass.GetLocalizedItemName() ?? itemValue.ItemClass.Name);
            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            Wallet.SubtractCoinsFromWallet(_cInfo.playerId, _price);
        }
    }
}
