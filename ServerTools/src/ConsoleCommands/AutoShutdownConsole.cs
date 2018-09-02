using System;
using System.Collections.Generic;

namespace ServerTools
{
    class AutoShutdownConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Auto Shutdown.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. AutoShutdown off\n" +
                   "  2. AutoShutdown on\n" +
                   "1. Turn off the world auto shutdown\n" +
                   "2. Turn on the world auto shutdown\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-AutoShutdown", "autoshutdown" };
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
                    AutoShutdown.IsEnabled = false;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    AutoShutdown.IsEnabled = true;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in AutoShutdownConsole.Run: {0}.", e));
            }
        }
    }
}