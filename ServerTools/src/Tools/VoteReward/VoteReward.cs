using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    class VoteReward
    {
        public static bool IsEnabled = false, IsRunning = false, RandomListRunning = false, Reward_Entity = false;
        public static int Reward_Count = 1, Delay_Between_Uses = 24, Entity_Id = 73, Weekly_Votes = 5;
        public static string Your_Voting_Site = "https://7daystodie-servers.com/server/12345", API_Key = "xxxxxxxx", Command_reward = "reward";
        
        private static bool PosFound = false;

        private static Dictionary<string, int[]> Dict = new Dictionary<string, int[]>();
        private static List<string> Items = new List<string>();

        private const string file = "VoteReward.xml";
        private static readonly string FilePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        private static readonly System.Random Random = new System.Random();

        private static XmlNodeList OldNodeList;

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        public static void Unload()
        {
            Dict.Clear();
            Items.Clear();
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
                    Items.Clear();
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
                                else if (line.HasAttribute("ItemOrBlock") && line.HasAttribute("MinCount") && line.HasAttribute("MaxCount") &&
                                    line.HasAttribute("MinQuality") && line.HasAttribute("MaxQuality"))
                                {
                                    if (!int.TryParse(line.GetAttribute("MinCount"), out int minCount))
                                    {
                                        Log.Out(string.Format("[SERVERTOOLS] Ignoring VoteReward.xml entry. Invalid (non-numeric) value for 'MinCount' attribute: {0}", line.OuterXml));
                                        continue;
                                    }
                                    if (!int.TryParse(line.GetAttribute("MaxCount"), out int maxCount))
                                    {
                                        Log.Out(string.Format("[SERVERTOOLS] Ignoring VoteReward.xml entry. Invalid (non-numeric) value for 'MaxCount' attribute: {0}", line.OuterXml));
                                        continue;
                                    }
                                    if (!int.TryParse(line.GetAttribute("MinQuality"), out int minQuality))
                                    {
                                        Log.Out(string.Format("[SERVERTOOLS] Ignoring VoteReward.xml entry. Invalid (non-numeric) value for 'MinQuality' attribute: {0}", line.OuterXml));
                                        continue;
                                    }
                                    if (!int.TryParse(line.GetAttribute("MaxQuality"), out int maxQuality))
                                    {
                                        Log.Out(string.Format("[SERVERTOOLS] Ignoring VoteReward.xml entry. Invalid (non-numeric) value for 'MaxQuality' attribute: {0}", line.OuterXml));
                                        continue;
                                    }
                                    string item = line.GetAttribute("ItemOrBlock");
                                    if (item == "WalletCoin")
                                    {
                                        if (Wallet.IsEnabled)
                                        {
                                            if (minCount < 1)
                                            {
                                                minCount = 1;
                                            }
                                        }
                                        else
                                        {
                                            Log.Out(string.Format("[SERVERTOOLS] Ignoring VoteReward.xml entry. Wallet tool is not enabled: {0}", line.OuterXml));
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        ItemValue itemValue = ItemClass.GetItem(item, false);
                                        if (itemValue.type == ItemValue.None.type)
                                        {
                                            Log.Out(string.Format("[SERVERTOOLS] Ignoring VoteReward.xml entry. Item not found: {0}", item));
                                            continue;
                                        }
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
                                    }
                                    if (minQuality < 1)
                                    {
                                        minQuality = 1;
                                    }
                                    if (maxQuality < 1)
                                    {
                                        maxQuality = 1;
                                    }
                                    if (!Dict.ContainsKey(item))
                                    {
                                        int[] c = new int[] { minCount, maxCount, minQuality, maxQuality };
                                        Dict.Add(item, c);
                                    }
                                }
                            }
                        }
                    }
                    if (Dict.Count > 0)
                    {
                        Items = new List<string>(Dict.Keys);
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
                            Log.Out(string.Format("[SERVERTOOLS] The existing VoteReward.xml was too old or misconfigured. File deleted and rebuilt for version {0}", Config.Version));
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
                    Log.Out(string.Format("[SERVERTOOLS] Error in VoteReward.LoadXml: {0}", e.Message));
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
                    sw.WriteLine("<VoteRewards>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("    <!-- Items that do not require a quality should be set to 0 or 1 for min and max -->");
                    sw.WriteLine("    <!-- <Reward ItemOrBlock=\"meleeToolTorch\" MinCount=\"5\" MaxCount=\"10\" MinQuality=\"1\" MaxQuality=\"1\" /> -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    if (Dict.Count > 0)
                    {
                        foreach (KeyValuePair<string, int[]> kvp in Dict)
                        {
                            sw.WriteLine(string.Format("    <Reward ItemOrBlock=\"{0}\" MinCount=\"{1}\" MaxCount=\"{2}\" MinQuality=\"{3}\" MaxQuality=\"{4}\" />", kvp.Key, kvp.Value[0], kvp.Value[1], kvp.Value[2], kvp.Value[3]));
                        }
                    }
                    sw.WriteLine("</VoteRewards>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in VoteReward.UpdateXml: {0}", e.Message));
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

        public static void Check(ClientInfo _cInfo)
        {
            try
            {
                if (!Reward_Entity && Dict.Count == 0)
                {
                    Log.Out(string.Format("[SERVERTOOLS] No items available for reward. Check for an error in the VoteReward.xml file"));
                    Phrases.Dict.TryGetValue("VoteReward2", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
                if (Delay_Between_Uses == 0)
                {
                    CheckSite(_cInfo);
                }
                else
                {
                    DateTime lastVoteReward = DateTime.Now;
                    if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastVote != null)
                    {
                        lastVoteReward = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastVote;
                    }
                    TimeSpan varTime = DateTime.Now - lastVoteReward;
                    double fractionalHours = varTime.TotalHours;
                    int timepassed = (int)fractionalHours;
                    if (timepassed >= Delay_Between_Uses)
                    {
                        CheckSite(_cInfo);
                    }
                    else
                    {
                        int timeleft = Delay_Between_Uses - timepassed;
                        Phrases.Dict.TryGetValue("VoteReward1", out string phrase);
                        phrase = phrase.Replace("{DelayBetweenRewards}", Delay_Between_Uses.ToString());
                        phrase = phrase.Replace("{TimeRemaining}", timeleft.ToString());
                        phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                        phrase = phrase.Replace("{Command_reward}", Command_reward);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in VoteReward.Check: {0}", e.Message));
            }
        }

        private static void CheckSite(ClientInfo _cInfo)
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback += (send, certificate, chain, sslPolicyErrors) => { return true; };
                var VoteUrl = string.Format("https://7daystodie-servers.com/api/?object=votes&element=claim&key={0}&username={1}", Uri.EscapeUriString(API_Key), Uri.EscapeUriString(_cInfo.playerName));
                using (var NewVote = new WebClient())
                {
                    string VoteResult = NewVote.DownloadString(VoteUrl);
                    if (VoteResult == "0")
                    {
                        NoVote(_cInfo);
                    }
                    else if (VoteResult == "1")
                    {
                        try
                        {
                            if (Reward_Entity)
                            {
                                Entity(_cInfo);
                            }
                            else
                            {
                                ItemOrBlockCounter(_cInfo, Reward_Count);
                            }
                            PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastVote = DateTime.Now;
                            PersistentContainer.DataChange = true;
                        }
                        catch
                        {
                            Log.Error("[SERVERTOOLS] Vote reward tool failed to spawn the reward for players");
                        }
                    }
                    else if (VoteResult == "2")
                    {
                        Phrases.Dict.TryGetValue("VoteReward3", out string phrase);
                        phrase = phrase.Replace("{PlayerName}", _cInfo.playerName);
                        phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                        phrase = phrase.Replace("{Command_reward}", Command_reward);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch
            {
                Log.Error("[SERVERTOOLS] Vote reward tool failed to communicate with the voting website");
            }
        }

        private static void NoVote(ClientInfo _cInfo)
        {
            Phrases.Dict.TryGetValue("VoteReward4", out string phrase);
            phrase = phrase.Replace("{PlayerName}", _cInfo.playerName);
            phrase = phrase.Replace("{VoteSite}", Your_Voting_Site);
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
        }

        public static void ItemOrBlockCounter(ClientInfo _cInfo, int _counter)
        {
            try
            {
                ItemOrBlockRandom(_cInfo);
                _counter--;
                if (_counter != 0)
                {
                    ItemOrBlockCounter(_cInfo, _counter);
                }
                else
                {
                    if (Weekly_Votes > 1)
                    {
                        DateTime lastVoteWeek;
                        if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastVoteWeek != null)
                        {
                            lastVoteWeek = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastVoteWeek;
                        }
                        else
                        {
                            lastVoteWeek = DateTime.Now;
                        }
                        TimeSpan varTime = DateTime.Now - lastVoteWeek;
                        double fractionalDays = varTime.TotalDays;
                        int timepassed = (int)fractionalDays;
                        if (timepassed < 7)
                        {
                            int voteWeekCount = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].VoteWeekCount;
                            if (voteWeekCount + 1 == Weekly_Votes)
                            {
                                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].VoteWeekCount = 1;
                                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastVoteWeek = DateTime.Now;
                                ItemOrBlockRandom(_cInfo);
                                Phrases.Dict.TryGetValue("VoteReward5", out string phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].VoteWeekCount = voteWeekCount + 1;
                                int remainingVotes = Weekly_Votes - voteWeekCount + 1;
                                DateTime date2 = lastVoteWeek.AddDays(7);
                                Phrases.Dict.TryGetValue("VoteReward6", out string phrase);
                                phrase = phrase.Replace("{Value}", voteWeekCount + 1.ToString());
                                phrase = phrase.Replace("{Date}", lastVoteWeek.ToString());
                                phrase = phrase.Replace("{Value2}", remainingVotes.ToString());
                                phrase = phrase.Replace("{Date2}", date2.ToString());
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        else
                        {
                            PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].VoteWeekCount = 1;
                            PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastVoteWeek = DateTime.Now;
                            int remainingVotes = Weekly_Votes - 1;
                            DateTime date2 = DateTime.Now.AddDays(7);
                            Phrases.Dict.TryGetValue("VoteReward6", out string phrase);
                            phrase = phrase.Replace("{Value}", 1.ToString());
                            phrase = phrase.Replace("{Date}", lastVoteWeek.ToString());
                            phrase = phrase.Replace("{Value2}", remainingVotes.ToString());
                            phrase = phrase.Replace("{Date2}", date2.ToString());
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        PersistentContainer.DataChange = true;
                    }
                    Phrases.Dict.TryGetValue("VoteReward7", out string phrase1);
                    phrase1 = phrase1.Replace("{Value}", Delay_Between_Uses.ToString());
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    Phrases.Dict.TryGetValue("VoteReward8", out phrase1);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    Phrases.Dict.TryGetValue("VoteReward11", out phrase1);
                    phrase1 = phrase1.Replace("{PlayerName}", _cInfo.playerName);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in VoteReward.ItemOrBlockCounter: {0}", e.Message));
            }
        }

        private static void ItemOrBlockRandom(ClientInfo _cInfo)
        {
            try
            {
                string randomItem = Items.RandomObject();
                if (Dict.TryGetValue(randomItem, out int[] itemData))
                {
                    int count = Random.Next(itemData[0], itemData[1] + 1);
                    if (randomItem == "WalletCoin")
                    {
                        if (Wallet.IsEnabled)
                        {
                            Wallet.AddCurrency(_cInfo.CrossplatformId.CombinedString, count);
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("VoteReward12", out string phrase);
                            Log.Out(string.Format("[SERVERTOOLS] {0}", phrase));
                        }
                    }
                    else
                    {
                        int quality = Random.Next(itemData[2], itemData[3] + 1);
                        ItemValue _itemValue = new ItemValue(ItemClass.GetItem(randomItem).type, quality, quality, false, null, 1);
                        Give(_cInfo, _itemValue, count);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in VoteReward.ItemOrBlockRandom: {0}", e.Message));
            }
        }

        private static void Give(ClientInfo _cInfo, ItemValue _itemValue, int _count)
        {
            try
            {
                EntityPlayer player = PersistentOperations.GetEntityPlayer(_cInfo.entityId);
                if (player != null && player.IsSpawned())
                {
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
                Log.Out(string.Format("[SERVERTOOLS] Error in VoteReward.Give: {0}", e.Message));
            }
        }

        private static void Entity(ClientInfo _cInfo)
        {
            try
            {
                Entityspawn(_cInfo);
                if (Weekly_Votes > 0)
                {
                    DateTime lastVoteWeek = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastVoteWeek;
                    TimeSpan varTime = DateTime.Now - lastVoteWeek;
                    double fractionalDays = varTime.TotalDays;
                    int timepassed = (int)fractionalDays;
                    if (timepassed < 7)
                    {
                        int voteWeekCount = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].VoteWeekCount;
                        if (voteWeekCount + 1 == Weekly_Votes)
                        {
                            PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].VoteWeekCount = 1;
                            PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastVoteWeek = DateTime.Now;
                            Entityspawn(_cInfo);
                            Phrases.Dict.TryGetValue("VoteReward5", out string _phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].VoteWeekCount = voteWeekCount + 1;
                            int remainingVotes = Weekly_Votes - voteWeekCount + 1;
                            DateTime date2 = lastVoteWeek.AddDays(7);
                            Phrases.Dict.TryGetValue("VoteReward6", out string phrase);
                            phrase = phrase.Replace("{Value}", voteWeekCount + 1.ToString());
                            phrase = phrase.Replace("{Date}", lastVoteWeek.ToString());
                            phrase = phrase.Replace("{Value2}", remainingVotes.ToString());
                            phrase = phrase.Replace("{Date2}", date2.ToString());
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].VoteWeekCount = 1;
                        PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastVoteWeek = DateTime.Now;
                        int remainingVotes = Weekly_Votes - 1;
                        DateTime date2 = DateTime.Now.AddDays(7);
                        Phrases.Dict.TryGetValue("VoteReward6", out string phrase);
                        phrase = phrase.Replace("{Value}", 1.ToString());
                        phrase = phrase.Replace("{Date}", lastVoteWeek.ToString());
                        phrase = phrase.Replace("{Value2}", remainingVotes.ToString());
                        phrase = phrase.Replace("{Date2}", date2.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    PersistentContainer.DataChange = true;
                }
                Phrases.Dict.TryGetValue("VoteReward7", out string phrase1);
                phrase1 = phrase1.Replace("{Value}", Delay_Between_Uses.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in VoteReward.Entity: {0}", e.Message));
            }
        }

        private static void Entityspawn(ClientInfo _cInfo)
        {
            try
            {
                EntityPlayer player = PersistentOperations.GetEntityPlayer(_cInfo.entityId);
                if (player != null && player.IsSpawned())
                {
                    Vector3 pos = player.GetPosition();
                    float x = pos.x;
                    float y = pos.y;
                    float z = pos.z;
                    PosFound = true;
                    PosFound = GameManager.Instance.World.FindRandomSpawnPointNearPosition(new Vector3((float)x, (float)y, (float)z), 15, out int _x, out int _y, out int _z, new Vector3((float)5, (float)5, (float)5), true);
                    if (!PosFound)
                    {
                        PosFound = GameManager.Instance.World.FindRandomSpawnPointNearPosition(new Vector3((float)x, (float)y, (float)z), 15, out _x, out _y, out _z, new Vector3((float)5 + 5, (float)5 + 10, (float)5 + 5), true);
                    }
                    if (PosFound)
                    {
                        int counter = 1;
                        Dictionary<int, EntityClass>.KeyCollection entityTypesCollection = EntityClass.list.Dict.Keys;
                        foreach (int i in entityTypesCollection)
                        {
                            EntityClass eClass = EntityClass.list[i];
                            if (!eClass.bAllowUserInstantiate)
                            {
                                continue;
                            }
                            if (Entity_Id == counter)
                            {
                                Entity entity = EntityFactory.CreateEntity(i, new Vector3((float)x, (float)y, (float)z));
                                GameManager.Instance.World.SpawnEntityInWorld(entity);
                                Log.Out(string.Format("[SERVERTOOLS] Spawned an entity reward {0} at {1} x, {2} y, {3} z for {4}", eClass.entityClassName, x, y, z, _cInfo.playerName));
                                Phrases.Dict.TryGetValue("VoteReward9", out string phrase);
                                phrase = phrase.Replace("{EntityName}", eClass.entityClassName);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            counter++;
                        }
                        if (counter == entityTypesCollection.Count + 1)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Failed to spawn entity Id {0} as a reward. Check your entity spawn list in console.", Entity_Id));
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("VoteReward10", out string phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in VoteReward.Entityspawn: {0}", e.Message));
            }
        }

        public static void SetLink(string _voteSite)
        {
            try
            {
                if (File.Exists(API.GamePath + "/Mods/ServerTools/Config/XUi/windows.xml"))
                {
                    string[] arrLines = File.ReadAllLines(API.GamePath + "/Mods/ServerTools/Config/XUi/windows.xml");
                    int lineNumber = 0;
                    for (int i = 0; i < arrLines.Length; i++)
                    {
                        if (arrLines[i].Contains("browserVote"))
                        {
                            lineNumber = i + 7;
                            if (arrLines[lineNumber].Contains(_voteSite))
                            {
                                return;
                            }
                            break;
                        }
                    }
                    arrLines[lineNumber] = string.Format("<label depth=\"4\" pos=\"0,-40\" height=\"30\" width=\"257\" name=\"ServerWebsiteURL\" text=\"{0}\" justify=\"center\" style=\"press,hover\" font_size=\"30\" upper_case=\"false\" />", _voteSite);
                    File.WriteAllLines(API.GamePath + "/Mods/ServerTools/Config/XUi/windows.xml", arrLines);
                }
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", API.GamePath + "/Mods/ServerTools/Config/XUi/windows.xml", e.Message));
                return;
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
                    sw.WriteLine("<VoteRewards>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("    <!-- Items that do not require a quality should be set to 0 or 1 for min and max -->");
                    sw.WriteLine("    <!-- <Reward ItemOrBlock=\"meleeToolTorch\" MinCount=\"5\" MaxCount=\"10\" MinQuality=\"1\" MaxQuality=\"1\" /> -->");
                    for (int i = 0; i < OldNodeList.Count; i++)
                    {
                        if (OldNodeList[i].NodeType == XmlNodeType.Comment && !OldNodeList[i].OuterXml.Contains("<!-- Items that do not") &&
                            !OldNodeList[i].OuterXml.Contains("<!-- <Reward ItemOrBlock=\"meleeToolTorch\"") &&
                            !OldNodeList[i].OuterXml.Contains("<!-- <Reward ItemOrBlock=\"\""))
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
                            if (line.HasAttributes && line.Name == "Reward")
                            {
                                string itemBlock = "", minCount = "", maxCount = "", minQuality = "", maxQuality = "";
                                if (line.HasAttribute("ItemOrBlock"))
                                {
                                    itemBlock = line.GetAttribute("ItemOrBlock");
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
                                sw.WriteLine(string.Format("    <Reward ItemOrBlock=\"{0}\" MinCount=\"{1}\" MaxCount=\"{2}\" MinQuality=\"{3}\" MaxQuality=\"{4}\" />", itemBlock, minCount, maxCount, minQuality, maxQuality));
                            }
                        }
                    }
                    sw.WriteLine("</VoteRewards>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in InvalidItems.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }
    }
}

