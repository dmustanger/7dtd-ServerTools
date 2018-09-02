using System;
using System.Collections.Generic;

namespace ServerTools
{
    class GimmeConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Gimme.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. Gimme off\n" +
                   "  2. Gimme on\n" +
                   "1. Turn off gimme\n" +
                   "2. Turn on gimme\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Gimme", "gimme" };
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
                    Gimme.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Gimme has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    Gimme.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Gimme has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in GimmeConsole.Run: {0}.", e));
            }
        }
    }
}