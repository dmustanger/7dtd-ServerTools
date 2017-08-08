namespace ServerTools
{
    public class MutePlayer
    {
        private static string[] _cmd = { "mute" };

        public static void Add(ClientInfo _cInfo, string _playerName)
        {
            if (!GameManager.Instance.adminTools.CommandAllowedFor(_cmd, _cInfo.playerId))
            {
                string _phrase200;
                if (!Phrases.Dict.TryGetValue(200, out _phrase200))
                {
                    _phrase200 = "{PlayerName} you do not have permissions to use this command.";
                }
                _phrase200 = _phrase200.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _phrase200), "Server", false, "", false));
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
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase201, CustomCommands.ChatColor), "Server", false, "", false));
                }
                else
                {
                    Player p = PersistentContainer.Instance.Players[_PlayertoMute.playerId, false];
                    if (p == null)
                    {
                        Mute(_cInfo, _PlayertoMute);
                    }
                    else
                    {
                        if (p.IsMuted)
                        {
                            string _phrase202;
                            if (!Phrases.Dict.TryGetValue(202, out _phrase202))
                            {
                                _phrase202 = "{AdminPlayerName} player {MutedPlayerName} is already muted.";
                            }
                            _phrase202 = _phrase202.Replace("{AdminPlayerName}", _cInfo.playerName);
                            _phrase202 = _phrase202.Replace("{MutedPlayerName}", _PlayertoMute.playerName);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase202, CustomCommands.ChatColor), "Server", false, "", false));
                        }
                        else
                        {
                            Mute(_cInfo, _PlayertoMute);
                        }
                    }
                }
            }
        }

        public static void Mute (ClientInfo _admin, ClientInfo _player)
        {
            PersistentContainer.Instance.Players[_player.playerId, true].IsMuted = true;
            PersistentContainer.Instance.Save();
            string _phrase203;
            if (!Phrases.Dict.TryGetValue(203, out _phrase203))
            {
                _phrase203 = "{AdminPlayerName} you have muted {MutedPlayerName}.";
            }
            _phrase203 = _phrase203.Replace("{AdminPlayerName}", _admin.playerName);
            _phrase203 = _phrase203.Replace("{MutedPlayerName}", _player.playerName);
            _admin.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase203, CustomCommands.ChatColor), "Server", false, "", false));
        }

        public static void Remove(ClientInfo _cInfo, string _playerName)
        {
            if (!GameManager.Instance.adminTools.CommandAllowedFor(_cmd, _cInfo.playerId))
            {
                string _phrase200;
                if (!Phrases.Dict.TryGetValue(200, out _phrase200))
                {
                    _phrase200 = "{PlayerName} you do not have permissions to use this command.";
                }
                _phrase200 = _phrase200.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _phrase200), "Server", false, "", false));
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
                        _phrase201 = "{AdminPlayerName} player {PlayerName} was not found.";
                    }
                    _phrase201 = _phrase201.Replace("{AdminPlayerName}", _cInfo.playerName);
                    _phrase201 = _phrase201.Replace("{PlayerName}", _playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase201, CustomCommands.ChatColor), "Server", false, "", false));
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
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase204, CustomCommands.ChatColor), "Server", false, "", false));
                    }
                    else
                    {
                        if (!p.IsMuted)
                        {
                            string _phrase204;
                            if (!Phrases.Dict.TryGetValue(204, out _phrase204))
                            {
                                _phrase204 = "{AdminPlayerName} player {PlayerName} is not muted.";
                            }
                            _phrase204 = _phrase204.Replace("{AdminPlayerName}", _cInfo.playerName);
                            _phrase204 = _phrase204.Replace("{PlayerName}", _PlayertoUnMute.playerName);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase204, CustomCommands.ChatColor), "Server", false, "", false));
                        }
                        else
                        {
                            PersistentContainer.Instance.Players[_PlayertoUnMute.playerId, false].IsMuted = false;
                            PersistentContainer.Instance.Save();
                            string _phrase205;
                            if (!Phrases.Dict.TryGetValue(205, out _phrase205))
                            {
                                _phrase205 = "{AdminPlayerName} you have unmuted {UnMutedPlayerName}.";
                            }
                            _phrase205 = _phrase205.Replace("{AdminPlayerName}", _cInfo.playerName);
                            _phrase205 = _phrase205.Replace("{UnMutedPlayerName}", _PlayertoUnMute.playerName);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase205, CustomCommands.ChatColor), "Server", false, "", false));
                        }
                    }
                }
            }
        }
    }
}