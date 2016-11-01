using System.Collections.Generic;

namespace ServerTools
{
    public class ClanManager
    {
        public static bool IsEnabled = false;

        public static void CheckforClantag(ClientInfo _cInfo)
        {
            string _pName = _cInfo.playerName.ToLower();
            foreach (string _clan in PersistentContainer.Instance.Players.ClanList)
            {
                if (_pName.Contains(_clan))
                {
                    Player p = PersistentContainer.Instance.Players[_cInfo.playerId, true];
                    if (p.ClanName != _clan)
                    {
                        string _phrase100;
                        if (!Phrases.Dict.TryGetValue(100, out _phrase100))
                        {
                            _phrase100 = "You do not belong to the clan {ClanName}. Please remove the clan tag and rejoin.";
                        }
                        _phrase100 = _phrase100.Replace("{ClanName}", _clan);
                        SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.entityId, _phrase100), _cInfo);
                    }
                }
            }
        }

        public static void AddClan(ClientInfo _cInfo, string _clanName)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, true];
            if (p.IsClanOwner)
            {
                string _phrase101;
                if (!Phrases.Dict.TryGetValue(101, out _phrase101))
                {
                    _phrase101 = "{PlayerName} you have already created the clan {ClanName}.";
                }
                _phrase101 = _phrase101.Replace("{PlayerName}", _cInfo.playerName);
                _phrase101 = _phrase101.Replace("{ClanName}", p.ClanName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase101, CustomCommands.ChatColor), "Server", false, "", false));
            }
            else
            {
                if (p.ClanName != null)
                {
                    string _phrase103;
                    if (!Phrases.Dict.TryGetValue(103, out _phrase103))
                    {
                        _phrase103 = "{PlayerName} you are currently a member of the clan {ClanName}.";
                    }
                    _phrase103 = _phrase103.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase103 = _phrase103.Replace("{ClanName}", p.ClanName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase103, CustomCommands.ChatColor), "Server", false, "", false));
                }
                else
                {
                    _clanName = _clanName.ToLower();
                    if (PersistentContainer.Instance.Players.ClanList.Contains(_clanName))
                    {
                        string _phrase102;
                        if (!Phrases.Dict.TryGetValue(102, out _phrase102))
                        {
                            _phrase102 = "{PlayerName} can not add the clan {ClanName} because it already exist.";
                        }
                        _phrase102 = _phrase102.Replace("{PlayerName}", _cInfo.playerName);
                        _phrase102 = _phrase102.Replace("{ClanName}", _clanName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase102, CustomCommands.ChatColor), "Server", false, "", false));
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(_clanName) || _clanName.Length < 3)
                        {
                            string _phrase129;
                            if (!Phrases.Dict.TryGetValue(129, out _phrase129))
                            {
                                _phrase129 = "{PlayerName} the clanName must be longer the 3 characters";
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase129, CustomCommands.ChatColor), "Server", false, "", false));
                            }
                            _phrase129 = _phrase129.Replace("{PlayerName}", _cInfo.playerName);
                        }
                        else
                        {
                            PersistentContainer.Instance.Players[_cInfo.playerId, true].ClanName = _clanName;
                            PersistentContainer.Instance.Players[_cInfo.playerId, true].IsClanOwner = true;
                            PersistentContainer.Instance.Players[_cInfo.playerId, true].IsClanOfficer = true;
                            PersistentContainer.Instance.Save();
                            PersistentContainer.Instance.Players.clans.Add(_clanName, _cInfo.playerId);
                            string _phrase104;
                            if (!Phrases.Dict.TryGetValue(104, out _phrase104))
                            {
                                _phrase104 = "{PlayerName} you have added the clan {ClanName}.";
                            }
                            _phrase104 = _phrase104.Replace("{PlayerName}", _cInfo.playerName);
                            _phrase104 = _phrase104.Replace("{ClanName}", _clanName);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase104, CustomCommands.ChatColor), "Server", false, "", false));
                        } 
                    }
                }
            }
        }

        public static void RemoveClan(ClientInfo _cInfo)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, true];
            if (!p.IsClanOwner)
            {
                string _phrase105;
                if (!Phrases.Dict.TryGetValue(105, out _phrase105))
                {
                    _phrase105 = "{PlayerName} you are not the owner of any clans.";
                }
                _phrase105 = _phrase105.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase105, CustomCommands.ChatColor), "Server", false, "", false));
            }
            else
            {
                List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                foreach (ClientInfo _cInfo1 in _cInfoList)
                {
                    Player p1 = PersistentContainer.Instance.Players[_cInfo1.playerId, true];
                    if (p1.ClanName == p.ClanName && !p1.IsClanOwner)
                    {
                        PersistentContainer.Instance.Players[_cInfo1.playerId, false].ClanName = null;
                        if (p1.IsClanOfficer)
                        {
                            PersistentContainer.Instance.Players[_cInfo1.playerId, false].IsClanOfficer = false;
                        }
                        string _phrase121;
                        if (!Phrases.Dict.TryGetValue(121, out _phrase121))
                        {
                            _phrase121 = "{PlayerName} you have been removed from the clan {ClanName}.";
                        }
                        _phrase121 = _phrase121.Replace("{PlayerName}", _cInfo1.playerName);
                        _phrase121 = _phrase121.Replace("{ClanName}", p.ClanName);
                        _cInfo1.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase121, CustomCommands.ChatColor), "Server", false, "", false));
                    }
                }
                foreach (string _id in PersistentContainer.Instance.Players.SteamIDs)
                {
                    Player p1 = PersistentContainer.Instance.Players[_id, true];
                    if (p1.InvitedToClan == p.ClanName)
                    {
                        PersistentContainer.Instance.Players[_id, false].InvitedToClan = null;
                    }
                    if (p1.ClanName == p.ClanName && p1.IsClanOfficer && !p1.IsClanOwner)
                    {
                        PersistentContainer.Instance.Players[_id, false].IsClanOfficer = false;
                    }
                    if (p1.ClanName == p.ClanName && !p1.IsClanOwner)
                    {
                        PersistentContainer.Instance.Players[_id, false].ClanName = null;
                    }
                }
                string _phrase106;
                if (!Phrases.Dict.TryGetValue(106, out _phrase106))
                {
                    _phrase106 = "{PlayerName} you have removed the clan {ClanName}.";
                }
                _phrase106 = _phrase106.Replace("{PlayerName}", _cInfo.playerName);
                _phrase106 = _phrase106.Replace("{ClanName}", p.ClanName);
                PersistentContainer.Instance.Players.clans.Remove(p.ClanName);
                PersistentContainer.Instance.Players[_cInfo.playerId, false].ClanName = null;
                PersistentContainer.Instance.Players[_cInfo.playerId, false].IsClanOfficer = false;
                PersistentContainer.Instance.Players[_cInfo.playerId, false].IsClanOwner = false;
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase106, CustomCommands.ChatColor), "Server", false, "", false));
                PersistentContainer.Instance.Save();
            }
        }

        public static void InviteMember(ClientInfo _cInfo, string _playerName)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, true];
            if (!p.IsClanOwner || !p.IsClanOfficer)
            {
                string _phrase107;
                if (!Phrases.Dict.TryGetValue(107, out _phrase107))
                {
                    _phrase107 = "{PlayerName} you do not have permissions to use this command.";
                }
                _phrase107 = _phrase107.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase107, CustomCommands.ChatColor), "Server", false, "", false));
            }
            else
            {
                ClientInfo _newMember = ConsoleHelper.ParseParamIdOrName(_playerName);
                if (_newMember == null)
                {
                    string _phrase108;
                    if (!Phrases.Dict.TryGetValue(108, out _phrase108))
                    {
                        _phrase108 = "{PlayerName} the name {TargetPlayerName} was not found.";
                    }
                    _phrase108 = _phrase108.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase108 = _phrase108.Replace("{TargetPlayerName}", _playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase108, CustomCommands.ChatColor), "Server", false, "", false));
                }
                else
                {
                    Player p1 = PersistentContainer.Instance.Players[_newMember.playerId, true];
                    if (p1.ClanName != null)
                    {
                        string _phrase109;
                        if (!Phrases.Dict.TryGetValue(109, out _phrase109))
                        {
                            _phrase109 = "{PlayerName} is already a member of a clan.";
                        }
                        _phrase109 = _phrase109.Replace("{PlayerName}", _playerName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase109, CustomCommands.ChatColor), "Server", false, "", false));
                    }
                    else
                    {
                        if (p1.InvitedToClan != null)
                        {
                            string _phrase110;
                            if (!Phrases.Dict.TryGetValue(110, out _phrase110))
                            {
                                _phrase110 = "{PlayerName} already has pending clan invites.";
                            }
                            _phrase110 = _phrase110.Replace("{PlayerName}", _playerName);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase110, CustomCommands.ChatColor), "Server", false, "", false));
                        }
                        else
                        {
                            string _phrase111;
                            string _phrase112;
                            if (!Phrases.Dict.TryGetValue(111, out _phrase111))
                            {
                                _phrase111 = "{PlayerName} you have been invited to join the clan {ClanName}. Type /clanaccept to join or /clandecline to decline the offer.";
                            }

                            if (!Phrases.Dict.TryGetValue(112, out _phrase112))
                            {
                                _phrase112 = "{PlayerName} you have invited {InvitedPlayerName} to the clan {ClanName}.";
                            }
                            _phrase111 = _phrase111.Replace("{PlayerName}", _newMember.playerName);
                            _phrase111 = _phrase111.Replace("{ClanName}", p.ClanName);
                            _phrase112 = _phrase112.Replace("{PlayerName}", _cInfo.playerName);
                            _phrase112 = _phrase112.Replace("{InvitedPlayerName}", _newMember.playerName);
                            _phrase112 = _phrase112.Replace("{ClanName}", p.ClanName);
                            PersistentContainer.Instance.Players[_newMember.playerId, true].InvitedToClan = p.ClanName;
                            PersistentContainer.Instance.Save();
                            _newMember.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase111, CustomCommands.ChatColor), "Server", false, "", false));
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase112, CustomCommands.ChatColor), "Server", false, "", false));
                        }
                    }
                }
            }
        }

        public static void InviteAccept(ClientInfo _cInfo)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, true];
            if (p.InvitedToClan == null)
            {
                string _phrase113;
                if (!Phrases.Dict.TryGetValue(113, out _phrase113))
                {
                    _phrase113 = "{PlayerName} you have not been invited to any clans.";
                }
                _phrase113 = _phrase113.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase113, CustomCommands.ChatColor), "Server", false, "", false));
            }
            else
            {
                PersistentContainer.Instance.Players[_cInfo.playerId, false].ClanName = p.InvitedToClan;
                PersistentContainer.Instance.Players[_cInfo.playerId, false].InvitedToClan = null;
                PersistentContainer.Instance.Save();
                List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                foreach (ClientInfo _cInfo1 in _cInfoList)
                {
                    Player p1 = PersistentContainer.Instance.Players[_cInfo1.playerId, true];
                    if (p1.ClanName == p.ClanName)
                    {
                        string _phrase115;
                        if (!Phrases.Dict.TryGetValue(115, out _phrase115))
                        {
                            _phrase115 = "{PlayerName} has joined the clan.";
                        }
                        _phrase115 = _phrase115.Replace("{PlayerName}", _cInfo.playerName);
                        _cInfo1.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase115, CustomCommands.ChatColor), "Server", false, "", false));
                    }
                }
            }
        }

        public static void InviteDecline(ClientInfo _cInfo)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, true];
            if (p.InvitedToClan == null)
            {
                string _phrase113;
                if (!Phrases.Dict.TryGetValue(113, out _phrase113))
                {
                    _phrase113 = "{PlayerName} you have not been invited to any clans.";
                }
                _phrase113 = _phrase113.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase113, CustomCommands.ChatColor), "Server", false, "", false));
            }
            else
            {
                PersistentContainer.Instance.Players[_cInfo.playerId, false].InvitedToClan = null;
                PersistentContainer.Instance.Save();
                string _phrase116;
                if (!Phrases.Dict.TryGetValue(116, out _phrase116))
                {
                    _phrase116 = "{PlayerName} you have declined the invite to the clan.";
                }
                _phrase116 = _phrase116.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase116, CustomCommands.ChatColor), "Server", false, "", false));
            }
        }

        public static void RemoveMember(ClientInfo _cInfo, string _playerName)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, true];
            if (!p.IsClanOfficer)
            {
                string _phrase107;
                if (!Phrases.Dict.TryGetValue(107, out _phrase107))
                {
                    _phrase107 = "{PlayerName} you do not have permissions to use this command.";
                }
                _phrase107 = _phrase107.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase107, CustomCommands.ChatColor), "Server", false, "", false));
            }
            else
            {
                string _steamId;
                ClientInfo _PlayertoRemove = ConsoleHelper.ParseParamIdOrName(_playerName);
                if (_PlayertoRemove != null)
                {
                    _steamId = _PlayertoRemove.playerId;
                }
                else
                {
                    if (ConsoleHelper.ParseParamSteamIdValid(_playerName))
                    {
                        _steamId = _playerName;
                    }
                    else
                    {
                        string _phrase108;
                        if (!Phrases.Dict.TryGetValue(108, out _phrase108))
                        {
                            _phrase108 = "{PlayerName} the name {TargetPlayerName} was not found.";
                        }
                        _phrase108 = _phrase108.Replace("{PlayerName}", _cInfo.playerName);
                        _phrase108 = _phrase108.Replace("{TargetPlayerName}", _playerName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase108, CustomCommands.ChatColor), "Server", false, "", false));
                        return;
                    }
                }
                Player p1 = PersistentContainer.Instance.Players[_steamId, true];
                if (p.ClanName != p1.ClanName)
                {
                    string _phrase117;
                    if (!Phrases.Dict.TryGetValue(117, out _phrase117))
                    {
                        _phrase117 = "{PlayerName} is not a member of your clan.";
                    }
                    _phrase117 = _phrase117.Replace("{PlayerName}", _playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase117, CustomCommands.ChatColor), "Server", false, "", false));
                }
                else
                {
                    if (p1.IsClanOfficer && !p.IsClanOwner && !p1.IsClanOwner)
                    {
                        string _phrase118;
                        if (!Phrases.Dict.TryGetValue(118, out _phrase118))
                        {
                            _phrase118 = "{PlayerName} only the clan owner can remove officers.";
                        }
                        _phrase118 = _phrase118.Replace("{PlayerName}", _cInfo.playerName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase118, CustomCommands.ChatColor), "Server", false, "", false));
                    }
                    else
                    {
                        PersistentContainer.Instance.Players[_steamId, false].ClanName = null;
                        PersistentContainer.Instance.Players[_steamId, false].IsClanOfficer = false;
                        PersistentContainer.Instance.Save();
                        string _phrase120;
                        string _phrase121;
                        if (!Phrases.Dict.TryGetValue(120, out _phrase120))
                        {
                            _phrase120 = "{PlayerName} you have removed {PlayertoRemove} from clan {ClanName}.";
                        }
                        if (!Phrases.Dict.TryGetValue(121, out _phrase121))
                        {
                            _phrase121 = "{PlayerName} you have been removed from the clan {ClanName}.";
                        }
                        _phrase120 = _phrase120.Replace("{PlayerName}", _cInfo.playerName);
                        _phrase120 = _phrase120.Replace("{PlayertoRemove}", _playerName);
                        _phrase120 = _phrase120.Replace("{ClanName}", p.ClanName);
                        _phrase121 = _phrase121.Replace("{PlayerName}", _playerName);
                        _phrase121 = _phrase121.Replace("{ClanName}", p.ClanName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase120, CustomCommands.ChatColor), "Server", false, "", false));
                        if (_PlayertoRemove != null)
                        {
                            _PlayertoRemove.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase121, CustomCommands.ChatColor), "Server", false, "", false));
                        }
                    }
                }
            }
        }

        public static void PromoteMember(ClientInfo _cInfo, string _playerName)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, true];
            if (!p.IsClanOwner)
            {
                string _phrase107;
                if (!Phrases.Dict.TryGetValue(107, out _phrase107))
                {
                    _phrase107 = "{PlayerName} you do not have permissions to use this command.";
                }
                _phrase107 = _phrase107.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase107, CustomCommands.ChatColor), "Server", false, "", false));
            }
            else
            {
                ClientInfo _playertoPromote = ConsoleHelper.ParseParamIdOrName(_playerName);
                if (_playertoPromote == null)
                {
                    string _phrase108;
                    if (!Phrases.Dict.TryGetValue(108, out _phrase108))
                    {
                        _phrase108 = "{PlayerName} the name {TargetPlayerName} was not found.";
                    }
                    _phrase108 = _phrase108.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase108 = _phrase108.Replace("{TargetPlayerName}", _playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase108, CustomCommands.ChatColor), "Server", false, "", false));
                }
                else
                {
                    Player p1 = PersistentContainer.Instance.Players[_playertoPromote.playerId, true];
                    if (p.ClanName != p1.ClanName)
                    {
                        string _phrase117;
                        if (!Phrases.Dict.TryGetValue(117, out _phrase117))
                        {
                            _phrase117 = "{PlayerName} is not a member of your clan.";
                        }
                        _phrase117 = _phrase117.Replace("{PlayerName}", _playerName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase117, CustomCommands.ChatColor), "Server", false, "", false));
                    }
                    else
                    {
                        if (p1.IsClanOfficer)
                        {
                            string _phrase122;
                            if (!Phrases.Dict.TryGetValue(122, out _phrase122))
                            {
                                _phrase122 = "{PlayerName} is already a officer.";
                            }
                            _phrase122 = _phrase122.Replace("{PlayerName}", _playerName);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase122, CustomCommands.ChatColor), "Server", false, "", false));
                        }
                        else
                        {
                            PersistentContainer.Instance.Players[_playertoPromote.playerId, false].IsClanOfficer = true;
                            PersistentContainer.Instance.Save();
                            string _phrase123;
                            if (!Phrases.Dict.TryGetValue(123, out _phrase123))
                            {
                                _phrase123 = "{PlayerName} has been promoted to an officer.";
                            }
                            _phrase123 = _phrase123.Replace("{PlayerName}", _playerName);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase123, CustomCommands.ChatColor), "Server", false, "", false));
                        }
                    }
                }
            }
        }

        public static void DemoteMember(ClientInfo _cInfo, string _playerName)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, true];
            if (!p.IsClanOwner)
            {
                string _phrase107;
                if (!Phrases.Dict.TryGetValue(107, out _phrase107))
                {
                    _phrase107 = "{PlayerName} you do not have permissions to use this command.";
                }
                _phrase107 = _phrase107.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase107, CustomCommands.ChatColor), "Server", false, "", false));
            }
            else
            {
                ClientInfo _membertoDemote = ConsoleHelper.ParseParamIdOrName(_playerName);
                if (_membertoDemote == null)
                {
                    string _phrase108;
                    if (!Phrases.Dict.TryGetValue(108, out _phrase108))
                    {
                        _phrase108 = "{PlayerName} the name {TargetPlayerName} was not found.";
                    }
                    _phrase108 = _phrase108.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase108 = _phrase108.Replace("{TargetPlayerName}", _playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase108, CustomCommands.ChatColor), "Server", false, "", false));
                }
                else
                {
                    Player p1 = PersistentContainer.Instance.Players[_membertoDemote.playerId, true];
                    if (p.ClanName != p1.ClanName)
                    {
                        string _phrase117;
                        if (!Phrases.Dict.TryGetValue(117, out _phrase117))
                        {
                            _phrase117 = "{PlayerName} is not a member of your clan.";
                        }
                        _phrase117 = _phrase117.Replace("{PlayerName}", _playerName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase117, CustomCommands.ChatColor), "Server", false, "", false));
                    }
                    else
                    {
                        if (!p1.IsClanOfficer)
                        {
                            string _phrase124;
                            if (!Phrases.Dict.TryGetValue(124, out _phrase124))
                            {
                                _phrase124 = "{PlayerName} is not an officer.";
                            }
                            _phrase124 = _phrase124.Replace("{PlayerName}", _playerName);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase124, CustomCommands.ChatColor), "Server", false, "", false));
                        }
                        else
                        {
                            PersistentContainer.Instance.Players[_membertoDemote.playerId, false].IsClanOfficer = false;
                            PersistentContainer.Instance.Save();
                            string _phrase125;
                            if (!Phrases.Dict.TryGetValue(125, out _phrase125))
                            {
                                _phrase125 = "{PlayerName} has been demoted.";
                            }
                            _phrase125 = _phrase125.Replace("{PlayerName}", _playerName);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase125, CustomCommands.ChatColor), "Server", false, "", false));
                        }
                    }
                }
            }
        }

        public static void LeaveClan(ClientInfo _cInfo)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, true];
            if (p.IsClanOwner)
            {
                string _phrase126;
                if (!Phrases.Dict.TryGetValue(126, out _phrase126))
                {
                    _phrase126 = "{PlayerName} you can not leave the clan because you are the owner. You can only delete the clan.";
                }
                _phrase126 = _phrase126.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase126, CustomCommands.ChatColor), "Server", false, "", false));
            }
            else
            {
                if (p.ClanName == null)
                {
                    string _phrase127;
                    if (!Phrases.Dict.TryGetValue(127, out _phrase127))
                    {
                        _phrase127 = "{PlayerName} you do not belong to any clans.";
                    }
                    _phrase127 = _phrase127.Replace("{PlayerName}", _cInfo.playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase127, CustomCommands.ChatColor), "Server", false, "", false));
                }
                else
                {
                    string _phrase121;
                    if (!Phrases.Dict.TryGetValue(121, out _phrase121))
                    {
                        _phrase121 = "{PlayerName} you have been removed from the clan {ClanName}.";
                    }
                    _phrase121 = _phrase121.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase121 = _phrase121.Replace("{ClanName}", p.ClanName);
                    PersistentContainer.Instance.Players[_cInfo.playerId, false].ClanName = null;
                    if (p.IsClanOfficer)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId, false].IsClanOfficer = false;
                    }
                    PersistentContainer.Instance.Save();
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase121, CustomCommands.ChatColor), "Server", false, "", false));
                }
            }
        }

        private static void RequestToJoinClan(ClientInfo _cInfo, string _clanName)
        {
        }

        public static void GetChatCommands(ClientInfo _cInfo)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, true];
            string _commands = string.Format("{0}Clan commands are:", CustomCommands.ChatColor); ;
            if (!p.IsClanOwner && !p.IsClanOfficer && p.ClanName == null && p.InvitedToClan == null)
            {
                _commands = string.Format("{0} /clanadd", _commands);
            }
            if (p.IsClanOwner)
            {
                _commands = string.Format("{0} /clanpromote /clandemote /clandel", _commands);
            }
            if (p.IsClanOwner || p.IsClanOfficer)
            {
                _commands = string.Format("{0} /claninvite /clanremove", _commands);
            }
            if (p.InvitedToClan != null)
            {
                _commands = string.Format("{0} /clanaccept /clandecline", _commands);
            }
            if (!p.IsClanOwner && p.ClanName != null)
            {
                _commands = string.Format("{0} /clanleave", _commands);
            }
            _commands = string.Format("{0}[-]", _commands);
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}", _commands), "Server", false, "ServerTools", false));
        }

        public static void Clan(ClientInfo _cInfo, string _message)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
            foreach (ClientInfo _cInfo1 in _cInfoList)
            {
                Player p1 = PersistentContainer.Instance.Players[_cInfo1.playerId, false];
                if (p.ClanName == p1.ClanName)
                {
                    _cInfo1.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("[804040]{0}[-]", _message), _cInfo.playerName, false, "", false));
                }
            }
        }
    }
}