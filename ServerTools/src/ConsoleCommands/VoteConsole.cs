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
                   "  1. st-vr off\n" +
                   "  2. st-vr on\n" +
                   "  3. st-vr reset <EOS/EntityId/PlayerName>\n" +
                   "  4. st-vr reset online\n" +
                   "  5. st-vr reset all\n" +
                   "1. Turn off voting\n" +
                   "2. Turn on voting\n" +
                   "3. Reset the vote reward delay of a player Id\n" +
                   "4. Reset the vote reward delay of all online players\n" +
                   "5. Reset the vote reward delay of all online and offline players\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-VoteReward", "vr", "st-vr" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 || _params.Count > 2)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1 or 2, found '{0}'", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (VoteReward.IsEnabled)
                    {
                        VoteReward.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Vote reward has been set to off"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Vote reward is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!VoteReward.IsEnabled)
                    {
                        VoteReward.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Vote reward has been set to on"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Vote reward is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("reset"))
                {
                    if (_params.Count != 2)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found '{0}'", _params.Count));
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
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Vote reward delay reset for '{0}'", _cInfo2.playerName));
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
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] Vote reward delay reset for all players");
                            return;
                        }
                    }
                    else
                    {
                        ClientInfo cInfo = PersistentOperations.GetClientInfoFromNameOrId(_params[1]);
                        if (cInfo != null)
                        {
                            PersistentContainer.Instance.Players[cInfo.CrossplatformId.CombinedString].LastVote = DateTime.Now.AddYears(-1);
                            PersistentContainer.DataChange = true;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Vote reward delay reset for '{0}'", cInfo.playerName));
                        }
                        else if (_params[1].Contains("_"))
                        {
                            PersistentPlayer p = PersistentContainer.Instance.Players[_params[1]];
                            if (p != null)
                            {
                                PersistentContainer.Instance.Players[_params[1]].LastVote = DateTime.Now.AddYears(-1);
                                PersistentContainer.DataChange = true;
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Vote reward delay reset for '{0}'", _params[1]));
                            }
                            else
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Id '{0}' does not have a vote reward delay to reset", _params[1]));
                            }
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Invalid Id '{0}'", _params[1]));
                        }
                    }
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in VotingConsole.Execute: {0}", e.Message));
            }
        }
    }
}