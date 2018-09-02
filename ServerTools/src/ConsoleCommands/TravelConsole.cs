using System;
using System.Collections.Generic;

namespace ServerTools
{
    class TravelConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Travel.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. Travel off\n" +
                   "  2. Travel on\n" +
                   "1. Turn off travel\n" +
                   "2. Turn on travel\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Travel", "travel" };
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
                    Travel.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Travel has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    Travel.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Travel has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in TravelConsole.Run: {0}.", e));
            }
        }
    }
}