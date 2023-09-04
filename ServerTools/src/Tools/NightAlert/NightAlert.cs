using System;

namespace ServerTools
{
    class NightAlert
    {
        public static bool IsEnabled = false;
        public static string Delay = "60";

        private static string EventDelay = "";
        private static DateTime time = new DateTime();

        public static void SetDelay(bool _loading)
        {
            if (EventDelay != Delay || _loading)
            {
                EventSchedule.Expired.Add("NightAlert");
                EventDelay = Delay;
                if (Delay.Contains(",") && Delay.Contains(":"))
                {
                    string[] times = Delay.Split(',');
                    for (int i = 0; i < times.Length; i++)
                    {
                        string[] timeSplit1 = times[i].Split(':');
                        int.TryParse(timeSplit1[0], out int hours1);
                        int.TryParse(timeSplit1[1], out int minutes1);
                        time = DateTime.Today.AddHours(hours1).AddMinutes(minutes1);
                        if (DateTime.Now < time)
                        {
                            EventSchedule.AddToSchedule("NightAlert", time);
                            return;
                        }
                    }
                    string[] timeSplit2 = times[0].Split(':');
                    int.TryParse(timeSplit2[0], out int hours2);
                    int.TryParse(timeSplit2[1], out int minutes2);
                    time = DateTime.Today.AddDays(1).AddHours(hours2).AddMinutes(minutes2);
                    EventSchedule.AddToSchedule("NightAlert", time);
                    return;
                }
                else if (Delay.Contains(":"))
                {
                    string[] timeSplit2 = Delay.Split(':');
                    int.TryParse(timeSplit2[0], out int hours2);
                    int.TryParse(timeSplit2[1], out int minutes2);
                    time = DateTime.Today.AddHours(hours2).AddMinutes(minutes2);
                    if (DateTime.Now < time)
                    {
                        EventSchedule.AddToSchedule("NightAlert", time);
                    }
                    else
                    {
                        time = DateTime.Today.AddDays(1).AddHours(hours2).AddMinutes(minutes2);
                        EventSchedule.AddToSchedule("NightAlert", time);
                    }
                    return;
                }
                else
                {
                    if (int.TryParse(Delay, out int delay))
                    {
                        time = DateTime.Now.AddMinutes(delay);
                        EventSchedule.AddToSchedule("NightAlert", time);
                    }
                    else
                    {
                        Log.Out("[SERVERTOOLS] Invalid Night_Alert Delay detected. Use a single integer, 24h time or multiple 24h time entries");
                        Log.Out("[SERVERTOOLS] Example: 120 or 03:00 or 03:00, 06:00, 09:00");
                    }
                    return;
                }
            }
        }

        public static void Exec()
        {
            if (ConnectionManager.Instance.ClientCount() > 0 && GameManager.Instance.World.IsDaytime())
            {
                ulong worldTime = GameManager.Instance.World.worldTime;
                int twentyFourHourTime = (int)(worldTime / 1000UL) % 24;
                int dusk = (int)SkyManager.GetDuskTime();
                int timeRemaining = dusk - twentyFourHourTime;
                Phrases.Dict.TryGetValue("NightAlert1", out string phrase);
                phrase = phrase.Replace("{Value}", timeRemaining.ToString());
                ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
            }
        }
    }
}
