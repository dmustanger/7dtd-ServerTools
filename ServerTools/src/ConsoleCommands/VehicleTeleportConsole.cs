using System;
using System.Collections.Generic;
using System.Xml;

namespace ServerTools
{
    class VehicleTeleportConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Travel.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. VehicleTeleport off\n" +
                   "  2. VehicleTeleport on\n" +
                   "1. Turn off vehicle teleport\n" +
                   "2. Turn on vehicle teleport\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-VehicleTeleport", "vehicleteleport", "st-vt", "vt" };
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
                        SdtdConsole.Instance.Output(string.Format("Vehicle teleport has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Vehicle teleport is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!VehicleTeleport.IsEnabled)
                    {
                        VehicleTeleport.IsEnabled = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Vehicle teleport has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Vehicle teleport is already on"));
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