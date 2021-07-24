using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace ServerTools
{
    class SpawnEntityInRadiusConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Spawn multiple entity in a radius from a location or player.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. st-ser <X> <Y> <Z> r.<Radius> <Entity Id>\n" +
                "  2. st-ser <SteamId/EntityId/PlayerName> r.<Radius> <Entity Id>\n" +
                "1. Spawn one or more entity with in a radius of the specified coordinates. Enter the x y z coordinates, radius and entity id you wish to spawn\n" +
                "2. Spawn one or more entity with in a radius of the specified player. Enter the steam id, player name or entity id of the player. Enter the radius and entity id to spawn\n" +
                "*Note*     Type spawnentity or se in the console to see all the available entity and their id\n" +
                "*Example*   st-SpawnEntityRadius -55 -1 1000 r.30 2 18 18 20 21 71\n" +
                "*Example*   st-ser 76561191234567890 r.30 61 62 63 63\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-SpawnEntityRadius", "ser", "st-ser" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 3)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 3 or more, found {0}", _params.Count));
                    return;
                }
                float _x1, _y1, _z1, _radius;
                if (!_params[1].Contains("r.") && !_params[3].Contains("r."))
                {
                    SdtdConsole.Instance.Output("[SERVERTOOLS] Invalid format for radius. Missing r.");
                    return;
                }
                if (_params[1].Contains("r."))
                {
                    ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[0]);
                    if (_cInfo != null)
                    {
                        if (GameManager.Instance.World.Players.dict.ContainsKey(_cInfo.entityId))
                        {
                            EntityPlayer _entityPlayer = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                            Vector3 pos = _entityPlayer.GetPosition();
                            _x1 = (int)pos.x;
                            _y1 = (int)pos.y;
                            _z1 = (int)pos.z;
                            if (float.TryParse(_params[1].Replace("r.", ""), out _radius))
                            {
                                if (_radius < 0)
                                {
                                    _radius = 1;
                                }
                                _params.RemoveRange(0, 2);
                                string[] _entities = _params.ToArray();
                                if (_entities.Length > 1)
                                {
                                    for (int i = 0; i < _entities.Length; i++)
                                    {
                                        SpawnEntity(new Vector3(_x1, _y1, _z1), _radius, _entities[i]);
                                        Thread.Sleep(250);
                                    }
                                }
                                else
                                {
                                    SpawnEntity(new Vector3(_x1, _y1, _z1), _radius, _entities[0]);
                                }
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer for radius: {0}", _params[1]));
                                return;
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Playername or entity/steam id not found online for {0}", _params[0]));
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Playername or entity/steam id not found online for {0}", _params[0]));
                        return;
                    }
                }
                else if (_params[3].Contains("r."))
                {
                    if (float.TryParse(_params[0], out _x1))
                    {
                        if (float.TryParse(_params[1], out _y1))
                        {
                            if (float.TryParse(_params[2], out _z1))
                            {
                                if (float.TryParse(_params[3].Replace("r.", ""), out _radius))
                                {
                                    if (_radius < 0)
                                    {
                                        _radius = 1;
                                    }
                                    _params.RemoveRange(0, 3);
                                    string[] _entities = _params.ToArray();
                                    if (_entities.Length > 1)
                                    {
                                        for (int i = 0; i < _entities.Length; i++)
                                        {
                                            SpawnEntity(new Vector3(_x1, _y1, _z1), _radius, _entities[i]);
                                            Thread.Sleep(250);
                                        }
                                    }
                                    else
                                    {
                                        SpawnEntity(new Vector3(_x1, _y1, _z1), _radius, _entities[0]);
                                    }
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer for radius: {0}", _params[3]));
                                    return;
                                }
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer: {0}", _params[2]));
                                return;
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer: {0}", _params[1]));
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer: {0}", _params[0]));
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in SpawnMultipleRadiusConsole.Execute: {0}", e.Message));
            }
        }

        private static void SpawnEntity(Vector3 _pos, float _radius, string _id)
        {
            try
            {
                if (int.TryParse(_id, out int _entity))
                {
                    PersistentOperations.EntityId.TryGetValue(_entity, out int _entityId);
                    bool posFound = GameManager.Instance.World.FindRandomSpawnPointNearPosition(_pos, 15, out int _x, out int _y, out int _z, new Vector3(_radius, _radius, _radius), true);
                    if (!posFound)
                    {
                        posFound = GameManager.Instance.World.FindRandomSpawnPointNearPosition(_pos, 15, out _x, out _y, out _z, new Vector3(_radius + 5, _radius + 50, _radius + 5), true);
                    }
                    if (posFound)
                    {
                        Entity entity = EntityFactory.CreateEntity(_entityId, new Vector3(_x, _y, _z));
                        GameManager.Instance.World.SpawnEntityInWorld(entity);
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Spawned a {0} at {1} x, {2} y, {3} z", entity.EntityClass.entityClassName, _x, _y, _z));
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] No spawn points were found near {0} x, {1} y, {2} z", _pos.x, _pos.y, _pos.z));
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in SpawnMultipleRadiusConsole.SpawnEntity: {0}", e.Message));
            }
        }
    }
}