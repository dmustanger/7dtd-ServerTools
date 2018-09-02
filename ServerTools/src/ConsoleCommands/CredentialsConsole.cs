using System;
using System.Collections.Generic;

namespace ServerTools
{
    class CredentialsConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Credentials.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. Credentials off\n" +
                   "  2. Credentials on\n" +
                   "1. Turn off all credential check tools\n" +
                   "2. Turn on all credential check tools\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Credentials", "credentials" };
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
                    CredentialCheck.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Credential check has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    CredentialCheck.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Credential check has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CredentialsConsole.Run: {0}.", e));
            }
        }
    }
}