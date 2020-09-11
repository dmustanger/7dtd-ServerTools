using System;
using System.Collections.Generic;

namespace ServerTools
{
    class HardcoreConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable hardcore.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. Hardcore off\n" +
                   "  2. Hardcore on\n" +
                   "1. Turn off hardcore\n" +
                   "2. Turn on hardcore\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Hardcore", "hc", "st-hc" };
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
                    if (Hardcore.IsEnabled)
                    {
                        Hardcore.IsEnabled = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Hardcore has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Hardcore is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!Hardcore.IsEnabled)
                    {
                        Hardcore.IsEnabled = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Hardcore has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Hardcore is already on"));
                        return;
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in HardcoreConsole.Execute: {0}", e.Message));
            }
        }
    }
}