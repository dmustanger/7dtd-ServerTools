using UnityEngine;

namespace ServerTools
{
    class SetMarket
    {
        public static string Market_Position = "0,0,0";
        private static string[] _cmd = { "Market", "market" };

        public static void Set(ClientInfo _cInfo)
        {
            if (!GameManager.Instance.adminTools.CommandAllowedFor(_cmd, _cInfo.playerId))
            {
                string _phrase107;
                if (!Phrases.Dict.TryGetValue(107, out _phrase107))
                {
                    _phrase107 = "you do not have permissions to use this command.";
                }
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase107 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper);
            }
            else
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                Vector3 _position = _player.GetPosition();
                int x = (int)_position.x;
                int y = (int)_position.y;
                int z = (int)_position.z;
                string _mposition = x + "," + y + "," + z;
                Market_Position = _mposition;
                string _phrase565;
                if (!Phrases.Dict.TryGetValue(565, out _phrase565))
                {
                    _phrase565 = "you have set the market position as {MarketPosition}.";
                }
                _phrase565 = _phrase565.Replace("{MarketPosition}", Market_Position);
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase565 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper);
                LoadConfig.UpdateXml();
            }
        }
    }
}
