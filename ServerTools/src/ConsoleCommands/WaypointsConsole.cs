using System;
using System.Collections.Generic;
using System.Xml;

namespace ServerTools
{
    class WaypointsConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Waypoints.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. Waypoints off\n" +
                   "  2. Waypoints on\n" +
                   "1. Turn off waypoints\n" +
                   "2. Turn on waypoints\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Waypoints", "Waypoints", "waypoints", "st-wp", "wp" };
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
                    if (Waypoints.IsEnabled)
                    {
                        Waypoints.IsEnabled = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Waypoints has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Waypoints is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!Waypoints.IsEnabled)
                    {
                        Waypoints.IsEnabled = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Waypoints has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Waypoints is already on"));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in WaypointsConsole.Execute: {0}", e));
            }
        }
    }
}