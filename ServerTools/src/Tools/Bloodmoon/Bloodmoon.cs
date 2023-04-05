using System;

namespace ServerTools
{
    public class Bloodmoon
    {
        public static bool IsEnabled = false, Show_On_Login = false, Show_On_Respawn = false;
        public static string Command_bloodmoon = "bloodmoon", Command_bm = "bm", Delay = "60";

        private static string EventDelay = "";

        public static void SetDelay(bool _reset)
        {
            if (EventDelay != Delay || _reset)
            {
                EventDelay = Delay;
                EventSchedule.Clear("Bloodmoon_");
                if (Delay.Contains(",") && Delay.Contains(":"))
                {
                    string[] times = Delay.Split(',');
                    for (int i = 0; i < times.Length; i++)
                    {
                        string[] timeSplit = times[i].Split(':');
                        int.TryParse(timeSplit[0], out int hours);
                        int.TryParse(timeSplit[1], out int minutes);
                        DateTime time = DateTime.Today.AddHours(hours).AddMinutes(minutes);
                        EventSchedule.Schedule.Add("Bloodmoon_" + time, time);
                    }
                }
                else if (Delay.Contains(":"))
                {
                    string[] timeSplit = Delay.Split(':');
                    int.TryParse(timeSplit[0], out int hours);
                    int.TryParse(timeSplit[1], out int minutes);
                    DateTime time = DateTime.Today.AddHours(hours).AddMinutes(minutes);
                    EventSchedule.Schedule.Add("Bloodmoon_" + time, time);
                }
                else
                {
                    if (int.TryParse(Delay, out int delay))
                    {
                        DateTime time = DateTime.Now.AddMinutes(delay);
                        EventSchedule.Schedule.Add("Bloodmoon_" + time, time);
                    }
                    else
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Invalid Bloodmoon Delay detected. Use a single integer, 24h time or multiple 24h time entries"));
                        Log.Out(string.Format("[SERVERTOOLS] Example: 120 or 03:00 or 03:00, 06:00, 09:00"));
                    }
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