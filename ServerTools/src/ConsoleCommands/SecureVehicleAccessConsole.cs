using System;
using System.Collections.Generic;

namespace ServerTools
{
    class SecureVehicleAccessConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Attempts to set access to all secure vehicle.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-sva\n" +
                   "1. Attempts to set access to all secure vehicle\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-SecureVehicleAccess", "sva", "st-sva" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                    return;
                }
                if (!string.IsNullOrEmpty(_senderInfo.RemoteClientInfo.CrossplatformId.CombinedString))
                {
                    List<Entity> Entities = GameManager.Instance.World.Entities.list;
                    if (Entities != null)
                    {
                        for (int i = 0; i < Entities.Count; i++)
                        {
                            Entity _entity = Entities[i];
                            if (_entity != null && !_entity.IsClientControlled() && _entity.IsSpawned())
                            {
                                string _name = EntityClass.list[_entity.entityClass].entityClassName;
                                if (_name == "vehicleBicycle" || _name == "vehicleMinibike" || _name == "vehicleMotorcycle" || _name == "vehicle4x4Truck" || _name == "vehicleGyrocopter")
                                {
                                    EntityVehicle _vehicle = _entity as EntityVehicle;
                                    if (!_vehicle.IsUserAllowed(_senderInfo.RemoteClientInfo.CrossplatformId))
                                    {
                                        List<PlatformUserIdentifierAbs> _users = _vehicle.GetUsers();
                                        _users.Add(_senderInfo.RemoteClientInfo.CrossplatformId);
                                        _vehicle.bPlayerStatsChanged = true;
                                    }
                                }
                            }
                        }
                    }
                }
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Vehicle access set for '{0}' in loaded areas. Unloaded areas are unchanged", _senderInfo.RemoteClientInfo.CrossplatformId.CombinedString));
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in SecureVehicleAccess.Execute: {0}", e.Message));
            }
        }
    }
}
