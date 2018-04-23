
namespace ServerTools
{
    class NightAlert
    {
        public static bool IsEnabled = false;

        public static void Exec()
        {
            if (GameManager.Instance.World.IsDaytime())
            {
                ulong _worldTime = GameManager.Instance.World.worldTime;
                int _worldHours = (int)(_worldTime / 1000UL) % 24;
                int _dusk = (int)SkyManager.GetDuskTime();
                int _hours = _dusk - _worldHours;
                string _phrase940;
                if (!Phrases.Dict.TryGetValue(940, out _phrase940))
                {
                    _phrase940 = "{Time} hours until night time.";
                }
                _phrase940 = _phrase940.Replace("{Time}", _hours.ToString());
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase940), Config.Server_Response_Name, false, "ServerTools", false);
            }
        }
    }
}
