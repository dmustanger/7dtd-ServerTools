using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    class Shop
    {
        public static bool IsEnabled = false, IsRunning = false, Anywhere = false;
        public static int Delay_Between_Uses = 60;
        private const string file = "Market.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static SortedDictionary<int, string[]> dict = new SortedDictionary<int, string[]>();
        private static SortedDictionary<int, int[]> dict1 = new SortedDictionary<int, int[]>();
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
            fileWatcher.Dispose();
            IsRunning = false;
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
                        if (!dict.ContainsKey(_item))
                        {
                            string[] _n = new string[] { _name, _secondaryname };
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
                            sw.WriteLine(string.Format("        <market item=\"{0}\" name=\"{1}\" secondaryname=\"{2}\" count=\"{3}\" quality=\"{4}\" price=\"{5}\" />", kvp.Key, kvp.Value[0], kvp.Value[1], _values[0], _values[1], _values[2]));
                        }
                    }
                }
                else
                {
                    sw.WriteLine("        <market item=\"1\" name=\"bottledWater\" secondaryName=\"Bottled Water\" count=\"1\" quality=\"1\" price=\"20\" />");
                    sw.WriteLine("        <market item=\"2\" name=\"beer\" secondaryName=\"Beer\" count=\"1\" quality=\"1\" price=\"50\" />");
                    sw.WriteLine("        <market item=\"3\" name=\"wood\" secondaryName=\"Wood\" count=\"10\" quality=\"1\" price=\"30\" />");
                    sw.WriteLine("        <market item=\"4\" name=\"woodFrameBlock\" secondaryName=\"Wood Frame Block\" count=\"1\" quality=\"1\" price=\"20\" />");
                    sw.WriteLine("        <market item=\"5\" name=\"keystoneBlock\" secondaryName=\"Land Claim Block\" count=\"1\" quality=\"1\" price=\"2500\" />");
                    sw.WriteLine("        <market item=\"6\" name=\"canChicken\" secondaryName=\"Can of Chicken\" count=\"1\" quality=\"1\" price=\"50\" />");
                    sw.WriteLine("        <market item=\"7\" name=\"canChili\" secondaryName=\"Can of Chilli\" count=\"1\" quality=\"1\" price=\"50\" />");
                    sw.WriteLine("        <market item=\"8\" name=\"corn\" secondaryName=\"Corn\" count=\"5\" quality=\"1\" price=\"50\" />");
                    sw.WriteLine("        <market item=\"9\" name=\"potato\" secondaryName=\"Potato\" count=\"5\" quality=\"1\" price=\"50\" />");
                    sw.WriteLine("        <market item=\"10\" name=\"firstAidBandage\" secondaryName=\"First Aid Bandage\" count=\"1\" quality=\"1\" price=\"50\" />");
                    sw.WriteLine("        <market item=\"11\" name=\"painkillers\" secondaryName=\"Pain Killers\" count=\"5\" quality=\"1\" price=\"150\" />");
                    sw.WriteLine("        <market item=\"12\" name=\"gunPistol\" secondaryName=\"Pistol\" count=\"1\" quality=\"100\" price=\"500\" />");
                    sw.WriteLine("        <market item=\"13\" name=\"casinoCoin\" secondaryName=\"casinoCoin\" count=\"1\" quality=\"1\" price=\"1\" />");
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

        public static void List(ClientInfo _cInfo, string _playerName)
        {
            if (!Anywhere)
            {
                World world = GameManager.Instance.World;
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                int x = (int)_player.position.x;
                int y = (int)_player.position.y;
                int z = (int)_player.position.z;
                Vector3i playerPos = new Vector3i(x, y, z);
                if (world.IsWithinTraderArea(playerPos))
                {
                    Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                    if (p == null)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].PlayerSpentCoins = 0;
                        PersistentContainer.Instance.Save();
                    }
                    else
                    {
                        int spentCoins = p.PlayerSpentCoins;
                        int currentCoins = 0;
                        int gameMode = world.GetGameMode();
                        if (gameMode == 7)
                        {
                            currentCoins = (_player.KilledZombies * Wallet.Zombie_Kills) + (_player.KilledPlayers * Wallet.Player_Kills) - (XUiM_Player.GetDeaths(_player) * Wallet.Deaths) + p.PlayerSpentCoins;
                        }
                        else
                        {
                            currentCoins = (_player.KilledZombies * Wallet.Zombie_Kills) - (XUiM_Player.GetDeaths(_player) * Wallet.Deaths) + p.PlayerSpentCoins;
                        }
                        if (!Wallet.Negative_Wallet)
                        {
                            if (currentCoins < 0)
                            {
                                currentCoins = 0;
                            }
                        }
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} your wallet contains: {2} {3}.[-]", Config.Chat_Response_Color, _cInfo.playerName, currentCoins, Wallet.Coin_Name), "Server", false, "", false));
                        string _phrase617;
                        if (!Phrases.Dict.TryGetValue(617, out _phrase617))
                        {
                            _phrase617 = "The shop contains the following:";
                        }
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase617), "Server", false, "", false));
                        foreach (var _sellable in dict)
                        {
                            int[] _values;
                            if (dict1.TryGetValue(_sellable.Key, out _values))
                            {
                                if (_values[1] > 1)
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}# {1}: {2} {3} {4} quality for {5} {6}[-]", Config.Chat_Response_Color, _sellable.Key, _values[0], _sellable.Value[1], _values[1], _values[2], Wallet.Coin_Name), "Server", false, "", false));
                                }
                                else
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}# {1}: {2} {3} for {4} {5}[-]", Config.Chat_Response_Color, _sellable.Key, _values[0], _sellable.Value[1], _values[2], Wallet.Coin_Name), "Server", false, "", false));
                                }
                            }
                        }
                        string _phrase618;
                        if (!Phrases.Dict.TryGetValue(618, out _phrase618))
                        {
                            _phrase618 = "Type /buy # to purchase the corresponding value from the shop list.";
                        }
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase618), "Server", false, "", false));
                    }
                }
                else
                {
                    string _phrase619;
                    if (!Phrases.Dict.TryGetValue(619, out _phrase619))
                    {
                        _phrase619 = "{PlayerName} you are not inside a trade area. Find a trader and use /shop again.";
                    }
                    _phrase619 = _phrase619.Replace("{PlayerName}", _playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase619), "Server", false, "", false));
                }
            }
            else
            {
                World world = GameManager.Instance.World;
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                if (p == null)
                {
                    PersistentContainer.Instance.Players[_cInfo.playerId, true].PlayerSpentCoins = 0;
                    PersistentContainer.Instance.Save();
                }
                else
                {
                    int spentCoins = p.PlayerSpentCoins;
                    int currentCoins = 0;
                    int gameMode = world.GetGameMode();
                    if (gameMode == 7)
                    {
                        currentCoins = (_player.KilledZombies * Wallet.Zombie_Kills) + (_player.KilledPlayers * Wallet.Player_Kills) - (XUiM_Player.GetDeaths(_player) * Wallet.Deaths) + p.PlayerSpentCoins;
                    }
                    else
                    {
                        currentCoins = (_player.KilledZombies * Wallet.Zombie_Kills) - (XUiM_Player.GetDeaths(_player) * Wallet.Deaths) + p.PlayerSpentCoins;
                    }
                    if (!Wallet.Negative_Wallet)
                    {
                        if (currentCoins < 0)
                        {
                            currentCoins = 0;
                        }
                    }
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} your wallet contains: {2} {3}.[-]", Config.Chat_Response_Color, _cInfo.playerName, currentCoins, Wallet.Coin_Name), "Server", false, "", false));
                }
                string _phrase617;
                if (!Phrases.Dict.TryGetValue(617, out _phrase617))
                {
                    _phrase617 = "The shop contains the following:";
                }
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase617), "Server", false, "", false));
                foreach (var _sellable in dict)
                {
                    int[] _values;
                    if (dict1.TryGetValue(_sellable.Key, out _values))
                    {
                        if (_values[1] > 1)
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}#{1}: {2} {3} {4} quality for {5} {6}[-]", Config.Chat_Response_Color, _sellable.Key, _values[0], _sellable.Value[1], _values[1], _values[2], Wallet.Coin_Name), "Server", false, "", false));
                        }
                        else
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}#{1}: {2} {3} for {4} {5}[-]", Config.Chat_Response_Color, _sellable.Key, _values[0], _sellable.Value[1], _values[2], Wallet.Coin_Name), "Server", false, "", false));
                        }
                    }
                }
                string _phrase618;
                if (!Phrases.Dict.TryGetValue(618, out _phrase618))
                {
                    _phrase618 = "Type /buy # to purchase the corresponding value from the shop list.";
                }
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase618), "Server", false, "", false));
            }
        }

        public static void Walletcheck(ClientInfo _cInfo, string _item, string _playerName)
        {
            int _id;
            if (!int.TryParse(_item, out _id))
            {
                string _phrase620;
                if (!Phrases.Dict.TryGetValue(620, out _phrase620))
                {
                    _phrase620 = "{PlayerName} the item # you are trying to buy is not an interger. Please input /buy 1 for example.";
                }
                _phrase620 = _phrase620.Replace("{PlayerName}", _playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase620), "Server", false, "", false));
            }
            else
            {
                if (dict.ContainsKey(_id))
                {
                    string[] _stringValues;
                    if (dict.TryGetValue(_id, out _stringValues))
                    {
                        int[] _intergerValues;
                        if (dict1.TryGetValue(_id, out _intergerValues))
                        {
                            World world = GameManager.Instance.World;
                            int currentCoins = 0;
                            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                            int gameMode = world.GetGameMode();
                            if (gameMode == 7)
                            {
                                currentCoins = (_player.KilledZombies * 10) + (_player.KilledPlayers * 50) - (XUiM_Player.GetDeaths(_player) * -25) + p.PlayerSpentCoins;
                            }
                            else
                            {
                                currentCoins = (_player.KilledZombies * 10) - (XUiM_Player.GetDeaths(_player) * -25) + p.PlayerSpentCoins;
                            }
                            if (!Wallet.Negative_Wallet)
                            {
                                if (currentCoins < 0)
                                {
                                    currentCoins = 0;
                                }
                            }
                            if (currentCoins >= _intergerValues[2])
                            {
                                ShopPurchase(_cInfo, _stringValues[0], _intergerValues[0], _intergerValues[1], _intergerValues[2], _playerName, currentCoins, p);
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
                                _phrase621 = _phrase621.Replace("{WalletBalance}", currentCoins.ToString());
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase621), "Server", false, "", false));
                            }
                        }
                    }
                }
                else
                {
                    string _phrase622;
                    if (!Phrases.Dict.TryGetValue(622, out _phrase622))
                    {
                        _phrase622 = "{PlayerName} there was no item # matching the shop goods. Type /shop to review the list.";
                    }
                    _phrase622 = _phrase622.Replace("{PlayerName}", _playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase622), "Server", false, "", false));
                }
            }
        }

        public static void ShopPurchase(ClientInfo _cInfo, string _itemName, int _count, int _quality, int _price, string _playerName, int currentCoins, Player p)
        {
            if (!Anywhere)
            {
                World world = GameManager.Instance.World;
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                int x = (int)_player.position.x;
                int y = (int)_player.position.y;
                int z = (int)_player.position.z;
                Vector3i playerPos = new Vector3i(x, y, z);
                if (world.IsWithinTraderArea(playerPos))
                {
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
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} {2} was purchased through the shop. If your bag is full, check the ground.[-]", Config.Chat_Response_Color, _count, itemValue.ItemClass.localizedName ?? itemValue.ItemClass.Name), "Server", false, "", false));
                        int newCoins = p.PlayerSpentCoins - _price;
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].PlayerSpentCoins = newCoins;
                        PersistentContainer.Instance.Save();
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
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase623), "Server", false, "", false));
                    }
                }
                else
                {
                    string _phrase624;
                    if (!Phrases.Dict.TryGetValue(624, out _phrase624))
                    {
                        _phrase624 = "{PlayerName} you are not inside a trade area. Find a trader and use /buy again.";
                    }
                    _phrase624 = _phrase624.Replace("{PlayerName}", _playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase624), "Server", false, "", false));
                }
            }
            else
            {
                World world = GameManager.Instance.World;
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
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
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} {2} was purchased through the shop. If your bag is full, check the ground.[-]", Config.Chat_Response_Color, _count, itemValue.ItemClass.localizedName ?? itemValue.ItemClass.Name), "Server", false, "", false));
                    int newCoins = p.PlayerSpentCoins - _price;
                    PersistentContainer.Instance.Players[_cInfo.playerId, true].PlayerSpentCoins = newCoins;
                    PersistentContainer.Instance.Save();
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
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase623), "Server", false, "", false));
                }
            }
        }
    }
}
