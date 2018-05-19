using System.Collections.Generic;

namespace ServerTools
{
    class KickVote
    {
        public static bool IsEnabled = false, VoteOpen = false;
        public static List<int> Kick = new List<int>();
        private static ClientInfo _playerKick;

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
                        _playerKick = _playerInfo;
                        string _phrase955;
                        if (!Phrases.Dict.TryGetValue(955, out _phrase955))
                        {
                            _phrase955 = "A vote to kick {PlayerName} has begun and will close in 30 seconds.";
                        }
                        _phrase955 = _phrase955.Replace("{PlayerName}", _playerInfo.playerName);
                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase955), Config.Server_Response_Name, false, "", false);
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
                        string _phrase956;
                        if (!Phrases.Dict.TryGetValue(956, out _phrase956))
                        {
                            _phrase956 = "This player id was not found.";
                        }
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase956), Config.Server_Response_Name, false, "ServerTools", false));
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
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase957), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void VoteCount()
        {
            if (Kick.Count > 7)
            {
                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"The players have kicked you from the game\"", _playerKick.entityId), (ClientInfo)null);
            }
            VoteOpen = false;
            Kick.Clear();
        }

        public static void List(ClientInfo _cInfo)
        {
            List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
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
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase958), Config.Server_Response_Name, false, "ServerTools", false));
                }
                string _phrase959;
                if (!Phrases.Dict.TryGetValue(959, out _phrase959))
                {
                    _phrase959 = "Type /kick # to start a vote to kick that player.";
                }
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase959), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }
    }
}
