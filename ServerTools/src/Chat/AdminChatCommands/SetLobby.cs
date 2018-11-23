using UnityEngine;

namespace ServerTools
{
    class SetLobby
    {
        public static string Lobby_Position = "0,0,0";
        private static string[] _cmd = { "lobby" };

        public static void Set(ClientInfo _cInfo)
        {
            if (!GameManager.Instance.adminTools.CommandAllowedFor(_cmd, _cInfo.playerId))
            {
                string _phrase107;
                if (!Phrases.Dict.TryGetValue(107, out _phrase107))
                {
                    _phrase107 = "you do not have permissions to use this command.";
                }
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase107 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                Vector3 _position = _player.GetPosition();
                int x = (int)_position.x;
                int y = (int)_position.y;
                int z = (int)_position.z;
                string _lposition = x + "," + y + "," + z;
                Lobby_Position = _lposition;
                string _phrase551;
                if (!Phrases.Dict.TryGetValue(551, out _phrase551))
                {
                    _phrase551 = "you have set the lobby position as {LobbyPosition}.";
                }
                _phrase551 = _phrase551.Replace("{LobbyPosition}", Lobby_Position);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase551 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                LoadConfig.UpdateXml();
            }
        }
    }
}
