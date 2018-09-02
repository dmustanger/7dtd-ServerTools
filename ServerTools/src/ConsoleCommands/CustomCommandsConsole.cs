using System;
using System.Collections.Generic;

namespace ServerTools
{
    class CustomCommandsConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Custom Commands.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. CustomCommands off\n" +
                   "  2. CustomCommands on\n" +
                   "1. Turn off custom commands\n" +
                   "2. Turn on custom commands\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-CustomCommands", "customcommands" };
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
                    CustomCommands.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Custom commands has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    CustomCommands.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Custom commands has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CustomCommandsConsole.Run: {0}.", e));
            }
        }
    }
}