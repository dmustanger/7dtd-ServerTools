using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class EntityCleanup
    {
        public static bool IsEnabled = false, FallingTreeEnabled = false, Underground = false, Bicycles = false, MiniBikes = false, 
            MotorBikes = false, Jeeps = false, Gyros = false, Drones = false;
        private static List<int> Tree = new List<int>();

        public static void EntityCheck()
        {
            try
            {
                if (GameManager.Instance.World.Entities.Count < 0)
                {
                    return;
                }
                List<Entity> entityList = GameManager.Instance.World.Entities.list;
                if (entityList == null)
                {
                    return;
                }
                for (int i = 0; i < entityList.Count; i++)
                {
                    Entity entity = entityList[i];
                    if (entity == null || !entity.IsSpawned())
                    {
                        continue;
                    }
                    Vector3 pos = entity.position;
                    if (FallingTreeEnabled && entity is EntityFallingTree)
                    {
                        if (Tree.Contains(entity.entityId))
                        {
                            Tree.Remove(entity.entityId);
                            entity.MarkToUnload();
                            Log.Out(string.Format("[SERVERTOOLS] Entity cleanup removed a falling tree @ '{0}'", pos));
                            continue;
                        }
                        else
                        {
                            Tree.Add(entity.entityId);
                            continue;
                        }
                    }
                    else if (Underground && (int)pos.y < 0 && !entity.IsMarkedForUnload())
                    {
                        if (entity is EntityZombie || entity is EntityAnimal)
                        {
                            entity.MarkToUnload();
                            Log.Out(string.Format("[SERVERTOOLS] Entity cleanup removed '{0}' with entity id '{1}' @ '{2}'", entity.name, entity.entityId, pos));
                            continue;
                        }
                        else if (entity is EntityVehicle)
                        {
                            entity.MarkToUnload();
                            Log.Out(string.Format("[SERVERTOOLS] Entity cleanup removed '{0}' with entity id '{1}' belonging to '{2}' @ '{3}'", entity.name, entity.entityId, entity.belongsPlayerId, pos));
                            continue;
                        }
                        else if (entity is EntityItem)
                        {
                            entity.MarkToUnload();
                            Log.Out(string.Format("[SERVERTOOLS] Entity cleanup removed '{0}' with entity id '{1}' belonging to '{2}' @ '{3}'", entity.name, entity.entityId, entity.belongsPlayerId, pos));
                            continue;
                        }
                    }
                    else if (Bicycles && entity is EntityBicycle)
                    {
                        entity.MarkToUnload();
                        EntityPlayer douche = GameManager.Instance.World.GetClosestPlayer((int)pos.x, (int)pos.y, (int)pos.z, -1, 10);
                        if (douche == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Entity cleanup removed bicycle id '{0}' belonging to '{1}'", entity.entityId, entity.belongsPlayerId));
                        }
                        else
                        {
                            ClientInfo cInfo = GeneralOperations.GetClientInfoFromEntityId(douche.entityId);
                            if (cInfo != null)
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Entity cleanup removed bicycle id '{0}' belonging to '{1}'. Closest player id '{2}' named '{3}'", entity.entityId, entity.belongsPlayerId, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                            }
                            else
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Entity cleanup removed bicycle id '{0}' belonging to '{1}'", entity.entityId, entity.belongsPlayerId));
                            }
                        }
                        continue;
                    }
                    else if (Drones && entity is EntityDrone)
                    {
                        entity.MarkToUnload();
                        EntityPlayer douche = GameManager.Instance.World.GetClosestPlayer((int)pos.x, (int)pos.y, (int)pos.z, -1, 10);
                        if (douche == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Entity cleanup removed drone id '{0}' belonging to '{1}'", entity.entityId, entity.belongsPlayerId));
                        }
                        else
                        {
                            ClientInfo cInfo = ConnectionManager.Instance.Clients.ForEntityId(douche.entityId);
                            if (cInfo != null)
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Entity cleanup removed drone id '{0}' belonging to '{1}'. Closest player id '{2}' named '{3}'", entity.entityId, entity.belongsPlayerId, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                            }
                            else
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Entity cleanup removed drone id '{0}' belonging to '{1}'", entity.entityId, entity.belongsPlayerId));
                            }
                        }
                        continue;
                    }
                    else if (MiniBikes && entity is EntityMinibike)
                    {
                        entity.MarkToUnload();
                        EntityPlayer douche = GameManager.Instance.World.GetClosestPlayer((int)pos.x, (int)pos.y, (int)pos.z, -1, 10);
                        if (douche == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Entity cleanup removed minibike id '{0}' belonging to '{1}'", entity.entityId, entity.belongsPlayerId));
                        }
                        else
                        {
                            ClientInfo cInfo = ConnectionManager.Instance.Clients.ForEntityId(douche.entityId);
                            if (cInfo != null)
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Entity cleanup removed minibike id '{0}' belonging to '{1}'. Closest player id '{2}' named '{3}'", entity.entityId, entity.belongsPlayerId, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                            }
                            else
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Entity cleanup removed minibike id '{0}' belonging to '{1}'", entity.entityId, entity.belongsPlayerId));
                            }
                        }
                        continue;
                    }
                    else if (MotorBikes && entity is EntityMotorcycle)
                    {
                        entity.MarkToUnload();
                        EntityPlayer douche = GameManager.Instance.World.GetClosestPlayer((int)pos.x, (int)pos.y, (int)pos.z, -1, 10);
                        if (douche == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Entity cleanup removed motorcycle id '{0}' belonging to '{1}'", entity.entityId, entity.belongsPlayerId));
                        }
                        else
                        {
                            ClientInfo cInfo = ConnectionManager.Instance.Clients.ForEntityId(douche.entityId);
                            if (cInfo != null)
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Entity cleanup removed motorcycle id '{0}' belonging to '{1}'. Closest player id '{2}' named '{3}'", entity.entityId, entity.belongsPlayerId, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                            }
                            else
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Entity cleanup removed motorcycle id '{0}' belonging to '{1}'", entity.entityId, entity.belongsPlayerId));
                            }
                        }
                        continue;
                    }
                    else if (Jeeps && entity is EntityVJeep)
                    {
                        entity.MarkToUnload();
                        EntityPlayer douche = GameManager.Instance.World.GetClosestPlayer((int)pos.x, (int)pos.y, (int)pos.z, -1, 10);
                        if (douche == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Entity cleanup removed jeep id '{0}' belonging to '{1}'", entity.entityId, entity.belongsPlayerId));
                        }
                        else
                        {
                            ClientInfo cInfo = ConnectionManager.Instance.Clients.ForEntityId(douche.entityId);
                            if (cInfo != null)
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Entity cleanup removed jeep id '{0}' belonging to '{1}'. Closest player id '{2}' named '{3}'", entity.entityId, entity.belongsPlayerId, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                            }
                            else
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Entity cleanup removed jeep id '{0}' belonging to '{1}'", entity.entityId, entity.belongsPlayerId));
                            }
                        }
                        continue;
                    }
                    else if (Gyros && entity is EntityVGyroCopter)
                    {
                        entity.MarkToUnload();
                        EntityPlayer douche = GameManager.Instance.World.GetClosestPlayer((int)pos.x, (int)pos.y, (int)pos.z, -1, 10);
                        if (douche == null)
                        {
                            Log.Out(string.Format("[SERVERTOOLS] Entity cleanup removed gyrocopter id '{0}' belonging to '{1}'", entity.entityId, entity.belongsPlayerId));
                        }
                        else
                        {
                            ClientInfo cInfo = ConnectionManager.Instance.Clients.ForEntityId(douche.entityId);
                            if (cInfo != null)
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Entity cleanup removed gyrocopter id '{0}' belonging to '{1}'. Closest player id '{2}' named '{3}'", entity.entityId, entity.belongsPlayerId, cInfo.CrossplatformId.CombinedString, cInfo.playerName));
                            }
                            else
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Entity cleanup removed gyrocopter id '{0}' belonging to '{1}'", entity.entityId, entity.belongsPlayerId));
                            }
                        }
                        continue;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in EntityCleanup.EntityCheck: {0}", e.Message));
            }
        }
    }
}
