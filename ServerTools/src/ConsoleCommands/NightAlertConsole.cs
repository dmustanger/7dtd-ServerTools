using System;
using System.Collections.Generic;

namespace ServerTools
{
    class NightAlertConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable night alert.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. NightAlert off\n" +
                   "  2. NightAlert on\n" +
                   "1. Turn off night alert\n" +
                   "2. Turn on night alert\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-NightAlert", "na", "st-na" };
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
                    if (NightAlert.IsEnabled)
                    {
                        NightAlert.IsEnabled = false;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Night alert has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Night alert is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (NightAlert.IsEnabled)
                    {
                        NightAlert.IsEnabled = true;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Night alert has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Night alert is already on"));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in NightAlertConsole.Execute: {0}", e.Message));
            }
        }
    }
}