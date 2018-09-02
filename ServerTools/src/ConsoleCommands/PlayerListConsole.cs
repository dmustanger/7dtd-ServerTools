using System;
using System.Collections.Generic;

namespace ServerTools
{
    class PlayerListConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Player List.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. PlayerList off\n" +
                   "  2. PlayerList on\n" +
                   "1. Turn off player list\n" +
                   "2. Turn on player list\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-PlayerList", "playerlist" };
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
                    PlayerList.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Player list has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    PlayerList.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Player list has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PlayerListConsole.Run: {0}.", e));
            }
        }
    }
}