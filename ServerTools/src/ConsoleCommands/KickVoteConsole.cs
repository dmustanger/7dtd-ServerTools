using System;
using System.Collections.Generic;
using System.Xml;

namespace ServerTools
{
    class KickVoteConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable or disable kick vote.";
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
            return new string[] { "st-KickVote", "kv", "st-kv" };
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
                    if (KickVote.IsEnabled)
                    {
                        KickVote.IsEnabled = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Kick vote has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Kick vote is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!KickVote.IsEnabled)
                    {
                        KickVote.IsEnabled = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("Kick vote has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Kick vote is already on"));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in KickVoteConsole.Execute: {0}", e.Message));
            }
        }
    }
}