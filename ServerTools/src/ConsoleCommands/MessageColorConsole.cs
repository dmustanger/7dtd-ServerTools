using System;
using System.Collections.Generic;
using System.Xml;

namespace ServerTools
{
    class MessageColorConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Message Color.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. MessageColor off\n" +
                   "  2. MessageColor on\n" +
                   "1. Turn off message color\n" +
                   "2. Turn on message color\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-MessageColor", "messagecolor", "st-mc", "mc" };
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
                    if (ChatHook.Message_Color_Enabled)
                    {
                        ChatHook.Message_Color_Enabled = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Message color has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Message color is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!ChatHook.Message_Color_Enabled)
                    {
                        ChatHook.Message_Color_Enabled = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Message_Color has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Message_Color is already on"));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in MotdConsole.Execute: {0}", e));
            }
        }
    }
}