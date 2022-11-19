using System;
using System.Collections.Generic;

namespace ServerTools
{
    class RealWorldTime
    {
        public static bool IsEnabled = false;
        public static string Time_Zone = "UTC", Delay = "60";
        public static int Adjustment = 0;

        private static string EventDelay = "";

        public static void SetDelay()
        {
            if (EventDelay != Delay)
            {
                EventDelay = Delay;
                EventSchedule.Clear("RealWorldTime_");
                if (Delay.Contains(",") && Delay.Contains(":"))
                {
                    string[] times = Delay.Split(',');
                    for (int i = 0; i < times.Length; i++)
                    {
                        if (DateTime.TryParse(DateTime.Today.ToString("d") + " " + times[i] + ":00", out DateTime time))
                        {
                            EventSchedule.Add("RealWorldTime_" + time);
                        }
                    }
                }
                else if (Delay.Contains(":"))
                {
                    if (DateTime.TryParse(DateTime.Today.ToString("d") + " " + Delay + ":00", out DateTime time))
                    {
                        EventSchedule.Add("RealWorldTime_" + time);
                    }
                }
                else
                {
                    if (int.TryParse(Delay, out int delay))
                    {
                        EventSchedule.Add("RealWorldTime_" + DateTime.Now.AddMinutes(delay));
                    }
                    else
                    {
                        Log.Out("[SERVERTOOLS] Invalid Real_World_Time Delay detected. Use a single integer, 24h time or multiple 24h time entries");
                        Log.Out("[SERVERTOOLS] Example: 120 or 03:00 or 03:00, 06:00, 09:00");
                    }
                }
            }
        }

        public static void Exec()
        {
            DateTime time = DateTime.Now;
            if (Adjustment != 0)
            {
                time = DateTime.Now.AddHours(Adjustment);
            }
            Phrases.Dict.TryGetValue("RealWorldTime1", out string phrase);
            phrase = phrase.Replace("{Time}", time.ToShortTimeString());
            phrase = phrase.Replace("{TimeZone}", Time_Zone);
            List<ClientInfo> clientList = GeneralFunction.ClientList();
            if (clientList != null)
            {
                for (int i = 0; i < clientList.Count; i++)
                {
                    ClientInfo cInfo = clientList[i];
                    if (cInfo != null)
                    {
                        ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
        }
    }
}
