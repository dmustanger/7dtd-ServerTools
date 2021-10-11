using System;
using System.Collections.Generic;

namespace ServerTools
{
    class HighPingKickerConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable high ping kicker.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-hpk off\n" +
                   "  2. st-hpk on\n" +
                   "1. Turn off high ping kicker\n" +
                   "2. Turn on high ping kicker\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-HighPingKicker", "hpk", "st-hpk" };
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
                    if (HighPingKicker.IsEnabled)
                    {
                        HighPingKicker.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] High ping kicker has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] High ping kicker is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!HighPingKicker.IsEnabled)
                    {
                        HighPingKicker.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] High ping kicker has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] High ping kicker is already on"));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in HighPingKickerConsole.Execute: {0}", e.Message));
            }
        }
    }
}