using System;
using System.Collections.Generic;
using System.Xml;

namespace ServerTools
{
    class PrivateMessageConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable private message.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. PrivateMessage off\n" +
                   "  2. PrivateMessage on\n" +
                   "1. Turn off private message\n" +
                   "2. Turn on private message\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-PrivateMessage", "pm", "st-pm" };
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
                    if (Whisper.IsEnabled)
                    {
                        Whisper.IsEnabled = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Private message has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Private message is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (Whisper.IsEnabled)
                    {
                        Whisper.IsEnabled = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Private message has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Private message is already on"));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in PrivateMessageConsole.Execute: {0}", e.Message));
            }
        }
    }
}