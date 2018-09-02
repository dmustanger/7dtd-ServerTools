using System;
using System.Collections.Generic;

namespace ServerTools
{
    class WorldRadiusConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable World Radius.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. WorldRadius off\n" +
                   "  2. WorldRadius on\n" +
                   "1. Turn off world radius\n" +
                   "2. Turn on world radius\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-WorldRadius", "worldradius" };
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
                    WorldRadius.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("World radius has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    WorldRadius.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("World radius has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WorldRadiusConsole.Run: {0}.", e));
            }
        }
    }
}