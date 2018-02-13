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
        public static bool IsEnabled = false;
        public static bool IsRunning = false;
        private const string file = "VoteReward.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        public static SortedDictionary<string, int[]> voteRewardList = new SortedDictionary<string, int[]>();
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        private static bool updateConfig = false;
        public static string YourVotingSite = ("https://7daystodie-servers.com/server/12345");
        public static string APIKey = ("xxxxxxxx");
        public static int DelayBetweenRewards = 24;

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
                    voteRewardList.Clear();
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Rewards' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("item"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring reward entry because of missing item attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("countMin"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring reward entry because of missing countMin attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("countMax"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring reward entry because of missing countMax attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("qualityMin"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring reward entry because of missing qualityMin attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("qualityMax"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring reward entry because of missing qualityMax attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        int _countMin = 1;
                        int _countMax = 1;
                        int _qualityMin = 1;
                        int _qualityMax = 1;
                        if (!int.TryParse(_line.GetAttribute("countMin"), out _countMin))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring reward entry because of invalid (non-numeric) value for 'count' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("countMax"), out _countMax))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring reward entry because of invalid (non-numeric) value for 'count' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("qualityMin"), out _qualityMin))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring reward entry because of invalid (non-numeric) value for 'quality' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!int.TryParse(_line.GetAttribute("qualityMax"), out _qualityMax))
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Ignoring reward entry because of invalid (non-numeric) value for 'quality' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string item = _line.GetAttribute("item");
                        if (!voteRewardList.ContainsKey(item))
                        {
                            int[] _c = new int[] { _countMin, _countMax, _qualityMin, _qualityMax };
                            voteRewardList.Add(item, _c);
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
                sw.WriteLine("<RewardItems>");
                sw.WriteLine("    <Rewards>");
                if (voteRewardList.Count > 0)
                {
                    foreach (KeyValuePair<string, int[]> kvp in voteRewardList)
                    {
                        sw.WriteLine(string.Format("        <reward item=\"{0}\" countMin=\"{1}\" countMax=\"{2}\" qualityMin=\"{3}\" qualityMax=\"{4}\" />", kvp.Key, kvp.Value[0], kvp.Value[1], kvp.Value[2], kvp.Value[3]));
                    }
                }
                else
                {
                    sw.WriteLine("        <reward item=\"torch\" countMin=\"5\" countMax=\"10\" qualityMin=\"1\" qualityMax=\"1\" />");
                    sw.WriteLine("        <reward item=\"9mmBullet\" countMin=\"10\" countMax=\"30\" qualityMin=\"1\" qualityMax=\"1\" />");
                    sw.WriteLine("        <reward item=\"44MagBullet\" countMin=\"5\" countMax=\"10\" qualityMin=\"1\" qualityMax=\"1\" />");
                    sw.WriteLine("        <reward item=\"ironChestArmor\" countMin=\"1\" countMax=\"1\" qualityMin=\"1\" qualityMax=\"600\" />");
                    sw.WriteLine("        <reward item=\"plantedCorn1\" countMin=\"5\" countMax=\"10\" qualityMin=\"1\" qualityMax=\"1\" />");
                }
                sw.WriteLine("    </Rewards>");
                sw.WriteLine("</RewardItems>");
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

        public static void CheckReward(ClientInfo _cInfo)
        {
            if (DelayBetweenRewards == 0)
            {
                Execute(_cInfo);
            }
            else
            {
                Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                if (p == null || p.LastVoteReward == null)
                {
                    Execute(_cInfo);
                }
                else
                {
                    TimeSpan varTime = DateTime.Now - p.LastVoteReward;
                    double fractionalHours = varTime.TotalHours;
                    int _timepassed = (int)fractionalHours;
                    if (_timepassed > DelayBetweenRewards)
                    {
                        Execute(_cInfo);
                    }
                    else
                    {
                        int _timeleft = DelayBetweenRewards - _timepassed;
                        string _phrase602;
                        if (!Phrases.Dict.TryGetValue(602, out _phrase602))
                        {
                            _phrase602 = "{PlayerName} you can only use /reward once every {DelayBetweenRewards} hours. Time remaining: {TimeRemaining} hour(s).";
                        }
                        string cinfoName = _cInfo.playerName;
                        _phrase602 = _phrase602.Replace("{PlayerName}", cinfoName);
                        _phrase602 = _phrase602.Replace("{DelayBetweenRewards}", DelayBetweenRewards.ToString());
                        _phrase602 = _phrase602.Replace("{TimeRemaining}", _timeleft.ToString());
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.ChatColor, _phrase602), "Server", false, "", false));
                        }
                    }
                }
            }
        }

        private static void Execute(ClientInfo _cInfo)
        {
            ServicePointManager.ServerCertificateValidationCallback += (send, certificate, chain, sslPolicyErrors) => { return true; };
            var VoteUrl = string.Format("https://7daystodie-servers.com/api/?object=votes&element=claim&key={0}&username={1}", Uri.EscapeUriString(APIKey), Uri.EscapeUriString(_cInfo.playerName));
            using (var NewVote = new WebClient())
            {
                var VoteResult = string.Empty;
                VoteResult = NewVote.DownloadString(VoteUrl);
                if (VoteResult == "0")
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Your vote has not been located {1}. Make sure you voted @ {2} and try again.[-]", Config.ChatColor, _cInfo.playerName, YourVotingSite), "Server", false, "", false));
                }
                if (VoteResult == "1")
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Thank you for your vote {1}. You can vote and receive another reward in {2} hours[-]", Config.ChatColor, _cInfo.playerName, DelayBetweenRewards), "Server", false, "", false));
                    foreach (KeyValuePair<string, int[]> kvp in voteRewardList)
                    {
                        System.Random rnd = new System.Random();
                        int count = rnd.Next(kvp.Value[0], kvp.Value[1]);
                        int quality = rnd.Next(kvp.Value[2], kvp.Value[3]);

                        ItemValue itemValue;
                        if (!ItemClass.ItemNames.Contains(kvp.Key))
                        {
                            Log.Out(string.Format("[SERVERTOOLS]Unable to find reward item {0}", kvp.Key));
                            continue;
                        }
                        else
                        {
                            itemValue = new ItemValue(ItemClass.GetItem(kvp.Key).type, quality, quality, true);
                            if (Equals(itemValue, ItemValue.None))
                            {
                                Log.Out(string.Format("[SERVERTOOLS]Unable to find reward item {0}", kvp.Key));
                                continue;
                            }
                            else
                            {
                                World world = GameManager.Instance.World;
                                if (world.Players.dict[_cInfo.entityId].IsSpawned())
                                {
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
                                    _cInfo.SendPackage(new NetPackageEntityCollect(entityItem.entityId, _cInfo.entityId));
                                    world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Killed);
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1} {2} was sent to your inventory. If your bag is full, check the ground[-]", Config.ChatColor, count, itemValue.ItemClass.localizedName ?? itemValue.ItemClass.Name), "Server", false, "", false));
                                }
                                else
                                {
                                    Log.Out(string.Format("[SERVERTOOLS]Player with steamdId {0} is not spawned. No reward given", _cInfo));
                                }
                            }
                        }
                    }
                    PersistentContainer.Instance.Players[_cInfo.playerId, true].LastVoteReward = DateTime.Now;
                    PersistentContainer.Instance.Save();
                }
            }
        }
    }
}
