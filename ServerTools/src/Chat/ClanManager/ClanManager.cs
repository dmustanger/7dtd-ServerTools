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
                    string _phrase14 = "You do not belong to the clan {ClanName}. Please remove the clan tag and rejoin.";
                    if (Phrases._Phrases.TryGetValue(14, out _phrase14))
                    {
                        _phrase14 = _phrase14.Replace("{ClanName}", _clan);
                    }
                    string _clan1;
                    if (!ClanData.Players.TryGetValue(_cInfo.playerId, out _clan1))
                    {
                        SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.entityId, _phrase14), _cInfo);
                    }
                    else
                    {
                        if (_clan != _clan1)
                        {
                            SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.entityId, _phrase14), _cInfo);
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
                if(ClanData.Players.TryGetValue(_cInfo.playerId, out _name))
                {
                    string _phrase15 = "{PlayerName} you have already created the clan {ClanName}.";
                    if (Phrases._Phrases.TryGetValue(15, out _phrase15))
                    {
                        _phrase15 = _phrase15.Replace("{PlayerName}", _cInfo.playerName);
                        _phrase15 = _phrase15.Replace("{ClanName}", _name);
                    }
                    _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase15, CustomCommands._chatcolor), "Server"));
                }
            }
            else
            {
                if (ClanData.ClansList.Contains(_clanName))
                {
                    string _phrase16 = "The clan {ClanName} already exist.";
                    if (Phrases._Phrases.TryGetValue(16, out _phrase16))
                    {
                        _phrase16 = _phrase16.Replace("{ClanName}", _clanName);
                    }
                    _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase16, CustomCommands._chatcolor), "Server"));
                }
                else
                {
                    if (ClanData.PlayersList.Contains(_cInfo.playerId))
                    {
                        string _phrase17 = "You are currently a member of the clan {ClanName}.";
                        if (Phrases._Phrases.TryGetValue(17, out _phrase17))
                        {
                            _phrase17 = _phrase17.Replace("{ClanName}", _clanName);
                        }
                        _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase17, CustomCommands._chatcolor), "Server"));
                    }
                    else
                    {
                        if(!ClanData.Clans.ContainsKey(_clanName) && !ClanData.Players.ContainsKey(_cInfo.playerId) && !ClanData.Officers.ContainsKey(_cInfo.playerId))
                        {
                            ClanData.AddClan(_clanName, _cInfo.playerId);
                            string _phrase18 = "The clan {ClanName} has been added.";
                            if (Phrases._Phrases.TryGetValue(18, out _phrase18))
                            {
                                _phrase18 = _phrase18.Replace("{ClanName}", _clanName);
                            }
                            _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase18, CustomCommands._chatcolor), "Server"));
                        }
                        else
                        {
                            string _phrase19 = "Something went wrong, could not add the clan {ClanName}.";
                            if (Phrases._Phrases.TryGetValue(19, out _phrase19))
                            {
                                _phrase19 = _phrase19.Replace("{ClanName}", _clanName);
                            }
                            _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase19, CustomCommands._chatcolor), "Server"));
                        }
                    }
                }
            }  
        }

        public static void RemoveClan(ClientInfo _cInfo)
        {
            if (!ClanData.OwnersList.Contains(_cInfo.playerId))
            {
                string _phrase20 = "You are not the owner of any clans.";
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase20, CustomCommands._chatcolor), "Server"));
            }
            else
            {
                string _clanName;
                if(ClanData.Owners.TryGetValue(_cInfo.playerId, out _clanName))
                {
                    ClanData.RemoveClan(_clanName, _cInfo.playerId);
                    string _phrase21 = "The clan {ClanName} has been removed.";
                    if (Phrases._Phrases.TryGetValue(21, out _phrase21))
                    {
                        _phrase21 = _phrase21.Replace("{ClanName}", _clanName);
                    }
                    _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase21, CustomCommands._chatcolor), "Server"));
                }     
            }
        }

        public static void InviteMember(ClientInfo _cInfo, string _playerName)
        {
            if (!ClanData.OfficersList.Contains(_cInfo.playerId))
            {
                string _phrase22 = "You do not have permissions to use this command.";
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase22, CustomCommands._chatcolor), "Server"));
            }
            else
            {
                ClientInfo _newMember = ConsoleHelper.ParseParamIdOrName(_playerName);
                if (_newMember == null)
                {
                    string _phrase23 = "Playername not found.";
                    _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase23, CustomCommands._chatcolor), "Server"));
                }
                else
                {
                    string _clanName;
                    if (ClanData.Officers.TryGetValue(_cInfo.playerId, out _clanName))
                    {
                        if (ClanData.PlayersList.Contains(_newMember.playerId))
                        {
                            string _phrase24 = "{PlayerName} is already a member of a clan.";
                            if (Phrases._Phrases.TryGetValue(24, out _phrase24))
                            {
                                _phrase24 = _phrase24.Replace("{PlayerName}", _playerName);
                            }
                            _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase24, CustomCommands._chatcolor), "Server"));
                        }
                        else
                        {
                            if (ClanData.InvitesList.Contains(_newMember.playerId))
                            {
                                string _phrase25 = "{PlayerName} already has pending clan invites.";
                                if (Phrases._Phrases.TryGetValue(25, out _phrase25))
                                {
                                    _phrase25 = _phrase25.Replace("{PlayerName}", _playerName);
                                }
                                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase25, CustomCommands._chatcolor), "Server"));
                            }
                            else
                            {
                                ClanData.Invites.Add(_newMember.playerId, _clanName);
                                ClanData.UpdateInviteData();
                                string _phrase26 = "You have been invited to join the clan {ClanName}. Type /clanaccept to join or /clandecline to decline the offer.";
                                if (Phrases._Phrases.TryGetValue(26, out _phrase26))
                                {
                                    _phrase26 = _phrase26.Replace("{ClanName}", _clanName);
                                }
                                _newMember.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase26, CustomCommands._chatcolor), "Server"));
                                string _phrase27 = "Invite has been sent.";
                                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase27, CustomCommands._chatcolor), "Server"));       
                            }
                        }
                    }
                }
            } 
        }

        public static void InviteAccept(ClientInfo _cInfo)
        {
            string _clanName;
            if (!ClanData.Invites.TryGetValue(_cInfo.playerId, out _clanName))
            {
                string _phrase28 = "You have not been invited to any clans.";
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase28, CustomCommands._chatcolor), "Server"));
            }
            else
            {
                if (!ClanData.ClansList.Contains(_clanName))
                {
                    string _phrase29 = "Clan could not be found.";
                    _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase29, CustomCommands._chatcolor), "Server"));
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
                            if(ClanData.Players.TryGetValue(_cInfop.playerId, out _clanName1))
                            {
                                if(_clanName1 == _clanName)
                                {
                                    string _phrase30 = "{PlayerName} has joined the clan.";
                                    if (Phrases._Phrases.TryGetValue(30, out _phrase30))
                                    {
                                        _phrase30 = _phrase30.Replace("{PlayerName}", _cInfo.playerName);
                                    }
                                    _cInfop.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase30, CustomCommands._chatcolor), "Server"));
                                }
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
                string _phrase28 = "You have not been invited to any clans.";
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase28, CustomCommands._chatcolor), "Server"));
            }
            else
            {
                ClanData.Invites.Remove(_cInfo.playerId);
                ClanData.UpdateInviteData();
                string _phrase31 = "You have declined the invite to the clan.";
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase31, CustomCommands._chatcolor), "Server"));
            }
        }

        public static void RemoveMember(ClientInfo _cInfo, string _playerName)
        {
            if (!ClanData.OfficersList.Contains(_cInfo.playerId))
            {
                string _phrase22 = "You do not have permissions to use this command.";
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase22, CustomCommands._chatcolor), "Server"));
            }
            else
            {
                string _clanName;
                if (ClanData.Players.TryGetValue(_cInfo.playerId, out _clanName))
                {
                    ClientInfo _PlayertoRemove = ConsoleHelper.ParseParamIdOrName(_playerName);
                    if (_PlayertoRemove == null)
                    {
                        string _phrase23 = "Player name not found.";
                        _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase23, CustomCommands._chatcolor), "Server"));
                    }
                    else
                    {
                        string _clan;
                        if (!ClanData.Players.TryGetValue(_PlayertoRemove.playerId, out _clan))
                        {
                            string _phrase32 = "{PlayerName} is not a member of your clan.";
                            if (Phrases._Phrases.TryGetValue(32, out _phrase32))
                            {
                                _phrase32 = _phrase32.Replace("{PlayerName}", _playerName);
                            }
                            _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase32, CustomCommands._chatcolor), "Server"));
                        }
                        else
                        {
                            if (_clan != _clanName)
                            {
                                string _phrase32 = "{PlayerName} is not a member of your clan.";
                                if (Phrases._Phrases.TryGetValue(32, out _phrase32))
                                {
                                    _phrase32 = _phrase32.Replace("{PlayerName}", _playerName);
                                }
                                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase32, CustomCommands._chatcolor), "Server"));
                            }
                            else
                            {
                                if (ClanData.OfficersList.Contains(_PlayertoRemove.playerId) && !ClanData.OwnersList.Contains(_cInfo.playerId))
                                {
                                    string _phrase33 = "Only the clan owner can remove officers.";
                                    _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase33, CustomCommands._chatcolor), "Server"));
                                }
                                else
                                {
                                    if(ClanData.OwnersList.Contains(_PlayertoRemove.playerId))
                                    {
                                        string _phrase34 = "Clan owners can not be removed.";
                                        _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase34, CustomCommands._chatcolor), "Server"));
                                    }
                                    else
                                    {
                                        ClanData.RemoveMember(_PlayertoRemove.playerId);
                                        string _phrase35 = "You have removed {PlayerName} from clan {ClanName}.";
                                        if (Phrases._Phrases.TryGetValue(35, out _phrase35))
                                        {
                                            _phrase35 = _phrase35.Replace("{PlayerName}", _PlayertoRemove.playerName);
                                            _phrase35 = _phrase35.Replace("{ClanName}", _clanName);
                                        }
                                        _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase35, CustomCommands._chatcolor), "Server"));
                                        string _phrase36 = "You have been removed from clan {ClanName}.";
                                        if (Phrases._Phrases.TryGetValue(36, out _phrase36))
                                        {
                                            _phrase36 = _phrase36.Replace("{ClanName}", _clanName);
                                        }
                                        _PlayertoRemove.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase36, CustomCommands._chatcolor), "Server"));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void PromoteMember(ClientInfo _cInfo, string _playerName)
        {
            if(!ClanData.OwnersList.Contains(_cInfo.playerId))
            {
                string _phrase22 = "You do not have permissions to use this command.";
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase22, CustomCommands._chatcolor), "Server"));
            }
            else
            {
                string _clan;
                if(ClanData.Owners.TryGetValue(_cInfo.playerId, out _clan))
                {
                    ClientInfo _PlayertoPromote = ConsoleHelper.ParseParamIdOrName(_playerName);
                    if (_PlayertoPromote == null)
                    {
                        string _phrase23 = "Player name not found.";
                        _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase23, CustomCommands._chatcolor), "Server"));
                    }
                    else
                    {
                        string _clanName;
                        if (!ClanData.Players.TryGetValue(_PlayertoPromote.playerId, out _clanName))
                        {
                            string _phrase32 = "{PlayerName} is not a member of your clan.";
                            if (Phrases._Phrases.TryGetValue(32, out _phrase32))
                            {
                                _phrase32 = _phrase32.Replace("{PlayerName}", _playerName);
                            }
                            _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase32, CustomCommands._chatcolor), "Server"));
                        }
                        else
                        {
                            if (_clanName != _clan)
                            {
                                string _phrase32 = "{PlayerName} is not a member of your clan.";
                                if (Phrases._Phrases.TryGetValue(32, out _phrase32))
                                {
                                    _phrase32 = _phrase32.Replace("{PlayerName}", _playerName);
                                }
                                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase32, CustomCommands._chatcolor), "Server"));
                            }
                            else
                            {
                                if (ClanData.OfficersList.Contains(_PlayertoPromote.playerId))
                                {
                                    string _phrase37 = "{PlayerName} is already a officer.";
                                    if (Phrases._Phrases.TryGetValue(37, out _phrase37))
                                    {
                                        _phrase37 = _phrase37.Replace("{PlayerName}", _playerName);
                                    }
                                    _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase37, CustomCommands._chatcolor), "Server"));
                                }
                                else
                                {
                                    ClanData.Officers.Add(_PlayertoPromote.playerId, _clanName);
                                    ClanData.UpdateOfficerData();
                                    string _phrase38 = "{PlayerName} has been promoted to an officer.";
                                    if (Phrases._Phrases.TryGetValue(38, out _phrase38))
                                    {
                                        _phrase38 = _phrase38.Replace("{PlayerName}", _playerName);
                                    }
                                    _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase38, CustomCommands._chatcolor), "Server"));
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void DemoteMember(ClientInfo _cInfo, string _playerName)
        {
            if (!ClanData.OwnersList.Contains(_cInfo.playerId))
            {
                string _phrase22 = "You do not have permissions to use this command.";
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase22, CustomCommands._chatcolor), "Server"));
            }
            else
            {
                string _clan;
                if (ClanData.Owners.TryGetValue(_cInfo.playerId, out _clan))
                {
                    ClientInfo _MembertoDemote = ConsoleHelper.ParseParamIdOrName(_playerName);
                    if (_MembertoDemote == null)
                    {
                        string _phrase23 = "Player name not found.";
                        _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase23, CustomCommands._chatcolor), "Server"));
                    }
                    else
                    {
                        string _clanName;
                        if (!ClanData.Owners.TryGetValue(_MembertoDemote.playerId, out _clanName))
                        {
                            string _phrase32 = "{PlayerName} is not a member of your clan.";
                            if (Phrases._Phrases.TryGetValue(32, out _phrase32))
                            {
                                _phrase32 = _phrase32.Replace("{PlayerName}", _playerName);
                            }
                            _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase32, CustomCommands._chatcolor), "Server"));
                        }
                        else
                        {
                            if (_clanName != _clan)
                            {
                                string _phrase32 = "{PlayerName} is not a member of your clan.";
                                if (Phrases._Phrases.TryGetValue(32, out _phrase32))
                                {
                                    _phrase32 = _phrase32.Replace("{PlayerName}", _playerName);
                                }
                                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase32, CustomCommands._chatcolor), "Server"));
                            }
                            else
                            {
                                if (!ClanData.OfficersList.Contains(_MembertoDemote.playerId))
                                {
                                    string _phrase39 = "{PlayerName} is not an officer.";
                                    if (Phrases._Phrases.TryGetValue(39, out _phrase39))
                                    {
                                        _phrase39 = _phrase39.Replace("{PlayerName}", _playerName);
                                    }
                                    _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase39, CustomCommands._chatcolor), "Server"));
                                }
                                else
                                {
                                    ClanData.Officers.Remove(_MembertoDemote.playerId);
                                    ClanData.UpdateOfficerData();
                                    string _phrase40 = "{PlayerName} has been demoted.";
                                    if (Phrases._Phrases.TryGetValue(40, out _phrase40))
                                    {
                                        _phrase40 = _phrase40.Replace("{PlayerName}", _playerName);
                                    }
                                    _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase40, CustomCommands._chatcolor), "Server"));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
