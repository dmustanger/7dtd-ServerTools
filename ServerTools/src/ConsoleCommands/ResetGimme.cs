using System;
using System.Collections.Generic;

namespace ServerTools
{
    class ResetGimme : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "Reset a player's gimme status so they can receive another gimme.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. gimme reset <steamID>\n" +
                   "1. Reset the status of a steamID from the gimme list\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "gimme" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 || _params.Count > 2)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}.", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("reset"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}.", _params.Count));
                        return;
                    }
                    if (_params[1].Length != 17)
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not reset SteamId: Invalid SteamId {0}", _params[1]));
                        return;
                    }
                    else
                    {
                        PersistentContainer.Instance.Players[_params[1], true].LastGimme = DateTime.Now.AddDays(-2);
                        PersistentContainer.Instance.Save();
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }


            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ResetGimme.Run: {0}.", e));
            }
        }
    }
}
