using System;
using System.Collections.Generic;

namespace ServerTools
{
    class MessageColorConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable message color.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-mc off\n" +
                   "  2. st-mc on\n" +
                   "1. Turn off message color\n" +
                   "2. Turn on message color\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-MessageColor", "mc", "st-mc" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (ChatHook.Message_Color_Enabled)
                    {
                        ChatHook.Message_Color_Enabled = false;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Message color has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Message color is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!ChatHook.Message_Color_Enabled)
                    {
                        ChatHook.Message_Color_Enabled = true;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Message color has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Message color is already on"));
                        return;
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in MessageColorConsole.Execute: {0}", e.Message));
            }
        }
    }
}