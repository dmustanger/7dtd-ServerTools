using System;
using System.Collections.Generic;

namespace ServerTools
{
    class FlightCheckConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Flight Check.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. FlightCheck off\n" +
                   "  2. FlightCheck on\n" +
                   "1. Turn off flight check\n" +
                   "2. Turn on flight check\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-FlightCheck", "flightcheck" };
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
                    FlightCheck.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Flight check has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    FlightCheck.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Flight check has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in FlightCheckConsole.Run: {0}.", e));
            }
        }
    }
}