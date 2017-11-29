using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    public class SpawnEntityInRadius : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "Spawn multiple entities in a radius around coordinates or of a player";
        }

        public override string GetHelp()
        {
            return "Spawn multiple entities around coordinates. Type \"ser\" to see all the available entity types\n" +
            "Usage:\n" +
            "   ser <x> <y> <z> <spawn radius> @ [<list of entities>]\n" +
            "or\n" +
            "   ser <x> <z> <spawn radius> @ [<list of entities>]>\n" +
            "or\n" +
            "   ser <steam id/player name/entity id> <spawn radius> @ [<list of entities>]\n" +
            "Example\n" +
            "   ser -1520 860 15 @ 1 1 18 18 21 21 21\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "spawnentityradius", "ser" };
        }

        private void PrintEntities()
        {
            SdtdConsole.Instance.Output("The entities need to be in the list below:");
            Dictionary<int, EntityClass>.KeyCollection entityTypesCollectionPrint = EntityClass.list.Keys;
            int interationVal = 1;
            foreach (int i in entityTypesCollectionPrint)
            {
                EntityClass eClass = EntityClass.list[i];
                if (!eClass.bAllowUserInstantiate)
                {
                    continue;
                }
                SdtdConsole.Instance.Output(" " + interationVal + " - " + eClass.entityClassName);
                ;
                interationVal++;
            }
        }


        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 2)
                {
                    SdtdConsole.Instance.Output("Wrong number of arguments, expected 3 or more, found " + _params.Count + ".");
                    SdtdConsole.Instance.Output(" ");
                    SdtdConsole.Instance.Output(GetHelp());
                    PrintEntities();
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
                    SdtdConsole.Instance.Output(" ");
                    SdtdConsole.Instance.Output(GetHelp());
                    PrintEntities();
                    return;
                }

                int x = int.MinValue;
                int y = int.MinValue;
                int z = int.MinValue;
                int radius = int.MinValue;

                if (paramType == 2)
                {
                    ClientInfo ci1 = ConsoleHelper.ParseParamIdOrName(_params[0]);
                    if (ci1 == null)
                    {
                        SdtdConsole.Instance.Output("Playername or entity/steamid id not found.");
                        return;
                    }
                    EntityPlayer ep1 = GameManager.Instance.World.Players.dict[ci1.entityId];
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
                    SdtdConsole.Instance.Output("Given radius is not a valid integer. It must be an integer greater or equal than 0.");
                    return;
                }

                Dictionary<int, EntityClass>.KeyCollection entityTypesCollection = EntityClass.list.Keys;
                for (int n = (paramType + 1); n < _params.Count; n++)
                {
                    int type = 0;
                    int.TryParse(_params[n], out type);

                    int interationNum = 1;
                    bool result = false;

                    foreach (int i in entityTypesCollection)
                    {
                        EntityClass eClass = EntityClass.list[i];
                        if (!eClass.bAllowUserInstantiate)
                        {
                            continue;
                        }
                        if (interationNum == type)
                        {
                            int realX, realY, realZ;
                            bool posFound = false;
                            if (radius == 0)
                            {
                                posFound = true;
                                realX = x;
                                realY = y;
                                realZ = z;
                            }
                            else
                            {
                                posFound = GameManager.Instance.World.FindRandomSpawnPointNearPosition(new Vector3((float)x, (float)y, (float)z), 15, out realX, out realY, out realZ, new Vector3((float)radius, (float)radius, (float)radius), true);
                                if (!posFound)
                                {
                                    posFound = GameManager.Instance.World.FindRandomSpawnPointNearPosition(new Vector3((float)x, (float)y, (float)z), 15, out realX, out realY, out realZ, new Vector3((float)radius, (float)150, (float)radius), true);
                                }
                            }
                            if (posFound)
                            {
                                Entity entity = EntityFactory.CreateEntity(i, new Vector3((float)realX, (float)realY, (float)realZ));
                                GameManager.Instance.World.SpawnEntityInWorld(entity);
                                result = true;
                                SdtdConsole.Instance.Output("Spawned " + eClass.entityClassName + " at " + realX + " " + realY + " " + realZ);
                            }
                            else
                            {
                                SdtdConsole.Instance.Output("No spawn point found near coordinate " + x + " " + y + " " + z);
                                result = true;
                            }
                            break;
                        }
                        interationNum++;
                    }
                    if (!result)
                    {
                        SdtdConsole.Instance.Output("Ignoring the invalid entity [" + type + "]");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out("Error in SpawnMultipleRadius: " + e);
            }
        }
    }
}