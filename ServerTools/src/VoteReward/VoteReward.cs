using System;
using System.Net;
using UnityEngine;

namespace ServerTools
{
    class VoteReward
    {
        public static bool IsEnabled = false;
        public static string YourVotingSite = ("https://7daystodie-servers.com/server/12345");
        public static string APIKey = ("xxxxxxxx");
        public static bool RewardIsItemOrBlock = false;
        public static bool RewardIsEntity = false;
        public static string ItemOrBlock = ("votecrate");
        public static string Entity = ("sc_General");
        public static int DelayBetweenRewards = 24;

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
                    if (RewardIsItemOrBlock)
                    {
                        if (_cInfo.playerId != null)
                        {
                            int count = 1;
                            int min = 1;
                            int max = 600;

                            ItemValue itemValue;
                            var itemId = 4096;
                            int _itemId;
                            if (int.TryParse(ItemOrBlock, out _itemId))
                            {
                                int calc = (_itemId + 4096);
                                itemId = calc;
                                itemValue = ItemClass.list[itemId] == null ? ItemValue.None : new ItemValue(itemId, min, max, true);
                            }
                            else
                            {
                                if (!ItemClass.ItemNames.Contains(ItemOrBlock))
                                {
                                    Log.Out(string.Format("[SERVERTOOLS]Unable to find reward item {0}", ItemOrBlock));
                                    return;
                                }

                                itemValue = new ItemValue(ItemClass.GetItem(ItemOrBlock).type, min, max, true);
                            }

                            if (Equals(itemValue, ItemValue.None))
                            {
                                Log.Out(string.Format("[SERVERTOOLS]Unable to find reward item or block {0}", ItemOrBlock));
                                return;
                            }

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
                        else
                        {
                            Log.Out(string.Format("[SERVERTOOLS]Player with steamdId {0} does not exist. No reward given", _cInfo));
                        }
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].LastVoteReward = DateTime.Now;
                        PersistentContainer.Instance.Save();
                    }
                    if (RewardIsEntity)
                    {
                        SdtdConsole.Instance.ExecuteSync(string.Format("se {0} {1}", _cInfo.entityId, Entity), _cInfo);
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].LastVoteReward = DateTime.Now;
                        PersistentContainer.Instance.Save();
                    }
                }
            }
        }
    }
}
