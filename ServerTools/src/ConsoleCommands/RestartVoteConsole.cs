using System;
using System.Collections.Generic;
using System.Xml;

namespace ServerTools
{
    class RestartVoteConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Enable or disable restart vote.";
        }
        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-rv off\n" +
                   "  2. st-rv on\n" +
                   "1. Turn off restart vote\n" +
                   "2. Turn on restart vote\n";
        }
        protected override string[] getCommands()
        {
            return new string[] { "st-RestartVote", "rv", "st-rv" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (RestartVote.IsEnabled)
                    {
                        RestartVote.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Restart vote has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Restart vote is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!RestartVote.IsEnabled)
                    {
                        RestartVote.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Restart vote has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Restart vote is already on"));
                        return;
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in RestartVoteConsole.Execute: {0}", e.Message));
            }
        }
    }
}