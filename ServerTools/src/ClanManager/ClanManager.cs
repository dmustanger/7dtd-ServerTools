using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class ClanManager
    {
        public static bool IsEnabled = false;
        public static string Command33 = "clanadd", Command34 = "clandel", Command35 = "claninvite", Command36 = "clanaccept", Command37 = "clandecline", Command38 = "clanremove",
            Command39 = "clanpromote", Command40 = "clandemote", Command41 = "clanleave", Command42 = "clancommands", Command43 = "clanchat", Command44 = "clanrename", Command124 = "cc", Command125 = "clanlist";
        public static string Private_Chat_Color = "[00FF00]";
        public static List<string> ClanMember = new List<string>();
        public static Dictionary<string, string> Clans = new Dictionary<string, string>();

        public static string GetClanList()
        {
            string _clanList = ("Clan names are:");
            int _counter = 0;
            if (Clans.Count > 0)
            {
                foreach (KeyValuePair<string, string> i in Clans)
                {
                    _counter++;
                    if (_counter == Clans.Count)
                    {
                        _clanList = string.Format("{0} {1}", _clanList, i.Value);
                    }
                    else
                    {
                        _clanList = string.Format("{0} {1},", _clanList, i.Value);
                    }
                }
                return _clanList;
            }
            else
            {
                _clanList = ("No clans were found");
                return _clanList;
            }
        }

        public static void ClanList()
        {
            try
            {
                for (int i = 0; i < PersistentContainer.Instance.Players.SteamIDs.Count; i++)
                {
                    string _id = PersistentContainer.Instance.Players.SteamIDs[i];
                    PersistentPlayer p = PersistentContainer.Instance.Players[_id];
                    {
                        if (p != null && p.ClanName != null)
                        {
                            if (p.ClanName.Length > 0)
                            {
                                if (p.ClanOwner)
                                {
                                    Clans.Add(_id, p.ClanName);
                                }
                                ClanMember.Add(_id);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ClanManager build list: {0}.", e.Message));
            }
        }

        public static void AddClan(ClientInfo _cInfo, string _clanName)
        {
            if (_clanName.Length < 2 || _clanName.Length > 6)
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + "The clan name is too short or too long. It must be 2 - 6 characters." + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                bool _clanOwner = PersistentContainer.Instance.Players[_cInfo.playerId].ClanOwner;
                if (_clanOwner)
                {
                    string _clan = PersistentContainer.Instance.Players[_cInfo.playerId].ClanName;
                    string _phrase101;
                    if (!Phrases.Dict.TryGetValue(101, out _phrase101))
                    {
                        _phrase101 = " you have already created the clan {ClanName}.";
                    }
                    _phrase101 = _phrase101.Replace("{ClanName}", _clan.ToString());
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase101 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    string _clan = PersistentContainer.Instance.Players[_cInfo.playerId].ClanName;
                    if (_clan != null && _clan.Length > 0)
                    {
                        string _phrase103;
                        if (!Phrases.Dict.TryGetValue(103, out _phrase103))
                        {
                            _phrase103 = " you are currently a member of the clan {ClanName}.";
                        }
                        _phrase103 = _phrase103.Replace("{ClanName}", _clan.ToString());
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase103 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        if (Clans.ContainsValue(_clanName))
                        {
                            string _phrase102;
                            if (!Phrases.Dict.TryGetValue(102, out _phrase102))
                            {
                                _phrase102 = " can not add the clan {ClanName} because it already exist.";
                            }
                            _phrase102 = _phrase102.Replace("{ClanName}", _clanName);
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase102 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(_clanName) || _clanName.Length < 2 || _clanName.Length > 6)
                            {
                                string _phrase129;
                                if (!Phrases.Dict.TryGetValue(129, out _phrase129))
                                {
                                    _phrase129 = " the clanName must be 2 - 6 characters.";
                                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase129 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase129 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                ClanMember.Add(_cInfo.playerId);
                                Clans.Add(_cInfo.playerId, _clanName);
                                PersistentContainer.Instance.Players[_cInfo.playerId].ClanName = _clanName;
                                PersistentContainer.Instance.Players[_cInfo.playerId].ClanOwner = true;
                                PersistentContainer.Instance.Players[_cInfo.playerId].ClanOfficer = true;
                                PersistentContainer.Instance.Save();
                                string _phrase104;
                                if (!Phrases.Dict.TryGetValue(104, out _phrase104))
                                {
                                    _phrase104 = " you have added the clan {ClanName}.";
                                }
                                _phrase104 = _phrase104.Replace("{ClanName}", _clanName);
                                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase104 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                }
            }
        }

        public static void RemoveClan(ClientInfo _cInfo)
        {
            bool _clanOwner = PersistentContainer.Instance.Players[_cInfo.playerId].ClanOwner;
            if (!_clanOwner)
            {
                string _phrase105;
                if (!Phrases.Dict.TryGetValue(105, out _phrase105))
                {
                    _phrase105 = " you are not the owner of any clans.";
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase105 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                string _clanName = PersistentContainer.Instance.Players[_cInfo.playerId].ClanName;
                PersistentContainer.Instance.Players[_cInfo.playerId].ClanName = "";
                PersistentContainer.Instance.Players[_cInfo.playerId].ClanOwner = false;
                PersistentContainer.Instance.Players[_cInfo.playerId].ClanOfficer = false;
                for (int i = 0; i < PersistentContainer.Instance.Players.SteamIDs.Count; i++)
                {
                    string _id = PersistentContainer.Instance.Players.SteamIDs[i];
                    PersistentPlayer p = PersistentContainer.Instance.Players[_id];
                    {
                        if (p.ClanName != null && p.ClanName == _clanName)
                        {
                            PersistentContainer.Instance.Players[_id].ClanName = "";
                            PersistentContainer.Instance.Players[_id].ClanOfficer = false;
                            ClanMember.Remove(_id);
                            ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForPlayerId(_id);
                            if (_cInfo2 != null && _cInfo != _cInfo2)
                            {
                                string _phrase121;
                                if (!Phrases.Dict.TryGetValue(121, out _phrase121))
                                {
                                    _phrase121 = " you have been removed from the clan {ClanName}.";
                                }
                                _phrase121 = _phrase121.Replace("{ClanName}", _clanName);
                                ChatHook.ChatMessage(_cInfo2, LoadConfig.Chat_Response_Color + _cInfo2.playerName + LoadConfig.Chat_Response_Color + _phrase121 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        else if (p.ClanInvite == _clanName)
                        {
                            PersistentContainer.Instance.Players[_id].ClanInvite = "";
                        }
                    }
                }
                PersistentContainer.Instance.Save();
                ClanMember.Remove(_cInfo.playerId);
                Clans.Remove(_cInfo.playerId);
                string _phrase106;
                if (!Phrases.Dict.TryGetValue(106, out _phrase106))
                {
                    _phrase106 = " you have removed the clan {ClanName}.";
                }
                _phrase106 = _phrase106.Replace("{ClanName}", _clanName);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase106 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }

        public static void ClanRename(ClientInfo _cInfo, string _clanName)
        {
            if (_clanName.Length < 2 || _clanName.Length > 6)
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + "The clan name is too short or too long. It must be 2 - 6 characters." + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                bool _clanOwner = PersistentContainer.Instance.Players[_cInfo.playerId].ClanOwner;
                if (_clanOwner)
                {
                    if (!Clans.ContainsValue(_clanName))
                    {
                        string _oldClanName = PersistentContainer.Instance.Players[_cInfo.playerId].ClanName;
                        for (int i = 0; i < ClanMember.Count; i++)
                        {
                            string _clanMember = ClanMember[i];
                            if (PersistentContainer.Instance.Players[_clanMember].ClanName == _oldClanName)
                            {
                                PersistentContainer.Instance.Players[_clanMember].ClanName = _clanName;
                                PersistentContainer.Instance.Save();
                                ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForPlayerId(_clanMember);
                                if (_cInfo2 != null && _cInfo != _cInfo2)
                                {
                                    string _phrase131;
                                    if (!Phrases.Dict.TryGetValue(131, out _phrase131))
                                    {
                                        _phrase131 = " your clan name has been changed by the owner to {ClanName}.";
                                    }
                                    _phrase131 = _phrase131.Replace("{ClanName}", _clanName);
                                    ChatHook.ChatMessage(_cInfo2, LoadConfig.Chat_Response_Color + _cInfo2.playerName + LoadConfig.Chat_Response_Color + _phrase131 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                        }
                        PersistentContainer.Instance.Save();
                        Clans[_cInfo.playerId] = _clanName;
                        string _phrase130;
                        if (!Phrases.Dict.TryGetValue(130, out _phrase130))
                        {
                            _phrase130 = " you have changed your clan name to {ClanName}.";
                        }
                        _phrase130 = _phrase130.Replace("{ClanName}", _clanName);
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase130 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        string _phrase102;
                        if (!Phrases.Dict.TryGetValue(102, out _phrase102))
                        {
                            _phrase102 = " can not add the clan {ClanName} because it already exists.";
                        }
                        _phrase102 = _phrase102.Replace("{PlayerName}", _cInfo.playerName);
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase102 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
                else
                {
                    string _phrase105;
                    if (!Phrases.Dict.TryGetValue(105, out _phrase105))
                    {
                        _phrase105 = " you are not the owner of any clans.";
                    }
                    _phrase105 = _phrase105.Replace("{PlayerName}", _cInfo.playerName);
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase105 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }

        public static void InviteMember(ClientInfo _cInfo, string _playerName)
        {
            bool _clanOfficer = PersistentContainer.Instance.Players[_cInfo.playerId].ClanOfficer;
            if (!_clanOfficer)
            {
                string _phrase107;
                if (!Phrases.Dict.TryGetValue(107, out _phrase107))
                {
                    _phrase107 = " you do not have permissions to use this command.";
                }
                _phrase107 = _phrase107.Replace("{PlayerName}", _cInfo.playerName);
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase107 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                ClientInfo _newMember = ConnectionManager.Instance.Clients.GetForPlayerName(_playerName, true, false);
                if (_newMember == null)
                {
                    _newMember = ConsoleHelper.ParseParamEntityIdToClientInfo(_playerName);
                    if (_newMember == null)
                    {
                        string _phrase108;
                        if (!Phrases.Dict.TryGetValue(108, out _phrase108))
                        {
                            _phrase108 = " the player {PlayerName} was not found.";
                        }
                        _phrase108 = _phrase108.Replace("{Player}", _playerName);
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase108 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        return;
                    }
                }
                else
                {
                    string _newMemberClanName = PersistentContainer.Instance.Players[_newMember.playerId].ClanName;
                    if (_newMemberClanName != null && _newMemberClanName.Length > 0)
                    {
                        string _phrase109;
                        if (!Phrases.Dict.TryGetValue(109, out _phrase109))
                        {
                            _phrase109 = "{PlayerName} is already a member of a clan named {ClanName}.";
                        }
                        _phrase109 = _phrase109.Replace("{PlayerName}", _newMember.playerName);
                        _phrase109 = _phrase109.Replace("{ClanName}", _newMemberClanName);
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase109 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        string _clanInvite = PersistentContainer.Instance.Players[_newMember.playerId].ClanInvite;
                        if (_clanInvite != null && _clanInvite.Length > 0)
                        {
                            string _phrase110;
                            if (!Phrases.Dict.TryGetValue(110, out _phrase110))
                            {
                                _phrase110 = "{PlayerName} already has a clan invitation.";
                            }
                            _phrase110 = _phrase110.Replace("{PlayerName}", _newMember.playerName);
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase110 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            string _clanName = PersistentContainer.Instance.Players[_cInfo.playerId].ClanName;
                            string _phrase111;
                            if (!Phrases.Dict.TryGetValue(111, out _phrase111))
                            {
                                _phrase111 = " you have been invited to join the clan {ClanName}. Type {CommandPrivate}{Command36} to join or {CommandPrivate}{Command37} to decline the offer.";
                            }
                            _phrase111 = _phrase111.Replace("{ClanName}", _clanName);
                            _phrase111 = _phrase111.Replace("{CommandPrivate}", ChatHook.Command_Private);
                            _phrase111 = _phrase111.Replace("{Command36}", Command36);
                            _phrase111 = _phrase111.Replace("{Command37}", Command37);
                            string _phrase112;
                            if (!Phrases.Dict.TryGetValue(112, out _phrase112))
                            {
                                _phrase112 = " you have invited {PlayerName} to the clan {ClanName}.";
                            }
                            _phrase112 = _phrase112.Replace("{PlayerName}", _newMember.playerName);
                            _phrase112 = _phrase112.Replace("{ClanName}", _clanName);
                            PersistentContainer.Instance.Players[_cInfo.playerId].ClanInvite = _clanName;
                            PersistentContainer.Instance.Save();
                            ChatHook.ChatMessage(_newMember, LoadConfig.Chat_Response_Color + _newMember.playerName + LoadConfig.Chat_Response_Color + _phrase111 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase112 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
            }
        }

        public static void InviteAccept(ClientInfo _cInfo)
        {
            string _clanInvite = PersistentContainer.Instance.Players[_cInfo.playerId].ClanInvite;
            if (_clanInvite != null && _clanInvite.Length == 0)
            {
                string _phrase113;
                if (!Phrases.Dict.TryGetValue(113, out _phrase113))
                {
                    _phrase113 = " you have not been invited to any clans.";
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase113 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                ClanMember.Add(_cInfo.playerId);
                PersistentContainer.Instance.Players[_cInfo.playerId].ClanInvite = "";
                PersistentContainer.Instance.Players[_cInfo.playerId].ClanName = _clanInvite;
                PersistentContainer.Instance.Save();
                for (int i = 0; i < ClanMember.Count; i++)
                {
                    string _clanMember = ClanMember[i];
                    if (PersistentContainer.Instance.Players[_clanMember].ClanName == _clanInvite)
                    {
                        ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForPlayerId(_clanMember);
                        if (_cInfo2 != null)
                        {
                            string _phrase115;
                            if (!Phrases.Dict.TryGetValue(115, out _phrase115))
                            {
                                _phrase115 = "{PlayerName} has joined the clan.";
                            }
                            _phrase115 = _phrase115.Replace("{PlayerName}", _cInfo.playerName);
                            ChatHook.ChatMessage(_cInfo2, LoadConfig.Chat_Response_Color + _phrase115 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
            }
        }

        public static void InviteDecline(ClientInfo _cInfo)
        {
            string _clanInvite = PersistentContainer.Instance.Players[_cInfo.playerId].ClanInvite;
            if (_clanInvite != null && _clanInvite.Length == 0)
            {
                string _phrase113;
                if (!Phrases.Dict.TryGetValue(113, out _phrase113))
                {
                    _phrase113 = " you have not been invited to any clans.";
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + _phrase113 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                PersistentContainer.Instance.Players[_cInfo.playerId].ClanInvite = "";
                PersistentContainer.Instance.Save();
                string _phrase116;
                if (!Phrases.Dict.TryGetValue(116, out _phrase116))
                {
                    _phrase116 = " you have declined the invite to the clan.";
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + _phrase116 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                for (int i = 0; i < ClanMember.Count; i++)
                {
                    string _clanMember = ClanMember[i];
                    if (PersistentContainer.Instance.Players[_clanMember].ClanName == _clanInvite)
                    {
                        ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForPlayerId(_clanMember);
                        if (_cInfo2 != null)
                        {
                            string _phrase115;
                            if (!Phrases.Dict.TryGetValue(115, out _phrase115))
                            {
                                _phrase115 = "{PlayerName} has declined the invite to the clan.";
                            }
                            _phrase115 = _phrase115.Replace("{PlayerName}", _cInfo.playerName);
                            ChatHook.ChatMessage(_cInfo2, LoadConfig.Chat_Response_Color + _phrase115 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
            }
        }

        public static void RemoveMember(ClientInfo _cInfo, string _playerName)
        {
            bool _clanOfficer = PersistentContainer.Instance.Players[_cInfo.playerId].ClanOfficer;
            if (!_clanOfficer)
            {
                string _phrase107;
                if (!Phrases.Dict.TryGetValue(107, out _phrase107))
                {
                    _phrase107 = " you do not have permissions to use this command.";
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + _phrase107 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                ClientInfo _PlayertoRemove = ConnectionManager.Instance.Clients.GetForPlayerName(_playerName, true, false);
                if (_PlayertoRemove == null)
                {
                    _PlayertoRemove = ConsoleHelper.ParseParamEntityIdToClientInfo(_playerName);
                    if (_PlayertoRemove == null)
                    {
                        string _phrase108;
                        if (!Phrases.Dict.TryGetValue(108, out _phrase108))
                        {
                            _phrase108 = " the player {PlayerName} was not found.";
                        }
                        _phrase108 = _phrase108.Replace("{Player}", _playerName);
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + _phrase108 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        return;
                    }
                }
                string _clanName = PersistentContainer.Instance.Players[_cInfo.playerId].ClanName;
                string _clanName2 = PersistentContainer.Instance.Players[_PlayertoRemove.playerId].ClanName;
                if (_clanName2 != null && _clanName != _clanName2)
                {
                    string _phrase117;
                    if (!Phrases.Dict.TryGetValue(117, out _phrase117))
                    {
                        _phrase117 = "{PlayerName} is not a member of your clan.";
                    }
                    _phrase117 = _phrase117.Replace("{PlayerName}", _playerName);
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase117 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    bool _clanOfficer2 = PersistentContainer.Instance.Players[_PlayertoRemove.playerId].ClanOfficer;
                    bool _clanOwner = PersistentContainer.Instance.Players[_cInfo.playerId].ClanOwner;
                    if (_clanOfficer2 && !_clanOwner)
                    {
                        string _phrase118;
                        if (!Phrases.Dict.TryGetValue(118, out _phrase118))
                        {
                            _phrase118 = " only the clan owner can remove officers.";
                        }
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + _phrase118 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        ClanMember.Remove(_PlayertoRemove.playerId);
                        PersistentContainer.Instance.Players[_PlayertoRemove.playerId].ClanName = "";
                        PersistentContainer.Instance.Players[_PlayertoRemove.playerId].ClanOfficer = false;
                        PersistentContainer.Instance.Save();
                        string _phrase120;
                        if (!Phrases.Dict.TryGetValue(120, out _phrase120))
                        {
                            _phrase120 = " you have removed {PlayerName} from clan {ClanName}.";
                        }
                        _phrase120 = _phrase120.Replace("{PlayerName}", _PlayertoRemove.playerName);
                        _phrase120 = _phrase120.Replace("{ClanName}", _clanName);
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + _phrase120 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        if (_PlayertoRemove != null)
                        {
                            string _phrase121;
                            if (!Phrases.Dict.TryGetValue(121, out _phrase121))
                            {
                                _phrase121 = " you have been removed from the clan {ClanName}.";
                            }
                            _phrase121 = _phrase121.Replace("{ClanName}", _clanName);
                            ChatHook.ChatMessage(_PlayertoRemove, ChatHook.Player_Name_Color + _PlayertoRemove.playerName + _phrase121 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                        for (int i = 0; i < ClanMember.Count; i++)
                        {
                            string _clanMember = ClanMember[i];
                            if (PersistentContainer.Instance.Players[_clanMember].ClanName == _clanName)
                            {
                                ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForPlayerId(_clanMember);
                                if (_cInfo2 != null && _cInfo != _cInfo2)
                                {
                                    string _phrase132;
                                    if (!Phrases.Dict.TryGetValue(115, out _phrase132))
                                    {
                                        _phrase132 = "Player {PlayerName} has been removed from the clan.";
                                    }
                                    _phrase132 = _phrase132.Replace("{PlayerName}", _PlayertoRemove.playerName);
                                    ChatHook.ChatMessage(_cInfo2, LoadConfig.Chat_Response_Color + _phrase132 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void PromoteMember(ClientInfo _cInfo, string _playerName)
        {
            bool _clanOfficer = PersistentContainer.Instance.Players[_cInfo.playerId].ClanOfficer;
            if (!_clanOfficer)
            {
                string _phrase107;
                if (!Phrases.Dict.TryGetValue(107, out _phrase107))
                {
                    _phrase107 = " you do not have permissions to use this command.";
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + _phrase107 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                ClientInfo _playertoPromote = ConnectionManager.Instance.Clients.GetForPlayerName(_playerName, true, false);
                if (_playertoPromote == null)
                {
                    _playertoPromote = ConsoleHelper.ParseParamEntityIdToClientInfo(_playerName);
                    string _phrase108;
                    if (!Phrases.Dict.TryGetValue(108, out _phrase108))
                    {
                        _phrase108 = " the player {PlayerName} was not found.";
                    }
                    _phrase108 = _phrase108.Replace("{PlayerName}", _playerName);
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + _phrase108 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    string _clanName = PersistentContainer.Instance.Players[_cInfo.playerId].ClanName;
                    string _clanName2 = PersistentContainer.Instance.Players[_playertoPromote.playerId].ClanName;
                    if (_clanName2 == null || _clanName != _clanName2)
                    {
                        string _phrase117;
                        if (!Phrases.Dict.TryGetValue(117, out _phrase117))
                        {
                            _phrase117 = "{PlayerName} is not a member of your clan.";
                        }
                        _phrase117 = _phrase117.Replace("{PlayerName}", _playerName);
                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase117 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        bool _clanOfficer2 = PersistentContainer.Instance.Players[_playertoPromote.playerId].ClanOfficer;
                        if (_clanOfficer2)
                        {
                            string _phrase122;
                            if (!Phrases.Dict.TryGetValue(122, out _phrase122))
                            {
                                _phrase122 = "{PlayerName} is already a officer.";
                            }
                            _phrase122 = _phrase122.Replace("{PlayerName}", _playerName);
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase122 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            PersistentContainer.Instance.Players[_playertoPromote.playerId].ClanOfficer = true;
                            PersistentContainer.Instance.Save();
                            string _phrase123;
                            if (!Phrases.Dict.TryGetValue(123, out _phrase123))
                            {
                                _phrase123 = "{PlayerName} has been promoted to an officer.";
                            }
                            _phrase123 = _phrase123.Replace("{PlayerName}", _playerName);
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase123 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
            }
        }

        public static void DemoteMember(ClientInfo _cInfo, string _playerName)
        {
            bool _clanOwner = PersistentContainer.Instance.Players[_cInfo.playerId].ClanOwner;
            if (!_clanOwner)
            {
                string _phrase107;
                if (!Phrases.Dict.TryGetValue(107, out _phrase107))
                {
                    _phrase107 = " you do not have permissions to use this command.";
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + _phrase107 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                ClientInfo _membertoDemote = ConsoleHelper.ParseParamIdOrName(_playerName);
                if (_membertoDemote == null)
                {
                    _membertoDemote = ConsoleHelper.ParseParamEntityIdToClientInfo(_playerName);
                    string _phrase108;
                    if (!Phrases.Dict.TryGetValue(108, out _phrase108))
                    {
                        _phrase108 = " the player {PlayerName} was not found.";
                    }
                    _phrase108 = _phrase108.Replace("{PlayerName}", _playerName);
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + _phrase108 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    string _clanName = PersistentContainer.Instance.Players[_cInfo.playerId].ClanName;
                    string _clanName2 = PersistentContainer.Instance.Players[_membertoDemote.playerId].ClanName;
                    if (_clanName2 == null || _clanName != _clanName2)
                    {
                        string _phrase117;
                        if (!Phrases.Dict.TryGetValue(117, out _phrase117))
                        {
                            _phrase117 = "{PlayerName} is not a member of your clan.";
                        }
                        _phrase117 = _phrase117.Replace("{PlayerName}", _playerName);
                        ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + _phrase117 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        bool _clanOfficer2 = PersistentContainer.Instance.Players[_membertoDemote.playerId].ClanOfficer;
                        if (!_clanOfficer2)
                        {
                            string _phrase124;
                            if (!Phrases.Dict.TryGetValue(124, out _phrase124))
                            {
                                _phrase124 = "{PlayerName} is not an officer.";
                            }
                            _phrase124 = _phrase124.Replace("{PlayerName}", _playerName);
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + _phrase124 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            PersistentContainer.Instance.Players[_membertoDemote.playerId].ClanOfficer = false;
                            PersistentContainer.Instance.Save();
                            string _phrase125;
                            if (!Phrases.Dict.TryGetValue(125, out _phrase125))
                            {
                                _phrase125 = "{PlayerName} has been demoted.";
                            }
                            _phrase125 = _phrase125.Replace("{PlayerName}", _playerName);
                            ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + _phrase125 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                }
            }
        }

        public static void LeaveClan(ClientInfo _cInfo)
        {
            bool _clanOwner = PersistentContainer.Instance.Players[_cInfo.playerId].ClanOwner;
            if (_clanOwner)
            {
                string _phrase126;
                if (!Phrases.Dict.TryGetValue(126, out _phrase126))
                {
                    _phrase126 = " you can not leave the clan because you are the owner. You can only delete the clan.";
                }
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + _phrase126 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            {
                string _clanName = PersistentContainer.Instance.Players[_cInfo.playerId].ClanName;
                if (_clanName == null || _clanName == "")
                {
                    string _phrase127;
                    if (!Phrases.Dict.TryGetValue(127, out _phrase127))
                    {
                        _phrase127 = " you do not belong to any clans.";
                    }
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + _phrase127 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    ClanMember.Remove(_cInfo.playerId);
                    PersistentContainer.Instance.Players[_cInfo.playerId].ClanName = "";
                    PersistentContainer.Instance.Save();
                    string _phrase121;
                    if (!Phrases.Dict.TryGetValue(121, out _phrase121))
                    {
                        _phrase121 = " you have been removed from the clan {ClanName}.";
                    }
                    _phrase121 = _phrase121.Replace("{ClanName}", _clanName);
                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + _phrase121 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                    for (int i = 0; i < ClanMember.Count; i++)
                    {
                        string _clanMember = ClanMember[i];
                        if (PersistentContainer.Instance.Players[_clanMember].ClanName == _clanName)
                        {
                            ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForPlayerId(_clanMember);
                            if (_cInfo2 != null)
                            {
                                string _phrase115;
                                if (!Phrases.Dict.TryGetValue(115, out _phrase115))
                                {
                                    _phrase115 = "Player {PlayerName} has left the clan.";
                                }
                                _phrase115 = _phrase115.Replace("{PlayerName}", _cInfo.playerName);
                                ChatHook.ChatMessage(_cInfo2, LoadConfig.Chat_Response_Color + _phrase115 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                }
            }
        }

        private static void RequestToJoinClan(ClientInfo _cInfo, string _clanName)
        {

        }

        public static string GetChatCommands(ClientInfo _cInfo)
        {
            string _clanName = PersistentContainer.Instance.Players[_cInfo.playerId].ClanName;
            string _clanInvite = PersistentContainer.Instance.Players[_cInfo.playerId].ClanInvite;
            bool _clanOwner = PersistentContainer.Instance.Players[_cInfo.playerId].ClanOwner;
            bool _clanOfficer = PersistentContainer.Instance.Players[_cInfo.playerId].ClanOfficer;
            string _commands = ("Available clan commands are:");
            if (!_clanOwner && !_clanOfficer && _clanName == "" && _clanInvite == "")
            {
                _commands = string.Format("{0} {1}{2} ClanName", _commands, ChatHook.Command_Private, ClanManager.Command33);
            }
            if (_clanOwner)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, ClanManager.Command39);
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, ClanManager.Command40);
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, ClanManager.Command34);
                _commands = string.Format("{0} {1}{2} ClanName", _commands, ChatHook.Command_Private, ClanManager.Command44);
            }
            if (_clanOwner || _clanOfficer)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, ClanManager.Command35);
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, ClanManager.Command38);
            }
            if (_clanInvite != "")
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, ClanManager.Command36);
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, ClanManager.Command37);
            }
            if (!_clanOwner && _clanName != "")
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Command_Private, ClanManager.Command41);
            }
            if (ClanMember.Contains(_cInfo.playerId))
            {
                _commands = string.Format("{0} {1}{2} or {3}{4}", _commands, ChatHook.Command_Private, ClanManager.Command43, ChatHook.Command_Private, ClanManager.Command124);
            }
            return _commands;
        }

        public static void Clan(ClientInfo _cInfo, string _message)
        {
            string _clanName = PersistentContainer.Instance.Players[_cInfo.playerId].ClanName;
            if (_clanName != "")
            {
                foreach (KeyValuePair<string, string> _clan in Clans)
                {
                    if (_clan.Value == _clanName)
                    {
                        ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForPlayerId(_clan.Key);
                        if (_cInfo2 != null)
                        {
                            string _senderName = string.Format("{0}(Clan) {1}[-]", Private_Chat_Color, _cInfo.playerName);
                            ChatHook.ChatMessage(_cInfo2, _message, _cInfo.entityId, _senderName, EChatType.Whisper, null);
                        }
                    }
                }
                if (ChatLog.IsEnabled)
                {
                    ChatLog.Log(_message, _cInfo.playerName);
                }
            }
            else
            {
                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + " you are not in a clan" + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            }
        }
    }
}