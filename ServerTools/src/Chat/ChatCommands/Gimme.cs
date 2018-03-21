using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    public class Gimme
    {
        public static bool IsEnabled = false, IsRunning = false, Always_Show_Response = false,
            Zombies = false;
        public static int Delay_Between_Uses = 60;
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
                        string _secondaryname;
                        if (_line.HasAttribute("secondaryname"))
                        {
                            _secondaryname = _line.GetAttribute("secondaryname");
                        }
                        else
                        {
                            _secondaryname = _item;
                        }
                        ItemValue _itemValue = ItemClass.GetItem(_item, true);
                        if (_itemValue.type == ItemValue.None.type)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Gimme item not found.: {0}", _item));
                            continue;
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
                    sw.WriteLine("        <item item=\"bottledWater\" secondaryname=\"Bottled Water\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"beer\" secondaryname=\"Beer\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"keystoneBlock\" secondaryname=\"Land Claim Block\" min=\"1\" max=\"1\" />");
                    sw.WriteLine("        <item item=\"canChicken\" secondaryname=\"Can of Chicken\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"canChili\" secondaryname=\"Can of Chilli\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"corn\" secondaryname=\"Corn\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"potato\" secondaryname=\"Potato\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"firstAidBandage\" secondaryname=\"First Aid Bandage\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"painkillers\" secondaryname=\"Pain Killers\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"scrapBrass\" secondaryname=\"Scrap Brass\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"antibiotics\" secondaryname=\"Antibiotics\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"moldyBread\" secondaryname=\"Moldy Bread\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"oil\" secondaryname=\"Oil\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"cornMeal\" secondaryname=\"Cornmeal\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"blueberries\" secondaryname=\"Blueberries\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"canHam\" secondaryname=\"Can of Hame\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"coffeeBeans\" secondaryname=\"Coffee Beans\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"casinoCoin\" secondaryname=\"Casino Coins\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"boneShiv\" secondaryname=\"Bone Shiv\" min=\"1\" max=\"1\" />");
                    sw.WriteLine("        <item item=\"canDogfood\" secondaryname=\"Can of Dog Food\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"animalHide\" secondaryname=\"Animal Hide\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"blueberryPie\" secondaryname=\"Blueberry Pie\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"canPeas\" secondaryname=\"Can of Peas\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"canStock\" secondaryname=\"Can of Stock\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"canCatfood\" secondaryname=\"Can of Cat Food\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"scrapIron\" secondaryname=\"Scrap Iron\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"goldenrodPlant\" secondaryname=\"Goldenrod Plant\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"clayLump\" secondaryname=\"Lumps of Clay\" min=\"1\" max=\"5\" />");
                    sw.WriteLine("        <item item=\"rottingFlesh\" secondaryname=\"Rotting Flesh\" min=\"1\" max=\"5\" />");
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
            if (Delay_Between_Uses < 1)
            {
                _GiveItem(_cInfo, _announce);
            }
            else
            {
                Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                if (p == null || p.LastGimme == null)
                {
                    _GiveItem(_cInfo, _announce);
                }
                else
                {
                    TimeSpan varTime = DateTime.Now - p.LastGimme;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int _timepassed = (int)fractionalMinutes;
                    if (_timepassed >= Delay_Between_Uses)
                    {
                        _GiveItem(_cInfo, _announce);
                    }
                    else
                    {
                        int _timeleft = Delay_Between_Uses - _timepassed;
                        string _phrase6;
                        if (!Phrases.Dict.TryGetValue(6, out _phrase6))
                        {
                            _phrase6 = "{PlayerName} you can only use /gimme once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                        }
                        _phrase6 = _phrase6.Replace("{PlayerName}", _playerName);
                        _phrase6 = _phrase6.Replace("{DelayBetweenUses}", Delay_Between_Uses.ToString());
                        _phrase6 = _phrase6.Replace("{TimeRemaining}", _timeleft.ToString());
                        if (_announce)
                        {
                            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase6), "Server", false, "", false);
                        }
                        else
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase6), "Server", false, "", false));
                        }
                    }
                }
            }
        }

        private static void _GiveItem(ClientInfo _cInfo, bool _announce)
        {
            if (Zombies)
            {
                int itemOrEntity = random.Next(1, 7);
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
                _quality = random.Next(1, 601);
                _itemValue.Quality = _quality;
            }
            int[] _counts;
            if (dict.TryGetValue(_randomItem, out _counts))
            {
                int _count = random.Next(_counts[0], _counts[1] + 1);
                ItemStack _itemDrop = new ItemStack(_itemValue, _count);
                ItemValue itemValue;
                itemValue = new ItemValue(ItemClass.GetItem(_randomItem).type, _quality, _quality, true);

                if (Equals(itemValue, ItemValue.None))
                {
                    SdtdConsole.Instance.Output(string.Format("Unable to find item {0}", _randomItem));
                    return;
                }
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
                    _phrase7 = "{PlayerName} has received {ItemCount} {ItemName}.";
                }
                _phrase7 = _phrase7.Replace("{PlayerName}", _cInfo.playerName);
                _phrase7 = _phrase7.Replace("{ItemCount}", _count.ToString());
                string _name;
                if (dict1.TryGetValue(_randomItem, out _name))
                {
                    _phrase7 = _phrase7.Replace("{ItemName}", _name);
                }
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase7), "Server", false, "", false);
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase7), "Server", false, "", false));
                }
                PersistentContainer.Instance.Players[_cInfo.playerId, true].LastGimme = DateTime.Now;
                PersistentContainer.Instance.Save();
            }
        }

        private static void RandomZombie(ClientInfo _cInfo, bool _announce)
        {
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            Vector3 pos = _player.GetPosition();
            float x = (int)pos.x;
            float y = (int)pos.y;
            float z = (int)pos.z;
            int _x, _y, _z;
            bool posFound = true;
            posFound = GameManager.Instance.World.FindRandomSpawnPointNearPosition(new Vector3((float)x, (float)y, (float)z), 15, out _x, out _y, out _z, new Vector3((float)5, (float)5, (float)5), true);
            if (!posFound)
            {
                posFound = GameManager.Instance.World.FindRandomSpawnPointNearPosition(new Vector3((float)x, (float)y, (float)z), 15, out _x, out _y, out _z, new Vector3((float)5 + 5, (float)5 + 5, (float)5 + 5), true);
            }
            if (posFound)
            {
                Log.Out("[SERVERTOOLS] Spawning zombie for player's gimme");
                int _rndZ = random.Next(1, 4);
                if (_rndZ == 1)
                {
                    SdtdConsole.Instance.ExecuteSync(string.Format("sea 4 {0} {1} {2}", _x, _y, _z), (ClientInfo)null);
                }
                if (_rndZ == 2)
                {
                    SdtdConsole.Instance.ExecuteSync(string.Format("sea 9 {0} {1} {2}", _x, _y, _z), (ClientInfo)null);
                }
                else
                {
                    SdtdConsole.Instance.ExecuteSync(string.Format("sea 11 {0} {1} {2}", _x, _y, _z), (ClientInfo)null);
                }
                string _phrase807;
                if (!Phrases.Dict.TryGetValue(807, out _phrase807))
                {
                    _phrase807 = "OH NO! How did that get in there? You have received a zombie.";
                }
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase807), "Server", false, "", false);
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase807), "Server", false, "", false));
                }
                PersistentContainer.Instance.Players[_cInfo.playerId, true].LastGimme = DateTime.Now;
                PersistentContainer.Instance.Save();
            }
            else
            {
                string _phrase808;
                if (!Phrases.Dict.TryGetValue(808, out _phrase808))
                {
                    _phrase808 = "No spawn points were found near you. Move locations and try /reward again.";
                }
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase808), "Server", false, "", false));
            }
        }
    }
}