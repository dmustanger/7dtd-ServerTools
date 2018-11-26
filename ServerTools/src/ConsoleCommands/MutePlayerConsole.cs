using System;
using System.Collections.Generic;
using System.Data;

namespace ServerTools
{
    public class MutedConsole : ConsoleCmdAbstract
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
                "  3. mute remove <steamId>\n" +
                "  4. mute list\n" +
                "1. Adds a steam Id to the mute list for 60 minutes\n" +
                "2. Adds a steam Id to the mute list for a specific time\n" +
                "3. Removes a steam Id from the mute list\n" +
                "4. Lists all steam Id in the mute list\n" +
                "*Note Use -1 for time to mute indefinitely*";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Mute", "mute", string.Empty };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (MutePlayer.IsEnabled)
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
                                string _sql;
                                if (_muteTime == -1)
                                {
                                    MutePlayer.Mutes.Add(_cInfo.playerId);
                                    _sql = string.Format("UPDATE Players SET muteTime = -1, playername = '{0}', WHERE steamid = '{1}'", _cInfo.playerName, _cInfo.playerId);
                                    SQL.FastQuery(_sql);
                                    SdtdConsole.Instance.Output(string.Format("Steam Id {0}, player name {1} has been muted indefinitely.", _cInfo.playerId, _cInfo.playerName));
                                    return;
                                }
                                MutePlayer.Mutes.Add(_cInfo.playerId);
                                _sql = string.Format("UPDATE Players SET muteTime = {0}, playername = '{1}', muteDate = '{2}' WHERE steamid = '{3}'", _muteTime, _cInfo.playerName, DateTime.Now, _cInfo.playerId);
                                SQL.FastQuery(_sql);
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
                            ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForPlayerId(_id);
                            if (_cInfo != null)
                            {
                                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + ", you have been unmuted.[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            MutePlayer.Mutes.Remove(_id);
                            string _sql = string.Format("UPDATE Players SET muteTime = 0 WHERE steamid = '{0}'", _id);
                            SQL.FastQuery(_sql);
                            SdtdConsole.Instance.Output(string.Format("Steam Id {0} has been unmuted.", _id));
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
                        if (MutePlayer.Mutes.Count > 0)
                        {
                            string _sql = "SELECT steamid, playername, muteTime, muteDate FROM Players WHERE muteTime > 0 OR muteTime = -1";
                            DataTable _result = SQL.TQuery(_sql);
                            int _steamid;
                            int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _steamid);
                            string playername = _result.Rows[0].ItemArray.GetValue(1).ToString();
                            int _muteTime;
                            int.TryParse(_result.Rows[0].ItemArray.GetValue(2).ToString(), out _muteTime);
                            DateTime _muteDate;
                            DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(3).ToString(), out _muteDate);
                            _result.Dispose();
                            if (_muteTime == -1)
                            {
                                SdtdConsole.Instance.Output(string.Format("Steam id {0}, player name {1} is muted indefinitely", _steamid, playername));
                                return;
                            }
                            if (_muteTime > 0)
                            {
                                TimeSpan varTime = DateTime.Now - _muteDate;
                                double fractionalMinutes = varTime.TotalMinutes;
                                int _timepassed = (int)fractionalMinutes;
                                int _timeleft = _muteTime - _timepassed;
                                SdtdConsole.Instance.Output(string.Format("Steam id {0}, player name {1} for {2} minutes", _steamid, playername, _timeleft));
                                return;
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
                    Log.Out(string.Format("[SERVERTOOLS] Error in ConsoleCommandMuteConsole.Run: {0}.", e));
                }
            }
            else
            {
                SdtdConsole.Instance.Output("Mute is not enabled.");
                return;
            }
        }
    }
}