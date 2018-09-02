using System;
using System.Collections.Generic;

namespace ServerTools
{
    class KickVoteConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Kick Vote.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. KickVote off\n" +
                   "  2. KickVote on\n" +
                   "1. Turn off kick vote\n" +
                   "2. Turn on kick vote\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-KickVote", "kickvote" };
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
                    KickVote.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Kick vote has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    KickVote.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Kick vote has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in KickVoteConsole.Run: {0}.", e));
            }
        }
    }
}