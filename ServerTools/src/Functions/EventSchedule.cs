using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerTools
{
    public class EventSchedule
    {
        public static List<string> Expired = new List<string>();
        public static Dictionary<string, DateTime> Schedule = new Dictionary<string, DateTime>();

        public static void Exec()
        {
            try
            {
                if (Schedule != null && Schedule.Count > 0)
                {
                    var schedule = Schedule.ToArray();
                    DateTime dateTime = DateTime.Now;
                    DateTime dateTimeTwo = dateTime.AddSeconds(-22);
                    string[] split;
                    for (int i = 0; i < schedule.Length; i++)
                    {
                        split = schedule[i].Key.Split('_');
                        switch (split[0])
                        {
                            case "AutoBackup":
                                if (!Expired.Contains(schedule[i].Key) && dateTime >= schedule[i].Value && dateTimeTwo <= schedule[i].Value)
                                {
                                    Expired.Add(schedule[i].Key);
                                    AutoBackup.Exec();
                                    AutoBackup.SetDelay(true);
                                }
                                continue;
                            case "AutoSaveWorld":
                                if (!Expired.Contains(schedule[i].Key) && dateTime >= schedule[i].Value && dateTimeTwo <= schedule[i].Value)
                                {
                                    Expired.Add(schedule[i].Key);
                                    AutoSaveWorld.SetDelay(true);
                                    AutoSaveWorld.Save();
                                }
                                continue;
                            case "Bloodmoon":
                                if (!Expired.Contains(schedule[i].Key) && dateTime >= schedule[i].Value && dateTimeTwo <= schedule[i].Value)
                                {
                                    Expired.Add(schedule[i].Key);
                                    Bloodmoon.SetDelay(true);
                                    Bloodmoon.StatusCheck();
                                }
                                continue;
                            case "Bonus":
                                if (dateTime >= schedule[i].Value)
                                {
                                    Schedule.Remove(schedule[i].Key);
                                    if (GeneralOperations.SessionBonus(split[1]))
                                    {
                                        Schedule.Add(schedule[i].Key, dateTime.AddMinutes(15));
                                    }
                                }
                                continue;
                            case "BreakTime":
                                if (!Expired.Contains(schedule[i].Key) && dateTime >= schedule[i].Value && dateTimeTwo <= schedule[i].Value)
                                {
                                    Expired.Add(schedule[i].Key);
                                    BreakTime.Exec();
                                    BreakTime.SetDelay(true);
                                }
                                continue;
                            case "InfoTicker":
                                if (!Expired.Contains(schedule[i].Key) && dateTime >= schedule[i].Value && dateTimeTwo <= schedule[i].Value)
                                {
                                    Expired.Add(schedule[i].Key);
                                    InfoTicker.Exec();
                                    InfoTicker.SetDelay(true);
                                }
                                continue;
                            case "Lottery":
                                if (!Expired.Contains(schedule[i].Key) && dateTime >= schedule[i].Value && dateTimeTwo <= schedule[i].Value)
                                {
                                    Expired.Add(schedule[i].Key);
                                    Lottery.DrawLottery();
                                }
                                continue;
                            case "NightAlert":
                                if (!Expired.Contains(schedule[i].Key) && dateTime >= schedule[i].Value && dateTimeTwo <= schedule[i].Value)
                                {
                                    Expired.Add(schedule[i].Key);
                                    NightAlert.Exec();
                                    NightAlert.SetDelay(true);
                                }
                                continue;
                            case "PlayerLogs":
                                if (!Expired.Contains(schedule[i].Key) && dateTime >= schedule[i].Value && dateTimeTwo <= schedule[i].Value)
                                {
                                    Expired.Add(schedule[i].Key);
                                    PlayerLogs.SetDelay(true);
                                    PlayerLogs.Exec();
                                }
                                continue;
                            case "RealWorldTime":
                                if (!Expired.Contains(schedule[i].Key) && dateTime >= schedule[i].Value && dateTimeTwo <= schedule[i].Value)
                                {
                                    Expired.Add(schedule[i].Key);
                                    RealWorldTime.SetDelay(true);
                                    RealWorldTime.Exec();
                                }
                                continue;
                            case "Reset":
                                if (dateTime >= schedule[i].Value)
                                {
                                    Schedule.Remove(schedule[i].Key);
                                    Reset();
                                }
                                continue;
                            case "Shutdown":
                                if (!Expired.Contains(schedule[i].Key) && dateTime >= schedule[i].Value && dateTimeTwo <= schedule[i].Value)
                                {
                                    Expired.Add(schedule[i].Key);
                                    Shutdown.PrepareShutdown();
                                }
                                continue;
                            case "WatchList":
                                if (!Expired.Contains(schedule[i].Key) && dateTime >= schedule[i].Value && dateTimeTwo <= schedule[i].Value)
                                {
                                    Expired.Add(schedule[i].Key);
                                    WatchList.Exec();
                                    WatchList.SetDelay(true);
                                }
                                continue;
                            case "Zones":
                                if (!Expired.Contains(schedule[i].Key) && dateTime >= schedule[i].Value && dateTimeTwo <= schedule[i].Value)
                                {
                                    Expired.Add(schedule[i].Key);
                                    Zones.ReminderExec();
                                    Zones.SetDelay(true);
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

        public static void Remove(string _classMethod)
        {
            if (Schedule.ContainsKey(_classMethod))
            {
                Schedule.Remove(_classMethod);
            }
        }

        public static void Reset()
        {
            if (AutoBackup.IsEnabled)
            {
                AutoBackup.SetDelay(true);
            }
            if (AutoSaveWorld.IsEnabled)
            {
                AutoSaveWorld.SetDelay(true);
            }
            if (Bloodmoon.IsEnabled)
            {
                Bloodmoon.SetDelay(true);
            }
            if (BreakTime.IsEnabled)
            {
                BreakTime.SetDelay(true);
            }
            if (InfoTicker.IsEnabled)
            {
                InfoTicker.SetDelay(true);
            }
            if (NightAlert.IsEnabled)
            {
                NightAlert.SetDelay(true);
            }
            if (PlayerLogs.IsEnabled)
            {
                PlayerLogs.SetDelay(true);
            }
            if (RealWorldTime.IsEnabled)
            {
                RealWorldTime.SetDelay(true);
            }
            if (WatchList.IsEnabled)
            {
                WatchList.SetDelay(true);
            }
            if (Zones.IsEnabled)
            {
                Zones.SetDelay(true);
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
