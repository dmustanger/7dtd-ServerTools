using System;
using System.Collections.Generic;

namespace ServerTools
{
    class AuctionConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Auction.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. Auction off\n" +
                   "  2. Auction on\n" +
                   "1. Turn off the auction\n" +
                   "2. Turn on the auction\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Auction", "auction" };
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
                    AuctionBox.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Auction has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    AuctionBox.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Auction has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in AuctionConsole.Run: {0}.", e));
            }
        }
    }
}