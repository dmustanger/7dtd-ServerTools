using System;

namespace ServerTools
{
    class NightAlert
    {
        public static bool IsEnabled = false;
        public static string Delay = "60";

        private static string EventDelay = "";

        public static void SetDelay(bool _reset)
        {
            if (EventDelay != Delay || _reset)
            {
                EventDelay = Delay;
                EventSchedule.Clear("NightAlert_");
                if (Delay.Contains(",") && Delay.Contains(":"))
                {
                    string[] times = Delay.Split(',');
                    for (int i = 0; i < times.Length; i++)
                    {
                        string[] timeSplit = times[i].Split(':');
                        int.TryParse(timeSplit[0], out int hours);
                        int.TryParse(timeSplit[1], out int minutes);
                        DateTime time = DateTime.Today.AddHours(hours).AddMinutes(minutes);
                        EventSchedule.Schedule.Add("NightAlert_" + time, time);
                    }
                }
                else if (Delay.Contains(":"))
                {
                    string[] timeSplit = Delay.Split(':');
                    int.TryParse(timeSplit[0], out int hours);
                    int.TryParse(timeSplit[1], out int minutes);
                    DateTime time = DateTime.Today.AddHours(hours).AddMinutes(minutes);
                    EventSchedule.Schedule.Add("NightAlert_" + time, time);
                }
                else
                {
                    if (int.TryParse(Delay, out int delay))
                    {
                        DateTime time = DateTime.Now.AddMinutes(delay);
                        EventSchedule.Schedule.Add("NightAlert_" + time, time);
                    }
                    else
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Invalid Night_Alert Delay detected. Use a single integer, 24h time or multiple 24h time entries"));
                        Log.Out(string.Format("[SERVERTOOLS] Example: 120 or 03:00 or 03:00, 06:00, 09:00"));
                    }
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
