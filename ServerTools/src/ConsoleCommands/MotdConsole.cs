using System;
using System.Collections.Generic;

namespace ServerTools
{
    class MotdConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Motd(messsage of the day).";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. Motd off\n" +
                   "  2. Motd on\n" +
                   "1. Turn off the motd\n" +
                   "2. Turn on the motd\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Motd", "motd" };
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
                    Motd.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Market has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    Motd.IsEnabled = true;
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
                Log.Out(string.Format("[SERVERTOOLS] Error in MotdConsole.Run: {0}.", e));
            }
        }
    }
}