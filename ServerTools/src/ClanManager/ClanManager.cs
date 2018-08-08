using System.Collections.Generic;

namespace ServerTools
{
    public class ClanManager
    {
        public static bool IsEnabled = false;
        public static List<string> ClanMember = new List<string>();
        public static string Private_Chat_Color = "[00FF00]";

        public static void BuildList()
        {
            for (int i = 0; i < PersistentContainer.Instance.Players.SteamIDs.Count; i++)
            {
                string _id = PersistentContainer.Instance.Players.SteamIDs[i];
                Player p = PersistentContainer.Instance.Players[_id, false];
                {
                    if (p.ClanName != null)
                    {
                        ClanMember.Add(_id);
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
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase101), Config.Server_Response_Name, false, "ServerTools", false));
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
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase103), Config.Server_Response_Name, false, "ServerTools", false));
                }
                else
                {
                    if (PersistentContainer.Instance.Players.ClanList.Contains(_clanName))
                    {
                        string _phrase102;
                        if (!Phrases.Dict.TryGetValue(102, out _phrase102))
                        {
                            _phrase102 = "{PlayerName} can not add the clan {ClanName} because it already exist.";
                        }
                        _phrase102 = _phrase102.Replace("{PlayerName}", _cInfo.playerName);
                        _phrase102 = _phrase102.Replace("{ClanName}", _clanName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase102), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(_clanName) || _clanName.Length < 3)
                        {
                            string _phrase129;
                            if (!Phrases.Dict.TryGetValue(129, out _phrase129))
                            {
                                _phrase129 = "{PlayerName} the clanName must be longer the 3 characters";
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase129), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                            _phrase129 = _phrase129.Replace("{PlayerName}", _cInfo.playerName);
                        }
                        else
                        {
                            ClanMember.Add(_cInfo.playerId);
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
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase104), Config.Server_Response_Name, false, "ServerTools", false));
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
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase105), Config.Server_Response_Name, false, "ServerTools", false));
            }
            else
            {
                List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                foreach (ClientInfo _cInfo1 in _cInfoList)
                {
                    Player p1 = PersistentContainer.Instance.Players[_cInfo1.playerId, true];
                    if (p1.ClanName == p.ClanName && !p1.IsClanOwner)
                    {
                        ClanMember.Remove(_cInfo1.playerId);
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
                        _cInfo1.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase121), Config.Server_Response_Name, false, "ServerTools", false));
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
                        ClanMember.Remove(_id);
                        PersistentContainer.Instance.Players[_id, false].IsClanOfficer = false;
                    }
                    if (p1.ClanName == p.ClanName && !p1.IsClanOwner)
                    {
                        ClanMember.Remove(_id);
                        PersistentContainer.Instance.Players[_id, false].ClanName = null;
                    }
                }
                ClanMember.Remove(_cInfo.playerId);
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
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase106), Config.Server_Response_Name, false, "ServerTools", false));
                PersistentContainer.Instance.Save();
            }
        }

        public static void ClanRename(ClientInfo _cInfo, string _clanName)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, true];
            if (p.IsClanOwner)
            {
                string _oldName = PersistentContainer.Instance.Players[_cInfo.playerId, false].ClanName;
                PersistentContainer.Instance.Players[_cInfo.playerId, true].ClanName = _clanName;
                PersistentContainer.Instance.Save();
                string _phrase130;
                if (!Phrases.Dict.TryGetValue(130, out _phrase130))
                {
                    _phrase130 = "{PlayerName} you have changed your clan name to {ClanName}.";
                }
                _phrase130 = _phrase130.Replace("{PlayerName}", _cInfo.playerName);
                _phrase130 = _phrase130.Replace("{ClanName}", p.ClanName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", _phrase130, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                for (int i = 0; i < PersistentContainer.Instance.Players.SteamIDs.Count; i++)
                {
                    string _id = PersistentContainer.Instance.Players.SteamIDs[i];
                    Player p1 = PersistentContainer.Instance.Players[_id, false];
                    {
                        if (p1.ClanName == _oldName)
                        {
                            PersistentContainer.Instance.Players[_id, false].ClanName = _clanName;
                            PersistentContainer.Instance.Save();
                            ClientInfo _cInfo1 = ConnectionManager.Instance.GetClientInfoForPlayerId(_id);
                            if (_cInfo1 != null)
                            {
                                string _phrase131;
                                if (!Phrases.Dict.TryGetValue(131, out _phrase131))
                                {
                                    _phrase131 = "{PlayerName} your clan name has been changed by the owner to {ClanName}.";
                                }
                                _phrase131 = _phrase131.Replace("{PlayerName}", _cInfo1.playerName);
                                _phrase131 = _phrase131.Replace("{ClanName}", p1.ClanName);
                                _cInfo1.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", _phrase131, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                        }
                    }
                }
            }
            else
            {
                string _phrase105;
                if (!Phrases.Dict.TryGetValue(105, out _phrase105))
                {
                    _phrase105 = "{PlayerName} you are not the owner of any clans.";
                }
                _phrase105 = _phrase105.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", _phrase105, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase107, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase108, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase109, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase110, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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
                            _newMember.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase111, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase112, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase113, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
            }
            else
            {
                ClanMember.Add(_cInfo.playerId);
                PersistentContainer.Instance.Players[_cInfo.playerId, true].ClanName = p.InvitedToClan;
                PersistentContainer.Instance.Players[_cInfo.playerId, true].InvitedToClan = null;
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
                        _cInfo1.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase115, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase113, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase116, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase107, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase108, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase117, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase118, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                    else
                    {
                        ClanMember.Remove(_cInfo.playerId);
                        PersistentContainer.Instance.Players[_steamId, true].ClanName = null;
                        PersistentContainer.Instance.Players[_steamId, true].IsClanOfficer = false;
                        PersistentContainer.Instance.Save();
                        string _phrase120;
                        string _phrase121;
                        if (!Phrases.Dict.TryGetValue(120, out _phrase120))
                        {
                            _phrase120 = "{PlayerName} you have removed {PlayertoRemove} from clan {ClanName}.";
                        }
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase120, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                        if (_PlayertoRemove != null)
                        {
                            if (!Phrases.Dict.TryGetValue(121, out _phrase121))
                            {
                                _phrase121 = "{PlayerName} you have been removed from the clan {ClanName}.";
                            }
                            _phrase120 = _phrase120.Replace("{PlayerName}", _cInfo.playerName);
                            _phrase120 = _phrase120.Replace("{PlayertoRemove}", _playerName);
                            _phrase120 = _phrase120.Replace("{ClanName}", p.ClanName);
                            _phrase121 = _phrase121.Replace("{PlayerName}", _playerName);
                            _phrase121 = _phrase121.Replace("{ClanName}", p.ClanName);
                            _PlayertoRemove.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase121, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase107, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase108, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase117, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase122, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase123, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase107, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase108, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase117, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase124, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase125, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase126, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase127, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                }
                else
                {
                    ClanMember.Remove(_cInfo.playerId);
                    string _phrase121;
                    if (!Phrases.Dict.TryGetValue(121, out _phrase121))
                    {
                        _phrase121 = "{PlayerName} you have been removed from the clan {ClanName}.";
                    }
                    _phrase121 = _phrase121.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase121 = _phrase121.Replace("{ClanName}", p.ClanName);
                    PersistentContainer.Instance.Players[_cInfo.playerId, true].ClanName = null;
                    if (p.IsClanOfficer)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].IsClanOfficer = false;
                    }
                    PersistentContainer.Instance.Save();
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase121, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
        }

        private static void RequestToJoinClan(ClientInfo _cInfo, string _clanName)
        {
        }

        public static string GetChatCommands(ClientInfo _cInfo)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, true];
            string _commands = string.Format("{0}Clan commands are:", Config.Chat_Response_Color);
            if (!p.IsClanOwner && !p.IsClanOfficer && p.ClanName == "" && p.InvitedToClan == null)
            {
                _commands = string.Format("{0} /clanadd {ClanName}", _commands);
            }
            if (p.IsClanOwner && !p.IsClanOfficer && p.ClanName != "")
            {
                _commands = string.Format("{0} /clanrename {ClanName}", _commands);
            }
            if (p.IsClanOwner)
            {
                _commands = string.Format("{0} /clanpromote", _commands);
                _commands = string.Format("{0} /clandemote", _commands);
                _commands = string.Format("{0} /clandel", _commands);
            }
            if (p.IsClanOwner || p.IsClanOfficer)
            {
                _commands = string.Format("{0} /claninvite", _commands);
                _commands = string.Format("{0} /clanremove", _commands);
            }
            if (p.InvitedToClan != null)
            {
                _commands = string.Format("{0} /clanaccept", _commands);
                _commands = string.Format("{0} /clandecline", _commands);
            }
            if (!p.IsClanOwner && p.ClanName != null)
            {
                _commands = string.Format("{0} /clanleave", _commands);
            }
            return _commands;
        }

        public static void Clan(ClientInfo _cInfo, string _message)
        {
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
            List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
            for (int i = 0; i < _cInfoList.Count; i++)
            {
                ClientInfo _cInfo1 = _cInfoList[i];
                Player p1 = PersistentContainer.Instance.Players[_cInfo1.playerId, false];
                if (p.ClanName == p1.ClanName)
                {
                    _cInfo1.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Private_Chat_Color, _message), _cInfo.playerName, false, "", false));
                    if (ChatLog.IsEnabled)
                    {
                        ChatLog.Log(_message, _cInfo.playerName);
                    }
                }
            }
        }
    }
}