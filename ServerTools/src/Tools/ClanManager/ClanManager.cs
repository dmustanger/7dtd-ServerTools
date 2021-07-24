using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerTools
{
    public class ClanManager
    {
        public static bool IsEnabled = false;
        public static string Command_add = "clan add", Command_delete = "clan del", Command_invite = "clan invite", Command_accept = "clan accept", 
            Command_decline = "clan decline", Command_remove = "clan remove", Command_promote = "clan promote", Command_demote = "clan demote",
            Command_leave = "clan leave", Command_commands = "clan commands", Command_chat = "clan chat", Command_rename = "clan rename", Command_request = "clan request", 
            Command_cc = "cc", Command_clan_list = "clan list";
        public static int Max_Name_Length = 6;
        public static string Private_Chat_Color = "[00FF00]";
        public static List<string> ClanMember = new List<string>();
        public static Dictionary<string, string> Clans = new Dictionary<string, string>();

        public static string GetClanList()
        {
            try
            {
                Phrases.Dict.TryGetValue(104, out string _phrase104);
                string _clanList = string.Format("{0}", _phrase104);
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
                    Phrases.Dict.TryGetValue(105, out string _phrase105);
                    _clanList = string.Format("{0}", _phrase105);
                    return _clanList;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ClanManager.GetClanList: {0}", e.Message));
                return null;
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
                                PersistentContainer.DataChange = true;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ClanManager.ClanList: {0}", e.Message));
            }
        }

        public static void AddClan(ClientInfo _cInfo, string _clanName)
        {
            try
            {
                if (_clanName.Length < 2 || _clanName.Length > Max_Name_Length)
                {
                    Phrases.Dict.TryGetValue(106, out string _phrase106);
                    _phrase106 = _phrase106.Replace("{MaxLength}", Max_Name_Length.ToString());
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase106 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    bool _clanOwner = PersistentContainer.Instance.Players[_cInfo.playerId].ClanOwner;
                    if (_clanOwner)
                    {
                        string _clan = PersistentContainer.Instance.Players[_cInfo.playerId].ClanName;
                        Phrases.Dict.TryGetValue(71, out string _phrase71);
                        _phrase71 = _phrase71.Replace("{ClanName}", _clan.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase71 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        string _clan = PersistentContainer.Instance.Players[_cInfo.playerId].ClanName;
                        if (!string.IsNullOrEmpty(_clan))
                        {
                            Phrases.Dict.TryGetValue(73, out string _phrase73);
                            _phrase73 = _phrase73.Replace("{ClanName}", _clan.ToString());
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase73 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            if (Clans.ContainsValue(_clanName))
                            {
                                Phrases.Dict.TryGetValue(72, out string _phrase72);
                                _phrase72 = _phrase72.Replace("{ClanName}", _clan.ToString());
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase72 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                ClanMember.Add(_cInfo.playerId);
                                Clans.Add(_cInfo.playerId, _clanName);
                                PersistentContainer.Instance.Players[_cInfo.playerId].ClanName = _clanName;
                                PersistentContainer.Instance.Players[_cInfo.playerId].ClanOwner = true;
                                PersistentContainer.Instance.Players[_cInfo.playerId].ClanOfficer = true;
                                PersistentContainer.DataChange = true;
                                Phrases.Dict.TryGetValue(74, out string _phrase74);
                                _phrase74 = _phrase74.Replace("{ClanName}", _clan.ToString());
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase74 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ClanManager.AddClan: {0}", e.Message));
            }
        }

        public static void RemoveClan(ClientInfo _cInfo)
        {
            try
            {
                bool _clanOwner = PersistentContainer.Instance.Players[_cInfo.playerId].ClanOwner;
                if (!_clanOwner)
                {
                    Phrases.Dict.TryGetValue(75, out string _phrase75);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase75 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    string _clanName = PersistentContainer.Instance.Players[_cInfo.playerId].ClanName;
                    PersistentContainer.Instance.Players[_cInfo.playerId].ClanName = "";
                    PersistentContainer.Instance.Players[_cInfo.playerId].ClanOwner = false;
                    PersistentContainer.Instance.Players[_cInfo.playerId].ClanOfficer = false;
                    PersistentContainer.Instance.Players[_cInfo.playerId].ClanRequestToJoin = null;
                    for (int i = 0; i < PersistentContainer.Instance.Players.SteamIDs.Count; i++)
                    {
                        string _id = PersistentContainer.Instance.Players.SteamIDs[i];
                        PersistentPlayer p = PersistentContainer.Instance.Players[_id];
                        {
                            if (p.ClanName != null && p.ClanName == _clanName)
                            {
                                p.ClanName = "";
                                p.ClanOfficer = false;
                                ClanMember.Remove(_id);
                                ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForPlayerId(_id);
                                if (_cInfo2 != null && _cInfo != _cInfo2)
                                {
                                    Phrases.Dict.TryGetValue(91, out string _phrase91);
                                    _phrase91 = _phrase91.Replace("{ClanName}", _clanName);
                                    ChatHook.ChatMessage(_cInfo2, Config.Chat_Response_Color + _phrase91 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else if (p.ClanInvite != null && p.ClanInvite == _clanName)
                            {
                                PersistentContainer.Instance.Players[_id].ClanInvite = "";
                            }
                        }
                    }
                    PersistentContainer.DataChange = true;
                    ClanMember.Remove(_cInfo.playerId);
                    Clans.Remove(_cInfo.playerId);
                    Phrases.Dict.TryGetValue(76, out string _phrase76);
                    _phrase76 = _phrase76.Replace("{ClanName}", _clanName);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase76 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ClanManager.RemoveClan: {0}", e.Message));
            }
        }

        public static void ClanRename(ClientInfo _cInfo, string _clanName)
        {
            try
            {
                if (_clanName.Length < 2 || _clanName.Length > Max_Name_Length)
                {
                    Phrases.Dict.TryGetValue(106, out string _phrase106);
                    _phrase106 = _phrase106.Replace("{MaxLength}", Max_Name_Length.ToString());
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase106 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                                    ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForPlayerId(_clanMember);
                                    if (_cInfo2 != null && _cInfo != _cInfo2)
                                    {
                                        Phrases.Dict.TryGetValue(101, out string _phrase101);
                                        _phrase101 = _phrase101.Replace("{ClanName}", _clanName);
                                        ChatHook.ChatMessage(_cInfo2, Config.Chat_Response_Color + _phrase101 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                            }
                            PersistentContainer.DataChange = true;
                            Clans[_cInfo.playerId] = _clanName;
                            Phrases.Dict.TryGetValue(100, out string _phrase100);
                            _phrase100 = _phrase100.Replace("{ClanName}", _clanName);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase100 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue(72, out string _phrase72);
                            _phrase72 = _phrase72.Replace("{ClanName}", _clanName);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase72 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue(75, out string _phrase75);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase75 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ClanManager.ClanRename: {0}", e.Message));
            }
        }

        public static void InviteMember(ClientInfo _cInfo, string _playerName)
        {
            try
            {
                bool _clanOfficer = PersistentContainer.Instance.Players[_cInfo.playerId].ClanOfficer;
                if (!_clanOfficer)
                {
                    Phrases.Dict.TryGetValue(77, out string _phrase77);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase77 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    ClientInfo _newMember = ConsoleHelper.ParseParamIdOrName(_playerName);
                    if (_newMember == null)
                    {
                        Phrases.Dict.TryGetValue(78, out string _phrase78);
                        _phrase78 = _phrase78.Replace("{PlayerName}", _playerName);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase78 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        return;
                    }
                    else
                    {
                        string _newMemberClanName = PersistentContainer.Instance.Players[_newMember.playerId].ClanName;
                        if (_newMemberClanName != null && _newMemberClanName.Length > 0)
                        {
                            Phrases.Dict.TryGetValue(79, out string _phrase79);
                            _phrase79 = _phrase79.Replace("{PlayerName}", _newMember.playerName);
                            _phrase79 = _phrase79.Replace("{ClanName}", _newMemberClanName);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase79 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            string _clanInvite = PersistentContainer.Instance.Players[_newMember.playerId].ClanInvite;
                            if (_clanInvite != null && _clanInvite.Length > 0)
                            {
                                Phrases.Dict.TryGetValue(80, out string _phrase80);
                                _phrase80 = _phrase80.Replace("{PlayerName}", _newMember.playerName);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase80 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                string _clanName = PersistentContainer.Instance.Players[_cInfo.playerId].ClanName;
                                PersistentContainer.Instance.Players[_newMember.playerId].ClanInvite = _clanName;
                                PersistentContainer.DataChange = true;
                                Phrases.Dict.TryGetValue(81, out string _phrase81);
                                _phrase81 = _phrase81.Replace("{ClanName}", _clanName);
                                _phrase81 = _phrase81.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                                _phrase81 = _phrase81.Replace("{Command_accept}", Command_accept);
                                _phrase81 = _phrase81.Replace("{Command_decline}", Command_decline);
                                ChatHook.ChatMessage(_newMember, Config.Chat_Response_Color + _phrase81 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                Phrases.Dict.TryGetValue(82, out string _phrase82);
                                _phrase82 = _phrase82.Replace("{PlayerName}", _newMember.playerName);
                                _phrase82 = _phrase82.Replace("{ClanName}", _clanName);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase82 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ClanManager.InviteMember: {0}", e.Message));
            }
        }

        public static void InviteAccept(ClientInfo _cInfo)
        {
            try
            {
                bool _clanOwner = PersistentContainer.Instance.Players[_cInfo.playerId].ClanOwner;
                if (!_clanOwner)
                {
                    string _clanInvite = PersistentContainer.Instance.Players[_cInfo.playerId].ClanInvite;
                    if (string.IsNullOrEmpty(_clanInvite))
                    {
                        Phrases.Dict.TryGetValue(83, out string _phrase83);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase83 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        ClanMember.Add(_cInfo.playerId);
                        PersistentContainer.Instance.Players[_cInfo.playerId].ClanInvite = "";
                        PersistentContainer.Instance.Players[_cInfo.playerId].ClanName = _clanInvite;
                        PersistentContainer.DataChange = true;
                        for (int i = 0; i < ClanMember.Count; i++)
                        {
                            string _clanMember = ClanMember[i];
                            string _clanName = PersistentContainer.Instance.Players[_clanMember].ClanName;
                            if (!string.IsNullOrEmpty(_clanName) && _clanName == _clanInvite)
                            {
                                ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForPlayerId(_clanMember);
                                if (_cInfo2 != null)
                                {
                                    Phrases.Dict.TryGetValue(99, out string _phrase99);
                                    _phrase99 = _phrase99.Replace("{PlayerName}", _cInfo.playerName);
                                    ChatHook.ChatMessage(_cInfo2, Config.Chat_Response_Color + _phrase99 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Dictionary<string, string> _clanRequests = PersistentContainer.Instance.Players[_cInfo.playerId].ClanRequestToJoin;
                    if (_clanRequests != null && _clanRequests.Count > 0)
                    {
                        KeyValuePair<string, string> _request = _clanRequests.First();
                        _clanRequests.Remove(_request.Key);
                        string _clanName = PersistentContainer.Instance.Players[_cInfo.playerId].ClanName;
                        string _clanName1 = PersistentContainer.Instance.Players[_request.Key].ClanName;
                        if (!string.IsNullOrEmpty(_clanName1))
                        {
                            PersistentContainer.Instance.Players[_cInfo.playerId].ClanRequestToJoin = _clanRequests;
                            Phrases.Dict.TryGetValue(107, out string _phrase107);
                            _phrase107 = _phrase107.Replace("{PlayerName}", _request.Value);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase107 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return;
                        }
                        ClanMember.Add(_request.Key);
                        PersistentContainer.Instance.Players[_request.Key].ClanInvite = "";
                        PersistentContainer.Instance.Players[_request.Key].ClanName = _clanName;
                        PersistentContainer.Instance.Players[_cInfo.playerId].ClanRequestToJoin = _clanRequests;
                        PersistentContainer.DataChange = true;
                        for (int i = 0; i < ClanMember.Count; i++)
                        {
                            string _clanMember = ClanMember[i];
                            string _clanName2 = PersistentContainer.Instance.Players[_clanMember].ClanName;
                            if (!string.IsNullOrEmpty(_clanName2) && _clanName2 == _clanName)
                            {
                                ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForPlayerId(_clanMember);
                                if (_cInfo2 != null)
                                {
                                    Phrases.Dict.TryGetValue(99, out string _phrase99);
                                    _phrase99 = _phrase99.Replace("{PlayerName}", _cInfo.playerName);
                                    ChatHook.ChatMessage(_cInfo2, Config.Chat_Response_Color + _phrase99 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                        }
                        if (_clanRequests.Count > 0)
                        {
                            _request = _clanRequests.First();
                            Phrases.Dict.TryGetValue(108, out string _phrase108);
                            _phrase108 = _phrase108.Replace("{PlayerName}", _request.Value);
                            _phrase108 = _phrase108.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                            _phrase108 = _phrase108.Replace("{Command_accept}", Command_accept);
                            _phrase108 = _phrase108.Replace("{Command_decline}", Command_decline);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase108 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue(109, out string _phrase109);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase109 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ClanManager.InviteAccept: {0}", e.Message));
            }
        }

        public static void InviteDecline(ClientInfo _cInfo)
        {
            try
            {
                bool _clanOwner = PersistentContainer.Instance.Players[_cInfo.playerId].ClanOwner;
                if (!_clanOwner)
                {
                    string _clanInvite = PersistentContainer.Instance.Players[_cInfo.playerId].ClanInvite;
                    if (string.IsNullOrEmpty(_clanInvite))
                    {
                        Phrases.Dict.TryGetValue(83, out string _phrase83);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase83 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId].ClanInvite = "";
                        PersistentContainer.DataChange = true;
                        Phrases.Dict.TryGetValue(86, out string _phrase86);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase86 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        for (int i = 0; i < ClanMember.Count; i++)
                        {
                            string _clanMember = ClanMember[i];
                            string _clanName = PersistentContainer.Instance.Players[_clanMember].ClanName;
                            if (string.IsNullOrEmpty(_clanName) && _clanName == _clanInvite)
                            {
                                ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForPlayerId(_clanMember);
                                if (_cInfo2 != null)
                                {
                                    Phrases.Dict.TryGetValue(85, out string _phrase85);
                                    _phrase85 = _phrase85.Replace("{PlayerName}", _cInfo.playerName);
                                    ChatHook.ChatMessage(_cInfo2, Config.Chat_Response_Color + _phrase85 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Dictionary<string, string> _clanRequests = PersistentContainer.Instance.Players[_cInfo.playerId].ClanRequestToJoin;
                    if (_clanRequests != null && _clanRequests.Count > 0)
                    {
                        KeyValuePair<string, string> _request = _clanRequests.First();
                        _clanRequests.Remove(_request.Key);
                        PersistentContainer.Instance.Players[_cInfo.playerId].ClanRequestToJoin = _clanRequests;
                        PersistentContainer.DataChange = true;
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + "Removed the request to join the group by player " + _request.Value + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        if (_clanRequests.Count > 0)
                        {
                            _request = _clanRequests.First();
                            Phrases.Dict.TryGetValue(108, out string _phrase108);
                            _phrase108 = _phrase108.Replace("{PlayerName}", _request.Value);
                            _phrase108 = _phrase108.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                            _phrase108 = _phrase108.Replace("{Command_accept}", Command_accept);
                            _phrase108 = _phrase108.Replace("{Command_decline}", Command_decline);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase108 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue(109, out string _phrase109);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase109 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ClanManager.InviteDecline: {0}", e.Message));
            }
        }

        public static void RemoveMember(ClientInfo _cInfo, string _playerName)
        {
            try
            {
                bool _clanOfficer = PersistentContainer.Instance.Players[_cInfo.playerId].ClanOfficer;
                if (!_clanOfficer)
                {
                    Phrases.Dict.TryGetValue(77, out string _phrase77);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase77 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    ClientInfo _PlayertoRemove = ConsoleHelper.ParseParamIdOrName(_playerName);
                    if (_PlayertoRemove == null)
                    {
                        Phrases.Dict.TryGetValue(78, out string _phrase78);
                        _phrase78 = _phrase78.Replace("{PlayerName}", _playerName);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase78 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        return;
                    }
                    else
                    {
                        string _clanName = PersistentContainer.Instance.Players[_cInfo.playerId].ClanName;
                        string _clanName2 = PersistentContainer.Instance.Players[_PlayertoRemove.playerId].ClanName;
                        if (_clanName2 == null || _clanName != _clanName2)
                        {
                            Phrases.Dict.TryGetValue(87, out string _phrase87);
                            _phrase87 = _phrase87.Replace("{PlayerName}", _playerName);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase87 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            bool _clanOfficer2 = PersistentContainer.Instance.Players[_PlayertoRemove.playerId].ClanOfficer;
                            bool _clanOwner = PersistentContainer.Instance.Players[_cInfo.playerId].ClanOwner;
                            if (_clanOfficer2 && !_clanOwner)
                            {
                                Phrases.Dict.TryGetValue(88, out string _phrase88);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase88 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                ClanMember.Remove(_PlayertoRemove.playerId);
                                PersistentContainer.Instance.Players[_PlayertoRemove.playerId].ClanName = "";
                                PersistentContainer.Instance.Players[_PlayertoRemove.playerId].ClanOfficer = false;
                                PersistentContainer.DataChange = true;
                                Phrases.Dict.TryGetValue(90, out string _phrase90);
                                _phrase90 = _phrase90.Replace("{PlayerName}", _PlayertoRemove.playerName);
                                _phrase90 = _phrase90.Replace("{ClanName}", _clanName);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase90 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                Phrases.Dict.TryGetValue(91, out string _phrase91);
                                _phrase91 = _phrase91.Replace("{ClanName}", _clanName);
                                ChatHook.ChatMessage(_PlayertoRemove, Config.Chat_Response_Color + _phrase91 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                for (int i = 0; i < ClanMember.Count; i++)
                                {
                                    string _clanMember = ClanMember[i];
                                    if (PersistentContainer.Instance.Players[_clanMember].ClanName == _clanName)
                                    {
                                        ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForPlayerId(_clanMember);
                                        if (_cInfo2 != null && _cInfo != _cInfo2)
                                        {
                                            Phrases.Dict.TryGetValue(102, out string _phrase102);
                                            _phrase102 = _phrase102.Replace("{PlayerName}", _PlayertoRemove.playerName);
                                            ChatHook.ChatMessage(_cInfo2, Config.Chat_Response_Color + _phrase102 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ClanManager.RemoveMember: {0}", e.Message));
            }
        }

        public static void PromoteMember(ClientInfo _cInfo, string _playerName)
        {
            try
            {
                bool _clanOfficer = PersistentContainer.Instance.Players[_cInfo.playerId].ClanOfficer;
                if (!_clanOfficer)
                {
                    Phrases.Dict.TryGetValue(77, out string _phrase77);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase77 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    ClientInfo _playertoPromote = ConsoleHelper.ParseParamIdOrName(_playerName);
                    if (_playertoPromote == null)
                    {
                        Phrases.Dict.TryGetValue(78, out string _phrase78);
                        _phrase78 = _phrase78.Replace("{PlayerName}", _playerName);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase78 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        string _clanName = PersistentContainer.Instance.Players[_cInfo.playerId].ClanName;
                        string _clanName2 = PersistentContainer.Instance.Players[_playertoPromote.playerId].ClanName;
                        if (_clanName2 == null || _clanName != _clanName2)
                        {
                            Phrases.Dict.TryGetValue(87, out string _phrase87);
                            _phrase87 = _phrase87.Replace("{PlayerName}", _playerName);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase87 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            bool _clanOfficer2 = PersistentContainer.Instance.Players[_playertoPromote.playerId].ClanOfficer;
                            if (_clanOfficer2)
                            {
                                Phrases.Dict.TryGetValue(92, out string _phrase92);
                                _phrase92 = _phrase92.Replace("{PlayerName}", _playerName);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase92 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                PersistentContainer.Instance.Players[_playertoPromote.playerId].ClanOfficer = true;
                                PersistentContainer.DataChange = true;
                                Phrases.Dict.TryGetValue(93, out string _phrase93);
                                _phrase93 = _phrase93.Replace("{PlayerName}", _playerName);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase93 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ClanManager.PromoteMember: {0}", e.Message));
            }
        }

        public static void DemoteMember(ClientInfo _cInfo, string _playerName)
        {
            try
            {
                bool _clanOwner = PersistentContainer.Instance.Players[_cInfo.playerId].ClanOwner;
                if (!_clanOwner)
                {
                    Phrases.Dict.TryGetValue(77, out string _phrase77);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase77 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    ClientInfo _membertoDemote = ConsoleHelper.ParseParamIdOrName(_playerName);
                    if (_membertoDemote == null)
                    {
                        Phrases.Dict.TryGetValue(78, out string _phrase78);
                        _phrase78 = _phrase78.Replace("{PlayerName}", _playerName);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase78 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        string _clanName = PersistentContainer.Instance.Players[_cInfo.playerId].ClanName;
                        string _clanName2 = PersistentContainer.Instance.Players[_membertoDemote.playerId].ClanName;
                        if (_clanName2 == null || _clanName != _clanName2)
                        {
                            Phrases.Dict.TryGetValue(87, out string _phrase87);
                            _phrase87 = _phrase87.Replace("{PlayerName}", _playerName);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase87 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            bool _clanOfficer2 = PersistentContainer.Instance.Players[_membertoDemote.playerId].ClanOfficer;
                            if (!_clanOfficer2)
                            {
                                Phrases.Dict.TryGetValue(94, out string _phrase94);
                                _phrase94 = _phrase94.Replace("{PlayerName}", _playerName);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase94 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                PersistentContainer.Instance.Players[_membertoDemote.playerId].ClanOfficer = false;
                                PersistentContainer.DataChange = true;
                                Phrases.Dict.TryGetValue(95, out string _phrase95);
                                _phrase95 = _phrase95.Replace("{PlayerName}", _playerName);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase95 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ClanManager.DemoteMember: {0}", e.Message));
            }
        }

        public static void LeaveClan(ClientInfo _cInfo)
        {
            try
            {
                bool _clanOwner = PersistentContainer.Instance.Players[_cInfo.playerId].ClanOwner;
                if (_clanOwner)
                {
                    Phrases.Dict.TryGetValue(96, out string _phrase96);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase96 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    string _clanName = PersistentContainer.Instance.Players[_cInfo.playerId].ClanName;
                    if (!string.IsNullOrEmpty(_clanName))
                    {
                        Phrases.Dict.TryGetValue(97, out string _phrase97);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase97 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        ClanMember.Remove(_cInfo.playerId);
                        PersistentContainer.Instance.Players[_cInfo.playerId].ClanName = "";
                        PersistentContainer.DataChange = true;
                        Phrases.Dict.TryGetValue(91, out string _phrase91);
                        _phrase91 = _phrase91.Replace("{ClanName}", _clanName);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase91 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        for (int i = 0; i < ClanMember.Count; i++)
                        {
                            string _clanMember = ClanMember[i];
                            if (PersistentContainer.Instance.Players[_clanMember].ClanName == _clanName)
                            {
                                ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForPlayerId(_clanMember);
                                if (_cInfo2 != null)
                                {
                                    Phrases.Dict.TryGetValue(102, out string _phrase102);
                                    _phrase102 = _phrase102.Replace("{PlayerName}", _cInfo.playerName);
                                    ChatHook.ChatMessage(_cInfo2, Config.Chat_Response_Color + _phrase102 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ClanManager.LeaveClan: {0}", e.Message));
            }
        }

        public static void RequestToJoinClan(ClientInfo _cInfo, string _clanName)
        {
            try
            {
                if (Clans.Count > 0 && Clans.ContainsValue(_clanName))
                {
                    foreach (var _clan in Clans)
                    {
                        if (_clan.Value == _clanName)
                        {
                            Dictionary<string, string> _clanRequests = new Dictionary<string, string>();
                            if (PersistentContainer.Instance.Players[_clan.Key].ClanRequestToJoin != null && PersistentContainer.Instance.Players[_clan.Key].ClanRequestToJoin.Count > 0)
                            {
                                _clanRequests = PersistentContainer.Instance.Players[_clan.Key].ClanRequestToJoin;
                            }
                            if (!_clanRequests.ContainsKey(_cInfo.playerId))
                            {
                                _clanRequests.Add(_cInfo.playerId, _cInfo.playerName);
                                PersistentContainer.Instance.Players[_clan.Key].ClanRequestToJoin = _clanRequests;
                                PersistentContainer.DataChange = true;
                                Phrases.Dict.TryGetValue(110, out string _phrase110);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase110 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForPlayerId(_clan.Key);
                                if (_cInfo2 != null)
                                {
                                    Phrases.Dict.TryGetValue(108, out string _phrase108);
                                    _phrase108 = _phrase108.Replace("{PlayerName}", _cInfo.playerName);
                                    _phrase108 = _phrase108.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                                    _phrase108 = _phrase108.Replace("{Command36}", Command_accept);
                                    _phrase108 = _phrase108.Replace("{Command37}", Command_decline);
                                    ChatHook.ChatMessage(_cInfo2, Config.Chat_Response_Color + _phrase108 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue(111, out string _phrase111);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase111 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            return;
                        }
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue(112, out string _phrase112);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase112 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ClanManager.RequestToJoinClan: {0}", e.Message));
            }
        }

        public static string GetChatCommands(ClientInfo _cInfo)
        {
            string _clanName = PersistentContainer.Instance.Players[_cInfo.playerId].ClanName;
            string _clanInvite = PersistentContainer.Instance.Players[_cInfo.playerId].ClanInvite;
            bool _clanOwner = PersistentContainer.Instance.Players[_cInfo.playerId].ClanOwner;
            bool _clanOfficer = PersistentContainer.Instance.Players[_cInfo.playerId].ClanOfficer;
            Phrases.Dict.TryGetValue(113, out string _phrase113);
            string _commands = _phrase113;
            if (string.IsNullOrEmpty(_clanName))
            {
                _commands = string.Format("{0} {1}{2} ClanName", _commands, ChatHook.Chat_Command_Prefix1, Command_add);
                _commands = string.Format("{0} {1}{2} ClanName", _commands, ChatHook.Chat_Command_Prefix1, Command_request);
            }
            if (_clanOwner)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Command_promote);
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Command_demote);
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Command_delete);
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Command_rename);
            }
            if (_clanOwner || _clanOfficer)
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Command_invite);
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Command_remove);
            }
            if (!string.IsNullOrEmpty(_clanInvite))
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Command_accept);
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Command_decline);
            }
            if (!_clanOwner && !string.IsNullOrEmpty(_clanName))
            {
                _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Command_leave);
            }
            if (ClanMember.Contains(_cInfo.playerId))
            {
                _commands = string.Format("{0} {1}{2} or {3}{4}", _commands, ChatHook.Chat_Command_Prefix1, Command_chat, ChatHook.Chat_Command_Prefix1, Command_cc);
            }
            _commands = string.Format("{0} {1}{2}", _commands, ChatHook.Chat_Command_Prefix1, Command_clan_list);
            return _commands;
        }

        public static void Clan(ClientInfo _cInfo, string _message)
        {
            try
            {
                string _clanName = PersistentContainer.Instance.Players[_cInfo.playerId].ClanName;
                if (!string.IsNullOrEmpty(_clanName))
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
                }
                else
                {
                    Phrases.Dict.TryGetValue(97, out string _phrase97);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase97 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ClanManager.Clan: {0}", e.Message));
            }
        }
    }
}