using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ServerTools
{
    class BankConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Bank.";
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
                    if (Bank.IsEnabled)
                    {
                        Bank.IsEnabled = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Bank has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Bank is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!Bank.IsEnabled)
                    {
                        Bank.IsEnabled = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Bank has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Bank is already on"));
                        return;
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in BankConsole.Execute: {0}", e));
            }
        }
    }
}