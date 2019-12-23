using System;
using System.Collections.Generic;

namespace ServerTools
{
    class KillEntConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Kills All Of The Zombies And Animals On The Map.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. KillEnt\n" +
                "1. Kills all of the zombies and animals on the map\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-KillEnt", "killent", "ke" };
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
                SdtdConsole.Instance.Output(string.Format("All current zombies and animals have been removed from the map"));
                Log.Out(string.Format("[SERVERTOOLS] All current zombies and animals have been removed from the map."));
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in KillAll.Run: {0}.", e));
            }
        }
    }
}
