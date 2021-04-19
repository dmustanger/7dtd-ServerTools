using System;
using System.Collections.Generic;

namespace ServerTools
{
    class StartingItemsConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable starting items.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. StartingItems off\n" +
                   "  2. StartingItems on\n" +
                   "1. Turn off starting items\n" +
                   "2. Turn on starting items\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-StartingItems", "sti", "st-sti" };
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
                    if (StartingItems.IsEnabled)
                    {
                        StartingItems.IsEnabled = false;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Starting items has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Starting items is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!StartingItems.IsEnabled)
                    {
                        StartingItems.IsEnabled = true;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Starting items has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Starting items is already on"));
                        return;
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[0]));
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in StartingItemsConsole.Execute: {0}", e.Message));
            }
        }
    }
}