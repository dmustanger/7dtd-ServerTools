using System;
using System.Collections.Generic;

namespace ServerTools
{
    class POIProtection
    {
        public static bool IsEnabled = false, Bed = false, Claim = false;
        public static int Offset = 5;

        public static Dictionary<int, bool> PlayerInZone = new Dictionary<int, bool>();

        public static void PlayerCheck(ClientInfo _cInfo, EntityAlive _player)
        {
            World _world = GameManager.Instance.World;
            if (PlayerInZone.ContainsKey(_player.entityId)) {
                Log.Out(PlayerInZone[_player.entityId].ToString());
            }
            if (_world.IsPositionWithinPOI(_player.position, 5))
            {
                bool SendMessage = false;
                if (PlayerInZone.ContainsKey(_player.entityId))
                {
                    if (PlayerInZone[_player.entityId] == false) {
                        SendMessage = true;
                    }
                }
                else {
                    SendMessage = true;
                }

                if (SendMessage)
                {
                    PlayerInZone[_player.entityId] = true;
                    Phrases.Dict.TryGetValue("POI3", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + "[-]" + _phrase, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            else {
                if (PlayerInZone.ContainsKey(_player.entityId) && PlayerInZone[_player.entityId] == true)
                {
                    PlayerInZone[_player.entityId] = false;
                }
            }
        }
    }
}
