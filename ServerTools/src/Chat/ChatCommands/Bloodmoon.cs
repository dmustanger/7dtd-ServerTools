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
            int _daysUntil7 = 7 - GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime()) % 7;
            string _phrase301;
            string _phrase306;

            if (!Phrases.Dict.TryGetValue(301, out _phrase301))
            {
                _phrase301 = "Next 7th day is in {DaysUntil7} days";
            }
            if (!Phrases.Dict.TryGetValue(306, out _phrase306))
            {
                _phrase306 = "Next 7th day is today";
            }
            _phrase301 = _phrase301.Replace("{DaysUntil7}", _daysUntil7.ToString());
            if (_announce)
            {
                if (_daysUntil7 == 7)
                {
                    GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase306, CustomCommands.ChatColor), "Server", false, "", false);
                }
                else
                {
                    GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase301, CustomCommands.ChatColor), "Server", false, "", false);
                }
            }
            else
            {
                if (_daysUntil7 == 7)
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase306, CustomCommands.ChatColor), "Server", false, "", false));
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase301, CustomCommands.ChatColor), "Server", false, "", false));
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
                    List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                    ClientInfo _cInfo = _cInfoList.RandomObject();
                    GetBloodmoon(_cInfo, true);
                }
                Thread.Sleep(60000 * AutoShowBloodmoon);
            }
        }
    }
}