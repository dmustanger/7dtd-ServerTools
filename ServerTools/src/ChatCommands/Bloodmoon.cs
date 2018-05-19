

namespace ServerTools
{
    public class Bloodmoon
    {
        public static bool IsEnabled = false, Show_On_Login = false, Show_On_Respawn = false, Auto_Enabled = false;
        public static int Days_Until_Horde = 7;

        public static void GetBloodmoon(ClientInfo _cInfo, bool _announce)
        {
            int _daysUntilHorde = Days_Until_Horde - GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime()) % Days_Until_Horde;
            string _phrase301;
            string _phrase306;
            if (!Phrases.Dict.TryGetValue(301, out _phrase301))
            {
                _phrase301 = "Next horde night is in {DaysUntilHorde} days";
            }
            if (!Phrases.Dict.TryGetValue(306, out _phrase306))
            {
                _phrase306 = "Next horde night is today";
            }
            _phrase301 = _phrase301.Replace("{DaysUntilHorde}", _daysUntilHorde.ToString());
            if (_daysUntilHorde == Days_Until_Horde)
            {
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase306), Config.Server_Response_Name, false, "", false);
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase306), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
            else
            {
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase301), Config.Server_Response_Name, false, "", false);
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase301), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
        }

        public static void StatusCheck()
        {
            if (ConnectionManager.Instance.ClientCount() > 0)
            {
                int _daysUntilHorde = Days_Until_Horde - GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime()) % Days_Until_Horde;
                string _phrase301;
                string _phrase306;
                if (!Phrases.Dict.TryGetValue(301, out _phrase301))
                {
                    _phrase301 = "Next horde night is in {DaysUntilHorde} days";
                }
                if (!Phrases.Dict.TryGetValue(306, out _phrase306))
                {
                    _phrase306 = "Next horde night is today";
                }
                _phrase301 = _phrase301.Replace("{DaysUntilHorde}", _daysUntilHorde.ToString());
                if (_daysUntilHorde == Days_Until_Horde)
                {
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase306, Config.Chat_Response_Color), Config.Server_Response_Name, false, "", false);
                }
                else
                {
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase301, Config.Chat_Response_Color), Config.Server_Response_Name, false, "", false);
                }
            }
        }
    }
}