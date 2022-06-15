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

        public static List<string[]> Dict = new List<string[]>();
        public static List<string> Categories = new List<string>();

        private const string file = "Shop.xml";
        private static string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        private static XmlNodeList OldNodeList;

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        public static void Unload()
        {
            Dict.Clear();
            Categories.Clear();
            FileWatcher.Dispose();
            IsRunning = false;
        }

        public static void LoadXml()
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
                bool upgrade = true;
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                if (childNodes != null)
                {
                    Dict.Clear();
                    Categories.Clear();
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        if (childNodes[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)childNodes[i];
                            if (line.HasAttributes)
                            {
                                if (line.HasAttribute("Version") && line.GetAttribute("Version") == Config.Version)
                                {
                                    upgrade = false;
                                    continue;
                                }
                                else if (line.HasAttribute("Name") && line.HasAttribute("Count") && line.HasAttribute("Quality") &&
                                    line.HasAttribute("Price") && line.HasAttribute("Category"))
                                {
                                    if (!int.TryParse(line.GetAttribute("Count"), out int count))
                                    {
                                        Log.Out(string.Format("[SERVERTOOLS] Ignoring Shop.xml entry. Invalid (non-numeric) value for 'Count' attribute: {0}", line.OuterXml));
                                        continue;
                                    }
                                    if (!int.TryParse(line.GetAttribute("Quality"), out int quality))
                                    {
                                        Log.Out(string.Format("[SERVERTOOLS] Ignoring Shop.xml entry. Invalid (non-numeric) value for 'Quality' attribute: {0}", line.OuterXml));
                                        continue;
                                    }
                                    if (!int.TryParse(line.GetAttribute("Price"), out int price))
                                    {
                                        Log.Out(string.Format("[SERVERTOOLS] Ignoring Shop.xml entry. Invalid (non-numeric) value for 'Price' attribute: {0}", line.OuterXml));
                                        continue;
                                    }
                                    string name = line.GetAttribute("Name");
                                    string secondaryname;
                                    if (line.HasAttribute("SecondaryName"))
                                    {
                                        secondaryname = line.GetAttribute("SecondaryName");
                                    }
                                    else
                                    {
                                        secondaryname = name;
                                    }
                                    ItemValue itemValue = ItemClass.GetItem(name, false);
                                    if (itemValue.type == ItemValue.None.type)
                                    {
                                        Log.Out(string.Format("[SERVERTOOLS] Ignoring Shop.xml entry. Item could not be found: {0}", name));
                                        continue;
                                    }
                                    if (count > itemValue.ItemClass.Stacknumber.Value)
                                    {
                                        count = itemValue.ItemClass.Stacknumber.Value;
                                    }
                                    if (quality < 1)
                                    {
                                        quality = 1;
                                    }
                                    else if (quality > 600)
                                    {
                                        quality = 600;
                                    }
                                    string category = line.GetAttribute("Category").ToLower();
                                    int id = Dict.Count + 1;
                                    string[] item = new string[] { id.ToString(), name, secondaryname, count.ToString(), quality.ToString(), price.ToString(), category };
                                    if (!Dict.Contains(item))
                                    {
                                        if (!Categories.Contains(category))
                                        {
                                            Categories.Add(category);
                                        }
                                        Dict.Add(item);
                                    }
                                }
                            }
                        }
                    }
                }
                if (upgrade)
                {
                    XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                    XmlNode node = nodeList[0];
                    XmlElement line = (XmlElement)nodeList[0];
                    if (line != null)
                    {
                        if (line.HasAttributes)
                        {
                            OldNodeList = nodeList;
                            File.Delete(FilePath);
                            UpgradeXml();
                            return;
                        }
                        else
                        {
                            nodeList = node.ChildNodes;
                            line = (XmlElement)nodeList[0];
                            if (line != null)
                            {
                                if (line.HasAttributes)
                                {
                                    OldNodeList = nodeList;
                                    File.Delete(FilePath);
                                    UpgradeXml();
                                    return;
                                }
                            }
                            File.Delete(FilePath);
                            UpdateXml();
                            Log.Out(string.Format("[SERVERTOOLS] The existing Shop.xml was too old or misconfigured. File deleted and rebuilt for version {0}", Config.Version));
                        }
                    }
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
                    Log.Out(string.Format("[SERVERTOOLS] Error in Shop.LoadXml: {0}", e.Message));
                }
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
                    sw.WriteLine("    <!-- Secondary name is what will show in chat instead of the item name -->");
                    sw.WriteLine("    <!-- <Item Name=\"\" SecondaryName=\"\" Count=\"\" Quality=\"\" Price=\"\" Category=\"\" /> -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    if (Dict.Count > 0)
                    {
                        for (int i = 0; i < Dict.Count; i++)
                        {
                            sw.WriteLine(string.Format("    <Item Name=\"{0}\" SecondaryName=\"{1}\" Count=\"{2}\" Quality=\"{3}\" Price=\"{4}\" Category=\"{5}\" />", Dict[i][1], Dict[i][2], Dict[i][3], Dict[i][4], Dict[i][5], Dict[i][6]));
                        }
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
            if (!File.Exists(FilePath))
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
                    EntityPlayer _player = PersistentOperations.GetEntityPlayer(_cInfo.entityId);
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
                                FormCheck(_cInfo, _categoryOrItem, _form, _count);
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
                                FormCheck(_cInfo, _categoryOrItem, _form, _count);
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
                                FormCheck(_cInfo, _categoryOrItem, _form, _count);
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("Shop3", out string _phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        else if (!Inside_Market && !Inside_Traders)
                        {
                            FormCheck(_cInfo, _categoryOrItem, _form, _count);
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

        public static void FormCheck(ClientInfo _cInfo, string _categoryOrItem, int _form, int _count)
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
                Phrases.Dict.TryGetValue("Shop13", out _phrase);
                _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                _phrase = _phrase.Replace("{Command_shop_buy}", Command_shop_buy);
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
                        string[] _itemData = Dict[i];
                        if (_itemData[6] == _category)
                        {
                            if (int.Parse(_itemData[4]) > 1)
                            {
                                Phrases.Dict.TryGetValue("Shop11", out string _phrase);
                                _phrase = _phrase.Replace("{Id}", _itemData[0]);
                                _phrase = _phrase.Replace("{Count}", _itemData[3]);
                                _phrase = _phrase.Replace("{Item}", _itemData[2]);
                                _phrase = _phrase.Replace("{Quality}", _itemData[4]);
                                _phrase = _phrase.Replace("{Price}", _itemData[5]);
                                _phrase = _phrase.Replace("{Name}", Wallet.Currency_Name);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("Shop12", out string _phrase);
                                _phrase = _phrase.Replace("{Id}", _itemData[0]);
                                _phrase = _phrase.Replace("{Count}", _itemData[3]);
                                _phrase = _phrase.Replace("{Item}", _itemData[2]);
                                _phrase = _phrase.Replace("{Price}", _itemData[5]);
                                _phrase = _phrase.Replace("{Name}", Wallet.Currency_Name);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
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
                for (int i = 0; i < Dict.Count; i++)
                {
                    string[] _itemData = Dict[i];
                    if (int.Parse(_itemData[0]) == _item)
                    {
                        int currency = 0;
                        int bankValue = 0;
                        if (Wallet.IsEnabled)
                        {
                            currency = Wallet.GetCurrency(_cInfo.CrossplatformId.CombinedString);
                        }
                        if (Bank.IsEnabled && Bank.Payments)
                        {
                            bankValue = Bank.GetCurrency(_cInfo.CrossplatformId.CombinedString);
                        }
                        int _cost = int.Parse(_itemData[5]) * _count;
                        if (currency + bankValue >= _cost)
                        {
                            _count = int.Parse(_itemData[3]) * _count;
                            ShopPurchase(_cInfo, _itemData[1], _itemData[2], _count, int.Parse(_itemData[4]), _cost);
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("Shop5", out string _phrase);
                            _phrase = _phrase.Replace("{CoinName}", Wallet.Currency_Name);
                            _phrase = _phrase.Replace("{Value}", currency + bankValue.ToString());
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        return;
                    }
                }
                Phrases.Dict.TryGetValue("Shop15", out string _phrase1);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                ItemValue itemValue = new ItemValue(ItemClass.GetItem(_itemName).type);
                if (itemValue.HasQuality)
                {
                    itemValue.Quality = 1;
                    if (_quality > 0)
                    {
                        itemValue.Quality = _quality;
                    }
                }
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
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
                world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                if (_price >= 1 && Wallet.IsEnabled)
                {
                    if (Bank.IsEnabled && Bank.Payments)
                    {
                        Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, _price, true);
                    }
                    else
                    {
                        Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, _price, false);
                    }
                }
                Log.Out(string.Format("Sold '{0}' to '{1}' '{2}' named '{3}' through the shop", itemValue.ItemClass.Name, _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName));
                Phrases.Dict.TryGetValue("Shop16", out string _phrase);
                _phrase = _phrase.Replace("{Count}", _count.ToString());
                if (_secondaryName != "")
                {
                    _phrase = _phrase.Replace("{Item}", _secondaryName);
                }
                else
                {
                    _phrase = _phrase.Replace("{Item}", itemValue.ItemClass.GetLocalizedItemName() ?? itemValue.ItemClass.GetItemName());
                }
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Shop.ShopPurchase: {0}", e.Message));
            }
        }

        private static void UpgradeXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<Shop>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("    <!-- Secondary name is what will show in chat instead of the item name -->");
                    sw.WriteLine("    <!-- <Item Name=\"\" SecondaryName=\"\" Count=\"\" Quality=\"\" Price=\"\" Category=\"\" /> -->");
                    for (int i = 0; i < OldNodeList.Count; i++)
                    {
                        if (OldNodeList[i].NodeType == XmlNodeType.Comment && !OldNodeList[i].OuterXml.Contains("<!-- Secondary name is") && 
                            !OldNodeList[i].OuterXml.Contains("<!-- <Item Name=\"\""))
                        {
                            sw.WriteLine(OldNodeList[i].OuterXml);
                        }
                    }
                    sw.WriteLine();
                    sw.WriteLine();
                    for (int i = 0; i < OldNodeList.Count; i++)
                    {
                        if (OldNodeList[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)OldNodeList[i];
                            if (line.HasAttributes && (line.Name == "Shop" || line.Name == "Item"))
                            {
                                string name = "", secondaryName = "", count = "", quality = "", price = "", category = "";
                                if (line.HasAttribute("Name"))
                                {
                                    name = line.GetAttribute("Name");
                                }
                                if (line.HasAttribute("SecondaryName"))
                                {
                                    secondaryName = line.GetAttribute("SecondaryName");
                                }
                                if (line.HasAttribute("Count"))
                                {
                                    count = line.GetAttribute("Count");
                                }
                                if (line.HasAttribute("Quality"))
                                {
                                    quality = line.GetAttribute("Quality");
                                }
                                if (line.HasAttribute("Price"))
                                {
                                    price = line.GetAttribute("Price");
                                }
                                if (line.HasAttribute("Category"))
                                {
                                    category = line.GetAttribute("Category");
                                }
                                sw.WriteLine(string.Format("    <Item Name=\"{0}\" SecondaryName=\"{1}\" Count=\"{2}\" Quality=\"{3}\" Price=\"{4}\" Category=\"{5}\" />", name, secondaryName, count, quality, price, category));
                            }
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