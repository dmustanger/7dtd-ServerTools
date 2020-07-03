using System.Collections.Generic;

namespace ServerTools
{
    class KickVote
    {
        public static bool IsEnabled = false, VoteOpen = false;
        public static int Players_Online = 5, Votes_Needed = 3;
        public static string Command68 = "kickvote";
        public static List<int> Kick = new List<int>();
        private static ClientInfo _playerKick;

        public static void Vote(ClientInfo _cInfo, string _player)
        {
            if (!VoteOpen)
            {
                int _playerCount = ConnectionManager.Instance.ClientCount();
                if (_playerCount >= Players_Online)
                {
                    int _entityId;
                    if (int.TryParse(_player, out _entityId))
                    {
                        ClientInfo _playerInfo = ConnectionManager.Instance.Clients.ForEntityId(_entityId);
                        if (_playerInfo != null)
                        {
                            _playerKick = _playerInfo;
                            string _phrase955;
                            if (!Phrases.Dict.TryGetValue(955, out _phrase955))
                            {
                                _phrase955 = "A vote to kick {PlayerName} has begun and will close in 60 seconds.";
                            }
                            _phrase955 = _phrase955.Replace("{PlayerName}", _playerInfo.playerName);
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase955 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                            string _phrase776;
                            if (!Phrases.Dict.TryGetValue(776, out _phrase776))
                            {
                                _phrase776 = "Type {CommandPrivate}{Command70} to cast your vote.";
                            }
                            _phrase776 = _phrase776.Replace("{CommandPrivate}", ChatHook.Command_Private);
                            _phrase776 = _phrase776.Replace("{Command70}", RestartVote.Command70);
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase776 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                            VoteOpen = true;
                        }
                        else
                        {
                            string _phrase956;
                            if (!Phrases.Dict.TryGetValue(956, out _phrase956))
                            {
                                _phrase956 = "This player id was not found.";
                            }
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase956 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
                else
                {
                    string _phrase957;
                    if (!Phrases.Dict.TryGetValue(957, out _phrase957))
                    {
                        _phrase957 = "Not enough players are online to start a vote to kick.";
                    }
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase957 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void ProcessKickVote()
        {
            if (Kick.Count > 0)
            {
                if (Kick.Count >= Votes_Needed)
                {
                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"The players have kicked you from the game\"", _playerKick.entityId), (ClientInfo)null);
                }
                else
                {
                    string _message = "Players voted to kick but not enough votes were cast.";
                    ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                }
            }
            else
            {
                string _message = "No votes were cast to kick the player";
                ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _message + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
            }
            Kick.Clear();
            VoteOpen = false;
        }

        public static void List(ClientInfo _cInfo)
        {
            bool _otherUser = false;
            List<ClientInfo> ClientInfoList = PersistentOperations.ClientList();
            for (int i = 0; i < ClientInfoList.Count; i++)
            {
                ClientInfo _cInfo2 = ClientInfoList[i];
                if (_cInfo2 != _cInfo)
                {
                    _otherUser = true;
                    string _phrase958;
                    if (!Phrases.Dict.TryGetValue(958, out _phrase958))
                    {
                        _phrase958 = "PlayerName = {PlayerName}, # = {Id}.";
                    }
                    _phrase958 = _phrase958.Replace("{PlayerName}", _cInfo2.playerName);
                    _phrase958 = _phrase958.Replace("{Id}", _cInfo2.entityId.ToString());
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase958 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            if (_otherUser)
            {
                string _phrase959;
                if (!Phrases.Dict.TryGetValue(959, out _phrase959))
                {
                    _phrase959 = "Type {CommandPrivate}{Command68} # to start a vote to kick that player.";
                }
                _phrase959 = _phrase959.Replace("{CommandPrivate}", ChatHook.Command_Private);
                _phrase959 = _phrase959.Replace("{Command68}", Command68);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase959 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "No other users were found online" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}
