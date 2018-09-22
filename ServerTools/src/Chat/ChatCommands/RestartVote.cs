using System.Collections.Generic;

namespace ServerTools
{
    class RestartVote
    {
        public static bool IsEnabled = false, VoteOpen = false, Startup = false;
        public static int Players_Online = 5, Votes_Needed = 3, Admin_Level = 0;
        public static List<int> Restart = new List<int>();
        private static bool StartedVote = false;

        public static void CallForVote1(ClientInfo _cInfo)
        {
            if (!Startup)
            {
                if (!StartedVote)
                {
                    bool adminOnline = false;
                    List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                    for (int i = 0; i < _cInfoList.Count; i++)
                    {
                        ClientInfo _cInfoAdmins = _cInfoList[i];
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
                            _cInfoAdmins.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase748), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                    }
                    if (!adminOnline)
                    {

                        int _playerCount = ConnectionManager.Instance.ClientCount();
                        if (_playerCount >= Players_Online)
                        {
                            StartedVote = true;
                            string _phrase740;
                            if (!Phrases.Dict.TryGetValue(740, out _phrase740))
                            {
                                _phrase740 = "A vote to restart the server has opened and will close in 30 seconds. Type /yes to cast your vote.";
                            }
                            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase740), Config.Server_Response_Name, false, "", false);
                        }
                        else
                        {
                            string _phrase741;
                            if (!Phrases.Dict.TryGetValue(741, out _phrase741))
                            {
                                _phrase741 = "There are not enough players online to start a restart vote.";
                            }
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase741), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                    }
                    else
                    {
                        string _phrase749;
                        if (!Phrases.Dict.TryGetValue(749, out _phrase749))
                        {
                            _phrase749 = "A administrator is currently online. They have been alerted.";
                        }
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase749), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
                else
                {
                    string _phrase824;
                    if (!Phrases.Dict.TryGetValue(824, out _phrase824))
                    {
                        _phrase824 = "{PlayerName} there is a vote already open.";
                    }
                    _phrase824 = _phrase824.Replace("{PlayerName}", _cInfo.playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase824), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
            else
            {
                string _phrase816;
                if (!Phrases.Dict.TryGetValue(816, out _phrase816))
                {
                    _phrase816 = "You must wait thirty minutes after the server starts before opening a restart vote.";
                }
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase816), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void CallForVote2()
        {
            if (Restart.Count > 0)
            {
                if (Restart.Count >= Votes_Needed)
                {
                    SdtdConsole.Instance.ExecuteSync(string.Format("stopserver 1"), (ClientInfo)null);
                    string _phrase742;
                    if (!Phrases.Dict.TryGetValue(742, out _phrase742))
                    {
                        _phrase742 = "Players voted yes to a server restart. Shutdown has been initiated.";
                    }
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase742), Config.Server_Response_Name, false, "", false);
                    SdtdConsole.Instance.ExecuteSync(string.Format("stopserver 1"), (ClientInfo)null);
                }
                else
                {
                    string _phrase743;
                    if (!Phrases.Dict.TryGetValue(743, out _phrase743))
                    {
                        _phrase743 = "Players voted yes but not enough votes were cast to restart.";
                    }
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase743), Config.Server_Response_Name, false, "", false);
                }
            }
            else
            {
                string _phrase746;
                if (!Phrases.Dict.TryGetValue(746, out _phrase746))
                {
                    _phrase746 = "No votes were cast to restart the server.";
                }
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase746), Config.Server_Response_Name, false, "", false);
            }
            Restart.Clear();
            VoteOpen = false;
        }
    }
}
