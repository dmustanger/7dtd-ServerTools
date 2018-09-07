using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    class Shop
    {
        public static bool IsEnabled = false, IsRunning = false, Anywhere = false, Inside_Market = false;
        public static int Delay_Between_Uses = 60;
        private const string file = "Market.xml";
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
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Market' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("item"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of missing item attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("name"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of missing name attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("secondaryname"))
                        {
                            updateConfig = true;
                        }
                        if (!_line.HasAttribute("count"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of missing count attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("quality"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of missing quality attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("price"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of missing price attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("category"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Market entry because of missing category attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        int _item = 1;
                        int _count = 1;
                        int _quality = 1;
                        int _price = 1;
                        if (!int.TryParse(_line.GetAttribute("item"), out _item))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring Market entry because of invalid (non-numeric) value for 'item' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("count"), out _count))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring Market entry because of invalid (non-numeric) value for 'count' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("quality"), out _quality))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring Market entry because of invalid (non-numeric) value for 'qualityMin' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("price"), out _price))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring Market Item entry because of invalid (non-numeric) value for 'price' attribute: {0}", subChild.OuterXml));
                            continue;
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
                        ItemValue _itemValue = ItemClass.GetItem(_name, true);
                        if (_itemValue.type == ItemValue.None.type)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Market item not found: {0}", _name));
                            continue;
                        }
                        string _category = _line.GetAttribute("category");
                        if (!categories.Contains(_category))
                        {
                            categories.Add(_category);
                        }
                        if (!dict.ContainsKey(_item))
                        {
                            string[] _n = new string[] { _name, _secondaryname, _category };
                            dict.Add(_item, _n);
                        }
                        if (!dict1.ContainsKey(_item))
                        {
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
                sw.WriteLine("<Market>");
                sw.WriteLine("    <items>");
                if (dict.Count > 0)
                {
                    foreach (KeyValuePair<int, string[]> kvp in dict)
                    {
                        int[] _values;
                        if (dict1.TryGetValue(kvp.Key, out _values))
                        {
                            sw.WriteLine(string.Format("        <market item=\"{0}\" name=\"{1}\" secondaryname=\"{2}\" count=\"{3}\" quality=\"{4}\" price=\"{5}\" category=\"{6}\" />", kvp.Key, kvp.Value[0], kvp.Value[1], _values[0], _values[1], _values[2], kvp.Value[2]));
                        }
                    }
                }
                else
                {
                    sw.WriteLine("        <market item=\"1\" name=\"bottledWater\" secondaryName=\"Bottled Water\" count=\"1\" quality=\"1\" price=\"20\" category=\"food\" />");
                    sw.WriteLine("        <market item=\"2\" name=\"beer\" secondaryName=\"Beer\" count=\"1\" quality=\"1\" price=\"50\" category=\"food\" />");
                    sw.WriteLine("        <market item=\"3\" name=\"wood\" secondaryName=\"Wood\" count=\"10\" quality=\"1\" price=\"30\" category=\"mats\" />");
                    sw.WriteLine("        <market item=\"4\" name=\"woodFrameBlock\" secondaryName=\"Wood Frame Block\" count=\"1\" quality=\"1\" price=\"20\" category=\"mats\" />");
                    sw.WriteLine("        <market item=\"5\" name=\"keystoneBlock\" secondaryName=\"Land Claim Block\" count=\"1\" quality=\"1\" price=\"2500\" category=\"extra\" />");
                    sw.WriteLine("        <market item=\"6\" name=\"canChicken\" secondaryName=\"Can of Chicken\" count=\"1\" quality=\"1\" price=\"50\" category=\"food\" />");
                    sw.WriteLine("        <market item=\"7\" name=\"canChili\" secondaryName=\"Can of Chilli\" count=\"1\" quality=\"1\" price=\"50\" category=\"food\" />");
                    sw.WriteLine("        <market item=\"8\" name=\"corn\" secondaryName=\"Corn\" count=\"5\" quality=\"1\" price=\"50\" category=\"food\" />");
                    sw.WriteLine("        <market item=\"9\" name=\"potato\" secondaryName=\"Potato\" count=\"5\" quality=\"1\" price=\"50\" category=\"food\" />");
                    sw.WriteLine("        <market item=\"10\" name=\"firstAidBandage\" secondaryName=\"First Aid Bandage\" count=\"1\" quality=\"1\" price=\"50\" category=\"meds\" />");
                    sw.WriteLine("        <market item=\"11\" name=\"painkillers\" secondaryName=\"Pain Killers\" count=\"5\" quality=\"1\" price=\"150\" category=\"meds\" />");
                    sw.WriteLine("        <market item=\"12\" name=\"gunPistol\" secondaryName=\"Pistol\" count=\"1\" quality=\"100\" price=\"500\" category=\"weapon\" />");
                    sw.WriteLine("        <market item=\"13\" name=\"casinoCoin\" secondaryName=\"casinoCoin\" count=\"1\" quality=\"1\" price=\"1\" category=\"extra\" />");
                }
                sw.WriteLine("    </items>");
                sw.WriteLine("</Market>");
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
                if (!Anywhere)
                {
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    World world = GameManager.Instance.World;
                    Vector3i playerPos = new Vector3i((int)_player.position.x, (int)_player.position.y, (int)_player.position.z);
                    if (world.IsWithinTraderArea(playerPos))
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
                    else
                    {
                        string _phrase619;
                        if (!Phrases.Dict.TryGetValue(619, out _phrase619))
                        {
                            _phrase619 = "{PlayerName} you are not inside a trader area. Find a trader and use this command again.";
                        }
                        _phrase619 = _phrase619.Replace("{PlayerName}", _playerName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase619), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
                else
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
            }
            else
            {
                string _phrase624;
                if (!Phrases.Dict.TryGetValue(624, out _phrase624))
                {
                    _phrase624 = "The shop does not contain any items. Contact an administrator";
                }
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase624), Config.Server_Response_Name, false, "ServerTools", false));
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
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase617), Config.Server_Response_Name, false, "ServerTools", false));
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _categories), Config.Server_Response_Name, false, "ServerTools", false));
            string _phrase618;
            if (!Phrases.Dict.TryGetValue(618, out _phrase618))
            {
                _phrase618 = "Type /shop {category} to view that list.";
            }
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} {2}[-]", Config.Chat_Response_Color, _playerName, _phrase618), Config.Server_Response_Name, false, "ServerTools", false));
        }

        public static void ShowCategory(ClientInfo _cInfo, string _playerName, string _category)
        {
            if (categories.Contains(_category))
            {
                for (int i = 0; i < dict.Count; i++)
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
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}# {1}: {2} {3} {4} quality for {5} {6}[-]", Config.Chat_Response_Color, i, _dict1Values[0], _dictValues[0], _dict1Values[1], _dict1Values[2], Wallet.Coin_Name), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                                else
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}# {1}: {2} {3} for {4} {5}[-]", Config.Chat_Response_Color, i, _dict1Values[0], _dictValues[0], _dict1Values[2], Wallet.Coin_Name), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                            }
                        }
                    }
                }
                string _phrase823;
                if (!Phrases.Dict.TryGetValue(823, out _phrase823))
                {
                    _phrase823 = "Type /buy # to purchase the shop item. You can add how many times you want to buy it. /buy # #";
                }
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} {2}[-]", Config.Chat_Response_Color, _playerName, _phrase823), Config.Server_Response_Name, false, "ServerTools", false));
            }
            else
            {
                string _phrase822;
                if (!Phrases.Dict.TryGetValue(822, out _phrase822))
                {
                    _phrase822 = "This category is missing. Check /shop.";
                }
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} {2}[-]", Config.Chat_Response_Color, _playerName, _phrase822), Config.Server_Response_Name, false, "ServerTools", false));
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
                        _phrase620 = "{PlayerName} the item or amount # you are trying to buy is not an integer. Please input /buy 1 2 for example.";
                    }
                    _phrase620 = _phrase620.Replace("{PlayerName}", _playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase620), Config.Server_Response_Name, false, "ServerTools", false));
                }
                else
                {
                    int _amount;
                    if (!int.TryParse(_idAmount[1], out _amount))
                    {
                        string _phrase620;
                        if (!Phrases.Dict.TryGetValue(620, out _phrase620))
                        {
                            _phrase620 = "{PlayerName} the item or amount # you are trying to buy is not an integer. Please input /buy 1 2 for example.";
                        }
                        _phrase620 = _phrase620.Replace("{PlayerName}", _playerName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase620), Config.Server_Response_Name, false, "ServerTools", false));
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
                                            _phrase621 = "{PlayerName} you do not have enough {CoinName}. Your wallet balance is {WalletBalance}.";
                                        }
                                        _phrase621 = _phrase621.Replace("{PlayerName}", _playerName);
                                        _phrase621 = _phrase621.Replace("{CoinName}", Wallet.Coin_Name);
                                        _phrase621 = _phrase621.Replace("{WalletBalance}", _currentCoins.ToString());
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase621), Config.Server_Response_Name, false, "ServerTools", false));
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
                                        _phrase621 = "{PlayerName} you do not have enough {CoinName}. Your wallet balance is {WalletBalance}.";
                                    }
                                    _phrase621 = _phrase621.Replace("{PlayerName}", _playerName);
                                    _phrase621 = _phrase621.Replace("{CoinName}", Wallet.Coin_Name);
                                    _phrase621 = _phrase621.Replace("{WalletBalance}", _currentCoins.ToString());
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase621), Config.Server_Response_Name, false, "ServerTools", false));
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
                        _phrase622 = "{PlayerName} there was no item # matching the shop. Check the shop category again.";
                    }
                    _phrase622 = _phrase622.Replace("{PlayerName}", _playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase622), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
        }

        public static void ShopPurchase(ClientInfo _cInfo, string _itemName, int _count, int _quality, int _price, string _playerName, int currentCoins)
        {
            World world = GameManager.Instance.World;
            ItemValue _itemValue = ItemClass.GetItem(_itemName, true);
            if (_itemValue.type != ItemValue.None.type)
            {
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
                SdtdConsole.Instance.Output(string.Format("Sold {0} to {1}.", itemValue.ItemClass.localizedName ?? itemValue.ItemClass.Name, _cInfo.playerName));
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} {2} was purchased through the shop. If your bag is full, check the ground.[-]", Config.Chat_Response_Color, _count, itemValue.ItemClass.localizedName ?? itemValue.ItemClass.Name), Config.Server_Response_Name, false, "ServerTools", false));
                Wallet.SubtractCoinsFromWallet(_cInfo.playerId, _price);
            }
            else
            {
                string _phrase623;
                if (!Phrases.Dict.TryGetValue(623, out _phrase623))
                {
                    _phrase623 = "{PlayerName} there was an error in the shop list. Unable to buy this item. Please alert an administrator.";
                    Log.Out(string.Format("Player {0} tried to buy item {1} from the shop. The item name in the Market.xml does not match an existing item. Check your Item.xml for the correct item name. It is case sensitive.", _cInfo.playerName, _itemName));
                }
                _phrase623 = _phrase623.Replace("{PlayerName}", _playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase623), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }
    }
}
