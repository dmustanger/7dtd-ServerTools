using System;
using System.Collections.Generic;

namespace ServerTools
{
    class AdminNameColoringConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Admin Name Coloring.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. AdminNameColoring off\n" +
                   "  2. AdminNameColoring on\n" +
                   "1. Turn off your admin chat and name colors\n" +
                   "2. Turn on your admin chat and name colors\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-AdminNameColoring", "adminnamecoloring" };
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
                    ChatHook.Admin_Name_Coloring = false;
                    SdtdConsole.Instance.Output(string.Format("Admin name coloring has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    ChatHook.Admin_Name_Coloring = true;
                    SdtdConsole.Instance.Output(string.Format("Admin name coloring set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in AdminNameColoringConsole.Run: {0}.", e));
            }
        }
    }
}