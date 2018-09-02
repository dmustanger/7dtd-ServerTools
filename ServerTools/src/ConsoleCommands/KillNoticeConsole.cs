using System;
using System.Collections.Generic;

namespace ServerTools
{
    class KillNoticeConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Kill Notice.";
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
            return new string[] { "st-KillNotice", "killnotice" };
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
                    KillNotice.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Kill notice has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    KillNotice.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Kill notice has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in KillNoticeConsole.Run: {0}.", e));
            }
        }
    }
}