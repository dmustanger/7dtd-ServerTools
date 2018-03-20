using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class ResetWeatherVote : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]-Reset the delay for players to start a new weather vote.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. weathervote reset\n" +
                   "1. Resets the delay between weather votes\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-WeatherVote", "weathervote", "wv" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}.", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("reset"))
                {
                    WeatherVote.VoteNew = true;
                    SdtdConsole.Instance.Output("Weather vote delay reset.");
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }


            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ResetWeatherVote.Run: {0}.", e));
            }
        }
    }
}
