using System.Timers;

namespace ServerTools
{
    public class Bloodmoon
    {
        private static int timerInstanceCount = 0;
        public static bool IsEnabled = false;
        public static bool Show_On_Login = false;
        public static bool Show_On_Respawn = false;
        public static int Auto_Show_Bloodmoon_Delay = 30;
        public static int Days_Until_Horde = 7;
        private static System.Timers.Timer t = new System.Timers.Timer();

        public static void TimerStart()
        {
            timerInstanceCount++;
            if (timerInstanceCount <= 1)
            {
                t.Interval = Auto_Show_Bloodmoon_Delay * 60000;
                t.Start();
                t.Elapsed += new ElapsedEventHandler(StatusCheck);
            }
        }

        public static void TimerStop()
        {
            t.Stop();
        }


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
            if (_announce)
            {
                if (_daysUntilHorde == Days_Until_Horde)
                {
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase306, Config.Chat_Response_Color), "Server", false, "", false);
                }
                else
                {
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase301, Config.Chat_Response_Color), "Server", false, "", false);
                }
            }
            else
            {
                if (_daysUntilHorde == Days_Until_Horde)
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase306, Config.Chat_Response_Color), "Server", false, "", false));
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase301, Config.Chat_Response_Color), "Server", false, "", false));
                }
            }
        }

        private static void StatusCheck(object sender, ElapsedEventArgs e)
        {
            if (ConnectionManager.Instance.ClientCount() > 0)
            {
                GetBloodmoon((ClientInfo)null, true);
            }
        }
    }
}