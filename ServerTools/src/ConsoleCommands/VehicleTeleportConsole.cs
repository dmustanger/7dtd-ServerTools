using System;
using System.Collections.Generic;

namespace ServerTools
{
    class VehicleTeleportConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Vehicle Teleport tool.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. VehicleTeleport off\n" +
                   "  2. VehicleTeleport on\n" +
                   "  3. VehicleTeleport bike\n" +
                   "  4. VehicleTeleport minibike\n" +
                   "  5. VehicleTeleport motorbike\n" +
                   "  6. VehicleTeleport jeep\n" +
                   "  7. VehicleTeleport gyro\n" +
                   "1. Turn off the vehicle teleport tool\n" +
                   "2. Turn on the vehicle teleport tool\n" +
                   "3. Turn on/off the vehicle teleport for bike\n" +
                   "4. Turn on/off the vehicle teleport for minibike\n" +
                   "5. Turn on/off the vehicle teleport for motorbike\n" +
                   "6. Turn on/off the vehicle teleport for jeep\n" +
                   "7. Turn on/off the vehicle teleport for gyro\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-VehicleTeleport", "VehicleTeleport", "vehicleteleport", "vt" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (VehicleTeleport.IsEnabled)
                    {
                        VehicleTeleport.IsEnabled = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Vehicle_Teleport has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Vehicle_Teleport is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!VehicleTeleport.IsEnabled)
                    {
                        VehicleTeleport.IsEnabled = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Vehicle_Teleport has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Vehicle_Teleport is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("bike"))
                {
                    if (VehicleTeleport.Bike)
                    {
                        VehicleTeleport.Bike = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Vehicle_Teleport bike has been set to off"));
                        return;
                    }
                    else
                    {
                        VehicleTeleport.Bike = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Vehicle_Teleport bike has been set to on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("minibike"))
                {
                    if (VehicleTeleport.Mini_Bike)
                    {
                        VehicleTeleport.Mini_Bike = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Vehicle_Teleport minibike has been set to off"));
                        return;
                    }
                    else
                    {
                        VehicleTeleport.Mini_Bike = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Vehicle_Teleport minibike has been set to on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("motorbike"))
                {
                    if (VehicleTeleport.Motor_Bike)
                    {
                        VehicleTeleport.Motor_Bike = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Vehicle_Teleport minibike has been set to off"));
                        return;
                    }
                    else
                    {
                        VehicleTeleport.Motor_Bike = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Vehicle_Teleport minibike has been set to on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("jeep"))
                {
                    if (VehicleTeleport.Jeep)
                    {
                        VehicleTeleport.Jeep = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Vehicle_Teleport jeep has been set to off"));
                        return;
                    }
                    else
                    {
                        VehicleTeleport.Jeep = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Vehicle_Teleport jeep has been set to on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("gyro"))
                {
                    if (VehicleTeleport.Gyro)
                    {
                        VehicleTeleport.Gyro = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Vehicle_Teleport gyro has been set to off"));
                        return;
                    }
                    else
                    {
                        VehicleTeleport.Gyro = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Vehicle_Teleport gyro has been set to on"));
                        return;
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in VehicleTeleportConsole.Execute: {0}", e));
            }
        }
    }
}