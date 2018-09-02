using System;
using System.Collections.Generic;

namespace ServerTools
{
    class ChatFloodProtectionConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Chat Flood Protection.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. ChatFloodProtection off\n" +
                   "  2. ChatFloodProtection on\n" +
                   "1. Turn off chat flood protection\n" +
                   "2. Turn on chat flood protection\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-ChatFloodProtection", "chatfloodprotection" };
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
                    ChatHook.ChatFlood = false;
                    SdtdConsole.Instance.Output(string.Format("Chat flood protection has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    ChatHook.ChatFlood = true;
                    SdtdConsole.Instance.Output(string.Format("Chat flood protection has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ChatFloodProtectionConsole.Run: {0}.", e));
            }
        }
    }
}