using System;
using System.Collections.Generic;
using ServerTools.AntiCheat;

namespace ServerTools
{
    class InvalidItemKickerConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable invalid item kicker.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. InvalidItemKicker off\n" +
                   "  2. InvalidItemKicker on\n" +
                   "1. Turn off invalid item kicker\n" +
                   "2. Turn on invalid item kicker\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-InvalidItemKicker", "iik", "st-iik" };
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
                    if (InvalidItems.IsEnabled)
                    {
                        InvalidItems.IsEnabled = false;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid item kicker has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid item kicker is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!InvalidItems.IsEnabled)
                    {
                        InvalidItems.IsEnabled = true;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid item kicker has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid item kicker is already on"));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in InvalidItemKickerConsole.Execute: {0}", e.Message));
            }
        }
    }
}