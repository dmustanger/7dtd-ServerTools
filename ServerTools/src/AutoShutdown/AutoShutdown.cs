using System.Collections.Generic;
using System;

namespace ServerTools
{
    public static class AutoShutdown
    {
        public static bool IsEnabled = false, Alert_On_Login = false, Bloodmoon = false;
        public static int Countdown_Timer = 2, Days_Until_Horde = 7;
        public static List<DateTime> timerStart = new List<DateTime>();
        private static bool Wait = false;

        public static void ShutdownList()
        {
            timerStart.Clear();
            timerStart.Add(DateTime.Now);
        }

        public static void Auto_Shutdown()
        {
            if (!Bloodmoon)
            {
                ulong _worldTime = GameManager.Instance.World.worldTime;
                int _daysUntilHorde = Days_Until_Horde - GameUtils.WorldTimeToDays(_worldTime) % Days_Until_Horde;
                int _daysUntil1 = Days_Until_Horde + 1;
                int _daysUntilHorde1 = _daysUntil1 - GameUtils.WorldTimeToDays(_worldTime) % _daysUntil1;
                int _worldHours = GameUtils.WorldTimeToHours(_worldTime);
                float _duskTime = SkyManager.GetDuskTime();
                if ((_daysUntilHorde == Days_Until_Horde && (_worldHours >= _duskTime - 2 && _worldHours <= _duskTime + 2)) || (_daysUntilHorde1 == _daysUntil1 && (_worldHours >= _duskTime - 22 && _worldHours <= _duskTime - 16)))
                {
                    Bloodmoon = true;
                }
                if (!Bloodmoon)
                {
                    if (Countdown_Timer < 1)
                    {
                        Countdown_Timer = 1;
                    }
                    Log.Out("[SERVERTOOLS] Running auto shutdown.");
                    GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, ("[FF0000]Auto shutdown initiated[-]"), Config.Server_Response_Name, false, "ServerTools", false);
                    SdtdConsole.Instance.ExecuteSync(string.Format("stopserver {0}", Countdown_Timer), (ClientInfo)null);
                }
            }
            else
            {
                if (Wait)
                {
                    Bloodmoon = false;
                    Wait = false;
                    Auto_Shutdown();
                }
                else
                {
                    World world = GameManager.Instance.World;
                    bool _day = world.IsDaytime();
                    if (_day)
                    {
                        Wait = true;
                    }
                }
            }
        }

        public static void CheckNextShutdown(ClientInfo _cInfo, bool _announce)
        {
            if (IsEnabled)
            {
                DateTime _timeStart = timerStart[0];
                TimeSpan varTime = DateTime.Now - _timeStart;
                double fractionalMinutes = varTime.TotalMinutes;
                int _timeMinutes = (int)fractionalMinutes;
                int _timeleftMinutes = Timers.Shutdown_Delay - _timeMinutes;
                if (_timeleftMinutes > 0)
                {
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
                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase730), Config.Server_Response_Name, false, "ServerTools", false);
                    }
                    else
                    {
                        string _phrase730;
                        if (!Phrases.Dict.TryGetValue(730, out _phrase730))
                        {
                            _phrase730 = "The next auto shutdown is in [FF8000]{TimeLeft}.";
                        }
                        _phrase730 = _phrase730.Replace("{TimeLeft}", TimeLeft);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase730), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
            }
        }
    }
}