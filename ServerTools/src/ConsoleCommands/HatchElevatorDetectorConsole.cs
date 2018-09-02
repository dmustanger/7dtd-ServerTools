using System;
using System.Collections.Generic;

namespace ServerTools
{
    class HatchElevatorDetectorConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Hatch Elevator Detector.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. HatchElevator off\n" +
                   "  2. HatchElevator on\n" +
                   "1. Turn off hatch elevator detector\n" +
                   "2. Turn on hatch elevator detector\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-HatchElevator", "hatchelevator" };
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
                    HatchElevator.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Hatch elevator has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    HatchElevator.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Hatch elevator has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in HatchElevatorDetectorConsole.Run: {0}.", e));
            }
        }
    }
}