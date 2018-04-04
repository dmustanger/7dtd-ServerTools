
namespace ServerTools
{
    class NightAlert
    {
        public static bool IsEnabled = true;

        public static void Exec()
        {
            float _dusk = SkyManager.GetDuskTime();
            ulong _worldTime = GameManager.Instance.World.worldTime;
            int _worldHours = (int)(_worldTime / 24000UL + 1UL);
            if (_worldHours < _dusk - 1 && _worldHours > _dusk - 16)
            {
                int _hours = (int)_dusk - _worldHours;
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} hours until night time[-]", Config.Chat_Response_Color, _hours), Config.Server_Response_Name, false, "ServerTools", false);
            }
        }
    }
}
