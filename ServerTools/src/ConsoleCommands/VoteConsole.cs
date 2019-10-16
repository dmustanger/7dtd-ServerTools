using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;

namespace ServerTools
{
    class VoteConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]- Enable, Disable Voting Tool and Reset Player Vote Reward Delays.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. Vote off\n" +
                   "  2. Vote on\n" +
                   "  3. Vote reset <steamId/entityId/playerName>\n" +
                   "  4. Vote reset online\n" +
                   "  5. Vote reset all\n" +
                   "1. Turn off voting\n" +
                   "2. Turn on voting\n" +
                   "3. Reset the vote reward delay of a player Id\n" +
                   "4. Reset the vote reward delay of all online players\n" +
                   "5. Reset the vote reward delay of all online and offline players\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Vote", "vote" };
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
                    VoteReward.IsEnabled = false;
                    LoadConfig.WriteXml();
                    SdtdConsole.Instance.Output(string.Format("Vote reward has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    VoteReward.IsEnabled = true;
                    LoadConfig.WriteXml();
                    SdtdConsole.Instance.Output(string.Format("Vote reward has been set to on"));
                    return;
                }
                else if (_params[0].ToLower().Equals("reset"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    if (_params[1].ToLower().Equals("online"))
                    {
                        List<ClientInfo> ClientInfoList = ConnectionManager.Instance.Clients.List.ToList();
                        for (int i = 0; i < ClientInfoList.Count; i++)
                        {
                            ClientInfo _cInfo2 = ClientInfoList[i];
                            if (_cInfo2 != null)
                            {
                                string _sql = string.Format("SELECT lastVoteReward FROM Players WHERE steamid = '{0}'", _cInfo2.playerId);
                                DataTable _result = SQL.TQuery(_sql);
                                if (_result.Rows.Count != 0)
                                {
                                    DateTime _lastVoteReward;
                                    DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _lastVoteReward);
                                    if (_lastVoteReward.ToString() != "10/29/2000 7:30:00 AM")
                                    {
                                        _sql = string.Format("UPDATE Players SET lastVoteReward = '10/29/2000 7:30:00 AM' WHERE steamid = '{0}'", _cInfo2.playerId);
                                        SQL.FastQuery(_sql, "VoteConsole");
                                        SdtdConsole.Instance.Output(string.Format("Vote reward delay reset for {0}.", _cInfo2.playerName));
                                    }
                                    else
                                    {
                                        SdtdConsole.Instance.Output(string.Format("Player named {0} does not have a vote reward delay that requires reset.", _cInfo2.playerName));
                                    }
                                }
                            }
                        }
                        return;
                    }
                    if (_params[1].ToLower().Equals("all"))
                    {
                        string _sql = string.Format("UPDATE Players SET lastVoteReward = '10/29/2000 7:30:00 AM' WHERE lastVoteReward != '10/29/2000 7:30:00 AM'");
                        SQL.FastQuery(_sql, "VoteConsole");
                        return;
                    }
                    ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                    if (_cInfo != null)
                    {
                        string _sql = string.Format("SELECT lastVoteReward FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                        DataTable _result = SQL.TQuery(_sql);
                        if (_result.Rows.Count != 0)
                        {
                            DateTime _lastVoteReward;
                            DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _lastVoteReward);
                            if (_lastVoteReward.ToString() != "10/29/2000 7:30:00 AM")
                            {
                                _sql = string.Format("UPDATE Players SET lastVoteReward = '10/29/2000 7:30:00 AM' WHERE steamid = '{0}'", _cInfo.playerId);
                                SQL.FastQuery(_sql, "VoteConsole");
                                SdtdConsole.Instance.Output("Vote reward delay reset.");
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("Player with id {0} does not have a vote reward delay that requires reset.", _params[1]));
                            }
                        }
                        _result.Dispose();
                    }
                    else
                    {
                        if (_params[1].Length != 17)
                        {
                            SdtdConsole.Instance.Output(string.Format("Can not reset Id: Invalid Id {0}", _params[1]));
                            return;
                        }
                        string _id = SQL.EscapeString(_params[1]);
                        string _sql = string.Format("SELECT lastVoteReward FROM Players WHERE steamid = '{0}'", _id);
                        DataTable _result = SQL.TQuery(_sql);
                        if (_result.Rows.Count != 0)
                        {
                            DateTime _lastVoteReward;
                            DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _lastVoteReward);
                            if (_lastVoteReward.ToString() != "10/29/2000 7:30:00 AM")
                            {
                                _sql = string.Format("UPDATE Players SET lastVoteReward = '10/29/2000 7:30:00 AM' WHERE steamid = '{0}'", _id);
                                SQL.FastQuery(_sql, "VoteConsole");
                                SdtdConsole.Instance.Output("Vote reward delay reset.");
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("Player with id {0} does not have a vote reward delay that requires reset.", _params[1]));
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Player with id {0} does not have a vote reward delay to reset.", _params[1]));
                        }
                        _result.Dispose();
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in VotingConsole.Run: {0}.", e));
            }
        }
    }
}