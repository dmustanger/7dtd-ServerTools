
using UnityEngine;

namespace ServerTools
{
    class Loc

    {
        public static bool IsEnabled = false;
        public static string Command_loc = "loc";

        public static void Exec(ClientInfo _cInfo)
        {
            Entity player = PersistentOperations.GetEntityPlayer(_cInfo.entityId);
            if (player != null)
            {
                Vector3 position = player.position;
                if (Zones.IsEnabled && Zones.ZonePlayer.ContainsKey(_cInfo.entityId))
                {
                    Zones.ZonePlayer.TryGetValue(_cInfo.entityId, out string[] zone);
                    Phrases.Dict.TryGetValue("Location1", out string phrase);
                    phrase = phrase.Replace("{Position}", position.ToString());
                    phrase = phrase.Replace("{Name}", zone[0]);
                    if (zone[9] == "0")
                    {
                        phrase = phrase.Replace("{Mode}", "no killing");
                    }
                    else if (zone[9] == "1")
                    {
                        phrase = phrase.Replace("{Mode}", "kill allies only");
                    }
                    else if (zone[9] == "2")
                    {
                        phrase = phrase.Replace("{Mode}", "kill strangers only");
                    }
                    else
                    {
                        phrase = phrase.Replace("{Mode}", "kill everyone");
                    }
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    return;
                }
                Phrases.Dict.TryGetValue("Location2", out string phrase1);
                phrase1 = phrase1.Replace("{Position}", position.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}
