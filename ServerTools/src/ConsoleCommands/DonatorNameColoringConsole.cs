using System;
using System.Collections.Generic;

namespace ServerTools
{
    class DonatorNameColoringConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Death Spot.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. DonatorNameColoring off\n" +
                   "  2. DonatorNameColoring on\n" +
                   "1. Turn off donator name coloring and prefix tags\n" +
                   "2. Turn on donator name coloring and prefix tags\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-DeathSpot", "deathspot" };
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
                    ChatHook.Donator_Name_Coloring = false;
                    SdtdConsole.Instance.Output(string.Format("Donator name coloring has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    ChatHook.Donator_Name_Coloring = true;
                    SdtdConsole.Instance.Output(string.Format("Donator name coloring has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in DonatorNameColoringConsole.Run: {0}.", e));
            }
        }
    }
}