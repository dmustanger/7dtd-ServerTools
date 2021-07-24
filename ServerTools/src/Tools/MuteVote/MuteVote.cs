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
                                Phrases.Dict.TryGetValue(915, out string _phrase915);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase915 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                return;
                            }
                            _playerMute = _playerToMute;
                            VoteOpen = true;
                            Phrases.Dict.TryGetValue(911, out string _phrase911);
                            _phrase911 = _phrase911.Replace("{PlayerName}", _playerToMute.playerName);
                            ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase911 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                            Phrases.Dict.TryGetValue(912, out string _phrase912);
                            _phrase912 = _phrase912.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                            _phrase912 = _phrase912.Replace("{Command_yes}", RestartVote.Command_yes);
                            ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase912 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue(916, out string _phrase916);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase916 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue(917, out string _phrase917);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase917 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                Phrases.Dict.TryGetValue(913, out string _phrase913);
                _phrase913 = _phrase913.Replace("{PlayerName}", _playerMute.playerName);
                ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase913 + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
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
                        Phrases.Dict.TryGetValue(918, out string _phrase918);
                        _phrase918 = _phrase918.Replace("{PlayerName}", _cInfo2.playerName);
                        _phrase918 = _phrase918.Replace("{Id}", _cInfo2.entityId.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase918 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);

                    }
                }
                Phrases.Dict.TryGetValue(914, out string _phrase914);
                _phrase914 = _phrase914.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                _phrase914 = _phrase914.Replace("{Command_mutevote}", Command_mutevote);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase914 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                Phrases.Dict.TryGetValue(919, out string _phrase919);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase919 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}
