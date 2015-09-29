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
                    string _reason = string.Format("You do not belong to the clan {0}. Please remove the clan tag and rejoin.", _clan);
                    string _clan1;
                    if (!ClanData.Players.TryGetValue(_cInfo.playerId, out _clan1))
                    {
                        SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.entityId, _reason), _cInfo);
                    }
                    else
                    {
                        if (_clan != _clan1)
                        {
                            SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.entityId, _reason), _cInfo);
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
                    string _reason = string.Format("You have already created the clan {0}.", _name);
                    _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
                }
            }
            else
            {
                if (ClanData.ClansList.Contains(_clanName))
                {
                    string _reason = string.Format("The clan {0} already exist.", _clanName);
                    _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
                }
                else
                {
                    if (ClanData.PlayersList.Contains(_cInfo.playerId))
                    {
                        string _reason = "You are currently a member of a clan.";
                        _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
                    }
                    else
                    {
                        if(!ClanData.Clans.ContainsKey(_clanName) && !ClanData.Players.ContainsKey(_cInfo.playerId) && !ClanData.Officers.ContainsKey(_cInfo.playerId))
                        {
                            ClanData.AddClan(_clanName, _cInfo.playerId);
                            string _reason = string.Format("The clan {0} has been added.", _clanName);
                            _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
                        }
                        else
                        {
                            string _reason = string.Format("Something went wrong, could not add the clan {0}.", _clanName);
                            _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
                        }
                    }
                }
            }  
        }

        public static void RemoveClan(ClientInfo _cInfo)
        {
            if (!ClanData.OwnersList.Contains(_cInfo.playerId))
            {
                string _reason = "You are not the owner of any clans.";
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
            }
            else
            {
                string _clanName;
                if(ClanData.Owners.TryGetValue(_cInfo.playerId, out _clanName))
                {
                    ClanData.RemoveClan(_clanName, _cInfo.playerId);
                    string _reason = string.Format("The clan {0} has been removed.", _clanName);
                    _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
                }     
            }
        }

        public static void InviteMember(ClientInfo _cInfo, string _playerName)
        {
            if (!ClanData.OfficersList.Contains(_cInfo.playerId))
            {
                string _reason = "You do not have permissions to use this command.";
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
            }
            else
            {
                ClientInfo _newMember = ConsoleHelper.ParseParamIdOrName(_playerName);
                if (_newMember == null)
                {
                    string _reason = "Playername not found.";
                    _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
                }
                else
                {
                    string _clanName;
                    if (ClanData.Officers.TryGetValue(_cInfo.playerId, out _clanName))
                    {
                        if (ClanData.PlayersList.Contains(_newMember.playerId))
                        {
                            string _reason = "Player is already a member of a clan.";
                            _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
                        }
                        else
                        {
                            if (ClanData.InvitesList.Contains(_newMember.playerId))
                            {
                                string _reason = "Player already has pending clain invites.";
                                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
                            }
                            else
                            {
                                ClanData.Invites.Add(_newMember.playerId, _clanName);
                                ClanData.UpdateInviteData();
                                string _reason = string.Format("You have been invited to join the clan {0}. Type /clanaccept to join or /clandecline to decline the offer.", _clanName);
                                _newMember.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
                                string _reason1 = "Invite has been sent.";
                                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason1, CustomCommands._chatcolor), "Server"));       
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
                string _reason = "You have not been invited to any clans.";
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
            }
            else
            {
                if (!ClanData.ClansList.Contains(_clanName))
                {
                    string _reason = "Clan could not be found.";
                    _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
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
                                    string _reasonp = string.Format("{0} has joined the clan.", _cInfo.playerName);
                                    _cInfop.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reasonp, CustomCommands._chatcolor), "Server"));
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
                string _reason = "You have not been invited to any clans.";
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
            }
            else
            {
                ClanData.Invites.Remove(_cInfo.playerId);
                ClanData.UpdateInviteData();
                string _reason = "You have declined the invite to the clan.";
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
            }
        }

        public static void RemoveMember(ClientInfo _cInfo, string _playerName)
        {
            if (!ClanData.OfficersList.Contains(_cInfo.playerId))
            {
                string _reason = "You do not have permissions to use this command.";
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
            }
            else
            {
                string _clanName;
                if (ClanData.Players.TryGetValue(_cInfo.playerId, out _clanName))
                {
                    ClientInfo _PlayertoRemove = ConsoleHelper.ParseParamIdOrName(_playerName);
                    if (_PlayertoRemove == null)
                    {
                        string _reason = "Player name not found.";
                        _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
                    }
                    else
                    {
                        string _clan;
                        if (!ClanData.Players.TryGetValue(_PlayertoRemove.playerId, out _clan))
                        {
                            string _reason = "Player is not a member of your clan.";
                            _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
                        }
                        else
                        {
                            if (_clan != _clanName)
                            {
                                string _reason = "Player is not a member of your clan.";
                                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
                            }
                            else
                            {
                                if (ClanData.OfficersList.Contains(_PlayertoRemove.playerId) && !ClanData.OwnersList.Contains(_cInfo.playerId))
                                {
                                    string _reason = "Only the clan owner can remove officers.";
                                    _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
                                }
                                else
                                {
                                    if(ClanData.OwnersList.Contains(_PlayertoRemove.playerId))
                                    {
                                        string _reason = "Clan owners can not be removed.";
                                        _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
                                    }
                                    else
                                    {
                                        ClanData.RemoveMember(_PlayertoRemove.playerId);
                                        string _reason = string.Format("You have removed {0} from clan {1}.", _PlayertoRemove.playerName, _clanName);
                                        _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
                                        string _reason1 = string.Format("You have been from clan {0}.", _clanName);
                                        _PlayertoRemove.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason1, CustomCommands._chatcolor), "Server"));
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
                string _reason = "You do not have permissions to use this command.";
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
            }
            else
            {
                string _clan;
                if(ClanData.Owners.TryGetValue(_cInfo.playerId, out _clan))
                {
                    ClientInfo _PlayertoPromote = ConsoleHelper.ParseParamIdOrName(_playerName);
                    if (_PlayertoPromote == null)
                    {
                        string _reason = "Player name not found.";
                        _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
                    }
                    else
                    {
                        string _clanName;
                        if (!ClanData.Players.TryGetValue(_PlayertoPromote.playerId, out _clanName))
                        {
                            string _reason = "Player is not a member of your clan.";
                            _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
                        }
                        else
                        {
                            if (_clanName != _clan)
                            {
                                string _reason = "Player is not a member of your clan.";
                                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
                            }
                            else
                            {
                                if (ClanData.OfficersList.Contains(_PlayertoPromote.playerId))
                                {
                                    string _reason = "Player is already a officer.";
                                    _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
                                }
                                else
                                {
                                    ClanData.Officers.Add(_PlayertoPromote.playerId, _clanName);
                                    ClanData.UpdateOfficerData();
                                    string _reason = "Player has been promoted.";
                                    _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
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
                string _reason = "You do not have permissions to use this command.";
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
            }
            else
            {
                string _clan;
                if (ClanData.Owners.TryGetValue(_cInfo.playerId, out _clan))
                {
                    ClientInfo _MembertoDemote = ConsoleHelper.ParseParamIdOrName(_playerName);
                    if (_MembertoDemote == null)
                    {
                        string _reason = "Player name not found.";
                        _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
                    }
                    else
                    {
                        string _clanName;
                        if (!ClanData.Owners.TryGetValue(_MembertoDemote.playerId, out _clanName))
                        {
                            string _reason = "Player is not a member of your clan.";
                            _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
                        }
                        else
                        {
                            if (_clanName != _clan)
                            {
                                string _reason = "Player is not a member of your clan.";
                                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
                            }
                            else
                            {
                                if (!ClanData.OfficersList.Contains(_MembertoDemote.playerId))
                                {
                                    string _reason = "Player is not an officer.";
                                    _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
                                }
                                else
                                {
                                    ClanData.Officers.Remove(_MembertoDemote.playerId);
                                    ClanData.UpdateOfficerData();
                                    string _reason = "Player has been demoted.";
                                    _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
