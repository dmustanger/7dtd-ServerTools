using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class VehicleListConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Lists or removes the vehicles in the world.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. st-vl\n" +
                "  2. st-vl remove\n" +
                "1. Shows a list of each vehicle, entity id, owner id, owner name, location and the player using the vehicle if applicable\n" +
                "2. Remove all vehicles shown on the list\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-VehicleList", "vl", "st-vl" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count == 0)
                {
                    Entity entity;
                    EntityVehicle entityVehicle;
                    Entity attachedPlayer;
                    List<Entity> Entities = GameManager.Instance.World.Entities.list;
                    SdtdConsole.Instance.Output("Vehicle List:");
                    for (int i = 0; i < Entities.Count; i++)
                    {
                        entity = Entities[i];
                        if (entity != null && entity is EntityVehicle)
                        {
                            entityVehicle = (EntityVehicle)entity;
                            if (entityVehicle != null)
                            {
                                string playerName = "";
                                PersistentPlayerData ppd = GeneralOperations.GetPersistentPlayerDataFromEntityId(entityVehicle.belongsPlayerId);
                                if (ppd != null)
                                {
                                    playerName = ppd.PlayerName;
                                }
                                Vector3 pos = entity.position;
                                int x = (int)pos.x;
                                int y = (int)pos.y;
                                int z = (int)pos.z;
                                attachedPlayer = entity.AttachedMainEntity;
                                if (attachedPlayer != null)
                                {
                                    ClientInfo cInfo = GeneralOperations.GetClientInfoFromEntityId(attachedPlayer.entityId);
                                    if (cInfo != null)
                                    {
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] '{0}' Id '{1}' Owner Id '{2}' Owner Name '{3}', located at 'x {4}  y {5} z {6}', player '{7}' is in this vehicle.", entityVehicle.EntityClass.entityClassName, entityVehicle.entityId, entityVehicle.belongsPlayerId, playerName, x, y, z, cInfo.playerName));
                                    }
                                    else
                                    {
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] '{0}' Id '{1}' Owner Id '{2}' Owner Name '{3}', located at 'x {4}  y {5} z {6}'", entityVehicle.EntityClass.entityClassName, entityVehicle.entityId, entityVehicle.belongsPlayerId, playerName, x, y, z));
                                    }
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] '{0}' Id '{1}' Owner Id '{2}' Owner Name '{3}', located at 'x {4}  y {5} z {6}'", entityVehicle.EntityClass.entityClassName, entityVehicle.entityId, entityVehicle.belongsPlayerId, playerName, x, y, z));
                                }
                            }
                        }
                    }
                }
                else if (_params.Count == 1)
                {
                    if (_params[0].ToLower() == "remove")
                    {
                        List<Entity> entities = GameManager.Instance.World.Entities.list;
                        for (int i = 0; i < entities.Count; i++)
                        {
                            if (entities[i] is EntityVehicle)
                            {
                                GameManager.Instance.World.RemoveEntity(entities[i].entityId, EnumRemoveEntityReason.Despawned);
                                SdtdConsole.Instance.Output("[SERVERTOOLS] Removed vehicle id '{0}' located at '{1}'", entities[i].entityId, entities[i].position);
                                SdtdConsole.Instance.Output("[SERVERTOOLS] *Note* Unloaded chunks may contain vehicles that have not been removed");
                            }
                        }
                        return;
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output("[SERVERTOOLS] Invalid arguments");
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in VehicleListConsole.Execute: {0}", e.Message));
            }
        }
    }
}
