using System;
using System.Collections.Generic;

namespace ServerTools
{
    class BadWordFilterConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable bad word filter.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. bwf off\n" +
                   "  2. bwf on\n" +
                   "1. Turn off the bad word filter\n" +
                   "2. Turn on the bad word filter\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-BadWordFilter", "bwf", "st-bwf" };
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
                    if (Badwords.IsEnabled)
                    {
                        Badwords.IsEnabled = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Bad word filter has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Bad word filter is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!Badwords.IsEnabled)
                    {
                        Badwords.IsEnabled = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Bad word filter has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Bad word filter is already on"));
                        return;
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in BadWordFilterConsole.Execute: {0}", e));
            }
        }
    }
}