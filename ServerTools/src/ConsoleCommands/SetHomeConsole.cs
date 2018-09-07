using System;
using System.Collections.Generic;
using System.Data;

namespace ServerTools
{
    class SetHomeConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable or Disable Set Home.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. Home off\n" +
                   "  2. Home on\n" +
                   "  3. Home reset <steamId/entityId>\n" +
                   "1. Turn off set home\n" +
                   "2. Turn on set home\n" +
                   "3. Reset the delay of the player's /home and /home2 command delays\n";
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
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1 or 2, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    TeleportHome.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Set home has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    TeleportHome.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Set home has been set to on"));
                    return;
                }
                else if (_params[0].ToLower().Equals("reset"))
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
                    string _sql = string.Format("SELECT lastsethome FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                    DataTable _result = SQL.TQuery(_sql);
                    DateTime _lastsethome;
                    DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(2).ToString(), out _lastsethome);
                    _result.Dispose();
                    if (_lastsethome.ToString() != "10/29/2000 7:30:00 AM")
                    {
                        _sql = string.Format("UPDATE Players SET lastsethome = '10/29/2000 7:30:00 AM' WHERE steamid = '{0}'", _cInfo.playerId);
                        SQL.FastQuery(_sql);
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
                Log.Out(string.Format("[SERVERTOOLS] Error in SetHomeConsole.Run: {0}.", e));
            }
        }
    }
}