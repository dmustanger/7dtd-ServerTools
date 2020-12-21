using System;
using System.Collections.Generic;

namespace ServerTools
{
    class StuckConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable stuck.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. Stuck off\n" +
                   "  2. Stuck on\n" +
                   "1. Turn off stuck\n" +
                   "2. Turn on stuck\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Stuck", "stk", "st-stk" };
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
                    if (Stuck.IsEnabled)
                    {
                        Stuck.IsEnabled = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Stuck has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Stuck is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!Stuck.IsEnabled)
                    {
                        Stuck.IsEnabled = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Stuck has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Stuck is already on"));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in StuckConsole.Execute: {0}", e.Message));
            }
        }
    }
}