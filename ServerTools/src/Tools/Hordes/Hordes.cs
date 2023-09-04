using System;
using System.Collections.Generic;

namespace ServerTools
{
    class Hordes
    {
        public static bool IsEnabled = false;
        public static int Players = 5, Zombie_Count = 30;
        public static string Delay = "20";

        private static string EventDelay = "";
        private static DateTime time = new DateTime();

        public static void SetDelay(bool _loading)
        {
            if (EventDelay != Delay || _loading)
            {
                EventSchedule.Expired.Add("Hordes");
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
                            EventSchedule.AddToSchedule("Hordes", time);
                            return;
                        }
                    }
                    string[] timeSplit2 = times[0].Split(':');
                    int.TryParse(timeSplit2[0], out int hours2);
                    int.TryParse(timeSplit2[1], out int minutes2);
                    time = DateTime.Today.AddDays(1).AddHours(hours2).AddMinutes(minutes2);
                    EventSchedule.AddToSchedule("Hordes", time);
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
                        EventSchedule.AddToSchedule("Hordes", time);
                    }
                    else
                    {
                        time = DateTime.Today.AddDays(1).AddHours(hours2).AddMinutes(minutes2);
                        EventSchedule.AddToSchedule("Hordes", time);
                    }
                    return;
                }
                else
                {
                    if (int.TryParse(Delay, out int delay))
                    {
                        time = DateTime.Now.AddMinutes(delay);
                        EventSchedule.AddToSchedule("Hordes", time);
                    }
                    else
                    {
                        Log.Out("[SERVERTOOLS] Invalid Hordes Delay detected. Use a single integer, 24h time or multiple 24h time entries");
                        Log.Out("[SERVERTOOLS] Example: 120 or 03:00 or 03:00, 06:00, 09:00");
                    }
                    return;
                }
            }
        }

        public static void Exec()
        {
            if (!GameManager.Instance.World.aiDirector.BloodMoonComponent.BloodMoonActive)
            {
                Dictionary<int, EntityPlayer> players = GeneralOperations.GetEntityPlayers();
                if (players != null && players.Count >= Players)
                {
                    int counter = 0;
                    List<Entity> Entities = GameManager.Instance.World.Entities.list;
                    Entity entity;
                    for (int i = 0; i < Entities.Count; i++)
                    {
                        entity = Entities[i];
                        if (entity != null && entity is EntityZombie)
                        {
                            counter++;
                        }
                    }
                    if (counter < 30)
                    {
                        AIDirectorChunkEventComponent aiDirector = GameManager.Instance.World.aiDirector.GetComponent<AIDirectorChunkEventComponent>();
                        if (aiDirector != null)
                        {
                            AIScoutHordeSpawner.IHorde horde = aiDirector.CreateHorde(players[new System.Random().Next(0, players.Count + 1)].position);
                            if (horde != null)
                            {
                                horde.SpawnMore(new System.Random().Next(4, Zombie_Count));
                                //GameManager.Instance.World.aiDirector.GetComponent<AIDirectorWanderingHordeComponent>().SpawnWanderingHorde(false);
                                Phrases.Dict.TryGetValue("Hordes1", out string phrase);
                                ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                            }
                        }
                    }
                }
            }
        }
    }
}
