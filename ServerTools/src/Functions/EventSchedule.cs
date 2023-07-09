using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class EventSchedule
    {
        public static Dictionary<string, DateTime> Schedule = new Dictionary<string, DateTime>();
        public static Dictionary<string, DateTime> NewEntries = new Dictionary<string, DateTime>();
        public static List<string> Expired = new List<string>();

        private static DateTime dateTime;
        private static string[] split;

        public static void Exec()
        {
            try
            {
                if (Expired.Count > 0)
                {
                    for (int i = 0; i < Expired.Count; i++)
                    {
                        RemoveFromSchedule(Expired[i]);
                    }
                    Expired.Clear();
                }
                if (NewEntries.Count > 0)
                {
                    foreach (var entry in NewEntries)
                    {
                        Schedule.Add(entry.Key, entry.Value);
                    }
                    NewEntries.Clear();
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
                                    AutoBackup.Exec();
                                    AutoBackup.SetDelay(true);
                                }
                                break;
                            case "AutoSaveWorld":
                                if (dateTime >= entry.Value)
                                {
                                    Expired.Add(entry.Key);
                                    AutoSaveWorld.Save();
                                    AutoSaveWorld.SetDelay(true);
                                }
                                break;
                            case "Bloodmoon":
                                if (dateTime >= entry.Value)
                                {
                                    Expired.Add(entry.Key);
                                    Bloodmoon.StatusCheck();
                                    Bloodmoon.SetDelay(true);
                                }
                                break;
                            case "Bonus":
                                if (dateTime >= entry.Value)
                                {
                                    Expired.Add(entry.Key);
                                    split = entry.Key.Split('_');
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
                                        BreakReminder.SetDelay(true);
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
                                        PlayerLogs.Exec();
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
                NewEntries.Add(_toolName, _time);
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
