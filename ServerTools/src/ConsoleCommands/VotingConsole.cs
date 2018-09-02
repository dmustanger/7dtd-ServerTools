using System;
using System.Collections.Generic;

namespace ServerTools
{
    class VotingConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Voting.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. Voting off\n" +
                   "  2. Voting on\n" +
                   "1. Turn off undergroundcheck\n" +
                   "2. Turn on undergroundcheck\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-UndergroundCheck", "undergroundcheck" };
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
                    VoteReward.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Underground check has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    VoteReward.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Underground check has been set to on"));
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in VotingConsole.Run: {0}.", e));
            }
        }
    }
}