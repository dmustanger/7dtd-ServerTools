using System.Timers;
using System.Collections.Generic;
using System;


namespace ServerTools
{
    public static class AutoRestart
    {
        public static int timerInstanceCount = 0;
        public static bool IsEnabled = false;
        public static int CountdownTimer = 2;
        public static int DelayBetweenRestarts = 60;
        public static List<DateTime> timerStart = new List<DateTime>();
        private static System.Timers.Timer timerRestart = new System.Timers.Timer();

        public static void TimerStart()
        {
            timerStart.Clear();
            timerStart.Add(DateTime.Now);
            timerInstanceCount++;
            if (timerInstanceCount <= 1)
            {               
                if (DelayBetweenRestarts <= CountdownTimer)
                {                  
                    int d = (CountdownTimer * 60000);
                    timerRestart.Interval = d;
                    timerRestart.Start();
                    timerRestart.Elapsed += new ElapsedEventHandler(shutdown);
                }
                else
                {
                    int d = (DelayBetweenRestarts * 60000);
                    timerRestart.Interval = d;
                    timerRestart.Start();
                    timerRestart.Elapsed += new ElapsedEventHandler(shutdown);
                }
            }
        }

        public static void TimerStop()
        {
            timerRestart.Stop();
        }

        public static void shutdown(object sender, ElapsedEventArgs e)
        {
            int _playerCount = ConnectionManager.Instance.ClientCount();
            if (_playerCount > 0)
            {
                List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                ClientInfo _cInfo = _cInfoList.RandomObject();
                Log.Out("[SERVERTOOLS] Running stopserver command.");
                SdtdConsole.Instance.ExecuteSync("say \"[FF0000]Auto Restart Initiated\"", _cInfo);
                SdtdConsole.Instance.ExecuteSync(string.Format("stopserver {0}", CountdownTimer), _cInfo);
            }
        }

        public static void CheckNextRestart(ClientInfo _cInfo, bool _announce, string _playerName)
        {
            if (IsEnabled)
            {
                var _timeStart = timerStart.RandomObject();
                TimeSpan varTime = DateTime.Now - _timeStart;
                double fractionalMinutes = varTime.TotalMinutes;
                int _timeMinutes = (int)fractionalMinutes;
                int _timeleftMinutes = DelayBetweenRestarts - _timeMinutes;
                string TimeLeft;
                TimeLeft = string.Format("{0:00} H : {1:00} M", _timeleftMinutes / 60, _timeleftMinutes % 60);
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("{0}The next auto restart is in [FF8000]{1}[-]", CustomCommands.ChatColor, TimeLeft), "Server", false, "", false);
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}The next auto restart is in [FF8000]{1}[-]", CustomCommands.ChatColor, TimeLeft), "Server", false, "", false));
                }
            }
        }
    }
}