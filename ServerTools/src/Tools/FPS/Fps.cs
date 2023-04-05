using System.Collections.Generic;

namespace ServerTools
{
    class Fps
    {
        public static bool IsEnabled = false;
        public static string Command_fps = "fps";

        public static void Exec(ClientInfo _cInfo)
        {
            int fps = (int)GameManager.Instance.fps.Counter;
            if (fps > 80)
            {
                fps = 80;
            }
            Phrases.Dict.TryGetValue("Fps1", out string _phrase);
            _phrase = _phrase.Replace("{Fps}", fps.ToString());
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
        }
    }
}
