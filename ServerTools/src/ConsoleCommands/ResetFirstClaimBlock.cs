using System;
using System.Collections.Generic;
using System.Data;

namespace ServerTools
{
    class ResetFirstClaimBlock : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]-Reset a players first claim block status.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. firstclaimblock reset <steamId/entityId>\n" +
                   "1. Reset the status of a players first claim block\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-FirstClaimBlock", "firstclaimblock", "fcb" };
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
                    ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                    if (_cInfo != null)
                    {
                        string _sql = string.Format("SELECT firstClaim FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                        DataTable _result = SQL.TQuery(_sql);
                        bool _firstClaim;
                        bool.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _firstClaim);
                        _result.Dispose();
                        if (_firstClaim)
                        {
                            _sql = string.Format("UPDATE Players SET firstClaim = 'false' WHERE steamid = '{0}'", _cInfo.playerId);
                            SQL.FastQuery(_sql);
                            SdtdConsole.Instance.Output("Players first claim block reset.");
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Player with id {0} does not have a first claim block to reset.", _params[1]));
                        }
                    }
                    else if (_params[1].Length == 17)
                    {
                        string _id = SQL.EscapeString(_params[1]);
                        string _sql = string.Format("SELECT firstClaim FROM Players WHERE steamid = '{0}'", _id);
                        DataTable _result = SQL.TQuery(_sql);
                        bool _firstClaim;
                        bool.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _firstClaim);
                        _result.Dispose();
                        if (_firstClaim)
                        {
                            _sql = string.Format("UPDATE Players SET firstClaim = 'false' WHERE steamid = '{0}'", _id);
                            SQL.FastQuery(_sql);
                            SdtdConsole.Instance.Output("Players first claim block reset.");
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Player with id {0} does not have a first claim block to reset.", _params[1]));
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not reset Id: Invalid Id {0}.", _params[1]));
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ResetFirstClaimBlock.Run: {0}.", e));
            }
        }
    }
}
