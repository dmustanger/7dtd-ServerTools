using System;
using System.Collections.Generic;

namespace ServerTools
{
    class DupeLogConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable dupe log.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-dl off\n" +
                   "  2. st-dl on\n" +
                   "1. Turn off the dupe log\n" +
                   "2. Turn on the dupe log\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-DupeLog", "dl", "st-dl" };
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
                    if (DupeLog.IsEnabled)
                    {
                        DupeLog.IsEnabled = false;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Dupe log has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Dupe log is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!DupeLog.IsEnabled)
                    {
                        DupeLog.IsEnabled = true;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Dupe log has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Dupe log is already on"));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in DupeLogConsole.Execute: {0}", e.Message));
            }
        }
    }
}