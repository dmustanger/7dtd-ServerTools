using System;
using System.Collections.Generic;
using System.Data;

namespace ServerTools
{
    public class MutePlayer
    {
        public static bool IsEnabled = false;
        public static List<string> Mutes = new List<string>();
        private static string[] _cmd = { "mute" };

        public static void Add(ClientInfo _cInfo, string _playerName)
        {
            if (IsEnabled)
            {
                if (!GameManager.Instance.adminTools.CommandAllowedFor(_cmd, _cInfo.playerId))
                {
                    string _phrase107;
                    if (!Phrases.Dict.TryGetValue(107, out _phrase107))
                    {
                        _phrase107 = "{PlayerName} you do not have permissions to use this command.";
                    }
                    _phrase107 = _phrase107.Replace("{PlayerName}", _cInfo.playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase107), Config.Server_Response_Name, false, "ServerTools", false));
                }
                else
                {
                    _playerName = _playerName.Replace("mute ", "");
                    ClientInfo _PlayertoMute = ConsoleHelper.ParseParamIdOrName(_playerName);
                    if (_PlayertoMute == null)
                    {
                        string _phrase201;
                        if (!Phrases.Dict.TryGetValue(201, out _phrase201))
                        {
                            _phrase201 = "{AdminPlayerName} player {PlayerName} was not found.";
                        }
                        _phrase201 = _phrase201.Replace("{AdminPlayerName}", _cInfo.playerName);
                        _phrase201 = _phrase201.Replace("{PlayerName}", _playerName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase201, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                    else
                    {
                        string _sql = string.Format("SELECT muteTime FROM Players WHERE steamid = '{0}'", _PlayertoMute.playerId);
                        DataTable _result = SQL.TQuery(_sql);
                        int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out int _muteTime);
                        _result.Dispose();
                        if (_muteTime > 0 || _muteTime == -1)
                        {
                            string _phrase202;
                            if (!Phrases.Dict.TryGetValue(202, out _phrase202))
                            {
                                _phrase202 = "{AdminPlayerName} player {MutedPlayerName} is already muted.";
                            }
                            _phrase202 = _phrase202.Replace("{AdminPlayerName}", _cInfo.playerName);
                            _phrase202 = _phrase202.Replace("{MutedPlayerName}", _PlayertoMute.playerName);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase202, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                        else
                        {
                            Mute(_cInfo, _PlayertoMute);
                        }
                    }
                }
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}This command is not enabled.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void Mute (ClientInfo _admin, ClientInfo _player)
        {
            Mutes.Add(_player.playerId);
            string _sql = string.Format("UPDATE Players SET muteTime = 60, muteName = '{0}', muteDate = '{1}' WHERE steamid = '{2}'", _player.playerName, DateTime.Now, _player.playerId);
            SQL.FastQuery(_sql);
            string _phrase203;
            if (!Phrases.Dict.TryGetValue(203, out _phrase203))
            {
                _phrase203 = "{AdminPlayerName} you have muted {MutedPlayerName} for 60 minutes.";
            }
            _phrase203 = _phrase203.Replace("{AdminPlayerName}", _admin.playerName);
            _phrase203 = _phrase203.Replace("{MutedPlayerName}", _player.playerName);
            _admin.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase203, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
        }

        public static void Remove(ClientInfo _cInfo, string _playerName)
        {
            if (IsEnabled)
            {
                if (!GameManager.Instance.adminTools.CommandAllowedFor(_cmd, _cInfo.playerId))
                {
                    string _phrase107;
                    if (!Phrases.Dict.TryGetValue(107, out _phrase107))
                    {
                        _phrase107 = "{PlayerName} you do not have permissions to use this command.";
                    }
                    _phrase107 = _phrase107.Replace("{PlayerName}", _cInfo.playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase107), Config.Server_Response_Name, false, "ServerTools", false));
                }
                else
                {
                    _playerName = _playerName.Replace("unmute ", "");
                    ClientInfo _PlayertoUnMute = ConsoleHelper.ParseParamIdOrName(_playerName);
                    if (_PlayertoUnMute == null)
                    {
                        string _phrase201;
                        if (!Phrases.Dict.TryGetValue(201, out _phrase201))
                        {
                            _phrase201 = "{AdminPlayerName} player {PlayerName} was not found online.";
                        }
                        _phrase201 = _phrase201.Replace("{AdminPlayerName}", _cInfo.playerName);
                        _phrase201 = _phrase201.Replace("{PlayerName}", _playerName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase201, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                    else
                    {
                        Player p = PersistentContainer.Instance.Players[_PlayertoUnMute.playerId, false];
                        if (p == null)
                        {
                            string _phrase204;
                            if (!Phrases.Dict.TryGetValue(204, out _phrase204))
                            {
                                _phrase204 = "{AdminPlayerName} player {PlayerName} is not muted.";
                            }
                            _phrase204 = _phrase204.Replace("{AdminPlayerName}", _cInfo.playerName);
                            _phrase204 = _phrase204.Replace("{PlayerName}", _PlayertoUnMute.playerName);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase204, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                        else
                        {
                            if (!Mutes.Contains(_PlayertoUnMute.playerId))
                            {
                                string _phrase204;
                                if (!Phrases.Dict.TryGetValue(204, out _phrase204))
                                {
                                    _phrase204 = "{AdminPlayerName} player {PlayerName} is not muted.";
                                }
                                _phrase204 = _phrase204.Replace("{AdminPlayerName}", _cInfo.playerName);
                                _phrase204 = _phrase204.Replace("{PlayerName}", _PlayertoUnMute.playerName);
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase204, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                            else
                            {
                                Mutes.Remove(_PlayertoUnMute.playerId);
                                string _sql = string.Format("UPDATE Players SET muteTime = 0 WHERE steamid = '{0}'", _PlayertoUnMute.playerId);
                                SQL.FastQuery(_sql);
                                string _phrase205;
                                if (!Phrases.Dict.TryGetValue(205, out _phrase205))
                                {
                                    _phrase205 = "{AdminPlayerName} you have unmuted {UnMutedPlayerName}.";
                                }
                                _phrase205 = _phrase205.Replace("{AdminPlayerName}", _cInfo.playerName);
                                _phrase205 = _phrase205.Replace("{UnMutedPlayerName}", _PlayertoUnMute.playerName);
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase205, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                        }
                    }
                }
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}This command is not enabled.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void MuteList()
        {
            string _sql = "SELECT steamid, muteTime, muteDate FROM Players WHERE muteTime > 0 OR muteTime = -1";
            DataTable _result = SQL.TQuery(_sql);
            if (_result.Rows.Count > 0)
            {
                foreach (DataRow row in _result.Rows)
                {
                    int.TryParse(row[1].ToString(), out int _muteTime);
                    if (_muteTime > 0 || _muteTime == -1)
                    {
                        if (_muteTime == -1)
                        {
                            Mutes.Add(row[0].ToString());
                            break;
                        }
                        else
                        {
                            DateTime.TryParse(row[2].ToString(), out DateTime _muteDate);
                            TimeSpan varTime = DateTime.Now - _muteDate;
                            double fractionalMinutes = varTime.TotalMinutes;
                            int _timepassed = (int)fractionalMinutes;
                            if (_timepassed < _muteTime)
                            {
                                Mutes.Add(row[0].ToString());
                            }
                            else
                            {
                                _sql = string.Format("UPDATE Players SET muteTime = 0 WHERE steamid = '{0}'", row[0].ToString());
                                SQL.FastQuery(_sql);
                            }
                        }
                    }
                }
            }
            _result.Dispose();
        }

        public static void Clear()
        {
            for (int i = 0; i < Mutes.Count; i++)
            {
                string _id = Mutes[i];
                string _sql = string.Format("SELECT muteTime, muteDate FROM Players WHERE steamid = '{0}'", _id);
                DataTable _result = SQL.TQuery(_sql);
                int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out int _muteTime);
                DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out DateTime _muteDate);
                _result.Dispose();
                if (_muteTime != -1)
                {
                    TimeSpan varTime = DateTime.Now - _muteDate;
                    double fractionalMinutes = varTime.TotalMinutes;
                    int _timepassed = (int)fractionalMinutes;
                    if (_timepassed >= _muteTime)
                    {
                        Mutes.Remove(_id);
                        _sql = string.Format("UPDATE Players SET muteTime = 0 WHERE steamid = '{0}'", _id);
                        SQL.FastQuery(_sql);
                    }
                }
            }
        }
    }
}