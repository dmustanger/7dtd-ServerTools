using System;
using System.Collections.Generic;

namespace ServerTools
{
    class SuicideConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Suicide.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. Suicide off\n" +
                   "  2. Suicide on\n" +
                   "1. Turn off suicide\n" +
                   "2. Turn on suicide\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Suicide", "suicide" };
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
                    Suicide.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Suicide has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    Suicide.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Suicide has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in SuicideConsole.Run: {0}.", e));
            }
        }
    }
}