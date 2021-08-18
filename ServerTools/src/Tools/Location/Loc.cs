
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
                Phrases.Dict.TryGetValue("Location1", out string _phrase);
                _phrase = _phrase.Replace("{Position}", _position.ToString());
                _phrase = _phrase.Replace("{Name}", _zone[0]);
                if (_zone[9] == "0")
                {
                    _phrase = _phrase.Replace("{Mode}", "no killing");
                }
                else if (_zone[9] == "1")
                {
                    _phrase = _phrase.Replace("{Mode}", "kill allies only");
                }
                else if (_zone[9] == "2")
                {
                    _phrase = _phrase.Replace("{Mode}", "kill strangers only");
                }
                else
                {
                    _phrase = _phrase.Replace("{Mode}", "kill everyone");
                }
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                return;
            }
            Phrases.Dict.TryGetValue("Location2", out string _phrase1);
            _phrase1 = _phrase1.Replace("{Position}", _position.ToString());
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
        }
    }
}
