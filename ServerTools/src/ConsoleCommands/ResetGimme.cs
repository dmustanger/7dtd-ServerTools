using System;
using System.Collections.Generic;
using System.Data;

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
                   "  1. gimme reset <steamId/entityId/playerName>\n" +
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
                if (_params.Count != 2)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}.", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("reset"))
                {
                    if (_params[1].Length < 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not reset Id: Invalid Id {0}", _params[1]));
                        return;
                    }
                    ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                    string _id;
                    if (_cInfo != null)
                    {
                        _id = _cInfo.playerId;
                    }
                    else
                    {
                        if (_params[1].Length != 17)
                        {
                            SdtdConsole.Instance.Output(string.Format("You can only use a player id or their name if online. Can not add Id: Invalid Id {0}", _params[1]));
                            return;
                        }
                        else
                        {
                            _id = _params[1];
                        }
                            
                    }
                    string _sql = string.Format("SELECT last_gimme FROM Players WHERE steamid = '{0}'", _id);
                    DataTable _result = SQL.TQuery(_sql);
                    if (_result.Rows.Count != 0)
                    {
                        _sql = string.Format("UPDATE Players SET last_gimme = '10/29/2000 7:30:00 AM' WHERE steamid = '{0}'", _id);
                        SQL.FastQuery(_sql);
                        SdtdConsole.Instance.Output(string.Format("Gimme delay reset for steamid {0}.", _id));
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Player with id {0} does not have a Gimme delay to reset.", _params[1]));
                    }
                    _result.Dispose();
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
