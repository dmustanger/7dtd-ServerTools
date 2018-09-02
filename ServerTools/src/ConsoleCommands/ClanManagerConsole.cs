using System;
using System.Collections.Generic;

namespace ServerTools
{
    class ClanManagerConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Clan Manager.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. ClanManager off\n" +
                   "  2. ClanManager on\n" +
                   "1. Turn off clan manager\n" +
                   "2. Turn on clan manager\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-ClanManager", "clanmanager" };
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
                    ClanManager.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Clan manager has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    ClanManager.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Clan manager has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ClanManagerConsole.Run: {0}.", e));
            }
        }
    }
}