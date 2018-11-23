using System.Collections.Generic;
using System.Linq;

namespace ServerTools
{
    class PlayerList
    {
        public static bool IsEnabled = false;

        public static void Exec(ClientInfo _cInfo, string _playerName)
        {
            List<ClientInfo> _cInfoList = ConnectionManager.Instance.Clients.List.ToList();
            for (int i = 0; i < _cInfoList.Count; i++)
            {
                ClientInfo _cInfo1 = _cInfoList[i];
                if (_cInfo != _cInfo1)
                {
                    string _response = "Player name {PlayerName}, Id = {EntityId}[-]";
                    _response = _response.Replace("{PlayerName}", _cInfo1.playerName);
                    _response = _response.Replace("{EntityId}", _cInfo1.playerName);
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _response, _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }
    }
}
