using System;
using System.Collections.Generic;

namespace ServerTools
{
    class EntityCleanupConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable entity cleanup.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-ec off\n" +
                   "  2. st-ec on\n" +
                   "1. Turn off entity cleanup\n" +
                   "2. Turn on entity cleanup\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-EntityCleanup", "ec", "st-ec" };
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
                    if (EntityCleanup.IsEnabled)
                    {
                        EntityCleanup.IsEnabled = false;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Entity cleanup has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Entity cleanup is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!EntityCleanup.IsEnabled)
                    {
                        EntityCleanup.IsEnabled = true;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Entity cleanup has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Entity cleanup is already on"));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in EntityCleanupConsole.Execute: {0}", e.Message));
            }
        }
    }
}