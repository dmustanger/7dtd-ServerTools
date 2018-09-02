using System;
using System.Collections.Generic;

namespace ServerTools
{
    class RestartVoteConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Restart vote.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. RestartVote off\n" +
                   "  2. RestartVote on\n" +
                   "1. Turn off restart vote\n" +
                   "2. Turn on restart vote\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-RestartVote", "restartvote" };
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
                    RestartVote.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Restart vote has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    RestartVote.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Restart vote has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in RestartVoteConsole.Run: {0}.", e));
            }
        }
    }
}