using System.Collections.Generic;
using System.Linq;

namespace ServerTools
{
    class PlayerList
    {
        public static bool IsEnabled = false;
        public static string Command89 = "playerlist";

        public static void Exec(ClientInfo _cInfo, string _playerName)
        {
            bool _found = false;
            List<ClientInfo> ClientInfoList = ConnectionManager.Instance.Clients.List.ToList();
            for (int i = 0; i < ClientInfoList.Count; i++)
            {
                ClientInfo _cInfo1 = ClientInfoList[i];
                if (_cInfo.entityId != _cInfo1.entityId)
                {
                    _found = true;
                    string _response = "Player = {PlayerName}, Id = {EntityId}[-]";
                    _response = _response.Replace("{PlayerName}", _cInfo1.playerName);
                    _response = _response.Replace("{EntityId}", _cInfo1.entityId.ToString());
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _response, _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            if (!_found)
            {
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "No other players found", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}
