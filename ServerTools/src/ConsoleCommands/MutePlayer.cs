using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class Mute : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]-Mutes a players chat.";
        }
        public override string GetHelp()
        {
            return "Usage: mute add <steamId/entityId>\n" +
                "Usage: mute remove <steamId/entityId>\n" +
                "Usage: mute list";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Mute", "mute", string.Empty };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 || _params.Count > 2)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1 to 2, found {0}.", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}.", _params.Count));
                        return;
                    }
                    if (_params[1].Length < 1 || _params[1].Length > 17)
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not add Id: Invalid Id {0}.", _params[1]));
                        return;
                    }
                    ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                    if (_cInfo != null)
                    {
                        Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                        if (p != null)
                        {
                            if (p.IsMuted)
                            {
                                SdtdConsole.Instance.Output(string.Format("Player with the Id {0} is already muted.", _params[1]));
                                return;
                            }
                            else
                            {
                                PersistentContainer.Instance.Players[_cInfo.playerId, true].IsMuted = true;
                                PersistentContainer.Instance.Save();
                                SdtdConsole.Instance.Output(string.Format("Player with the Id {0} has been muted.", _params[1]));
                            }
                        }
                        else
                        {
                            PersistentContainer.Instance.Players[_cInfo.playerId, true].IsMuted = true;
                            PersistentContainer.Instance.Save();
                            SdtdConsole.Instance.Output(string.Format("Player with the Id {0} has been muted.", _cInfo.entityId));
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Player with Id {0} can not be found.", _params[1]));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}.", _params.Count));
                        return;
                    }
                    if (_params[1].Length != 17)
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not add Id: Invalid Id {0}.", _params[1]));
                        return;
                    }
                    ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                    if (_cInfo != null)
                    {
                        Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                        if (p != null)
                        {
                            if (!p.IsMuted)
                            {
                                SdtdConsole.Instance.Output(string.Format("Player with the Id {0} is not muted.", _params[1]));
                                return;
                            }
                            else
                            {
                                PersistentContainer.Instance.Players[_cInfo.playerId, false].IsMuted = false;
                                PersistentContainer.Instance.Save();
                                SdtdConsole.Instance.Output(string.Format("Player with the Id {0} has been unmuted.", _params[1]));
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Player with Id {0} is not muted.", _params[1]));
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("No Player with the Id {0} found.", _params[1]));
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}.", _params.Count));
                        return;
                    }                  
                    bool _mutedPlayersFound = false;
                    foreach (string _id in PersistentContainer.Instance.Players.SteamIDs)
                    {
                        Player p = PersistentContainer.Instance.Players[_id, false];
                        {
                            if (p.IsMuted)
                            {
                                ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForPlayerId(_params[1]);
                                _mutedPlayersFound = true;
                                SdtdConsole.Instance.Output(string.Format("{0} {1}", _id, _cInfo.playerId));
                            }
                        }
                    }
                    if (!_mutedPlayersFound)
                    {
                        SdtdConsole.Instance.Output(string.Format("No players are muted."));
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }

            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ConsoleCommandMute.Run: {0}.", e));
            }
        }
    }
}