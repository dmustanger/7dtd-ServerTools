using System;
using System.Collections.Generic;

namespace ServerTools
{
    class KillNoticeConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable kill notice.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. KillNotice off\n" +
                   "  2. KillNotice on\n" +
                   "1. Turn off kill notice\n" +
                   "2. Turn on kill notice\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-KillNotice", "kn", "st-kn" };
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
                    if (KillNotice.IsEnabled)
                    {
                        KillNotice.IsEnabled = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Kill notice has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Kill notice is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!KillNotice.IsEnabled)
                    {
                        KillNotice.IsEnabled = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Kill notice has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Kill notice is already on"));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in KillNoticeConsole.Execute: {0}", e.Message));
            }
        }
    }
}