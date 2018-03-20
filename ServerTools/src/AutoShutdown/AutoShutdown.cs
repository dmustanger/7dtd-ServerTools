using System.Collections.Generic;
using System;

namespace ServerTools
{
    public static class AutoShutdown
    {
        public static bool IsEnabled = false, Alert_On_Login = false;
        public static int Countdown_Timer = 2;
        public static List<DateTime> timerStart = new List<DateTime>();

        public static void ShutdownList()
        {
            timerStart.Clear();
            timerStart.Add(DateTime.Now);
        }

        public static void Auto_Shutdown()
        {
            if (Countdown_Timer < 1)
            {
                Countdown_Timer = 1;
            }
            Log.Out("[SERVERTOOLS] Running auto shutdown.");
            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, ("[FF0000]Auto shutdown initiated[-]"), "Server", false, "", false);
            SdtdConsole.Instance.ExecuteSync(string.Format("stopserver {0}", Countdown_Timer), (ClientInfo)null);
        }

        public static void CheckNextShutdown(ClientInfo _cInfo, bool _announce)
        {
            if (IsEnabled)
            {
                var _timeStart = timerStart.RandomObject();
                TimeSpan varTime = DateTime.Now - _timeStart;
                double fractionalMinutes = varTime.TotalMinutes;
                int _timeMinutes = (int)fractionalMinutes;
                int _timeleftMinutes = Timers.Shutdown_Delay - _timeMinutes;
                string TimeLeft;
                TimeLeft = string.Format("{0:00} H : {1:00} M", _timeleftMinutes / 60, _timeleftMinutes % 60);
                if (_announce)
                {
                    string _phrase730;
                    if (!Phrases.Dict.TryGetValue(730, out _phrase730))
                    {
                        _phrase730 = "The next auto shutdown is in [FF8000]{TimeLeft}.";
                    }
                    _phrase730 = _phrase730.Replace("{TimeLeft}", TimeLeft);
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase730), "Server", false, "", false);
                }
                else
                {
                    string _phrase730;
                    if (!Phrases.Dict.TryGetValue(730, out _phrase730))
                    {
                        _phrase730 = "The next auto shutdown is in [FF8000]{TimeLeft}.";
                    }
                    _phrase730 = _phrase730.Replace("{TimeLeft}", TimeLeft);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase730), "Server", false, "", false));
                }
            }
        }
    }
}