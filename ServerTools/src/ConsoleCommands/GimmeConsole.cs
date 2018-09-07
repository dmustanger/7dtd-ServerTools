using System;
using System.Collections.Generic;
using System.Data;

namespace ServerTools
{
    class GimmeConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable, Disable, Reset Gimme.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. Gimme off\n" +
                   "  2. Gimme on\n" +
                   "  3. Gimme reset <steamId/entityId/playerName>\n" +
                   "1. Turn off gimme\n" +
                   "2. Turn on gimme\n" +
                   "3. Reset the delay of a player's gimme command\n";
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
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1 or 2, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    Gimme.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Gimme has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    Gimme.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Gimme has been set to on"));
                    return;
                }
                else if (_params[0].ToLower().Equals("reset"))
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
                Log.Out(string.Format("[SERVERTOOLS] Error in GimmeConsole.Run: {0}.", e));
            }
        }
    }
}