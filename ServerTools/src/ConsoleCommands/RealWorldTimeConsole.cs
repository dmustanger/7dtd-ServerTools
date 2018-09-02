using System;
using System.Collections.Generic;

namespace ServerTools
{
    class RealWorldTimeConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Real World Time.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. RealWorldTime off\n" +
                   "  2. RealWorldTime on\n" +
                   "1. Turn off real world time\n" +
                   "2. Turn on real world time\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-RealWorldTime", "realworldtime" };
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
                    RealWorldTime.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Real world time has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    RealWorldTime.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Real world time has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in RealWorldTimeConsole.Run: {0}.", e));
            }
        }
    }
}