using System.Collections.Generic;
using System.Linq;

namespace ServerTools
{
    class MuteVote
    {
        public static bool IsEnabled = false, VoteOpen = false;
        public static int Players_Online = 5, Votes_Needed = 3;
        public static string Command67 = "mutevote";
        public static List<int> Mute = new List<int>();
        private static ClientInfo _playerMute;

        public static void Vote(ClientInfo _cInfo, string _player)
        {
            if (!VoteOpen)
            {
                int _playerCount = ConnectionManager.Instance.ClientCount();
                if (_playerCount > Players_Online)
                {
                    int _entityId;
                    if (int.TryParse(_player, out _entityId))
                    {
                        ClientInfo _playerInfo = ConnectionManager.Instance.Clients.ForEntityId(_entityId);
                        if (_playerInfo != null)
                        {
                            _playerMute = _playerInfo;
                            string _phrase775;
                            if (!Phrases.Dict.TryGetValue(775, out _phrase775))
                            {
                                _phrase775 = "A vote to mute {PlayerName} in chat has begun and will close in 60 seconds.";
                            }
                            _phrase775 = _phrase775.Replace("{PlayerName}", _playerInfo.playerName);
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase775 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                            string _phrase776;
                            if (!Phrases.Dict.TryGetValue(776, out _phrase776))
                            {
                                _phrase776 = "Type {CommandPrivate}{Command70} to cast your vote.";
                            }
                            _phrase776 = _phrase776.Replace("{CommandPrivate}", ChatHook.Command_Private);
                            _phrase776 = _phrase776.Replace("{Command70}", RestartVote.Command70);
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase776 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                            VoteOpen = true;
                        }
                        else
                        {
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", this player id was not found.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", not enough players are online to start a vote to mute.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void VoteCount()
        {
            if (Mute.Count >= Votes_Needed)
            {
                MutePlayer.MuteVoteAdd(_playerMute);
                string _phrase777;
                if (!Phrases.Dict.TryGetValue(777, out _phrase777))
                {
                    _phrase777 = "{PlayerName} has been muted for 60 minutes.";
                }
                _phrase777 = _phrase777.Replace("{PlayerName}", _playerMute.playerName);
                ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase777 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
            }
            VoteOpen = false;
            Mute.Clear();
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
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase958 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            string _phrase778;
            if (!Phrases.Dict.TryGetValue(778, out _phrase778))
            {
                _phrase778 = " type {CommandPrivate}{Command67} # to start a vote to mute that player from chat.";
            }
            _phrase778 = _phrase778.Replace("{CommandPrivate}", ChatHook.Command_Private);
            _phrase778 = _phrase778.Replace("{Command67}", Command67);
            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase778 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
        }
    }
}
