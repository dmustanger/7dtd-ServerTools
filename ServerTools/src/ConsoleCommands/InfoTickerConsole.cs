using System;
using System.Collections.Generic;

namespace ServerTools
{
    class InfoTickerConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable InfoTicker.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. InfoTicker off\n" +
                   "  2. InfoTicker on\n" +
                   "1. Turn off info ticker\n" +
                   "2. Turn on info ticker\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-InfoTicker", "infoticker" };
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
                    InfoTicker.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Info ticker has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    InfoTicker.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Info ticker has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in InfoTickerConsole.Run: {0}.", e));
            }
        }
    }
}