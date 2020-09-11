
namespace ServerTools
{
    class Fps
    {
        public static bool IsEnabled = false;
        public static int Set_Target = 60;
        public static string Command75 = "fps";

        public static void FPS(ClientInfo _cInfo)
        {
            string _fps = GameManager.Instance.fps.Counter.ToString();
            Phrases.Dict.TryGetValue(41, out string _phrase41);
            _phrase41 = _phrase41.Replace("{Fps}", _fps);
            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase41 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
        }

        public static void SetTarget()
        {
            SdtdConsole.Instance.ExecuteSync(string.Format("SetTargetFps {0}", Set_Target), null);
        }
    }
}
