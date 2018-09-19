using System.Collections.Generic;
using System.Data;

namespace ServerTools
{
    public class ClanManager
    {
        public static bool IsEnabled = false;
        public static List<string> ClanMember = new List<string>();
        public static string Private_Chat_Color = "[00FF00]";
        public static Dictionary<string, string> clans = new Dictionary<string, string>();

        public static List<string> ClanList
        {
            get
            {
                return new List<string>(clans.Keys);
            }
        }

        public static void GetClans()
        {
            string _sql = "SELECT steamid, clanname FROM Players WHERE clanname != 'Unknown' AND isclanowner = 'true'";
            DataTable _result = SQL.TQuery(_sql);
            foreach (DataRow row in _result.Rows)
            {
                if (!clans.ContainsKey(row[1].ToString()))
                {
                    clans.Add(row[1].ToString(), row[0].ToString());
                }
            }
            _result.Dispose();
        }

        public static void BuildList()
        {
            string _sql = "SELECT steamid FROM Players WHERE clanname != 'Unknown'";
            DataTable _result = SQL.TQuery(_sql);
            foreach (DataRow row in _result.Rows)
            {
                ClanMember.Add(row[0].ToString());
            }
            _result.Dispose();
        }

        public static void AddClan(ClientInfo _cInfo, string _clanName)
        {
            string _sql = string.Format("SELECT clanname, isclanowner FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            string _clan = _result.Rows[0].ItemArray.GetValue(0).ToString();
            bool _isclanowner;
            bool.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _isclanowner);
            _result.Dispose();
            if (_isclanowner)
            {
                string _phrase101;
                if (!Phrases.Dict.TryGetValue(101, out _phrase101))
                {
                    _phrase101 = "{PlayerName} you have already created the clan {ClanName}.";
                }
                _phrase101 = _phrase101.Replace("{PlayerName}", _cInfo.playerName);
                _phrase101 = _phrase101.Replace("{ClanName}", _clan.ToString());
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase101), Config.Server_Response_Name, false, "ServerTools", false));
            }
            else
            {
                if (_clan.ToString() != "Unknown")
                {
                    string _phrase103;
                    if (!Phrases.Dict.TryGetValue(103, out _phrase103))
                    {
                        _phrase103 = "{PlayerName} you are currently a member of the clan {ClanName}.";
                    }
                    _phrase103 = _phrase103.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase103 = _phrase103.Replace("{ClanName}", _clan.ToString());
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase103), Config.Server_Response_Name, false, "ServerTools", false));
                }
                else
                {
                    if (ClanList.Contains(_clanName))
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
                                _phrase129 = "{PlayerName} the clanName must be longer than 2 characters.";
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase129), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                            _phrase129 = _phrase129.Replace("{PlayerName}", _cInfo.playerName);
                        }
                        else
                        {
                            _sql = string.Format("UPDATE Players SET clanname = '{0}', isclanowner = 'true', isclanofficer = 'true' WHERE steamid = '{1}'", _clanName, _cInfo.playerId);
                            SQL.FastQuery(_sql);
                            ClanMember.Add(_cInfo.playerId);
                            clans.Add(_clanName, _cInfo.playerId);
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
            string _sql = string.Format("SELECT clanname, isclanowner FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            string _clanname = _result.Rows[0].ItemArray.GetValue(0).ToString();
            bool _isclanowner;
            bool.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _isclanowner);
            _result.Dispose();
            if (!_isclanowner)
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
                _sql = string.Format("SELECT steamid FROM Players WHERE clanname = '{0}'", _clanname);
                DataTable _result1 = SQL.TQuery(_sql);
                _sql = string.Format("UPDATE Players SET clanname = 'Unknown', isclanowner = 'false', isclanofficer = 'false' WHERE clanname = '{0}'", _clanname);
                SQL.FastQuery(_sql);
                _sql = string.Format("UPDATE Players SET invitedtoclan = 'Unknown' WHERE invitedtoclan = '{0}'", _clanname);
                SQL.FastQuery(_sql);
                foreach (DataRow row in _result1.Rows)
                {
                    ClientInfo _cInfo1 = ConsoleHelper.ParseParamIdOrName(row[0].ToString());
                    if (_cInfo1 != null)
                    {
                        string _phrase121;
                        if (!Phrases.Dict.TryGetValue(121, out _phrase121))
                        {
                            _phrase121 = "{PlayerName} you have been removed from the clan {ClanName}.";
                        }
                        _phrase121 = _phrase121.Replace("{PlayerName}", _cInfo1.playerName);
                        _phrase121 = _phrase121.Replace("{ClanName}", _clanname);
                        _cInfo1.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase121), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                    if (ClanMember.Contains(row[0].ToString()))
                    {
                        ClanMember.Remove(row[0].ToString());
                    }
                }
                _result1.Dispose();
                string _phrase106;
                if (!Phrases.Dict.TryGetValue(106, out _phrase106))
                {
                    _phrase106 = "{PlayerName} you have removed the clan {ClanName}.";
                }
                _phrase106 = _phrase106.Replace("{PlayerName}", _cInfo.playerName);
                _phrase106 = _phrase106.Replace("{ClanName}", _clanname);
                clans.Remove(_clanname);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase106), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static void ClanRename(ClientInfo _cInfo, string _clanName)
        {
            string _sql = string.Format("SELECT clanname, isclanowner FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            string _oldClanName = _result.Rows[0].ItemArray.GetValue(0).ToString();
            bool _isclanowner;
            bool.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _isclanowner);
            _result.Dispose();
            Player p = PersistentContainer.Instance.Players[_cInfo.playerId, true];
            if (_isclanowner)
            {
                if (!clans.ContainsKey(_clanName))
                {
                    clans.Remove(_oldClanName);
                    clans.Add(_clanName, _cInfo.playerId);
                    _sql = string.Format("UPDATE Players SET clanname = '{0}' WHERE clanname = '{1}'", _clanName, _oldClanName);
                    SQL.FastQuery(_sql);
                    _sql = string.Format("UPDATE Players SET invitedtoclan = '{0}' WHERE invitedtoclan = '{1}'", _clanName, _oldClanName);
                    SQL.FastQuery(_sql);
                    string _phrase130;
                    if (!Phrases.Dict.TryGetValue(130, out _phrase130))
                    {
                        _phrase130 = "{PlayerName} you have changed your clan name to {ClanName}.";
                    }
                    _phrase130 = _phrase130.Replace("{PlayerName}", _cInfo.playerName);
                    _phrase130 = _phrase130.Replace("{ClanName}", _clanName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", _phrase130, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                    _sql = string.Format("SELECT steamid FROM Players WHERE clanname = '{0}'", _clanName);
                    DataTable _result1 = SQL.TQuery(_sql);
                    foreach (DataRow row in _result1.Rows)
                    {
                        ClientInfo _cInfo1 = ConnectionManager.Instance.GetClientInfoForPlayerId(row[0].ToString());
                        if (_cInfo1 != null)
                        {
                            string _phrase131;
                            if (!Phrases.Dict.TryGetValue(131, out _phrase131))
                            {
                                _phrase131 = "{PlayerName} your clan name has been changed by the owner to {ClanName}.";
                            }
                            _phrase131 = _phrase131.Replace("{PlayerName}", _cInfo1.playerName);
                            _phrase131 = _phrase131.Replace("{ClanName}", _clanName);
                            _cInfo1.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", _phrase131, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                    }
                    _result1.Dispose();
                }
                else
                {
                    string _phrase102;
                    if (!Phrases.Dict.TryGetValue(102, out _phrase102))
                    {
                        _phrase102 = "{PlayerName} can not add the clan {ClanName} because it already exist.";
                    }
                    _phrase102 = _phrase102.Replace("{PlayerName}", _cInfo.playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", _phrase102, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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
            string _sql = string.Format("SELECT clanname, isclanofficer FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            string _clanName = _result.Rows[0].ItemArray.GetValue(0).ToString();
            bool _isclanofficer;
            bool.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _isclanofficer);
            _result.Dispose();
            if (!_isclanofficer)
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
                    _sql = string.Format("SELECT clanname , invitedtoclan FROM Players WHERE steamid = '{0}'", _newMember.playerId);
                    DataTable _result1 = SQL.TQuery(_sql);
                    string _newMemberclanName = _result1.Rows[0].ItemArray.GetValue(0).ToString();
                    string _invitedtoclan = _result1.Rows[0].ItemArray.GetValue(1).ToString();
                    _result1.Dispose();
                    if (_newMemberclanName != "Unknown")
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
                        if (_invitedtoclan != "Unknown")
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
                            _phrase111 = _phrase111.Replace("{ClanName}", _clanName);
                            _phrase112 = _phrase112.Replace("{PlayerName}", _cInfo.playerName);
                            _phrase112 = _phrase112.Replace("{InvitedPlayerName}", _newMember.playerName);
                            _phrase112 = _phrase112.Replace("{ClanName}", _clanName);
                            _sql = string.Format("UPDATE Players SET invitedtoclan = '{0}' WHERE steamid = '{1}'", _clanName, _newMember.playerId);
                            SQL.FastQuery(_sql);
                            _newMember.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase111, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase112, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                    }
                }
            }
        }

        public static void InviteAccept(ClientInfo _cInfo)
        {
            string _sql = string.Format("SELECT invitedtoclan FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            string _invitedtoclan = _result.Rows[0].ItemArray.GetValue(0).ToString();
            _result.Dispose();
            if (_invitedtoclan == "Unknown")
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
                _sql = string.Format("UPDATE Players SET clanname= '{0}', invitedtoclan = 'Unknown' WHERE steamid = '{1}'", _invitedtoclan, _cInfo.playerId);
                SQL.FastQuery(_sql);
                _sql = string.Format("SELECT steamid FROM Players WHERE clanname = '{0}'", _invitedtoclan);
                DataTable _result1 = SQL.TQuery(_sql);
                foreach (DataRow row in _result1.Rows)
                {
                    ClientInfo _cInfo1 = ConsoleHelper.ParseParamIdOrName(row[0].ToString());
                    if (_cInfo1 != null)
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
                _result1.Dispose(); 
            }
        }

        public static void InviteDecline(ClientInfo _cInfo)
        {
            string _sql = string.Format("SELECT invitedtoclan FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            string _invitedtoclan = _result.Rows[0].ItemArray.GetValue(0).ToString();
            _result.Dispose();
            if (_invitedtoclan == "Unknown")
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
                _sql = string.Format("UPDATE Players SET invitedtoclan = 'Unknown' WHERE steamid = '{0}'", _cInfo.playerId);
                SQL.FastQuery(_sql);
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
            string _sql = string.Format("SELECT clanname, isclanowner, isclanofficer FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            string _clanname = _result.Rows[0].ItemArray.GetValue(0).ToString();
            bool _isclanowner;
            bool.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _isclanowner);
            bool _isclanofficer;
            bool.TryParse(_result.Rows[0].ItemArray.GetValue(2).ToString(), out _isclanofficer);
            _result.Dispose();
            if (!_isclanofficer)
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
                _sql = string.Format("SELECT clanname, isclanowner, isclanofficer FROM Players WHERE steamid = '{0}'", _steamId);
                DataTable _result1 = SQL.TQuery(_sql);
                string _clanname1 = _result1.Rows[0].ItemArray.GetValue(0).ToString();
                bool _isclanowner1;
                bool.TryParse(_result1.Rows[0].ItemArray.GetValue(1).ToString(), out _isclanowner1);
                bool _isclanofficer1;
                bool.TryParse(_result1.Rows[0].ItemArray.GetValue(2).ToString(), out _isclanofficer1);
                _result1.Dispose();
                if (_clanname != _clanname1)
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
                    if (_isclanofficer1 && !_isclanowner && !_isclanowner1)
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
                        ClanMember.Remove(_steamId);
                        _sql = string.Format("UPDATE Players SET clanname = 'Unknown', isclanofficer = 'false' WHERE steamid = '{0}'", _steamId);
                        SQL.FastQuery(_sql);
                        string _phrase120;
                        string _phrase121;
                        if (!Phrases.Dict.TryGetValue(120, out _phrase120))
                        {
                            _phrase120 = "{PlayerName} you have removed {PlayertoRemove} from clan {ClanName}.";
                        }
                        _phrase120 = _phrase120.Replace("{PlayerName}", _cInfo.playerName);
                        _phrase120 = _phrase120.Replace("{PlayertoRemove}", _playerName);
                        _phrase120 = _phrase120.Replace("{ClanName}", _clanname);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase120, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                        if (_PlayertoRemove != null)
                        {
                            if (!Phrases.Dict.TryGetValue(121, out _phrase121))
                            {
                                _phrase121 = "{PlayerName} you have been removed from the clan {ClanName}.";
                            }
                            _phrase121 = _phrase121.Replace("{PlayerName}", _playerName);
                            _phrase121 = _phrase121.Replace("{ClanName}", _clanname);
                            _PlayertoRemove.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase121, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                    }
                }
            }
        }

        public static void PromoteMember(ClientInfo _cInfo, string _playerName)
        {
            string _sql = string.Format("SELECT clanname, isclanowner FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            string _clanname = _result.Rows[0].ItemArray.GetValue(0).ToString();
            bool _isclanowner;
            bool.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _isclanowner);
            _result.Dispose();
            if (!_isclanowner)
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
                    _sql = string.Format("SELECT clanname, isclanofficer FROM Players WHERE steamid = '{0}'", _playertoPromote.playerId);
                    DataTable _result1 = SQL.TQuery(_sql);
                    string _clanname1 = _result1.Rows[0].ItemArray.GetValue(0).ToString();
                    bool _isclanofficer1;
                    bool.TryParse(_result1.Rows[0].ItemArray.GetValue(1).ToString(), out _isclanofficer1);
                    _result1.Dispose();
                    if (_clanname != _clanname1)
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
                        if (_isclanofficer1)
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
                            _sql = string.Format("UPDATE Players SET isclanofficer = 'true' WHERE steamid = '{0}'", _playertoPromote.playerId);
                            SQL.FastQuery(_sql);
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
            string _sql = string.Format("SELECT clanname, isclanowner FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            string _clanname = _result.Rows[0].ItemArray.GetValue(0).ToString();
            bool _isclanowner;
            bool.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _isclanowner);
            _result.Dispose();
            if (!_isclanowner)
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
                    _sql = string.Format("SELECT clanname, isclanofficer FROM Players WHERE steamid = '{0}'", _membertoDemote.playerId);
                    DataTable _result1 = SQL.TQuery(_sql);
                    string _clanname1 = _result1.Rows[0].ItemArray.GetValue(0).ToString();
                    bool _isclanofficer1;
                    bool.TryParse(_result1.Rows[0].ItemArray.GetValue(1).ToString(), out _isclanofficer1);
                    _result1.Dispose();
                    if (_clanname != _clanname1)
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
                        if (!_isclanofficer1)
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
                            _sql = string.Format("UPDATE Players SET isclanofficer = 'false' WHERE steamid = '{0}'", _membertoDemote.playerId);
                            SQL.FastQuery(_sql);
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
            string _sql = string.Format("SELECT clanname, isclanowner FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            string _clanname = _result.Rows[0].ItemArray.GetValue(0).ToString();
            bool _isclanowner;
            bool.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _isclanowner);
            _result.Dispose();
            if (_isclanowner)
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
                if (_clanname == "Unknown")
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
                    _phrase121 = _phrase121.Replace("{ClanName}", _clanname);
                    _sql = string.Format("UPDATE Players SET clanname = 'Unknown', isclanofficer = 'false' WHERE steamid = '{0}'", _cInfo.playerId);
                    SQL.FastQuery(_sql);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase121, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                }
            }
        }

        private static void RequestToJoinClan(ClientInfo _cInfo, string _clanName)
        {
        }

        public static string GetChatCommands(ClientInfo _cInfo)
        {
            string _sql = string.Format("SELECT clanname, invitedtoclan, isclanowner, isclanofficer FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            string _clanname = _result.Rows[0].ItemArray.GetValue(0).ToString();
            string _invitedtoclan = _result.Rows[0].ItemArray.GetValue(1).ToString();
            string _commands = ("Available clan commands are:");
            bool _isclanowner;
            if (bool.TryParse(_result.Rows[0].ItemArray.GetValue(2).ToString(), out _isclanowner))
            {
                bool _isclanofficer;
                if (bool.TryParse(_result.Rows[0].ItemArray.GetValue(3).ToString(), out _isclanofficer))
                {
                    _result.Dispose();
                    
                    if (!_isclanowner && !_isclanofficer && _clanname == "Unknown" && _invitedtoclan == "Unknown")
                    {
                        _commands = string.Format("{0} /clanadd ClanName", _commands);
                    }
                    if (_isclanowner)
                    {
                        _commands = string.Format("{0} /clanpromote", _commands);
                        _commands = string.Format("{0} /clandemote", _commands);
                        _commands = string.Format("{0} /clandel", _commands);
                        _commands = string.Format("{0} /clanrename ClanName", _commands);
                    }
                    if (_isclanowner || _isclanofficer)
                    {
                        _commands = string.Format("{0} /claninvite", _commands);
                        _commands = string.Format("{0} /clanremove", _commands);
                    }
                    if (_invitedtoclan != "Unknown")
                    {
                        _commands = string.Format("{0} /clanaccept", _commands);
                        _commands = string.Format("{0} /clandecline", _commands);
                    }
                    if (!_isclanowner && _clanname != "Unknown")
                    {
                        _commands = string.Format("{0} /clanleave", _commands);
                    }
                }
            }
            return _commands;
        }

        public static void Clan(ClientInfo _cInfo, string _message)
        {
            string _sql = string.Format("SELECT clanname FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
            DataTable _result = SQL.TQuery(_sql);
            string _clanname = _result.Rows[0].ItemArray.GetValue(0).ToString();
            _result.Dispose();
            _sql = string.Format("SELECT steamid FROM Players WHERE clanname = '{0}'", _clanname);
            DataTable _result1 = SQL.TQuery(_sql);
            foreach (DataRow row in _result1.Rows)
            {
                ClientInfo _cInfo1 = ConsoleHelper.ParseParamIdOrName(row[0].ToString());
                if (_cInfo1 != null)
                {
                    _cInfo1.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Private_Chat_Color, _message), _cInfo.playerName, false, "ServerTools", false));
                    if (ChatLog.IsEnabled)
                    {
                        ChatLog.Log(_message, _cInfo.playerName);
                    }
                }
            }
            _result1.Dispose();
        }
    }
}