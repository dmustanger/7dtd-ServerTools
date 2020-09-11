using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerTools
{
    class VoteConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable, disable voting. Reset player vote reward delay.";
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
            return new string[] { "st-VoteReward", "vote", "st-vote" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 || _params.Count > 2)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1 or 2, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (VoteReward.IsEnabled)
                    {
                        VoteReward.IsEnabled = false;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Vote reward has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Vote reward is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!VoteReward.IsEnabled)
                    {
                        VoteReward.IsEnabled = true;
                        LoadConfig.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Vote reward has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Vote reward is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("reset"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found {0}", _params.Count));
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
                                PersistentContainer.Instance.Players[_cInfo2.playerId].LastVote = DateTime.Now.AddYears(-1);
                                PersistentContainer.Instance.Save();
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Vote reward delay reset for {0}", _cInfo2.playerName));
                            }
                        }
                        return;
                    }
                    if (_params[1].ToLower().Equals("all"))
                    {
                        for (int i = 0; i < PersistentContainer.Instance.Players.SteamIDs.Count; i++)
                        {
                            string _id = PersistentContainer.Instance.Players.SteamIDs[i];
                            PersistentPlayer p = PersistentContainer.Instance.Players[_id];
                            {
                                PersistentContainer.Instance.Players[_id].LastVote = DateTime.Now.AddYears(-1);
                            }
                        }
                        PersistentContainer.Instance.Save();
                        SdtdConsole.Instance.Output("[SERVERTOOLS] Vote reward delay reset for all players");
                        return;
                    }
                    else
                    {
                        ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                        if (_cInfo != null)
                        {
                            PersistentContainer.Instance.Players[_cInfo.playerId].LastVote = DateTime.Now.AddYears(-1);
                            PersistentContainer.Instance.Save();
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Vote reward delay reset for {0}", _cInfo.playerName));
                        }
                        else
                        {
                            if (_params[1].Length != 17)
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not reset Id: Invalid Id {0}", _params[1]));
                                return;
                            }
                            PersistentPlayer p = PersistentContainer.Instance.Players[_params[1]];
                            if (p != null)
                            {
                                PersistentContainer.Instance.Players[_params[1]].LastVote = DateTime.Now.AddYears(-1);
                                PersistentContainer.Instance.Save();
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Vote reward delay reset for {0}", _params[1]));
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("Player with id {0} does not have a Vote reward delay to reset", _params[1]));
                            }
                        }
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in VotingConsole.Execute: {0}", e.Message));
            }
        }
    }
}