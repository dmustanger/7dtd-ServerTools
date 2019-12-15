
namespace ServerTools
{
    class NightAlert
    {
        public static bool IsEnabled = false;
        public static int Delay = 60;

        public static void Exec()
        {
            if (GameManager.Instance.World.IsDaytime())
            {
                ulong _worldTime = GameManager.Instance.World.worldTime;
                int _24HourTime = (int)(_worldTime / 1000UL) % 24;
                int _dusk = (int)SkyManager.GetDuskTime();
                int _timeRemaining = _dusk - _24HourTime;
                string _phrase940;
                if (!Phrases.Dict.TryGetValue(940, out _phrase940))
                {
                    _phrase940 = "{Time} hours until night time.";
                }
                _phrase940 = _phrase940.Replace("{Time}", _timeRemaining.ToString());
                ChatHook.ChatMessage(null, LoadConfig.Chat_Response_Color + _phrase940 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
            }
        }
    }
}
