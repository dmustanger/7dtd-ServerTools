using System;
using System.Collections.Generic;

namespace ServerTools
{
    class MuteVote
    {
        public static bool IsEnabled = false, VoteOpen = false;
        public static int Players_Online = 5, Votes_Needed = 3;
        public static string Command_mutevote = "mutevote";
        public static List<int> Votes = new List<int>();
        private static ClientInfo _playerMute;

        public static void Vote(ClientInfo _cInfo, string _player)
        {
            if (!VoteOpen)
            {
                if (ConnectionManager.Instance.ClientCount() >= Players_Online)
                {
                    if (int.TryParse(_player, out int _entityId))
                    {
                        ClientInfo _playerToMute = ConnectionManager.Instance.Clients.ForEntityId(_entityId);
                        if (_playerToMute != null)
                        {
                            if (Mute.Mutes.Contains(_playerToMute.playerId))
                            {
                                Phrases.Dict.TryGetValue("MuteVote5", out string _phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                return;
                            }
                            _playerMute = _playerToMute;
                            VoteOpen = true;
                            Phrases.Dict.TryGetValue("MuteVote1", out string _phrase1);
                            _phrase1 = _phrase1.Replace("{PlayerName}", _playerToMute.playerName);
                            ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                            Phrases.Dict.TryGetValue("MuteVote2", out _phrase1);
                            _phrase1 = _phrase1.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                            _phrase1 = _phrase1.Replace("{Command_yes}", RestartVote.Command_yes);
                            ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("MuteVote6", out string _phrase);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("MuteVote7", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void ProcessMuteVote()
        {
            if (MuteVote.Votes.Count >= Votes_Needed)
            {
                Mute.Mutes.Add(_playerMute.playerId);
                PersistentContainer.Instance.Players[_playerMute.playerId].MuteTime = 60;
                PersistentContainer.Instance.Players[_playerMute.playerId].MuteName = _playerMute.playerName;
                PersistentContainer.Instance.Players[_playerMute.playerId].MuteDate = DateTime.Now;
                PersistentContainer.DataChange = true;
                Phrases.Dict.TryGetValue("MuteVote3", out string _phrase);
                _phrase = _phrase.Replace("{PlayerName}", _playerMute.playerName);
                ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
            }
            _playerMute = null;
            MuteVote.Votes.Clear();
        }

        public static void List(ClientInfo _cInfo)
        {
            List<ClientInfo> ClientInfoList = PersistentOperations.ClientList();
            if (ClientInfoList != null && ClientInfoList.Count > 0)
            {
                for (int i = 0; i < ClientInfoList.Count; i++)
                {
                    ClientInfo _cInfo2 = ClientInfoList[i];
                    if (_cInfo2 != _cInfo)
                    {
                        Phrases.Dict.TryGetValue("MuteVote8", out string _phrase);
                        _phrase = _phrase.Replace("{PlayerName}", _cInfo2.playerName);
                        _phrase = _phrase.Replace("{Id}", _cInfo2.entityId.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);

                    }
                }
                Phrases.Dict.TryGetValue("MuteVote4", out string _phrase1);
                _phrase1 = _phrase1.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                _phrase1 = _phrase1.Replace("{Command_mutevote}", Command_mutevote);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                Phrases.Dict.TryGetValue("MuteVote9", out string _phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}
