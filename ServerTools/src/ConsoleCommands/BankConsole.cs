using System;
using System.Collections.Generic;

namespace ServerTools
{
    class BankConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Bad Word Filter.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. Bank off\n" +
                   "  2. Bank on\n" +
                   "1. Turn off the bank\n" +
                   "2. Turn on the bank\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Bank", "bank" };
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
                    Bank.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Bank has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    Bank.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Bank has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in BankConsole.Run: {0}.", e));
            }
        }
    }
}