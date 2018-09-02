using System;
using System.Collections.Generic;

namespace ServerTools
{
    class HordesConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Hordes.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. Hordes off\n" +
                   "  2. Hordes on\n" +
                   "1. Turn off extra hordes\n" +
                   "2. Turn on extra hordes\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Hordes", "hordes" };
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
                    Hordes.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Hordes has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    Hordes.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Hordes has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in HordesConsole.Run: {0}.", e));
            }
        }
    }
}