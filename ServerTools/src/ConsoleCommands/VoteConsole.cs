using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerTools
{
    class VoteConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Enable, disable voting. Reset player vote reward delay.";
        }

        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-vote off\n" +
                   "  2. st-vote on\n" +
                   "  3. st-vote reset <EOS/EntityId/PlayerName>\n" +
                   "  4. st-vote reset online\n" +
                   "  5. st-vote reset all\n" +
                   "  6. st-vote reward <EOS/EntityId/PlayerName>\n" +
                   "1. Turn off voting\n" +
                   "2. Turn on voting\n" +
                   "3. Reset the vote reward delay of a player Id\n" +
                   "4. Reset the vote reward delay of all online players\n" +
                   "5. Reset the vote reward delay of all online and offline players\n" +
                   "6. Give a player a vote reward. Identical to running chat command /votereward. Does not count towards their delay\n";
        }

        protected override string[] getCommands()
        {
            return new string[] { "st-Vote", "vo", "st-vo" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params[0].ToLower().Equals("off"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                        return;
                    }
                    if (Voting.IsEnabled)
                    {
                        Voting.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Voting has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Voting is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                        return;
                    }
                    if (!Voting.IsEnabled)
                    {
                        Voting.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Voting has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Voting is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("reset"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found '{0}'", _params.Count));
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
                                PersistentContainer.Instance.Players[_cInfo2.CrossplatformId.CombinedString].LastVote = DateTime.Now.AddYears(-1);
                                PersistentContainer.DataChange = true;
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Vote delay reset for '{0}'", _cInfo2.playerName));
                            }
                        }
                        return;
                    }
                    if (_params[1].ToLower().Equals("all"))
                    {
                        if (PersistentContainer.Instance.Players.IDs.Count > 0)
                        {
                            for (int i = 0; i < PersistentContainer.Instance.Players.IDs.Count; i++)
                            {
                                PersistentContainer.Instance.Players[PersistentContainer.Instance.Players.IDs[i]].LastVote = DateTime.Now.AddYears(-1);
                                PersistentContainer.DataChange = true;
                            }
                            SdtdConsole.Instance.Output("[SERVERTOOLS] Vote delay reset for all players");
                            return;
                        }
                    }
                    else
                    {
                        ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_params[1]);
                        if (cInfo != null)
                        {
                            PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].LastVote = DateTime.Now.AddYears(-1);
                            PersistentContainer.DataChange = true;
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Vote delay reset for '{0}'", cInfo.playerName));
                        }
                        else if (_params[1].Contains("_"))
                        {
                            PersistentPlayer p = PersistentContainer.Instance.Players[_params[1]];
                            if (p != null)
                            {
                                PersistentContainer.Instance.Players[_params[1]].LastVote = DateTime.Now.AddYears(-1);
                                PersistentContainer.DataChange = true;
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Vote delay reset for '{0}'", _params[1]));
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("Id '{0}' does not have a vote delay to reset", _params[1]));
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Invalid Id '{0}'", _params[1]));
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("reward"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found '{0}'", _params.Count));
                        return;
                    }
                    string id = "", playerName = "";
                    ClientInfo cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                    if (cInfo != null)
                    {
                        id = cInfo.CrossplatformId.CombinedString;
                        playerName = cInfo.playerName;
                    }
                    else if (_params[1].Contains("EOS_"))
                    {
                        PersistentPlayerData ppd = GeneralOperations.GetPersistentPlayerDataFromId(_params[1]);
                        if (ppd != null)
                        {
                            id = ppd.UserIdentifier.CombinedString;
                            playerName = ppd.PlayerName;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to locate player data for '{0}'", _params[1]));
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid id '{0}'. Use their EOS id when offline", _params[1]));
                        return;
                    }
                    Voting.ItemOrBlockSpawn(cInfo, Voting.Reward_Count);
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in VoteConsole.Execute: {0}", e.Message));
            }
        }
    }
}