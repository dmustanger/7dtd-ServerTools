using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    class VoteReward
    {
        public static bool IsEnabled = false, IsRunning = false, RandomListRunning = false, Reward_Entity = false;
        public static int Reward_Count = 1, Delay_Between_Uses = 24, Entity_Id = 73, _counter = 0, Weekly_Votes = 5;
        public static string Command46 = "reward";
        public static string Your_Voting_Site = ("https://7daystodie-servers.com/server/12345"), API_Key = ("xxxxxxxx");
        private const string file = "VoteReward.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static Dictionary<string, int[]> dict = new Dictionary<string, int[]>();
        private static List<string> list = new List<string>();
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        private static bool updateConfig = false, posFound = false;
        private static System.Random rnd = new System.Random();

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
            fileWatcher.Dispose();
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
                if (childNode.Name == "Rewards")
                {
                    dict.Clear();
                    list.Clear();
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'VoteReward' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("ItemOrBlock"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vote Reward entry because of missing ItemOrBlock attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("CountMin"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vote Reward entry because of missing CountMin attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("CountMax"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vote Reward entry because of missing CountMax attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("QualityMin"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vote Reward entry because of missing QualityMin attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("QualityMax"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Vote Reward entry because of missing QualityMax attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("CountMin"), out int _countMin))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring Vote Reward entry because of invalid (non-numeric) value for 'CountMin' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("CountMax"), out int _countMax))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring Vote Reward entry because of invalid (non-numeric) value for 'CountMax' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("QualityMin"), out int _qualityMin))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring Vote Reward entry because of invalid (non-numeric) value for 'QualityMin' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("QualityMax"), out int _qualityMax))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring Vote Reward entry because of invalid (non-numeric) value for 'QualityMax' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (_qualityMax > 6)
                        {
                            _qualityMax = 6;
                        }
                        string _item = _line.GetAttribute("ItemOrBlock");
                        ItemClass _class;
                        Block _block;
                        if (int.TryParse(_item, out int _id))
                        {
                            _class = ItemClass.GetForId(_id);
                            _block = Block.GetBlockByName(_item, true);
                        }
                        else
                        {
                            _class = ItemClass.GetItemClass(_item, true);
                            _block = Block.GetBlockByName(_item, true);
                        }
                        if (_class == null && _block == null)
                        {
                            SdtdConsole.Instance.Output(string.Format("Unable to find item or block {0}", _item));
                            continue;
                        }
                        else
                        {
                            if (!dict.ContainsKey(_item))
                            {
                                int[] _c = new int[] { _countMin, _countMax, _qualityMin, _qualityMax };
                                dict.Add(_item, _c);
                            }
                        }
                    }
                }
            }
            list = new List<string>(dict.Keys);
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
                sw.WriteLine("<VoteRewards>");
                sw.WriteLine("    <Rewards>");
                if (dict.Count > 0)
                {
                    foreach (KeyValuePair<string, int[]> kvp in dict)
                    {
                        sw.WriteLine(string.Format("        <Reward ItemOrBlock=\"{0}\" CountMin=\"{1}\" CountMax=\"{2}\" QualityMin=\"{3}\" QualityMax=\"{4}\" />", kvp.Key, kvp.Value[0], kvp.Value[1], kvp.Value[2], kvp.Value[3]));
                    }
                }
                else
                {
                    sw.WriteLine("        <Reward ItemOrBlock=\"meleeToolTorch\" CountMin=\"5\" CountMax=\"10\" QualityMin=\"1\" QualityMax=\"1\" />");
                    sw.WriteLine("        <Reward ItemOrBlock=\"ammo9mmBullet\" CountMin=\"10\" CountMax=\"30\" QualityMin=\"1\" QualityMax=\"1\" />");
                    sw.WriteLine("        <Reward ItemOrBlock=\"ammo44MagnumBullet\" CountMin=\"5\" CountMax=\"10\" QualityMin=\"1\" QualityMax=\"1\" />");
                    sw.WriteLine("        <Reward ItemOrBlock=\"armorIronChest\" CountMin=\"1\" CountMax=\"1\" QualityMin=\"1\" QualityMax=\"6\" />");
                    sw.WriteLine("        <Reward ItemOrBlock=\"foodCropCorn\" CountMin=\"5\" CountMax=\"10\" QualityMin=\"1\" QualityMax=\"1\" />");
                    sw.WriteLine("        <Reward ItemOrBlock=\"terrSand\" CountMin=\"5\" CountMax=\"10\" QualityMin=\"1\" QualityMax=\"1\" />");
                    sw.WriteLine("        <Reward ItemOrBlock=\"terrSnow\" CountMin=\"2\" CountMax=\"10\" QualityMin=\"1\" QualityMax=\"1\" />");
                }
                sw.WriteLine("    </Rewards>");
                sw.WriteLine("</VoteRewards>");
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

        public static void Check(ClientInfo _cInfo)
        {
            if (!Reward_Entity && dict.Count == 0)
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Vote reward count is set to zero. Contact an administrator." + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                Log.Out(string.Format("[SERVERTOOLS] No items available for reward. Check for an error in the VoteReward.xml file."));
                return;
            }
            if (Delay_Between_Uses == 0)
            {
                CheckSite(_cInfo);
            }
            else
            {
                DateTime _lastVoteReward = PersistentContainer.Instance.Players[_cInfo.playerId].LastVote;
                TimeSpan varTime = DateTime.Now - _lastVoteReward;
                double fractionalHours = varTime.TotalHours;
                int _timepassed = (int)fractionalHours;
                if (_timepassed >= Delay_Between_Uses)
                {
                    CheckSite(_cInfo);
                }
                else
                {
                    int _timeleft = Delay_Between_Uses - _timepassed;
                    string _phrase602;
                    if (!Phrases.Dict.TryGetValue(602, out _phrase602))
                    {
                        _phrase602 = "You can only use {CommandPrivate}{Command46} once every {DelayBetweenRewards} hours. Time remaining: {TimeRemaining} hour(s).";
                    }
                    _phrase602 = _phrase602.Replace("{DelayBetweenRewards}", Delay_Between_Uses.ToString());
                    _phrase602 = _phrase602.Replace("{TimeRemaining}", _timeleft.ToString());
                    _phrase602 = _phrase602.Replace("{CommandPrivate}", ChatHook.Command_Private);
                    _phrase602 = _phrase602.Replace("{Command46}", Command46);
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase602 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        private static void CheckSite(ClientInfo _cInfo)
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback += (send, certificate, chain, sslPolicyErrors) => { return true; };
                var VoteUrl = string.Format("https://7daystodie-servers.com/api/?object=votes&element=claim&key={0}&steamid={1}", Uri.EscapeUriString(API_Key), Uri.EscapeUriString(_cInfo.playerId));
                //var VoteUrl = string.Format("https://7daystodie-servers.com/api/?object=votes&element=claim&key={0}&username={1}", Uri.EscapeUriString(API_Key), Uri.EscapeUriString(_cInfo.playerName));
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
                            PersistentContainer.Instance.Players[_cInfo.playerId].LastVote = DateTime.Now;
                            PersistentContainer.Instance.Save();
                        }
                        catch
                        {
                            Log.Error("[SERVERTOOLS] Vote reward failed to spawn the reward for players");
                        }
                    }
                    else if (VoteResult == "2")
                    {
                        string _phrase702;
                        if (!Phrases.Dict.TryGetValue(702, out _phrase702))
                        {
                            _phrase702 = "Unable to get a result from the website, {PlayerName}. Please try {CommandPrivate}{Command46} again.";
                        }
                        _phrase702 = _phrase702.Replace("{PlayerName}", _cInfo.playerName);
                        _phrase702 = _phrase702.Replace("{CommandPrivate}", ChatHook.Command_Private);
                        _phrase702 = _phrase702.Replace("{Command46}", Command46);
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase702 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch
            {
                Log.Error("[SERVERTOOLS] Vote reward failed to communicate with the website");
            }
        }

        private static void NoVote(ClientInfo _cInfo)
        {
            string _phrase700;
            if (!Phrases.Dict.TryGetValue(700, out _phrase700))
            {
                _phrase700 = "Your vote has not been located {PlayerName}. Make sure you voted @ {VoteSite} and try again.";
            }
            _phrase700 = _phrase700.Replace("{PlayerName}", _cInfo.playerName);
            _phrase700 = _phrase700.Replace("{VoteSite}", Your_Voting_Site);
            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase700 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
        }

        public static void ItemOrBlockCounter(ClientInfo _cInfo, int _counter)
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
                    DateTime _lastVoteWeek;
                    if (PersistentContainer.Instance.Players[_cInfo.playerId].LastVoteWeek != null)
                    {
                        _lastVoteWeek = PersistentContainer.Instance.Players[_cInfo.playerId].LastVoteWeek;
                    }
                    else
                    {
                        _lastVoteWeek = DateTime.Now;
                    }
                    TimeSpan varTime = DateTime.Now - _lastVoteWeek;
                    double fractionalDays = varTime.TotalDays;
                    int _timepassed = (int)fractionalDays;
                    if (_timepassed < 7)
                    {
                        int _voteWeekCount = PersistentContainer.Instance.Players[_cInfo.playerId].VoteWeekCount;
                        if (_voteWeekCount + 1 == Weekly_Votes)
                        {
                            PersistentContainer.Instance.Players[_cInfo.playerId].VoteWeekCount = 1;
                            PersistentContainer.Instance.Players[_cInfo.playerId].LastVoteWeek = DateTime.Now;
                            PersistentContainer.Instance.Save();
                            ItemOrBlockRandom(_cInfo);
                            string _phrase704;
                            if (!Phrases.Dict.TryGetValue(704, out _phrase704))
                            {
                                _phrase704 = "You have reached the votes needed in a week. Thank you! Sent you an extra reward and reset your weekly votes to 1.";
                            }
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase704 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            PersistentContainer.Instance.Players[_cInfo.playerId].VoteWeekCount = _voteWeekCount + 1;
                            PersistentContainer.Instance.Save();
                            int _remainingVotes = Weekly_Votes - _voteWeekCount + 1;
                            DateTime _date2 = _lastVoteWeek.AddDays(7);
                            string _phrase705;
                            if (!Phrases.Dict.TryGetValue(705, out _phrase705))
                            {
                                _phrase705 = "You have voted {Votes} time since {Date}. You need {Count} more votes before {Date2} to reach the bonus.";
                            }
                            _phrase705 = _phrase705.Replace("{Votes}", _voteWeekCount + 1.ToString());
                            _phrase705 = _phrase705.Replace("{Date}", _lastVoteWeek.ToString());
                            _phrase705 = _phrase705.Replace("{Count}", _remainingVotes.ToString());
                            _phrase705 = _phrase705.Replace("{Date2}", _date2.ToString());
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase705 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].VoteWeekCount = 1;
                        PersistentContainer.Instance.Players[_cInfo.playerId].LastVoteWeek = DateTime.Now;
                        PersistentContainer.Instance.Save();
                        int _remainingVotes = Weekly_Votes - 1;
                        DateTime _date2 = DateTime.Now.AddDays(7);
                        string _phrase705;
                        if (!Phrases.Dict.TryGetValue(705, out _phrase705))
                        {
                            _phrase705 = "You have voted {Votes} time since {Date}. You need {Count} more votes before {Date2} to reach the bonus.";
                        }
                        _phrase705 = _phrase705.Replace("{Votes}", 1.ToString());
                        _phrase705 = _phrase705.Replace("{Date}", DateTime.Now.ToString());
                        _phrase705 = _phrase705.Replace("{Count}", _remainingVotes.ToString());
                        _phrase705 = _phrase705.Replace("{Date2}", _date2.ToString());
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase705 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                string _phrase701;
                if (!Phrases.Dict.TryGetValue(701, out _phrase701))
                {
                    _phrase701 = "Thank you for your vote. You can vote and receive another reward in {VoteDelay} hours.";
                }
                _phrase701 = _phrase701.Replace("{VoteDelay}", Delay_Between_Uses.ToString());
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase701 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                string _phrase703;
                if (!Phrases.Dict.TryGetValue(703, out _phrase703))
                {
                    _phrase703 = "Reward items were sent to your inventory. If it is full, check the ground.";
                }
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase703 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        private static void ItemOrBlockRandom(ClientInfo _cInfo)
        {
            string _item = list.RandomObject();
            int[] _values;
            if (dict.TryGetValue(_item, out _values))
            {
                int _count = 1;
                if (_values[0] != _values[1] && _values[1] > _values[0])
                {
                    _count = rnd.Next(_values[0], _values[1] + 1);
                }
                else if (_values[0] > 0)
                {
                    _count = _values[0];
                }
                int quality = rnd.Next(_values[2], _values[3] + 1);
                if (quality < 1)
                {
                    quality = 1;
                }
                ItemValue _itemValue = new ItemValue(ItemClass.GetItem(_item).type, quality, quality, false, null, 1);
                Give(_cInfo, _itemValue, _count);
            }
        }

        private static void Give(ClientInfo _cInfo, ItemValue _itemValue, int _count)
        {
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            if (_player.IsSpawned())
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

        private static void Entity(ClientInfo _cInfo)
        {
            Entityspawn(_cInfo);
            if (Weekly_Votes > 0)
            {
                DateTime _lastVoteWeek = PersistentContainer.Instance.Players[_cInfo.playerId].LastVoteWeek;
                TimeSpan varTime = DateTime.Now - _lastVoteWeek;
                double fractionalDays = varTime.TotalDays;
                int _timepassed = (int)fractionalDays;
                if (_timepassed < 7)
                {
                    int _voteWeekCount = PersistentContainer.Instance.Players[_cInfo.playerId].VoteWeekCount;
                    if (_voteWeekCount + 1 == Weekly_Votes)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].VoteWeekCount = 1;
                        PersistentContainer.Instance.Players[_cInfo.playerId].LastVoteWeek = DateTime.Now;
                        PersistentContainer.Instance.Save();
                        Entityspawn(_cInfo);
                        string _phrase704;
                        if (!Phrases.Dict.TryGetValue(704, out _phrase704))
                        {
                            _phrase704 = "You have reached the votes needed in a week. Thank you! Sent you an extra reward and reset your weekly votes to 1.";
                        }
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase704 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].VoteWeekCount = _voteWeekCount + 1;
                        PersistentContainer.Instance.Save();
                        int _remainingVotes = Weekly_Votes - _voteWeekCount + 1;
                        DateTime _date2 = _lastVoteWeek.AddDays(7);
                        string _phrase705;
                        if (!Phrases.Dict.TryGetValue(705, out _phrase705))
                        {
                            _phrase705 = "You have voted {Votes} time since {Date}. You need {Count} more votes before {Date2} to reach the bonus.";
                        }
                        _phrase705 = _phrase705.Replace("{Votes}", _voteWeekCount + 1.ToString());
                        _phrase705 = _phrase705.Replace("{Date}", _lastVoteWeek.ToString());
                        _phrase705 = _phrase705.Replace("{Count}", _remainingVotes.ToString());
                        _phrase705 = _phrase705.Replace("{Date2}", _date2.ToString());
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase705 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    PersistentContainer.Instance.Players[_cInfo.playerId].VoteWeekCount = 1;
                    PersistentContainer.Instance.Players[_cInfo.playerId].LastVoteWeek = DateTime.Now;
                    PersistentContainer.Instance.Save();
                    int _remainingVotes = Weekly_Votes - 1;
                    DateTime _date2 = DateTime.Now.AddDays(7);
                    string _phrase705;
                    if (!Phrases.Dict.TryGetValue(705, out _phrase705))
                    {
                        _phrase705 = "You have voted 1 time since {Date}. You need {Count} more votes before {Date2} to reach the bonus.";
                    }
                    _phrase705 = _phrase705.Replace("{Date}", DateTime.Now.ToString());
                    _phrase705 = _phrase705.Replace("{Count}", _remainingVotes.ToString());
                    _phrase705 = _phrase705.Replace("{Date2}", _date2.ToString());
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase705 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            string _phrase701;
            if (!Phrases.Dict.TryGetValue(701, out _phrase701))
            {
                _phrase701 = "Thank you for your vote. You can vote and receive another reward in {VoteDelay} hours.";
            }
            _phrase701 = _phrase701.Replace("{VoteDelay}", Delay_Between_Uses.ToString());
            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase701 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
        }

        private static void Entityspawn(ClientInfo _cInfo)
        {
            EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            if (_player.IsSpawned())
            {
                Vector3 pos = _player.GetPosition();
                float x = pos.x;
                float y = pos.y;
                float z = pos.z;
                int _x, _y, _z;
                posFound = true;
                posFound = GameManager.Instance.World.FindRandomSpawnPointNearPosition(new Vector3((float)x, (float)y, (float)z), 15, out _x, out _y, out _z, new Vector3((float)5, (float)5, (float)5), true);
                if (!posFound)
                {
                    posFound = GameManager.Instance.World.FindRandomSpawnPointNearPosition(new Vector3((float)x, (float)y, (float)z), 15, out _x, out _y, out _z, new Vector3((float)5 + 5, (float)5 + 10, (float)5 + 5), true);
                }
                if (posFound)
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
                            Entity entity = EntityFactory.CreateEntity(i, new Vector3((float)_x, (float)_y, (float)_z));
                            GameManager.Instance.World.SpawnEntityInWorld(entity);
                            Log.Out(string.Format("[SERVERTOOLS] Spawned an entity reward {0} at {1} x, {2} y, {3} z for {4}", eClass.entityClassName, _x, _y, _z, _cInfo.playerName));
                            string _message = "Spawned a {EntityName} near you.";
                            _message = _message.Replace("{EntityName}", eClass.entityClassName);
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "No spawn point was found near you. Please move locations and try again.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Can not give you a vote reward unless spawned. Please type " + ChatHook.Command_Private + Command46 + " again.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}

