using System.Collections.Generic;
using System.Linq;

namespace ServerTools
{
    class KickVote
    {
        public static bool IsEnabled = false, VoteOpen = false;
        public static int Players_Online = 5, Votes_Needed = 3;
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
                                _phrase955 = "A vote to kick {PlayerName} has begun and will close in 30 seconds.";
                            }
                            _phrase955 = _phrase955.Replace("{PlayerName}", _playerInfo.playerName);
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase955 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global);
                            string _phrase776;
                            if (!Phrases.Dict.TryGetValue(776, out _phrase776))
                            {
                                _phrase776 = "Type /yes to cast your vote.";
                            }
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase776 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global);
                            VoteOpen = true;
                        }
                        else
                        {
                            string _phrase956;
                            if (!Phrases.Dict.TryGetValue(956, out _phrase956))
                            {
                                _phrase956 = "this player id was not found.";
                            }
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase956 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper);
                        }
                    }
                }
                else
                {
                    string _phrase957;
                    if (!Phrases.Dict.TryGetValue(957, out _phrase957))
                    {
                        _phrase957 = "not enough players are online to start a vote to kick.";
                    }
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase957 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper);
                }
            }
        }

        public static void VoteCount()
        {
            if (Kick.Count >= Votes_Needed)
            {
                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"The players have kicked you from the game\"", _playerKick.entityId), (ClientInfo)null);
            }
            Kick.Clear();
            VoteOpen = false;
        }

        public static void List(ClientInfo _cInfo)
        {
            List<ClientInfo> _cInfoList = ConnectionManager.Instance.Clients.List.ToList();
            for (int i = 0; i < _cInfoList.Count; i++)
            {
                ClientInfo _cInfo2 = _cInfoList[i];
                if (_cInfo2 != _cInfo)
                {
                    string _phrase958;
                    if (!Phrases.Dict.TryGetValue(958, out _phrase958))
                    {
                        _phrase958 = "PlayerName = {PlayerName}, # = {Id}.";
                    }
                    _phrase958 = _phrase958.Replace("{PlayerName}", _cInfo2.playerName);
                    _phrase958 = _phrase958.Replace("{Id}", _cInfo2.entityId.ToString());
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase958 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper);
                }
            }
            string _phrase959;
            if (!Phrases.Dict.TryGetValue(959, out _phrase959))
            {
                _phrase959 = "type /kickvote # to start a vote to kick that player.";
            }
            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase959 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper);
        }
    }
}
