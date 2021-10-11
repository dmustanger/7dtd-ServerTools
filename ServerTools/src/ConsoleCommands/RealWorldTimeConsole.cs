using System;
using System.Collections.Generic;

namespace ServerTools
{
    class RealWorldTimeConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable real world time.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-rwt off\n" +
                   "  2. st-rwt on\n" +
                   "1. Turn off real world time\n" +
                   "2. Turn on real world time\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-RealWorldTime", "rwt", "st-rwt" };
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
                    if (RealWorldTime.IsEnabled)
                    {
                        RealWorldTime.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Real world time has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Real world time is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!RealWorldTime.IsEnabled)
                    {
                        RealWorldTime.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Real world time has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Real world time is already on"));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in RealWorldTimeConsole.Execute: {0}", e.Message));
            }
        }
    }
}