
using UnityEngine;

namespace ServerTools
{
    class Loc

    {
        public static bool IsEnabled = false;
        public static string Command_loc = "loc";

        public static void Exec(ClientInfo _cInfo)
        {
            Entity _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            Vector3 _position = _player.position;
            if (Zones.IsEnabled && Zones.ZonePlayer.ContainsKey(_cInfo.entityId))
            {
                Zones.ZonePlayer.TryGetValue(_cInfo.entityId, out string[] _zone);
                Phrases.Dict.TryGetValue(461, out string _phrase461);
                _phrase461 = _phrase461.Replace("{Position}", _position.ToString());
                _phrase461 = _phrase461.Replace("{Name}", _zone[0]);
                if (_zone[9] == "0")
                {
                    _phrase461 = _phrase461.Replace("{Mode}", "no killing");
                }
                else if (_zone[9] == "1")
                {
                    _phrase461 = _phrase461.Replace("{Mode}", "kill allies only");
                }
                else if (_zone[9] == "2")
                {
                    _phrase461 = _phrase461.Replace("{Mode}", "kill strangers only");
                }
                else
                {
                    _phrase461 = _phrase461.Replace("{Mode}", "kill everyone");
                }
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase461 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                return;
            }
            Phrases.Dict.TryGetValue(462, out string _phrase462);
            _phrase462 = _phrase462.Replace("{Position}", _position.ToString());
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase462 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
        }
    }
}
