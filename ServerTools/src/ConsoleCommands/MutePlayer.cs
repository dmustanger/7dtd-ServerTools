using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class Mute : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "Mutes a players chat.";
        }
        public override string GetHelp()
        {
            return "Usage: mute add <steamId>\n" +
                "Usage: mute remove <steamId>\n" +
                "Usage: mute list";
        }
        public override string[] GetCommands()
        {
            return new string[] { "mute", string.Empty };
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
                    if (_params[1].Length != 17)
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not add SteamId: Invalid SteamId {0}.", _params[1]));
                        return;
                    }
                    Player p = PersistentContainer.Instance.Players[_params[1], false];
                    if (p != null)
                    {
                        if (p.IsMuted)
                        {
                            SdtdConsole.Instance.Output(string.Format("Player with the steamid {0} is already muted.", _params[1]));
                            return;
                        }
                    }
                    PersistentContainer.Instance.Players[_params[1], true].IsMuted = true;
                    PersistentContainer.Instance.Save();
                    SdtdConsole.Instance.Output(string.Format("Player with the steamid {0} has been muted.", _params[1]));
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
                        SdtdConsole.Instance.Output(string.Format("Can not add SteamId: Invalid SteamId {0}.", _params[1]));
                        return;
                    }
                    Player p = PersistentContainer.Instance.Players[_params[1], false];
                    if (p == null)
                    {
                        SdtdConsole.Instance.Output(string.Format("Player with the steamid {0} is not muted.", _params[1]));
                        return;
                    }
                    if (!p.IsMuted)
                    {
                        SdtdConsole.Instance.Output(string.Format("Player with the steamid {0} is not muted.", _params[1]));
                        return;
                    }
                    PersistentContainer.Instance.Players[_params[1], false].IsMuted = false;
                    PersistentContainer.Instance.Save();
                    SdtdConsole.Instance.Output(string.Format("Player with the steamid {0} has been unmuted.", _params[1]));
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