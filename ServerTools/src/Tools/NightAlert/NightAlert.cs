
namespace ServerTools
{
    class NightAlert
    {
        public static bool IsEnabled = false;
        public static int Delay = 60;

        public static void Exec()
        {
            if (ConnectionManager.Instance.ClientCount() > 0 && GameManager.Instance.World.IsDaytime())
            {
                ulong _worldTime = GameManager.Instance.World.worldTime;
                int _24HourTime = (int)(_worldTime / 1000UL) % 24;
                int _dusk = (int)SkyManager.GetDuskTime();
                int _timeRemaining = _dusk - _24HourTime;
                Phrases.Dict.TryGetValue("NightAlert1", out string _phrase);
                _phrase = _phrase.Replace("{Value}", _timeRemaining.ToString());
                ChatHook.ChatMessage(null, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
            }
        }
    }
}
