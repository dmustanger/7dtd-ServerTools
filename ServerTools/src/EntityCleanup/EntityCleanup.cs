using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class EntityCleanup
    {
        public static bool IsEnabled = false, BlockIsEnabled = false, FallingTreeEnabled = false, Underground = false, Bikes = false;
        private static List<Entity> Entities = new List<Entity>();
        private static int _xMinCheck, _yMinCheck, _zMinCheck, _xMaxCheck, _yMaxCheck, _zMaxCheck;

        public static void EntityCheck()
        {
            try
            {
                Entities = GameManager.Instance.World.Entities.list;
                for (int i = 0; i < Entities.Count; i++)
                {
                    Entity _entity = Entities[i];
                    if (_entity != null)
                    {
                        if (!_entity.IsClientControlled())
                        {
                            string _name = EntityClass.list[_entity.entityClass].entityClassName;
                            if (BlockIsEnabled && _name == "fallingBlock")
                            {
                                GameManager.Instance.World.RemoveEntity(_entity.entityId, EnumRemoveEntityReason.Despawned);
                                Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed falling block id {0}", _entity.entityId));
                            }
                            if (FallingTreeEnabled && _name == "fallingTree")
                            {
                                GameManager.Instance.World.RemoveEntity(_entity.entityId, EnumRemoveEntityReason.Despawned);
                                Log.Out("[SERVERTOOLS] Entity cleanup: Removed falling tree");
                            }
                            if (Underground)
                            {
                                int y = (int)_entity.position.y;
                                if (y <= -60)
                                {
                                    int x = (int)_entity.position.x;
                                    int z = (int)_entity.position.z;
                                    _entity.SetPosition(new Vector3(x, -1, z));
                                    Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Teleported entity id {0} to the surface @ {1} -1 {2}", _entity.entityId, x, z));
                                }
                            }
                            if (Bikes && _name == "vehicleMinibike")
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
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in EntityCleanup.EntityCheck: {0}.", e));
            }
        }

        public static void ZombieCheck()
        {
            try
            {
                if (Zones.Box1.Count > 0)
                {
                    for (int i = 0; i < Zones.Box1.Count; i++)
                    {
                        string[] _box1 = Zones.Box1[i];
                        bool[] _box2 = Zones.Box2[i];
                        if (_box2[2])
                        {
                            Entities = GameManager.Instance.World.Entities.list;
                            for (int j = 0; j < Entities.Count; j++)
                            {
                                Entity _entity = Entities[j];
                                if (_entity != null)
                                {
                                    if (!_entity.IsClientControlled() && !_entity.IsDead())
                                    {
                                        EntityType _type = _entity.entityType;
                                        if (_type == EntityType.Zombie)
                                        {
                                            Vector3 _vec = _entity.position;
                                            int _X = (int)_entity.position.x;
                                            int _Y = (int)_entity.position.y;
                                            int _Z = (int)_entity.position.z;
                                            int xMin, yMin, zMin;
                                            string[] _corner1 = _box1[0].Split(',');
                                            int.TryParse(_corner1[0], out xMin);
                                            int.TryParse(_corner1[1], out yMin);
                                            int.TryParse(_corner1[2], out zMin);
                                            if (!_box2[0])
                                            {
                                                int xMax, yMax, zMax;
                                                string[] _corner2 = _box1[1].Split(',');
                                                int.TryParse(_corner2[0], out xMax);
                                                int.TryParse(_corner2[1], out yMax);
                                                int.TryParse(_corner2[2], out zMax);
                                                if (xMin >= 0 & xMax >= 0)
                                                {
                                                    if (xMin < xMax)
                                                    {
                                                        if (_X >= xMin)
                                                        {
                                                            _xMinCheck = 1;
                                                        }
                                                        else
                                                        {
                                                            _xMinCheck = 0;
                                                        }
                                                        if (_X <= xMax)
                                                        {
                                                            _xMaxCheck = 1;
                                                        }
                                                        else
                                                        {
                                                            _xMaxCheck = 0;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (_X <= xMin)
                                                        {
                                                            _xMinCheck = 1;
                                                        }
                                                        else
                                                        {
                                                            _xMinCheck = 0;
                                                        }
                                                        if (_X >= xMax)
                                                        {
                                                            _xMaxCheck = 1;
                                                        }
                                                        else
                                                        {
                                                            _xMaxCheck = 0;
                                                        }
                                                    }
                                                }
                                                else if (xMin <= 0 & xMax <= 0)
                                                {
                                                    if (xMin < xMax)
                                                    {
                                                        if (_X >= xMin)
                                                        {
                                                            _xMinCheck = 1;
                                                        }
                                                        else
                                                        {
                                                            _xMinCheck = 0;
                                                        }
                                                        if (_X <= xMax)
                                                        {
                                                            _xMaxCheck = 1;
                                                        }
                                                        else
                                                        {
                                                            _xMaxCheck = 0;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (_X <= xMin)
                                                        {
                                                            _xMinCheck = 1;
                                                        }
                                                        else
                                                        {
                                                            _xMinCheck = 0;
                                                        }
                                                        if (_X >= xMax)
                                                        {
                                                            _xMaxCheck = 1;
                                                        }
                                                        else
                                                        {
                                                            _xMaxCheck = 0;
                                                        }
                                                    }
                                                }
                                                else if (xMin <= 0 & xMax >= 0)
                                                {
                                                    if (_X >= xMin)
                                                    {
                                                        _xMinCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _xMinCheck = 0;
                                                    }
                                                    if (_X <= xMax)
                                                    {
                                                        _xMaxCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _xMaxCheck = 0;
                                                    }
                                                }
                                                else if (xMin >= 0 & xMax <= 0)
                                                {
                                                    if (_X <= xMin)
                                                    {
                                                        _xMinCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _xMinCheck = 0;
                                                    }
                                                    if (_X >= xMax)
                                                    {
                                                        _xMaxCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _xMaxCheck = 0;
                                                    }
                                                }
                                                if (yMin >= 0 & yMax >= 0)
                                                {
                                                    if (yMin < yMax)
                                                    {
                                                        if (_Y >= yMin)
                                                        {
                                                            _yMinCheck = 1;
                                                        }
                                                        else
                                                        {
                                                            _yMinCheck = 0;
                                                        }
                                                        if (_Y <= yMax)
                                                        {
                                                            _yMaxCheck = 1;
                                                        }
                                                        else
                                                        {
                                                            _yMaxCheck = 0;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (_Y <= yMin)
                                                        {
                                                            _yMinCheck = 1;
                                                        }
                                                        else
                                                        {
                                                            _yMinCheck = 0;
                                                        }
                                                        if (_Y >= yMax)
                                                        {
                                                            _yMaxCheck = 1;
                                                        }
                                                        else
                                                        {
                                                            _yMaxCheck = 0;
                                                        }
                                                    }
                                                }
                                                else if (yMin <= 0 & yMax <= 0)
                                                {
                                                    if (yMin < yMax)
                                                    {
                                                        if (_Y >= yMin)
                                                        {
                                                            _yMinCheck = 1;
                                                        }
                                                        else
                                                        {
                                                            _yMinCheck = 0;
                                                        }
                                                        if (_Y <= yMax)
                                                        {
                                                            _yMaxCheck = 1;
                                                        }
                                                        else
                                                        {
                                                            _yMaxCheck = 0;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (_Y <= yMin)
                                                        {
                                                            _yMinCheck = 1;
                                                        }
                                                        else
                                                        {
                                                            _yMinCheck = 0;
                                                        }
                                                        if (_Y >= yMax)
                                                        {
                                                            _yMaxCheck = 1;
                                                        }
                                                        else
                                                        {
                                                            _yMaxCheck = 0;
                                                        }
                                                    }
                                                }
                                                else if (yMin <= 0 & yMax >= 0)
                                                {
                                                    if (_Y >= yMin)
                                                    {
                                                        _yMinCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _yMinCheck = 0;
                                                    }
                                                    if (_Y <= yMax)
                                                    {
                                                        _yMaxCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _yMaxCheck = 0;
                                                    }
                                                }
                                                else if (yMin >= 0 & yMax <= 0)
                                                {
                                                    if (_Y <= yMin)
                                                    {
                                                        _yMinCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _yMinCheck = 0;
                                                    }
                                                    if (_Y >= yMax)
                                                    {
                                                        _yMaxCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _yMaxCheck = 0;
                                                    }
                                                }
                                                if (zMin >= 0 & zMax >= 0)
                                                {
                                                    if (zMin < zMax)
                                                    {
                                                        if (_Z >= zMin)
                                                        {
                                                            _zMinCheck = 1;
                                                        }
                                                        else
                                                        {
                                                            _zMinCheck = 0;
                                                        }
                                                        if (_Z <= zMax)
                                                        {
                                                            _zMaxCheck = 1;
                                                        }
                                                        else
                                                        {
                                                            _zMaxCheck = 0;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (_Z <= zMin)
                                                        {
                                                            _zMinCheck = 1;
                                                        }
                                                        else
                                                        {
                                                            _zMinCheck = 0;
                                                        }
                                                        if (_Z >= zMax)
                                                        {
                                                            _zMaxCheck = 1;
                                                        }
                                                        else
                                                        {
                                                            _zMaxCheck = 0;
                                                        }
                                                    }
                                                }
                                                else if (zMin <= 0 & zMax <= 0)
                                                {
                                                    if (zMin < zMax)
                                                    {
                                                        if (_Z >= zMin)
                                                        {
                                                            _zMinCheck = 1;
                                                        }
                                                        else
                                                        {
                                                            _zMinCheck = 0;
                                                        }
                                                        if (_Z <= zMax)
                                                        {
                                                            _zMaxCheck = 1;
                                                        }
                                                        else
                                                        {
                                                            _zMaxCheck = 0;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (_Z <= zMin)
                                                        {
                                                            _zMinCheck = 1;
                                                        }
                                                        else
                                                        {
                                                            _zMinCheck = 0;
                                                        }
                                                        if (_Z >= zMax)
                                                        {
                                                            _zMaxCheck = 1;
                                                        }
                                                        else
                                                        {
                                                            _zMaxCheck = 0;
                                                        }
                                                    }
                                                }
                                                else if (zMin <= 0 & zMax >= 0)
                                                {
                                                    if (_Z >= zMin)
                                                    {
                                                        _zMinCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _zMinCheck = 0;
                                                    }
                                                    if (_Z <= zMax)
                                                    {
                                                        _zMaxCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _zMaxCheck = 0;
                                                    }
                                                }
                                                else if (zMin >= 0 & zMax <= 0)
                                                {
                                                    if (_Z <= zMin)
                                                    {
                                                        _zMinCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _zMinCheck = 0;
                                                    }
                                                    if (_Z >= zMax)
                                                    {
                                                        _zMaxCheck = 1;
                                                    }
                                                    else
                                                    {
                                                        _zMaxCheck = 0;
                                                    }
                                                }
                                                if (_xMinCheck == 1 & _yMinCheck == 1 & _zMinCheck == 1 & _xMaxCheck == 1 & _yMaxCheck == 1 & _zMaxCheck == 1)
                                                {
                                                    GameManager.Instance.World.RemoveEntity(_entity.entityId, EnumRemoveEntityReason.Despawned);
                                                    Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed zombie from protected zone @ {0} {1} {2}", _X, _Y, _Z));
                                                }
                                            }
                                            else
                                            {
                                                int _radius;
                                                if (int.TryParse(_box1[1], out _radius))
                                                {
                                                    if ((xMin - _X) * (xMin - _X) + (zMin - _Z) * (zMin - _Z) <= _radius * _radius)
                                                    {
                                                        GameManager.Instance.World.RemoveEntity(_entity.entityId, EnumRemoveEntityReason.Despawned);
                                                        Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed zombie from protected zone @ {0} {1} {2}", _X, _Y, _Z));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in EntityCleanup.ZombieCheck: {0}.", e));
            }
        }
    }
}
