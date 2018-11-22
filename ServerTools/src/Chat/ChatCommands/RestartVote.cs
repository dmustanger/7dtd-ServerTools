using System.Collections.Generic;
using System.Linq;

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
                    List<ClientInfo> _cInfoList = ConnectionManager.Instance.Clients.List.ToList();
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
                            ChatHook.ChatMessage(_cInfoAdmins, LoadConfig.Chat_Response_Color + _cInfoAdmins.playerName + ", " + _phrase748 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper);
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
                            ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase740 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global);
                        }
                        else
                        {
                            string _phrase741;
                            if (!Phrases.Dict.TryGetValue(741, out _phrase741))
                            {
                                _phrase741 = "there are not enough players online to start a restart vote.";
                            }
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase741 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", LoadConfig.Chat_Response_Color, _phrase741), LoadConfig.Server_Response_Name, false, "ServerTools", false));
                        }
                    }
                    else
                    {
                        string _phrase749;
                        if (!Phrases.Dict.TryGetValue(749, out _phrase749))
                        {
                            _phrase749 = "a administrator is currently online. They have been alerted.";
                        }
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase749 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper);
                    }
                }
                else
                {
                    string _phrase824;
                    if (!Phrases.Dict.TryGetValue(824, out _phrase824))
                    {
                        _phrase824 = "there is a vote already open.";
                    }
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase824 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper);
                }
            }
            else
            {
                string _phrase816;
                if (!Phrases.Dict.TryGetValue(816, out _phrase816))
                {
                    _phrase816 = "you must wait thirty minutes after the server starts before opening a restart vote.";
                }
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase816 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper);
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
                    ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase742 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global);
                    SdtdConsole.Instance.ExecuteSync(string.Format("stopserver 1"), (ClientInfo)null);
                }
                else
                {
                    string _phrase743;
                    if (!Phrases.Dict.TryGetValue(743, out _phrase743))
                    {
                        _phrase743 = "Players voted yes but not enough votes were cast to restart.";
                    }
                    ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase743 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global);
                }
            }
            else
            {
                string _phrase746;
                if (!Phrases.Dict.TryGetValue(746, out _phrase746))
                {
                    _phrase746 = "No votes were cast to restart the server.";
                }
                ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase746 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global);
            }
            Restart.Clear();
            VoteOpen = false;
        }
    }
}
