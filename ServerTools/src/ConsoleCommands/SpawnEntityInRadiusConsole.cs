using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace ServerTools
{
    class SpawnEntityInRadiusConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Spawn multiple entity in a radius from a location or player.";
        }

        protected override string getHelp()
        {
            return "Usage:\n" +
                "  1. st-ser <X> <Y> <Z> r.<Radius> <Entity Id>\n" +
                "  2. st-ser <EOS/EntityId/PlayerName> r.<Radius> <Entity Id>\n" +
                "1. Spawn one or more entity with in a radius of the specified coordinates. Enter the x y z coordinates, radius and entity id you wish to spawn\n" +
                "2. Spawn one or more entity with in a radius of the specified player. Enter the id, name or entity id of the player. Enter the radius and entity id to spawn\n" +
                "*Note*     Type spawnentity or se in the console to see all the available entity and their id\n" +
                "*Example*   st-SpawnEntityRadius -55 -1 1000 r.30 2 18 18 20 21 71\n" +
                "*Example*   st-ser Steam_76561191234567890 r.30 61 62 63 63\n";
        }

        protected override string[] getCommands()
        {
            return new string[] { "st-SpawnEntityRadius", "ser", "st-ser" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 3)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 3 or more, found '{0}'", _params.Count));
                    return;
                }
                float x1, y1, z1, radius;
                if (!_params[1].Contains("r.") && !_params[3].Contains("r."))
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] Invalid format for radius. Missing r.");
                    return;
                }
                if (_params[1].Contains("r."))
                {
                    ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_params[0]);
                    if (cInfo != null)
                    {
                        EntityPlayer entityPlayer = GeneralOperations.GetEntityPlayer(cInfo.entityId);
                        if (entityPlayer != null)
                        {
                            Vector3 pos = entityPlayer.GetPosition();
                            x1 = (int)pos.x;
                            y1 = (int)pos.y;
                            z1 = (int)pos.z;
                            if (float.TryParse(_params[1].Replace("r.", ""), out radius))
                            {
                                if (radius < 0)
                                {
                                    radius = 1;
                                }
                                _params.RemoveRange(0, 2);
                                string[] _entities = _params.ToArray();
                                if (_entities.Length > 1)
                                {
                                    for (int i = 0; i < _entities.Length; i++)
                                    {

                                        SpawnEntity(new Vector3(x1, y1, z1), radius, _entities[i]);
                                        Thread.Sleep(250);
                                    }
                                }
                                else
                                {
                                    SpawnEntity(new Vector3(x1, y1, z1), radius, _entities[0]);
                                }
                            }
                            else
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer for radius '{0}'", _params[1]));
                                return;
                            }
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Player name, Entity id or EOS id not found online for '{0}'", _params[0]));
                        return;
                    }
                }
                else if (_params[3].Contains("r."))
                {
                    if (float.TryParse(_params[0], out x1))
                    {
                        if (float.TryParse(_params[1], out y1))
                        {
                            if (float.TryParse(_params[2], out z1))
                            {
                                if (float.TryParse(_params[3].Replace("r.", ""), out radius))
                                {
                                    if (radius < 0)
                                    {
                                        radius = 1;
                                    }
                                    _params.RemoveRange(0, 3);
                                    string[] _entities = _params.ToArray();
                                    if (_entities.Length > 1)
                                    {
                                        for (int i = 0; i < _entities.Length; i++)
                                        {
                                            SpawnEntity(new Vector3(x1, y1, z1), radius, _entities[i]);
                                            Thread.Sleep(250);
                                        }
                                    }
                                    else
                                    {
                                        SpawnEntity(new Vector3(x1, y1, z1), radius, _entities[0]);
                                    }
                                }
                                else
                                {
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer for radius '{0}'", _params[3]));
                                    return;
                                }
                            }
                            else
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer '{0}'", _params[2]));
                                return;
                            }
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer '{0}'", _params[1]));
                            return;
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid integer '{0}'", _params[0]));
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
                if (int.TryParse(_id, out int id))
                {
                    GeneralOperations.EntityId.TryGetValue(id, out int entityId);
                    bool posFound = GameManager.Instance.World.FindRandomSpawnPointNearPosition(_pos, 15, out int _x, out int _y, out int _z, new Vector3(_radius, _radius, _radius), true);
                    if (!posFound)
                    {
                        posFound = GameManager.Instance.World.FindRandomSpawnPointNearPosition(_pos, 15, out _x, out _y, out _z, new Vector3(_radius + 5, _radius + 50, _radius + 5), true);
                    }
                    if (posFound)
                    {
                        Entity entity = EntityFactory.CreateEntity(entityId, new Vector3(_x, _y, _z));
                        GameManager.Instance.World.SpawnEntityInWorld(entity);
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Spawned a {0} at {1} x, {2} y, {3} z", entity.EntityClass.entityClassName, _x, _y, _z));
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] No spawn points were found near {0} x, {1} y, {2} z", _pos.x, _pos.y, _pos.z));
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