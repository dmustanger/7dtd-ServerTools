using System;
using System.Collections.Generic;

namespace ServerTools
{
    class BountiesConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Bounties.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. Bounties off\n" +
                   "  2. Bounties on\n" +
                   "1. Turn off bounties\n" +
                   "2. Turn on bounties\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Bounties", "bounties" };
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
                    Bounties.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Bounties has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    Bounties.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Bounties has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in BountiesConsole.Run: {0}.", e));
            }
        }
    }
}