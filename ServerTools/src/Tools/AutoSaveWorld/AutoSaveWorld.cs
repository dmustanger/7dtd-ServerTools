
using System;

namespace ServerTools
{
    public class AutoSaveWorld
    {
        public static bool IsEnabled = false;
        public static string Delay = "60";

        public static void SetDelay()
        {
            if (EventSchedule.autoSaveWorld != Delay)
            {
                EventSchedule.autoSaveWorld = Delay;
                if (Delay.Contains(",") && Delay.Contains(":"))
                {
                    string[] times = Delay.Split(',');
                    for (int i = 0; i < times.Length; i++)
                    {
                        if (DateTime.TryParse(DateTime.Today.ToString("d") + " " + times[i] + ":00", out DateTime time))
                        {
                            if (DateTime.Now < time)
                            {
                                EventSchedule.Add("AutoSaveWorld", time);
                                return;
                            }
                        }
                    }
                    for (int i = 0; i < times.Length; i++)
                    {
                        if (DateTime.TryParse(DateTime.Today.AddDays(1).ToString("d") + " " + times[i] + ":00", out DateTime time))
                        {
                            if (DateTime.Now < time)
                            {
                                EventSchedule.Add("AutoSaveWorld", time);
                                return;
                            }
                        }
                    }
                }
                else if (Delay.Contains(":"))
                {
                    if (DateTime.TryParse(DateTime.Today.ToString("d") + " " + Delay + ":00", out DateTime time))
                    {
                        if (DateTime.Now < time)
                        {
                            EventSchedule.Add("AutoSaveWorld", time);
                        }
                        else if (DateTime.TryParse(DateTime.Today.AddDays(1).ToString("d") + " " + Delay + ":00", out DateTime secondaryTime))
                        {
                            EventSchedule.Add("AutoSaveWorld", secondaryTime);
                        }
                    }
                }
                else
                {
                    if (int.TryParse(Delay, out int delay))
                    {
                        EventSchedule.Add("AutoSaveWorld", DateTime.Now.AddMinutes(delay));
                    }
                    else
                    {
                        Log.Out("[SERVERTOOLS] Invalid Auto_Save_World Delay_Between_Saves detected. Use a single integer, 24h time or multiple 24h time entries");
                        Log.Out("[SERVERTOOLS] Example: 120 or 03:00 or 03:00, 06:00, 09:00");
                    }
                }
            }
        }

        public static void Save()
        {
            Log.Out("[SERVERTOOLS] World save has begun.");
            GameManager.Instance.SaveLocalPlayerData();
            GameManager.Instance.SaveWorld();
            Log.Out("[SERVERTOOLS] World save complete.");
        }
    }
}