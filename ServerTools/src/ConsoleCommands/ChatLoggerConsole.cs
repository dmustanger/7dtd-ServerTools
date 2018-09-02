using System;
using System.Collections.Generic;

namespace ServerTools
{
    class ChatLoggerConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Chat Log.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. ChatLog off\n" +
                   "  2. ChatLog on\n" +
                   "1. Turn off chat log\n" +
                   "2. Turn on chat log\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-ChatLog", "chatLog" };
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
                    ChatLog.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Chat log has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    ChatLog.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Chat log has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ChatLoggerConsole.Run: {0}.", e));
            }
        }
    }
}