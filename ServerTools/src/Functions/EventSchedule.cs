using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerTools
{
    class EventSchedule
    {
        public static string autoBackup = "", autoSaveWorld = "", bloodmoon = "", breakTime = "", infoTicker = "",
            nightAlert = "", playerLogs = "", realWorldTime = "", shutdown = "", watchList = "", zones = "";
        public static Dictionary<string, DateTime> Schedule = new Dictionary<string, DateTime>();

        public static void Add(string _classMethod, DateTime _time)
        {
            if (_time != null)
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
        }

        public static void Remove(string _className)
        {
            if (Schedule.ContainsKey(_className))
            {
                Schedule.Remove(_className);
            }
        }

        public static void Exec()
        {
            try
            {
                if (Schedule != null && Schedule.Count > 0)
                {
                    foreach (var entry in Schedule.ToArray())
                    {
                        switch (entry.Key)
                        {
                            case "AutoBackup":
                                if (DateTime.Now >= entry.Value)
                                {
                                    autoBackup = "";
                                    AutoBackup.SetDelay();
                                    AutoBackup.Exec();
                                }
                                continue;
                            case "AutoSaveWorld":
                                if (DateTime.Now >= entry.Value)
                                {
                                    autoSaveWorld = "";
                                    AutoSaveWorld.SetDelay();
                                    AutoSaveWorld.Save();
                                }
                                continue;
                            case "Bloodmoon":
                                if (DateTime.Now >= entry.Value)
                                {
                                    bloodmoon = "";
                                    Bloodmoon.SetDelay();
                                    Bloodmoon.StatusCheck();
                                }
                                continue;
                            case "BreakTime":
                                if (DateTime.Now >= entry.Value)
                                {
                                    breakTime = "";
                                    BreakTime.SetDelay();
                                    BreakTime.Exec();
                                }
                                continue;
                            case "InfoTicker":
                                if (DateTime.Now >= entry.Value)
                                {
                                    infoTicker = "";
                                    InfoTicker.SetDelay();
                                    InfoTicker.Exec();
                                }
                                continue;
                            case "NightAlert":
                                if (DateTime.Now >= entry.Value)
                                {
                                    nightAlert = "";
                                    NightAlert.SetDelay();
                                    NightAlert.Exec();
                                }
                                continue;
                            case "PlayerLogs":
                                if (DateTime.Now >= entry.Value)
                                {
                                    playerLogs = "";
                                    PlayerLogs.SetDelay();
                                    PlayerLogs.Exec();
                                }
                                continue;
                            case "RealWorldTime":
                                if (DateTime.Now >= entry.Value)
                                {
                                    realWorldTime = "";
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
                            case "WatchList":
                                if (DateTime.Now >= entry.Value)
                                {
                                    watchList = "";
                                    WatchList.SetDelay();
                                    WatchList.Exec();
                                }
                                continue;
                            case "Zones":
                                if (DateTime.Now >= entry.Value)
                                {
                                    zones = "";
                                    Zones.SetDelay();
                                    Zones.ReminderExec();
                                }
                                continue;
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
