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
        public static string Command_gimme = "gimme", Command_gimmie = "gimmie", Zombie_Id = "4,9,11";

        private static Dictionary<string, string[]> Dict = new Dictionary<string, string[]>();

        private const string file = "Gimme.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
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
                    Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", file, e.Message));
                    return;
                }
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                Dict.Clear();
                if (childNodes != null && (childNodes[0] != null && childNodes[0].OuterXml.Contains("Version") && childNodes[0].OuterXml.Contains(Config.Version)))
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
                            if (!int.TryParse(line.GetAttribute("MinCount"), out int minCount))
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Ignoring Gimme.xml entry because of invalid (non-numeric) value for 'MinCount' attribute: {0}", line.OuterXml));
                                continue;
                            }
                            if (!int.TryParse(line.GetAttribute("MaxCount"), out int maxCount))
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Ignoring Gimme.xml entry because of invalid (non-numeric) value for 'MaxCount' attribute: {0}", line.OuterXml));
                                continue;
                            }
                            if (!int.TryParse(line.GetAttribute("MinQuality"), out int minQuality))
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Ignoring Gimme.xml entry because of invalid (non-numeric) value for 'MinQuality' attribute: {0}", line.OuterXml));
                                continue;
                            }
                            if (!int.TryParse(line.GetAttribute("MaxQuality"), out int maxQuality))
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Ignoring Gimme.xml entry because of invalid (non-numeric) value for 'MaxQuality' attribute: {0}", line.OuterXml));
                                continue;
                            }

                            if (!GeneralOperations.IsValidItem(name))
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Ignoring Gimme.xml entry. Name not found: {0}", name));
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
                            string secondary;
                            if (line.HasAttribute("SecondaryName"))
                            {
                                secondary = line.GetAttribute("SecondaryName");
                            }
                            else
                            {
                                secondary = name;
                            }
                            if (!Dict.ContainsKey(name))
                            {
                                string[] c = new string[] { secondary, minCount.ToString(), maxCount.ToString(), minQuality.ToString(), maxQuality.ToString() };
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
                        File.Delete(FilePath);
                        UpgradeXml(nodeList);
                        return;
                    }
                    File.Delete(FilePath);
                    UpdateXml();
                    return;
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
                    Log.Out(string.Format("[SERVERTOOLS] Error in Gimme.LoadXml: {0}", e.Message));
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
                    sw.WriteLine("<Gimme>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Secondary name is what will show in chat instead of the item name -->");
                    sw.WriteLine("    <!-- Items that do not require a quality should be set to 1 for both min and max -->");
                    sw.WriteLine("    <!-- <Item Name=\"drinkJarBoiledWater\" SecondaryName=\"boiled water\" MinCount=\"1\" MaxCount=\"6\" MinQuality=\"1\" MaxQuality=\"1\" /> -->");
                    sw.WriteLine("    <Item Name=\"\" SecondaryName=\"\" MinCount=\"\" MaxCount=\"\" MinQuality=\"\" MaxQuality=\"\" />");
                    if (Dict.Count > 0)
                    {
                        foreach (KeyValuePair<string, string[]> kvp in Dict)
                        {
                            sw.WriteLine(string.Format("    <Item Name=\"{0}\" SecondaryName=\"{1}\" MinCount=\"{2}\" MaxCount=\"{3}\" MinQuality=\"{4}\" MaxQuality=\"{5}\" />", kvp.Key, kvp.Value[0], kvp.Value[1], kvp.Value[2], kvp.Value[3], kvp.Value[4]));
                        }
                    }
                    sw.WriteLine("</Gimme>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Gimme.UpdateXml: {0}", e.Message));
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

        public static void Exec(ClientInfo _cInfo)
        {
            try
            {
                if (Dict.Count > 0)
                {
                    if (Delay_Between_Uses < 1)
                    {
                        if (Command_Cost >= 1 && Wallet.IsEnabled)
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
                        DateTime lastgimme = DateTime.Now;
                        if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastGimme != null)
                        {
                            lastgimme = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastGimme;
                        }
                        TimeSpan varTime = DateTime.Now - lastgimme;
                        double fractionalMinutes = varTime.TotalMinutes;
                        int timepassed = (int)fractionalMinutes;
                        if (ReservedSlots.IsEnabled)
                        {
                            if (ReservedSlots.Reduced_Delay && (ReservedSlots.Dict.ContainsKey(_cInfo.PlatformId.CombinedString) || ReservedSlots.Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString)))
                            {
                                if (ReservedSlots.Dict.TryGetValue(_cInfo.PlatformId.CombinedString, out DateTime dt))
                                {
                                    if (DateTime.Now < dt)
                                    {
                                        int delay = Delay_Between_Uses / 2;
                                        Time(_cInfo, timepassed, delay);
                                        return;
                                    }
                                }
                                else if (ReservedSlots.Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out dt))
                                {
                                    if (DateTime.Now < dt)
                                    {
                                        int delay = Delay_Between_Uses / 2;
                                        Time(_cInfo, timepassed, delay);
                                        return;
                                    }
                                }
                            }
                        }
                        if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ReducedDelay)
                        {
                            int delay = Delay_Between_Uses / 2;
                            Time(_cInfo, timepassed, delay);
                            return;
                        }
                        Time(_cInfo, timepassed, Delay_Between_Uses);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Gimme.Exec: {0}", e.Message));
            }
        }

        private static void Time(ClientInfo _cInfo, int _timepassed, int _delay)
        {
            try
            {
                if (_timepassed >= _delay)
                {
                    if (Command_Cost >= 1 && Wallet.IsEnabled)
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
                    int timeleft = _delay - _timepassed;
                    Phrases.Dict.TryGetValue("Gimme1", out string phrase);
                    phrase = phrase.Replace("{DelayBetweenUses}", _delay.ToString());
                    phrase = phrase.Replace("{TimeRemaining}", timeleft.ToString());
                    phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    phrase = phrase.Replace("{Command_gimme}", Command_gimme);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Gimme.Time: {0}", e.Message));
            }
        }

        private static void CommandCost(ClientInfo _cInfo)
        {
            try
            {
                if (Wallet.GetCurrency(_cInfo.CrossplatformId.CombinedString) >= Command_Cost)
                {
                    ZCheck(_cInfo);
                }
                else
                {
                    Phrases.Dict.TryGetValue("Gimme3", out string phrase);
                    phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Gimme.CommandCost: {0}", e.Message));
            }
        }

        private static void ZCheck(ClientInfo _cInfo)
        {
            try
            {
                if (Zombies)
                {
                    int itemOrEntity = new System.Random().Next(1, 9);
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
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Gimme.ZCheck: {0}", e.Message));
            }
        }

        private static void RandomItem(ClientInfo _cInfo)
        {
            try
            {
                string randomItem = List.RandomObject();
                if (Dict.TryGetValue(randomItem, out string[] item))
                {
                    int.TryParse(item[1], out int minCount);
                    int.TryParse(item[2], out int maxCount);
                    int.TryParse(item[3], out int minQuality);
                    int.TryParse(item[4], out int maxQuality);
                    int count = new System.Random().Next(minCount, maxCount + 1);
                    int quality = new System.Random().Next(minQuality, maxQuality + 1);
                    ItemValue itemValue = new ItemValue(ItemClass.GetItem(randomItem, false).type);
                    itemValue.Quality = 0;
                    itemValue.Modifications = new ItemValue[0];
                    itemValue.CosmeticMods = new ItemValue[0];
                    int modSlots = (int)EffectManager.GetValue(PassiveEffects.ModSlots, itemValue, itemValue.Quality - 1);
                    if (modSlots > 0)
                    {
                        itemValue.Modifications = new ItemValue[modSlots];
                    }
                    itemValue.CosmeticMods = new ItemValue[itemValue.ItemClass.HasAnyTags(ItemClassModifier.CosmeticItemTags) ? 1 : 0];
                    if (itemValue.HasQuality)
                    {
                        if (quality > 0)
                        {
                            itemValue.Quality = quality;
                        }
                        else
                        {
                            itemValue.Quality = 1;
                        }
                    }
                    World world = GameManager.Instance.World;
                    EntityItem entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
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
                    if (Command_Cost >= 1 && Wallet.IsEnabled)
                    {
                        Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, Command_Cost);
                    }
                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastGimme = DateTime.Now;
                    PersistentContainer.DataChange = true;
                    Phrases.Dict.TryGetValue("Gimme2", out string _phrase);
                    _phrase = _phrase.Replace("{ItemCount}", count.ToString());
                    if (item[0] != "")
                    {
                        _phrase = _phrase.Replace("{ItemName}", item[0]);
                    }
                    else
                    {
                        _phrase = _phrase.Replace("{ItemName}", itemValue.ItemClass.GetLocalizedItemName() ?? itemValue.ItemClass.Name);
                    }
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Gimme.RandomItem: {0}", e.Message));
            }
        }

        private static void RandomZombie(ClientInfo _cInfo)
        {
            try
            {
                if (Zombie_Id != "")
                {
                    if (Zombie_Id.Contains(","))
                    {
                        string[] zombieIds = Zombie_Id.Split(',');
                        int count = new System.Random().Next(1, zombieIds.Length + 1);
                        string zId = zombieIds[count];
                        if (int.TryParse(zId, out int zombieId))
                        {
                            SdtdConsole.Instance.ExecuteSync(string.Format("st-Ser {0} r.15 {1}", _cInfo.CrossplatformId.CombinedString, zombieId), null);
                            Log.Out(string.Format("[SERVERTOOLS] Gimme result spawned an entity for id '{0}' '{1}' named '{2}'", _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName));
                            if (Command_Cost >= 1 && Wallet.IsEnabled)
                            {
                                Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, Command_Cost);
                            }
                            PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastGimme = DateTime.Now;
                            PersistentContainer.DataChange = true;
                            Phrases.Dict.TryGetValue("Gimme4", out string phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                            SdtdConsole.Instance.ExecuteSync(string.Format("st-Ser {0} r.15 {1}", _cInfo.CrossplatformId.CombinedString, _zombieId), null);
                            Log.Out(string.Format("[SERVERTOOLS] Gimme result spawned an entity for id '{0}' '{1}' named '{2}'", _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName));
                            if (Command_Cost >= 1 && Wallet.IsEnabled)
                            {
                                Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, Command_Cost);
                            }
                            PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastGimme = DateTime.Now;
                            PersistentContainer.DataChange = true;
                            Phrases.Dict.TryGetValue("Gimme4", out string _phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Gimme.RandomZombie: {0}", e.Message));
            }
        }

        private static void UpgradeXml(XmlNodeList nodeList)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<Gimme>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Secondary name is what will show in chat instead of the item name -->");
                    sw.WriteLine("    <!-- Items that do not require a quality should be set to 1 for both min and max -->");
                    sw.WriteLine("    <!-- <Item Name=\"drinkJarBoiledWater\" SecondaryName=\"boiled water\" MinCount=\"1\" MaxCount=\"6\" MinQuality=\"1\" MaxQuality=\"1\" /> -->");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (!nodeList[i].OuterXml.Contains("<!-- Secondary name") &&
                            !nodeList[i].OuterXml.Contains("<!-- Items that do") && !nodeList[i].OuterXml.Contains("<!-- <Item Name=\"drinkJarBoiledWater\"") &&
                            !nodeList[i].OuterXml.Contains("<Item Name=\"\"") && !nodeList[i].OuterXml.Contains("<!-- <Version"))
                        {
                            sw.WriteLine(nodeList[i].OuterXml);
                        }
                    }
                    sw.WriteLine("    <Item Name=\"\" SecondaryName=\"\" MinCount=\"\" MaxCount=\"\" MinQuality=\"\" MaxQuality=\"\" />");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)nodeList[i];
                            if (line.HasAttributes && line.Name == "Item")
                            {
                                string name = "", secondary = "", minCount = "", maxCount = "", minQuality = "", maxQuality = "";
                                if (line.HasAttribute("Name"))
                                {
                                    name = line.GetAttribute("Name");
                                }
                                if (line.HasAttribute("SecondaryName"))
                                {
                                    secondary = line.GetAttribute("SecondaryName");
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
                                sw.WriteLine(string.Format("    <Item Name=\"{0}\" SecondaryName=\"{1}\" MinCount=\"{2}\" MaxCount=\"{3}\" MinQuality=\"{4}\" MaxQuality=\"{5}\" />", name, secondary, minCount, maxCount, minQuality, maxQuality));
                            }
                        }
                    }
                    sw.WriteLine("</Gimme>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Gimme.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}