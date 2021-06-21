using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    public class Gimme
    {
        public static bool IsEnabled = false, IsRunning = false, Zombies = false;
        public static int Delay_Between_Uses = 60, Command_Cost = 0;
        public static string Command24 = "gimme", Command25 = "gimmie", Zombie_Id = "4,9,11";
        private const string file = "Gimme.xml";
        private static readonly string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static Dictionary<string, int[]> Dict = new Dictionary<string, int[]>();
        private static Dictionary<string, string> Dict1 = new Dictionary<string, string>();
        private static readonly FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        private static readonly System.Random Rnd = new System.Random();
        private static bool UpdateConfig = false;

        private static List<string> List
        {
            get { return new List<string>(Dict.Keys); }
        }

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        public static void Unload()
        {
            Dict.Clear();
            Dict1.Clear();
            FileWatcher.Dispose();
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
                if (childNode.Name == "Items")
                {
                    Dict.Clear();
                    Dict1.Clear();
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
                        if (!_line.HasAttribute("Name"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of missing Name attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("SecondaryName"))
                        {
                            UpdateConfig = true;
                        }
                        if (!_line.HasAttribute("MinCount"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of missing MinCount attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("MaxCount"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of missing MaxCount attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("MinQuality"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of missing MinQuality attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("MaxQuality"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of missing MaxQuality attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("MinCount"), out int _minCount))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of invalid (non-numeric) value for 'MinCount' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("MaxCount"), out int _maxCount))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of invalid (non-numeric) value for 'MaxCount' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("MinQuality"), out int _minQuality))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of invalid (non-numeric) value for 'MinQuality' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("MaxQuality"), out int _maxQuality))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of invalid (non-numeric) value for 'MaxQuality' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string _item = _line.GetAttribute("Name");
                        if (_item == "WalletCoin")
                        {
                            if (Wallet.IsEnabled)
                            {
                                if (_minCount < 1)
                                {
                                    _minCount = 1;
                                }
                            }
                            else
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Gimme.xml entry skipped because the Wallet tool is not enabled: {0}", subChild.OuterXml));
                                continue;
                            }
                        }
                        else
                        {
                            ItemValue _itemValue = ItemClass.GetItem(_item, false);
                            if (_itemValue.type == ItemValue.None.type)
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Gimme entry skipped. Item not found: {0}", _item));
                                continue;
                            }
                            if (_minCount > _itemValue.ItemClass.Stacknumber.Value)
                            {
                                _minCount = _itemValue.ItemClass.Stacknumber.Value;
                            }
                            else if (_minCount < 1)
                            {
                                _minCount = 1;
                            }
                            if (_maxCount > _itemValue.ItemClass.Stacknumber.Value)
                            {
                                _maxCount = _itemValue.ItemClass.Stacknumber.Value;
                            }
                            else if (_maxCount < 1)
                            {
                                _maxCount = 1;
                            }
                        }
                        string _secondaryname;
                        if (_line.HasAttribute("SecondaryName"))
                        {
                            _secondaryname = _line.GetAttribute("SecondaryName");
                        }
                        else
                        {
                            _secondaryname = _item;
                        }
                        if (_minQuality == 0)
                        {
                            _minQuality = 1;
                        }
                        if (_maxQuality < 1)
                        {
                            _maxQuality = 1;
                        }
                        if (!Dict.ContainsKey(_item))
                        {
                            int[] _c = new int[] { _minCount, _maxCount, _minQuality, _maxQuality };
                            Dict.Add(_item, _c);
                            Dict1.Add(_item, _secondaryname);
                        }
                    }
                }
            }
            if (UpdateConfig)
            {
                UpdateConfig = false;
                UpdateXml();
            }
        }

        private static void UpdateXml()
        {
            FileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<!--  Secondary name is what will show in chat instead of the item name  -->");
                sw.WriteLine("<!--  Items that do not require a quality should be set to 0 or 1 for min and max  -->");
                sw.WriteLine("<!--  WalletCoin can be used as the item name. Secondary name should be set to your Wallet Coin_Name option  -->");
                sw.WriteLine("<Gimme>");
                sw.WriteLine("    <Items>");
                if (Dict.Count > 0)
                {
                    foreach (KeyValuePair<string, int[]> kvp in Dict)
                    {
                        if (Dict1.TryGetValue(kvp.Key, out string _name))
                        {
                            sw.WriteLine(string.Format("        <Item Name=\"{0}\" SecondaryName=\"{1}\" MinCount=\"{2}\" MaxCount=\"{3}\" MinQuality=\"{4}\" MaxQuality=\"{5}\" />", kvp.Key, _name, kvp.Value[0], kvp.Value[1], kvp.Value[2], kvp.Value[3]));
                        }
                    }
                }
                else
                {
                    sw.WriteLine("        <Item Name=\"drinkJarBoiledWater\" SecondaryName=\"Bottled Water\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"drinkJarBeer\" SecondaryName=\"Beer\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"foodCanChicken\" SecondaryName=\"Can of Chicken\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"foodCanChili\" SecondaryName=\"Can of Chili\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"foodCropCorn\" SecondaryName=\"Corn\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"foodCropPotato\" SecondaryName=\"Potato\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"medicalBandage\" SecondaryName=\"First Aid Bandage\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"drugPainkillers\" SecondaryName=\"Pain Killers\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"resourceScrapBrass\" SecondaryName=\"Scrap Brass\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"drugAntibiotics\" SecondaryName=\"Antibiotics\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"foodMoldyBread\" SecondaryName=\"Moldy Bread\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"resourceOil\" SecondaryName=\"Oil\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"foodCornMeal\" SecondaryName=\"Cornmeal\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"foodCropBlueberries\" SecondaryName=\"Blueberries\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"resourceCropCoffeeBeans\" SecondaryName=\"Coffee Beans\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"casinoCoin\" SecondaryName=\"Casino Coins\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"meleeWpnBladeT0BoneKnife\" SecondaryName=\"Bone Knife\" MinCount=\"1\" MaxCount=\"1\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"foodCanDogfood\" SecondaryName=\"Can of Dog Food\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"foodBlueberryPie\" SecondaryName=\"Blueberry Pie\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"foodCanPeas\" SecondaryName=\"Can of Peas\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"foodCanCatfood\" SecondaryName=\"Can of Cat Food\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"resourceScrapIron\" SecondaryName=\"Scrap Iron\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"resourceCropGoldenrodPlant\" SecondaryName=\"Goldenrod Plant\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"resourceClayLump\" SecondaryName=\"Lumps of Clay\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"foodRottingFlesh\" SecondaryName=\"Rotting Flesh\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                }
                sw.WriteLine("    </Items>");
                sw.WriteLine("</Gimme>");
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

        public static void Exec(ClientInfo _cInfo)
        {
            if (Dict.Count == 0)
            {
                return;
            }
            if (Delay_Between_Uses < 1)
            {
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    CommandCost(_cInfo);
                }
                else
                {
                    ZCheck(_cInfo);
                }
            }
            else
            {
                DateTime _lastgimme = DateTime.Now;
                if (PersistentContainer.Instance.Players[_cInfo.playerId].LastGimme != null)
                {
                    _lastgimme = PersistentContainer.Instance.Players[_cInfo.playerId].LastGimme;
                }
                TimeSpan varTime = DateTime.Now - _lastgimme;
                double fractionalMinutes = varTime.TotalMinutes;
                int _timepassed = (int)fractionalMinutes;
                if (ReservedSlots.IsEnabled)
                {
                    if (ReservedSlots.Reduced_Delay && ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                    {
                        ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out DateTime _dt);
                        if (DateTime.Now < _dt)
                        {
                            int _delay = Delay_Between_Uses / 2;
                            Time(_cInfo, _timepassed, _delay);
                            return;
                        }
                    }
                }
                Time(_cInfo, _timepassed, Delay_Between_Uses);
            }
        }

        private static void Time(ClientInfo _cInfo, int _timepassed, int _delay)
        {
            if (_timepassed >= _delay)
            {
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    CommandCost(_cInfo);
                }
                else
                {
                    ZCheck(_cInfo);
                }
            }
            else
            {
                int _timeleft = _delay - _timepassed;
                Phrases.Dict.TryGetValue(21, out string _phrase21);
                _phrase21 = _phrase21.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase21 = _phrase21.Replace("{TimeRemaining}", _timeleft.ToString());
                _phrase21 = _phrase21.Replace("{CommandPrivate}", ChatHook.Chat_Command_Prefix1);
                _phrase21 = _phrase21.Replace("{Command24}", Command24);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase21 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void CommandCost(ClientInfo _cInfo)
        {
            if (Wallet.IsEnabled && Command_Cost > 0)
            {
                if (Wallet.GetCurrentCoins(_cInfo.playerId) >= Command_Cost)
                {
                    ZCheck(_cInfo);
                }
                else
                {
                    Phrases.Dict.TryGetValue(23, out string _phrase23);
                    _phrase23 = _phrase23.Replace("{CoinName}", Wallet.Coin_Name);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase23 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                ZCheck(_cInfo);
            }
        }

        private static void ZCheck(ClientInfo _cInfo)
        {
            if (Zombies)
            {
                int itemOrEntity = Rnd.Next(1, 9);
                if (itemOrEntity != 4)
                {
                    RandomItem(_cInfo);
                }
                else
                {
                    RandomZombie(_cInfo);
                }
            }
            else
            {
                RandomItem(_cInfo);
            }
        }

        private static void RandomItem(ClientInfo _cInfo)
        {
            try
            {
                string _randomItem = List.RandomObject();
                if (Dict.TryGetValue(_randomItem, out int[] _itemData))
                {
                    if (_randomItem == "WalletCoin")
                    {
                        if (Wallet.IsEnabled)
                        {
                            int _count = 1;
                            if (_itemData[0] > _itemData[1])
                            {
                                _count = Rnd.Next(_itemData[1], _itemData[0] + 1);
                            }
                            else
                            {
                                _count = Rnd.Next(_itemData[0], _itemData[1] + 1);
                            }
                            if (Command_Cost >= 1)
                            {
                                Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                            }
                            Wallet.AddCoinsToWallet(_cInfo.playerId, _count);
                            PersistentContainer.Instance.Players[_cInfo.playerId].LastGimme = DateTime.Now;
                            PersistentContainer.DataChange = true;
                            Phrases.Dict.TryGetValue(22, out string _phrase22);
                            _phrase22 = _phrase22.Replace("{ItemCount}", _count.ToString());
                            Dict1.TryGetValue(_randomItem, out string _name);
                            if (_name != "")
                            {
                                _phrase22 = _phrase22.Replace("{ItemName}", _name);
                            }
                            else
                            {
                                _phrase22 = _phrase22.Replace("{ItemName}", Wallet.Coin_Name);
                            }
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase22 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue(25, out string _phrase25);
                            Log.Out(string.Format("[SERVERTOOLS] {0}", _phrase25));
                        }
                    }
                    else
                    {
                        ItemValue _itemValue = new ItemValue(ItemClass.GetItem(_randomItem, false).type, false);
                        int _count = 0;
                        if (_itemData[0] > _itemData[1])
                        {
                            _count = Rnd.Next(_itemData[1], _itemData[0] + 1);
                        }
                        else
                        {
                            _count = Rnd.Next(_itemData[0], _itemData[1] + 1);
                        }
                        if (_itemValue.HasQuality && _itemData[2] > 0 && _itemData[3] >= _itemData[2])
                        {
                            _itemValue.Quality = Rnd.Next(_itemData[2], _itemData[3] + 1);
                        }
                        EntityItem entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                        {
                            entityClass = EntityClass.FromString("item"),
                            id = EntityFactory.nextEntityID++,
                            itemStack = new ItemStack(_itemValue, _count),
                            pos = GameManager.Instance.World.Players.dict[_cInfo.entityId].position,
                            rot = new Vector3(20f, 0f, 20f),
                            lifetime = 60f,
                            belongsPlayerId = _cInfo.entityId
                        });
                        GameManager.Instance.World.SpawnEntityInWorld(entityItem);
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
                        GameManager.Instance.World.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                        if (Wallet.IsEnabled && Command_Cost >= 1)
                        {
                            Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                        }
                        PersistentContainer.Instance.Players[_cInfo.playerId].LastGimme = DateTime.Now;
                        PersistentContainer.DataChange = true;
                        Phrases.Dict.TryGetValue(22, out string _phrase22);
                        _phrase22 = _phrase22.Replace("{ItemCount}", _count.ToString());
                        Dict1.TryGetValue(_randomItem, out string _name);
                        if (_name != "")
                        {
                            _phrase22 = _phrase22.Replace("{ItemName}", _name);
                        }
                        else
                        {
                            _phrase22 = _phrase22.Replace("{ItemName}", _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.Name);
                        }
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase22 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Gimme.RandomItem: {0}", e.Message));
            }
        }

        private static void RandomZombie(ClientInfo _cInfo)
        {
            if (Zombie_Id != "")
            {
                if (Zombie_Id.Contains(","))
                {
                    string[] _zombieIds = Zombie_Id.Split(',');
                    int _count = Rnd.Next(1, _zombieIds.Length + 1);
                    string _zId = _zombieIds[_count];
                    if (int.TryParse(_zId, out int _zombieId))
                    {
                        SdtdConsole.Instance.ExecuteSync(string.Format("ser {0} 10 @ {1}", _cInfo.playerId, _zombieId), null);
                        Log.Out(string.Format("[SERVERTOOLS] Spawned an entity for {0}'s gimme", _cInfo.playerName));
                        if (Wallet.IsEnabled && Command_Cost >= 1)
                        {
                            Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                        }
                        PersistentContainer.Instance.Players[_cInfo.playerId].LastGimme = DateTime.Now;
                        PersistentContainer.DataChange = true;
                        Phrases.Dict.TryGetValue(24, out string _phrase24);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase24 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        RandomItem(_cInfo);
                    }
                }
                else
                {
                    if (int.TryParse(Zombie_Id, out int _zombieId))
                    {
                        SdtdConsole.Instance.ExecuteSync(string.Format("ser {0} 10 @ {1}", _cInfo.playerId, _zombieId), null);
                        Log.Out(string.Format("[SERVERTOOLS] Spawned an entity for {0}'s gimme", _cInfo.playerName));
                        if (Wallet.IsEnabled && Command_Cost >= 1)
                        {
                            Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                        }
                        PersistentContainer.Instance.Players[_cInfo.playerId].LastGimme = DateTime.Now;
                        PersistentContainer.DataChange = true;
                        Phrases.Dict.TryGetValue(24, out string _phrase24);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase24 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        RandomItem(_cInfo);
                    }
                }
            }
            else
            {
                RandomItem(_cInfo);
            }
        }
    }
}