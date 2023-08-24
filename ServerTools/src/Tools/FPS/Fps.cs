using System.Collections.Generic;

namespace ServerTools
{
    class Fps
    {
        public static bool IsEnabled = false;
        public static string Command_fps = "fps";
        public static int Max_FPS = 0;

        public static void Exec(ClientInfo _cInfo)
        {
            int fps = (int)GameManager.Instance.fps.Counter;
            if (fps > Max_FPS)
            {
                fps = Max_FPS;
            }
            Phrases.Dict.TryGetValue("Fps1", out string _phrase);
            _phrase = _phrase.Replace("{Fps}", fps.ToString());
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
        }

        public static void SetTarget(int _target)
        {
            if (_target > 0)
            {
                if (_target != Max_FPS)
                {
                    Max_FPS = _target;
                    GameManager.Instance.waitForTargetFPS.TargetFPS = Max_FPS;
                }
            }
            else
            {
                GameManager.Instance.waitForTargetFPS.TargetFPS = 20;
                Max_FPS = 0;
            }
        }
    }
}
