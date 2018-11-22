
namespace ServerTools
{
    class Fps
    {
        public static bool IsEnabled = false;
        public static int Set_Target = 30;

        public static void FPS(ClientInfo _cInfo, bool _announce)
        {
            string _fps = GameManager.Instance.fps.Counter.ToString();
            string _phrase300;
            if (!Phrases.Dict.TryGetValue(300, out _phrase300))
            {
                _phrase300 = "Server FPS: {Fps}";
            }
            _phrase300 = _phrase300.Replace("{Fps}", _fps);
            if (_announce)
            {
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", LoadConfig.Chat_Response_Color, _phrase300), LoadConfig.Server_Response_Name, false, "", false);
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", LoadConfig.Chat_Response_Color, _phrase300), LoadConfig.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void SetTarget()
        {
            SdtdConsole.Instance.ExecuteSync(string.Format("SetTargetFps {0}", Set_Target), null);
            Log.Out(string.Format("[SERVERTOOLS] Set target FPS to {0}", Set_Target));
        }
    }
}
