using System;
using System.Collections.Generic;

namespace ServerTools
{
    class ResetGimme : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]-Reset a player's gimme status so they can receive another gimme.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. gimme reset <steamId/entityId>\n" +
                   "1. Reset the status of a Id from the gimme list\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-Gimme", "gimme" };
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
                    if (_params[1].Length < 1 || _params[1].Length > 17)
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not reset Id: Invalid Id {0}", _params[1]));
                        return;
                    }
                    if (_params[1].Length == 17)
                    {
                        Player p = PersistentContainer.Instance.Players[_params[1], false];
                        if (p.LastGimme != null)
                        {
                            PersistentContainer.Instance.Players[_params[1], true].LastGimme = DateTime.Now.AddDays(-2);
                            PersistentContainer.Instance.Save();
                            SdtdConsole.Instance.Output("Gimme delay reset.");
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Player with id {0} does not have a Gimme delay to reset.", _params[1]));
                        }
                    }
                    else
                    {
                        ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                        Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                        if (p.LastGimme != null)
                        {
                            PersistentContainer.Instance.Players[_cInfo.playerId, true].LastGimme = DateTime.Now.AddDays(-2);
                            PersistentContainer.Instance.Save();
                            SdtdConsole.Instance.Output("Gimme delay reset.");
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Player with id {0} does not have a Gimme delay to reset.", _params[1]));
                        }
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
