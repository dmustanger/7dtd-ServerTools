using System;
using System.Net;

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
        private static int DelayBetweenUses = 24;

        public static void CheckReward(ClientInfo _cInfo)
        {
            if (DelayBetweenUses == 0)
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
                    if (_timepassed > DelayBetweenUses)
                    {
                        Execute(_cInfo);
                    }
                    else
                    {
                        int _timeleft = DelayBetweenUses - _timepassed;
                        string _phrase602;
                        if (!Phrases.Dict.TryGetValue(602, out _phrase602))
                        {
                            _phrase602 = "{PlayerName} you can only use /reward once every 24 hours. Time remaining: {TimeRemaining} hour(s).";
                        }
                        string cinfoName = _cInfo.playerName;
                        _phrase602 = _phrase602.Replace("{PlayerName}", cinfoName);
                        _phrase602 = _phrase602.Replace("{TimeRemaining}", _timeleft.ToString());
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _phrase602), "Server", false, "", false));
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
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Your vote has not been located {1}. Make sure you voted @ {2} and try again.[-]", CustomCommands.ChatColor, _cInfo.playerName, YourVotingSite), "Server", false, "", false));
                }
                if (VoteResult == "1")
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Thank you for your vote {1}. You can vote and receive another reward in 24 hours[-]", CustomCommands.ChatColor, _cInfo.playerName), "Server", false, "", false));
                    if (RewardIsItemOrBlock)
                    {
                        SdtdConsole.Instance.ExecuteSync(string.Format("give {0} {1} {2}", _cInfo.playerId, ItemOrBlock, 1), _cInfo);
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
