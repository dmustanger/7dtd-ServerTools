using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerTools
{
    class EventSchedule
    {
        public static List<string> Expired = new List<string>();
        public static Dictionary<string, DateTime> Schedule = new Dictionary<string, DateTime>();

        public static void Add(string _classMethod_Time)
        {
            if (!string.IsNullOrEmpty(_classMethod_Time))
            {
                string[] split = _classMethod_Time.Split('_');
                if (DateTime.TryParse(split[1], out DateTime dateTime))
                {
                    if (!Schedule.ContainsKey(_classMethod_Time))
                    {
                        Schedule.Add(_classMethod_Time, dateTime);
                    }
                }
            }
        }

        public static void Remove(string _classMethod)
        {
            if (Schedule.ContainsKey(_classMethod))
            {
                Schedule.Remove(_classMethod);
            }
        }

        public static void Exec()
        {
            try
            {
                if (Schedule != null && Schedule.Count > 0)
                {
                    var schedule = Schedule.ToArray();
                    for (int i = 0; i < schedule.Length; i++)
                    {
                        string[] split = schedule[i].Key.Split('_');
                        switch (split[0])
                        {
                            case "AutoBackup":
                                if (!Expired.Contains(schedule[i].Key) && DateTime.Now >= schedule[i].Value)
                                {
                                    Expired.Add(schedule[i].Key);
                                    AutoBackup.SetDelay();
                                    AutoBackup.Exec();
                                }
                                continue;
                            case "AutoSaveWorld":
                                if (!Expired.Contains(schedule[i].Key) && DateTime.Now >= schedule[i].Value)
                                {
                                    Expired.Add(schedule[i].Key);
                                    AutoSaveWorld.SetDelay();
                                    AutoSaveWorld.Save();
                                }
                                continue;
                            case "Bloodmoon":
                                if (!Expired.Contains(schedule[i].Key) && DateTime.Now >= schedule[i].Value)
                                {
                                    Expired.Add(schedule[i].Key);
                                    Bloodmoon.SetDelay();
                                    Bloodmoon.StatusCheck();
                                }
                                continue;
                            case "Bonus":
                                if (DateTime.Now >= schedule[i].Value)
                                {
                                    Schedule.Remove(schedule[i].Key);
                                    if (GeneralFunction.SessionBonus(split[1]))
                                    {
                                        Schedule.Add(schedule[i].Key, DateTime.Now.AddMinutes(15));
                                    }
                                }
                                continue;
                            case "BreakTime":
                                if (!Expired.Contains(schedule[i].Key) && DateTime.Now >= schedule[i].Value)
                                {
                                    Expired.Add(schedule[i].Key);
                                    BreakTime.SetDelay();
                                    BreakTime.Exec();
                                }
                                continue;
                            case "InfoTicker":
                                if (!Expired.Contains(schedule[i].Key) && DateTime.Now >= schedule[i].Value)
                                {
                                    Expired.Add(schedule[i].Key);
                                    InfoTicker.SetDelay();
                                    InfoTicker.Exec();
                                }
                                continue;
                            case "Lottery":
                                if (!Expired.Contains(schedule[i].Key) && DateTime.Now >= schedule[i].Value)
                                {
                                    Expired.Add(schedule[i].Key);
                                    Lottery.DrawLottery();
                                }
                                continue;
                            case "NightAlert":
                                if (!Expired.Contains(schedule[i].Key) && DateTime.Now >= schedule[i].Value)
                                {
                                    Expired.Add(schedule[i].Key);
                                    NightAlert.SetDelay();
                                    NightAlert.Exec();
                                }
                                continue;
                            case "PlayerLogs":
                                if (!Expired.Contains(schedule[i].Key) && DateTime.Now >= schedule[i].Value)
                                {
                                    Expired.Add(schedule[i].Key);
                                    PlayerLogs.SetDelay();
                                    PlayerLogs.Exec();
                                }
                                continue;
                            case "RealWorldTime":
                                if (!Expired.Contains(schedule[i].Key) && DateTime.Now >= schedule[i].Value)
                                {
                                    Expired.Add(schedule[i].Key);
                                    RealWorldTime.SetDelay();
                                    RealWorldTime.Exec();
                                }
                                continue;
                            case "Reset":
                                if (DateTime.Now >= schedule[i].Value)
                                {
                                    Schedule.Remove(schedule[i].Key);
                                    Reset();
                                }
                                continue;
                            case "Shutdown":
                                if (!Expired.Contains(schedule[i].Key) && DateTime.Now >= schedule[i].Value)
                                {
                                    Expired.Add(schedule[i].Key);
                                    Shutdown.PrepareShutdown();
                                }
                                continue;
                            case "WatchList":
                                if (!Expired.Contains(schedule[i].Key) && DateTime.Now >= schedule[i].Value)
                                {
                                    Expired.Add(schedule[i].Key);
                                    WatchList.SetDelay();
                                    WatchList.Exec();
                                }
                                continue;
                            case "Zones":
                                if (!Expired.Contains(schedule[i].Key) && DateTime.Now >= schedule[i].Value)
                                {
                                    Expired.Add(schedule[i].Key);
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

        public static void Reset()
        {
            if (AutoBackup.IsEnabled)
            {
                AutoBackup.SetDelay();
            }
            if (AutoSaveWorld.IsEnabled)
            {
                AutoSaveWorld.SetDelay();
            }
            if (Bloodmoon.IsEnabled)
            {
                Bloodmoon.SetDelay();
            }
            if (BreakTime.IsEnabled)
            {
                BreakTime.SetDelay();
            }
            if (InfoTicker.IsEnabled)
            {
                InfoTicker.SetDelay();
            }
            if (NightAlert.IsEnabled)
            {
                NightAlert.SetDelay();
            }
            if (PlayerLogs.IsEnabled)
            {
                PlayerLogs.SetDelay();
            }
            if (RealWorldTime.IsEnabled)
            {
                RealWorldTime.SetDelay();
            }
            if (WatchList.IsEnabled)
            {
                WatchList.SetDelay();
            }
            if (Zones.IsEnabled)
            {
                Zones.SetDelay();
            }
            Schedule.Add("Reset_", DateTime.Today.AddDays(1).AddSeconds(1));
        }

        public static void Clear(string _toolName)
        {
            if (Schedule != null && Schedule.Count > 0)
            {
                var schedule = Schedule.ToArray();
                for (int i = 0; i < schedule.Length; i++)
                {
                    if (schedule[i].Key.Contains(_toolName))
                    {
                        Schedule.Remove(_toolName);
                    }
                }
            }
        }

        public static void RemoveBonusEntry(string _name)
        {
            if (Schedule.ContainsKey(_name))
            {
                Schedule.Remove(_name);
            }
        }
    }
}
