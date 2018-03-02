using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class ResetHome : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]-Reset a player's vote reward status so they can receive another reward.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. home reset <steamId/entityId>\n" +
                   "1. Reset the delay of the players /home and /home2 command delays\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-Home", "home" };
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
                    ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                    Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                    if (p.LastSetHome != null)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].LastSetHome = DateTime.Now.AddDays(-2);
                        PersistentContainer.Instance.Save();
                        SdtdConsole.Instance.Output("Players chat command /home and /home2 delay reset.");
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Player with id {0} does not have a Home delay to reset.", _params[1]));
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ResetHome.Run: {0}.", e));
            }
        }
    }
}