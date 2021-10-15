using System.Collections.Generic;

namespace ServerTools
{
    class RestartVote
    {
        public static bool IsEnabled = false, VoteOpen = false, ThirtyMin = false;
        public static int Players_Online = 5, Votes_Needed = 3, Admin_Level = 0;
        public static string Command_restartvote = "restartvote", Command_yes = "yes";
        public static List<int> Restart = new List<int>();

        public static void CallForVote1(ClientInfo _cInfo)
        {
            if (!ThirtyMin)
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
                                    Phrases.Dict.TryGetValue("RestartVote9", out string _phrase);
                                    _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
                                    ChatHook.ChatMessage(_cInfoAdmins, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                        }
                    }
                    if (!_adminOnline)
                    {
                        if (ConnectionManager.Instance.ClientCount() >= Players_Online)
                        {
                            VoteOpen = true;
                            Phrases.Dict.TryGetValue("RestartVote1", out string _phrase);
                            _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                            _phrase = _phrase.Replace("{Command_yes}", Command_yes);
                            ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("RestartVote2", out string _phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("RestartVote10", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("RestartVote11", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else
            {
                Phrases.Dict.TryGetValue("RestartVote12", out string _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void ProcessRestartVote()
        {
            if (Restart.Count > 0)
            {
                if (Restart.Count >= Votes_Needed)
                {
                    Phrases.Dict.TryGetValue("RestartVote3", out string _phrase);
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    SdtdConsole.Instance.ExecuteSync(string.Format("st-StopServer 2"), (ClientInfo)null);
                }
                else
                {
                    Phrases.Dict.TryGetValue("RestartVote4", out string _phrase);
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                }
            }
            else
            {
                Phrases.Dict.TryGetValue("RestartVote7", out string _phrase);
                ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
            }
            Restart.Clear();;
        }
    }
}
