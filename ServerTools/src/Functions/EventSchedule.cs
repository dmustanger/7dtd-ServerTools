using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ServerTools
{
    public class EventSchedule
    {
        public static Dictionary<string, DateTime> Schedule = new Dictionary<string, DateTime>();
        public static ConcurrentDictionary<string, DateTime> NewEntries = new ConcurrentDictionary<string, DateTime>();
        public static ConcurrentBag<string> Expired = new ConcurrentBag<string>();

        private static DateTime dateTime;
        private static string[] split;
        private static KeyValuePair<string, DateTime>[] Entries;

        public static void Exec()
        {
            try
            {
                while (Expired.Count > 0)
                {
                    if (Expired.TryTake(out string entry))
                    {
                        RemoveFromSchedule(entry);
                    }
                }
                if (NewEntries.Count > 0)
                {
                    Entries = NewEntries.ToArray();
                    for (int i = 0; i < Entries.Length; i++)
                    {
                        if (Entries[i].Key != null && !Schedule.ContainsKey(Entries[i].Key) && NewEntries.TryRemove(Entries[i].Key, out DateTime date))
                        {
                            Schedule.Add(Entries[i].Key, date);
                        }
                    }
                }
                if (Schedule.Count > 0)
                {
                    dateTime = DateTime.Now;
                    foreach (var entry in Schedule)
                    {
                        switch (entry.Key)
                        {
                            case "AutoBackup":
                                if (dateTime >= entry.Value)
                                {
                                    Expired.Add(entry.Key);
                                    try
                                    {
                                        AutoBackup.Exec();
                                    }
                                    finally
                                    {
                                        AutoBackup.SetDelay(true);
                                    }
                                }
                                break;
                            case "AutoSaveWorld":
                                if (dateTime >= entry.Value)
                                {
                                    Expired.Add(entry.Key);
                                    try
                                    {
                                        AutoSaveWorld.Save();
                                    }
                                    finally
                                    {
                                        AutoSaveWorld.SetDelay(true);
                                    }
                                }
                                break;
                            case "Bloodmoon":
                                if (dateTime >= entry.Value)
                                {
                                    Expired.Add(entry.Key);
                                    try
                                    {
                                        Bloodmoon.StatusCheck();
                                    }
                                    finally
                                    {
                                        Bloodmoon.SetDelay(true);
                                    }
                                }
                                break;
                            case "Bonus":
                                if (dateTime >= entry.Value)
                                {
                                    Expired.Add(entry.Key);
                                    split = entry.Key.Split('_');
                                    GeneralOperations.SessionBonus(split[1]);
                                    AddToSchedule(entry.Key, dateTime.AddMinutes(15));
                                }
                                break;
                            case "BreakReminder":
                                if (dateTime >= entry.Value)
                                {
                                    Expired.Add(entry.Key);
                                    try
                                    {
                                        BreakReminder.Exec();
                                    }
                                    finally
                                    {
                                        BreakReminder.SetDelay(true);
                                    }
                                }
                                break;
                            case "Hordes":
                                if (dateTime >= entry.Value)
                                {
                                    Expired.Add(entry.Key);
                                    try
                                    {
                                        Hordes.Exec();
                                    }
                                    finally
                                    {
                                        Hordes.SetDelay(true);
                                    }
                                }
                                break;
                            case "InfoTicker":
                                if (dateTime >= entry.Value)
                                {
                                    Expired.Add(entry.Key);
                                    try
                                    {
                                        ThreadManager.AddSingleTask(delegate (ThreadManager.TaskInfo _taskInfo)
                                        {
                                            InfoTicker.Exec();
                                        });
                                    }
                                    finally
                                    {
                                        InfoTicker.SetDelay(true);
                                    }
                                }
                                break;
                            case "Lottery":
                                if (dateTime >= entry.Value)
                                {
                                    Expired.Add(entry.Key);
                                    try
                                    {
                                        Lottery.DrawLottery();
                                    }
                                    catch
                                    {
                                        Log.Warning("[SERVERTOOLS] Failed to run the scheduled Lottery");
                                    }
                                }
                                break;
                            case "NightAlert":
                                if (dateTime >= entry.Value)
                                {
                                    Expired.Add(entry.Key);
                                    try
                                    {
                                        NightAlert.Exec();
                                    }
                                    finally
                                    {
                                        NightAlert.SetDelay(true);
                                    }
                                }
                                break;
                            case "PlayerLogs":
                                if (dateTime >= entry.Value)
                                {
                                    Expired.Add(entry.Key);
                                    try
                                    {
                                        ThreadManager.AddSingleTask(delegate (ThreadManager.TaskInfo _taskInfo)
                                        {
                                            PlayerLogs.Exec();
                                        });
                                    }
                                    finally
                                    {
                                        PlayerLogs.SetDelay(true);
                                    }
                                }
                                break;
                            case "RealWorldTime":
                                if (dateTime >= entry.Value)
                                {
                                    Expired.Add(entry.Key);
                                    try
                                    {
                                        RealWorldTime.Exec();
                                    }
                                    finally
                                    {
                                        RealWorldTime.SetDelay(true);
                                    }
                                }
                                break;
                            case "Shutdown":
                                if (dateTime >= entry.Value)
                                {
                                    Expired.Add(entry.Key);
                                    try
                                    {
                                        Shutdown.PrepareShutdown();
                                    }
                                    catch
                                    {
                                        Log.Warning("[SERVERTOOLS] Failed to run the scheduled shutdown");
                                    }
                                }
                                break;
                            case "WatchList":
                                if (dateTime >= entry.Value)
                                {
                                    Expired.Add(entry.Key);
                                    try
                                    {
                                        WatchList.Exec();
                                    }
                                    finally
                                    {
                                        WatchList.SetDelay(true);
                                    }
                                }
                                break;
                            case "Zones":
                                if (dateTime >= entry.Value)
                                {
                                    Expired.Add(entry.Key);
                                    try
                                    {
                                        Zones.ReminderExec();
                                    }
                                    finally
                                    {
                                        Zones.SetDelay(true);
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in EventSchedule.Exec: {0}", e.Message);
            }
        }
        
        public static void AddToSchedule(string _toolName, DateTime _time)
        {
            if (!NewEntries.ContainsKey(_toolName))
            {
                if (!NewEntries.TryAdd(_toolName, _time))
                {
                    Timers.AddEventToSchedule(_toolName, _time);
                }
            }
        }

        public static void RemoveFromSchedule(string _toolName)
        {
            if (Schedule.ContainsKey(_toolName))
            {
                Schedule.Remove(_toolName);
            }
        }
    }
}
