using System;
using System.Collections.Generic;

namespace ServerTools
{
    class WeatherVoteConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable, disable, reset weather vote.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-wv off\n" +
                   "  2. st-wv on\n" +
                   "1. Turn off weather vote\n" +
                   "2. Turn on weather vote\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-WeatherVote", "wv", "st-wv" };
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
                    if (WeatherVote.IsEnabled)
                    {
                        WeatherVote.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Weather vote has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Weather vote is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!WeatherVote.IsEnabled)
                    {
                        WeatherVote.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Weather vote has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Weather vote is already on"));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in WeatherVoteConsole.Execute: {0}", e.Message));
            }
        }
    }
}