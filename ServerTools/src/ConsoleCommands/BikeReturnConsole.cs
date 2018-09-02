using System;
using System.Collections.Generic;

namespace ServerTools
{
    class BikeReturnConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Bike Return.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. BikeReturn off\n" +
                   "  2. BikeReturn on\n" +
                   "1. Turn off the bike return\n" +
                   "2. Turn on the bike return\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-BikeReturn", "bikereturn" };
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
                    BikeReturn.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Bike return has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    BikeReturn.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Bike return has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in BikeReturnConsole.Run: {0}.", e));
            }
        }
    }
}