using System.Collections.Generic;

namespace ServerTools
{
    public class ClanManager
    {
        public static bool IsEnabled = false;

        private static void CheckforClantag(ClientInfo _cInfo)
        {
            string _pName = _cInfo.playerName.ToLower();
            foreach (string _clan in ClanData.ClansList)
            {
                if (_pName.Contains(_clan))
                {
                    if (!ClanData.PlayersList.Contains(_cInfo.playerId))
                    {
                        string _phrase100 = "You do not belong to the clan {ClanName}. Please remove the clan tag and rejoin.";
                        if (!Phrases.Dict.TryGetValue(100, out _phrase100))
                        {
                            Log.Out("[SERVERTOOLS] Phrase 100 not found using default.");
                        }
                        _phrase100 = _phrase100.Replace("{ClanName}", _clan);
                        KickPlayer(_cInfo, _phrase100);
                    }
                    else
                    {
                        string _c = null;
                        if (ClanData.Pdict.TryGetValue(_cInfo.playerId, out _c))
                        {
                            if (_clan != _c)
                            {
                                string _phrase100 = "You do not belong to the clan {ClanName}. Please remove the clan tag and rejoin.";
                                if (!Phrases.Dict.TryGetValue(100, out _phrase100))
                                {
                                    Log.Out("[SERVERTOOLS] Phrase 100 not found using default.");
                                }
                                _phrase100 = _phrase100.Replace("{ClanName}", _clan);
                                KickPlayer(_cInfo, _phrase100);
                            }
                        }
                    }
                }
            }
        }

