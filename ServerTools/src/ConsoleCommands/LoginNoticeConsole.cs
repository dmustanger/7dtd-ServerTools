using System;
using System.Collections.Generic;

namespace ServerTools
{
    class LoginNoticeConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Login Notice.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. LoginNotice off\n" +
                   "  2. LoginNotice on\n" +
                   "1. Turn off login notice\n" +
                   "2. Turn on login notice\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-LoginNotice", "loginnotice" };
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
                    LoginNotice.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Login notice has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    LoginNotice.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Login notice has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in LoginNoticeConsole.Run: {0}.", e));
            }
        }
    }
}