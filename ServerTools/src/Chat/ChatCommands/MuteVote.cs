using System.Collections.Generic;

namespace ServerTools
{
    class MuteVote
    {
        public static bool IsEnabled = false, VoteOpen = false;
        public static List<int> Mute = new List<int>();
        private static ClientInfo _playerMute;

        public static void Vote(ClientInfo _cInfo, string _player)
        {
            int _playerCount = ConnectionManager.Instance.ClientCount();
            if (_playerCount > 9)
            {
                int _entityId;
                if (int.TryParse(_player, out _entityId))
                {
                    ClientInfo _playerInfo = ConnectionManager.Instance.GetClientInfoForEntityId(_entityId);
                    if (_playerInfo != null)
                    {
                        _playerMute = _playerInfo;
                        string _phrase775;
                        if (!Phrases.Dict.TryGetValue(775, out _phrase775))
                        {
                            _phrase775 = "A vote to mute {PlayerName} in chat has begun and will close in 30 seconds.";
                        }
                        _phrase775 = _phrase775.Replace("{PlayerName}", _playerInfo.playerName);
                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase775), Config.Server_Response_Name, false, "", false);
                        string _phrase776;
                        if (!Phrases.Dict.TryGetValue(776, out _phrase776))
                        {
                            _phrase776 = "Type /yes to cast your vote.";
                        }
                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase776), Config.Server_Response_Name, false, "", false);
                        VoteOpen = true;
                    }
                    else
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}This player id was not found.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Not enough players are online to start a vote to mute.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void VoteCount()
        {
            if (Mute.Count > 7)
            {
                SdtdConsole.Instance.ExecuteSync(string.Format("mute add {0}", _playerMute.playerId), (ClientInfo)null);
                string _phrase777;
                if (!Phrases.Dict.TryGetValue(777, out _phrase777))
                {
                    _phrase777 = "{PlayerName} has been muted for 30 minutes.";
                }
                _phrase777 = _phrase777.Replace("{PlayerName}", _playerMute.playerName);
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase777), Config.Server_Response_Name, false, "", false);
            }
            VoteOpen = false;
            Mute.Clear();
        }

        public static void List(ClientInfo _cInfo)
        {
            List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
            for (int i = 0; i < _cInfoList.Count; i++)
            {
                ClientInfo _cInfo2 = _cInfoList[i];
                if (_cInfo2 != _cInfo)
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Player name = {1} # = {2}.[-]", Config.Chat_Response_Color, _cInfo2.playerName, _cInfo2.entityId), Config.Server_Response_Name, false, "ServerTools", false));
                }
                string _phrase778;
                if (!Phrases.Dict.TryGetValue(778, out _phrase778))
                {
                    _phrase778 = "Type /mute # to start a vote to mute that player in chat.";
                }
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase778), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }
    }
}
