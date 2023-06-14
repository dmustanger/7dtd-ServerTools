using System;
using System.Collections.Generic;

namespace ServerTools
{
    class RemoveZombieAnimalConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Remove all zombies and animals on the map.";
        }

        protected override string getHelp()
        {
            return "Usage:\n" +
                "  1. st-rza\n" +
                "1. Removes all current zombies and animals on the map\n";
        }

        protected override string[] getCommands()
        {
            return new string[] { "st-RemoveZombieAnimals", "rza", "st-rza" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                List<Entity> Entities = GameManager.Instance.World.Entities.list;
                for (int i = 0; i < Entities.Count; i++)
                {
                    Entity entity = Entities[i];
                    if (entity != null)
                    {
                        if (!entity.IsClientControlled() && !entity.IsDead())
                        {
                            if (entity.entityType == EntityType.Animal || entity.entityType == EntityType.Zombie)
                            {
                                GameManager.Instance.World.RemoveEntity(entity.entityId, EnumRemoveEntityReason.Despawned);
                            }
                        }
                    }
                }
                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] All current zombies and animals have been removed from the map"));
                Log.Out(string.Format("[SERVERTOOLS] Console command st-RemoveZombieAnimals has been run. All current zombies and animals have been removed from the map."));
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in RemoveZombieAnimalConsole.Execute: {0}", e.Message));
            }
        }
    }
}
