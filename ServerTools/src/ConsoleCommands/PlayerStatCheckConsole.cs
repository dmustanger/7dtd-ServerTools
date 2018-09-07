using System;
using System.Collections.Generic;

namespace ServerTools
{
    class PlayerStatCheckConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Player Stat Check.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. PlayerStatCheck off\n" +
                   "  2. PlayerStatCheck on\n" +
                   "1. Turn off player stat check\n" +
                   "2. Turn on player stat check\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-PlayerStatCheck", "playerstatcheck", "psc" };
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
                    PlayerStatCheck.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Player stat check has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    PlayerStatCheck.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Player stat check has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PlayerStatCheckConsole.Run: {0}.", e));
            }
        }
    }
}