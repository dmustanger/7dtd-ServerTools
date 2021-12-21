using System;
using System.Collections.Generic;

namespace ServerTools
{
    class ChatFloodProtectionConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable chat flood protection.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-cfp off\n" +
                   "  2. st-cfp on\n" +
                   "1. Turn off chat flood protection\n" +
                   "2. Turn on chat flood protection\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-ChatFloodProtection", "cfp", "st-cfp" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (ChatHook.ChatFlood)
                    {
                        ChatHook.ChatFlood = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Chat flood protection has been set to off"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Chat flood protection is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!ChatHook.ChatFlood)
                    {
                        ChatHook.ChatFlood = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Chat flood protection has been set to on"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Chat flood protection is already on"));
                        return;
                    }
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ChatFloodProtectionConsole.Execute: {0}", e.Message));
            }
        }
    }
}