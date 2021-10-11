using System;
using System.Collections.Generic;

namespace ServerTools
{
    class RealWorldTime
    {
        public static bool IsEnabled = false;
        public static string Time_Zone = "UTC", Delay = "60";
        public static int Adjustment = 0;

        public static void SetDelay()
        {
            if (EventSchedule.realWorldTime != Delay)
            {
                EventSchedule.realWorldTime = Delay;
                if (Delay.Contains(",") && Delay.Contains(":"))
                {
                    string[] times = Delay.Split(',');
                    for (int i = 0; i < times.Length; i++)
                    {
                        if (DateTime.TryParse(DateTime.Today.ToString("d") + " " + times[i] + ":00", out DateTime time))
                        {
                            if (DateTime.Now < time)
                            {
                                EventSchedule.Add("RealWorldTime", time);
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
                                EventSchedule.Add("RealWorldTime", time);
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
                            EventSchedule.Add("RealWorldTime", time);
                        }
                        else if (DateTime.TryParse(DateTime.Today.AddDays(1).ToString("d") + " " + Delay + ":00", out DateTime secondaryTime))
                        {
                            EventSchedule.Add("RealWorldTime", secondaryTime);
                        }
                    }
                }
                else
                {
                    if (int.TryParse(Delay, out int delay))
                    {
                        EventSchedule.Add("RealWorldTime", DateTime.Now.AddSeconds(delay));
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
            DateTime _time = DateTime.Now;
            if (Adjustment != 0)
            {
                _time = DateTime.Now.AddHours(Adjustment);
            }
            Phrases.Dict.TryGetValue("RealWorldTime1", out string _phrase);
            _phrase = _phrase.Replace("{Time}", _time.ToShortTimeString());
            _phrase = _phrase.Replace("{TimeZone}", Time_Zone);
            List<ClientInfo> _cInfoList = PersistentOperations.ClientList();
            if (_cInfoList != null && _cInfoList.Count > 0)
            {
                for (int i = 0; i < _cInfoList.Count; i++)
                {
                    ClientInfo _cInfo = _cInfoList[i];
                    if (_cInfo != null)
                    {
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
        }
    }
}
