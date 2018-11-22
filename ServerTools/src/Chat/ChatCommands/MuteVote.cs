using System.Collections.Generic;
using System.Linq;

namespace ServerTools
{
    class MuteVote
    {
        public static bool IsEnabled = false, VoteOpen = false;
        public static int Players_Online = 5, Votes_Needed = 3;
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
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase775 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global);
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
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", this player id was not found.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper);
                        }
                    }
                }
                else
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", not enough players are online to start a vote to mute.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper);
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
                    _phrase777 = "has been muted for 60 minutes.";
                }
                ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase777 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global);
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", LoadConfig.Chat_Response_Color, _phrase777), LoadConfig.Server_Response_Name, false, "", false);
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
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Player name = {1} # = {2}.[-]", LoadConfig.Chat_Response_Color, _cInfo2.playerName, _cInfo2.entityId), LoadConfig.Server_Response_Name, false, "ServerTools", false));
                }
            }
            string _phrase778;
            if (!Phrases.Dict.TryGetValue(778, out _phrase778))
            {
                _phrase778 = "type /mute # to start a vote to mute that player from chat.";
            }
            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase778 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper);
        }
    }
}
