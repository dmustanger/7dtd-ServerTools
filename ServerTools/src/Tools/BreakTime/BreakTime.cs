

using System;

namespace ServerTools
{
    class BreakTime
    {
        public static bool IsEnabled = false;
        public static string Message = "It has been {Time} minutes since the last break reminder. Stretch and get some water.", Delay = "60";

        public static void SetDelay()
        {
            if (EventSchedule.breakTime != Delay)
            {
                EventSchedule.breakTime = Delay;
                if (Delay.Contains(",") && Delay.Contains(":"))
                {
                    string[] times = Delay.Split(',');
                    for (int i = 0; i < times.Length; i++)
                    {
                        if (DateTime.TryParse(DateTime.Today.ToString("d") + " " + times[i] + ":00", out DateTime time))
                        {
                            if (DateTime.Now < time)
                            {
                                EventSchedule.Add("BreakTime", time);
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
                                EventSchedule.Add("BreakTime", time);
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
                            EventSchedule.Add("BreakTime", time);
                        }
                        else if (DateTime.TryParse(DateTime.Today.AddDays(1).ToString("d") + " " + Delay + ":00", out DateTime secondaryTime))
                        {
                            EventSchedule.Add("BreakTime", secondaryTime);
                        }
                    }
                }
                else
                {
                    if (int.TryParse(Delay, out int delay))
                    {
                        EventSchedule.Add("BreakTime", DateTime.Now.AddMinutes(delay));
                    }
                    else
                    {
                        Log.Out("[SERVERTOOLS] Invalid Break_Time Delay detected. Use a single integer, 24h time or multiple 24h time entries");
                        Log.Out("[SERVERTOOLS] Example: 120 or 03:00 or 03:00, 06:00, 09:00");
                    }
                }
            }
        }

        public static void Exec()
        {
            if (ConnectionManager.Instance.ClientCount() > 0)
            {
                Message = Message.Replace("{Time}", Delay.ToString());
                ChatHook.ChatMessage(null, Config.Chat_Response_Color + Message + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
            }
        }
    }
}
