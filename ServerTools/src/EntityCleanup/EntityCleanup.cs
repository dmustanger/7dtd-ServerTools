using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class EntityCleanup
    {
        public static bool ItemIsEnabled = false, BlockIsEnabled = false, FallingTreeEnabled = false;
        private static List<Entity> Entities = new List<Entity>();
        private static List<int> FallingTree = new List<int>();
        private static int _xMinCheck, _yMinCheck, _zMinCheck, _xMaxCheck, _yMaxCheck, _zMaxCheck;

        public static void EntityCheck()
        {
            Entities = GameManager.Instance.World.Entities.list;
            for (int i = 0; i < Entities.Count; i++)
            {
                int _itemCounter = 0;
                int _blockCounter = 0;
                Entity _entity = Entities[i];
                if (_entity != null)
                {
                    if (!_entity.IsClientControlled())
                    {
                        string _name = EntityClass.list[_entity.entityClass].entityClassName;
                        if (ItemIsEnabled)
                        {
                            if (_name == "item")
                            {
                                Vector3 _vec = _entity.position;
                                for (int j = 0; j < Entities.Count; j++)
                                {
                                    Entity _entity2 = Entities[j];
                                    if (_entity2 != null)
                                    {
                                        if (_entity != _entity2)
                                        {
                                            string _name2 = EntityClass.list[_entity2.entityClass].entityClassName;
                                            if (_name2 == "item")
                                            {
                                                if ((_entity.position.x - _entity2.position.x) * (_entity.position.x - _entity2.position.x) + (_entity.position.z - _entity2.position.z) * (_entity.position.z - _entity2.position.z) <= 3 * 3)
                                                {
                                                    _itemCounter++;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (_itemCounter >= 11)
                                {
                                    GameManager.Instance.World.RemoveEntity(_entity.entityId, EnumRemoveEntityReason.Despawned);
                                    Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed item id {0} @ {1} {2} {3}", _entity.entityId, _vec.x, _vec.y, _vec.z));
                                }
                            }
                        }
                        if (BlockIsEnabled)
                        {
                            World world = GameManager.Instance.World;
                            if (_name == "fallingBlock")
                            {
                                for (int j = 0; j < Entities.Count; j++)
                                {
                                    Entity _entity2 = Entities[j];
                                    if (_entity2 != null)
                                    {
                                        if (_entity != _entity2)
                                        {
                                            string _name2 = EntityClass.list[_entity2.entityClass].entityClassName;
                                            if (_name2 == "fallingBlock")
                                            {
                                                if ((_entity.position.x - _entity2.position.x) * (_entity.position.x - _entity2.position.x) + (_entity.position.z - _entity2.position.z) * (_entity.position.z - _entity2.position.z) <= 20 * 20)
                                                {
                                                    _blockCounter++;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (_blockCounter >= 12)
                                {
                                    GameManager.Instance.World.RemoveEntity(_entity.entityId, EnumRemoveEntityReason.Despawned);
                                    EntityPlayer _douche = world.GetClosestPlayer(_entity.position.x, _entity.position.y, _entity.position.z, 10, false);
                                    Log.Out(string.Format("[SERVERTOOLS] Entity cleanup: Removed falling block id {0} @ {1} {2} {3}. Closest player is {4}", _entity.entityId, _entity.position.x, _entity.position.y, _entity.position.z, _douche.EntityName));
                                }
                            }
                        }
                        if (FallingTreeEnabled)
                        {
                            if (_name == "fallingTree")
                            {
                                if (!FallingTree.Contains(_entity.entityId))
                                {
                                    FallingTree.Add(_entity.entityId);
                                }
                                else
                                {
                                    GameManager.Instance.World.RemoveEntity(_entity.entityId, EnumRemoveEntityReason.Despawned);
                                    FallingTree.Remove(_entity.entityId);
                                    Log.Out("[SERVERTOOLS] Entity cleanup: Removed falling tree");
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void ZombieCheck()
        {
            Entities = GameManager.Instance.World.Entities.list;
            if (ZoneProtection.Box.Count > 0)
            {
                for (int i = 0; i < Entities.Count; i++)
                {
                    Entity _entity = Entities[i];
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
                                foreach (KeyValuePair<string, string[]> kvpCorners in ZoneProtection.Box)
                                {
                                    int xMin, yMin, zMin;
                                    string[] _corner1 = kvpCorners.Value[0].Split(',');
                                    int.TryParse(_corner1[0], out xMin);
                                    int.TryParse(_corner1[1], out yMin);
                                    int.TryParse(_corner1[2], out zMin);
                                    int xMax, yMax, zMax;
                                    string[] _corner2 = kvpCorners.Value[1].Split(',');
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
                            }
                        }
                    }
                }
            }
        }
    }
}
