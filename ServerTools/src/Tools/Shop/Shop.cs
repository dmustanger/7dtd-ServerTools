using System;
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
        public static string Command_shop = "shop", Command_shop_buy = "shop buy";
        public static SortedDictionary<int, string[]> Dict = new SortedDictionary<int, string[]>();
        public static SortedDictionary<int, int[]> Dict1 = new SortedDictionary<int, int[]>();
        public static List<string> Categories = new List<string>();

        private const string file = "Shop.xml";
        private static string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        public static void Unload()
        {
            Dict.Clear();
            Dict1.Clear();
            Categories.Clear();
            FileWatcher.Dispose();
            IsRunning = false;
        }

        public static void LoadXml()
        {
            try
            {
                if (!Utils.FileExists(FilePath))
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
                XmlNodeList _childNodes = xmlDoc.DocumentElement.ChildNodes;
                if (_childNodes != null && _childNodes.Count > 0)
                {
                    Dict.Clear();
                    Dict1.Clear();
                    Categories.Clear();
                    bool upgrade = true;
                    for (int i = 0; i < _childNodes.Count; i++)
                    {
                        if (_childNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement _line = (XmlElement)_childNodes[i];
                        if (_line.HasAttributes)
                        {
                            if (_line.HasAttribute("Version") && _line.GetAttribute("Version") == Config.Version)
                            {
                                upgrade = false;
                            }
                            else if (_line.HasAttribute("Name") && _line.HasAttribute("Count") && _line.HasAttribute("Quality") && 
                                _line.HasAttribute("Price") && _line.HasAttribute("Category"))
                            {
                                if (!int.TryParse(_line.GetAttribute("Count"), out int _count))
                                {
                                    Log.Out(string.Format("[SERVERTOOLS] Ignoring Shop.xml entry. Invalid (non-numeric) value for 'Count' attribute: {0}", _line.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Quality"), out int _quality))
                                {
                                    Log.Out(string.Format("[SERVERTOOLS] Ignoring Shop.xml entry. Invalid (non-numeric) value for 'Quality' attribute: {0}", _line.OuterXml));
                                    continue;
                                }
                                if (!int.TryParse(_line.GetAttribute("Price"), out int _price))
                                {
                                    Log.Out(string.Format("[SERVERTOOLS] Ignoring Shop.xml entry. Invalid (non-numeric) value for 'Price' attribute: {0}", _line.OuterXml));
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
                                    Log.Out(string.Format("[SERVERTOOLS] Ignoring Shop.xml entry. Item could not be found: {0}", _name));
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
                                int _id = Dict.Count + 1;
                                if (!Dict.ContainsKey(_id))
                                {
                                    string[] _strings = new string[] { _name, _secondaryname, _category };
                                    Dict.Add(_id, _strings);
                                    int[] _integers = new int[] { _count, _quality, _price };
                                    Dict1.Add(_id, _integers);
                                }
                            }
                        }
                    }
                    if (upgrade)
                    {
                        UpgradeXml(_childNodes);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Shop.LoadXml: {0}", e.Message));
            }
        }

        public static void UpdateXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<Shop>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine();
                    sw.WriteLine();
                    if (Dict.Count > 0)
                    {
                        foreach (KeyValuePair<int, string[]> kvp in Dict)
                        {
                            if (Dict1.TryGetValue(kvp.Key, out int[] _values))
                            {
                                sw.WriteLine(string.Format("    <Item Name=\"{0}\" SecondaryName=\"{1}\" Count=\"{2}\" Quality=\"{3}\" Price=\"{4}\" Category=\"{5}\" />", kvp.Value[0], kvp.Value[1], _values[0], _values[1], _values[2], kvp.Value[2]));
                            }
                        }
                    }
                    else
                    {
                        sw.WriteLine("    <Item Name=\"drinkJarBoiledWater\" SecondaryName=\"Bottled Water\" Count=\"1\" Quality=\"1\" Price=\"20\" Category=\"Food\" />");
                        sw.WriteLine("    <Item Name=\"casinoCoin\" SecondaryName=\"Dukes\" Count=\"1\" Quality=\"1\" Price=\"1\" Category=\"Extra\" />");
                    }
                    sw.WriteLine("</Shop>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Shop.UpdateXml: {0}", e.Message));
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
            if (!Utils.FileExists(FilePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        public static void PosCheck(ClientInfo _cInfo, string _categoryOrItem, int _form, int _count)
        {
            try
            {
                if (Dict.Count > 0)
                {
                    EntityPlayer _player = PersistentOperations.GetEntityPlayer(_cInfo.playerId);
                    if (_player != null)
                    {
                        if (Inside_Market && Inside_Traders)
                        {
                            string[] _cords = Market.Market_Position.Split(',');
                            int.TryParse(_cords[0], out int x);
                            int.TryParse(_cords[1], out int y);
                            int.TryParse(_cords[2], out int z);
                            if ((x - _player.position.x) * (x - _player.position.x) + (z - _player.position.z) * (z - _player.position.z) <= Market.Market_Size * Market.Market_Size && GameManager.Instance.World.IsWithinTraderArea(new Vector3i(_player.position.x, _player.position.y, _player.position.z)))
                            {
                                PosCheck2(_cInfo, _categoryOrItem, _form, _count);
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("Shop9", out string _phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        else if (Inside_Market && !Inside_Traders)
                        {
                            string[] _cords = Market.Market_Position.Split(',');
                            int.TryParse(_cords[0], out int x);
                            int.TryParse(_cords[1], out int y);
                            int.TryParse(_cords[2], out int z);
                            if ((x - _player.position.x) * (x - _player.position.x) + (z - _player.position.z) * (z - _player.position.z) <= Market.Market_Size * Market.Market_Size)
                            {
                                PosCheck2(_cInfo, _categoryOrItem, _form, _count);
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("Shop10", out string _phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                                Phrases.Dict.TryGetValue("Shop3", out string _phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                    Phrases.Dict.TryGetValue("Shop8", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Shop.PosCheck: {0}", e.Message));
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
            try
            {
                Phrases.Dict.TryGetValue("Shop1", out string _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                string _categories = "";
                if (Categories.Count > 1)
                {
                    _categories = string.Join(", ", Categories.ToArray());
                }
                else
                {
                    _categories = Categories[0];
                }
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _categories + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                Phrases.Dict.TryGetValue("Shop2", out _phrase);
                _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                _phrase = _phrase.Replace("{Command_shop}", Command_shop);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Shop.ListCategories: {0}", e.Message));
            }
        }

        public static void ShowCategory(ClientInfo _cInfo, string _category)
        {
            try
            {
                if (Categories.Contains(_category))
                {
                    for (int i = 0; i < Dict.Count; i++)
                    {
                        if (Dict.TryGetValue(i, out string[] _dictValues))
                        {
                            if (_dictValues[2] == _category)
                            {
                                if (Dict1.TryGetValue(i, out int[] _dict1Values))
                                {
                                    if (_dict1Values[1] > 1)
                                    {
                                        Phrases.Dict.TryGetValue("Shop11", out string _phrase);
                                        _phrase = _phrase.Replace("{Id}", i + 1.ToString());
                                        _phrase = _phrase.Replace("{Count}", _dict1Values[0].ToString());
                                        _phrase = _phrase.Replace("{Item}", _dictValues[0]);
                                        _phrase = _phrase.Replace("{Quality}", _dict1Values[1].ToString());
                                        _phrase = _phrase.Replace("{Price}", _dict1Values[2].ToString());
                                        _phrase = _phrase.Replace("{Name}", Wallet.Coin_Name);
                                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                        return;
                                    }
                                    else
                                    {
                                        Phrases.Dict.TryGetValue("Shop12", out string _phrase);
                                        _phrase = _phrase.Replace("{Id}", i + 1.ToString());
                                        _phrase = _phrase.Replace("{Count}", _dict1Values[0].ToString());
                                        _phrase = _phrase.Replace("{Item}", _dictValues[0]);
                                        _phrase = _phrase.Replace("{Price}", _dict1Values[2].ToString());
                                        _phrase = _phrase.Replace("{Name}", Wallet.Coin_Name);
                                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                        return;
                                    }
                                }
                            }
                        }
                        Phrases.Dict.TryGetValue("Shop13", out string _phrase1);
                        _phrase1 = _phrase1.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                        _phrase1 = _phrase1.Replace("{Command_shop_buy}", Command_shop_buy);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Shop14", out string _phrase);
                    _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    _phrase = _phrase.Replace("{Command_shop}", Command_shop);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Shop.ShowCategory: {0}", e.Message));
            }
        }

        public static void Walletcheck(ClientInfo _cInfo, int _item, int _count)
        {
            try
            {
                if (Dict.ContainsKey(_item - 1))
                {
                    if (Dict.TryGetValue(_item - 1, out string[] _stringValues))
                    {
                        if (Dict1.TryGetValue(_item - 1, out int[] _integerValues))
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
                                Phrases.Dict.TryGetValue("Shop5", out string _phrase);
                                _phrase = _phrase.Replace("{CoinName}", Wallet.Coin_Name);
                                _phrase = _phrase.Replace("{Value}", _currentCoins.ToString());
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Shop15", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Shop.Walletcheck: {0}", e.Message));
            }
        }

        public static void ShopPurchase(ClientInfo _cInfo, string _itemName, string _secondaryName, int _count, int _quality, int _price)
        {
            try
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
                    Phrases.Dict.TryGetValue("Shop16", out string _phrase);
                    _phrase = _phrase.Replace("{Count}", _count.ToString());
                    if (_secondaryName != "")
                    {
                        _phrase = _phrase.Replace("{Item}", _secondaryName);
                    }
                    else
                    {
                        _phrase = _phrase.Replace("{Item}", _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.GetItemName());
                    }
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    Phrases.Dict.TryGetValue("Shop17", out string _phrase);
                    _phrase = _phrase.Replace("{Value}", _maxAllowed.ToString());
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Shop.ShopPurchase: {0}", e.Message));
            }
        }

        private static void UpgradeXml(XmlNodeList _oldChildNodes)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<Shop>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine();
                    sw.WriteLine();
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement _line = (XmlElement)_oldChildNodes[i];
                        if (_line.HasAttributes && (_line.Name == "Shop" || _line.Name == "Item"))
                        {
                            string _name = "", _secondaryName = "", _count = "", _quality = "", _price = "", _category = "";
                            if (_line.HasAttribute("Name"))
                            {
                                _name = _line.GetAttribute("Name");
                            }
                            if (_line.HasAttribute("SecondaryName"))
                            {
                                _secondaryName = _line.GetAttribute("SecondaryName");
                            }
                            if (_line.HasAttribute("Count"))
                            {
                                _count = _line.GetAttribute("Count");
                            }
                            if (_line.HasAttribute("Quality"))
                            {
                                _quality = _line.GetAttribute("Quality");
                            }
                            if (_line.HasAttribute("Price"))
                            {
                                _price = _line.GetAttribute("Price");
                            }
                            if (_line.HasAttribute("Category"))
                            {
                                _category = _line.GetAttribute("Category");
                            }
                            sw.WriteLine(string.Format("    <Item Name=\"{0}\" SecondaryName=\"{1}\" Count=\"{2}\" Quality=\"{3}\" Price=\"{4}\" Category=\"{5}\" />", _name, _secondaryName, _count, _quality, _price, _category));
                        }
                    }
                    sw.WriteLine("</Shop>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Shop.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}