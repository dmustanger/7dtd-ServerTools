
using UnityEngine;

namespace ServerTools
{
    class Loc

    {
        public static bool IsEnabled = false;

        public static void Exec(ClientInfo _cInfo)
        {
            Entity _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            Vector3 _position = _player.position;
            int _x = (int)_position.x;
            int _y = (int)_position.y - 59;
            int _z = (int)_position.z;
            string _phrase760;
            if (!Phrases.Dict.TryGetValue(760, out _phrase760))
            {
                _phrase760 = "your current position is X  {X}, Y  {Y}, Z  {Z}.";
            }
            _phrase760 = _phrase760.Replace("{X}", _x.ToString());
            _phrase760 = _phrase760.Replace("{Y}", _y.ToString());
            _phrase760 = _phrase760.Replace("{Z}", _z.ToString());
            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName  + _phrase760 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
        }
    }
}
