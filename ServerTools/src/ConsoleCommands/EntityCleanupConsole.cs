using System;
using System.Collections.Generic;

namespace ServerTools
{
    class EntityCleanupConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Entity Cleanup.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. EntityCleanup off\n" +
                   "  2. EntityCleanup on\n" +
                   "1. Turn off entity cleanup\n" +
                   "2. Turn on entity cleanup\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-EntityCleanup", "entitycleanup" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    EntityCleanup.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Entity cleanup has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    EntityCleanup.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Entity cleanup has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in EntityCleanupConsole.Run: {0}.", e));
            }
        }
    }
}