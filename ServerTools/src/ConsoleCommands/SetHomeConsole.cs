using System;
using System.Collections.Generic;

namespace ServerTools
{
    class SetHomeConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Set Home.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. Home off\n" +
                   "  2. Home on\n" +
                   "1. Turn off set home\n" +
                   "2. Turn on set home\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Home", "home" };
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
                    TeleportHome.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Set home has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    TeleportHome.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Set home has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in SetHomeConsole.Run: {0}.", e));
            }
        }
    }
}