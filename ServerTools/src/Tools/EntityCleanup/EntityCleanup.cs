using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class EntityCleanup
    {
        public static bool IsEnabled = false, BlockIsEnabled = false, FallingTreeEnabled = false, Underground = false, Bicycles = false, MiniBikes = false, 
            MotorBikes = false, Jeeps = false, Gyros = false;
        private static List<int> Tree = new List<int>();

        public static void EntityCheck()
        {
            try
            {
                if (GameManager.Instance.World != null && GameManager.Instance.World.Entities.Count > 0)
                {
                    List<Entity> Entities = GameManager.Instance.World.Entities.list;
                    if (Entities != null)
                    {
                        for (int i = 0; i < Entities.Count; i++)
                        {
                            Entity _entity = Entities[i];
                            if (_entity != null)
                            {
                                if (!_entity.IsClientControlled() && _entity.IsSpawned())
                                {

                                    string _name = EntityClass.list[_entity.entityClass].entityClassName;
                                    if (BlockIsEnabled && _name == "fallingBlock")
                                    {
                                        GameManager.Instance.World.RemoveEntity(_entity.entityId, EnumRemoveEntityReason.Despawned);
                                        Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed falling block id {0}", _entity.entityId));
                                        continue;
                                    }
                                    else if (FallingTreeEnabled && _name == "fallingTree")
                                    {
                                        if (Tree.Contains(_entity.entityId))
                                        {
                                            Tree.Remove(_entity.entityId);
                                            GameManager.Instance.World.RemoveEntity(_entity.entityId, EnumRemoveEntityReason.Despawned);
                                            Log.Out("[SERVERTOOLS] Entity cleanup: Removed falling tree");
                                            continue;
                                        }
                                        else
                                        {
                                            Tree.Add(_entity.entityId);
                                            continue;
                                        }
                                    }
                                    else if (Underground)
                                    {
                                        string _tags = _entity.EntityClass.Tags.ToString();
                                        if (_tags.Contains("zombie") || _tags.Contains("animal"))
                                        {
                                            if ((int)_entity.position.y <= -10)
                                            {
                                                GameManager.Instance.World.RemoveEntity(_entity.entityId, EnumRemoveEntityReason.Despawned);
                                                Log.Out("[SERVERTOOLS] Entity cleanup: Removed {0} with entity id {1}", _name, _entity.entityId);
                                                continue;
                                            }
                                        }
                                    }
                                    else if (Bicycles && _name == "vehicleBicycle")
                                    {
                                        Vector3 _vec = _entity.position;
                                        GameManager.Instance.World.RemoveEntity(_entity.entityId, EnumRemoveEntityReason.Despawned);
                                        EntityPlayer _douche = GameManager.Instance.World.GetClosestPlayer((int)_vec.x, (int)_vec.y, (int)_vec.z, -1, 10);
                                        if (_douche == null)
                                        {
                                            Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed bicycle id {0}", _entity.entityId));
                                        }
                                        else
                                        {
                                            ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForEntityId(_douche.entityId);
                                            if (_cInfo != null)
                                            {
                                                Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed bicycle id {0}. Closest player is {1}", _entity.entityId, _cInfo.playerName));
                                            }
                                            else
                                            {
                                                Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed bicycle id {0}", _entity.entityId));
                                            }
                                        }
                                        continue;
                                    }
                                    else if (MiniBikes && _name == "vehicleMinibike")
                                    {
                                        Vector3 _vec = _entity.position;
                                        GameManager.Instance.World.RemoveEntity(_entity.entityId, EnumRemoveEntityReason.Despawned);
                                        EntityPlayer _douche = GameManager.Instance.World.GetClosestPlayer((int)_vec.x, (int)_vec.y, (int)_vec.z, -1, 10);
                                        if (_douche == null)
                                        {
                                            Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed minibike id {0}", _entity.entityId));
                                        }
                                        else
                                        {
                                            ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForEntityId(_douche.entityId);
                                            if (_cInfo != null)
                                            {
                                                Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed minibike id {0}. Closest player is {1}", _entity.entityId, _cInfo.playerName));
                                            }
                                            else
                                            {
                                                Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed minibike id {0}", _entity.entityId));
                                            }
                                        }
                                        continue;
                                    }
                                    else if (MotorBikes && _name == "vehicleMotorcycle")
                                    {
                                        Vector3 _vec = _entity.position;
                                        GameManager.Instance.World.RemoveEntity(_entity.entityId, EnumRemoveEntityReason.Despawned);
                                        EntityPlayer _douche = GameManager.Instance.World.GetClosestPlayer((int)_vec.x, (int)_vec.y, (int)_vec.z, -1, 10);
                                        if (_douche == null)
                                        {
                                            Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed motorcycle id {0}", _entity.entityId));
                                        }
                                        else
                                        {
                                            ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForEntityId(_douche.entityId);
                                            if (_cInfo != null)
                                            {
                                                Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed motorcycle id {0}. Closest player is {1}", _entity.entityId, _cInfo.playerName));
                                            }
                                            else
                                            {
                                                Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed motorcycle id {0}", _entity.entityId));
                                            }
                                        }
                                        continue;
                                    }
                                    else if (Jeeps && _name == "vehicle4x4Truck")
                                    {
                                        Vector3 _vec = _entity.position;
                                        GameManager.Instance.World.RemoveEntity(_entity.entityId, EnumRemoveEntityReason.Despawned);
                                        EntityPlayer _douche = GameManager.Instance.World.GetClosestPlayer((int)_vec.x, (int)_vec.y, (int)_vec.z, -1, 10);
                                        if (_douche == null)
                                        {
                                            Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed jeep id {0}", _entity.entityId));
                                        }
                                        else
                                        {
                                            ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForEntityId(_douche.entityId);
                                            if (_cInfo != null)
                                            {
                                                Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed jeep id {0}. Closest player is {1}", _entity.entityId, _cInfo.playerName));
                                            }
                                            else
                                            {
                                                Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed jeep id {0}", _entity.entityId));
                                            }
                                        }
                                        continue;
                                    }
                                    else if (Gyros && _name == "vehicleGyrocopter")
                                    {
                                        Vector3 _vec = _entity.position;
                                        GameManager.Instance.World.RemoveEntity(_entity.entityId, EnumRemoveEntityReason.Despawned);
                                        EntityPlayer _douche = GameManager.Instance.World.GetClosestPlayer((int)_vec.x, (int)_vec.y, (int)_vec.z, -1, 10);
                                        if (_douche == null)
                                        {
                                            Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed gyrocopter id {0}", _entity.entityId));
                                        }
                                        else
                                        {
                                            ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForEntityId(_douche.entityId);
                                            if (_cInfo != null)
                                            {
                                                Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed gyrocopter id {0}. Closest player is {1}", _entity.entityId, _cInfo.playerName));
                                            }
                                            else
                                            {
                                                Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed gyrocopter id {0}", _entity.entityId));
                                            }
                                        }
                                        continue;
                                    }
                                }
                            }
                        }
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
