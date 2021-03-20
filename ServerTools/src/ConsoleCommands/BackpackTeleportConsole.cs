using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class TeleportBackpackConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Teleports a backpack in the game";
        }

        public override string GetHelp()
        {
            return "Teleports a backpack. Backpack can not be attached to a player.\n" +
                "Usage: st-bt <BackpackId> <EntityId>\n" +
                "Usage: st-bt <BackpackId> <X> <Y> <Z>\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-BackpackTeleport", "bt", "st-bt" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 2 && _params.Count != 4)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, 4, found {0}", _params.Count));
                    return;
                }
                if (_params.Count == 2)
                {
                    int _entityId1;
                    if (!int.TryParse(_params[0], out _entityId1))
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid backpack id value: {0}", _entityId1));
                        return;
                    }
                    int _entityId2;
                    if (!int.TryParse(_params[1], out _entityId2))
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid backpack id value: {0}", _entityId2));
                        return;
                    }
                    Entity Entity1 = GameManager.Instance.World.Entities.dict[_entityId1];
                    if (Entity1 == null)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Backpack not found: {0}", _entityId1));
                        return;
                    }
                    if (Entity1.IsClientControlled())
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Backpack with id {0} can not be a player. Use a different command for teleporting players", _entityId1));
                        return;
                    }
                    Entity Entity2 = GameManager.Instance.World.Players.dict[_entityId2];
                    if (Entity2 == null)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Entity not found: {0}", _entityId2));
                        return;
                    }
                    string _name = EntityClass.list[Entity1.entityClass].entityClassName;
                    if (_name.ToLower() == "backpack")
                    {
                        if (!Entity1.lootContainer.IsEmpty())
                        {
                            ItemStack[] _items = Entity1.lootContainer.GetItems();
                            if (Entity1.belongsPlayerId != 0)
                            {
                                GameManager.Instance.World.RemoveEntity(Entity1.entityId, EnumRemoveEntityReason.Despawned);
                                foreach (KeyValuePair<int, EntityClass> i in EntityClass.list.Dict)
                                {
                                    if (i.Value == Entity1.EntityClass)
                                    {
                                        Entity entity = EntityFactory.CreateEntity(i.Key, new UnityEngine.Vector3(Entity2.position.x, Entity2.position.y, Entity2.position.z));
                                        entity.belongsPlayerId = Entity1.belongsPlayerId;
                                        entity.lootContainer.items = _items;
                                        GameManager.Instance.World.SpawnEntityInWorld(entity);
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Spawned a new backpack to {0} {1} {2} with the old backpack loot", Entity2.position.x, Entity2.position.y, Entity2.position.z));
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                if (_params.Count == 4)
                {
                    int _entityId;
                    if (!int.TryParse(_params[0], out _entityId))
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid backpack id value: {0}", _entityId));
                        return;
                    }
                    Entity Entity = GameManager.Instance.World.Entities.dict[_entityId];
                    if (Entity == null)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid backpack. Backpack not found: {0}", _entityId));
                        return;
                    }
                    if (Entity.IsClientControlled())
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Backpack with id {0} can not be a player. Use a different command for teleporting players", _entityId));
                        return;
                    }
                    float _x, _y, _z;
                    if (!float.TryParse(_params[1], out _x))
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid backpack value: {0}", _x));
                        return;
                    }
                    if (!float.TryParse(_params[2], out _y))
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid backpack value: {0}", _y));
                        return;
                    }
                    if (!float.TryParse(_params[3], out _z))
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid backpack value: {0}", _z));
                        return;
                    }
                    string _name = EntityClass.list[Entity.entityClass].entityClassName;
                    if (_name.ToLower() == "backpack")
                    {
                        if (!Entity.lootContainer.IsEmpty())
                        {
                            ItemStack[] _items = Entity.lootContainer.GetItems();
                            if (Entity.belongsPlayerId != 0)
                            {
                                GameManager.Instance.World.RemoveEntity(Entity.entityId, EnumRemoveEntityReason.Despawned);
                                foreach (KeyValuePair<int, EntityClass> i in EntityClass.list.Dict)
                                {
                                    if (i.Value == Entity.EntityClass)
                                    {
                                        Entity entity = EntityFactory.CreateEntity(i.Key, new UnityEngine.Vector3(_x, _y, _z));
                                        entity.belongsPlayerId = Entity.belongsPlayerId;
                                        entity.lootContainer.items = _items;
                                        GameManager.Instance.World.SpawnEntityInWorld(entity);
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Spawned a new backpack to {0} {1} {2} with the old backpack loot", _x, _y, _z));
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in TeleportBackpackConsole.Execute: {0}", e.Message));
            }
        }
    }
}
