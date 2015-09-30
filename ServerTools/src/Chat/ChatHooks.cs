namespace ServerTools
{
    public class ChatHook
    {
        public static bool IsCommand(ClientInfo _cInfo, string _message, string _playerName)
        {
            if (!string.IsNullOrEmpty(_message) && _cInfo != null && _playerName != "" && _playerName != "Server")
            {
                if(AdminChat.MutedPlayersList.Contains(_cInfo.playerId))
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{0}You are currently muted.[-]", CustomCommands._chatcolor), "Server"));
                    return false;
                }
                string _filter = "[ffffffff][/url][/b][/i][/u][/s][/sub][/sup][ff]";
                if (_message.EndsWith(_filter + _filter))
                {
                    _message = _message.Remove(_message.Length - 2 * _filter.Length);
                }
                if (ChatLog.IsEnabled && !_message.EndsWith(_filter))
                {
                    ChatLog.Send(_message, _playerName);
                }
                if(Badwords.IsEnabled && !_message.EndsWith(_filter))
                {
                    string _message1 = _message.ToLower();
                    foreach (string _word in Badwords.BadWordslist)
                    {
                        if (_message1.Contains(_word))
                        {
                            _message1 = _message1.Replace(_word, "*****");
                            GameManager.Instance.GameMessageServer(_cInfo, _message1, _playerName);
                            return false;
                        }
                    }
                }
                if (_message.StartsWith("/") || _message.StartsWith("!") || _message.StartsWith("@"))
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
                    if (_message.StartsWith("mute ") || _message.StartsWith("unmute "))
                    {
                        if (AdminChat.IsEnabled)
                        {
                            if (_message.StartsWith("mute "))
                            {
                                _message = _message.Replace("mute ", "");
                                AdminChat.MutePlayer(_cInfo, _message);
                            }
                            if(_message.StartsWith("unmute "))
                            {
                                _message = _message.Replace("unmute ", "");
                                AdminChat.UnMutePlayer(_cInfo, _message);
                            }
                        }
                        else
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{0}AdminChat is not enabled.[-]", CustomCommands._chatcolor), "Server"));
                        }
                        return false;
                    }
                    if (_message.StartsWith("clanadd ") || _message == "clandel" || _message.StartsWith("claninvite ") || _message == "clanaccept" || _message == "clandecline" || _message.StartsWith("clanremove ") || _message.StartsWith("clanpromote ") || _message.StartsWith("clandemote "))
                    {
                        if (ClanManager.IsEnabled)
                        {
                            if (_message.StartsWith("clanadd "))
                            {
                                _message = _message.Replace("clanadd ", "");
                                ClanManager.AddClan(_cInfo, _message);
                            }
                            if (_message == "clandel")
                            {
                                ClanManager.RemoveClan(_cInfo);
                            }
                            if (_message.StartsWith("claninvite "))
                            {
                                _message = _message.Replace("claninvite ", "");
                                ClanManager.InviteMember(_cInfo, _message);
                            }
                            if (_message == "clanaccept")
                            {
                                ClanManager.InviteAccept(_cInfo);
                            }
                            if (_message == "clandecline")
                            {
                                ClanManager.InviteDecline(_cInfo);
                            }
                            if (_message.StartsWith("clanremove "))
                            {
                                _message = _message.Replace("clanremove ", "");
                                ClanManager.RemoveMember(_cInfo, _message);
                            }
                            if (_message.StartsWith("clanpromote "))
                            {
                                _message = _message.Replace("clanpromote ", "");
                                ClanManager.PromoteMember(_cInfo, _message);
                            }
                            if (_message.StartsWith("clandemote "))
                            {
                                _message = _message.Replace("clandemote ", "");
                                ClanManager.DemoteMember(_cInfo, _message);
                            }
                        }
                        else
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{0}ClanManager is not enabled.[-]", CustomCommands._chatcolor), "Server"));
                        }
                        return false;
                    }
                    _message = _message.ToLower();
                    if (_message.StartsWith("@admins ") || _message.StartsWith("@all "))
                    {
                        if (!AdminChat.IsEnabled)
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{0}AdminChat is not enabled.[-]", CustomCommands._chatcolor), "Server"));
                        }
                        else
                        {
                            if (_message.StartsWith("@admins "))
                            {
                                _message = _message.Replace("@admins ", "");
                                AdminChat.SendAdmins(_cInfo, _message);
                            }
                            if (_message.StartsWith("@all "))
                            {
                                _message = _message.Replace("@all ", "");
                                AdminChat.SendAll(_cInfo, _message);
                            }
                        }
                        return false;
                    }
                    if (_message == "info" || _message == "help" || _message == "commands")
                    {
                        string _commands = CustomCommands.GetChatCommands(_cInfo);
                        if (_announce)
                        {
                            GameManager.Instance.GameMessageServer(_cInfo, string.Format("!{0}", _message), _playerName);
                            GameManager.Instance.GameMessageServer(_cInfo, _commands, "Server");
                        }
                        else
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(_commands, "Server"));
                        }
                        return false;
                    }
                    if (_message == "killme" || _message == "wrist" || _message == "suicide")
                    {
                        if (_announce)
                        {
                            GameManager.Instance.GameMessageServer(_cInfo, string.Format("!{0}", _message), _playerName);
                        }
                        if (KillMe.IsEnabled)
                        {
                            KillMe.KillPlayer(_cInfo, _announce, _message, _playerName);
                        }
                        else
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{0}Killme is not enabled.[-]", CustomCommands._chatcolor), "Server"));
                        }
                        return false;
                    }
                    if (_message == "gimme" || _message == "gimmie")
                    {
                        if (_announce)
                        {
                            GameManager.Instance.GameMessageServer(_cInfo, string.Format("!{0}", _message), _playerName);
                        }
                        if (Gimme.AlwaysShowResponse && !_announce)
                        {
                            GameManager.Instance.GameMessageServer(_cInfo, string.Format("/{0}", _message), _playerName);
                        }
                        if (Gimme.IsEnabled)
                        {
                            Gimme.Checkplayer(_cInfo, _announce, _playerName);
                        }
                        else
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{0}Gimme is not enabled.[-]", CustomCommands._chatcolor), "Server"));
                        }
                        return false;
                    }
                    if (_message == "sethome")
                    {
                        if (_announce)
                        {
                            GameManager.Instance.GameMessageServer(_cInfo, string.Format("!{0}", _message), _playerName);
                        }
                        if (TeleportHome.IsEnabled)
                        {
                            TeleportHome.SetHome(_cInfo);
                        }
                        else
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{0}Sethome is not enabled.[-]", CustomCommands._chatcolor), "Server"));
                        }
                        return false;
                    }
                    if (_message == "delhome")
                    {
                        if (_announce)
                        {
                            GameManager.Instance.GameMessageServer(_cInfo, string.Format("!{0}", _message), _playerName);
                        }
                        if (TeleportHome.IsEnabled)
                        {
                            TeleportHome.DelHome(_cInfo);
                        }
                        else
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{0}Delhome is not enabled.[-]", CustomCommands._chatcolor), "Server"));
                        }
                        return false;
                    }
                    if (_message == "home")
                    {
                        if (_announce)
                        {
                            GameManager.Instance.GameMessageServer(_cInfo, string.Format("!{0}", _message), _playerName);
                        }
                        if (TeleportHome.IsEnabled)
                        {
                            TeleportHome.TeleHome(_cInfo);
                        }
                        else
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{0}Home is not enabled.[-]", CustomCommands._chatcolor), "Server"));
                        }
                        return false;
                    }
                    if (_message == "day7")
                    {
                        if (_announce)
                        {
                            GameManager.Instance.GameMessageServer(_cInfo, string.Format("!{0}", _message), _playerName);
                        }
                        if (Day7.IsEnabled)
                        {
                            Day7.GetInfo(_cInfo, _announce);
                        }
                        return false;
                    }
                    string _response = null;
                    if (CustomCommands.IsEnabled && CustomCommands._customCommands.TryGetValue(_message, out _response))
                    {
                        _response = _response.Replace("{0}", _cInfo.entityId.ToString());
                        _response = _response.Replace("{1}", _cInfo.playerId);
                        _response = _response.Replace("{2}", _playerName);
                        if (_announce)
                        {
                            GameManager.Instance.GameMessageServer(_cInfo, string.Format("!{0}", _message), _playerName);
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
                                _cInfo.SendPackage(new NetPackageGameMessage(string.Format(_response), "Server"));
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.ExecuteSync(_response, _cInfo);
                        }
                        return false;
                    }
                }
            }
            return true;
        }
    }
}