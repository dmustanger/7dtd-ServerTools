using System;
using System.Collections.Generic;
using System.Xml;

namespace ServerTools
{
    class PlayerLogsConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Player Logs.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. PlayerLogs off\n" +
                   "  2. PlayerLogs on\n" +
                   "1. Turn off player logs\n" +
                   "2. Turn on player logs\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-PlayerLogs", "playerlogs" };
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
                    if (PlayerLogs.IsEnabled)
                    {
                        PlayerLogs.IsEnabled = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Player logs has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Player logs is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!PlayerLogs.IsEnabled)
                    {
                        PlayerLogs.IsEnabled = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Player logs has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Player logs is already on"));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in PlayerLogsConsole.Execute: {0}", e));
            }
        }
    }
}