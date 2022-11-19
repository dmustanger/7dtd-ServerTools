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
                "  2. st-vl remove all\n" +
                "1. Shows a list of each vehicle, entity id, location and the player using the vehicle if applicable\n" +
                "2. Removes all vehicles shown on the list\n";
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
                    List<Entity> Entities = GameManager.Instance.World.Entities.list;
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Bicycle List:");
                    for (int i = 0; i < Entities.Count; i++)
                    {
                        Entity entity = Entities[i];
                        string name = EntityClass.list[entity.entityClass].entityClassName;
                        if (name == "vehicleBicycle")
                        {
                            Vector3 pos = entity.position;
                            int x = (int)pos.x;
                            int y = (int)pos.y;
                            int z = (int)pos.z;
                            Entity attachedPlayer = entity.AttachedMainEntity;
                            if (attachedPlayer != null)
                            {
                                ClientInfo cInfo = GeneralFunction.GetClientInfoFromEntityId(attachedPlayer.entityId);
                                if (cInfo != null)
                                {
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] #{0}: Id {1}, located at x {2}  y {3} z {4}, player {5} is in this vehicle.", i, entity.entityId, x, y, z, cInfo.playerName));
                                }
                                else
                                {
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] #{0}: Id {1}, located at x {2}  y {3} z {4}", i, entity.entityId, x, y, z));
                                }
                            }
                            else
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] #{0}: Id {1}, located at x {2}  y {3} z {4}", i, entity.entityId, x, y, z));
                            }
                        }
                    }
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Minibike List:");
                    for (int i = 0; i < Entities.Count; i++)
                    {
                        Entity entity = Entities[i];
                        string name = EntityClass.list[entity.entityClass].entityClassName;
                        if (name == "vehicleMinibike")
                        {
                            Vector3 pos = entity.position;
                            int x = (int)pos.x;
                            int y = (int)pos.y;
                            int z = (int)pos.z;
                            Entity attachedPlayer = entity.AttachedMainEntity;
                            if (attachedPlayer != null)
                            {
                                ClientInfo cInfo = GeneralFunction.GetClientInfoFromEntityId(attachedPlayer.entityId);
                                if (cInfo != null)
                                {
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] #{0}: Id {1}, located at x {2}  y {3} z {4}, player {5} is in this vehicle", i, entity.entityId, x, y, z, cInfo.playerName));
                                }
                                else
                                {
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] #{0}: Id {1}, located at x {2}  y {3} z {4}", i, entity.entityId, x, y, z));
                                }
                            }
                            else
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] #{0}: Id {1}, located at x {2}  y {3} z {4}", i, entity.entityId, x, y, z));
                            }
                        }
                    }
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Motorcycle List:");
                    for (int i = 0; i < Entities.Count; i++)
                    {
                        Entity entity = Entities[i];
                        string name = EntityClass.list[entity.entityClass].entityClassName;
                        if (name == "vehicleMotorcycle")
                        {
                            Vector3 pos = entity.position;
                            int x = (int)pos.x;
                            int y = (int)pos.y;
                            int z = (int)pos.z;
                            Entity attachedPlayer = entity.AttachedMainEntity;
                            if (attachedPlayer != null)
                            {
                                ClientInfo cInfo = GeneralFunction.GetClientInfoFromEntityId(attachedPlayer.entityId);
                                if (cInfo != null)
                                {
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] #{0}: Id {1}, located at x {2}  y {3} z {4}, player {5} is in this vehicle", i, entity.entityId, x, y, z, cInfo.playerName));
                                }
                                else
                                {
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] #{0}: Id {1}, located at x {2}  y {3} z {4}", i, entity.entityId, x, y, z));
                                }
                            }
                            else
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] #{0}: Id {1}, located at x {2}  y {3} z {4}", i, entity.entityId, x, y, z));
                            }
                        }
                    }
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output("4x4 List:");
                    for (int i = 0; i < Entities.Count; i++)
                    {
                        Entity entity = Entities[i];
                        string name = EntityClass.list[entity.entityClass].entityClassName;
                        if (name == "vehicle4x4Truck")
                        {
                            Vector3 pos = entity.position;
                            int x = (int)pos.x;
                            int y = (int)pos.y;
                            int z = (int)pos.z;
                            Entity attachedPlayer = entity.AttachedMainEntity;
                            if (attachedPlayer != null)
                            {
                                ClientInfo cInfo = GeneralFunction.GetClientInfoFromEntityId(attachedPlayer.entityId);
                                if (cInfo != null)
                                {
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] #{0}: Id {1}, located at x {2}  y {3} z {4}, player {5} is in this vehicle", i, entity.entityId, x, y, z, cInfo.playerName));
                                }
                                else
                                {
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] #{0}: Id {1}, located at x {2}  y {3} z {4}", i, entity.entityId, x, y, z));
                                }
                            }
                            else
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] #{0}: Id {1}, located at x {2}  y {3} z {4}", i, entity.entityId, x, y, z));
                            }
                        }
                    }
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Gyro List:");
                    for (int i = 0; i < Entities.Count; i++)
                    {
                        Entity entity = Entities[i];
                        string name = EntityClass.list[entity.entityClass].entityClassName;
                        if (name == "vehicleGyrocopter")
                        {
                            Vector3 pos = entity.position;
                            int x = (int)pos.x;
                            int y = (int)pos.y;
                            int z = (int)pos.z;
                            Entity attachedPlayer = entity.AttachedMainEntity;
                            if (attachedPlayer != null)
                            {
                                ClientInfo cInfo = GeneralFunction.GetClientInfoFromEntityId(attachedPlayer.entityId);
                                if (cInfo != null)
                                {
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] #{0}: Id {1}, located at x {2}  y {3} z {4}, player {5} is in this vehicle", i, entity.entityId, x, y, z, cInfo.playerName));
                                }
                                else
                                {
                                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] #{0}: Id {1}, located at x {2}  y {3} z {4}", i, entity.entityId, x, y, z));
                                }
                            }
                            else
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] #{0}: Id {1}, located at x {2}  y {3} z {4}", i, entity.entityId, x, y, z));
                            }
                        }
                    }
                }
                else if (_params.Count == 1)
                {
                    List<Entity> Entities = GameManager.Instance.World.Entities.list;
                    for (int i = 0; i < Entities.Count; i++)
                    {
                        if (Entities[i] is EntityVehicle)
                        {
                            GameManager.Instance.World.RemoveEntity(Entities[i].entityId, EnumRemoveEntityReason.Despawned);
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Removed vehicle id '{0}' located at '{1}'", Entities[i].entityId, Entities[i].position));
                        }
                    }
                    return;
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 0 or 1, found '{0}'", _params.Count));
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