        private static void KickPlayer(ClientInfo _cInfo, string _reason)
        {
            SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.entityId, _reason), _cInfo);
        }

        public static void AddClan(ClientInfo _cInfo, string _clanName)
        {
            if (ClanData.OwnersList.Contains(_cInfo.playerId))
            {
                foreach (KeyValuePair<string, string> kvp in ClanData.Cdict)
                {
                    if (kvp.Value == _cInfo.playerId)
                    {
                        string _phrase101 = "{PlayerName} you have already created the clan {ClanName}.";
                        if (!Phrases.Dict.TryGetValue(101, out _phrase101))
                        {
                            Log.Out("[SERVERTOOLS] Phrase 101 not found using default.");
                        }
                        _phrase101 = _phrase101.Replace("{PlayerName}", _cInfo.playerName);
                        _phrase101 = _phrase101.Replace("{ClanName}", kvp.Key);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase101, CustomCommands.ChatColor), "Server", false, "", false));
                        return;
                    }
                }
            }
            else
            {
                if (ClanData.PlayersList.Contains(_cInfo.playerId))
                {
                    string _c = null;
                    if (ClanData.Pdict.TryGetValue(_cInfo.playerId, out _c))
                    {
                        string _phrase103 = "{PlayerName} you are currently a member of the clan {ClanName}.";
                        if (!Phrases.Dict.TryGetValue(103, out _phrase103))
                        {
                            Log.Out("[SERVERTOOLS] Phrase 103 not found using default.");
                        }
                        _phrase103 = _phrase103.Replace("{PlayerName}", _cInfo.playerName);
                        _phrase103 = _phrase103.Replace("{ClanName}", _c);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase103, CustomCommands.ChatColor), "Server", false, "", false));
                    }
                }
                
                else
                {
                    _clanName = _clanName.ToLower();
                    if (ClanData.ClansList.Contains(_clanName))
                    {
                        string _phrase102 = "{PlayerName} can not add the clan {ClanName} because it already exist.";
                        if (!Phrases.Dict.TryGetValue(102, out _phrase102))
                        {
                            Log.Out("[SERVERTOOLS] Phrase 102 not found using default.");
                        }
                        _phrase102 = _phrase102.Replace("{PlayerName}", _cInfo.playerName);
                        _phrase102 = _phrase102.Replace("{ClanName}", _clanName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase102, CustomCommands.ChatColor), "Server", false, "", false));
                    }
                    else
                    {
                        ClanData.AddClan(_clanName, _cInfo.playerId);
                        string _phrase104 = "{PlayerName} you have added the clan {ClanName}.";
                        if (!Phrases.Dict.TryGetValue(104, out _phrase104))
                        {
                            Log.Out("[SERVERTOOLS] Phrase 104 not found using default.");
                        }
                        _phrase104 = _phrase104.Replace("{PlayerName}", _cInfo.playerName);
                        _phrase104 = _phrase104.Replace("{ClanName}", _clanName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase104, CustomCommands.ChatColor), "Server", false, "", false));
                    }
                }
            }
        }

        public static void RemoveClan(ClientInfo _cInfo)
        {
            if (!ClanData.OwnersList.Contains(_cInfo.playerId))
            {
                string _phrase105 = "{PlayerName} you are not the owner of any clans.";
                if (!Phrases.Dict.TryGetValue(105, out _phrase105))
                {
                    Log.Out("[SERVERTOOLS] Phrase 105 not found using default.");
                }
                _phrase105 = _phrase105.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase105, CustomCommands.ChatColor), "Server", false, "", false));
            }
            else
            {
                foreach (KeyValuePair<string, string> kvp in ClanData.Cdict)
                {
                    if (kvp.Value == _cInfo.playerId)
                    {
                        ClanData.RemoveClan(kvp.Key, _cInfo.playerId);
                        string _phrase106 = "{PlayerName} you have removed the clan {ClanName}.";
                        if (!Phrases.Dict.TryGetValue(106, out _phrase106))
                        {
                            Log.Out("[SERVERTOOLS] Phrase 106 not found using default.");
                        }
                        _phrase106 = _phrase106.Replace("{PlayerName}", _cInfo.playerName);
                        _phrase106 = _phrase106.Replace("{ClanName}", kvp.Key);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase106, CustomCommands.ChatColor), "Server", false, "", false));
                        return;
                    }
                }
            }
        }

        public static void InviteMember(ClientInfo _cInfo, string _playerName)
        {
            if (!ClanData.OwnersList.Contains(_cInfo.playerId) || !ClanData.OfficersList.Contains(_cInfo.playerId))
            {
                string _phrase107 = "{PlayerName} you do not have permissions to use this command.";
                if (!Phrases.Dict.TryGetValue(107, out _phrase107))
                {
                    Log.Out("[SERVERTOOLS] Phrase 107 not found using default.");
                }
                _phrase107 = _phrase107.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase107, CustomCommands.ChatColor), "Server", false, "", false));
            }
            else
            {
                ClientInfo _newMember = ConsoleHelper.ParseParamIdOrName(_playerName);
                if (_newMember == null)
                {
                    string _phrase108 = "{PlayerName} the name {TargetPlayerName} was not found.";
                    if (!Phrases.Dict.TryGetValue(108, out _phrase108))
                    {
                        Log.Out("[SERVERTOOLS] Phrase 108 not found using default.");
                    }
                    _phrase108 = _phrase108.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase108 = _phrase108.Replace("{TargetPlayerName}", _playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase108, CustomCommands.ChatColor), "Server", false, "", false));
                }
                else
                {
                    if (ClanData.PlayersList.Contains(_newMember.playerId))
                    {
                        string _phrase109 = "{PlayerName} is already a member of a clan.";
                        if (!Phrases.Dict.TryGetValue(109, out _phrase109))
                        {
                            Log.Out("[SERVERTOOLS] Phrase 109 not found using default.");
                        }
                        _phrase109 = _phrase109.Replace("{PlayerName}", _playerName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase109, CustomCommands.ChatColor), "Server", false, "", false));
                    }
                    else
                    {
                        if (ClanData.InvitesList.Contains(_newMember.playerId))
                        {
                            string _phrase110 = "{PlayerName} already has pending clan invites.";
                            if (!Phrases.Dict.TryGetValue(110, out _phrase110))
                            {
                                Log.Out("[SERVERTOOLS] Phrase 110 not found using default.");
                            }
                            _phrase110 = _phrase110.Replace("{PlayerName}", _playerName);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase110, CustomCommands.ChatColor), "Server", false, "", false));
                        }
                        else
                        {
                            string _phrase111 = "{PlayerName} you have been invited to join the clan {ClanName}. Type /clanaccept to join or /clandecline to decline the offer.";
                            string _phrase112 = "{PlayerName} you have invited {InvitedPlayerName} to the clan {ClanName}.";
                            if (!Phrases.Dict.TryGetValue(111, out _phrase111))
                            {
                                Log.Out("[SERVERTOOLS] Phrase 111 not found using default.");
                            }

                            if (!Phrases.Dict.TryGetValue(112, out _phrase112))
                            {
                                Log.Out("[SERVERTOOLS] Phrase 112 not found using default.");
                            }
                            string _clan = null;
                            if (ClanData.Pdict.TryGetValue(_cInfo.playerId, out _clan))
                            {
                                _phrase111 = _phrase111.Replace("{PlayerName}", _newMember.playerName);
                                _phrase111 = _phrase111.Replace("{ClanName}", _clan);
                                _phrase112 = _phrase112.Replace("{PlayerName}", _cInfo.playerName);
                                _phrase112 = _phrase112.Replace("{InvitedPlayerName}", _newMember.playerName);
                                _phrase112 = _phrase112.Replace("{ClanName}", _clan);
                                ClanData.InviteMember(_newMember.playerId, _clan);
                                _newMember.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase111, CustomCommands.ChatColor), "Server", false, "", false));
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase112, CustomCommands.ChatColor), "Server", false, "", false));
                            }
                        }
                    }
                }
            }
        }

        public static void InviteAccept(ClientInfo _cInfo)
        {
            if (!ClanData.InvitesList.Contains(_cInfo.playerId))
            {
                string _phrase113 = "{PlayerName} you have not been invited to any clans.";
                if (!Phrases.Dict.TryGetValue(113, out _phrase113))
                {
                    Log.Out("[SERVERTOOLS] Phrase 113 not found using default.");
                }
                _phrase113 = _phrase113.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase113, CustomCommands.ChatColor), "Server", false, "", false));
            }
            else
            {
                string _clan = null;
                if (!ClanData.idict.TryGetValue(_cInfo.playerId, out _clan))
                {
                    string _phrase114 = "{PlayerName} the clan could not be found.";
                    if (!Phrases.Dict.TryGetValue(114, out _phrase114))
                    {
                        Log.Out("[SERVERTOOLS] Phrase 114 not found using default.");
                    }
                    _phrase114 = _phrase114.Replace("{PlayerName}", _cInfo.playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase114, CustomCommands.ChatColor), "Server", false, "", false));
                }
                else
                {
                    ClanData.AddMember(_clan, _cInfo.playerId);
                    List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                    foreach (ClientInfo _cInfop in _cInfoList)
                    {
                        string _clan1 = null;
                        if (ClanData.Pdict.TryGetValue(_cInfop.playerId, out _clan1))
                        {
                            if (_clan == _clan1)
                            {
                                string _phrase115 = "{PlayerName} has joined the clan.";
                                if (!Phrases.Dict.TryGetValue(115, out _phrase115))
                                {
                                    Log.Out("[SERVERTOOLS] Phrase 115 not found using default.");
                                }
                                _phrase115 = _phrase115.Replace("{PlayerName}", _cInfo.playerName);
                                _cInfop.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase115, CustomCommands.ChatColor), "Server", false, "", false));
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
                if (!Phrases.Dict.TryGetValue(113, out _phrase113))
                {
                    Log.Out("[SERVERTOOLS] Phrase 113 not found using default.");
                }
                _phrase113 = _phrase113.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase113, CustomCommands.ChatColor), "Server", false, "", false));
            }
            else
            {
                ClanData.idict.Remove(_cInfo.playerId);
                ClanData.UpdateInviteData();
                string _phrase116 = "{PlayerName} you have declined the invite to the clan.";
                if (!Phrases.Dict.TryGetValue(116, out _phrase116))
                {
                    Log.Out("[SERVERTOOLS] Phrase 116 not found using default.");
                }
                _phrase116 = _phrase116.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase116, CustomCommands.ChatColor), "Server", false, "", false));
            }
        }

        public static void RemoveMember(ClientInfo _cInfo, string _playerName)
        {
            if (!ClanData.OfficersList.Contains(_cInfo.playerId))
            {
                string _phrase107 = "{PlayerName} you do not have permissions to use this command.";
                if (!Phrases.Dict.TryGetValue(107, out _phrase107))
                {
                    Log.Out("[SERVERTOOLS] Phrase 107 not found using default.");
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
                        string _phrase108 = "{PlayerName} the name {TargetPlayerName} was not found.";
                        if (!Phrases.Dict.TryGetValue(108, out _phrase108))
                        {
                            Log.Out("[SERVERTOOLS] Phrase 108 not found using default.");
                        }
                        _phrase108 = _phrase108.Replace("{PlayerName}", _cInfo.playerName);
                        _phrase108 = _phrase108.Replace("{TargetPlayerName}", _playerName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase108, CustomCommands.ChatColor), "Server", false, "", false));
                        return;
                    }
                }
                string _clan = null;
                string _clanName = null;
                if (!ClanData.Pdict.TryGetValue(_cInfo.playerId, out _clan) || !ClanData.Pdict.TryGetValue(_steamId, out _clanName))
                {

                    string _phrase117 = "{PlayerName} is not a member of your clan.";
                    if (!Phrases.Dict.TryGetValue(117, out _phrase117))
                    {
                        Log.Out("[SERVERTOOLS] Phrase 117 not found using default.");
                    }
                    _phrase117 = _phrase117.Replace("{PlayerName}", _playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase117, CustomCommands.ChatColor), "Server", false, "", false));
                }
                else
                {
                    if (_clanName != _clan)
                    {
                        string _phrase117 = "{PlayerName} is not a member of your clan.";
                        if (!Phrases.Dict.TryGetValue(117, out _phrase117))
                        {
                            Log.Out("[SERVERTOOLS] Phrase 117 not found using default.");
                        }
                        _phrase117 = _phrase117.Replace("{PlayerName}", _playerName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase117, CustomCommands.ChatColor), "Server", false, "", false));
                    }
                    else
                    {
                        if (ClanData.OfficersList.Contains(_steamId) || ClanData.OwnersList.Contains(_steamId))
                        {
                            string _phrase118 = "{PlayerName} only the clan owner can remove officers.";
                            if (!Phrases.Dict.TryGetValue(118, out _phrase118))
                            {
                                Log.Out("[SERVERTOOLS] Phrase 118 not found using default.");
                            }
                            _phrase118 = _phrase118.Replace("{PlayerName}", _cInfo.playerName);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase118, CustomCommands.ChatColor), "Server", false, "", false));
                        }
                        else
                        {
                            ClanData.RemoveMember(_steamId);
                            string _phrase120 = "{PlayerName} you have removed {PlayertoRemove} from clan {ClanName}.";
                            string _phrase121 = "{PlayerName} you have been removed from the clan {ClanName}.";
                            if (!Phrases.Dict.TryGetValue(120, out _phrase120))
                            {
                                Log.Out("[SERVERTOOLS] Phrase 120 not found using default.");
                            }
                            if (!Phrases.Dict.TryGetValue(121, out _phrase121))
                            {
                                Log.Out("[SERVERTOOLS] Phrase 121 not found using default.");
                            }
                            _phrase120 = _phrase120.Replace("{PlayerName}", _cInfo.playerName);
                            _phrase120 = _phrase120.Replace("{PlayertoRemove}", _playerName);
                            _phrase120 = _phrase120.Replace("{ClanName}", _clanName);
                            _phrase121 = _phrase121.Replace("{PlayerName}", _playerName);
                            _phrase121 = _phrase121.Replace("{ClanName}", _clanName);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase120, CustomCommands.ChatColor), "Server", false, "", false));
                            if (_PlayertoRemove != null)
                            {
                                _PlayertoRemove.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase121, CustomCommands.ChatColor), "Server", false, "", false));
                            }
                        }
                    }
                }
            }
        }

        public static void PromoteMember(ClientInfo _cInfo, string _playerName)
        {
            if (!ClanData.OwnersList.Contains(_cInfo.playerId) )
            {
                string _phrase107 = "{PlayerName} you do not have permissions to use this command.";
                if (!Phrases.Dict.TryGetValue(107, out _phrase107))
                {
                    Log.Out("[SERVERTOOLS] Phrase 107 not found using default.");
                }
                _phrase107 = _phrase107.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase107, CustomCommands.ChatColor), "Server", false, "", false));
            }
            else
            {
                ClientInfo _playertoPromote = ConsoleHelper.ParseParamIdOrName(_playerName);
                if (_playertoPromote == null)
                {
                    string _phrase108 = "{PlayerName} the name {TargetPlayerName} was not found.";
                    if (!Phrases.Dict.TryGetValue(108, out _phrase108))
                    {
                        Log.Out("[SERVERTOOLS] Phrase 108 not found using default.");
                    }
                    _phrase108 = _phrase108.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase108 = _phrase108.Replace("{TargetPlayerName}", _playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase108, CustomCommands.ChatColor), "Server", false, "", false));
                }
                else
                {
                    if (!ClanData.PlayersList.Contains(_playertoPromote.playerId))
                    {
                        string _phrase117 = "{PlayerName} is not a member of your clan.";
                        if (!Phrases.Dict.TryGetValue(117, out _phrase117))
                        {
                            Log.Out("[SERVERTOOLS] Phrase 117 not found using default.");
                        }
                        _phrase117 = _phrase117.Replace("{PlayerName}", _playerName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase117, CustomCommands.ChatColor), "Server", false, "", false));
                    }
                    else
                    {
                        string _clan = null;
                        string _clan1 = null;
                        if (!ClanData.Pdict.TryGetValue(_playertoPromote.playerId, out _clan) || !ClanData.Pdict.TryGetValue(_cInfo.playerId, out _clan1))
                        {
                            string _phrase117 = "{PlayerName} is not a member of your clan.";
                            if (!Phrases.Dict.TryGetValue(117, out _phrase117))
                            {
                                Log.Out("[SERVERTOOLS] Phrase 117 not found using default.");
                            }
                            _phrase117 = _phrase117.Replace("{PlayerName}", _playerName);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase117, CustomCommands.ChatColor), "Server", false, "", false));    
                        }
                        else
                        {
                            if (_clan1 != _clan)
                            {
                                string _phrase117 = "{PlayerName} is not a member of your clan.";
                                if (!Phrases.Dict.TryGetValue(117, out _phrase117))
                                {
                                    Log.Out("[SERVERTOOLS] Phrase 117 not found using default.");
                                }
                                _phrase117 = _phrase117.Replace("{PlayerName}", _playerName);
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase117, CustomCommands.ChatColor), "Server", false, "", false));
                            }
                            else
                            {
                                if (ClanData.OfficersList.Contains(_playertoPromote.playerId))
                                {
                                    string _phrase122 = "{PlayerName} is already a officer.";
                                    if (!Phrases.Dict.TryGetValue(122, out _phrase122))
                                    {
                                        Log.Out("[SERVERTOOLS] Phrase 122 not found using default.");
                                    }
                                    _phrase122 = _phrase122.Replace("{PlayerName}", _playerName);
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase122, CustomCommands.ChatColor), "Server", false, "", false));
                                }
                                else
                                {
                                    PromoteMember(_playertoPromote, _clan);
                                    string _phrase123 = "{PlayerName} has been promoted to an officer.";
                                    if (!Phrases.Dict.TryGetValue(123, out _phrase123))
                                    {
                                        Log.Out("[SERVERTOOLS] Phrase 123 not found using default.");
                                    }
                                    _phrase123 = _phrase123.Replace("{PlayerName}", _playerName);
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase123, CustomCommands.ChatColor), "Server", false, "", false));
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
                string _phrase107 = "{PlayerName} you do not have permissions to use this command.";
                if (!Phrases.Dict.TryGetValue(107, out _phrase107))
                {
                    Log.Out("[SERVERTOOLS] Phrase 107 not found using default.");
                }
                _phrase107 = _phrase107.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase107, CustomCommands.ChatColor), "Server", false, "", false));
            }
            else
            {
                ClientInfo _membertoDemote = ConsoleHelper.ParseParamIdOrName(_playerName);
                if (_membertoDemote == null)
                {
                    string _phrase108 = "{PlayerName} the name {TargetPlayerName} was not found.";
                    if (!Phrases.Dict.TryGetValue(108, out _phrase108))
                    {
                        Log.Out("[SERVERTOOLS] Phrase 108 not found using default.");
                    }
                    _phrase108 = _phrase108.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase108 = _phrase108.Replace("{TargetPlayerName}", _playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase108, CustomCommands.ChatColor), "Server", false, "", false));
                }
                else
                {
                    string _clan = null;
                    string _clan1 = null;
                    if (!ClanData.Pdict.TryGetValue(_membertoDemote.playerId, out _clan) || !ClanData.Pdict.TryGetValue(_cInfo.playerId, out _clan1))
                    {
                        string _phrase117 = "{PlayerName} is not a member of your clan.";
                        if (!Phrases.Dict.TryGetValue(117, out _phrase117))
                        {
                            Log.Out("[SERVERTOOLS] Phrase 117 not found using default.");
                        }
                        _phrase117 = _phrase117.Replace("{PlayerName}", _playerName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase117, CustomCommands.ChatColor), "Server", false, "", false));
                    }
                    else
                    {
                        if (_clan1 != _clan)
                        {
                            string _phrase117 = "{PlayerName} is not a member of your clan.";
                            if (!Phrases.Dict.TryGetValue(117, out _phrase117))
                            {
                                Log.Out("[SERVERTOOLS] Phrase 117 not found using default.");
                            }
                            _phrase117 = _phrase117.Replace("{PlayerName}", _playerName);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase117, CustomCommands.ChatColor), "Server", false, "", false));
                        }
                        else
                        {
                            if (!ClanData.OfficersList.Contains(_membertoDemote.playerId))
                            {
                                string _phrase124 = "{PlayerName} is not an officer.";
                                if (!Phrases.Dict.TryGetValue(124, out _phrase124))
                                {
                                    Log.Out("[SERVERTOOLS] Phrase 124 not found using default.");
                                }
                                _phrase124 = _phrase124.Replace("{PlayerName}", _playerName);
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase124, CustomCommands.ChatColor), "Server", false, "", false));
                            }
                            else
                            {
                                ClanData.DemoteMember(_membertoDemote.playerId);
                                string _phrase125 = "{PlayerName} has been demoted.";
                                if (!Phrases.Dict.TryGetValue(125, out _phrase125))
                                {
                                    Log.Out("[SERVERTOOLS] Phrase 125 not found using default.");
                                }
                                _phrase125 = _phrase125.Replace("{PlayerName}", _playerName);
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase125, CustomCommands.ChatColor), "Server", false, "", false));
                            }
                        }
                    }
                }
            }
        }

        public static void LeaveClan(ClientInfo _cInfo)
        {
            if (ClanData.OwnersList.Contains(_cInfo.playerId))
            {
                string _phrase126 = "{PlayerName} you can not leave the clan because you are the owner. You can only delete the clan.";
                if (!Phrases.Dict.TryGetValue(126, out _phrase126))
                {
                    Log.Out("[SERVERTOOLS] Phrase 126 not found using default.");
                }
                _phrase126 = _phrase126.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase126, CustomCommands.ChatColor), "Server", false, "", false));
            }
            else
            {
                if (!ClanData.PlayersList.Contains(_cInfo.playerId))
                {
                    string _phrase127 = "{PlayerName} you do not belong to any clans.";
                    if (!Phrases.Dict.TryGetValue(127, out _phrase127))
                    {
                        Log.Out("[SERVERTOOLS] Phrase 127 not found using default.");
                    }
                    _phrase127 = _phrase127.Replace("{PlayerName}", _cInfo.playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase127, CustomCommands.ChatColor), "Server", false, "", false));
                }
                else
                {
                    string _clanName = null;
                    if (!ClanData.Pdict.TryGetValue(_cInfo.playerId, out _clanName))
                    {
                        string _phrase127 = "{PlayerName} you do not belong to any clans.";
                        if (!Phrases.Dict.TryGetValue(127, out _phrase127))
                        {
                            Log.Out("[SERVERTOOLS] Phrase 127 not found using default.");
                        }
                        _phrase127 = _phrase127.Replace("{PlayerName}", _cInfo.playerName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase127, CustomCommands.ChatColor), "Server", false, "", false));
                    }
                    else
                    {
                        ClanData.RemoveMember(_cInfo.playerId);
                        string _phrase121 = "{PlayerName} you have been removed from the clan {ClanName}.";
                        if (!Phrases.Dict.TryGetValue(121, out _phrase121))
                        {
                            Log.Out("[SERVERTOOLS] Phrase 121 not found using default.");
                        }
                        _phrase121 = _phrase121.Replace("{PlayerName}", _cInfo.playerName);
                        _phrase121 = _phrase121.Replace("{ClanName}", _clanName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase121, CustomCommands.ChatColor), "Server", false, "", false));
                    } 
                }
            }
        }

        private static void RequestToJoinClan(ClientInfo _cInfo, string _clanName)
        {
        }

        private static void GetChatCommands(ClientInfo _cInfo)
        {
            string _commands = string.Format("{0}Clan commands are:", CustomCommands.ChatColor); ;
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
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}", _commands), "Server", false, "", false));
        }

        public static void Clan(ClientInfo _cInfo, string _message)
        {
            string _clan = null;
            if (ClanData.Pdict.TryGetValue(_cInfo.playerId, out _clan))
            {
                List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                foreach (ClientInfo _cInfo1 in _cInfoList)
                {
                    if (ClanData.PlayersList.Contains(_cInfo1.playerId))
                    {
                        string _clanName = null;
                        if (ClanData.Pdict.TryGetValue(_cInfo1.playerId, out _clanName))
                        {
                            if (_clan == _clanName)
                            {
                                _cInfo1.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("[804040]{0}[-]", _message), _cInfo.playerName, false, "", false));
                            }
                        }
                    }
                }
            }
        }
    }
}