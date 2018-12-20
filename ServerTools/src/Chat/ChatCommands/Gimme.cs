using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    public class Gimme
    {
        public static bool IsEnabled = false, IsRunning = false, Always_Show_Response = false,
            Zombies = false;
        public static int Delay_Between_Uses = 60, Command_Cost = 0;
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
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring items entry because of missing item attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("secondaryname"))
                        {
                            updateConfig = true;
                        }
                        if (!_line.HasAttribute("min"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring items entry because of missing min attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("max"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring items entry because of missing max attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        int _min = 1;
                        int _max = 1;
                        if (!int.TryParse(_line.GetAttribute("min"), out _min))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring items entry because of invalid (non-numeric) value for 'min' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("max"), out _max))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring items entry because of invalid (non-numeric) value for 'max' attribute: {0}", subChild.OuterXml));
                            continue;
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
                            int[] _c = new int[] { _min, _max };
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
                            sw.WriteLine(string.Format("        <item item=\"{0}\" secondaryname=\"{1}\" min=\"{2}\" max=\"{3}\" />", kvp.Key, _name, kvp.Value[0], kvp.Value[1]));
                        }
                    }
                }
                else
                {
                    sw.WriteLine("        <item item=\"drinkJarBoiledWater\" secondaryname=\"Bottled Water\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"drinkJarBeer\" secondaryname=\"Beer\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"foodCanChicken\" secondaryname=\"Can of Chicken\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"foodcanChili\" secondaryname=\"Can of Chilli\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"foodCropCorn\" secondaryname=\"Corn\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"foodCropPotato\" secondaryname=\"Potato\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"medicalBandage\" secondaryname=\"First Aid Bandage\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"drugPainkillers\" secondaryname=\"Pain Killers\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"resourceScrapBrass\" secondaryname=\"Scrap Brass\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"drugAntibiotics\" secondaryname=\"Antibiotics\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"foodMoldyBread\" secondaryname=\"Moldy Bread\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"resourceOil\" secondaryname=\"Oil\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"foodCornMeal\" secondaryname=\"Cornmeal\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"foodCropBlueberries\" secondaryname=\"Blueberries\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"foodCanHam\" secondaryname=\"Can of Ham\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"resourceCropCoffeeBeans\" secondaryname=\"Coffee Beans\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"casinoCoin\" secondaryname=\"Casino Coins\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"meleeBoneShiv\" secondaryname=\"Bone Shiv\" min=\"1\" max=\"1\" />");
                    sw.WriteLine("        <item item=\"foodCanDogfood\" secondaryname=\"Can of Dog Food\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"foodBlueberryPie\" secondaryname=\"Blueberry Pie\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"foodCanPeas\" secondaryname=\"Can of Peas\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"foodCanCatfood\" secondaryname=\"Can of Cat Food\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"resourceScrapIron\" secondaryname=\"Scrap Iron\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"resourceCropGoldenrodPlant\" secondaryname=\"Goldenrod Plant\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"resourceClayLump\" secondaryname=\"Lumps of Clay\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"foodRottingFlesh\" secondaryname=\"Rotting Flesh\" min=\"1\" max=\"5\" />");
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

        public static void Checkplayer(ClientInfo _cInfo, bool _announce, string _playerName)
        {
            bool _donator = false;
            if (Delay_Between_Uses < 1)
            {
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    CommandCost(_cInfo, _announce);
                }
                else
                {
                    GiveItem(_cInfo, _announce);
                }
            }
            else
            {
                string _sql = string.Format("SELECT last_gimme FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                DataTable _result = SQL.TQuery(_sql);
                if (_result.Rows.Count > 0)
                {
                    DateTime _lastgimme;
                    DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _lastgimme);
                    _result.Dispose();
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
                                _donator = true;
                                int _newDelay = Delay_Between_Uses / 2;
                                if (_timepassed >= _newDelay)
                                {
                                    if (Wallet.IsEnabled && Command_Cost >= 1)
                                    {
                                        CommandCost(_cInfo, _announce);
                                    }
                                    else
                                    {
                                        GiveItem(_cInfo, _announce);
                                    }
                                }
                                else
                                {
                                    int _timeleft = _newDelay - _timepassed;
                                    string _phrase6;
                                    if (!Phrases.Dict.TryGetValue(6, out _phrase6))
                                    {
                                        _phrase6 = " you can only use /gimme once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                                    }
                                    _phrase6 = _phrase6.Replace("{DelayBetweenUses}", _newDelay.ToString());
                                    _phrase6 = _phrase6.Replace("{TimeRemaining}", _timeleft.ToString());
                                    if (_announce)
                                    {
                                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color  + _phrase6 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                    }
                                    else
                                    {
                                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color  + _phrase6 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                            }
                        }
                    }
                    if (!_donator)
                    {
                        if (_timepassed >= Delay_Between_Uses)
                        {
                            if (Wallet.IsEnabled && Command_Cost >= 1)
                            {
                                CommandCost(_cInfo, _announce);
                            }
                            else
                            {
                                GiveItem(_cInfo, _announce);
                            }
                        }
                        else
                        {
                            int _timeleft = Delay_Between_Uses - _timepassed;
                            string _phrase6;
                            if (!Phrases.Dict.TryGetValue(6, out _phrase6))
                            {
                                _phrase6 = " you can only use /gimme once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                            }
                            _phrase6 = _phrase6.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                            _phrase6 = _phrase6.Replace("{TimeRemaining}", _timeleft.ToString());
                            if (_announce)
                            {
                                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color  + _phrase6 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                            }
                            else
                            {
                                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color  + _phrase6 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                }
                else
                {
                    _result.Dispose();
                }
            }
        }

        public static void CommandCost(ClientInfo _cInfo, bool _announce)
        {
            int _currentCoins = Wallet.GetcurrentCoins(_cInfo);
            if (_currentCoins >= Command_Cost)
            {
                GiveItem(_cInfo, _announce);
            }
            else
            {
                string _phrase814;
                if (!Phrases.Dict.TryGetValue(814, out _phrase814))
                {
                    _phrase814 = " you do not have enough {WalletCoinName} in your wallet to run this command.";
                }
                _phrase814 = _phrase814.Replace("{WalletCoinName}", Wallet.Coin_Name);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color  + _phrase814 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void GiveItem(ClientInfo _cInfo, bool _announce)
        {
            if (Zombies)
            {
                int itemOrEntity = random.Next(1, 9);
                if (itemOrEntity != 4)
                {
                    RandomItem(_cInfo, _announce);
                }
                else
                {
                    RandomZombie(_cInfo, _announce);
                }
            }
            else
            {
                RandomItem(_cInfo, _announce);
            }
        }

        private static void RandomItem(ClientInfo _cInfo, bool _announce)
        {
            string _randomItem = list.RandomObject();
            ItemValue _itemValue = ItemClass.GetItem(_randomItem, true);
            _itemValue = new ItemValue(_itemValue.type, true);
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            int _quality = 1;
            if (_itemValue.HasQuality)
            {
                _quality = random.Next(1, 7);
                _itemValue.Quality = _quality;
            }
            int[] _counts;
            if (dict.TryGetValue(_randomItem, out _counts))
            {
                int _count = random.Next(_counts[0], _counts[1] + 1);
                ItemStack _itemDrop = new ItemStack(_itemValue, _count);
                ItemValue itemValue;
                itemValue = new ItemValue(ItemClass.GetItem(_randomItem).type, _quality, _quality, true);
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
                _cInfo.SendPackage(new NetPackageEntityCollect(entityItem.entityId, _cInfo.entityId));
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
                if (_announce)
                {
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase7 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase7 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                string _sql;
                if (Wallet.IsEnabled && Command_Cost >= 1)
                {
                    Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
                }
                _sql = string.Format("UPDATE Players SET last_gimme = '{0}' WHERE steamid = '{1}'", DateTime.Now.ToString(), _cInfo.playerId);
                SQL.FastQuery(_sql);
            }
        }

        private static void RandomZombie(ClientInfo _cInfo, bool _announce)
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
            if (_announce)
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase807 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase807 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            string _sql;
            if (Wallet.IsEnabled && Command_Cost >= 1)
            {
                Wallet.SubtractCoinsFromWallet(_cInfo.playerId, Command_Cost);
            }
            _sql = string.Format("UPDATE Players SET last_gimme = '{0}' WHERE steamid = '{1}'", DateTime.Now.ToString(), _cInfo.playerId);
            SQL.FastQuery(_sql);
        }
    }
}