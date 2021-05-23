using ServerTools.AntiCheat;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerTools
{
    class EventSchedule
    {
        public static int _autoBackup = int.MinValue, _autoSaveWorld = int.MinValue, _bloodmoon = int.MinValue, _breakTime = int.MinValue,
            _infoTicker = int.MinValue, _nightAlert = int.MinValue, _playerLogs = int.MinValue, _realWorldTime = int.MinValue,
            _shutdown = int.MinValue, _watchlist = int.MinValue, _zones = int.MinValue;
        public static Dictionary<string, DateTime> Schedule = new Dictionary<string, DateTime>();

        public static void Add(string _classMethod, DateTime _time)
        {
            try
            {
                if (Schedule.ContainsKey(_classMethod))
                {
                    Schedule[_classMethod] = _time;
                }
                else
                {
                    Schedule.Add(_classMethod, _time);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in EventSchedule.Add: {0}", e.Message));
            }
        }

        public static void Remove(string _className)
        {
            try
            {
                if (Schedule.ContainsKey(_className))
                {
                    Schedule.Remove(_className);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in EventSchedule.Remove: {0}", e.Message));
            }
        }

        public static void Exec() //Dictionary keys in schedule correspond to specific class in order to execute corresponding methods
        {
            try
            {
                foreach (var _event in Schedule.ToArray())
                {
                    if (DateTime.Now >= _event.Value)
                    {
                        if (_event.Key == "AutoBackup")
                        {
                            Add("AutoBackup", DateTime.Now.AddMinutes(AutoBackup.Delay));
                            AutoBackup.Exec();
                        }
                        else if (_event.Key == "AutoSaveWorld")
                        {
                            Add("AutoSaveWorld", DateTime.Now.AddMinutes(AutoSaveWorld.Delay));
                            AutoSaveWorld.Save();
                        }
                        else if (_event.Key == "Bloodmoon")
                        {
                            Add("Bloodmoon", DateTime.Now.AddMinutes(Bloodmoon.Delay));
                            Bloodmoon.StatusCheck();
                        }
                        else if (_event.Key == "BreakTime")
                        {
                            Add("BreakTime", DateTime.Now.AddMinutes(BreakTime.Delay));
                            BreakTime.Exec();
                        }
                        else if (_event.Key == "InfoTicker")
                        {
                            Add("InfoTicker", DateTime.Now.AddMinutes(InfoTicker.Delay));
                            InfoTicker.Exec();
                        }
                        else if (_event.Key == "NightAlert")
                        {
                            Add("NightAlert", DateTime.Now.AddMinutes(NightAlert.Delay));
                            NightAlert.Exec();
                        }
                        else if (_event.Key == "PlayerLogs")
                        {
                            Add("PlayerLogs", DateTime.Now.AddMinutes(PlayerLogs.Delay));
                            PlayerLogs.Exec();
                        }
                        else if (_event.Key == "RealWorldTime")
                        {
                            Add("RealWorldTime", DateTime.Now.AddMinutes(RealWorldTime.Delay));
                            RealWorldTime.Exec();
                        }
                        else if (_event.Key == "Watchlist")
                        {
                            Add("Watchlist", DateTime.Now.AddMinutes(Watchlist.Delay));
                            Watchlist.CheckWatchlist();
                        }
                        else if (_event.Key == "Zones")
                        {
                            Add("Zones", DateTime.Now.AddMinutes(Zones.Reminder_Delay));
                            Zones.ReminderExec();
                        }
                        else if (_event.Key == "Shutdown")
                        {
                            StopServer.PrepareShutdown(); 
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in EventSchedule.Exec: {0}", e.Message));
            }
        }
    }
}
