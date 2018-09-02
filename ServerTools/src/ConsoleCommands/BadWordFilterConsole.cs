using System;
using System.Collections.Generic;

namespace ServerTools
{
    class BadWordFilterConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Bad Word Filter.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. BadWordFilter off\n" +
                   "  2. BadWordFilter on\n" +
                   "1. Turn off the bad word filter\n" +
                   "2. Turn on the bad word filter\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-BadWordFilter", "badwordfilter" };
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
                    Badwords.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Bad word filter has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    Badwords.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Bad word filter has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in BadWordFilterConsole.Run: {0}.", e));
            }
        }
    }
}