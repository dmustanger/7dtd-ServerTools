namespace ServerTools
{
    public class ChatHook
    {
        public static bool ChatFlood = false;
        public static bool AdminNameColoring = false;
        public static string AdminColor = "[FF0000]";
        public static string AdminPrefix = "(ADMIN)";
        public static int AdminLevel = 0;
        public static string ModColor = "[008000]";
        public static string ModPrefix = "(MOD)";
        public static int ModLevel = 1;

        public static bool Hook(ClientInfo _cInfo, string _message, string _playerName, string _secondaryName, bool _localizeSecondary)
        {
            if (!string.IsNullOrEmpty(_message) && _cInfo != null && _playerName != "Server" && _secondaryName != "ServerTools")
            {
                if (ChatFlood)
                {
                    if (_message.Length > 500)
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Message to long.[-]", CustomCommands.ChatColor), "Server", false, "ServerTools", false));
                        return false;
                    }
                }
                if (ChatLog.IsEnabled)
                {
                    ChatLog.Log(_message, _playerName);
                }
                if (MutePlayer.Dict.ContainsKey(_cInfo.playerId))
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, "You are muted.", "Server", false, "ServerTools", false));
                    return false;
                }
                if (AdminNameColoring && !_message.StartsWith("/") && !_message.StartsWith("@") && _secondaryName != "ServerTools1" && GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId))
                {
                    AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                    if (Admin.PermissionLevel == AdminLevel)
                    {
                        _playerName = string.Format("{0}{1} {2}[-]", AdminColor, AdminPrefix, _playerName);
                        GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, _message, _playerName, false, "ServerTools1", false);
                        return false;
                    }
                    if (Admin.PermissionLevel == ModLevel)
                    {
                        _playerName = string.Format("{0}{1} {2}[-]", ModColor, ModPrefix, _playerName);
                        GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, _message, _playerName, false, "ServerTools1", false);
                        return false;
                    }
                }
                if (Badwords.IsEnabled)
                {
                    bool _hasBadWord = false;
                    string _message1 = _message.ToLower();
                    foreach (string _word in Badwords.List)
                    {
                        if (_message1.Contains(_word))
                        {
                            string _replace = "";
                            for (int i = 0; i < _word.Length; i++)
                            {
                                _replace = string.Format("{0}*", _replace);
                            }
                            _message1 = _message1.Replace(_word, _replace);
                            _hasBadWord = true;
                        }
                    }
                    if (_hasBadWord)
                    {
                        GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, _message1, _playerName, false, "ServerTools", false);
                        return false;
                    }
                }
                if (_message.StartsWith("/") || _message.StartsWith("!"))
                {
                    bool _announce = false;
                    if (_message.StartsWith("!"))
                    {
                        _announce = true;
                        _message = _message.Replace("!", "");
                    }
                    if (_message.StartsWith("/"))
                    {
                        _message = _message.Replace("/", "");
                    }
                    if (_message.StartsWith("w ") || _message.StartsWith("W ") || _message.StartsWith("pm ") || _message.StartsWith("PM "))
                    {
                        if (CustomCommands.IsEnabled)
                        {
                            Whisper.Send(_cInfo, _message);
                            return false;
                        }
                    }
                    if (_message.StartsWith("r ") || _message.StartsWith("R ") || _message.StartsWith("RE ") || _message.StartsWith("re "))
                    {
                        if (CustomCommands.IsEnabled)
                        {
                            Whisper.Reply(_cInfo, _message);
                            return false;
                        }
                    }
                    _message = _message.ToLower();
                    if (_message == "sethome")
                    {
                        if (_announce)
                        {
                            GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("!{0}", _message), _playerName, false, "ServerTools", true);
                        }
                        if (TeleportHome.IsEnabled)
                        {
                            TeleportHome.SetHome(_cInfo);
                        }
                        else
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Sethome is not enabled.[-]", CustomCommands.ChatColor), "Server", false, "ServerTools", false));
                        }
                        return false;
                    }
                    if (_message == "home")
                    {
                        if (_announce)
                        {
                            GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("!{0}", _message), _playerName, false, "ServerTools", true);
                        }
                        if (TeleportHome.IsEnabled)
                        {
                            TeleportHome.TeleHome(_cInfo);
                        }
                        else
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Home is not enabled.[-]", CustomCommands.ChatColor), "Server", false, "ServerTools", false));
                        }
                        return false;
                    }
                    if (AdminChat.IsEnabled)
                    {
                        if (_message.StartsWith("mute ") || _message.StartsWith("unmute "))
                        {
                            if (_message.StartsWith("mute "))
                            {
                                MutePlayer.Add(_cInfo, _message);
                            }
                            if (_message.StartsWith("unmute "))
                            {
                                MutePlayer.Remove(_cInfo, _message);
                            }
                            return false;
                        }
                    }
                    if (_message == "commands")
                    {
                        string _commands = CustomCommands.GetChatCommands(_cInfo);
                        if (_announce)
                        {
                            GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("!{0}", _message), _playerName, false, "ServerTools", true);
                            GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, _commands, "Server", false, "ServerTools", false);
                        }
                        else
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, _commands, "Server", false, "ServerTools", false));
                        }
                        return false;
                    }
                    if (_message == "day7")
                    {
                        if (Day7.IsEnabled)
                        {
                            if (_announce)
                            {
                                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("!{0}", _message), _playerName, false, "ServerTools", true);
                            }
                            Day7.GetInfo(_cInfo, _announce);
                            return false;
                        }
                    }
                    if (_message == "killme" || _message == "wrist" || _message == "suicide")
                    {
                        if (_announce)
                        {
                            GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("!{0}", _message), _playerName, false, "ServerTools", true);
                        }
                        if (KillMe.IsEnabled)
                        {
                            KillMe.CheckPlayer(_cInfo, _announce);
                        }
                        else
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Killme is not enabled.[-]", CustomCommands.ChatColor), "Server", false, "ServerTools", false));
                        }
                        return false;
                    }
                    if (_message == "gimme" || _message == "gimmie")
                    {
                        if (_announce)
                        {
                            GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("!{0}", _message), _playerName, false, "ServerTools", true);
                        }
                        if (Gimme.AlwaysShowResponse && !_announce)
                        {
                            GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("/{0}", _message), _playerName, false, "ServerTools", true);
                        }
                        if (Gimme.IsEnabled)
                        {
                            Gimme.Checkplayer(_cInfo, _announce, _playerName);
                        }
                        else
                        {
                            return true;
                        }
                        return false;
                    }
                    if (_message.StartsWith("clanadd") || _message == "clandel" || _message.StartsWith("claninvite") || _message == "clanaccept" || _message == "clandecline" || _message.StartsWith("clanremove") || _message.StartsWith("clanpromote") || _message.StartsWith("clandemote") || _message.StartsWith("clan") || _message.StartsWith("c") || _message == "clancommands")
                    {
                        if (ClanManager.IsEnabled)
                        {
                            if (_message == "clancommands")
                            {
                                ClanManager.GetChatCommands(_cInfo);
                                return false;
                            }
                            if (_message.StartsWith("clanadd"))
                            {
                                if(_message == "clanadd")
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Usage: /clanadd clanName[-]", CustomCommands.ChatColor), "Server", false, "ServerTools", false));
                                }
                                else
                                {
                                    _message = _message.Replace("clanadd ", "");
                                    ClanManager.AddClan(_cInfo, _message);
                                }
                                return false;
                            }
                            if (_message == "clandel")
                            {
                                ClanManager.RemoveClan(_cInfo);
                                return false;
                            }
                            if (_message.StartsWith("claninvite"))
                            {
                                if(_message == "claninvite")
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Usage: /claninvite playerName[-]", CustomCommands.ChatColor), "Server", false, "ServerTools", false));
                                }
                                else
                                {
                                    _message = _message.Replace("claninvite ", "");
                                    ClanManager.InviteMember(_cInfo, _message);
                                }
                                return false;
                            }
                            if (_message == "clanaccept")
                            {
                                ClanManager.InviteAccept(_cInfo);
                                return false;
                            }
                            if (_message == "clandecline")
                            {
                                ClanManager.InviteDecline(_cInfo);
                                return false;
                            }
                            if (_message.StartsWith("clanremove"))
                            {
                                if (_message == "clanremove")
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Usage: /clanremove playerName[-]", CustomCommands.ChatColor), "Server", false, "ServerTools", false));
                                }
                                else
                                {
                                    _message = _message.Replace("clanremove ", "");
                                    ClanManager.RemoveMember(_cInfo, _message);
                                }
                                return false;
                            }
                            if (_message.StartsWith("clanpromote"))
                            {
                                if (_message == "clanpromote")
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Usage: /clanpromote playerName[-]", CustomCommands.ChatColor), "Server", false, "ServerTools", false));
                                }
                                else
                                {
                                    _message = _message.Replace("clanpromote ", "");
                                    ClanManager.PromoteMember(_cInfo, _message);
                                }
                                return false;
                            }
                            if (_message.StartsWith("clandemote"))
                            {
                                if (_message == "clandemote")
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Usage: /clandemote playerName[-]", CustomCommands.ChatColor), "Server", false, "ServerTools", false));
                                }
                                else
                                {
                                    _message = _message.Replace("clandemote ", "");
                                    ClanManager.DemoteMember(_cInfo, _message);
                                }
                                return false;
                            }
                            if (_message == "clanleave")
                            {
                                ClanManager.LeaveClan(_cInfo);
                                return false;
                            }
                            if (_message.StartsWith("clan"))
                            {
                                if (_message == "clan")
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Usage: /clan message[-]", CustomCommands.ChatColor), "Server", false, "ServerTools", false));
                                }
                                else
                                {
                                    _message = _message.Replace("clan ", "");
                                    ClanManager.Clan(_cInfo, _message);
                                }
                                return false;
                            }
                            if (_message.StartsWith("c"))
                            {
                                if (_message == "c")
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Usage: /c message[-]", CustomCommands.ChatColor), "Server", false, "ServerTools", false));
                                }
                                else
                                {
                                    _message = _message.Replace("c ", "");
                                    ClanManager.Clan(_cInfo, _message);
                                }
                                return false;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                    if (CustomCommands.IsEnabled && CustomCommands.Dict.Count > 0 && CustomCommands.Dict.ContainsKey(_message))
                    {
                        string[] _r;
                        if (CustomCommands.Dict.TryGetValue(_message, out _r))
                        {
                            string _response = _r[0];
                            _response = _response.Replace("{EntityId}", _cInfo.entityId.ToString());
                            _response = _response.Replace("{SteamId}", _cInfo.playerId);
                            _response = _response.Replace("{PlayerName}", _playerName);
                            if (_announce)
                            {
                                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("!{0}", _message), _playerName, false, "ServerTools", true);
                            }
                            if (_response.StartsWith("say "))
                            {
                                if (_announce)
                                {
                                    SdtdConsole.Instance.ExecuteSync(_response, _cInfo);
                                }
                                else
                                {
                                    _response = _response.Replace("say ", "");
                                    _response = _response.Replace("\"", "");
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format(_response), "Server", false, "ServerTools", false));
                                }
                            }
                            else
                            {
                                SdtdConsole.Instance.ExecuteSync(_response, _cInfo);
                            }
                        }
                        return false;
                    }
                }
                if (_message.StartsWith("@"))
                {
                    if (_message.StartsWith("@admins ") || _message.StartsWith("@ADMINS "))
                    {
                        if (!AdminChat.IsEnabled)
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}AdminChat is not enabled.[-]", CustomCommands.ChatColor), "Server", false, "ServerTools", false));
                        }
                        else
                        {
                            AdminChat.SendAdmins(_cInfo, _message);
                        }
                        return false;
                    }
                    if (_message.StartsWith("@all ") || _message.StartsWith("@ALL "))
                    {
                        if (!AdminChat.IsEnabled)
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}AdminChat is not enabled.[-]", CustomCommands.ChatColor), "Server", false, "ServerTools", false));
                        }
                        else
                        {
                            AdminChat.SendAll(_cInfo, _message);
                        }
                        return false;
                    }
                }
            }
            return true;
        }
    }
}