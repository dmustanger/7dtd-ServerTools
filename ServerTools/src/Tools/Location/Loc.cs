
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
            if (Zones.IsEnabled && Zones.ZoneInfo.ContainsKey(_cInfo.entityId))
            {
                Zones.ZoneInfo.TryGetValue(_cInfo.entityId, out string[] _info);
                if (_info[0] != "")
                {
                    Phrases.Dict.TryGetValue(461, out string _phrase461);
                    _phrase461 = _phrase461.Replace("{X}", _x.ToString());
                    _phrase461 = _phrase461.Replace("{Y}", _y.ToString());
                    _phrase461 = _phrase461.Replace("{Z}", _z.ToString());
                    _phrase461 = _phrase461.Replace("{Name}", _z.ToString());
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase461 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
            }
            Phrases.Dict.TryGetValue(462, out string _phrase462);
            _phrase462 = _phrase462.Replace("{X}", _x.ToString());
            _phrase462 = _phrase462.Replace("{Y}", _y.ToString());
            _phrase462 = _phrase462.Replace("{Z}", _z.ToString());
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase462 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
        }
    }
}
