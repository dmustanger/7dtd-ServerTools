using System;
using System.Collections.Generic;

namespace ServerTools
{
    class HighPingKickerConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable High Ping Kicker.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. HighPingKicker off\n" +
                   "  2. HighPingKicker on\n" +
                   "1. Turn off high ping kicker\n" +
                   "2. Turn on high ping kicker\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-HighPingKicker", "highpingkicker" };
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
                    HighPingKicker.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("High ping kicker has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    HighPingKicker.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("High ping kicker has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in HighPingKickerConsole.Run: {0}.", e));
            }
        }
    }
}