using System;
using System.Collections.Generic;

namespace ServerTools
{
    class DeathSpotConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Death Spot.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. DeathSpot off\n" +
                   "  2. DeathSpot on\n" +
                   "1. Turn off death spot\n" +
                   "2. Turn on death spot\n";
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
                    DeathSpot.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Death spot has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    DeathSpot.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Death spot has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in DeathSpotConsole.Run: {0}.", e));
            }
        }
    }
}