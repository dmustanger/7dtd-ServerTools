using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    public class SpawnEntityInRadiusConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]-Spawn multiple entity in a radius around coordinates or in a radius around a player";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. ser <x> <y> <z> <spawn radius> @ [<list of entities>]\n" +
                "  2. ser <x> <z> <spawn radius> @ [<list of entities>]>\n" +
                "  3. ser <steamId/entityId/playerName> <spawn radius> @ [<list of entities>]\n" +
                "1. Spawn one or more entity with in a radius of the specified coordinates. Enter the x y z coordinates, radius and entity id of the entity to spawn\n" +
                "2. Spawn one or more entity with in a radius of the specified coordinates. Enter the x z coordinates, radius and entity id of the entity to spawn\n" +
                "3. Spawn one or more entity with in a radius of the specified player. Enter the steam id, player name or entity id of the player. Enter the radius and entity id to spawn\n" +
                "*Note*     Type spawnentity or se in the console to see all the available entity types and their Id\n" +
                "*Example*   ser -55 -1 1000 30 @ 2 18 18 20 21 71\n" +
                "*Example*   ser -55 1000 30 @ 61 62 63 63\n" +
                "*Example*   ser 76561191234567890 30 @ 61 62 63 63\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-SpawnEntityRadius", "spawnentityradius", "ser" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 3)
                {
                    SdtdConsole.Instance.Output("Wrong number of arguments, expected 3 or more, found " + _params.Count + ".");
                    SdtdConsole.Instance.Output(GetHelp());
                    return;
                }
                int paramType = 0;
                for (int n = 0; n < _params.Count; n++)
                {
                    if (_params[n] == "@")
                    {
                        if (n == 2 || n == 3 || n == 4)
                        {
                            paramType = n;
                            break;
                        }
                    }
                }
                if (paramType == 0)
                {
                    SdtdConsole.Instance.Output("Wrong pattern of arguments.");
                    SdtdConsole.Instance.Output(GetHelp());
                    return;
                }
        
                int x = int.MinValue;
                int y = int.MinValue;
                int z = int.MinValue;
                int radius = int.MinValue;
        
                if (paramType == 2)
                {
                    ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[0]);
                    if (_cInfo == null)
                    {
                        SdtdConsole.Instance.Output("Playername or entity/steamid id not found.");
                        return;
                    }
                    EntityPlayer ep1 = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    Vector3 pos = ep1.GetPosition();
                    x = (int)pos.x;
                    y = (int)pos.y;
                    z = (int)pos.z;
                }
                else if (paramType == 3 || paramType == 4)
                {
                    int.TryParse(_params[0], out x);
                    if (paramType == 3)
                    {
                        y = 150;
                        int.TryParse(_params[1], out z);
                    }
                    else if (paramType == 4)
                    {
                        int.TryParse(_params[1], out y);
                        int.TryParse(_params[2], out z);
                    }
                }
                if (x == int.MinValue || y == int.MinValue || z == int.MinValue)
                {
                    SdtdConsole.Instance.Output("x:" + x);
                    SdtdConsole.Instance.Output("y:" + y);
                    SdtdConsole.Instance.Output("z:" + z);
                    SdtdConsole.Instance.Output("At least one of the given coordinates is not a valid integer");
                    return;
                }
        
                int.TryParse(_params[(paramType - 1)], out radius);
                if (radius < 0)
                {
                    radius = 0;
                }
        
                Dictionary<int, EntityClass>.KeyCollection entityTypesCollection = EntityClass.list.Dict.Keys;
                for (int n = (paramType + 1); n < _params.Count; n++)
                {
                    int _id = 0;
                    int.TryParse(_params[n], out _id);
        
                    int counter = 1;
                    int _x, _y, _z;
                    bool posFound = false;
                    foreach (int i in entityTypesCollection)
                    {
                        EntityClass eClass = EntityClass.list[i];
                        if (!eClass.bAllowUserInstantiate)
                        {
                            continue;
                        }
                        if (_id == counter)
                        {
                            if (radius == 0)
                            {
                                posFound = true;
                                _x = x;
                                _y = y;
                                _z = z;
                            }
                            else
                            {
                                posFound = GameManager.Instance.World.FindRandomSpawnPointNearPosition(new Vector3((float)x, (float)y, (float)z), 15, out _x, out _y, out _z, new Vector3((float)radius, (float)radius, (float)radius), true);
                                if (!posFound)
                                {
                                    posFound = GameManager.Instance.World.FindRandomSpawnPointNearPosition(new Vector3((float)x, (float)y, (float)z), 15, out _x, out _y, out _z, new Vector3((float)radius + 5, (float)radius + 50, (float)radius + 5), true);
                                }
                            }
                            if (posFound)
                            {
                                Entity entity = EntityFactory.CreateEntity(i, new Vector3((float)_x, (float)_y, (float)_z));
                                GameManager.Instance.World.SpawnEntityInWorld(entity);
                                SdtdConsole.Instance.Output(string.Format("Spawned a {0} at {1} x, {2} y, {3} z", eClass.entityClassName, _x, _y, _z));
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("No spawn points were found near {0} x, {1} y, {2} z", _x, _y, _z));
                            }
                            break;
                        }
                        counter++;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in SpawnMultipleRadiusConsole.Run: {0}.", e));
            }
        }
    }
}