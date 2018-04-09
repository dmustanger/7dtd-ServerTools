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
                string _phrase200;
                if (!Phrases.Dict.TryGetValue(200, out _phrase200))
                {
                    _phrase200 = "{PlayerName} you do not have permissions to use this command.";
                }
                _phrase200 = _phrase200.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase200), Config.Server_Response_Name, false, "ServerTools", false));
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
                    _phrase551 = "{PlayerName} you have set the lobby position as {LobbyPosition}.";
                }
                _phrase551 = _phrase551.Replace("{PlayerName}", _cInfo.playerName);
                _phrase551 = _phrase551.Replace("{LobbyPosition}", Lobby_Position);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase551), Config.Server_Response_Name, false, "ServerTools", false));
                Config.UpdateXml();
            }
        }
    }
}
