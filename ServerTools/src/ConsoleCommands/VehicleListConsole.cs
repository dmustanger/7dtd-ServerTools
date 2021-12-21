using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class VehicleListConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Lists the vehicles in the world.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. st-vl" +
                "1. Shows a list of each vehicle, entity id, location and the player using the vehicle if applicable\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-VehicleList", "vl", "st-vl" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                List<Entity> Entities = GameManager.Instance.World.Entities.list;
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Bicycle List:");
                for (int i = 0; i < Entities.Count; i++)
                {
                    Entity _entity = Entities[i];
                    string _name = EntityClass.list[_entity.entityClass].entityClassName;
                    if (_name == "vehicleBicycle")
                    {
                        Vector3 _pos = _entity.position;
                        int x = (int)_pos.x;
                        int y = (int)_pos.y;
                        int z = (int)_pos.z;
                        Entity _attachedPlayer = _entity.AttachedMainEntity;
                        if (_attachedPlayer != null)
                        {
                            ClientInfo _cInfo = PersistentOperations.GetClientInfoFromEntityId(_attachedPlayer.entityId);
                            if (_cInfo != null)
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] #{0}: Id {1}, located at x {2}  y {3} z {4}, player {5} is in this vehicle.", i, _entity.entityId, x, y, z, _cInfo.playerName));
                            }
                            else
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] #{0}: Id {1}, located at x {2}  y {3} z {4}", i, _entity.entityId, x, y, z));
                            }
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] #{0}: Id {1}, located at x {2}  y {3} z {4}", i, _entity.entityId, x, y, z));
                        }
                    }
                }
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Minibike List:");
                for (int i = 0; i < Entities.Count; i++)
                {
                    Entity _entity = Entities[i];
                    string _name = EntityClass.list[_entity.entityClass].entityClassName;
                    if (_name == "vehicleMinibike")
                    {
                        Vector3 _pos = _entity.position;
                        int x = (int)_pos.x;
                        int y = (int)_pos.y;
                        int z = (int)_pos.z;
                        Entity _attachedPlayer = _entity.AttachedMainEntity;
                        if (_attachedPlayer != null)
                        {
                            ClientInfo _cInfo = PersistentOperations.GetClientInfoFromEntityId(_attachedPlayer.entityId);
                            if (_cInfo != null)
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] #{0}: Id {1}, located at x {2}  y {3} z {4}, player {5} is in this vehicle", i, _entity.entityId, x, y, z, _cInfo.playerName));
                            }
                            else
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] #{0}: Id {1}, located at x {2}  y {3} z {4}", i, _entity.entityId, x, y, z));
                            }
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] #{0}: Id {1}, located at x {2}  y {3} z {4}", i, _entity.entityId, x, y, z));
                        }
                    }
                }
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Motorcycle List:");
                for (int i = 0; i < Entities.Count; i++)
                {
                    Entity _entity = Entities[i];
                    string _name = EntityClass.list[_entity.entityClass].entityClassName;
                    if (_name == "vehicleMotorcycle")
                    {
                        Vector3 _pos = _entity.position;
                        int x = (int)_pos.x;
                        int y = (int)_pos.y;
                        int z = (int)_pos.z;
                        Entity _attachedPlayer = _entity.AttachedMainEntity;
                        if (_attachedPlayer != null)
                        {
                            ClientInfo _cInfo = PersistentOperations.GetClientInfoFromEntityId(_attachedPlayer.entityId);
                            if (_cInfo != null)
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] #{0}: Id {1}, located at x {2}  y {3} z {4}, player {5} is in this vehicle", i, _entity.entityId, x, y, z, _cInfo.playerName));
                            }
                            else
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] #{0}: Id {1}, located at x {2}  y {3} z {4}", i, _entity.entityId, x, y, z));
                            }
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] #{0}: Id {1}, located at x {2}  y {3} z {4}", i, _entity.entityId, x, y, z));
                        }
                    }
                }
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output("4x4 List:");
                for (int i = 0; i < Entities.Count; i++)
                {
                    Entity _entity = Entities[i];
                    string _name = EntityClass.list[_entity.entityClass].entityClassName;
                    if (_name == "vehicle4x4Truck")
                    {
                        Vector3 _pos = _entity.position;
                        int x = (int)_pos.x;
                        int y = (int)_pos.y;
                        int z = (int)_pos.z;
                        Entity _attachedPlayer = _entity.AttachedMainEntity;
                        if (_attachedPlayer != null)
                        {
                            ClientInfo _cInfo = PersistentOperations.GetClientInfoFromEntityId(_attachedPlayer.entityId);
                            if (_cInfo != null)
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] #{0}: Id {1}, located at x {2}  y {3} z {4}, player {5} is in this vehicle", i, _entity.entityId, x, y, z, _cInfo.playerName));
                            }
                            else
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] #{0}: Id {1}, located at x {2}  y {3} z {4}", i, _entity.entityId, x, y, z));
                            }
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] #{0}: Id {1}, located at x {2}  y {3} z {4}", i, _entity.entityId, x, y, z));
                        }
                    }
                }
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Gyro List:");
                for (int i = 0; i < Entities.Count; i++)
                {
                    Entity _entity = Entities[i];
                    string _name = EntityClass.list[_entity.entityClass].entityClassName;
                    if (_name == "vehicleGyrocopter")
                    {
                        Vector3 _pos = _entity.position;
                        int x = (int)_pos.x;
                        int y = (int)_pos.y;
                        int z = (int)_pos.z;
                        Entity _attachedPlayer = _entity.AttachedMainEntity;
                        if (_attachedPlayer != null)
                        {
                            ClientInfo _cInfo = PersistentOperations.GetClientInfoFromEntityId(_attachedPlayer.entityId);
                            if (_cInfo != null)
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] #{0}: Id {1}, located at x {2}  y {3} z {4}, player {5} is in this vehicle", i, _entity.entityId, x, y, z, _cInfo.playerName));
                            }
                            else
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] #{0}: Id {1}, located at x {2}  y {3} z {4}", i, _entity.entityId, x, y, z));
                            }
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] #{0}: Id {1}, located at x {2}  y {3} z {4}", i, _entity.entityId, x, y, z));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in VehicleListConsole.Execute: {0}", e.Message));
            }
        }
    }
}
