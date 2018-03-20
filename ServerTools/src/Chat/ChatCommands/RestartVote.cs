using System.Collections.Generic;

namespace ServerTools
{
    class RestartVote
    {
        public static bool IsEnabled = false, VoteOpen = false, VoteNew = true;
        public static int Minimum_Players = 10, Admin_Level = 0;
        private static int minimumVotes = Minimum_Players / 2 + 1;
        public static List<int> yes = new List<int>();
        public static List<int> no = new List<int>();
        public static List<int> StartedVote = new List<int>();

        public static void CallForVote1(ClientInfo _cInfo)
        {
            bool adminOnline = false;
            List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
            foreach (var _cInfoAdmins in _cInfoList)
            {
                GameManager.Instance.adminTools.IsAdmin(_cInfoAdmins.playerId);
                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfoAdmins.playerId);
                if (Admin.PermissionLevel <= Admin_Level)
                {
                    adminOnline = true;
                    string _phrase748;
                    if (!Phrases.Dict.TryGetValue(748, out _phrase748))
                    {
                        _phrase748 = "{Player} has requested a restart vote.";
                    }
                    _phrase748 = _phrase748.Replace("{Player}", _cInfo.playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase748), "Server", false, "", false));
                }
            }
            if (!adminOnline)
            {
                if (!StartedVote.Contains(_cInfo.entityId))
                {
                    int _playerCount = ConnectionManager.Instance.ClientCount();
                    if (_playerCount >= Minimum_Players)
                    {
                        StartedVote.Clear();
                        StartedVote.Add(_cInfo.entityId);
                        string _phrase740;
                        if (!Phrases.Dict.TryGetValue(740, out _phrase740))
                        {
                            _phrase740 = "A vote to restart the server has opened and will close in 30 seconds. Type /yes or /no to cast your vote.";
                        }
                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase740), "Server", false, "", false);
                        VoteOpen = true;
                        if (!Timers.timer1Running)
                        {
                            Timers.TimerStart1Second();
                        }
                    }
                    else
                    {
                        string _phrase741;
                        if (!Phrases.Dict.TryGetValue(741, out _phrase741))
                        {
                            _phrase741 = "There are not enough players online to start a restart vote.";
                        }
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase741), "Server", false, "", false));
                    }
                }
                else
                {
                    string _phrase747;
                    if (!Phrases.Dict.TryGetValue(747, out _phrase747))
                    {
                        _phrase747 = "You started the last restart vote. Another player must initiate the next vote.";
                    }
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase747), "Server", false, "", false));
                }
            }
            else
            {
                string _phrase749;
                if (!Phrases.Dict.TryGetValue(749, out _phrase749))
                {
                    _phrase749 = "A administrator is currently online. They have been alerted.";
                }
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase749), "Server", false, "", false));
            }
        }

        public static void CallForVote2()
        {
            if (yes.Count > no.Count)
            {
                if (yes.Count >= minimumVotes)
                {
                    VoteNew = false;
                    SdtdConsole.Instance.ExecuteSync(string.Format("stopserver 1"), (ClientInfo)null);
                    string _phrase742;
                    if (!Phrases.Dict.TryGetValue(742, out _phrase742))
                    {
                        _phrase742 = "Players voted yes to a server restart. Shutdown has been initiated.";
                    }
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase742), "Server", false, "", false);
                    SdtdConsole.Instance.ExecuteSync(string.Format("stopserver 1"), (ClientInfo)null);
                }
                else
                {
                    VoteNew = false;
                    string _phrase743;
                    if (!Phrases.Dict.TryGetValue(743, out _phrase743))
                    {
                        _phrase743 = "Players voted yes but not enough votes cast to restart. A new vote can open in {RestartDelay} minutes.";
                    }
                    _phrase743 = _phrase743.Replace("{RestartDelay}", Timers.Restart_Vote_Delay.ToString());
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase743), "Server", false, "", false);
                }
            }
            else if (no.Count > yes.Count)
            {
                VoteNew = false;
                string _phrase744;
                if (!Phrases.Dict.TryGetValue(744, out _phrase744))
                {
                    _phrase744 = "Players voted no to a server restart. A new vote can open in {RestartDelay} minutes.";
                }
                _phrase744 = _phrase744.Replace("{RestartDelay}", Timers.Restart_Vote_Delay.ToString());
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase744), "Server", false, "", false);
            }
            else if (no.Count == yes.Count)
            {
                VoteNew = false;
                string _phrase745;
                if (!Phrases.Dict.TryGetValue(745, out _phrase745))
                {
                    _phrase745 = "The restart vote was a tie. A new vote can open in {RestartDelay} minutes.";
                }
                _phrase745 = _phrase745.Replace("{RestartDelay}", Timers.Restart_Vote_Delay.ToString());
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase745), "Server", false, "", false);
            }
            else
            {
                string _phrase746;
                if (!Phrases.Dict.TryGetValue(746, out _phrase746))
                {
                    _phrase746 = "No votes were cast to restart the server.";
                }
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase746), "Server", false, "", false);
            }
        }
    }
}
