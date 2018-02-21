using System.Timers;
using System.Collections.Generic;
using System;


namespace ServerTools
{
    public static class AutoShutdown
    {
        public static int timerInstanceCount = 0;
        public static bool IsEnabled = false;
        public static int CountdownTimer = 2;
        public static int TimeBeforeShutdown = 60;
        public static List<DateTime> timerStart = new List<DateTime>();
        private static System.Timers.Timer timerShutdown = new System.Timers.Timer();

        public static void TimerStart()
        {
            timerStart.Clear();
            timerStart.Add(DateTime.Now);
            timerInstanceCount++;
            if (timerInstanceCount <= 1)
            {               
                if (TimeBeforeShutdown <= CountdownTimer)
                {                  
                    int d = (CountdownTimer * 60000);
                    timerShutdown.Interval = d;
                    timerShutdown.Start();
                    timerShutdown.Elapsed += new ElapsedEventHandler(Auto_Shutdown);
                }
                else
                {
                    int d = (TimeBeforeShutdown * 60000);
                    timerShutdown.Interval = d;
                    timerShutdown.Start();
                    timerShutdown.Elapsed += new ElapsedEventHandler(Auto_Shutdown);
                }
            }
        }

        public static void TimerStop()
        {
            timerShutdown.Stop();
            timerStart.Clear();
        }

        public static void Auto_Shutdown(object sender, ElapsedEventArgs e)
        {
            Log.Out("[SERVERTOOLS] Running stopserver command.");
            SdtdConsole.Instance.ExecuteSync("say \"[FF0000]Auto Shutdown Initiated\"", (ClientInfo)null);
            SdtdConsole.Instance.ExecuteSync(string.Format("stopserver {0}", CountdownTimer), (ClientInfo)null);
        }

        public static void CheckNextShutdown(ClientInfo _cInfo, bool _announce, string _playerName)
        {
            if (IsEnabled)
            {
                var _timeStart = timerStart.RandomObject();
                TimeSpan varTime = DateTime.Now - _timeStart;
                double fractionalMinutes = varTime.TotalMinutes;
                int _timeMinutes = (int)fractionalMinutes;
                int _timeleftMinutes = TimeBeforeShutdown - _timeMinutes;
                string TimeLeft;
                TimeLeft = string.Format("{0:00} H : {1:00} M", _timeleftMinutes / 60, _timeleftMinutes % 60);
                if (_announce)
                {
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}The next auto shutdown is in [FF8000]{1}[-]", Config.ChatResponseColor, TimeLeft), "Server", false, "", false);
                }
                else
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}The next auto shutdown is in [FF8000]{1}[-]", Config.ChatResponseColor, TimeLeft), "Server", false, "", false));
                }
            }
        }
    }
}