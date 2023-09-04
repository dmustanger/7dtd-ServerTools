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
        public static int Zombie_Kills = 10, Chance = 50, Reward_Count = 1, Level_Required = 10;

        public static List<int> WarriorList = new List<int>();
        public static Dictionary<int, int> KilledZombies = new Dictionary<int, int>();

        private static readonly Dictionary<string, string[]> Dict = new Dictionary<string, string[]>();

        private const string file = "BloodmoonWarrior.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static readonly System.Random Random = new System.Random();
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);

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
                    Log.Error("[SERVERTOOLS] Failed loading {0}: {1}", file, e.Message);
                    return;
                }
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                if (childNodes == null)
                {
                    return;
                }
                Dict.Clear();
                if (childNodes[0] != null && childNodes[0].OuterXml.Contains("Version") && childNodes[0].OuterXml.Contains(Config.Version))
                {
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        if (childNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement line = (XmlElement)childNodes[i];
                        if (!line.HasAttributes)
                        {
                            continue;
                        }
                        if (line.HasAttribute("Name") && line.HasAttribute("SecondaryName") && line.HasAttribute("MinCount") && line.HasAttribute("MaxCount") &&
                            line.HasAttribute("MinQuality") && line.HasAttribute("MaxQuality"))
                        {
                            string name = line.GetAttribute("Name");
                            if (name == "")
                            {
                                continue;
                            }
                            if (!GeneralOperations.IsValidItem(name))
                            {
                                Log.Out("[SERVERTOOLS] Ignoring BloodmoonWarrior.xml entry. Item name not found: {0}", name);
                                continue;
                            }
                            if (!int.TryParse(line.GetAttribute("MinCount"), out int minCount))
                            {
                                Log.Out("[SERVERTOOLS] Ignoring BloodmoonWarrior.xml entry because of invalid (non-numeric) value for 'MinCount' attribute: {0}", line.OuterXml);
                                continue;
                            }
                            if (!int.TryParse(line.GetAttribute("MaxCount"), out int maxCount))
                            {
                                Log.Out("[SERVERTOOLS] Ignoring BloodmoonWarrior.xml entry because of invalid (non-numeric) value for 'MaxCount' attribute: {0}", line.OuterXml);
                                continue;
                            }
                            if (!int.TryParse(line.GetAttribute("MinQuality"), out int minQuality))
                            {
                                Log.Out("[SERVERTOOLS] Ignoring BloodmoonWarrior.xml entry because of invalid (non-numeric) value for 'MinQuality' attribute: {0}", line.OuterXml);
                                continue;
                            }
                            if (!int.TryParse(line.GetAttribute("MaxQuality"), out int maxQuality))
                            {
                                Log.Out("[SERVERTOOLS] Ignoring BloodmoonWarrior.xml entry because of invalid (non-numeric) value for 'MaxQuality' attribute: {0}", line.OuterXml);
                                continue;
                            }
                            ItemValue itemValue = ItemClass.GetItem(name, false);
                            if (minCount > itemValue.ItemClass.Stacknumber.Value)
                            {
                                minCount = itemValue.ItemClass.Stacknumber.Value;
                            }
                            else if (minCount < 1)
                            {
                                minCount = 1;
                            }
                            if (maxCount > itemValue.ItemClass.Stacknumber.Value)
                            {
                                maxCount = itemValue.ItemClass.Stacknumber.Value;
                            }
                            else if (maxCount < 1)
                            {
                                maxCount = 1;
                            }
                            int exchange;
                            if (minCount > maxCount)
                            {
                                exchange = maxCount;
                                maxCount = minCount;
                                minCount = exchange;
                            }
                            if (minQuality > maxQuality)
                            {
                                exchange = maxQuality;
                                maxQuality = minQuality;
                                minQuality = exchange;
                            }
                            string secondaryname;
                            if (line.HasAttribute("SecondaryName"))
                            {
                                secondaryname = line.GetAttribute("SecondaryName");
                            }
                            else
                            {
                                secondaryname = name;
                            }
                            if (!Dict.ContainsKey(name))
                            {
                                string[] c = new string[] { secondaryname, minCount.ToString(), maxCount.ToString(), minQuality.ToString(), maxQuality.ToString() };
                                Dict.Add(name, c);
                            }
                        }
                    }
                }
                else
                {
                    XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                    if (nodeList != null)
                    {
                        Timers.UpgradeBloodmoonWarriorXml(nodeList);
                        //UpgradeXml(nodeList);
                        Log.Out("[SERVERTOOLS] The existing BloodmoonWarrior.xml was too old or misconfigured. File deleted and rebuilt for version {0}", Config.Version);
                        return;
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
                    Log.Out("[SERVERTOOLS] Error in BloodmoonWarrior.LoadXml: {0}", e.Message);
                }
            }
        }

        private static void UpdateXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<BloodmoonWarrior>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- <Item Name=\"gunPistolExample\" SecondaryName=\"pistol\" MinCount=\"1\" MaxCount=\"1\" MinQuality=\"3\" MaxQuality=\"3\" /> -->");
                    if (Dict.Count > 0)
                    {
                        foreach (KeyValuePair<string, string[]> kvp in Dict)
                        {
                            sw.WriteLine("    <Item Name=\"{0}\" SecondaryName=\"{1}\" MinCount=\"{2}\" MaxCount=\"{3}\" MinQuality=\"{4}\" MaxQuality=\"{5}\" />", kvp.Key, kvp.Value[0], kvp.Value[1], kvp.Value[2], kvp.Value[3], kvp.Value[4]);
                        }
                    }
                    sw.WriteLine("</BloodmoonWarrior>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in BloodmoonWarrior.UpdateXml: {0}", e.Message);
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

        public static void Exec()
        {

            try
            {

                if (!BloodmoonStarted)
                {
                    if (GeneralOperations.IsBloodmoon())
                    {
                        BloodmoonStarted = true;
                        List<ClientInfo> clientList = GeneralOperations.ClientList();
                        if (clientList == null || clientList.Count == 0)
                        {
                            return;
                        }
                        ClientInfo cInfo;
                        EntityPlayer player;
                        for (int i = 0; i < clientList.Count; i++)
                        {
                            cInfo = clientList[i];
                            if (cInfo == null)
                            {
                                continue;
                            }
                            player = GeneralOperations.GetEntityPlayer(cInfo.entityId);
                            if (player != null && player.IsSpawned() && player.IsAlive() && player.Died > 0 && player.Progression.GetLevel() >= 10 && Random.Next(0, 100) <= Chance)
                            {
                                WarriorList.Add(cInfo.entityId);
                                KilledZombies.Add(cInfo.entityId, 0);
                                Phrases.Dict.TryGetValue("BloodmoonWarrior1", out string phrase);
                                phrase = phrase.Replace("{Count}", Zombie_Kills.ToString());
                                ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                }
                else if (!GeneralOperations.IsBloodmoon())
                {
                    BloodmoonStarted = false;
                    RewardWarriors();
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in BloodmoonWarrior.Exec: {0}", e.Message);
            }

        }

        public static void RewardWarriors()
        {
            try
            {
                List<int> warriors = WarriorList;
                EntityPlayer player;
                ClientInfo cInfo;
                for (int i = 0; i < warriors.Count; i++)
                {
                    int warrior = warriors[i];
                    player = GeneralOperations.GetEntityPlayer(warrior);
                    if (player == null || !player.IsAlive())
                    {
                        continue;
                    }
                    if (KilledZombies.TryGetValue(warrior, out int _killedZ))
                    {
                        if (_killedZ >= Zombie_Kills)
                        {
                            cInfo = GeneralOperations.GetClientInfoFromEntityId(warrior);
                            if (cInfo == null)
                            {
                                continue;
                            }
                            Counter(cInfo, Reward_Count);
                            if (Reduce_Death_Count)
                            {
                                if (player.Died > 0)
                                {
                                    player.Died = player.Died - 1;
                                    player.bPlayerStatsChanged = true;
                                    cInfo.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerStats>().Setup(player));
                                    Phrases.Dict.TryGetValue("BloodmoonWarrior2", out string phrase);
                                    ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("BloodmoonWarrior3", out string phrase);
                                ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in BloodmoonWarrior.RewardWarriors: {0}", e.Message);
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
                System.Timers.Timer singleUseTimer = new System.Timers.Timer(500)
                {
                    AutoReset = false
                };
                singleUseTimer.Start();
                singleUseTimer.Elapsed += (sender, e) =>
                {
                    Counter(_cInfo, _counter);
                    singleUseTimer.Stop();
                    singleUseTimer.Close();
                    singleUseTimer.Dispose();
                };
            }
        }

        private static void RandomItem(ClientInfo _cInfo)
        {
            try
            {
                string randomItem = List.RandomObject();
                if (Dict.TryGetValue(randomItem, out string[] item))
                {
                    int minCount = int.Parse(item[1]);
                    int maxCount = int.Parse(item[2]);
                    int minQuality = int.Parse(item[3]);
                    int maxQuality = int.Parse(item[4]);
                    int count = Random.Next(minCount, maxCount + 1);
                    int quality = Random.Next(minCount, maxCount + 1);
                    ItemValue itemValue = new ItemValue(ItemClass.GetItem(randomItem, false).type, quality, quality, true, null, 1f);
                    if (itemValue == null)
                    {
                        return;
                    }
                    World world = GameManager.Instance.World;
                    var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                    {
                        entityClass = EntityClass.FromString("item"),
                        id = EntityFactory.nextEntityID++,
                        itemStack = new ItemStack(itemValue, count),
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
                Log.Out("[SERVERTOOLS] Error in BloodmoonWarrior.RandomItem: {0}", e.Message);
            }
        }

        public static void UpgradeXml(XmlNodeList nodeList)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                File.Delete(FilePath);
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<BloodmoonWarrior>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- <Item Name=\"gunPistolExample\" SecondaryName=\"pistol\" MinCount=\"1\" MaxCount=\"1\" MinQuality=\"3\" MaxQuality=\"3\" /> -->");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType == XmlNodeType.Comment)
                        {
                            if (!nodeList[i].OuterXml.Contains("<!-- <Item Name=\"gunPistolExample\"") && !nodeList[i].OuterXml.Contains("<!-- Do not forget") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Version") && !nodeList[i].OuterXml.Contains("<Item Name=\"\""))
                            {
                                sw.WriteLine(nodeList[i].OuterXml);
                            }
                        }
                    }
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)nodeList[i];
                            if (line.HasAttributes && line.Name == "Item")
                            {
                                string name = "", secondaryName = "", minCount = "", maxCount = "", minQuality = "", maxQuality = "";
                                if (line.HasAttribute("Name"))
                                {
                                    name = line.GetAttribute("Name");
                                }
                                if (line.HasAttribute("SecondaryName"))
                                {
                                    secondaryName = line.GetAttribute("SecondaryName");
                                }
                                if (line.HasAttribute("MinCount"))
                                {
                                    minCount = line.GetAttribute("MinCount");
                                }
                                if (line.HasAttribute("MaxCount"))
                                {
                                    maxCount = line.GetAttribute("MaxCount");
                                }
                                if (line.HasAttribute("MinQuality"))
                                {
                                    minQuality = line.GetAttribute("MinQuality");
                                }
                                if (line.HasAttribute("MaxQuality"))
                                {
                                    maxQuality = line.GetAttribute("MaxQuality");
                                }
                                sw.WriteLine(string.Format("    <Item Name=\"{0}\" SecondaryName=\"{1}\" MinCount=\"{2}\" MaxCount=\"{3}\" MinQuality=\"{4}\" MaxQuality=\"{5}\" />", name, secondaryName, minCount, maxCount, minQuality, maxQuality));
                            }
                        }
                    }
                    sw.WriteLine("</BloodmoonWarrior>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in BloodmoonWarrior.UpgradeXml: {0}", e.Message);
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}
