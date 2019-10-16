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
        public static string Command24 = "gimme", Command25 = "gimmie";
        private const string file = "GimmeItems.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static Dictionary<string, int[]> dict = new Dictionary<string, int[]>();
        private static Dictionary<string, string> dict1 = new Dictionary<string, string>();
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        private static System.Random random = new System.Random();
        private static bool updateConfig = false;

        private static List<string> list
        {
            get { return new List<string>(dict.Keys); }
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
                dict.Clear();
                dict1.Clear();
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
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'items' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("item"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of missing item attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("secondaryname"))
                        {
                            updateConfig = true;
                        }
                        if (!_line.HasAttribute("minCount"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of missing minCount attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("maxCount"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of missing maxCount attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("minQuality"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of missing minQuality attribute: {0}", subChild.OuterXml));
                        }
                        if (!_line.HasAttribute("maxQuality"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of missing maxQuality attribute: {0}", subChild.OuterXml));
                        }
                        int _minCount = 1, _maxCount = 1, _minQuality = 1, _maxQuality = 1;
                        if (!int.TryParse(_line.GetAttribute("minCount"), out _minCount))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of invalid (non-numeric) value for 'minCount' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("maxCount"), out _maxCount))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of invalid (non-numeric) value for 'maxCount' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("minQuality"), out _minQuality))
                        {

                            Log.Out(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of invalid (non-numeric) value for 'minQuality' attribute: {0}", subChild.OuterXml));
                        }
                        if (!int.TryParse(_line.GetAttribute("maxQuality"), out _maxQuality))
                        {

                            Log.Out(string.Format("[SERVERTOOLS] Ignoring Gimme entry because of invalid (non-numeric) value for 'maxQuality' attribute: {0}", subChild.OuterXml));
                        }
                        string _item = _line.GetAttribute("item");
                        ItemClass _class = ItemClass.GetItemClass(_item, true);
                        Block _block = Block.GetBlockByName(_item, true);
                        if (_class == null && _block == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Gimme entry skipped. Item not found: {0}", _item));
                            continue;
                        }
                        string _secondaryname;
                        if (_line.HasAttribute("secondaryname"))
                        {
                            _secondaryname = _line.GetAttribute("secondaryname");
                        }
                        else
                        {
                            _secondaryname = _item;
                        }
                        if (!dict.ContainsKey(_item))
                        {
                            int[] _c = new int[] { _minCount, _maxCount, _minQuality, _maxQuality };
                            dict.Add(_item, _c);
                        }
                        if (!dict1.ContainsKey(_item))
                        {
                            dict1.Add(_item, _secondaryname);
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
                sw.WriteLine("    <items>");
                if (dict.Count > 0)
                {
                    foreach (KeyValuePair<string, int[]> kvp in dict)
                    {
                        string _name;
                        if (dict1.TryGetValue(kvp.Key, out _name))
                        {
                            sw.WriteLine(string.Format("        <item item=\"{0}\" secondaryname=\"{1}\" minCount=\"{2}\" maxCount=\"{3}\" minQuality=\"{4}\" maxQuality=\"{5}\" />", kvp.Key, _name, kvp.Value[0], kvp.Value[1], kvp.Value[2], kvp.Value[3]));
                        }
                    }
                }
                else
                {
                    sw.WriteLine("        <item item=\"drinkJarBoiledWater\" secondaryname=\"Bottled Water\" minCount=\"1\" maxCount=\"5\" minQuality=\"1\" maxQuality=\"1\" />");
                    sw.WriteLine("        <item item=\"drinkJarBeer\" secondaryname=\"Beer\" minCount=\"1\" maxCount=\"5\" minQuality=\"1\" maxQuality=\"1\" />");
                    sw.WriteLine("        <item item=\"foodCanChicken\" secondaryname=\"Can of Chicken\" minCount=\"1\" maxCount=\"5\" minQuality=\"1\" maxQuality=\"1\" />");
                    sw.WriteLine("        <item item=\"foodcanChili\" secondaryname=\"Can of Chilli\" minCount=\"1\" maxCount=\"5\" minQuality=\"1\" maxQuality=\"1\" />");
                    sw.WriteLine("        <item item=\"foodCropCorn\" secondaryname=\"Corn\" minCount=\"1\" maxCount=\"5\" minQuality=\"1\" maxQuality=\"1\" />");
                    sw.WriteLine("        <item item=\"foodCropPotato\" secondaryname=\"Potato\" minCount=\"1\" maxCount=\"5\" minQuality=\"1\" maxQuality=\"1\" />");
                    sw.WriteLine("        <item item=\"medicalBandage\" secondaryname=\"First Aid Bandage\" minCount=\"1\" maxCount=\"5\" minQuality=\"1\" maxQuality=\"1\" />");
                    sw.WriteLine("        <item item=\"drugPainkillers\" secondaryname=\"Pain Killers\" minCount=\"1\" maxCount=\"5\" minQuality=\"1\" maxQuality=\"1\" />");
                    sw.WriteLine("        <item item=\"resourceScrapBrass\" secondaryname=\"Scrap Brass\" minCount=\"1\" maxCount=\"5\" minQuality=\"1\" maxQuality=\"1\" />");
                    sw.WriteLine("        <item item=\"drugAntibiotics\" secondaryname=\"Antibiotics\" minCount=\"1\" maxCount=\"5\" minQuality=\"1\" maxQuality=\"1\" />");
                    sw.WriteLine("        <item item=\"foodMoldyBread\" secondaryname=\"Moldy Bread\" minCount=\"1\" maxCount=\"5\" minQuality=\"1\" maxQuality=\"1\" />");
                    sw.WriteLine("        <item item=\"resourceOil\" secondaryname=\"Oil\" minCount=\"1\" maxCount=\"5\" minQuality=\"1\" maxQuality=\"1\" />");
                    sw.WriteLine("        <item item=\"foodCornMeal\" secondaryname=\"Cornmeal\" minCount=\"1\" maxCount=\"5\" minQuality=\"1\" maxQuality=\"1\" />");
                    sw.WriteLine("        <item item=\"foodCropBlueberries\" secondaryname=\"Blueberries\" minCount=\"1\" maxCount=\"5\" minQuality=\"1\" maxQuality=\"1\" />");
                    sw.WriteLine("        <item item=\"resourceCropCoffeeBeans\" secondaryname=\"Coffee Beans\" minCount=\"1\" maxCount=\"5\" minQuality=\"1\" maxQuality=\"1\" />");
                    sw.WriteLine("        <item item=\"casinoCoin\" secondaryname=\"Casino Coins\" minCount=\"1\" maxCount=\"5\" minQuality=\"1\" maxQuality=\"1\" />");
                    sw.WriteLine("        <item item=\"meleeToolKnifeBone\" secondaryname=\"Bone Shiv\" minCount=\"1\" maxCount=\"1\" minQuality=\"1\" maxQuality=\"1\" />");
                    sw.WriteLine("        <item item=\"foodCanDogfood\" secondaryname=\"Can of Dog Food\" minCount=\"1\" maxCount=\"5\" minQuality=\"1\" maxQuality=\"1\" />");
                    sw.WriteLine("        <item item=\"foodBlueberryPie\" secondaryname=\"Blueberry Pie\" minCount=\"1\" maxCount=\"5\" minQuality=\"1\" maxQuality=\"1\" />");
                    sw.WriteLine("        <item item=\"foodCanPeas\" secondaryname=\"Can of Peas\" minCount=\"1\" maxCount=\"5\" minQuality=\"1\" maxQuality=\"1\" />");
                    sw.WriteLine("        <item item=\"foodCanCatfood\" secondaryname=\"Can of Cat Food\" minCount=\"1\" maxCount=\"5\" minQuality=\"1\" maxQuality=\"1\" />");
                    sw.WriteLine("        <item item=\"resourceScrapIron\" secondaryname=\"Scrap Iron\" minCount=\"1\" maxCount=\"5\" minQuality=\"1\" maxQuality=\"1\" />");
                    sw.WriteLine("        <item item=\"resourceCropGoldenrodPlant\" secondaryname=\"Goldenrod Plant\" minCount=\"1\" maxCount=\"5\" minQuality=\"1\" maxQuality=\"1\" />");
                    sw.WriteLine("        <item item=\"resourceClayLump\" secondaryname=\"Lumps of Clay\" minCount=\"1\" maxCount=\"5\" minQuality=\"1\" maxQuality=\"1\" />");
                    sw.WriteLine("        <item item=\"foodRottingFlesh\" secondaryname=\"Rotting Flesh\" minCount=\"1\" maxCount=\"5\" minQuality=\"1\" maxQuality=\"1\" />");
                }
                sw.WriteLine("    </items>");
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
                DateTime _lastgimme = PersistentContainer.Instance.Players[_cInfo.playerId].LastGimme;
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

        public static void Time(ClientInfo _cInfo, int _timepassed, int _delay)
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
                    _phrase6 = " you can only use {CommandPrivate}{Command24} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                }
                _phrase6 = _phrase6.Replace("{DelayBetweenUses}", _delay.ToString());
                _phrase6 = _phrase6.Replace("{TimeRemaining}", _timeleft.ToString());
                _phrase6 = _phrase6.Replace("{CommandPrivate}", ChatHook.Command_Private);
                _phrase6 = _phrase6.Replace("{Command24}", Command24);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase6 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void CommandCost(ClientInfo _cInfo)
        {
            int _currentCoins = Wallet.GetCurrentCoins(_cInfo);
            if (_currentCoins >= Command_Cost)
            {
                ZCheck(_cInfo);
            }
            else
            {
                string _phrase814;
                if (!Phrases.Dict.TryGetValue(814, out _phrase814))
                {
                    _phrase814 = " you do not have enough {WalletCoinName} in your wallet to run this command.";
                }
                _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase814 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
            string _randomItem = list.RandomObject();
            ItemValue _itemValue = ItemClass.GetItem(_randomItem, false);
            _itemValue = new ItemValue(_itemValue.type, false);
            int[] _itemData;
            if (dict.TryGetValue(_randomItem, out _itemData))
            {
                int _count = random.Next(_itemData[0], _itemData[1] + 1);
                ItemStack _itemDrop = new ItemStack(_itemValue, _count);
                int _qualityMin = 1, _qualityMax = 1;
                if (_itemValue.HasQuality)
                {
                    _qualityMin = _itemData[2];
                    _qualityMax = _itemData[3];
                }
                ItemValue itemValue;
                itemValue = new ItemValue(ItemClass.GetItem(_randomItem).type, _qualityMin, _qualityMax, false, null, 1);
                World world = GameManager.Instance.World;
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
                world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Killed);
                string _phrase7;
                if (!Phrases.Dict.TryGetValue(7, out _phrase7))
                {
                    _phrase7 = " received {ItemCount} {ItemName} from gimme.";
                }
                _phrase7 = _phrase7.Replace("{ItemCount}", _count.ToString());
                string _name;
                if (dict1.TryGetValue(_randomItem, out _name))
                {
                    _phrase7 = _phrase7.Replace("{ItemName}", _name);
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase7 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                }
                PersistentContainer.Instance.Players[_cInfo.playerId].LastGimme = DateTime.Now;
                PersistentContainer.Instance.Save();
            }
        }

        private static void RandomZombie(ClientInfo _cInfo)
        {
            Log.Out("[SERVERTOOLS] Spawning zombie for player's gimme");
            int _rndZ = random.Next(1, 4);
            if (_rndZ == 1)
            {
                SdtdConsole.Instance.ExecuteSync(string.Format("ser {0} 10 @ 4", _cInfo.entityId), (ClientInfo)null);
            }
            if (_rndZ == 2)
            {
                SdtdConsole.Instance.ExecuteSync(string.Format("ser {0} 10 @ 9", _cInfo.entityId), (ClientInfo)null);
            }
            else
            {
                SdtdConsole.Instance.ExecuteSync(string.Format("ser {0} 10 @ 11", _cInfo.entityId), (ClientInfo)null);
            }
            string _phrase807;
            if (!Phrases.Dict.TryGetValue(807, out _phrase807))
            {
                _phrase807 = "OH NO! How did that get in there? You have received a zombie.";
            }
            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase807 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            if (Wallet.IsEnabled && Command_Cost >= 1)
            {
                Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
            }
            PersistentContainer.Instance.Players[_cInfo.playerId].LastGimme = DateTime.Now;
            PersistentContainer.Instance.Save();
        }
    }
}