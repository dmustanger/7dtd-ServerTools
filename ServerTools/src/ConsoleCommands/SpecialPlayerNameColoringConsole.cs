using System;
using System.Collections.Generic;

namespace ServerTools
{
    class SpecialPlayerNameColoringConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Special Player Name Coloring.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. SpecialPlayerNameColoring off\n" +
                   "  2. SpecialPlayerNameColoring on\n" +
                   "1. Turn off special player name coloring\n" +
                   "2. Turn on special player name coloring\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-SpecialPlayerNameColoring", "specialplayernamecoloring", "spnc" };
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
                    ChatHook.Special_Player_Name_Coloring = false;
                    SdtdConsole.Instance.Output(string.Format("Special player name coloring has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    ChatHook.Special_Player_Name_Coloring = true;
                    SdtdConsole.Instance.Output(string.Format("Special player name coloring has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in SpecialPlayerNameColoringConsole.Run: {0}.", e));
            }
        }
    }
}