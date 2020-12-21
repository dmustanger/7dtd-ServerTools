using System;
using System.Collections.Generic;

namespace ServerTools
{
    class LotteryConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable lottery.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. Lottery off\n" +
                   "  2. Lottery on\n" +
                   "1. Turn off lottery\n" +
                   "2. Turn on lottery\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Lottery", "lott", "st-lott" };
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
                    if (Lottery.IsEnabled)
                    {
                        Lottery.IsEnabled = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Lottery has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Lottery is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!Lottery.IsEnabled)
                    {
                        Lottery.IsEnabled = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Lottery has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Lottery is already on"));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in LotteryConsole.Execute: {0}", e.Message));
            }
        }
    }
}