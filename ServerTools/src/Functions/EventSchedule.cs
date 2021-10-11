using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerTools
{
    class EventSchedule
    {
        public static string autoBackup = "", autoSaveWorld = "", bloodmoon = "", breakTime = "", infoTicker = "",
            nightAlert = "", playerLogs = "", realWorldTime = "", shutdown = "", watchlist = "", zones = "";
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
                Log.Out(string.Format("[SERVERTOOLS] Error in EventSchedule.AddSingle: {0}", e.Message));
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

        public static void Exec()
        {
            try
            {
                foreach (var entry in Schedule.ToArray())
                {
                    switch (entry.Key)
                    {
                        case "AutoBackup":
                            if (DateTime.Now >= entry.Value)
                            {
                                AutoBackup.SetDelay();
                                AutoBackup.Exec();
                            }
                            continue;
                        case "AutoSaveWorld":
                            if (DateTime.Now >= entry.Value)
                            {
                                AutoSaveWorld.SetDelay();
                                AutoSaveWorld.Save();
                            }
                            continue;
                        case "Bloodmoon":
                            if (DateTime.Now >= entry.Value)
                            {
                                Bloodmoon.SetDelay();
                                Bloodmoon.StatusCheck();
                            }
                            continue;
                        case "BreakTime":
                            if (DateTime.Now >= entry.Value)
                            {
                                BreakTime.SetDelay();
                                BreakTime.Exec();
                            }
                            continue;
                        case "InfoTicker":
                            if (DateTime.Now >= entry.Value)
                            {
                                InfoTicker.SetDelay();
                                InfoTicker.Exec();
                            }
                            continue;
                        case "NightAlert":
                            if (DateTime.Now >= entry.Value)
                            {
                                NightAlert.SetDelay();
                                NightAlert.Exec();
                            }
                            continue;
                        case "PlayerLogs":
                            if (DateTime.Now >= entry.Value)
                            {
                                PlayerLogs.SetDelay();
                                PlayerLogs.Exec();
                            }
                            continue;
                        case "RealWorldTime":
                            if (DateTime.Now >= entry.Value)
                            {
                                RealWorldTime.SetDelay();
                                RealWorldTime.Exec();
                            }
                            continue;
                        case "Shutdown":
                            if (DateTime.Now >= entry.Value)
                            {
                                Remove("Shutdown");
                                Shutdown.PrepareShutdown();
                            }
                            continue;
                        case "Watchlist":
                            if (DateTime.Now >= entry.Value)
                            {
                                WatchList.SetDelay();
                                WatchList.Exec();
                            }
                            continue;
                        case "Zones":
                            if (DateTime.Now >= entry.Value)
                            {
                                Zones.SetDelay();
                                Zones.ReminderExec();
                            }
                            continue;
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
