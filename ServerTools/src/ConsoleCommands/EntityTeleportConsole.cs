using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class TeleportEntityConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]-Teleports an entity in the game";
        }

        public override string GetHelp()
        {
            return "Teleports an entity. Entity can not be a player.\n" +
                "Usage: EntityTeleport <EntityId> <EntityId>\n" +
                "Usage: EntityTeleport <EntityId> <X> <Y> <Z>\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-EntityTeleport", "EntityTeleport", "entityteleport", "et" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 2 && _params.Count != 4)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, 4, found {0}", _params.Count));
                    return;
                }
                if (_params.Count == 2)
                {
                    int _entityId1;
                    if (!int.TryParse(_params[0], out _entityId1))
                    {
                        SdtdConsole.Instance.Output(string.Format("Invalid entity id value: {0}", _entityId1));
                        return;
                    }
                    int _entityId2;
                    if (!int.TryParse(_params[1], out _entityId2))
                    {
                        SdtdConsole.Instance.Output(string.Format("Invalid entity id value: {0}", _entityId2));
                        return;
                    }
                    Entity Entity1 = GameManager.Instance.World.Entities.dict[_entityId1];
                    if (Entity1 == null)
                    {
                        SdtdConsole.Instance.Output(string.Format("Entity not found: {0}", _entityId1));
                        return;
                    }
                    Entity Entity2 = GameManager.Instance.World.Entities.dict[_entityId2];
                    if (Entity2 == null)
                    {
                        SdtdConsole.Instance.Output(string.Format("Entity not found: {0}", _entityId2));
                        return;
                    }
                    if (Entity1.IsClientControlled())
                    {
                        SdtdConsole.Instance.Output(string.Format("Entity with id {0} can not be a player. Use a different command for teleporting players", _entityId1));
                        return;
                    }
                    Entity1.SetPosition(Entity2.position);
                    SdtdConsole.Instance.Output(string.Format("Teleported entity {0} to entity {1} located at {2} {3} {4}", _entityId1, _entityId2, (int)Entity2.position.x, (int)Entity2.position.y, (int)Entity2.position.z));
                }
                if (_params.Count == 4)
                {
                    int _entityId;
                    if (!int.TryParse(_params[0], out _entityId))
                    {
                        SdtdConsole.Instance.Output(string.Format("Invalid entityId value: {0}", _entityId));
                        return;
                    }
                    Entity Entity = GameManager.Instance.World.Entities.dict[_entityId];
                    if (Entity == null)
                    {
                        SdtdConsole.Instance.Output(string.Format("Invalid entityId. Entity not found: {0}", _entityId));
                        return;
                    }
                    if (Entity.IsClientControlled())
                    {
                        SdtdConsole.Instance.Output(string.Format("Entity with id {0} can not be a player. Use a different command for teleporting players", _entityId));
                        return;
                    }
                    int _x, _y, _z;
                    if (!int.TryParse(_params[1], out _x))
                    {
                        SdtdConsole.Instance.Output(string.Format("Invalid entityId value: {0}", _x));
                        return;
                    }
                    if (!int.TryParse(_params[2], out _y))
                    {
                        SdtdConsole.Instance.Output(string.Format("Invalid entityId value: {0}", _y));
                        return;
                    }
                    if (!int.TryParse(_params[3], out _z))
                    {
                        SdtdConsole.Instance.Output(string.Format("Invalid entityId value: {0}", _z));
                        return;
                    }
                    Entity.SetPosition(new UnityEngine.Vector3(_x, _y, _z));
                    SdtdConsole.Instance.Output(string.Format("Teleported entity {0} to {1} {2} {3}", _entityId, _x, _y, _z));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in EntityTeleportConsole.Execute: {0}", e));
            }
        }
    }
}
