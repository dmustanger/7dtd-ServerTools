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
                if (!Utils.FileExists(FilePath))
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
                XmlNodeList _childNodes = xmlDoc.DocumentElement.ChildNodes;
                if (_childNodes != null && _childNodes.Count > 0)
                {
                    Dict.Clear();
                    Items.Clear();
                    for (int i = 0; i < _childNodes.Count; i++)
                    {
                        if (_childNodes[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement _line = (XmlElement)_childNodes[i];
                            if (_line.HasAttributes)
                            {
                                if (_line.HasAttribute("Version") && _line.GetAttribute("Version") != Config.Version)
                                {
                                    UpgradeXml(_childNodes);
                                    return;
                                }
                                else if (_line.HasAttribute("ItemOrBlock") && _line.HasAttribute("MinCount") && _line.HasAttribute("MaxCount") &&
                                    _line.HasAttribute("MinQuality") && _line.HasAttribute("MaxQuality"))
                                {
                                    if (!int.TryParse(_line.GetAttribute("MinCount"), out int _minCount))
                                    {
                                        Log.Out(string.Format("[SERVERTOOLS] Ignoring VoteReward.xml entry. Invalid (non-numeric) value for 'MinCount' attribute: {0}", _line.OuterXml));
                                        continue;
                                    }
                                    if (!int.TryParse(_line.GetAttribute("MaxCount"), out int _maxCount))
                                    {
                                        Log.Out(string.Format("[SERVERTOOLS] Ignoring VoteReward.xml entry. Invalid (non-numeric) value for 'MaxCount' attribute: {0}", _line.OuterXml));
                                        continue;
                                    }
                                    if (!int.TryParse(_line.GetAttribute("MinQuality"), out int _minQuality))
                                    {
                                        Log.Out(string.Format("[SERVERTOOLS] Ignoring VoteReward.xml entry. Invalid (non-numeric) value for 'MinQuality' attribute: {0}", _line.OuterXml));
                                        continue;
                                    }
                                    if (!int.TryParse(_line.GetAttribute("MaxQuality"), out int _maxQuality))
                                    {
                                        Log.Out(string.Format("[SERVERTOOLS] Ignoring VoteReward.xml entry. Invalid (non-numeric) value for 'MaxQuality' attribute: {0}", _line.OuterXml));
                                        continue;
                                    }
                                    string _item = _line.GetAttribute("ItemOrBlock");
                                    if (_item == "WalletCoin")
                                    {
                                        if (Wallet.IsEnabled)
                                        {
                                            if (_minCount < 1)
                                            {
                                                _minCount = 1;
                                            }
                                        }
                                        else
                                        {
                                            Log.Out(string.Format("[SERVERTOOLS] Ignoring VoteReward.xml entry. Wallet tool is not enabled: {0}", _line.OuterXml));
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        ItemValue _itemValue = ItemClass.GetItem(_item, false);
                                        if (_itemValue.type == ItemValue.None.type)
                                        {
                                            Log.Out(string.Format("[SERVERTOOLS] Ignoring VoteReward.xml entry. Item not found: {0}", _item));
                                            continue;
                                        }
                                        if (_minCount > _itemValue.ItemClass.Stacknumber.Value)
                                        {
                                            _minCount = _itemValue.ItemClass.Stacknumber.Value;
                                        }
                                        else if (_minCount < 1)
                                        {
                                            _minCount = 1;
                                        }
                                        if (_maxCount > _itemValue.ItemClass.Stacknumber.Value)
                                        {
                                            _maxCount = _itemValue.ItemClass.Stacknumber.Value;
                                        }
                                        else if (_maxCount < 1)
                                        {
                                            _maxCount = 1;
                                        }
                                    }
                                    if (_minQuality < 1)
                                    {
                                        _minQuality = 1;
                                    }
                                    if (_maxQuality < 1)
                                    {
                                        _maxQuality = 1;
                                    }
                                    if (!Dict.ContainsKey(_item))
                                    {
                                        int[] _c = new int[] { _minCount, _maxCount, _minQuality, _maxQuality };
                                        Dict.Add(_item, _c);
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
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in VoteReward.LoadXml: {0}", e.Message));
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
                    sw.WriteLine("<!-- Items that do not require a quality should be set to 0 or 1 for min and max -->");
                    sw.WriteLine("<!-- WalletCoin can be used as the item name -->");
                    sw.WriteLine("<!-- <Reward ItemOrBlock=\"meleeToolTorch\" MinCount=\"5\" MaxCount=\"10\" MinQuality=\"1\" MaxQuality=\"1\" /> -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    if (Dict.Count > 0)
                    {
                        foreach (KeyValuePair<string, int[]> kvp in Dict)
                        {
                            sw.WriteLine(string.Format("    <Reward ItemOrBlock=\"{0}\" MinCount=\"{1}\" MaxCount=\"{2}\" MinQuality=\"{3}\" MaxQuality=\"{4}\" />", kvp.Key, kvp.Value[0], kvp.Value[1], kvp.Value[2], kvp.Value[3]));
                        }
                    }
                    else
                    {
                        sw.WriteLine("    <!-- <Reward ItemOrBlock=\"\" MinCount=\"\" MaxCount=\"\" MinQuality=\"\" MaxQuality=\"\" /> -->");
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
            if (!Utils.FileExists(FilePath))
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
                    Phrases.Dict.TryGetValue("VoteReward2", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                        Phrases.Dict.TryGetValue("VoteReward1", out string _phrase);
                        _phrase = _phrase.Replace("{DelayBetweenRewards}", Delay_Between_Uses.ToString());
                        _phrase = _phrase.Replace("{TimeRemaining}", _timeleft.ToString());
                        _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                        _phrase = _phrase.Replace("{Command_reward}", Command_reward);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                var VoteUrl = string.Format("https://7daystodie-servers.com/api/?object=votes&element=claim&key={0}&steamid={1}", Uri.EscapeUriString(API_Key), Uri.EscapeUriString(_cInfo.playerId));
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
                            PersistentContainer.DataChange = true;
                        }
                        catch
                        {
                            Log.Error("[SERVERTOOLS] Vote reward tool failed to spawn the reward for players");
                        }
                    }
                    else if (VoteResult == "2")
                    {
                        Phrases.Dict.TryGetValue("VoteReward3", out string _phrase);
                        _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
                        _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                        _phrase = _phrase.Replace("{Command_reward}", Command_reward);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
            Phrases.Dict.TryGetValue("VoteReward4", out string _phrase);
            _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
            _phrase = _phrase.Replace("{VoteSite}", Your_Voting_Site);
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                                ItemOrBlockRandom(_cInfo);
                                Phrases.Dict.TryGetValue("VoteReward5", out string _phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                PersistentContainer.Instance.Players[_cInfo.playerId].VoteWeekCount = _voteWeekCount + 1;
                                int _remainingVotes = Weekly_Votes - _voteWeekCount + 1;
                                DateTime _date2 = _lastVoteWeek.AddDays(7);
                                Phrases.Dict.TryGetValue("VoteReward6", out string _phrase);
                                _phrase = _phrase.Replace("{Value}", _voteWeekCount + 1.ToString());
                                _phrase = _phrase.Replace("{Date}", _lastVoteWeek.ToString());
                                _phrase = _phrase.Replace("{Value2}", _remainingVotes.ToString());
                                _phrase = _phrase.Replace("{Date2}", _date2.ToString());
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        else
                        {
                            PersistentContainer.Instance.Players[_cInfo.playerId].VoteWeekCount = 1;
                            PersistentContainer.Instance.Players[_cInfo.playerId].LastVoteWeek = DateTime.Now;
                            int _remainingVotes = Weekly_Votes - 1;
                            DateTime _date2 = DateTime.Now.AddDays(7);
                            Phrases.Dict.TryGetValue("VoteReward6", out string _phrase);
                            _phrase = _phrase.Replace("{Value}", 1.ToString());
                            _phrase = _phrase.Replace("{Date}", _lastVoteWeek.ToString());
                            _phrase = _phrase.Replace("{Value2}", _remainingVotes.ToString());
                            _phrase = _phrase.Replace("{Date2}", _date2.ToString());
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        PersistentContainer.DataChange = true;
                    }
                    Phrases.Dict.TryGetValue("VoteReward7", out string _phrase1);
                    _phrase1 = _phrase1.Replace("{Value}", Delay_Between_Uses.ToString());
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    Phrases.Dict.TryGetValue("VoteReward8", out _phrase1);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    Phrases.Dict.TryGetValue("VoteReward11", out _phrase1);
                    _phrase1 = _phrase1.Replace("{PlayerName}", _cInfo.playerName);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
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
                string _randomItem = Items.RandomObject();
                if (Dict.TryGetValue(_randomItem, out int[] _itemData))
                {
                    int _count = Random.Next(_itemData[0], _itemData[1] + 1);
                    if (_randomItem == "WalletCoin")
                    {
                        if (Wallet.IsEnabled)
                        {
                            Wallet.AddCoinsToWallet(_cInfo.playerId, _count);
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("VoteReward12", out string _phrase);
                            Log.Out(string.Format("[SERVERTOOLS] {0}", _phrase));
                        }
                    }
                    else
                    {
                        int _quality = Random.Next(_itemData[2], _itemData[3] + 1);
                        ItemValue _itemValue = new ItemValue(ItemClass.GetItem(_randomItem).type, _quality, _quality, false, null, 1);
                        Give(_cInfo, _itemValue, _count);
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
                            Entityspawn(_cInfo);
                            Phrases.Dict.TryGetValue("VoteReward5", out string _phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            PersistentContainer.Instance.Players[_cInfo.playerId].VoteWeekCount = _voteWeekCount + 1;
                            int _remainingVotes = Weekly_Votes - _voteWeekCount + 1;
                            DateTime _date2 = _lastVoteWeek.AddDays(7);
                            Phrases.Dict.TryGetValue("VoteReward6", out string _phrase);
                            _phrase = _phrase.Replace("{Value}", _voteWeekCount + 1.ToString());
                            _phrase = _phrase.Replace("{Date}", _lastVoteWeek.ToString());
                            _phrase = _phrase.Replace("{Value2}", _remainingVotes.ToString());
                            _phrase = _phrase.Replace("{Date2}", _date2.ToString());
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].VoteWeekCount = 1;
                        PersistentContainer.Instance.Players[_cInfo.playerId].LastVoteWeek = DateTime.Now;
                        int _remainingVotes = Weekly_Votes - 1;
                        DateTime _date2 = DateTime.Now.AddDays(7);
                        Phrases.Dict.TryGetValue("VoteReward6", out string _phrase);
                        _phrase = _phrase.Replace("{Value}", 1.ToString());
                        _phrase = _phrase.Replace("{Date}", _lastVoteWeek.ToString());
                        _phrase = _phrase.Replace("{Value2}", _remainingVotes.ToString());
                        _phrase = _phrase.Replace("{Date2}", _date2.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    PersistentContainer.DataChange = true;
                }
                Phrases.Dict.TryGetValue("VoteReward7", out string _phrase1);
                _phrase1 = _phrase1.Replace("{Value}", Delay_Between_Uses.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                if (_player.IsSpawned())
                {
                    Vector3 pos = _player.GetPosition();
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
                                Entity entity = EntityFactory.CreateEntity(i, new Vector3((float)_x, (float)_y, (float)_z));
                                GameManager.Instance.World.SpawnEntityInWorld(entity);
                                Log.Out(string.Format("[SERVERTOOLS] Spawned an entity reward {0} at {1} x, {2} y, {3} z for {4}", eClass.entityClassName, _x, _y, _z, _cInfo.playerName));
                                Phrases.Dict.TryGetValue("VoteReward9", out string _phrase);
                                _phrase = _phrase.Replace("{EntityName}", eClass.entityClassName);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                        Phrases.Dict.TryGetValue("VoteReward10", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in VoteReward.Entityspawn: {0}", e.Message));
            }
        }

        private static void UpgradeXml(XmlNodeList _oldChildNodes)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<VoteRewards>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("<!-- Items that do not require a quality should be set to 0 or 1 for min and max -->");
                    sw.WriteLine("<!-- WalletCoin can be used as the item name -->");
                    sw.WriteLine("<!-- <Reward ItemOrBlock=\"meleeToolTorch\" MinCount=\"5\" MaxCount=\"10\" MinQuality=\"1\" MaxQuality=\"1\" /> -->");
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType == XmlNodeType.Comment && !_oldChildNodes[i].OuterXml.StartsWith("<!-- Items that do not") &&
                            !_oldChildNodes[i].OuterXml.StartsWith("<!-- WalletCoin can be") && !_oldChildNodes[i].OuterXml.StartsWith("<!-- <Reward ItemOrBlock=\"meleeToolTorch\"") &&
                            !_oldChildNodes[i].OuterXml.StartsWith("    <!-- <Reward ItemOrBlock=\"\""))
                        {
                            sw.WriteLine(_oldChildNodes[i].OuterXml);
                        }
                    }
                    sw.WriteLine();
                    sw.WriteLine();
                    bool _blank = true;
                    for (int i = 0; i < _oldChildNodes.Count; i++)
                    {
                        if (_oldChildNodes[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement _line = (XmlElement)_oldChildNodes[i];
                            if (_line.HasAttributes && _line.Name == "Reward")
                            {
                                _blank = false;
                                string _itemBlock = "", _minCount = "", _maxCount = "", _minQuality = "", _maxQuality = "";
                                if (_line.HasAttribute("ItemOrBlock"))
                                {
                                    _itemBlock = _line.GetAttribute("ItemOrBlock");
                                }
                                if (_line.HasAttribute("MinCount"))
                                {
                                    _minCount = _line.GetAttribute("MinCount");
                                }
                                if (_line.HasAttribute("MaxCount"))
                                {
                                    _maxCount = _line.GetAttribute("MaxCount");
                                }
                                if (_line.HasAttribute("MinQuality"))
                                {
                                    _minQuality = _line.GetAttribute("MinQuality");
                                }
                                if (_line.HasAttribute("MaxQuality"))
                                {
                                    _maxQuality = _line.GetAttribute("MaxQuality");
                                }
                                sw.WriteLine(string.Format("    <Reward ItemOrBlock=\"{0}\" MinCount=\"{1}\" MaxCount=\"{2}\" MinQuality=\"{3}\" MaxQuality=\"{4}\" />", _itemBlock, _minCount, _maxCount, _minQuality, _maxQuality));
                            }
                        }
                    }
                    if (_blank)
                    {
                        sw.WriteLine("    <!-- <Reward ItemOrBlock=\"\" MinCount=\"\" MaxCount=\"\" MinQuality=\"\" MaxQuality=\"\" /> -->");
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

