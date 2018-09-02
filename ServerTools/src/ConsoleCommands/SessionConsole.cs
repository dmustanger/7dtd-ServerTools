using System;
using System.Collections.Generic;

namespace ServerTools
{
    class SessionConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Session.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. Session off\n" +
                   "  2. Session on\n" +
                   "1. Turn off session\n" +
                   "2. Turn on session\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Session", "session" };
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
                    Session.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Session has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    Session.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Session has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in RestartVoteConsole.Run: {0}.", e));
            }
        }
    }
}