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
                        string _phrase775;
                        if (!Phrases.Dict.TryGetValue(775, out _phrase775))
                        {
                            _phrase775 = "A vote to kick {PlayerName} has begun and will close in 30 seconds.";
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
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Not enough players are online to start a vote to kick.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}PlayerName = {1} # = {2}.[-]", Config.Chat_Response_Color, _cInfo2.playerName, _cInfo2.entityId), Config.Server_Response_Name, false, "ServerTools", false));
                }
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Type /kick # to start a vote to kick that player.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }
    }
}
