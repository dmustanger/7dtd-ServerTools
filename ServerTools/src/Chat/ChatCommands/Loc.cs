
using UnityEngine;

namespace ServerTools
{
    class Loc

    {
        public static bool IsEnabled = false;
        public static string Command76 = "loc";

        public static void Exec(ClientInfo _cInfo)
        {
            Entity _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            Vector3 _position = _player.position;
            int _x = (int)_position.x;
            int _y = (int)_position.y;
            int _z = (int)_position.z;
            string _exit = "";
            if (Zones.IsEnabled && Zones.ZoneExit.ContainsKey(_cInfo.entityId))
            {
                Zones.ZoneExit.TryGetValue(_cInfo.entityId, out _exit);
                if (_exit != "")
                {
                    string _phrase761;
                    if (!Phrases.Dict.TryGetValue(761, out _phrase761))
                    {
                        _phrase761 = " your current position is X  {X}, Y  {Y}, Z  {Z}. You are inside a zone.";
                    }
                    _phrase761 = _phrase761.Replace("{X}", _x.ToString());
                    _phrase761 = _phrase761.Replace("{Y}", _y.ToString());
                    _phrase761 = _phrase761.Replace("{Z}", _z.ToString());
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase761 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            string _phrase760;
            if (!Phrases.Dict.TryGetValue(760, out _phrase760))
            {
                _phrase760 = " your current position is X  {X}, Y  {Y}, Z  {Z}.";
            }
            _phrase760 = _phrase760.Replace("{X}", _x.ToString());
            _phrase760 = _phrase760.Replace("{Y}", _y.ToString());
            _phrase760 = _phrase760.Replace("{Z}", _z.ToString());
            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase760 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
        }
    }
}
