using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    class BloodmoonWarrior
    {
        public static bool IsEnabled = false, IsRunning = false, BloodmoonStarted = false, Reduce_Death_Count = false;
        public static int Zombie_Kills = 10, Chance = 50, Reward_Count = 1;
        public static List<string> WarriorList = new List<string>();
        public static Dictionary<string, int> KilledZombies = new Dictionary<string, int>();
        private const string file = "BloodmoonWarrior.xml";
        private static readonly string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static readonly Dictionary<string, int[]> Dict = new Dictionary<string, int[]>();
        private static readonly Dictionary<string, string> Dict1 = new Dictionary<string, string>();
        private static readonly FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        private static readonly System.Random random = new System.Random();
        private static bool updateConfig = false;

        private static List<string> List
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
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Bloodmoon_Warrior.xml' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("Name"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring BloodmoonWarrior entry because of missing name attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("SecondaryName"))
                        {
                            updateConfig = true;
                        }
                        if (!_line.HasAttribute("MinCount"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring BloodmoonWarrior entry because of missing minCount attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("MaxCount"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring BloodmoonWarrior entry because of missing maxCount attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("MinQuality"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring BloodmoonWarrior entry because of missing minQuality attribute: {0}", subChild.OuterXml));
                        }
                        if (!_line.HasAttribute("MaxQuality"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring BloodmoonWarrior entry because of missing maxQuality attribute: {0}", subChild.OuterXml));
                        }
                        if (!int.TryParse(_line.GetAttribute("MinCount"), out int _minCount))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring BloodmoonWarrior entry because of invalid (non-numeric) value for 'minCount' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("MaxCount"), out int _maxCount))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring BloodmoonWarrior entry because of invalid (non-numeric) value for 'maxCount' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("MinQuality"), out int _minQuality))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring BloodmoonWarrior entry because of invalid (non-numeric) value for 'minQuality' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("MaxQuality"), out int _maxQuality))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring BloodmoonWarrior entry because of invalid (non-numeric) value for 'maxQuality' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string _name = _line.GetAttribute("Name");
                        ItemClass _class = ItemClass.GetItemClass(_name, true);
                        Block _block = Block.GetBlockByName(_name, true);
                        if (_class == null && _block == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] BloodmoonWarrior entry skipped. Item name not found: {0}", _name));
                            continue;
                        }
                        string _secondaryname;
                        if (_line.HasAttribute("SecondaryName"))
                        {
                            _secondaryname = _line.GetAttribute("SecondaryName");
                        }
                        else
                        {
                            _secondaryname = _name;
                        }
                        if (!Dict.ContainsKey(_name))
                        {
                            int[] _c = new int[] { _minCount, _maxCount, _minQuality, _maxQuality };
                            Dict.Add(_name, _c);
                        }
                        if (!Dict1.ContainsKey(_name))
                        {
                            Dict1.Add(_name, _secondaryname);
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
            using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<BloodmoonWarrior>");
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
                    sw.WriteLine("        <Item Name=\"drinkJarBoiledWater\" SecondaryName=\"Bottled Water\" MinCount=\"20\" MaxCount=\"30\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"drinkJarBeer\" SecondaryName=\"Beer\" MinCount=\"10\" MaxCount=\"15\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"gunHandgunT2Magnum44\" SecondaryName=\"44 Magnum\" MinCount=\"1\" MaxCount=\"1\" MinQuality=\"3\" MaxQuality=\"5\" />");
                    sw.WriteLine("        <Item Name=\"gunShotgunT2PumpShotgun\" SecondaryName=\"Pump Shotgun\" MinCount=\"1\" MaxCount=\"1\" MinQuality=\"3\" MaxQuality=\"5\" />");
                    sw.WriteLine("        <Item Name=\"gunRifleT3SniperRifle\" SecondaryName=\"Sniper Rifle\" MinCount=\"1\" MaxCount=\"1\" MinQuality=\"2\" MaxQuality=\"5\" />");
                    sw.WriteLine("        <Item Name=\"gunExplosivesT3RocketLauncher\" SecondaryName=\"Rocket Launcher\" MinCount=\"1\" MaxCount=\"1\" MinQuality=\"2\" MaxQuality=\"5\" />");
                    sw.WriteLine("        <Item Name=\"ammoRocketHE\" SecondaryName=\"HE Rocket\" MinCount=\"5\" MaxCount=\"10\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"ammo44MagnumBulletHP\" SecondaryName=\"HP 44 Magnum Ammo\" MinCount=\"25\" MaxCount=\"50\" MinQuality=\"1\" MaxQuality=\"1\" />");
                    sw.WriteLine("        <Item Name=\"ammo762mmBulletHP\" SecondaryName=\"HP AK47 Ammo\" MinCount=\"50\" MaxCount=\"100\" MinQuality=\"1\" MaxQuality=\"1\" />");
                }
                sw.WriteLine("    </Items>");
                sw.WriteLine("</BloodmoonWarrior>");
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

        public static void Exec()
        {
            try
            {
                if (!BloodmoonStarted)
                {
                    if (PersistentOperations.BloodMoonSky())
                    {
                        BloodmoonStarted = true;
                        List<ClientInfo> _cInfoList = PersistentOperations.ClientList();
                        if (_cInfoList != null)
                        {
                            for (int i = 0; i < _cInfoList.Count; i++)
                            {
                                ClientInfo _cInfo = _cInfoList[i];
                                if (_cInfo != null)
                                {
                                    if (GameManager.Instance.World.Players.dict.ContainsKey(_cInfo.entityId))
                                    {
                                        EntityPlayer _player = PersistentOperations.GetEntityPlayer(_cInfo.playerId);
                                        if (_player != null && _player.IsSpawned() && _player.IsAlive() && _player.Died > 0 && _player.Progression.GetLevel() >= 10 && random.Next(0, 100) < Chance)
                                        {
                                            WarriorList.Add(_cInfo.playerId);
                                            KilledZombies.Add(_cInfo.playerId, 0);
                                            Phrases.Dict.TryGetValue(691, out string _phrase691);
                                            _phrase691 = _phrase691.Replace("{Count}", Zombie_Kills.ToString());
                                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase691 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (!PersistentOperations.BloodMoonSky())
                {
                    BloodmoonStarted = false;
                    RewardWarriors();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in BloodmoonWarrior.Exec: {0}", e.Message));
            }
        }

        public static void RewardWarriors()
        {
            try
            {
                List<string> _warriors = WarriorList;
                for (int i = 0; i < _warriors.Count; i++)
                {
                    string _warrior = _warriors[i];
                    EntityPlayer _player = PersistentOperations.GetEntityPlayer(_warrior);
                    if (_player != null && _player.IsAlive())
                    {
                        if (KilledZombies.TryGetValue(_warrior, out int _killedZ))
                        {
                            if (_killedZ >= Zombie_Kills)
                            {
                                ClientInfo _cInfo = PersistentOperations.GetClientInfoFromSteamId(_warrior);
                                if (_cInfo != null)
                                {
                                    Counter(_cInfo, Reward_Count);
                                    if (Reduce_Death_Count)
                                    {
                                        int _deathCount = _player.Died - 1;
                                        _player.Died = _deathCount;
                                        _player.bPlayerStatsChanged = true;
                                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerStats>().Setup(_player));
                                        Phrases.Dict.TryGetValue(692, out string _phrase692);
                                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase692 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                    else
                                    {
                                        Phrases.Dict.TryGetValue(693, out string _phrase693);
                                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase693 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in BloodmoonWarrior.RewardWarriors: {0}", e.Message));
            }
            WarriorList.Clear();
            KilledZombies.Clear();
        }

        private static void Counter(ClientInfo _cInfo, int _counter)
        {
            RandomItem(_cInfo);
            _counter--;
            if (_counter != 0)
            {
                RandomItem(_cInfo);
            }
        }

        private static void RandomItem(ClientInfo _cInfo)
        {
            try
            {
                string _randomItem = List.RandomObject();
                ItemValue _itemValue = new ItemValue(ItemClass.GetItem(_randomItem, false).type, false);
                if (Dict.TryGetValue(_randomItem, out int[]  _itemData))
                {
                    int _count = 1;
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
                    World world = GameManager.Instance.World;
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
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in BloodmoonWarrior.RandomItem: {0}", e.Message));
            }
        }
    }
}
