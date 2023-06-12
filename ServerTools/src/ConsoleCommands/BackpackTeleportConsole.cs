using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class TeleportBackpackConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Teleports a backpack in the game";
        }

        protected override string getHelp()
        {
            return "Teleports a backpack. Backpack can not be attached to a player.\n" +
                "Usage: st-bt <BackpackId> <EntityId>\n" +
                "Usage: st-bt <BackpackId> <X> <Y> <Z>\n";
        }

        protected override string[] getCommands()
        {
            return new string[] { "st-BackpackTeleport", "bt", "st-bt" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 2 && _params.Count != 4)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, 4, found {0}", _params.Count));
                    return;
                }
                if (_params.Count == 2)
                {
                    if (!int.TryParse(_params[0], out int _entityId1))
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid backpack id value: {0}", _entityId1));
                        return;
                    }
                    if (!int.TryParse(_params[1], out int _entityId2))
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid backpack id value: {0}", _entityId2));
                        return;
                    }
                    Entity Entity1 = GameManager.Instance.World.Entities.dict[_entityId1];
                    if (Entity1 == null)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Backpack not found: {0}", _entityId1));
                        return;
                    }
                    if (Entity1.IsClientControlled())
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Backpack with id {0} can not be a player. Use a different command for teleporting players", _entityId1));
                        return;
                    }
                    Entity Entity2 = GameManager.Instance.World.Players.dict[_entityId2];
                    if (Entity2 == null)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Entity not found: {0}", _entityId2));
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
                                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Spawned a new backpack to {0} {1} {2} with the old backpack loot", Entity2.position.x, Entity2.position.y, Entity2.position.z));
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                if (_params.Count == 4)
                {
                    if (!int.TryParse(_params[0], out int _entityId))
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid backpack id value: {0}", _entityId));
                        return;
                    }
                    Entity Entity = GeneralOperations.GetEntity(_entityId);
                    if (Entity == null)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid backpack. Backpack not found: {0}", _entityId));
                        return;
                    }
                    if (Entity.IsClientControlled())
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Backpack with id {0} can not be a player. Use a different command for teleporting players", _entityId));
                        return;
                    }
                    if (!float.TryParse(_params[1], out float _x))
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid backpack value: {0}", _x));
                        return;
                    }
                    if (!float.TryParse(_params[2], out float _y))
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid backpack value: {0}", _y));
                        return;
                    }
                    if (!float.TryParse(_params[3], out float _z))
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid backpack value: {0}", _z));
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
                                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Spawned a new backpack to {0} {1} {2} with the old backpack loot", _x, _y, _z));
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
