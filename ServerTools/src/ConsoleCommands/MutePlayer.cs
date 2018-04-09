using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class Muted : ConsoleCmdAbstract
    {

        public override string GetDescription()
        {
            return "[ServerTools]-Mutes a players chat.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. mute add <steamId/entityId>\n" +
                "  2. mute add <steamId/entityId> <time>\n" +
                "  3. mute remove <steamId>" +
                "  4. mute list\n" +
                "1. Adds a steam Id to the mute list for 60 minutes\n" +
                "2. Adds a steam Id to the mute list for a specific time\n" +
                "3. Removes a steam Id from the mute list\n" +
                "4. Lists all steam Id in the mute list" +
                "*Note Use -1 for time to mute indefinitely*";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Mute", "mute", string.Empty };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 || _params.Count > 3)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1 to 3, found {0}.", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count < 2 || _params.Count > 3)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2 or 3, found {0}.", _params.Count));
                        return;
                    }
                    if (_params[1].Length < 1 || _params[1].Length > 17)
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not add Id: Invalid Id {0}.", _params[1]));
                        return;
                    }
                    int _muteTime = 60;
                    if (_params[2] != null)
                    {
                        int _value;
                        if (int.TryParse(_params[2], out _value))
                            _muteTime = _value;
                    }
                    ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                    if (_cInfo != null)
                    {
                        if (MutePlayer.Mutes.Contains(_cInfo.playerId))
                        {
                            SdtdConsole.Instance.Output(string.Format("Steam Id {0}, player name {1} is already muted.", _cInfo.playerId, _cInfo.playerName));
                            return;
                        }
                        else
                        {
                            if (_muteTime == -1)
                            {
                                MutePlayer.Mutes.Add(_cInfo.playerId);
                                PersistentContainer.Instance.Players[_cInfo.playerId, true].MuteTime = -1;
                                PersistentContainer.Instance.Players[_cInfo.playerId, true].MuteName = _cInfo.playerName;
                                PersistentContainer.Instance.Save();
                                SdtdConsole.Instance.Output(string.Format("Steam Id {0}, player name {1} has been muted indefinitely.", _cInfo.playerId, _cInfo.playerName));
                                return;
                            }
                            MutePlayer.Mutes.Add(_cInfo.playerId);
                            PersistentContainer.Instance.Players[_cInfo.playerId, true].MuteTime = _muteTime;
                            PersistentContainer.Instance.Players[_cInfo.playerId, true].MuteName = _cInfo.playerName;
                            PersistentContainer.Instance.Players[_cInfo.playerId, true].MuteDate = DateTime.Now;
                            PersistentContainer.Instance.Save();
                            SdtdConsole.Instance.Output(string.Format("Steam Id {0}, player name {1} has been muted for {2} minutes.", _cInfo.playerId, _cInfo.playerName, _muteTime));
                            return;
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
                    string _id = _params[1];
                    if (MutePlayer.Mutes.Contains(_id))
                    {
                        ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForPlayerId(_id);
                        if (_cInfo != null)
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You have been unmuted.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                        MutePlayer.Mutes.Remove(_id);
                        PersistentContainer.Instance.Players[_id, true].MuteTime = 0;
                        PersistentContainer.Instance.Save();
                        SdtdConsole.Instance.Output(string.Format("Steam Id {0}, player name {1} has been unmuted.", _id, PersistentContainer.Instance.Players[_id, false].MuteName));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Steam Id {0} is not muted.", _id));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}.", _params.Count));
                        return;
                    }
                    string _id = _params[1];
                    if (MutePlayer.Mutes.Contains(_id))
                    {
                        for (int i = 0; i < MutePlayer.Mutes.Count; i++)
                        {
                            string _muteId = MutePlayer.Mutes[i];
                            Player p = PersistentContainer.Instance.Players[_id, false];
                            {
                                if (p.MuteTime == -1)
                                {
                                    SdtdConsole.Instance.Output(string.Format("Steam id {0}, player name {1} is muted indefinitely", _id, p.MuteName));
                                    return;
                                }
                                if (p.MuteTime > 0)
                                {
                                    TimeSpan varTime = DateTime.Now - p.MuteDate;
                                    double fractionalMinutes = varTime.TotalMinutes;
                                    int _timepassed = (int)fractionalMinutes;
                                    int _timeleft = p.MuteTime - _timepassed;
                                    SdtdConsole.Instance.Output(string.Format("Steam id {0}, player name {1} for {2} minutes", _id, p.MuteName, _timeleft));
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("No players are muted."));
                        return;
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ConsoleCommandMute.Run: {0}.", e));
            }
        }
    }
}