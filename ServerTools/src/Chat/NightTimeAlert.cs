
namespace ServerTools
{
    class NightAlert
    {
        public static bool IsEnabled = false;

        public static void Exec()
        {
            if (GameManager.Instance.World.IsDaytime())
            {
                int _dusk = (int)SkyManager.GetDuskTime();
                ulong _worldTime = GameManager.Instance.World.worldTime;
                int _worldHours = (int)(_worldTime / 1000UL) % 24;
                if (_worldHours < _dusk - 1)
                {
                    int _hours = _dusk - _worldHours;
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} hours until night time[-]", Config.Chat_Response_Color, _hours), Config.Server_Response_Name, false, "ServerTools", false);
                }
            }
        }
    }
}
