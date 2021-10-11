using System;
using System.Collections.Generic;

namespace ServerTools
{
    class CredentialsConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable credential checks when players join.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-creds off\n" +
                   "  2. st-creds on\n" +
                   "1. Turn off all credential check tools\n" +
                   "2. Turn on all credential check tools\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-Credentials", "creds", "st-creds" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (Credentials.IsEnabled)
                    {
                        Credentials.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Credentials has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Credentials is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!Credentials.IsEnabled)
                    {
                        Credentials.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Credentials has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Credentials is already on"));
                        return;
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CredentialsConsole.Execute: {0}", e.Message));
            }
        }
    }
}