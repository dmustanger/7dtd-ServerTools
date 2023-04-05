using System.Collections.Generic;

namespace ServerTools
{
    class Hordes
    {
        public static bool IsEnabled = false;
        public static int Players = 5, Zombies = 30;

        public static void Exec()
        {
            if (!GameManager.Instance.World.aiDirector.BloodMoonComponent.BloodMoonActive)
            {
                Dictionary<int, EntityPlayer> players = GeneralOperations.GetEntityPlayers();
                if (players != null && players.Count >= Players)
                {
                    int counter = 0;
                    List<Entity> Entities = GameManager.Instance.World.Entities.list;
                    for (int i = 0; i < Entities.Count; i++)
                    {
                        Entity entity = Entities[i];
                        if (entity != null && entity is EntityZombie)
                        {
                            counter++;
                        }
                    }
                    if (counter < Zombies)
                    {
                        AIDirectorChunkEventComponent aiDirector = GameManager.Instance.World.aiDirector.GetComponent<AIDirectorChunkEventComponent>();
                        if (aiDirector != null)
                        {
                            AIScoutHordeSpawner.IHorde horde = aiDirector.CreateHorde(players[new System.Random().Next(0, players.Count + 1)].position);
                            if (horde != null)
                            {
                                horde.SpawnMore(new System.Random().Next(4, 13));
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
