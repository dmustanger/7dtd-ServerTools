using System;

namespace ServerTools
{
    public class Bloodmoon
    {
        public static bool IsEnabled = false, Show_On_Login = false, Show_On_Respawn = false;
        public static string Command_bloodmoon = "bloodmoon", Command_bm = "bm", Delay = "60";

        private static string EventDelay = "";
        private static DateTime time = new DateTime();

        public static void SetDelay(bool _reset)
        {
            if (EventDelay != Delay || _reset)
            {
                EventSchedule.Expired.Add("Bloodmoon");
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
                            EventSchedule.AddToSchedule("Bloodmoon", time);
                            return;
                        }
                    }
                    string[] timeSplit2 = times[0].Split(':');
                    int.TryParse(timeSplit2[0], out int hours2);
                    int.TryParse(timeSplit2[1], out int minutes2);
                    time = DateTime.Today.AddDays(1).AddHours(hours2).AddMinutes(minutes2);
                    EventSchedule.AddToSchedule("Bloodmoon", time);
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
                        EventSchedule.AddToSchedule("Bloodmoon", time);
                    }
                    else
                    {
                        time = DateTime.Today.AddDays(1).AddHours(hours2).AddMinutes(minutes2);
                        EventSchedule.AddToSchedule("Bloodmoon", time);
                    }
                    return;
                }
                else
                {
                    if (int.TryParse(Delay, out int delay))
                    {
                        time = DateTime.Now.AddMinutes(delay);
                        EventSchedule.AddToSchedule("Bloodmoon", time);
                    }
                    else
                    {
                        Log.Out("[SERVERTOOLS] Invalid Bloodmoon Delay detected. Use a single integer, 24h time or multiple 24h time entries");
                        Log.Out("[SERVERTOOLS] Example: 120 or 03:00 or 03:00, 06:00, 09:00");
                    }
                    return;
                }
            }
        }

        public static void Exec(ClientInfo _cInfo)
        {
            int daysRemaining = Day7.DaysRemaining(GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime()));
            if (daysRemaining == 0 && !GeneralOperations.IsBloodmoon())
            {
                Phrases.Dict.TryGetValue("Bloodmoon2", out string phrase);
                phrase = phrase.Replace("{Time}", GameManager.Instance.World.DuskHour.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else if (GeneralOperations.IsBloodmoon())
            {
                Phrases.Dict.TryGetValue("Bloodmoon3", out string phrase);
                phrase = phrase.Replace("{Time}", GameManager.Instance.World.DawnHour.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                Phrases.Dict.TryGetValue("Bloodmoon1", out string phrase);
                phrase = phrase.Replace("{Value}", daysRemaining.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void StatusCheck()
        {
            if (ConnectionManager.Instance.ClientCount() > 0)
            {
                int daysRemaining = Day7.DaysRemaining(GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime()));
                if (daysRemaining == 0 && !GeneralOperations.IsBloodmoon())
                {
                    Phrases.Dict.TryGetValue("Bloodmoon2", out string phrase);
                    phrase = phrase.Replace("{Time}", GameManager.Instance.World.DuskHour.ToString());
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                }
                else if (GeneralOperations.IsBloodmoon())
                {
                    Phrases.Dict.TryGetValue("Bloodmoon3", out string phrase);
                    phrase = phrase.Replace("{Time}", GameManager.Instance.World.DawnHour.ToString());
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                }
                else
                {
                    Phrases.Dict.TryGetValue("Bloodmoon1", out string phrase);
                    phrase = phrase.Replace("{Value}", daysRemaining.ToString());
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                }
            }
        }
    }
}