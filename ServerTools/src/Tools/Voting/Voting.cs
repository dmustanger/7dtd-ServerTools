using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    class Voting
    {
        public static bool IsEnabled = false, IsRunning = false, RandomListRunning = false, Reward_Entity = false;
        public static int Reward_Count = 1, Delay_Between_Uses = 24, Entity_Id = 113, Weekly_Votes = 5;
        public static string Link = "https://7daystodie-servers.com/server/12345", API_Key = "xxxxxxxx", Command_reward = "reward", Command_vote = "vote";
        
        private static bool PosFound = false;

        private static Dictionary<string, int[]> Rewards = new Dictionary<string, int[]>();
        private static Dictionary<string, int[]> BonusRewards = new Dictionary<string, int[]>();
        private static List<string> RewardPayout = new List<string>();
        private static List<string> BonusPayout = new List<string>();

        private const string fileReward = "VoteReward.xml";
        private static readonly string FilePathReward = string.Format("{0}/{1}", API.ConfigPath, fileReward);
        private static FileSystemWatcher FileWatcherReward = new FileSystemWatcher(API.ConfigPath, fileReward);

        private const string fileBonus = "VoteRewardBonus.xml";
        private static readonly string FilePathBonus = string.Format("{0}/{1}", API.ConfigPath, fileBonus);
        private static FileSystemWatcher FileWatcherBonus = new FileSystemWatcher(API.ConfigPath, fileBonus);

        public static void Load()
        {
            LoadRewardXml();
            LoadBonusXml();
            InitFileWatchers();
        }

        public static void Unload()
        {
            Rewards.Clear();
            BonusRewards.Clear();
            RewardPayout.Clear();
            BonusPayout.Clear();
            FileWatcherReward.Dispose();
            FileWatcherBonus.Dispose();
            IsRunning = false;
        }

        public static void LoadRewardXml()
        {
            try
            {
                if (!File.Exists(FilePathReward))
                {
                    UpdateRewardXml();
                }
                XmlDocument xmlDoc = new XmlDocument();
                try
                {
                    xmlDoc.Load(FilePathReward);
                }
                catch (XmlException e)
                {
                    Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", fileReward, e.Message));
                    return;
                }
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                Rewards.Clear();
                RewardPayout.Clear();
                if (childNodes != null && childNodes[0] != null && childNodes[0].OuterXml.Contains("Version") && childNodes[0].OuterXml.Contains(Config.Version))
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
                        if (line.HasAttribute("ItemOrBlock") && line.HasAttribute("MinCount") && line.HasAttribute("MaxCount") &&
                            line.HasAttribute("MinQuality") && line.HasAttribute("MaxQuality"))
                        {
                            string item = line.GetAttribute("ItemOrBlock");
                            if (item == "")
                            {
                                continue;
                            }
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
                            if (item != "" && !Rewards.ContainsKey(item))
                            {
                                int[] c = new int[] { minCount, maxCount, minQuality, maxQuality };
                                Rewards.Add(item, c);
                            }
                        }
                    }
                    if (Rewards.Count > 0)
                    {
                        RewardPayout = new List<string>(Rewards.Keys);
                    }
                }
                else
                {
                    File.Delete(FilePathReward);
                    if (childNodes != null && childNodes[0] != null)
                    {
                        UpgradeRewardXml(childNodes);
                        return;
                    }
                    UpdateRewardXml();
                    return;
                }
            }
            catch (Exception e)
            {
                if (e.Message == "Specified cast is not valid.")
                {
                    File.Delete(FilePathReward);
                    UpdateRewardXml();
                }
                else
                {
                    Log.Out(string.Format("[SERVERTOOLS] Error in Voting.LoadRewardXml: {0}", e.Message));
                }
            }
        }

        public static void LoadBonusXml()
        {
            try
            {
                if (!File.Exists(FilePathBonus))
                {
                    UpdateBonusXml();
                }
                XmlDocument xmlDoc = new XmlDocument();
                try
                {
                    xmlDoc.Load(FilePathBonus);
                }
                catch (XmlException e)
                {
                    Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", fileBonus, e.Message));
                    return;
                }
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                BonusRewards.Clear();
                BonusPayout.Clear();
                if (childNodes != null && childNodes[0] != null && childNodes[0].OuterXml.Contains("Version") && childNodes[0].OuterXml.Contains(Config.Version))
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
                        if (line.HasAttribute("ItemOrBlock") && line.HasAttribute("MinCount") && line.HasAttribute("MaxCount") &&
                            line.HasAttribute("MinQuality") && line.HasAttribute("MaxQuality"))
                        {
                            string item = line.GetAttribute("ItemOrBlock");
                            if (item == "")
                            {
                                continue;
                            }
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
                            if (item != "" && !BonusRewards.ContainsKey(item))
                            {
                                int[] c = new int[] { minCount, maxCount, minQuality, maxQuality };
                                BonusRewards.Add(item, c);
                            }
                        }
                    }
                    if (BonusRewards.Count > 0)
                    {
                        BonusPayout = new List<string>(BonusRewards.Keys);
                    }
                }
                else
                {
                    File.Delete(FilePathBonus);
                    if (childNodes != null && childNodes[0] != null)
                    {
                        UpgradeBonusXml(childNodes);
                        return;
                    }
                    UpdateBonusXml();
                    return;
                }
            }
            catch (Exception e)
            {
                if (e.Message == "Specified cast is not valid.")
                {
                    File.Delete(FilePathBonus);
                    UpdateBonusXml();
                }
                else
                {
                    Log.Out(string.Format("[SERVERTOOLS] Error in Voting.LoadBonusXml: {0}", e.Message));
                }
            }
        }

        private static void UpdateRewardXml()
        {
            try
            {
                FileWatcherReward.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePathReward, false, Encoding.UTF8))
                {
                    sw.WriteLine("<VoteRewards>");
                    sw.WriteLine(string.Format("    <!-- <Version=\"{0}\" /> -->", Config.Version));
                    sw.WriteLine("    <!-- Items that do not require a quality should be set to 0 or 1 for min and max -->");
                    sw.WriteLine("    <!-- <Reward ItemOrBlock=\"meleeToolTorch\" MinCount=\"5\" MaxCount=\"10\" MinQuality=\"1\" MaxQuality=\"1\" /> -->");
                    sw.WriteLine("    <Reward ItemOrBlock=\"\" MinCount=\"\" MaxCount=\"\" MinQuality=\"\" MaxQuality=\"\" />");
                    if (Rewards.Count > 0)
                    {
                        foreach (KeyValuePair<string, int[]> kvp in Rewards)
                        {
                            sw.WriteLine(string.Format("    <Reward ItemOrBlock=\"{0}\" MinCount=\"{1}\" MaxCount=\"{2}\" MinQuality=\"{3}\" MaxQuality=\"{4}\" />", kvp.Key, kvp.Value[0], kvp.Value[1], kvp.Value[2], kvp.Value[3]));
                        }
                    }
                    sw.WriteLine("</VoteRewards>");
                    sw.Dispose();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Voting.UpdateRewardXml: {0}", e.Message));
            }
            FileWatcherReward.EnableRaisingEvents = true;
        }

        private static void UpdateBonusXml()
        {
            try
            {
                FileWatcherBonus.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePathBonus, false, Encoding.UTF8))
                {
                    sw.WriteLine("<BonusVoteRewards>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Items that do not require a quality should be set to 0 or 1 for min and max -->");
                    sw.WriteLine("    <!-- <Bonus ItemOrBlock=\"meleeToolTorch\" MinCount=\"5\" MaxCount=\"10\" MinQuality=\"1\" MaxQuality=\"1\" /> -->");
                    sw.WriteLine("    <Bonus ItemOrBlock=\"\" MinCount=\"\" MaxCount=\"\" MinQuality=\"\" MaxQuality=\"\" />");
                    if (BonusRewards.Count > 0)
                    {
                        foreach (KeyValuePair<string, int[]> kvp in BonusRewards)
                        {
                            sw.WriteLine(string.Format("    <Bonus ItemOrBlock=\"{0}\" MinCount=\"{1}\" MaxCount=\"{2}\" MinQuality=\"{3}\" MaxQuality=\"{4}\" />", kvp.Key, kvp.Value[0], kvp.Value[1], kvp.Value[2], kvp.Value[3]));
                        }
                    }
                    sw.WriteLine("</BonusVoteRewards>");
                    sw.Dispose();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Voting.UpdateBonusXml: {0}", e.Message));
            }
            FileWatcherBonus.EnableRaisingEvents = true;
        }

        private static void InitFileWatchers()
        {
            FileWatcherReward.Changed += new FileSystemEventHandler(OnRewardFileChanged);
            FileWatcherReward.Created += new FileSystemEventHandler(OnRewardFileChanged);
            FileWatcherReward.Deleted += new FileSystemEventHandler(OnRewardFileChanged);
            FileWatcherReward.EnableRaisingEvents = true;
            FileWatcherBonus.Changed += new FileSystemEventHandler(OnBonusFileChanged);
            FileWatcherBonus.Created += new FileSystemEventHandler(OnBonusFileChanged);
            FileWatcherBonus.Deleted += new FileSystemEventHandler(OnBonusFileChanged);
            FileWatcherBonus.EnableRaisingEvents = true;
            IsRunning = true;
        }

        private static void OnRewardFileChanged(object source, FileSystemEventArgs e)
        {
            if (!File.Exists(FilePathReward))
            {
                UpdateRewardXml();
            }
            LoadRewardXml();
        }

        private static void OnBonusFileChanged(object source, FileSystemEventArgs e)
        {
            if (!File.Exists(FilePathBonus))
            {
                UpdateBonusXml();
            }
            LoadBonusXml();
        }

        public static void Check(ClientInfo _cInfo)
        {
            try
            {
                if (!Reward_Entity && Rewards.Count == 0)
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
                Log.Out(string.Format("[SERVERTOOLS] Error in Voting.Check: {0}", e.Message));
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
                                EntitySpawn(_cInfo);
                            }
                            else
                            {
                                ItemOrBlockSpawn(_cInfo, Reward_Count);
                            }
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
            phrase = phrase.Replace("{VoteSite}", Link);
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
        }

        public static void ItemOrBlockSpawn(ClientInfo _cInfo, int _count)
        {
            try
            {
                for (int i = 0; i < _count; i++)
                {
                    ItemOrBlockRandom(_cInfo, false);
                }
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
                            if (BonusPayout.Count > 0)
                            {
                                ItemOrBlockRandom(_cInfo, true);
                            }
                            else
                            {
                                ItemOrBlockRandom(_cInfo, false);
                            }
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
                }
                Phrases.Dict.TryGetValue("VoteReward7", out string phrase1);
                phrase1 = phrase1.Replace("{Value}", Delay_Between_Uses.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                Phrases.Dict.TryGetValue("VoteReward8", out phrase1);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                Phrases.Dict.TryGetValue("VoteReward11", out phrase1);
                phrase1 = phrase1.Replace("{PlayerName}", _cInfo.playerName);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastVote = DateTime.Now;
                PersistentContainer.DataChange = true;
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Voting.ItemOrBlockCounter: {0}", e.Message));
            }
        }

        private static void ItemOrBlockRandom(ClientInfo _cInfo, bool _bonus)
        {
            try
            {
                string randomItem;
                int[] itemData;
                if (!_bonus)
                {
                    randomItem = RewardPayout.RandomObject();
                    if (!Rewards.TryGetValue(randomItem, out itemData))
                    {
                        return;
                    }
                }
                else
                {
                    randomItem = BonusPayout.RandomObject();
                    if (!BonusRewards.TryGetValue(randomItem, out itemData))
                    {
                        return;
                    }
                }
                int count = itemData[0];
                if (itemData[0] != itemData[1] + 1)
                {
                    count = new System.Random().Next(itemData[0], itemData[1] + 1);
                }
                int quality = itemData[2];
                if (itemData[2] != itemData[3] + 1)
                {
                    quality = new System.Random().Next(itemData[2], itemData[3] + 1);
                }
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
                Give(_cInfo, itemValue, count);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Voting.ItemOrBlockRandom: {0}", e.Message));
            }
        }

        private static void Give(ClientInfo _cInfo, ItemValue _itemValue, int _count)
        {
            try
            {
                EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
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
                Log.Out(string.Format("[SERVERTOOLS] Error in Voting.Give: {0}", e.Message));
            }
        }

        private static void EntitySpawn(ClientInfo _cInfo)
        {
            try
            {
                Spawn(_cInfo);
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
                            Spawn(_cInfo);
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
                }
                Phrases.Dict.TryGetValue("VoteReward7", out string phrase1);
                phrase1 = phrase1.Replace("{Value}", Delay_Between_Uses.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastVote = DateTime.Now;
                PersistentContainer.DataChange = true;
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Voting.Entity: {0}", e.Message));
            }
        }

        private static void Spawn(ClientInfo _cInfo)
        {
            try
            {
                EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
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
                Log.Out(string.Format("[SERVERTOOLS] Error in Voting.EntitySpawn: {0}", e.Message));
            }
        }

        public static void Exec(ClientInfo _cInfo)
        {
            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui open browserVote", true));
        }

        public static void SetLink(string _link)
        {
            try
            {
                if (File.Exists(GeneralOperations.XPathDir + "XUi/windows.xml"))
                {
                    List<string> lines = File.ReadAllLines(GeneralOperations.XPathDir + "XUi/windows.xml").ToList();
                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (lines[i].Contains("browserVote"))
                        {
                            if (!lines[i + 7].Contains(_link))
                            {
                                lines[i + 7] = string.Format("          <label depth=\"2\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"ServerWebsiteURL\" text=\"{0}\" justify=\"center\" style=\"press,hover\" font_size=\"1\" upper_case=\"false\" sound=\"[paging_click]\" />", _link);
                                File.WriteAllLines(GeneralOperations.XPathDir + "XUi/windows.xml", lines.ToArray());
                            }
                            return;
                        }
                    }
                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (lines[i].Contains("/append"))
                        {
                            lines.RemoveRange(i, 3);
                            lines.Add("  <window name=\"browserVote\" controller=\"ServerInfo\">");
                            lines.Add("      <panel name=\"header\" pos=\"-300,0\" height=\"40\" depth=\"1\" backgroundspritename=\"ui_game_panel_header\" >");
                            lines.Add("          <label style=\"header.name\" pos=\"0,0\" width=\"197\" justify=\"center\" text=\"Voting Site\" />");
                            lines.Add("      </panel>");
                            lines.Add("      <panel name=\"\" pos=\"-300,0\" height=\"63\">");
                            lines.Add("          <sprite depth=\"5\" pos=\"0,0\" height=\"33\" width=\"200\" name=\"background\" color=\"[darkGrey]\" type=\"sliced\" />");
                            lines.Add("          <label name=\"ServerDescription\" />");
                            lines.Add(string.Format("          <label depth=\"2\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"ServerWebsiteURL\" text=\"{0}\" justify=\"center\" style=\"press,hover\" font_size=\"1\" upper_case=\"false\" sound=\"[paging_click]\" />", _link));
                            lines.Add("          <sprite depth=\"3\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"URLMask\" color=\"[white]\" foregroundlayer=\"true\" fillcenter=\"true\" />");
                            lines.Add("          <sprite depth=\"4\" name=\"computerIcon\" style=\"icon30px\" pos=\"5,-40\" color=\"[black]\" sprite=\"ui_game_symbol_computer\" />");
                            lines.Add("          <label depth=\"4\" style=\"header.name\" pos=\"0,-40\" height=\"32\" width=\"200\" justify=\"center\" color=\"[black]\" text=\"Click Here\" />");
                            lines.Add("          <!-- Change the text IP and Port to the one needed by ServerTools web api -->");
                            lines.Add("      </panel>");
                            lines.Add("  </window>");
                            lines.Add("");
                            lines.Add("</append>");
                            lines.Add("");
                            lines.Add("</configs>");
                            File.WriteAllLines(GeneralOperations.XPathDir + "XUi/windows.xml", lines.ToArray());
                        }
                    }
                }
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", GeneralOperations.XPathDir + "XUi/windows.xml", e.Message));
            }
        }

        private static void UpgradeRewardXml(XmlNodeList nodeList)
        {
            try
            {
                FileWatcherReward.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePathReward, false, Encoding.UTF8))
                {
                    sw.WriteLine("<VoteRewards>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Items that do not require a quality should be set to 1 for MinQuality and MaxQuality -->");
                    sw.WriteLine("    <!-- <Reward ItemOrBlock=\"meleeToolTorch\" MinCount=\"5\" MaxCount=\"10\" MinQuality=\"1\" MaxQuality=\"1\" /> -->");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (!nodeList[i].OuterXml.Contains("<!-- Items that") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Reward ItemOrBlock=\"meleeToolTorch\"") && !nodeList[i].OuterXml.Contains("<Reward ItemOrBlock=\"\"") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Version"))
                        {
                            sw.WriteLine(nodeList[i].OuterXml);
                        }
                    }
                    sw.WriteLine("    <Reward ItemOrBlock=\"\" MinCount=\"\" MaxCount=\"\" MinQuality=\"\" MaxQuality=\"\" />");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)nodeList[i];
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
                    sw.Dispose();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Voting.UpgradeRewardXml: {0}", e.Message));
            }
            FileWatcherReward.EnableRaisingEvents = true;
            LoadRewardXml();
        }

        private static void UpgradeBonusXml(XmlNodeList nodeList)
        {
            try
            {
                FileWatcherBonus.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePathBonus, false, Encoding.UTF8))
                {
                    sw.WriteLine("<BonusVoteRewards>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Items that do not require a quality should be set to 1 for MinQuality and MaxQuality -->");
                    sw.WriteLine("    <!-- <Bonus ItemOrBlock=\"meleeToolTorch\" MinCount=\"5\" MaxCount=\"10\" MinQuality=\"1\" MaxQuality=\"1\" /> -->");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (!nodeList[i].OuterXml.Contains("<!-- Items that") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Bonus ItemOrBlock=\"meleeToolTorch\"") && !nodeList[i].OuterXml.Contains("<Bonus ItemOrBlock=\"\"") &&
                            !nodeList[i].OuterXml.Contains("<!-- <Version"))
                        {
                            sw.WriteLine(nodeList[i].OuterXml);
                        }
                    }
                    sw.WriteLine("    <Bonus ItemOrBlock=\"\" MinCount=\"\" MaxCount=\"\" MinQuality=\"\" MaxQuality=\"\" />");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)nodeList[i];
                            if (line.HasAttributes && line.Name == "Bonus")
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
                                sw.WriteLine(string.Format("    <Bonus ItemOrBlock=\"{0}\" MinCount=\"{1}\" MaxCount=\"{2}\" MinQuality=\"{3}\" MaxQuality=\"{4}\" />", itemBlock, minCount, maxCount, minQuality, maxQuality));
                            }
                        }
                    }
                    sw.WriteLine("</BonusVoteRewards>");
                    sw.Dispose();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Voting.UpgradeBonusXml: {0}", e.Message));
            }
            FileWatcherBonus.EnableRaisingEvents = true;
            LoadBonusXml();
        }
    }
}

