using System;
using System.Data;
using System.Collections.Generic;

namespace ServerTools
{
    public class PingImmunityConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]-Add, Remove and View steamids on the PingImmunity list.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. pingimmunity add <steamId/entityId/playerName>\n" +
                   "  2. pingimmunity remove <steamId/entityId/playerName>\n" +
                   "  3. pingimmunity list\n" +
                   "1. Adds a steam ID to the Ping Immunity list\n" +
                   "2. Removes a steam ID from the Ping Immunity list\n" +
                   "3. Lists all steam IDs that have Ping Immunity" +
                   "4. *Note* You can use the player id or name if they are online otherwise use their steam Id";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-PingImmunity", "pingimmunity", "pi" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}.", _params.Count));
                        return;
                    }
                    ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                    if (_cInfo != null)
                    {
                        string _sql = string.Format("SELECT pingimmunity FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                        DataTable _result = SQL.TQuery(_sql);
                        bool _isHighPingImmune = false;
                        if (_result.Rows.Count > 0)
                        {
                            bool.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _isHighPingImmune);
                        }
                        if (_isHighPingImmune)
                        {
                            SdtdConsole.Instance.Output(string.Format("Can not add Id. {0} is already in the Ping Immunity list.", _params[1]));
                        }
                        else
                        {
                            if (_result.Rows.Count > 0)
                            {
                                _sql = string.Format("UPDATE Players SET pingimmunity = 'true' WHERE steamid = '{0}'", _cInfo.playerId);
                            }
                            else
                            {
                                _sql = string.Format("INSERT INTO Players (steamid, pingimmunity) VALUES ('{0}', 'true')", _cInfo.playerId);
                            }
                            SQL.FastQuery(_sql);
                            SdtdConsole.Instance.Output(string.Format("Added Id {0} to the Ping Immunity list.", _params[1]));
                        }
                        _result.Dispose();
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
                            string _id = SQL.EscapeString(_params[1]);
                            string _sql = string.Format("SELECT pingimmunity FROM Players WHERE steamid = '{0}'", _id);
                            DataTable _result = SQL.TQuery(_sql);
                            bool _isHighPingImmune = false;
                            if (_result.Rows.Count > 0)
                            {
                                bool.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _isHighPingImmune);
                            }
                            if (_isHighPingImmune)
                            {
                                SdtdConsole.Instance.Output(string.Format("Can not add Id. {0} is already in the Ping Immunity list.", _params[1]));
                            }
                            else
                            {
                                if (_result.Rows.Count > 0)
                                {
                                    _sql = string.Format("UPDATE Players SET pingimmunity = 'true' WHERE steamid = '{0}'", _id);
                                }
                                else
                                {
                                    _sql = string.Format("INSERT INTO Players (steamid, pingimmunity) VALUES ('{0}', 'true')", _id);
                                }
                                SQL.FastQuery(_sql);
                                SdtdConsole.Instance.Output(string.Format("Added Id {0} to the Ping Immunity list.", _params[1]));
                            }
                            _result.Dispose();
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                    if (_cInfo != null)
                    {
                        string _sql = string.Format("SELECT pingimmunity FROM Players WHERE steamid = '{0}' AND pingimmunity = 'true'", _cInfo.playerId);
                        DataTable _result = SQL.TQuery(_sql);
                        if (_result.Rows.Count > 0)
                        {
                            _sql = string.Format("UPDATE Players SET pingimmunity = 'false' WHERE steamid = '{0}'", _cInfo.playerId);
                            SQL.FastQuery(_sql);
                            SdtdConsole.Instance.Output(string.Format("Removed Id {0} from Ping Immunity list.", _params[1]));
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Id {0} was not found.", _params[1]));
                        }
                        _result.Dispose();
                    }
                    else
                    {
                        if (_params[1].Length != 17)
                        {
                            SdtdConsole.Instance.Output(string.Format("You can only use a player id or their name if online. Can not remove Id: Invalid Id {0}", _params[1]));
                        }
                        else
                        {
                            string _id = SQL.EscapeString(_params[1]);
                            string _sql = string.Format("SELECT pingimmunity FROM Players WHERE steamid = '{0}' AND pingimmunity = 'true'", _id);
                            DataTable _result = SQL.TQuery(_sql);
                            if (_result.Rows.Count > 0)
                            {
                                _sql = string.Format("UPDATE Players SET pingimmunity = 'false' WHERE steamid = '{0}'", _id);
                                SQL.FastQuery(_sql);
                                SdtdConsole.Instance.Output(string.Format("Removed Id {0} from the Ping Immunity list.", _params[1]));
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("Id {0} was not found.", _params[1]));
                            }
                            _result.Dispose();
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}.", _params.Count));
                        return;
                    }
                    string _sql = "SELECT steamid FROM Players WHERE pingimmunity = 'true'";
                    DataTable _result = SQL.TQuery(_sql);
                    if (_result.Rows.Count < 1)
                    {
                        SdtdConsole.Instance.Output("There are no Ids on the Ping Immunity list.");
                    }
                    else
                    {
                        foreach (DataRow row in _result.Rows)
                        {
                            SdtdConsole.Instance.Output(string.Format("{0}", row[0]));
                        }
                        SdtdConsole.Instance.Output(string.Format("Total: {0}", _result.Rows.Count.ToString()));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in PingImmunityCommandConsole.Run: {0}.", e));
            }
        }
    }
}