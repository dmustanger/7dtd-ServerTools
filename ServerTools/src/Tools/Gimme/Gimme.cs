using System;
using System.Collections.Generic;
using System.IO;
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
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static Dictionary<string, int[]> Dict = new Dictionary<string, int[]>();
        private static Dictionary<string, string> Dict1 = new Dictionary<string, string>();
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        private static System.Random random = new System.Random();
        private static bool updateConfig = false;

        private static List<string> list
        {
            get { return new List<string>(Dict.Keys); }
        }

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
                            updateConfig = true;
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
                        ItemClass _class = ItemClass.GetItemClass(_item, true);
                        Block _block = Block.GetBlockByName(_item, true);
                        if (_class == null)
                        {
                            if (_block == null)
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Gimme entry skipped. Item not found: {0}", _item));
                                continue;
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
                        if (!Dict.ContainsKey(_item))
                        {
                            int[] _c = new int[] { _minCount, _maxCount, _minQuality, _maxQuality };
                            Dict.Add(_item, _c);
                            Dict1.Add(_item, _secondaryname);
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
                sw.WriteLine("<Gimme>");
                sw.WriteLine("    <Items>");
                if (Dict.Count > 0)
                {
                    foreach (KeyValuePair<string, int[]> kvp in Dict)
                    {
                        string _name;
                        if (Dict1.TryGetValue(kvp.Key, out _name))
                        {
                            sw.WriteLine(string.Format("        <Item Name=\"{0}\" Secondaryname=\"{1}\" MinCount=\"{2}\" MaxCount=\"{3}\" MinQuality=\"{4}\" MaxQuality=\"{5}\" />", kvp.Key, _name, kvp.Value[0], kvp.Value[1], kvp.Value[2], kvp.Value[3]));
                        }
                    }
                }
                else
                {
                    sw.WriteLine("        <Item Name=\"drinkJarBoiledWater\" Secondaryname=\"Bottled Water\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"drinkJarBeer\" Secondaryname=\"Beer\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"foodCanChicken\" Secondaryname=\"Can of Chicken\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"foodCanChili\" Secondaryname=\"Can of Chili\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"foodCropCorn\" Secondaryname=\"Corn\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"foodCropPotato\" Secondaryname=\"Potato\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"medicalBandage\" Secondaryname=\"First Aid Bandage\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"drugPainkillers\" Secondaryname=\"Pain Killers\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"resourceScrapBrass\" Secondaryname=\"Scrap Brass\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"drugAntibiotics\" Secondaryname=\"Antibiotics\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"foodMoldyBread\" Secondaryname=\"Moldy Bread\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"resourceOil\" Secondaryname=\"Oil\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"foodCornMeal\" Secondaryname=\"Cornmeal\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"foodCropBlueberries\" Secondaryname=\"Blueberries\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"resourceCropCoffeeBeans\" Secondaryname=\"Coffee Beans\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"casinoCoin\" Secondaryname=\"Casino Coins\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"meleeWpnBladeT0BoneKnife\" Secondaryname=\"Bone Knife\" MinCount=\"1\" MaxCount=\"1\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"foodCanDogfood\" Secondaryname=\"Can of Dog Food\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"foodBlueberryPie\" Secondaryname=\"Blueberry Pie\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"foodCanPeas\" Secondaryname=\"Can of Peas\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"foodCanCatfood\" Secondaryname=\"Can of Cat Food\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"resourceScrapIron\" Secondaryname=\"Scrap Iron\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"resourceCropGoldenrodPlant\" Secondaryname=\"Goldenrod Plant\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"resourceClayLump\" Secondaryname=\"Lumps of Clay\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"foodRottingFlesh\" Secondaryname=\"Rotting Flesh\" MinCount=\"1\" MaxCount=\"5\" MinQuality=\"1\" MaxQuality=\"1\" />");
                }
                sw.WriteLine("    </Items>");
                sw.WriteLine("</Gimme>");
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
                        DateTime _dt;
                        ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
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
                string _phrase6;
                if (!Phrases.Dict.TryGetValue(6, out _phrase6))
                {
                    _phrase6 = "You can only use {CommandPrivate}{Command24} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                _phrase6 = _phrase6.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase6 = _phrase6.Replace("{TimeRemaining}", _timeleft.ToString());
                _phrase6 = _phrase6.Replace("{CommandPrivate}", ChatHook.Command_Private);
                _phrase6 = _phrase6.Replace("{Command24}", Command24);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase6 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                    string _phrase814;
                    if (!Phrases.Dict.TryGetValue(814, out _phrase814))
                    {
                        _phrase814 = "You do not have enough {Currency} in your wallet to run this command.";
                    }
                    _phrase814 = _phrase814.Replace("{Currency}", TraderInfo.CurrencyItem);
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase814 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                int itemOrEntity = random.Next(1, 9);
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
                string _randomItem = list.RandomObject();
                ItemValue _itemValue = new ItemValue(ItemClass.GetItem(_randomItem, false).type, false);
                int[] _itemData;
                if (Dict.TryGetValue(_randomItem, out _itemData))
                {
                    int _count = 0;
                    if (_itemData[0] > _itemData[1])
                    {
                        _count = random.Next(_itemData[1], _itemData[0] + 1);
                    }
                    else
                    {
                        _count = random.Next(_itemData[0], _itemData[1] + 1);
                    }
                    if (_itemValue.HasQuality && _itemData[2] > 0 && _itemData[3] >= _itemData[2])
                    {
                        _itemValue.Quality = random.Next(_itemData[2], _itemData[3] + 1);
                    }
                    var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
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
                    string _phrase7;
                    if (!Phrases.Dict.TryGetValue(7, out _phrase7))
                    {
                        _phrase7 = "Received {ItemCount} {ItemName} from gimme.";
                    }
                    _phrase7 = _phrase7.Replace("{ItemCount}", _count.ToString());
                    string _name;
                    if (Dict1.TryGetValue(_randomItem, out _name))
                    {
                        _phrase7 = _phrase7.Replace("{ItemName}", _name);
                    }
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase7 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    if (Wallet.IsEnabled && Command_Cost >= 1)
                    {
                        Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                    }
                    PersistentContainer.Instance.Players[_cInfo.playerId].LastGimme = DateTime.Now;
                    PersistentContainer.Instance.Save();
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
                    int _count = random.Next(1, _zombieIds.Length + 1);
                    string _zId = _zombieIds[_count];
                    if (int.TryParse(_zId, out int _zombieId))
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Spawned an entity for {0}'s gimme", _cInfo.playerName));
                        SdtdConsole.Instance.ExecuteSync(string.Format("ser {0} 10 @ {1}", _cInfo.playerId, _zombieId), null);
                        string _phrase807;
                        if (!Phrases.Dict.TryGetValue(807, out _phrase807))
                        {
                            _phrase807 = "OH NO! How did that get in there? You have received a zombie.";
                        }
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase807 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        if (Wallet.IsEnabled && Command_Cost >= 1)
                        {
                            Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                        }
                        PersistentContainer.Instance.Players[_cInfo.playerId].LastGimme = DateTime.Now;
                        PersistentContainer.Instance.Save();
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
                        Log.Out(string.Format("[SERVERTOOLS] Spawned an entity for {0}'s gimme", _cInfo.playerName));
                        SdtdConsole.Instance.ExecuteSync(string.Format("ser {0} 10 @ {1}", _cInfo.playerId, _zombieId), null);
                        string _phrase807;
                        if (!Phrases.Dict.TryGetValue(807, out _phrase807))
                        {
                            _phrase807 = "OH NO! How did that get in there? You have received a zombie.";
                        }
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase807 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        if (Wallet.IsEnabled && Command_Cost >= 1)
                        {
                            Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                        }
                        PersistentContainer.Instance.Players[_cInfo.playerId].LastGimme = DateTime.Now;
                        PersistentContainer.Instance.Save();
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