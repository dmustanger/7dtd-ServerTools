using System.Collections.Generic;

namespace ServerTools
{
    public class ClanManager
    {
        public static bool IsEnabled = false;
        
        public static void CheckforClantag(ClientInfo _cInfo)
        {
            foreach (string _clan in ClanData.ClansList)
            {
                if (_cInfo.playerName.Contains(_clan))
                {
                    string _phrase100 = "You do not belong to the clan {ClanName}. Please remove the clan tag and rejoin.";
                    if (Phrases._Phrases.TryGetValue(100, out _phrase100))
                    {
                        _phrase100 = _phrase100.Replace("{ClanName}", _clan);
                    }
                    string _clan1;
                    if (!ClanData.Players.TryGetValue(_cInfo.playerId, out _clan1))
                    {
                        SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.entityId, _phrase100), _cInfo);
                    }
                    else
                    {
                        if (_clan != _clan1)
                        {
                            SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.entityId, _phrase100), _cInfo);
                        }
                    }
                }
            }
        }

        public static void AddClan(ClientInfo _cInfo, string _clanName)
        {
            if (ClanData.OwnersList.Contains(_cInfo.playerId))
            {
                string _name;
                if (ClanData.Players.TryGetValue(_cInfo.playerId, out _name))
                {
                    string _phrase101 = "{PlayerName} you have already created the clan {ClanName}.";
                    if (Phrases._Phrases.TryGetValue(101, out _phrase101))
                    {
                        _phrase101 = _phrase101.Replace("{PlayerName}", _cInfo.playerName);
                        _phrase101 = _phrase101.Replace("{ClanName}", _name);
                    }
                    _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase101, CustomCommands._chatcolor), "Server"));
                }
            }
            else if (ClanData.ClansList.Contains(_clanName))
            {
                string _phrase102 = "{PlayerName} can not add the clan {ClanName} because it already exist.";
                if (Phrases._Phrases.TryGetValue(102, out _phrase102))
                {
                    _phrase102 = _phrase102.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase102 = _phrase102.Replace("{ClanName}", _clanName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase102, CustomCommands._chatcolor), "Server"));
            }
            else if (ClanData.PlayersList.Contains(_cInfo.playerId))
            {
                string _phrase103 = "{PlayerName} you are currently a member of the clan {ClanName}.";
                if (Phrases._Phrases.TryGetValue(103, out _phrase103))
                {
                    _phrase103 = _phrase103.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase103 = _phrase103.Replace("{ClanName}", _clanName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase103, CustomCommands._chatcolor), "Server"));
            }
            else
            {
                ClanData.AddClan(_clanName, _cInfo.playerId);
                string _phrase104 = "{PlayerName} you have add the clan {ClanName}.";
                if (Phrases._Phrases.TryGetValue(104, out _phrase104))
                {
                    _phrase104 = _phrase104.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase104 = _phrase104.Replace("{ClanName}", _clanName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase104, CustomCommands._chatcolor), "Server"));
            }
        }

        public static void RemoveClan(ClientInfo _cInfo)
        {
            if (!ClanData.OwnersList.Contains(_cInfo.playerId))
            {
                string _phrase105 = "{PlayerName} you are not the owner of any clans.";
                if (Phrases._Phrases.TryGetValue(105, out _phrase105))
                {
                    _phrase105 = _phrase105.Replace("{PlayerName}", _cInfo.playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase105, CustomCommands._chatcolor), "Server"));
            }
            else
            {
                string _clanName;
                if(ClanData.Owners.TryGetValue(_cInfo.playerId, out _clanName))
                {
                    ClanData.RemoveClan(_clanName, _cInfo.playerId);
                    string _phrase106 = "{PlayerName} you have removed the clan {ClanName}.";
                    if (Phrases._Phrases.TryGetValue(106, out _phrase106))
                    {
                        _phrase106 = _phrase106.Replace("{PlayerName}", _cInfo.playerName);
                        _phrase106 = _phrase106.Replace("{ClanName}", _clanName);
                    }
                    _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase106, CustomCommands._chatcolor), "Server"));
                }     
            }
        }

        public static void InviteMember(ClientInfo _cInfo, string _playerName)
        {
            ClientInfo _newMember = ConsoleHelper.ParseParamIdOrName(_playerName);
            if (!ClanData.OfficersList.Contains(_cInfo.playerId))
            {
                string _phrase107 = "{PlayerName} you do not have permissions to use this command.";
                if (Phrases._Phrases.TryGetValue(107, out _phrase107))
                {
                    _phrase107 = _phrase107.Replace("{PlayerName}", _cInfo.playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase107, CustomCommands._chatcolor), "Server"));
            }
            else if (_newMember == null)
            {
                string _phrase108 = "{PlayerName} the name {TargetPlayerName} was not found.";
                if (Phrases._Phrases.TryGetValue(108, out _phrase108))
                {
                    _phrase108 = _phrase108.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase108 = _phrase108.Replace("{TargetPlayerName}", _playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase108, CustomCommands._chatcolor), "Server"));
            }
            else if (ClanData.PlayersList.Contains(_newMember.playerId))
            {
                string _phrase109 = "{PlayerName} is already a member of a clan.";
                if (Phrases._Phrases.TryGetValue(109, out _phrase109))
                {
                    _phrase109 = _phrase109.Replace("{PlayerName}", _playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase109, CustomCommands._chatcolor), "Server"));
            }
            else if (ClanData.InvitesList.Contains(_newMember.playerId))
            {
                string _phrase110 = "{PlayerName} already has pending clan invites.";
                if (Phrases._Phrases.TryGetValue(110, out _phrase110))
                {
                    _phrase110 = _phrase110.Replace("{PlayerName}", _playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase110, CustomCommands._chatcolor), "Server"));
            }
            else
            {
                string _clanName;
                if (ClanData.Officers.TryGetValue(_cInfo.playerId, out _clanName))
                {
                    string _phrase111 = "{PlayerName} you have been invited to join the clan {ClanName}. Type /clanaccept to join or /clandecline to decline the offer.";
                    if (Phrases._Phrases.TryGetValue(111, out _phrase111))
                    {
                        _phrase111 = _phrase111.Replace("{PlayerName}", _newMember.playerName);
                        _phrase111 = _phrase111.Replace("{ClanName}", _clanName);
                    }
                    string _phrase112 = "{PlayerName} you have invited {InvitedPlayerName} to the clan {ClanName}.";
                    if (Phrases._Phrases.TryGetValue(112, out _phrase112))
                    {
                        _phrase112 = _phrase112.Replace("{PlayerName}", _cInfo.playerName);
                        _phrase112 = _phrase112.Replace("{InvitedPlayerName}", _newMember.playerName);
                        _phrase112 = _phrase112.Replace("{ClanName}", _clanName);
                    }
                    ClanData.Invites.Add(_newMember.playerId, _clanName);
                    ClanData.UpdateInviteData();
                    _newMember.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase111, CustomCommands._chatcolor), "Server"));
                    _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase112, CustomCommands._chatcolor), "Server"));
                }
            }
        }

        public static void InviteAccept(ClientInfo _cInfo)
        {
            string _clanName;
            if (!ClanData.Invites.TryGetValue(_cInfo.playerId, out _clanName))
            {
                string _phrase113 = "{PlayerName} you have not been invited to any clans.";
                if (Phrases._Phrases.TryGetValue(113, out _phrase113))
                {
                    _phrase113 = _phrase113.Replace("{PlayerName}", _cInfo.playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase113, CustomCommands._chatcolor), "Server"));
            }
            else if (!ClanData.ClansList.Contains(_clanName))
            {
                string _phrase114 = "{PlayerName} the clan could not be found.";
                if (Phrases._Phrases.TryGetValue(114, out _phrase114))
                {
                    _phrase114 = _phrase114.Replace("{PlayerName}", _cInfo.playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase114, CustomCommands._chatcolor), "Server"));
                ClanData.Invites.Remove(_cInfo.playerId);
                ClanData.UpdateInviteData();
            }
            else
            {
                ClanData.AddMember(_clanName, _cInfo.playerId);
                List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                foreach (ClientInfo _cInfop in _cInfoList)
                {
                    if (ClanData.PlayersList.Contains(_cInfop.playerId))
                    {
                        string _clanName1;
                        if (ClanData.Players.TryGetValue(_cInfop.playerId, out _clanName1))
                        {
                            if (_clanName1 == _clanName)
                            {
                                string _phrase115 = "{PlayerName} has joined the clan.";
                                if (Phrases._Phrases.TryGetValue(115, out _phrase115))
                                {
                                    _phrase115 = _phrase115.Replace("{PlayerName}", _cInfo.playerName);
                                }
                                _cInfop.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase115, CustomCommands._chatcolor), "Server"));
                            }
                        }
                    }
                }
            }
        }

        public static void InviteDecline(ClientInfo _cInfo)
        {
            if (!ClanData.InvitesList.Contains(_cInfo.playerId))
            {
                string _phrase113 = "{PlayerName} you have not been invited to any clans.";
                if (Phrases._Phrases.TryGetValue(113, out _phrase113))
                {
                    _phrase113 = _phrase113.Replace("{PlayerName}", _cInfo.playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase113, CustomCommands._chatcolor), "Server"));
            }
            else
            {
                ClanData.Invites.Remove(_cInfo.playerId);
                ClanData.UpdateInviteData();
                string _phrase116 = "{PlayerName} you have declined the invite to the clan.";
                if (Phrases._Phrases.TryGetValue(116, out _phrase116))
                {
                    _phrase116 = _phrase116.Replace("{PlayerName}", _cInfo.playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase116, CustomCommands._chatcolor), "Server"));
            }
        }

        public static void RemoveMember(ClientInfo _cInfo, string _playerName)
        {
            if (!ClanData.OfficersList.Contains(_cInfo.playerId))
            {
                string _phrase107 = "{PlayerName} you do not have permissions to use this command.";
                if (Phrases._Phrases.TryGetValue(107, out _phrase107))
                {
                    _phrase107 = _phrase107.Replace("{PlayerName}", _cInfo.playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase107, CustomCommands._chatcolor), "Server"));
                return;
            }
            string _steamId;
            ClientInfo _PlayertoRemove = ConsoleHelper.ParseParamIdOrName(_playerName);
            if (_PlayertoRemove != null)
            {
                _steamId = _PlayertoRemove.playerId;
            }
            else if (ClanData.PlayersList.Contains(_playerName))
            {
                _steamId = _playerName;
            }
            else
            {
                string _phrase108 = "{PlayerName} the name {TargetPlayerName} was not found.";
                if (Phrases._Phrases.TryGetValue(108, out _phrase108))
                {
                    _phrase108 = _phrase108.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase108 = _phrase108.Replace("{TargetPlayerName}", _playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase108, CustomCommands._chatcolor), "Server"));
                return;
            }
            string _clanName;
            string _clan;
            if (!ClanData.Players.TryGetValue(_cInfo.playerId, out _clanName) || !ClanData.Players.TryGetValue(_steamId, out _clan))
            {
                string _phrase117 = "{PlayerName} is not a member of your clan.";
                if (Phrases._Phrases.TryGetValue(117, out _phrase117))
                {
                    _phrase117 = _phrase117.Replace("{PlayerName}", _playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase117, CustomCommands._chatcolor), "Server"));
            }
            else if (_clan != _clanName)
            {
                string _phrase117 = "{PlayerName} is not a member of your clan.";
                if (Phrases._Phrases.TryGetValue(117, out _phrase117))
                {
                    _phrase117 = _phrase117.Replace("{PlayerName}", _playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase117, CustomCommands._chatcolor), "Server"));
            }
            else if (ClanData.OfficersList.Contains(_steamId) && !ClanData.OwnersList.Contains(_cInfo.playerId))
            {
                string _phrase118 = "{PlayerName} only the clan owner can remove officers.";
                if (Phrases._Phrases.TryGetValue(118, out _phrase118))
                {
                    _phrase118 = _phrase118.Replace("{PlayerName}", _cInfo.playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase118, CustomCommands._chatcolor), "Server"));
            }
            else if (ClanData.OwnersList.Contains(_steamId))
            {
                string _phrase119 = "{PlayerName} clan owners can not be removed.";
                if (Phrases._Phrases.TryGetValue(119, out _phrase119))
                {
                    _phrase119 = _phrase119.Replace("{PlayerName}", _cInfo.playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase119, CustomCommands._chatcolor), "Server"));
            }
            else
            {
                string _phrase120 = "{PlayerName} you have removed {PlayertoRemove} from clan {ClanName}.";
                if (Phrases._Phrases.TryGetValue(120, out _phrase120))
                {
                    _phrase120 = _phrase120.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase120 = _phrase120.Replace("{PlayertoRemove}", _playerName);
                    _phrase120 = _phrase120.Replace("{ClanName}", _clanName);
                }
                string _phrase121 = "{PlayerName} you have been removed from the clan {ClanName}.";
                if (Phrases._Phrases.TryGetValue(121, out _phrase121))
                {
                    _phrase121 = _phrase121.Replace("{PlayerName}", _playerName);
                    _phrase121 = _phrase121.Replace("{ClanName}", _clanName);
                }
                ClanData.RemoveMember(_steamId);
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase120, CustomCommands._chatcolor), "Server"));
                if (_PlayertoRemove != null)
                {
                    _PlayertoRemove.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase121, CustomCommands._chatcolor), "Server"));
                }
            }
        }

        public static void PromoteMember(ClientInfo _cInfo, string _playerName)
        {
            string _clan;
            string _clanName;
            ClientInfo _PlayertoPromote = ConsoleHelper.ParseParamIdOrName(_playerName);
            if (!ClanData.OwnersList.Contains(_cInfo.playerId))
            {
                string _phrase107 = "{PlayerName} you do not have permissions to use this command.";
                if (Phrases._Phrases.TryGetValue(107, out _phrase107))
                {
                    _phrase107 = _phrase107.Replace("{PlayerName}", _cInfo.playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase107, CustomCommands._chatcolor), "Server"));
            }
            else if (_PlayertoPromote == null)
            {
                string _phrase108 = "{PlayerName} the name {TargetPlayerName} was not found.";
                if (Phrases._Phrases.TryGetValue(108, out _phrase108))
                {
                    _phrase108 = _phrase108.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase108 = _phrase108.Replace("{TargetPlayerName}", _playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase108, CustomCommands._chatcolor), "Server"));
            }
            else if (ClanData.Owners.TryGetValue(_cInfo.playerId, out _clan) || !ClanData.Players.TryGetValue(_PlayertoPromote.playerId, out _clanName))
            {
                string _phrase117 = "{PlayerName} is not a member of your clan.";
                if (Phrases._Phrases.TryGetValue(117, out _phrase117))
                {
                    _phrase117 = _phrase117.Replace("{PlayerName}", _playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase117, CustomCommands._chatcolor), "Server"));
            }
            else if (_clanName != _clan)
            {
                string _phrase117 = "{PlayerName} is not a member of your clan.";
                if (Phrases._Phrases.TryGetValue(117, out _phrase117))
                {
                    _phrase117 = _phrase117.Replace("{PlayerName}", _playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase117, CustomCommands._chatcolor), "Server"));
            }
            else if (ClanData.OfficersList.Contains(_PlayertoPromote.playerId))
            {
                string _phrase122 = "{PlayerName} is already a officer.";
                if (Phrases._Phrases.TryGetValue(122, out _phrase122))
                {
                    _phrase122 = _phrase122.Replace("{PlayerName}", _playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase122, CustomCommands._chatcolor), "Server"));
            }
            else
            {
                ClanData.Officers.Add(_PlayertoPromote.playerId, _clanName);
                ClanData.UpdateOfficerData();
                string _phrase123 = "{PlayerName} has been promoted to an officer.";
                if (Phrases._Phrases.TryGetValue(123, out _phrase123))
                {
                    _phrase123 = _phrase123.Replace("{PlayerName}", _playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase123, CustomCommands._chatcolor), "Server"));
            }
        }

        public static void DemoteMember(ClientInfo _cInfo, string _playerName)
        {
            string _clan;
            string _clanName;
            ClientInfo _MembertoDemote = ConsoleHelper.ParseParamIdOrName(_playerName);
            if (!ClanData.OwnersList.Contains(_cInfo.playerId))
            {
                string _phrase107 = "{PlayerName} you do not have permissions to use this command.";
                if (Phrases._Phrases.TryGetValue(107, out _phrase107))
                {
                    _phrase107 = _phrase107.Replace("{PlayerName}", _cInfo.playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase107, CustomCommands._chatcolor), "Server"));
            }
            else if (_MembertoDemote == null)
            {
                string _phrase108 = "{PlayerName} the name {TargetPlayerName} was not found.";
                if (Phrases._Phrases.TryGetValue(108, out _phrase108))
                {
                    _phrase108 = _phrase108.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase108 = _phrase108.Replace("{TargetPlayerName}", _playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase108, CustomCommands._chatcolor), "Server"));
            }
            else if (!ClanData.Owners.TryGetValue(_cInfo.playerId, out _clan) || !ClanData.Owners.TryGetValue(_MembertoDemote.playerId, out _clanName))
            {
                string _phrase117 = "{PlayerName} is not a member of your clan.";
                if (Phrases._Phrases.TryGetValue(117, out _phrase117))
                {
                    _phrase117 = _phrase117.Replace("{PlayerName}", _playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase117, CustomCommands._chatcolor), "Server"));
            }
            else if (_clanName != _clan)
            {
                string _phrase117 = "{PlayerName} is not a member of your clan.";
                if (Phrases._Phrases.TryGetValue(117, out _phrase117))
                {
                    _phrase117 = _phrase117.Replace("{PlayerName}", _playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase117, CustomCommands._chatcolor), "Server"));
            }
            else if (!ClanData.OfficersList.Contains(_MembertoDemote.playerId))
            {
                string _phrase124 = "{PlayerName} is not an officer.";
                if (Phrases._Phrases.TryGetValue(124, out _phrase124))
                {
                    _phrase124 = _phrase124.Replace("{PlayerName}", _playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase124, CustomCommands._chatcolor), "Server"));
            }
            else
            {
                string _phrase125 = "{PlayerName} has been demoted.";
                if (Phrases._Phrases.TryGetValue(40, out _phrase125))
                {
                    _phrase125 = _phrase125.Replace("{PlayerName}", _playerName);
                }
                ClanData.Officers.Remove(_MembertoDemote.playerId);
                ClanData.UpdateOfficerData();
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase125, CustomCommands._chatcolor), "Server"));
            }
        }

        public static void LeaveClan(ClientInfo _cInfo)
        {
            if (ClanData.OwnersList.Contains(_cInfo.playerId))
            {
                string _phrase126 = "{PlayerName} you can not leave the clan because you are the owner. You can only delete the clan.";
                if (Phrases._Phrases.TryGetValue(126, out _phrase126))
                {
                    _phrase126 = _phrase126.Replace("{PlayerName}", _cInfo.playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase126, CustomCommands._chatcolor), "Server"));
            }
            else if (!ClanData.PlayersList.Contains(_cInfo.playerId))
            {
                string _phrase127 = "{PlayerName} you do not belong to any clans.";
                if (Phrases._Phrases.TryGetValue(127, out _phrase127))
                {
                    _phrase127 = _phrase127.Replace("{PlayerName}", _cInfo.playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase127, CustomCommands._chatcolor), "Server"));
            }
            else
            {
                string _clanName;
                if (ClanData.Players.TryGetValue(_cInfo.playerId, out _clanName))
                {
                    string _phrase121 = "{PlayerName} you have been removed from the clan {ClanName}.";
                    if (Phrases._Phrases.TryGetValue(121, out _phrase121))
                    {
                        _phrase121 = _phrase121.Replace("{PlayerName}", _cInfo.playerName);
                        _phrase121 = _phrase121.Replace("{ClanName}", _clanName);
                    }
                    ClanData.RemoveMember(_cInfo.playerId);
                    _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase121, CustomCommands._chatcolor), "Server"));
                }
            }
        }

        public static void GetChatCommands(ClientInfo _cInfo)
        {
            string _commands = string.Format("{0}Clan commands are:", CustomCommands._chatcolor); ;
            if (!ClanData.OwnersList.Contains(_cInfo.playerId) && !ClanData.OfficersList.Contains(_cInfo.playerId) && !ClanData.PlayersList.Contains(_cInfo.playerId) && !ClanData.InvitesList.Contains(_cInfo.playerId))
            {
                _commands = string.Format("{0} /clanadd", _commands);
            }
            if (ClanData.OwnersList.Contains(_cInfo.playerId))
            {
                _commands = string.Format("{0} /clanpromote /clandemote /clandel", _commands);
            }
            if (ClanData.OwnersList.Contains(_cInfo.playerId) || ClanData.OfficersList.Contains(_cInfo.playerId))
            {
                _commands = string.Format("{0} /claninvite /clanremove", _commands);
            }
            if (ClanData.InvitesList.Contains(_cInfo.playerId))
            {
                _commands = string.Format("{0} /clanaccept /clandecline", _commands);
            }
            if (!ClanData.OwnersList.Contains(_cInfo.playerId) && ClanData.PlayersList.Contains(_cInfo.playerId))
            {
                _commands = string.Format("{0} /clanleave", _commands);
            }
            _commands = string.Format("{0}[-]", _commands);
            _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{0}", _commands), "Server"));
        }
    }
}
