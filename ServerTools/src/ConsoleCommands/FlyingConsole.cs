using ServerTools.AntiCheat;
using System;
using System.Collections.Generic;

namespace ServerTools
{
    class FlyingConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable, disable flying detection.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-fly off\n" +
                   "  2. st-fly on\n" +
                   "1. Turn off flight detection\n" +
                   "2. Turn on flight detection\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Flying", "fly", "st-fly" };
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
                    if (PlayerChecks.FlyEnabled)
                    {
                        PlayerChecks.FlyEnabled = false;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Flying has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Flying is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!PlayerChecks.FlyEnabled)
                    {
                        PlayerChecks.FlyEnabled = true;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Flying has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Flying is already on"));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in FlyingConsole.Execute: {0}", e.Message));
            }
        }
    }
}