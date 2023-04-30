using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerTools
{
    public class EventSchedule
    {
        public static Dictionary<string, DateTime> Schedule = new Dictionary<string, DateTime>();
        public static Dictionary<string, DateTime> NewEntries = new Dictionary<string, DateTime>();
        public static List<string> Expired = new List<string>();

        private static DateTime dateTime;
        private static string[] split;
        private static KeyValuePair<string, DateTime>[] expired;

        public static void Exec()
        {
            try
            {
                if (Expired.Count > 0)
                {
                    expired = Schedule.ToArray();
                    for (int i = 0; i < Expired.Count; i++)
                    {
                        for (int j = 0; j < expired.Length; j++)
                        {
                            if (expired[j].Key.Contains(Expired[i]))
                            {
                                Schedule.Remove(expired[j].Key);
                            }
                        }
                    }
                    Expired.Clear();
                }
                if (Schedule != null && Schedule.Count > 0)
                {
                    dateTime = DateTime.Now;
                    foreach (var entry in Schedule)
                    {
                        split = entry.Key.Split('_');
                        switch (split[0])
                        {
                            case "AutoBackup":
                                if (dateTime >= entry.Value)
                                {
                                    Expired.Add(entry.Key);
                                    AutoBackup.Exec();
                                    AutoBackup.SetDelay(true, false);
                                }
                                break;
                            case "AutoSaveWorld":
                                if (dateTime >= entry.Value)
                                {
                                    Expired.Add(entry.Key);
                                    AutoSaveWorld.Save();
                                    AutoSaveWorld.SetDelay(true, false);
                                }
                                break;
                            case "Bloodmoon":
                                if (dateTime >= entry.Value)
                                {
                                    Expired.Add(entry.Key);
                                    Bloodmoon.StatusCheck();
                                    Bloodmoon.SetDelay(true, false);
                                }
                                break;
                            case "Bonus":
                                if (dateTime >= entry.Value)
                                {
                                    Expired.Add(entry.Key);
                                    if (GeneralOperations.SessionBonus(split[1]))
                                    {
                                        AddToSchedule(entry.Key, dateTime.AddMinutes(15));
                                    }
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
                                        BreakReminder.SetDelay(true, false);
                                    }
                                }
                                break;
                            case "InfoTicker":
                                if (dateTime >= entry.Value)
                                {
                                    Expired.Add(entry.Key);
                                    try
                                    {
                                        InfoTicker.Exec();
                                    }
                                    finally
                                    {
                                        InfoTicker.SetDelay(true, false);
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
                                        NightAlert.SetDelay(true, false);
                                    }
                                }
                                break;
                            case "PlayerLogs":
                                if (dateTime >= entry.Value)
                                {
                                    Expired.Add(entry.Key);
                                    try
                                    {
                                        PlayerLogs.Exec();
                                    }
                                    finally
                                    {
                                        PlayerLogs.SetDelay(true, false);
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
                                        RealWorldTime.SetDelay(true, false);
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
                                        Log.Warning("[SERVERTOOLS] Failed to run the scheduled Shutdown");
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
                                        WatchList.SetDelay(true, false);
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
                                        Zones.SetDelay(true, false);
                                    }
                                }
                                break;
                        }
                    }
                    if (Expired.Count > 0)
                    {
                        expired = Schedule.ToArray();
                        for (int i = 0; i < Expired.Count; i++)
                        {
                            for (int j = 0; j < expired.Length; j++)
                            {
                                if (expired[j].Key.Contains(Expired[i]))
                                {
                                    Schedule.Remove(expired[j].Key);
                                }
                            }
                        }
                        Expired.Clear();
                    }
                }
                if (NewEntries.Count > 0)
                {
                    foreach (var entry in NewEntries)
                    {
                        Schedule.Add(entry.Key, entry.Value);
                    }
                    NewEntries.Clear();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in EventSchedule.Exec: {0}", e.Message));
            }
        }
        
        public static void AddToSchedule(string _toolName, DateTime _date)
        {
            if (!NewEntries.ContainsKey(_toolName))
            {
                NewEntries.Add(_toolName, _date);
            }
        }
    }
}
