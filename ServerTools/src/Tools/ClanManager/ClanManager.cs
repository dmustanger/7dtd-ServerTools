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
                Phrases.Dict.TryGetValue("Clan34", out string _phrase);
                string _clanList = string.Format("{0}", _phrase);
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
                    Phrases.Dict.TryGetValue("Clan35", out _phrase);
                    _clanList = string.Format("{0}", _phrase);
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
                for (int i = 0; i < PersistentContainer.Instance.Players.IDs.Count; i++)
                {
                    string _id = PersistentContainer.Instance.Players.IDs[i];
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
                    Phrases.Dict.TryGetValue("Clan36", out string _phrase);
                    _phrase = _phrase.Replace("{MaxLength}", Max_Name_Length.ToString());
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    bool _clanOwner = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanOwner;
                    if (_clanOwner)
                    {
                        string _clan = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanName;
                        Phrases.Dict.TryGetValue("Clan1", out string _phrase);
                        _phrase = _phrase.Replace("{ClanName}", _clan.ToString());
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        string _clan = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanName;
                        if (!string.IsNullOrEmpty(_clan))
                        {
                            Phrases.Dict.TryGetValue("Clan3", out string _phrase);
                            _phrase = _phrase.Replace("{ClanName}", _clan.ToString());
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            if (Clans.ContainsValue(_clanName))
                            {
                                Phrases.Dict.TryGetValue("Clan2", out string _phrase);
                                _phrase = _phrase.Replace("{ClanName}", _clan.ToString());
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                ClanMember.Add(_cInfo.CrossplatformId.CombinedString);
                                Clans.Add(_cInfo.CrossplatformId.CombinedString, _clanName);
                                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanName = _clanName;
                                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanOwner = true;
                                PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanOfficer = true;
                                PersistentContainer.DataChange = true;
                                Phrases.Dict.TryGetValue("Clan4", out string _phrase);
                                _phrase = _phrase.Replace("{ClanName}", _clan.ToString());
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                bool _clanOwner = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanOwner;
                if (!_clanOwner)
                {
                    Phrases.Dict.TryGetValue("Clan5", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    string _clanName = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanName;
                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanName = "";
                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanOwner = false;
                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanOfficer = false;
                    PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanRequestToJoin = null;
                    for (int i = 0; i < PersistentContainer.Instance.Players.IDs.Count; i++)
                    {
                        string _id = PersistentContainer.Instance.Players.IDs[i];
                        PersistentPlayer p = PersistentContainer.Instance.Players[_id];
                        {
                            if (p.ClanName != null && p.ClanName == _clanName)
                            {
                                p.ClanName = "";
                                p.ClanOfficer = false;
                                ClanMember.Remove(_id);
                                ClientInfo cInfo2 = PersistentOperations.GetClientInfoFromNameOrId(_id);
                                if (cInfo2 != null && _cInfo != cInfo2)
                                {
                                    Phrases.Dict.TryGetValue("Clan21", out string phrase1);
                                    phrase1 = phrase1.Replace("{ClanName}", _clanName);
                                    ChatHook.ChatMessage(cInfo2, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else if (p.ClanInvite != null && p.ClanInvite == _clanName)
                            {
                                PersistentContainer.Instance.Players[_id].ClanInvite = "";
                            }
                        }
                    }
                    PersistentContainer.DataChange = true;
                    ClanMember.Remove(_cInfo.CrossplatformId.CombinedString);
                    Clans.Remove(_cInfo.CrossplatformId.CombinedString);
                    Phrases.Dict.TryGetValue("Clan6", out string _phrase);
                    _phrase = _phrase.Replace("{ClanName}", _clanName);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                    Phrases.Dict.TryGetValue("Clan36", out string _phrase);
                    _phrase = _phrase.Replace("{MaxLength}", Max_Name_Length.ToString());
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    bool _clanOwner = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanOwner;
                    if (_clanOwner)
                    {
                        if (!Clans.ContainsValue(_clanName))
                        {
                            string _oldClanName = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanName;
                            for (int i = 0; i < ClanMember.Count; i++)
                            {
                                string _clanMember = ClanMember[i];
                                if (PersistentContainer.Instance.Players[_clanMember].ClanName == _oldClanName)
                                {
                                    PersistentContainer.Instance.Players[_clanMember].ClanName = _clanName;
                                    ClientInfo cInfo2 = PersistentOperations.GetClientInfoFromNameOrId(_clanMember);
                                    if (cInfo2 != null && _cInfo != cInfo2)
                                    {
                                        Phrases.Dict.TryGetValue("Clan31", out string phrase1);
                                        phrase1 = phrase1.Replace("{ClanName}", _clanName);
                                        ChatHook.ChatMessage(cInfo2, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                            }
                            PersistentContainer.DataChange = true;
                            Clans[_cInfo.CrossplatformId.CombinedString] = _clanName;
                            Phrases.Dict.TryGetValue("Clan30", out string _phrase);
                            _phrase = _phrase.Replace("{ClanName}", _clanName);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("Clan2", out string _phrase);
                            _phrase = _phrase.Replace("{ClanName}", _clanName);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Clan5", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                bool _clanOfficer = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanOfficer;
                if (!_clanOfficer)
                {
                    Phrases.Dict.TryGetValue("Clan7", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    ClientInfo _newMember = ConsoleHelper.ParseParamIdOrName(_playerName);
                    if (_newMember == null)
                    {
                        Phrases.Dict.TryGetValue("Clan8", out string _phrase);
                        _phrase = _phrase.Replace("{PlayerName}", _playerName);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        return;
                    }
                    else
                    {
                        string _newMemberClanName = PersistentContainer.Instance.Players[_newMember.CrossplatformId.CombinedString].ClanName;
                        if (_newMemberClanName != null && _newMemberClanName.Length > 0)
                        {
                            Phrases.Dict.TryGetValue("Clan9", out string _phrase);
                            _phrase = _phrase.Replace("{PlayerName}", _newMember.playerName);
                            _phrase = _phrase.Replace("{ClanName}", _newMemberClanName);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            string _clanInvite = PersistentContainer.Instance.Players[_newMember.CrossplatformId.CombinedString].ClanInvite;
                            if (_clanInvite != null && _clanInvite.Length > 0)
                            {
                                Phrases.Dict.TryGetValue("Clan10", out string _phrase);
                                _phrase = _phrase.Replace("{PlayerName}", _newMember.playerName);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                string _clanName = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanName;
                                PersistentContainer.Instance.Players[_newMember.CrossplatformId.CombinedString].ClanInvite = _clanName;
                                PersistentContainer.DataChange = true;
                                Phrases.Dict.TryGetValue("Clan11", out string _phrase);
                                _phrase = _phrase.Replace("{ClanName}", _clanName);
                                _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                                _phrase = _phrase.Replace("{Command_accept}", Command_accept);
                                _phrase = _phrase.Replace("{Command_decline}", Command_decline);
                                ChatHook.ChatMessage(_newMember, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                Phrases.Dict.TryGetValue("Clan12", out _phrase);
                                _phrase = _phrase.Replace("{PlayerName}", _newMember.playerName);
                                _phrase = _phrase.Replace("{ClanName}", _clanName);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                bool _clanOwner = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanOwner;
                if (!_clanOwner)
                {
                    string _clanInvite = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanInvite;
                    if (string.IsNullOrEmpty(_clanInvite))
                    {
                        Phrases.Dict.TryGetValue("Clan13", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        ClanMember.Add(_cInfo.CrossplatformId.CombinedString);
                        PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanInvite = "";
                        PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanName = _clanInvite;
                        PersistentContainer.DataChange = true;
                        for (int i = 0; i < ClanMember.Count; i++)
                        {
                            string _clanMember = ClanMember[i];
                            string _clanName = PersistentContainer.Instance.Players[_clanMember].ClanName;
                            if (!string.IsNullOrEmpty(_clanName) && _clanName == _clanInvite)
                            {
                                ClientInfo cInfo2 = PersistentOperations.GetClientInfoFromNameOrId(_clanMember);
                                if (cInfo2 != null)
                                {
                                    Phrases.Dict.TryGetValue("Clan29", out string phrase);
                                    phrase = phrase.Replace("{PlayerName}", _cInfo.playerName);
                                    ChatHook.ChatMessage(cInfo2, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Dictionary<string, string> _clanRequests = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanRequestToJoin;
                    if (_clanRequests != null && _clanRequests.Count > 0)
                    {
                        KeyValuePair<string, string> _request = _clanRequests.First();
                        _clanRequests.Remove(_request.Key);
                        string _clanName = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanName;
                        string _clanName1 = PersistentContainer.Instance.Players[_request.Key].ClanName;
                        if (!string.IsNullOrEmpty(_clanName1))
                        {
                            PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanRequestToJoin = _clanRequests;
                            Phrases.Dict.TryGetValue("Clan37", out string _phrase);
                            _phrase = _phrase.Replace("{PlayerName}", _request.Value);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return;
                        }
                        ClanMember.Add(_request.Key);
                        PersistentContainer.Instance.Players[_request.Key].ClanInvite = "";
                        PersistentContainer.Instance.Players[_request.Key].ClanName = _clanName;
                        PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanRequestToJoin = _clanRequests;
                        PersistentContainer.DataChange = true;
                        for (int i = 0; i < ClanMember.Count; i++)
                        {
                            string _clanMember = ClanMember[i];
                            string _clanName2 = PersistentContainer.Instance.Players[_clanMember].ClanName;
                            if (!string.IsNullOrEmpty(_clanName2) && _clanName2 == _clanName)
                            {
                                ClientInfo cInfo2 = PersistentOperations.GetClientInfoFromNameOrId(_clanMember);
                                if (cInfo2 != null)
                                {
                                    Phrases.Dict.TryGetValue("Clan29", out string phrase);
                                    phrase = phrase.Replace("{PlayerName}", _cInfo.playerName);
                                    ChatHook.ChatMessage(cInfo2, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                        }
                        if (_clanRequests.Count > 0)
                        {
                            _request = _clanRequests.First();
                            Phrases.Dict.TryGetValue("Clan38", out string _phrase);
                            _phrase = _phrase.Replace("{PlayerName}", _request.Value);
                            _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                            _phrase = _phrase.Replace("{Command_accept}", Command_accept);
                            _phrase = _phrase.Replace("{Command_decline}", Command_decline);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Clan39", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                bool _clanOwner = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanOwner;
                if (!_clanOwner)
                {
                    string _clanInvite = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanInvite;
                    if (string.IsNullOrEmpty(_clanInvite))
                    {
                        Phrases.Dict.TryGetValue("Clan13", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanInvite = "";
                        PersistentContainer.DataChange = true;
                        Phrases.Dict.TryGetValue("Clan16", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        for (int i = 0; i < ClanMember.Count; i++)
                        {
                            string _clanMember = ClanMember[i];
                            string _clanName = PersistentContainer.Instance.Players[_clanMember].ClanName;
                            if (string.IsNullOrEmpty(_clanName) && _clanName == _clanInvite)
                            {
                                ClientInfo _cInfo2 = PersistentOperations.GetClientInfoFromNameOrId(_clanMember);
                                if (_cInfo2 != null)
                                {
                                    Phrases.Dict.TryGetValue("Clan15", out _phrase);
                                    _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
                                    ChatHook.ChatMessage(_cInfo2, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Dictionary<string, string> _clanRequests = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanRequestToJoin;
                    if (_clanRequests != null && _clanRequests.Count > 0)
                    {
                        KeyValuePair<string, string> _request = _clanRequests.First();
                        _clanRequests.Remove(_request.Key);
                        PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanRequestToJoin = _clanRequests;
                        PersistentContainer.DataChange = true;
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + "Removed the request to join the group by player " + _request.Value + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        if (_clanRequests.Count > 0)
                        {
                            _request = _clanRequests.First();
                            Phrases.Dict.TryGetValue("Clan38", out string _phrase);
                            _phrase = _phrase.Replace("{PlayerName}", _request.Value);
                            _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                            _phrase = _phrase.Replace("{Command_accept}", Command_accept);
                            _phrase = _phrase.Replace("{Command_decline}", Command_decline);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                    }
                    else
                    {
                        Phrases.Dict.TryGetValue("Clan39", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                bool _clanOfficer = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanOfficer;
                if (!_clanOfficer)
                {
                    Phrases.Dict.TryGetValue("Clan7", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    ClientInfo _PlayertoRemove = ConsoleHelper.ParseParamIdOrName(_playerName);
                    if (_PlayertoRemove == null)
                    {
                        Phrases.Dict.TryGetValue("Clan8", out string _phrase);
                        _phrase = _phrase.Replace("{PlayerName}", _playerName);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        return;
                    }
                    else
                    {
                        string _clanName = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanName;
                        string _clanName2 = PersistentContainer.Instance.Players[_PlayertoRemove.CrossplatformId.CombinedString].ClanName;
                        if (_clanName2 == null || _clanName != _clanName2)
                        {
                            Phrases.Dict.TryGetValue("Clan16", out string _phrase);
                            _phrase = _phrase.Replace("{PlayerName}", _playerName);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            bool _clanOfficer2 = PersistentContainer.Instance.Players[_PlayertoRemove.CrossplatformId.CombinedString].ClanOfficer;
                            bool _clanOwner = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanOwner;
                            if (_clanOfficer2 && !_clanOwner)
                            {
                                Phrases.Dict.TryGetValue("Clan18", out string _phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                ClanMember.Remove(_PlayertoRemove.CrossplatformId.CombinedString);
                                PersistentContainer.Instance.Players[_PlayertoRemove.CrossplatformId.CombinedString].ClanName = "";
                                PersistentContainer.Instance.Players[_PlayertoRemove.CrossplatformId.CombinedString].ClanOfficer = false;
                                PersistentContainer.DataChange = true;
                                Phrases.Dict.TryGetValue("Clan20", out string _phrase);
                                _phrase = _phrase.Replace("{PlayerName}", _PlayertoRemove.playerName);
                                _phrase = _phrase.Replace("{ClanName}", _clanName);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                Phrases.Dict.TryGetValue("Clan21", out _phrase);
                                _phrase = _phrase.Replace("{ClanName}", _clanName);
                                ChatHook.ChatMessage(_PlayertoRemove, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                for (int i = 0; i < ClanMember.Count; i++)
                                {
                                    string _clanMember = ClanMember[i];
                                    if (PersistentContainer.Instance.Players[_clanMember].ClanName == _clanName)
                                    {
                                        ClientInfo _cInfo2 = PersistentOperations.GetClientInfoFromNameOrId(_clanMember);
                                        if (_cInfo2 != null && _cInfo != _cInfo2)
                                        {
                                            Phrases.Dict.TryGetValue("Clan32", out _phrase);
                                            _phrase = _phrase.Replace("{PlayerName}", _PlayertoRemove.playerName);
                                            ChatHook.ChatMessage(_cInfo2, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                bool _clanOfficer = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanOfficer;
                if (!_clanOfficer)
                {
                    Phrases.Dict.TryGetValue("Clan7", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    ClientInfo _playertoPromote = ConsoleHelper.ParseParamIdOrName(_playerName);
                    if (_playertoPromote == null)
                    {
                        Phrases.Dict.TryGetValue("Clan8", out string _phrase);
                        _phrase = _phrase.Replace("{PlayerName}", _playerName);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        string _clanName = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanName;
                        string _clanName2 = PersistentContainer.Instance.Players[_playertoPromote.CrossplatformId.CombinedString].ClanName;
                        if (_clanName2 == null || _clanName != _clanName2)
                        {
                            Phrases.Dict.TryGetValue("Clan17", out string _phrase);
                            _phrase = _phrase.Replace("{PlayerName}", _playerName);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            bool _clanOfficer2 = PersistentContainer.Instance.Players[_playertoPromote.CrossplatformId.CombinedString].ClanOfficer;
                            if (_clanOfficer2)
                            {
                                Phrases.Dict.TryGetValue("Clan22", out string _phrase);
                                _phrase = _phrase.Replace("{PlayerName}", _playerName);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                PersistentContainer.Instance.Players[_playertoPromote.CrossplatformId.CombinedString].ClanOfficer = true;
                                PersistentContainer.DataChange = true;
                                Phrases.Dict.TryGetValue("Clan23", out string _phrase);
                                _phrase = _phrase.Replace("{PlayerName}", _playerName);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                bool _clanOwner = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanOwner;
                if (!_clanOwner)
                {
                    Phrases.Dict.TryGetValue("Clan7", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    ClientInfo _membertoDemote = ConsoleHelper.ParseParamIdOrName(_playerName);
                    if (_membertoDemote == null)
                    {
                        Phrases.Dict.TryGetValue("Clan8", out string _phrase);
                        _phrase = _phrase.Replace("{PlayerName}", _playerName);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        string _clanName = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanName;
                        string _clanName2 = PersistentContainer.Instance.Players[_membertoDemote.CrossplatformId.CombinedString].ClanName;
                        if (_clanName2 == null || _clanName != _clanName2)
                        {
                            Phrases.Dict.TryGetValue("Clan17", out string _phrase);
                            _phrase = _phrase.Replace("{PlayerName}", _playerName);
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        else
                        {
                            bool _clanOfficer2 = PersistentContainer.Instance.Players[_membertoDemote.CrossplatformId.CombinedString].ClanOfficer;
                            if (!_clanOfficer2)
                            {
                                Phrases.Dict.TryGetValue("Clan24", out string _phrase);
                                _phrase = _phrase.Replace("{PlayerName}", _playerName);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                PersistentContainer.Instance.Players[_membertoDemote.CrossplatformId.CombinedString].ClanOfficer = false;
                                PersistentContainer.DataChange = true;
                                Phrases.Dict.TryGetValue("Clan25", out string _phrase);
                                _phrase = _phrase.Replace("{PlayerName}", _playerName);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                bool _clanOwner = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanOwner;
                if (_clanOwner)
                {
                    Phrases.Dict.TryGetValue("Clan26", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                else
                {
                    string _clanName = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanName;
                    if (!string.IsNullOrEmpty(_clanName))
                    {
                        Phrases.Dict.TryGetValue("Clan27", out string _phrase);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    }
                    else
                    {
                        ClanMember.Remove(_cInfo.CrossplatformId.CombinedString);
                        PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanName = "";
                        PersistentContainer.DataChange = true;
                        Phrases.Dict.TryGetValue("Clan21", out string _phrase);
                        _phrase = _phrase.Replace("{ClanName}", _clanName);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        for (int i = 0; i < ClanMember.Count; i++)
                        {
                            string _clanMember = ClanMember[i];
                            if (PersistentContainer.Instance.Players[_clanMember].ClanName == _clanName)
                            {
                                ClientInfo _cInfo2 = PersistentOperations.GetClientInfoFromNameOrId(_clanMember);
                                if (_cInfo2 != null)
                                {
                                    Phrases.Dict.TryGetValue("Clan32", out _phrase);
                                    _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
                                    ChatHook.ChatMessage(_cInfo2, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                            if (!_clanRequests.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                            {
                                _clanRequests.Add(_cInfo.CrossplatformId.CombinedString, _cInfo.playerName);
                                PersistentContainer.Instance.Players[_clan.Key].ClanRequestToJoin = _clanRequests;
                                PersistentContainer.DataChange = true;
                                Phrases.Dict.TryGetValue("Clan40", out string _phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                ClientInfo _cInfo2 = PersistentOperations.GetClientInfoFromNameOrId(_clan.Key);
                                if (_cInfo2 != null)
                                {
                                    Phrases.Dict.TryGetValue("Clan38", out _phrase);
                                    _phrase = _phrase.Replace("{PlayerName}", _cInfo.playerName);
                                    _phrase = _phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                                    _phrase = _phrase.Replace("{Command36}", Command_accept);
                                    _phrase = _phrase.Replace("{Command37}", Command_decline);
                                    ChatHook.ChatMessage(_cInfo2, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("Clan41", out string _phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            return;
                        }
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Clan42", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ClanManager.RequestToJoinClan: {0}", e.Message));
            }
        }

        public static string GetChatCommands(ClientInfo _cInfo)
        {
            string _clanName = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanName;
            string _clanInvite = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanInvite;
            bool _clanOwner = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanOwner;
            bool _clanOfficer = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanOfficer;
            Phrases.Dict.TryGetValue("Clan43", out string _phrase);
            string _commands = _phrase;
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
            if (ClanMember.Contains(_cInfo.PlatformId.ReadablePlatformUserIdentifier))
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
                string _clanName = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanName;
                if (!string.IsNullOrEmpty(_clanName))
                {
                    foreach (KeyValuePair<string, string> _clan in Clans)
                    {
                        if (_clan.Value == _clanName)
                        {
                            ClientInfo _cInfo2 = PersistentOperations.GetClientInfoFromNameOrId(_clan.Key);
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
                    Phrases.Dict.TryGetValue("Clan27", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ClanManager.Clan: {0}", e.Message));
            }
        }
    }
}