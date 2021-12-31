using System.Collections.Generic;

namespace ServerTools
{
    class Hordes
    {
        public static bool IsEnabled = false;

        public static void Exec()
        {
            if (!GameManager.Instance.World.aiDirector.BloodMoonComponent.BloodMoonActive)
            {
                int playerCount = ConnectionManager.Instance.ClientCount();
                if (playerCount > 5)
                {
                    int counter = 0;
                    List<Entity> Entities = GameManager.Instance.World.Entities.list;
                    for (int i = 0; i < Entities.Count; i++)
                    {
                        Entity entity = Entities[i];
                        if (entity != null)
                        {
                            if (!entity.IsClientControlled())
                            {
                                EntityType type = entity.entityType;
                                if (type == EntityType.Zombie)
                                {
                                    counter++;
                                }
                            }
                        }
                    }
                    if (counter < 30)
                    {
                        GameManager.Instance.World.aiDirector.GetComponent<AIDirectorWanderingHordeComponent>().SpawnWanderingHorde(false);
                        Phrases.Dict.TryGetValue("Hordes1", out string phrase);
                        ChatHook.ChatMessage(null, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
                    }
                }
            }
        }
    }
}
