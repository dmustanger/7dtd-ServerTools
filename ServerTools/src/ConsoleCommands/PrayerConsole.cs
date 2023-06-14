using System;
using System.Collections.Generic;

namespace ServerTools
{
    class PrayerConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Enable or disable prayer.";
        }
        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-pray off\n" +
                   "  2. st-pray on\n" +
                   "1. Turn off prayer\n" +
                   "2. Turn on prayer\n";
        }
        protected override string[] getCommands()
        {
            return new string[] { "st-Prayer", "pray", "st-pray" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (Prayer.IsEnabled)
                    {
                        Prayer.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Prayer has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Prayer is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (Prayer.IsEnabled)
                    {
                        Prayer.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Prayer has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Prayer is already on"));
                        return;
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in PrayerConsole.Execute: {0}", e.Message));
            }
        }
    }
}