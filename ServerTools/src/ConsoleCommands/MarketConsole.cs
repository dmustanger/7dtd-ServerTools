using System;
using System.Collections.Generic;

namespace ServerTools
{
    class MarketConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Market.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. Market off\n" +
                   "  2. Market on\n" +
                   "1. Turn off the market\n" +
                   "2. Turn on the market\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Market", "market" };
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
                    MarketChat.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Market has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    MarketChat.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Market has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in MarketConsole.Run: {0}.", e));
            }
        }
    }
}