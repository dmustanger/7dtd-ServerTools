using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class MutedConsole : ConsoleCmdAbstract
    {

        public override string GetDescription()
        {
            return "[ServerTools] - Mutes a players chat.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. st-mt add <steamId/entityId>\n" +
                "  2. st-mt add <steamId/entityId> <time>\n" +
                "  3. st-mt remove <steamId>\n" +
                "  4. st-mt list\n" +
                "1. Adds a steam Id to the mute list for 60 minutes\n" +
                "2. Adds a steam Id to the mute list for a specific time\n" +
                "3. Removes a steam Id from the mute list\n" +
                "4. Lists all steam Id in the mute list\n" +
                "*Note Use -1 for time to mute indefinitely*";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Mute", "mt", "st-mt" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (Mute.IsEnabled)
            {
                try
                {
                    if (_params.Count < 1 || _params.Count > 3)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1 to 3, found {0}.", _params.Count));
                        return;
                    }
                    if (_params[0].ToLower().Equals("add"))
                    {
                        if (_params.Count < 2 || _params.Count > 3)
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2 or 3, found {0}.", _params.Count));
                            return;
                        }
                        if (_params[1].Length < 1 || _params[1].Length > 17)
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not add Id: Invalid Id {0}.", _params[1]));
                            return;
                        }
                        int _muteTime = 60;
                        if (_params[2] != null)
                        {
                            if (int.TryParse(_params[2], out int _value))
                                _muteTime = _value;
                        }
                        ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                        if (_cInfo != null)
                        {
                            if (Mute.Mutes.Contains(_cInfo.playerId))
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Steam Id {0}, player name {1} is already muted.", _cInfo.playerId, _cInfo.playerName));
                                return;
                            }
                            else
                            {
                                if (_muteTime == -1)
                                {
                                    Mute.Mutes.Add(_cInfo.playerId);
                                    PersistentContainer.Instance.Players[_cInfo.playerId].MuteTime = -1;
                                    PersistentContainer.Instance.Players[_cInfo.playerId].MuteName = _cInfo.playerName;
                                    PersistentContainer.Instance.Players[_cInfo.playerId].MuteDate = DateTime.Now;
                                    PersistentContainer.DataChange = true;
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Steam Id {0}, player name {1} has been muted indefinitely.", _cInfo.playerId, _cInfo.playerName));
                                    return;
                                }
                                else
                                {
                                    Mute.Mutes.Add(_cInfo.playerId);
                                    PersistentContainer.Instance.Players[_cInfo.playerId].MuteTime = _muteTime;
                                    PersistentContainer.Instance.Players[_cInfo.playerId].MuteName = _cInfo.playerName;
                                    PersistentContainer.Instance.Players[_cInfo.playerId].MuteDate = DateTime.Now;

                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Steam Id {0}, player name {1} has been muted for {2} minutes.", _cInfo.playerId, _cInfo.playerName, _muteTime));
                                    return;
                                }
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Player with Id {0} can not be found.", _params[1]));
                            return;
                        }
                    }
                    else if (_params[0].ToLower().Equals("remove"))
                    {
                        if (_params.Count != 2)
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found {0}.", _params.Count));
                            return;
                        }
                        if (_params[1].Length != 17)
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not add Id: Invalid Id {0}.", _params[1]));
                            return;
                        }
                        string _id = _params[1];
                        if (Mute.Mutes.Contains(_id))
                        {
                            ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForPlayerId(_id);
                            if (_cInfo != null)
                            {
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + "You have been unmuted.[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            Mute.Mutes.Remove(_id);
                            PersistentContainer.Instance.Players[_cInfo.playerId].MuteTime = 0;
                            PersistentContainer.DataChange = true;
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Steam Id {0} has been unmuted.", _id));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Steam Id {0} is not muted.", _id));
                            return;
                        }
                    }
                    else if (_params[0].ToLower().Equals("list"))
                    {
                        if (_params.Count != 1)
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}.", _params.Count));
                            return;
                        }
                        if (Mute.Mutes.Count == 0)
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] No players are muted."));
                            return;
                        }
                        else
                        {
                            for (int i = 0; i < Mute.Mutes.Count; i++)
                            {
                                string _id = Mute.Mutes[i];
                                int _muteTime = PersistentContainer.Instance.Players[_id].MuteTime;
                                string _muteName = PersistentContainer.Instance.Players[_id].MuteName;
                                if (_muteTime > 0)
                                {
                                    DateTime _muteDate = PersistentContainer.Instance.Players[_id].MuteDate;
                                    TimeSpan varTime = DateTime.Now - _muteDate;
                                    double fractionalMinutes = varTime.TotalMinutes;
                                    int _timepassed = (int)fractionalMinutes;
                                    int _timeleft = _muteTime - _timepassed;
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Muted player: steam Id {0} named {1} for {2} more minutes.", _id, _muteName, _timeleft));
                                }
                                else if (_muteTime == -1)
                                {
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Muted player: steam Id {0} named {1} forever.", _id, _muteName));
                                }
                            }
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[0]));
                        return;
                    }
                }
                catch (Exception e)
                {
                    Log.Out(string.Format("[SERVERTOOLS] Error in ConsoleCommandMuteConsole.Execute: {0}", e.Message));
                }
            }
            else
            {
                SdtdConsole.Instance.Output("[SERVERTOOLS] Mute is not enabled.");
                return;
            }
        }
    }
}