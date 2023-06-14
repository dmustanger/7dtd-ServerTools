using System;
using System.Collections.Generic;

namespace ServerTools
{
    class PlayerStatCheckConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Enable or disable player stat check.";
        }
        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-psc off\n" +
                   "  2. st-psc on\n" +
                   "1. Turn off player stat check\n" +
                   "2. Turn on player stat check\n";
        }
        protected override string[] getCommands()
        {
            return new string[] { "st-PlayerStatCheck", "psc", "st-psc" };
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
                    if (PlayerStats.IsEnabled)
                    {
                        PlayerStats.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Player stat check has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Player stat check is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!PlayerStats.IsEnabled)
                    {
                        PlayerStats.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Player stat check has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Player stat check is already on"));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in PlayerStatCheckConsole.Execute: {0}", e.Message));
            }
        }
    }
}