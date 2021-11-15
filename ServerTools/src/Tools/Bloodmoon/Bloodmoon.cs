using System;

namespace ServerTools
{
    public class Bloodmoon
    {
        public static bool IsEnabled = false, Show_On_Login = false, Show_On_Respawn = false;
        public static string Command_bloodmoon = "bloodmoon", Command_bm = "bm", Delay = "60";

        public static void SetDelay()
        {
            if (EventSchedule.bloodmoon != Delay)
            {
                EventSchedule.bloodmoon = Delay;
                if (Delay.Contains(",") && Delay.Contains(":"))
                {
                    string[] times = Delay.Split(',');
                    for (int i = 0; i < times.Length; i++)
                    {
                        if (DateTime.TryParse(DateTime.Today.ToString("d") + " " + times[i] + ":00", out DateTime time))
                        {
                            if (DateTime.Now < time)
                            {
                                EventSchedule.Add("Bloodmoon", time);
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
                                EventSchedule.Add("Bloodmoon", time);
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
                            EventSchedule.Add("Bloodmoon", time);
                        }
                        else if (DateTime.TryParse(DateTime.Today.AddDays(1).ToString("d") + " " + Delay + ":00", out DateTime secondaryTime))
                        {
                            EventSchedule.Add("Bloodmoon", secondaryTime);
                        }
                    }
                }
                else
                {
                    if (int.TryParse(Delay, out int delay))
                    {
                        EventSchedule.Add("Bloodmoon", DateTime.Now.AddMinutes(delay));
                    }
                    else
                    {
                        Log.Out("[SERVERTOOLS] Invalid Bloodmoon Delay detected. Use a single integer, 24h time or multiple 24h time entries");
                        Log.Out("[SERVERTOOLS] Example: 120 or 03:00 or 03:00, 06:00, 09:00");
                    }
                }
            }
        }

        public static void Exec(ClientInfo _cInfo)
        {
            int daysRemaining = Day7.DaysRemaining(GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime()));
            if (daysRemaining == 0 && !SkyManager.BloodMoon())
            {
                Phrases.Dict.TryGetValue("Bloodmoon2", out string phrase);
                phrase = phrase.Replace("{Time}", GameManager.Instance.World.DuskHour.ToString());
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else if (SkyManager.BloodMoon())
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
                if (daysRemaining == 0 && !PersistentOperations.IsBloodmoon())
                {
                    Phrases.Dict.TryGetValue("Bloodmoon2", out string phrase);
                    ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                }
                else if (PersistentOperations.IsBloodmoon())
                {
                    Phrases.Dict.TryGetValue("Bloodmoon3", out string phrase);
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