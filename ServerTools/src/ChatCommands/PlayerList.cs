using System.Collections.Generic;

namespace ServerTools
{
    class PlayerList
    {
        public static bool IsEnabled = false;

        public static void Exec(ClientInfo _cInfo, string _playerName)
        {
            List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
            for (int i = 0; i < _cInfoList.Count; i++)
            {
                ClientInfo _cInfo1 = _cInfoList[i];
                if (_cInfo != _cInfo1)
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Player name {1}, Id = {2}[-]", Config.Chat_Response_Color, _cInfo1.playerName, _cInfo1.entityId), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
        }
    }
}
