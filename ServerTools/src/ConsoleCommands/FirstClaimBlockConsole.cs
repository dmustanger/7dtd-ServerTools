using System;
using System.Collections.Generic;

namespace ServerTools
{
    class FirstClaimBlockConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable First Claim Block.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. FirstClaimBlock off\n" +
                   "  2. FirstClaimBlock on\n" +
                   "1. Turn off first claim block\n" +
                   "2. Turn on first claim block\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-FirstClaimBlock", "firstclaimblock" };
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
                    FirstClaimBlock.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("First claim block has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    FirstClaimBlock.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("First claim block has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in FirstClaimBlockConsole.Run: {0}.", e));
            }
        }
    }
}