using System;
using System.Collections.Generic;

namespace ServerTools
{
    class VehicleTeleportConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable vehicle teleport tool.";
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
            return new string[] { "st-VehicleTeleport", "st-vt", "st-vt" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (VehicleTeleport.IsEnabled)
                    {
                        VehicleTeleport.IsEnabled = false;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Vehicle teleport has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Vehicle teleport is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!VehicleTeleport.IsEnabled)
                    {
                        VehicleTeleport.IsEnabled = true;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Vehicle teleport has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Vehicle teleport is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("bike"))
                {
                    if (VehicleTeleport.Bike)
                    {
                        VehicleTeleport.Bike = false;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Vehicle teleport bike has been set to off"));
                        return;
                    }
                    else
                    {
                        VehicleTeleport.Bike = true;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Vehicle teleport bike has been set to on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("minibike"))
                {
                    if (VehicleTeleport.Mini_Bike)
                    {
                        VehicleTeleport.Mini_Bike = false;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Vehicle teleport minibike has been set to off"));
                        return;
                    }
                    else
                    {
                        VehicleTeleport.Mini_Bike = true;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Vehicle teleport minibike has been set to on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("motorbike"))
                {
                    if (VehicleTeleport.Motor_Bike)
                    {
                        VehicleTeleport.Motor_Bike = false;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Vehicle teleport minibike has been set to off"));
                        return;
                    }
                    else
                    {
                        VehicleTeleport.Motor_Bike = true;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Vehicle teleport minibike has been set to on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("jeep"))
                {
                    if (VehicleTeleport.Jeep)
                    {
                        VehicleTeleport.Jeep = false;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Vehicle teleport jeep has been set to off"));
                        return;
                    }
                    else
                    {
                        VehicleTeleport.Jeep = true;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Vehicle teleport jeep has been set to on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("gyro"))
                {
                    if (VehicleTeleport.Gyro)
                    {
                        VehicleTeleport.Gyro = false;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Vehicle teleport gyro has been set to off"));
                        return;
                    }
                    else
                    {
                        VehicleTeleport.Gyro = true;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Vehicle teleport gyro has been set to on"));
                        return;
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in VehicleTeleportConsole.Execute: {0}", e.Message));
            }
        }
    }
}