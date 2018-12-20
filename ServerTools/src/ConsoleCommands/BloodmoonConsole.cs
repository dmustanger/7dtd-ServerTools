using System;
using System.Collections.Generic;

namespace ServerTools
{
    class BloodmoonConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Bloodmoon alert.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. Bloodmoon off\n" +
                   "  2. Bloodmoon on\n" +
                   "1. Turn off the bloodmoon alert\n" +
                   "2. Turn on the bloodmoon alert\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Bloodmoon", "bloodmoon" };
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
                    Bloodmoon.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Bloodmoon has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    Bloodmoon.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Bloodmoon has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in BloodmoonConsole.Run: {0}.", e));
            }
        }
    }
}