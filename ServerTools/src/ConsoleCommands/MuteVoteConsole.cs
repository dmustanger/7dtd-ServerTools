using System;
using System.Collections.Generic;
using System.Xml;

namespace ServerTools
{
    class MuteVoteConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Mute Vote.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. MuteVote off\n" +
                   "  2. MuteVote on\n" +
                   "1. Turn off mute vote\n" +
                   "2. Turn on mute vote\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-MuteVote", "MuteVote", "mutevote" };
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
                    if (MuteVote.IsEnabled)
                    {
                        MuteVote.IsEnabled = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Mute vote has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Mute vote is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!MuteVote.IsEnabled)
                    {
                        MuteVote.IsEnabled = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Mute vote has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Mute vote is already on"));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in MuteVoteConsole.Execute: {0}", e));
            }
        }
    }
}