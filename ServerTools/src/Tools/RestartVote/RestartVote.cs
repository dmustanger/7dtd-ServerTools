using System.Collections.Generic;

namespace ServerTools
{
    class RestartVote
    {
        public static bool IsEnabled = false, VoteOpen = false, Cycle = false;
        public static int Players_Online = 5, Votes_Needed = 3, Admin_Level = 0;
        public static string Command66 = "restartvote", Command70 = "yes";
        public static List<int> Restart = new List<int>();

        public static void CallForVote1(ClientInfo _cInfo)
        {
            if (!Cycle)
            {
                if (!VoteOpen)
                {
                    bool _adminOnline = false;
                    List<ClientInfo> ClientInfoList = PersistentOperations.ClientList();
                    if (ClientInfoList != null && ClientInfoList.Count > 0)
                    {
                        for (int i = 0; i < ClientInfoList.Count; i++)
                        {
                            ClientInfo _cInfoAdmins = ClientInfoList[i];
                            if (_cInfo != _cInfoAdmins)
                            {
                                if (GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo) <= Admin_Level)
                                {
                                    _adminOnline = true;
                                    Phrases.Dict.TryGetValue(449, out string _phrase449);
                                    _phrase449 = _phrase449.Replace("{PlayerName}", _cInfo.playerName);
                                    ChatHook.ChatMessage(_cInfoAdmins, Config.Chat_Response_Color + _phrase449 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                        }
                    }
                    if (!_adminOnline)
                    {
                        if (ConnectionManager.Instance.ClientCount() >= Players_Online)
                        {
                            VoteOpen = true;
                            Phrases.Dict.TryGetValue(441, out string _phrase441);
                            _phrase441 = _phrase441.Replace("{CommandPrivate}", ChatHook.Command_Private);
                            _phrase441 = _phrase441.Replace("{Command70}", Command70);
                            ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase441 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue(442, out string _phrase442);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase442 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue(450, out string _phrase450);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase450 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue(451, out string _phrase451);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase451 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                Phrases.Dict.TryGetValue(452, out string _phrase452);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase452 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void ProcessRestartVote()
        {
            if (Restart.Count > 0)
            {
                if (Restart.Count >= Votes_Needed)
                {
                    Phrases.Dict.TryGetValue(443, out string _phrase443);
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase443 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    SdtdConsole.Instance.ExecuteSync(string.Format("st-StopServer 2"), (ClientInfo)null);
                }
                else
                {
                    Phrases.Dict.TryGetValue(444, out string _phrase444);
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase444 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                }
            }
            else
            {
                Phrases.Dict.TryGetValue(447, out string _phrase447);
                ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase447 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
            }
            Restart.Clear();;
        }
    }
}
