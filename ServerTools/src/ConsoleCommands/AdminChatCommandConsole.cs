using System;
using System.Collections.Generic;

namespace ServerTools
{
    class AdminChatCommandsConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Admin Chat Commands.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. AdminChatCommands off\n" +
                   "  2. AdminChatCommands on\n" +
                   "1. Turn off server admin chat commands\n" +
                   "2. Turn on server admin chat commands\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-AdminChatCommands", "AdminChatCommands", "adminchatcommands", "st-acc", "acc" };
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
                    if (AdminChat.IsEnabled)
                    {
                        AdminChat.IsEnabled = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Admin chat commands has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Admin chat is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!AdminChat.IsEnabled)
                    {
                        AdminChat.IsEnabled = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Admin chat commands has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Admin chat is already on"));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in AdminChatCommandsConsole.Execute: {0}", e));
            }
        }
    }
}
