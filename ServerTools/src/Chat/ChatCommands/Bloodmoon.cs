using System.Collections.Generic;
using System.Threading;

namespace ServerTools
{
    public class Bloodmoon
    {
        public static bool IsRunning = false;
        public static bool IsEnabled = false;
        public static bool ShowOnSpawn = false;
        public static bool ShowOnRespawn = false;
        public static int AutoShowBloodmoon = 30;
        public static int DaysUntilHorde = 7;
        private static Thread th;

        public static void Load()
        {
            Start();
            IsRunning = true;
        }

        public static void Unload()
        {
            th.Abort();
            IsRunning = false;
        }

        public static void GetBloodmoon(ClientInfo _cInfo, bool _announce)
        {
            int _daysUntilHorde = DaysUntilHorde - GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime()) % DaysUntilHorde;
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
                if (_daysUntilHorde == DaysUntilHorde)
                {
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase306, Config.ChatResponseColor), "Server", false, "", false);
                }
                else
                {
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase301, Config.ChatResponseColor), "Server", false, "", false);
                }
            }
            else
            {
                if (_daysUntilHorde == DaysUntilHorde)
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase301, Config.ChatResponseColor), "Server", false, "", false));
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase301, Config.ChatResponseColor), "Server", false, "", false));
                }
            }
        }

        private static void Start()
        {
            th = new Thread(new ThreadStart(StatusCheck));
            th.IsBackground = true;
            th.Start();
        }

        private static void StatusCheck()
        {
            while (AutoShowBloodmoon > 0)
            {
                if (ConnectionManager.Instance.ClientCount() > 0)
                {
                    GetBloodmoon((ClientInfo)null, true);
                }
                Thread.Sleep(60000 * AutoShowBloodmoon);
            }
        }
    }
}