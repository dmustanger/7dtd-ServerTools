using System;
using System.Collections.Generic;

namespace ServerTools
{
    class RemoveEntConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Remove all zombies and animals on the map.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. st-rza\n" +
                "1. Removes all current zombies and animals on the map\n";
        }

        public override string[] GetCommands()
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
                    Entity _entity = Entities[i];
                    if (_entity != null)
                    {
                        if (!_entity.IsClientControlled() && !_entity.IsDead())
                        {
                            EntityType _type = _entity.entityType;
                            if (_type == EntityType.Animal || _type == EntityType.Zombie)
                            {
                                GameManager.Instance.World.RemoveEntity(_entity.entityId, EnumRemoveEntityReason.Despawned);
                            }
                        }
                    }
                }
                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] All current zombies and animals have been removed from the map"));
                Log.Out(string.Format("[SERVERTOOLS] Console command KillEnt has been run. All current zombies and animals have been removed from the map."));
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in KillEntConsole.Execute: {0}", e.Message));
            }
        }
    }
}
