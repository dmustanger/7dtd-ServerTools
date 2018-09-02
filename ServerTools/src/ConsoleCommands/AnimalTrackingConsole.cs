using System;
using System.Collections.Generic;

namespace ServerTools
{
    class AnimalTrackingConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Animal Tracking.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. AnimalTracking off\n" +
                   "  2. AnimalTracking on\n" +
                   "1. Turn off animal tracking\n" +
                   "2. Turn on animal tracking\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-AnimalTracking", "animaltracking" };
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
                    Animals.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Animals has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    Animals.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Animals has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in AnimalTrackingConsole.Run: {0}.", e));
            }
        }
    }
}