
namespace ServerTools
{
    class Fps
    {
        public static bool IsEnabled = false, Set = false;
        public static int Target = 0;

        public static void FPS(ClientInfo _cInfo, bool _announce)
        {
            string _fps = GameManager.Instance.fps.Counter.ToString();
            string _phrase755;
            if (!Phrases.Dict.TryGetValue(755, out _phrase755))
            {
                _phrase755 = "Server FPS: {Fps}";
            }
            _phrase755 = _phrase755.Replace("{Fps}", _fps);
            if (_announce)
            {
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase755), "Server", false, "", false);
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase755), "Server", false, "", false));
            }
        }

        public static void _0_()
        {
            if (Set)
            {
                SdtdConsole.Instance.ExecuteSync(string.Format("SetTargetFps {0}", Target), null);
            }
        }
    }
}
