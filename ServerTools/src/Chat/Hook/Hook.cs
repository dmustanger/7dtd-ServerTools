using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerTools
{
    public class ChatHook
    {
        public static bool ChatFlood = false, Normal_Player_Color_Prefix = false, Message_Color_Enabled = false, Alter_Message_Color = false, Role_Play = false;
        public static string Normal_Player_Name_Color = "[00B3B3]", Normal_Player_Prefix = "NOOB", Friend_Chat_Color = "[33CC33]", Party_Chat_Color = "[FFCC00]",
            Chat_Command_Prefix1 = "/", Chat_Command_Prefix2 = "!", Message_Color = "[FFFFFF]", Normal_Player_Prefix_Color = "[FFFFFF]";
        public static int Admin_Level = 0, Mod_Level = 1, Max_Length = 250, Messages_Per_Min = 8, Wait_Time = 60;
        private static readonly Dictionary<string, int> ChatFloodCount = new Dictionary<string, int>();
        private static readonly Dictionary<string, DateTime> ChatFloodTime = new Dictionary<string, DateTime>();
        private static readonly Dictionary<string, DateTime> ChatFloodLock = new Dictionary<string, DateTime>();

        public static bool Hook(ClientInfo _cInfo, EChatType _type, int _senderId, string _message, string _mainName, List<int> _recipientEntityIds)
        {
            try
            {
                if (!string.IsNullOrEmpty(_message) && _cInfo != null && _mainName != Config.Server_Response_Name)
                {
                    if (Mute.IsEnabled && Mute.Mutes.Contains(_cInfo.playerId))
                    {
                        if (Mute.Block_Commands && (_message.StartsWith(Chat_Command_Prefix1) || _message.StartsWith(Chat_Command_Prefix2)))
                        {
                            Phrases.Dict.TryGetValue(760, out string _phrase760);
                            ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase760 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return false;
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue(761, out string _phrase761);
                            ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase761 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return false;
                        }
                    }
                    if (Badwords.IsEnabled)
                    {
                        if (Badwords.Invalid_Name)
                        {
                            if (_mainName.Contains(" "))
                            {
                                string[] _nameSplit = _mainName.Split(' ');
                                for (int i = 0; i < _nameSplit.Length; i++)
                                {
                                    string _partialName = _nameSplit[i].ToLower();
                                    for (int j = 0; j < Badwords.Words.Count; j++)
                                    {
                                        if (_partialName == Badwords.Words[j])
                                        {
                                            _mainName = _mainName.ToLower().Replace(Badwords.Words[j], "***");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < Badwords.Words.Count; i++)
                                {
                                    if (_mainName == Badwords.Words[i])
                                    {
                                        _mainName = "***";
                                        continue;
                                    }
                                }
                            }
                        }
                        for (int i = 0; i < Badwords.Words.Count; i++)
                        {
                            if (_message.ToLower().Contains(Badwords.Words[i]))
                            {
                                _message = _message.ToLower().Replace(Badwords.Words[i], "***");
                            }
                        }
                    }
                    if (!Jail.IsEnabled || !Jail.Jailed.Contains(_cInfo.playerId))
                    {
                        if (_message.StartsWith(" "))
                        {
                            _message.Substring(1);
                        }
                        else if (_message.StartsWith("  "))
                        {
                            _message.Substring(2);
                        }
                        if (!_message.StartsWith("@") && _senderId != -1 && !_message.StartsWith(Chat_Command_Prefix1) && !_message.StartsWith(Chat_Command_Prefix2))
                        {
                            if (ChatLog.IsEnabled)
                            {
                                ChatLog.Exec(_message, _mainName);
                            }
                            if (ChatFlood)
                            {
                                if (_message.Length >= Max_Length)
                                {
                                    Phrases.Dict.TryGetValue(612, out string _phrase612);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase612 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    return false;
                                }
                                else if (Messages_Per_Min > 1)
                                {
                                    if (ChatFloodLock.ContainsKey(_cInfo.playerId))
                                    {
                                        ChatFloodLock.TryGetValue(_cInfo.playerId, out DateTime _lockTime);
                                        TimeSpan varTime = DateTime.Now - _lockTime;
                                        double fractionalSeconds = varTime.TotalSeconds;
                                        if ((int)fractionalSeconds >= Wait_Time)
                                        {
                                            ChatFloodLock.Remove(_cInfo.playerId);
                                        }
                                        else
                                        {
                                            Phrases.Dict.TryGetValue(611, out string _phrase611);
                                            ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase611 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                            return false;
                                        }
                                    }
                                    if (ChatFloodCount.ContainsKey(_cInfo.playerId))
                                    {
                                        ChatFloodCount.TryGetValue(_cInfo.playerId, out int _count);
                                        ChatFloodTime.TryGetValue(_cInfo.playerId, out DateTime _chatTime);
                                        TimeSpan varTime = DateTime.Now - _chatTime;
                                        double fractionalSeconds = varTime.TotalSeconds;
                                        if ((int)fractionalSeconds >= 60)
                                        {
                                            ChatFloodCount[_cInfo.playerId] = 1;
                                            ChatFloodTime[_cInfo.playerId] = DateTime.Now;
                                        }
                                        else
                                        {
                                            if (_count + 1 == Messages_Per_Min)
                                            {
                                                ChatFloodCount.Remove(_cInfo.playerId);
                                                ChatFloodTime.Remove(_cInfo.playerId);
                                                ChatFloodLock.Add(_cInfo.playerId, DateTime.Now);
                                                Phrases.Dict.TryGetValue(611, out string _phrase611);
                                                ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase611 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                return false;
                                            }
                                            else
                                            {
                                                ChatFloodCount[_cInfo.playerId] = _count + 1;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ChatFloodCount.Add(_cInfo.playerId, 1);
                                        ChatFloodTime.Add(_cInfo.playerId, DateTime.Now);
                                    }
                                }
                            }
                            if (ChatColorPrefix.IsEnabled && ChatColorPrefix.Players.ContainsKey(_cInfo.playerId))
                            {
                                ChatColorPrefix.ExpireDate.TryGetValue(_cInfo.playerId, out DateTime _dt);
                                if (DateTime.Now < _dt)
                                {
                                    ChatColorPrefix.Players.TryGetValue(_cInfo.playerId, out string[] _colorPrefix);
                                    if (ClanManager.IsEnabled && ClanManager.ClanMember.Contains(_cInfo.playerId))
                                    {
                                        string _clanName = PersistentContainer.Instance.Players[_cInfo.playerId].ClanName;
                                        PrepMessage(_cInfo, _message, _senderId, _mainName, _clanName, _colorPrefix[3], _colorPrefix[4], _type, _recipientEntityIds);
                                    }
                                    else
                                    {
                                        PrepMessage(_cInfo, _message, _senderId, _mainName, _colorPrefix[2], _colorPrefix[3], _colorPrefix[4], _type, _recipientEntityIds);
                                    }
                                    return false;
                                }
                                else
                                {
                                    ChatColorPrefix.Players.Remove(_cInfo.playerId);
                                    ChatColorPrefix.ExpireDate.Remove(_cInfo.playerId);
                                    ChatColorPrefix.UpdateXml();
                                    Phrases.Dict.TryGetValue(931, out string _phrase931);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase931 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    return false;
                                }
                            }
                            else if (ClanManager.IsEnabled && ClanManager.ClanMember.Contains(_cInfo.playerId))
                            {
                                if (!string.IsNullOrEmpty(PersistentContainer.Instance.Players[_cInfo.playerId].ClanName))
                                {
                                    string _clanName = PersistentContainer.Instance.Players[_cInfo.playerId].ClanName;
                                    string _npnc = Normal_Player_Name_Color, _nppc = Normal_Player_Prefix_Color;
                                    if (Normal_Player_Name_Color == "")
                                    {
                                        _npnc = string.Empty;
                                    }
                                    if (Normal_Player_Prefix_Color == "")
                                    {
                                        _nppc = string.Empty;
                                    }
                                    PrepMessage(_cInfo, _message, _senderId, _mainName, _clanName, _npnc, _nppc, _type, _recipientEntityIds);
                                    return false;
                                }
                            }
                            else if (Normal_Player_Color_Prefix)
                            {
                                string _npp = Normal_Player_Prefix, _npnc = Normal_Player_Name_Color, _nppc = Normal_Player_Prefix_Color;
                                if (Normal_Player_Prefix == "")
                                {
                                    _npp = string.Empty;
                                }
                                if (Normal_Player_Name_Color == "")
                                {
                                    _npnc = string.Empty;
                                }
                                if (Normal_Player_Prefix_Color == "")
                                {
                                    _nppc = string.Empty;
                                }
                                PrepMessage(_cInfo, _message, _senderId, _mainName, _npp, _npnc, _nppc, _type, _recipientEntityIds);
                                return false;
                            }
                        }
                        if ((_message.StartsWith(Chat_Command_Prefix1) || _message.StartsWith(Chat_Command_Prefix2)) && (!_message.StartsWith(Chat_Command_Prefix1 + "***") || !_message.StartsWith(Chat_Command_Prefix2 + "***")))
                        {
                            if (ChatCommandLog.IsEnabled)
                            {
                                ChatCommandLog.Exec(_message, _cInfo);
                            }
                            if (_message.StartsWith(Chat_Command_Prefix1))
                            {
                                _message = _message.Replace(Chat_Command_Prefix1, "");
                            }
                            else if (_message.StartsWith(Chat_Command_Prefix2))
                            {
                                _message = _message.Replace(Chat_Command_Prefix2, "");
                            }
                            string messageLowerCase = _message.ToLower();
                            if (Homes.IsEnabled)
                            {
                                if (messageLowerCase == Homes.Command_home)
                                {
                                    Homes.List(_cInfo);
                                    return false;
                                }
                                else if (messageLowerCase.StartsWith(Homes.Command_fhome + " "))
                                {
                                    _message = messageLowerCase.Replace(Homes.Command_fhome + " ", "");
                                    Homes.TeleDelay(_cInfo, _message, true);
                                    return false;
                                }
                                else if (messageLowerCase.StartsWith(Homes.Command_save + " "))
                                {
                                    _message = messageLowerCase.Replace(Homes.Command_save + " ", "");
                                    Homes.SaveClaimCheck(_cInfo, _message);
                                    return false;
                                }
                                else if (messageLowerCase.StartsWith(Homes.Command_delete + " "))
                                {
                                    _message = messageLowerCase.Replace(Homes.Command_delete + " ", "");
                                    Homes.DelHome(_cInfo, _message);
                                    return false;
                                }
                                else if (messageLowerCase == Homes.Command_go)
                                {
                                    if (Homes.Invite.ContainsKey(_cInfo.entityId))
                                    {
                                        Homes.FriendHome(_cInfo);
                                        return false;
                                    }
                                }
                                else if (messageLowerCase.StartsWith(Homes.Command_set + " "))
                                {
                                    _message = messageLowerCase.Replace(Homes.Command_set + " ", "");
                                    Homes.SaveClaimCheck(_cInfo, _message);
                                    return false;
                                }
                                else if (messageLowerCase.StartsWith(Homes.Command_home + " "))
                                {
                                    _message = messageLowerCase.Replace(Homes.Command_home + " ", "");
                                    Homes.TeleDelay(_cInfo, _message, false);
                                    return false;
                                }
                            }
                            if (Hardcore.IsEnabled && messageLowerCase == Hardcore.Command_top3)
                            {
                                Hardcore.TopThree(_cInfo);
                                return false;
                            }
                            if (Hardcore.IsEnabled && messageLowerCase == Hardcore.Command_score)
                            {
                                Hardcore.Score(_cInfo);
                                return false;
                            }
                            if (Hardcore.IsEnabled && Hardcore.Max_Extra_Lives > 0 && Wallet.IsEnabled && messageLowerCase == Hardcore.Command_buy_life)
                            {
                                Hardcore.BuyLife(_cInfo);
                                return false;
                            }
                            if (MuteVote.IsEnabled && Mute.IsEnabled && messageLowerCase.StartsWith(MuteVote.Command_mutevote))
                            {
                                if (!KickVote.VoteOpen && !RestartVote.VoteOpen && !MuteVote.VoteOpen)
                                {
                                    if (messageLowerCase == MuteVote.Command_mutevote)
                                    {
                                        MuteVote.List(_cInfo);
                                    }
                                    else
                                    {
                                        _message = messageLowerCase.Replace(MuteVote.Command_mutevote + " ", "");
                                        {
                                            MuteVote.Vote(_cInfo, _message);
                                        }
                                    }
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue(920, out string _phrase920);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase920 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                            if (Mute.IsEnabled && messageLowerCase == Mute.Command_mutelist)
                            {
                                Mute.List(_cInfo);
                                return false;
                            }
                            if (Mute.IsEnabled && messageLowerCase.StartsWith(Mute.Command_mute + " "))
                            {
                                Mute.Add(_cInfo, _message);
                                return false;
                            }
                            if (Mute.IsEnabled && messageLowerCase.StartsWith(Mute.Command_unmute + " "))
                            {
                                Mute.Remove(_cInfo, _message);
                                return false;
                            }
                            if (messageLowerCase == CustomCommands.Command_commands)
                            {
                                CustomCommands.CommandList(_cInfo);
                                if (CustomCommands.IsEnabled)
                                {
                                    CustomCommands.CustomCommandList(_cInfo);
                                }
                                if (GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo) <= Admin_Level)
                                {
                                    CustomCommands.AdminCommandList(_cInfo);
                                }
                                return false;
                            }
                            if (Day7.IsEnabled && (messageLowerCase == Day7.Command_day7 || messageLowerCase == Day7.Command_day))
                            {
                                Day7.GetInfo(_cInfo);
                                return false;
                            }
                            if (Bloodmoon.IsEnabled && (messageLowerCase == Bloodmoon.Command_bloodmoon || messageLowerCase == Bloodmoon.Command_bm))
                            {
                                Bloodmoon.Exec(_cInfo);
                                return false;
                            }
                            if (Suicide.IsEnabled && (messageLowerCase == Suicide.Command_killme || messageLowerCase == Suicide.Command_wrist || messageLowerCase == Suicide.Command_hang || messageLowerCase == Suicide.Command_suicide))
                            {
                                Suicide.Exec(_cInfo);
                                return false;
                            }
                            if (Gimme.IsEnabled && (messageLowerCase == Gimme.Command_gimme || messageLowerCase == Gimme.Command_gimmie))
                            {
                                Gimme.Exec(_cInfo);
                                return false;
                            }
                            if (Jail.IsEnabled && (messageLowerCase == Jail.Command_set_jail || messageLowerCase.StartsWith(Jail.Command_jail) || messageLowerCase.StartsWith(Jail.Command_unjail)))
                            {
                                if (messageLowerCase == Jail.Command_set_jail)
                                {
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _message, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    Jail.SetJail(_cInfo);
                                }
                                else if (messageLowerCase.StartsWith(Jail.Command_jail))
                                {
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _message, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    Jail.PutInJail(_cInfo, _message);
                                }
                                else if (messageLowerCase.StartsWith(Jail.Command_unjail))
                                {
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _message, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    Jail.RemoveFromJail(_cInfo, _message);
                                }
                                return false;
                            }
                            if (NewSpawnTele.IsEnabled && messageLowerCase == NewSpawnTele.Command_setspawn)
                            {
                                NewSpawnTele.SetNewSpawnTele(_cInfo);
                                return false;
                            }
                            if (Animals.IsEnabled && (messageLowerCase == Animals.Command_trackanimal || messageLowerCase == Animals.Command_track))
                            {
                                Animals.Exec(_cInfo);
                                return false;
                            }
                            if (FirstClaimBlock.IsEnabled && messageLowerCase == FirstClaimBlock.Command_claim)
                            {
                                FirstClaimBlock.FirstClaim(_cInfo);
                                return false;
                            }
                            if (ClanManager.IsEnabled)
                            {
                                if (messageLowerCase == ClanManager.Command_add)
                                {
                                    Phrases.Dict.TryGetValue(114, out string _phrase114);
                                    _phrase114 = _phrase114.Replace("{Command_Prefix1}", Chat_Command_Prefix1);
                                    _phrase114 = _phrase114.Replace("{Command_add}", ClanManager.Command_add);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase114 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    return false;
                                }
                                else if (messageLowerCase.StartsWith(ClanManager.Command_add))
                                {
                                    _message = _message.Replace(ClanManager.Command_add + " ", "");
                                    ClanManager.AddClan(_cInfo, _message);
                                    return false;
                                }
                                else if (messageLowerCase == ClanManager.Command_delete)
                                {
                                    ClanManager.RemoveClan(_cInfo);
                                    return false;
                                }
                                else if (messageLowerCase == ClanManager.Command_invite)
                                {
                                    Phrases.Dict.TryGetValue(115, out string _phrase115);
                                    _phrase115 = _phrase115.Replace("{Command_Prefix1}", Chat_Command_Prefix1);
                                    _phrase115 = _phrase115.Replace("{Command_invite}", ClanManager.Command_invite);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase115 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    return false;
                                }
                                else if (messageLowerCase.StartsWith(ClanManager.Command_invite))
                                {
                                    _message = _message.Replace(ClanManager.Command_invite + " ", "");
                                    ClanManager.InviteMember(_cInfo, _message);
                                    return false;
                                }
                                else if (messageLowerCase == ClanManager.Command_accept)
                                {
                                    ClanManager.InviteAccept(_cInfo);
                                    return false;
                                }
                                else if (messageLowerCase == ClanManager.Command_decline)
                                {
                                    ClanManager.InviteDecline(_cInfo);
                                    return false;
                                }
                                else if (messageLowerCase == ClanManager.Command_remove)
                                {
                                    Phrases.Dict.TryGetValue(116, out string _phrase116);
                                    _phrase116 = _phrase116.Replace("{Command_Prefix1}", Chat_Command_Prefix1);
                                    _phrase116 = _phrase116.Replace("{Command_remove}", ClanManager.Command_remove);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase116 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    return false;
                                }
                                else if (messageLowerCase.StartsWith(ClanManager.Command_remove))
                                {
                                    _message = _message.Replace(ClanManager.Command_remove + " ", "");
                                    ClanManager.RemoveMember(_cInfo, _message);
                                    return false;
                                }
                                else if (messageLowerCase == ClanManager.Command_promote)
                                {
                                    Phrases.Dict.TryGetValue(117, out string _phrase117);
                                    _phrase117 = _phrase117.Replace("{Command_Prefix1}", Chat_Command_Prefix1);
                                    _phrase117 = _phrase117.Replace("{Command_promote}", ClanManager.Command_promote);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase117 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else if (messageLowerCase.StartsWith(ClanManager.Command_promote))
                                {
                                    _message = _message.Replace(ClanManager.Command_promote + " ", "");
                                    ClanManager.PromoteMember(_cInfo, _message);
                                    return false;
                                }
                                else if (messageLowerCase == ClanManager.Command_demote)
                                {
                                    Phrases.Dict.TryGetValue(118, out string _phrase118);
                                    _phrase118 = _phrase118.Replace("{Command_Prefix1}", Chat_Command_Prefix1);
                                    _phrase118 = _phrase118.Replace("{Command_demote}", ClanManager.Command_demote);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase118 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    return false;
                                }
                                else if (messageLowerCase.StartsWith(ClanManager.Command_demote))
                                {
                                    _message = _message.Replace(ClanManager.Command_demote + " ", "");
                                    ClanManager.DemoteMember(_cInfo, _message);
                                    return false;
                                }
                                else if (messageLowerCase == ClanManager.Command_leave)
                                {
                                    ClanManager.LeaveClan(_cInfo);
                                    return false;
                                }
                                else if (messageLowerCase == ClanManager.Command_commands)
                                {
                                    string _clanCommands = ClanManager.GetChatCommands(_cInfo);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _clanCommands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    return false;
                                }
                                else if (messageLowerCase == ClanManager.Command_chat || messageLowerCase == ClanManager.Command_cc)
                                {
                                    Phrases.Dict.TryGetValue(119, out string _phrase119);
                                    _phrase119 = _phrase119.Replace("{Command_Prefix1}", Chat_Command_Prefix1);
                                    _phrase119 = _phrase119.Replace("{Command_chat}", ClanManager.Command_chat);
                                    _phrase119 = _phrase119.Replace("{Command_cc}", ClanManager.Command_cc);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase119 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else if (messageLowerCase == ClanManager.Command_chat || messageLowerCase == ClanManager.Command_cc && ClanManager.ClanMember.Contains(_cInfo.playerId))
                                {
                                    if (messageLowerCase.StartsWith(ClanManager.Command_chat))
                                    {
                                        _message = messageLowerCase.Replace(ClanManager.Command_chat + " ", "");
                                        ClanManager.Clan(_cInfo, _message);
                                    }
                                    else
                                    {
                                        _message = messageLowerCase.Replace(ClanManager.Command_cc + " ", "");
                                        ClanManager.Clan(_cInfo, _message);
                                    }
                                    return false;
                                }
                                else if (messageLowerCase == ClanManager.Command_rename)
                                {
                                    Phrases.Dict.TryGetValue(120, out string _phrase120);
                                    _phrase120 = _phrase120.Replace("{Command_Prefix1}", Chat_Command_Prefix1);
                                    _phrase120 = _phrase120.Replace("{Command_rename}", ClanManager.Command_rename);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase120 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else if (messageLowerCase.StartsWith(ClanManager.Command_rename))
                                {
                                    _message = _message.Replace(ClanManager.Command_rename + " ", "");
                                    ClanManager.ClanRename(_cInfo, _message);
                                    return false;
                                }
                                else if (messageLowerCase.StartsWith(ClanManager.Command_request))
                                {
                                    _message = _message.Replace(ClanManager.Command_request + " ", "");
                                    ClanManager.RequestToJoinClan(_cInfo, _message);
                                    return false;
                                }
                                else if (messageLowerCase == ClanManager.Command_clan_list)
                                {
                                    string _clanlist = ClanManager.GetClanList();
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _clanlist, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    return false;
                                }
                            }
                            if (VoteReward.IsEnabled && messageLowerCase == VoteReward.Command_reward)
                            {
                                VoteReward.Check(_cInfo);
                                return false;
                            }
                            if (Shutdown.IsEnabled && messageLowerCase == Shutdown.Command_shutdown)
                            {
                                Shutdown.NextShutdown(_cInfo);
                                return false;
                            }
                            if (AdminList.IsEnabled && messageLowerCase == AdminList.Command_adminlist)
                            {
                                AdminList.List(_cInfo);
                                return false;
                            }
                            if (Travel.IsEnabled && messageLowerCase == Travel.Command_travel)
                            {
                                Travel.Exec(_cInfo);
                                return false;
                            }
                            if (Market.IsEnabled && (messageLowerCase == Market.Command_marketback || messageLowerCase == Market.Command_mback))
                            {
                                if (Market.MarketPlayers.Contains(_cInfo.entityId))
                                {
                                    Market.SendBack(_cInfo);
                                    return false;
                                }
                            }
                            if (Market.IsEnabled && messageLowerCase == Market.Command_set)
                            {
                                Market.Set(_cInfo);
                                return false;
                            }
                            if (Market.IsEnabled && messageLowerCase == Market.Command_market)
                            {
                                Market.Exec(_cInfo);
                                return false;
                            }
                            if (Lobby.IsEnabled && (messageLowerCase == Lobby.Command_lobbyback || messageLowerCase == Lobby.Command_lback))
                            {
                                if (Lobby.LobbyPlayers.Contains(_cInfo.entityId))
                                {
                                    Lobby.SendBack(_cInfo);
                                    return false;
                                }
                            }
                            if (Lobby.IsEnabled && messageLowerCase == Lobby.Command_set)
                            {
                                Lobby.Set(_cInfo);
                                return false;
                            }
                            if (Lobby.IsEnabled && messageLowerCase == Lobby.Command_lobby)
                            {
                                Lobby.Exec(_cInfo);
                                return false;
                            }
                            if (Wallet.IsEnabled && messageLowerCase == Wallet.Command_wallet)
                            {
                                Wallet.CurrentValue(_cInfo);
                                return false;
                            }
                            if (Shop.IsEnabled && Wallet.IsEnabled && messageLowerCase.StartsWith(Shop.Command_shop_buy + " "))
                            {
                                _message = _message.Replace(Shop.Command_shop_buy + " ", "");
                                if (_message.Length == 1)
                                {
                                    Shop.PosCheck(_cInfo, _message, 3, 1);
                                }
                                else if (_message.Contains(" "))
                                {
                                    string[] _split = _message.Split(' ');
                                    string _id = _split[0];
                                    string _amount = _split[1];
                                    if (int.TryParse(_amount, out int _count))
                                    {
                                        Shop.PosCheck(_cInfo, _id, 3, _count);
                                    }
                                    else
                                    {
                                        Phrases.Dict.TryGetValue(353, out string _phrase353);
                                        _phrase353 = _phrase353.Replace("{Command_Prefix1}", Chat_Command_Prefix1);
                                        _phrase353 = _phrase353.Replace("{Command58}", Shop.Command_shop_buy);
                                        ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase353 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                                return false;
                            }
                            if (Shop.IsEnabled && Wallet.IsEnabled && messageLowerCase.StartsWith(Shop.Command_shop + " "))//show specific category
                            {
                                string _category = messageLowerCase.Replace(Shop.Command_shop + " ", "");
                                if (Shop.Categories.Contains(_category))
                                {
                                    Shop.PosCheck(_cInfo, _category, 2, 0);
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue(358, out string _phrase358);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase358 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                            if (Shop.IsEnabled && Wallet.IsEnabled && messageLowerCase == Shop.Command_shop)//show all categories
                            {
                                Shop.PosCheck(_cInfo, _message, 1, 0);
                                return false;
                            }
                            if (FriendTeleport.IsEnabled && messageLowerCase == FriendTeleport.Command_friend)
                            {
                                FriendTeleport.ListFriends(_cInfo);
                                return false;
                            }
                            if (FriendTeleport.IsEnabled && messageLowerCase.StartsWith(FriendTeleport.Command_friend + " "))
                            {
                                DateTime _date = DateTime.Now;
                                if (PersistentContainer.Instance.Players[_cInfo.playerId].LastFriendTele != null)
                                {
                                    _date = PersistentContainer.Instance.Players[_cInfo.playerId].LastFriendTele;
                                }
                                TimeSpan varTime = DateTime.Now - _date;
                                double fractionalMinutes = varTime.TotalMinutes;
                                int _timepassed = (int)fractionalMinutes;
                                if (_timepassed >= FriendTeleport.Delay_Between_Uses)
                                {
                                    _message = messageLowerCase.Replace(FriendTeleport.Command_friend + " ", "");
                                    FriendTeleport.Exec(_cInfo, _message);
                                }
                                else
                                {
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + "" + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                            if (FriendTeleport.IsEnabled && messageLowerCase == FriendTeleport.Command_accept && FriendTeleport.Dict.ContainsKey(_cInfo.entityId))
                            {
                                FriendTeleport.Dict.TryGetValue(_cInfo.entityId, out int _dictValue);
                                FriendTeleport.Dict1.TryGetValue(_cInfo.entityId, out DateTime _dict1Value);
                                TimeSpan varTime = DateTime.Now - _dict1Value;
                                double fractionalSeconds = varTime.TotalSeconds;
                                int _timepassed = (int)fractionalSeconds;
                                if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay && ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out DateTime _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        int _newTime = _timepassed / 2;
                                        _timepassed = _newTime;
                                    }
                                }
                                if (_timepassed <= 120)
                                {
                                    FriendTeleport.TeleFriend(_cInfo, _dictValue);
                                    FriendTeleport.Dict.Remove(_cInfo.entityId);
                                    FriendTeleport.Dict1.Remove(_cInfo.entityId);
                                }
                                else
                                {
                                    FriendTeleport.Dict.Remove(_cInfo.entityId);
                                    FriendTeleport.Dict1.Remove(_cInfo.entityId);
                                    Phrases.Dict.TryGetValue(372, out string _phrase372);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase372 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                            if (Died.IsEnabled && messageLowerCase == Died.Command_died)
                            {
                                Died.Exec(_cInfo);
                                return false;
                            }
                            if (WeatherVote.IsEnabled && messageLowerCase == WeatherVote.Command_weathervote)
                            {
                                if (!WeatherVote.VoteOpen)
                                {
                                    WeatherVote.CallForVote(_cInfo);
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue(878, out string _phrase878);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase878 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                            if (WeatherVote.IsEnabled && messageLowerCase == WeatherVote.Command_sun)
                            {
                                if (WeatherVote.VoteOpen)
                                {
                                    if (!WeatherVote.Snow.Contains(_cInfo.entityId) && !WeatherVote.Sun.Contains(_cInfo.entityId) && !WeatherVote.Rain.Contains(_cInfo.entityId))
                                    {
                                        WeatherVote.Sun.Add(_cInfo.entityId);
                                        Phrases.Dict.TryGetValue(887, out string _phrase887);
                                        ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase887 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                    else
                                    {
                                        Phrases.Dict.TryGetValue(888, out string _phrase888);
                                        ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase888 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue(889, out string _phrase889);
                                    _phrase889 = _phrase889.Replace("{Command_Prefix1}", Chat_Command_Prefix1);
                                    _phrase889 = _phrase889.Replace("{Command62}", WeatherVote.Command_weathervote);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase889 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                            if (WeatherVote.IsEnabled && messageLowerCase == WeatherVote.Command_rain)
                            {
                                if (WeatherVote.VoteOpen)
                                {
                                    if (!WeatherVote.Snow.Contains(_cInfo.entityId) && !WeatherVote.Sun.Contains(_cInfo.entityId) && !WeatherVote.Rain.Contains(_cInfo.entityId))
                                    {
                                        WeatherVote.Rain.Add(_cInfo.entityId);
                                        Phrases.Dict.TryGetValue(890, out string _phrase890);
                                        ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase890 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                    else
                                    {
                                        Phrases.Dict.TryGetValue(888, out string _phrase888);
                                        ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase888 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue(889, out string _phrase889);
                                    _phrase889 = _phrase889.Replace("{Command_Prefix1}", Chat_Command_Prefix1);
                                    _phrase889 = _phrase889.Replace("{Command62}", WeatherVote.Command_weathervote);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase889 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                            if (WeatherVote.IsEnabled && messageLowerCase == WeatherVote.Command_snow)
                            {
                                if (WeatherVote.VoteOpen)
                                {
                                    if (!WeatherVote.Snow.Contains(_cInfo.entityId) && !WeatherVote.Sun.Contains(_cInfo.entityId) && !WeatherVote.Rain.Contains(_cInfo.entityId))
                                    {
                                        WeatherVote.Snow.Add(_cInfo.entityId);
                                        Phrases.Dict.TryGetValue(891, out string _phrase891);
                                        ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase891 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                    else
                                    {
                                        Phrases.Dict.TryGetValue(888, out string _phrase888);
                                        ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase888 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue(889, out string _phrase889);
                                    _phrase889 = _phrase889.Replace("{Command_Prefix1}", Chat_Command_Prefix1);
                                    _phrase889 = _phrase889.Replace("{Command62}", WeatherVote.Command_weathervote);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase889 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                            if (RestartVote.IsEnabled && messageLowerCase == RestartVote.Command_restartvote)
                            {
                                if (!KickVote.VoteOpen && !RestartVote.VoteOpen && !MuteVote.VoteOpen)
                                {
                                    RestartVote.CallForVote1(_cInfo);
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue(451, out string _phrase451);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase451 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                            if (KickVote.IsEnabled && messageLowerCase.StartsWith(KickVote.Command_kickvote))
                            {
                                if (!KickVote.VoteOpen && !RestartVote.VoteOpen && !MuteVote.VoteOpen)
                                {
                                    if (messageLowerCase == KickVote.Command_kickvote)
                                    {
                                        KickVote.List(_cInfo);
                                    }
                                    else
                                    {
                                        _message = messageLowerCase.Replace(KickVote.Command_kickvote + " ", "");
                                        {
                                            KickVote.Vote(_cInfo, _message);
                                        }
                                    }
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue(722, out string _phrase722);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase722 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                            if ((KickVote.IsEnabled || RestartVote.IsEnabled || MuteVote.IsEnabled) && messageLowerCase == RestartVote.Command_yes)
                            {
                                if (KickVote.VoteOpen)
                                {
                                    if (!KickVote.Kick.Contains(_cInfo.entityId))
                                    {
                                        KickVote.Kick.Add(_cInfo.entityId);
                                        Phrases.Dict.TryGetValue(723, out string _phrase723);
                                        _phrase723 = _phrase723.Replace("{Value}", KickVote.Kick.Count.ToString());
                                        _phrase723 = _phrase723.Replace("{VotesNeeded}", KickVote.Votes_Needed.ToString());
                                        ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase723 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                    else
                                    {
                                        Phrases.Dict.TryGetValue(724, out string _phrase724);
                                        ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase724 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                    return false;
                                }
                                else if (RestartVote.VoteOpen)
                                {
                                    if (!RestartVote.Restart.Contains(_cInfo.entityId))
                                    {
                                        RestartVote.Restart.Add(_cInfo.entityId);
                                        Phrases.Dict.TryGetValue(453, out string _phrase453);
                                        _phrase453 = _phrase453.Replace("{Value}", RestartVote.Restart.Count.ToString());
                                        _phrase453 = _phrase453.Replace("{VotesNeeded}", RestartVote.Votes_Needed.ToString());
                                        ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase453 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                    else
                                    {
                                        Phrases.Dict.TryGetValue(454, out string _phrase454);
                                        ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase454 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                    return false;
                                }
                                else if (MuteVote.VoteOpen)
                                {
                                    if (!MuteVote.Votes.Contains(_cInfo.entityId))
                                    {
                                        MuteVote.Votes.Add(_cInfo.entityId);
                                        Phrases.Dict.TryGetValue(921, out string _phrase921);
                                        _phrase921 = _phrase921.Replace("{Value}", MuteVote.Votes.Count.ToString());
                                        _phrase921 = _phrase921.Replace("{VotesNeeded}", MuteVote.Votes_Needed.ToString());
                                        ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase921 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                    else
                                    {
                                        Phrases.Dict.TryGetValue(922, out string _phrase922);
                                        ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase922 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                    return false;
                                }
                            }
                            if (Auction.IsEnabled && messageLowerCase == Auction.Command_auction)
                            {
                                Auction.AuctionList(_cInfo);
                                return false;
                            }
                            if (Auction.IsEnabled && messageLowerCase.StartsWith(Auction.Command_auction_cancel + " "))
                            {
                                _message = messageLowerCase.Replace(Auction.Command_auction_cancel + " ", "");
                                Auction.CancelAuction(_cInfo, _message);
                                return false;
                            }
                            if (Auction.IsEnabled && messageLowerCase.StartsWith(Auction.Command_auction_buy + " "))
                            {
                                if (Wallet.IsEnabled)
                                {
                                    if (Auction.No_Admins)
                                    {
                                        if (GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo) <= Admin_Level)
                                        {
                                            Phrases.Dict.TryGetValue(633, out string _phrase633);
                                            ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase633 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                    }
                                    _message = messageLowerCase.Replace(Auction.Command_auction_buy + " ", "");
                                    {
                                        if (int.TryParse(_message, out int _purchase))
                                        {
                                            if (Auction.AuctionItems.ContainsKey(_purchase))
                                            {
                                                Auction.WalletCheck(_cInfo, _purchase);
                                            }
                                            else
                                            {
                                                Phrases.Dict.TryGetValue(634, out string _phrase634);
                                                ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase634 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue(635, out string _phrase635);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase635 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                            if (Auction.IsEnabled && Wallet.IsEnabled && messageLowerCase.StartsWith(Auction.Command_auction_sell + " "))
                            {
                                if (Wallet.IsEnabled)
                                {
                                    if (Auction.No_Admins)
                                    {
                                        if (GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo) > Admin_Level)
                                        {
                                            _message = messageLowerCase.Replace(Auction.Command_auction_sell + " ", "");
                                            Auction.CheckBox(_cInfo, _message);
                                        }
                                        else
                                        {
                                            Phrases.Dict.TryGetValue(633, out string _phrase633);
                                            ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase633 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                    }
                                    else
                                    {
                                        _message = messageLowerCase.Replace(Auction.Command_auction_sell + " ", "");
                                        Auction.CheckBox(_cInfo, _message);
                                    }
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue(635, out string _phrase635);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase635 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                            if (Fps.IsEnabled && messageLowerCase == Fps.Command_fps)
                            {
                                Fps.FPS(_cInfo);
                                return false;
                            }
                            if (Loc.IsEnabled && messageLowerCase == Loc.Command_loc)
                            {
                                Loc.Exec(_cInfo);
                                return false;
                            }
                            if (VehicleTeleport.IsEnabled)
                            {
                                if (VehicleTeleport.Bike && messageLowerCase == VehicleTeleport.Command_bike)
                                {
                                    VehicleTeleport.Exec(_cInfo, 1);
                                    return false;
                                }
                                else if (VehicleTeleport.Mini_Bike && messageLowerCase == VehicleTeleport.Command_minibike)
                                {
                                    VehicleTeleport.Exec(_cInfo, 2);
                                    return false;
                                }
                                else if (VehicleTeleport.Motor_Bike && messageLowerCase == VehicleTeleport.Command_motorbike)
                                {
                                    VehicleTeleport.Exec(_cInfo, 3);
                                    return false;
                                }
                                else if (VehicleTeleport.Jeep && messageLowerCase == VehicleTeleport.Command_jeep)
                                {
                                    VehicleTeleport.Exec(_cInfo, 4);
                                    return false;
                                }
                                else if (VehicleTeleport.Gyro && messageLowerCase == VehicleTeleport.Command_gyro)
                                {
                                    VehicleTeleport.Exec(_cInfo, 5);
                                    return false;
                                }
                            }
                            if (Report.IsEnabled && messageLowerCase.StartsWith(Report.Command_report + " "))
                            {
                                _message = messageLowerCase.Replace(Report.Command_report + " ", "");
                                Report.Check(_cInfo, _message);
                                return false;
                            }
                            if (Bounties.IsEnabled && messageLowerCase == Bounties.Command_bounty)
                            {
                                Bounties.BountyList(_cInfo);
                                return false;
                            }
                            if (Bounties.IsEnabled && messageLowerCase.StartsWith(Bounties.Command_bounty + " "))
                            {
                                _message = messageLowerCase.Replace(Bounties.Command_bounty + " ", "");
                                Bounties.NewBounty(_cInfo, _message);
                                return false;
                            }
                            if (Lottery.IsEnabled && messageLowerCase == Lottery.Command_lottery)
                            {
                                Lottery.Response(_cInfo);
                                return false;
                            }
                            if (Lottery.IsEnabled && messageLowerCase == Lottery.Command_lottery_enter)
                            {
                                Lottery.EnterLotto(_cInfo);
                                return false;
                            }
                            if (Lottery.IsEnabled && messageLowerCase.StartsWith(Lottery.Command_lottery + " "))
                            {
                                _message = _message.Replace(Lottery.Command_lottery + " ", "");
                                Lottery.NewLotto(_cInfo, _message);
                                return false;
                            }
                            if (NewSpawnTele.IsEnabled && NewSpawnTele.Return && messageLowerCase == NewSpawnTele.Command_ready)
                            {
                                NewSpawnTele.ReturnPlayer(_cInfo);
                                return false;
                            }
                            if (PlayerList.IsEnabled && messageLowerCase == PlayerList.Command_playerlist)
                            {
                                PlayerList.Exec(_cInfo);
                                return false;
                            }
                            if (Stuck.IsEnabled && messageLowerCase == Stuck.Command_stuck)
                            {
                                Stuck.Exec(_cInfo);
                                return false;
                            }
                            if (Poll.IsEnabled && PersistentContainer.Instance.PollOpen && messageLowerCase == Poll.Command_poll_yes)
                            {
                                Poll.Yes(_cInfo);
                                return false;
                            }
                            if (Poll.IsEnabled && PersistentContainer.Instance.PollOpen && messageLowerCase == Poll.Command_poll_no)
                            {
                                Poll.No(_cInfo);
                                return false;
                            }
                            if (Poll.IsEnabled && PersistentContainer.Instance.PollOpen && messageLowerCase == Poll.Command_poll)
                            {
                                string[] _pollData = PersistentContainer.Instance.PollData;
                                Phrases.Dict.TryGetValue(562, out string _phrase562);
                                _phrase562 = _phrase562.Replace("{Message}", _pollData[2]);
                                ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase562 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                Phrases.Dict.TryGetValue(561, out string _phrase561);
                                int _yes = 0, _no = 0;
                                Dictionary<string, bool> _pollVotes = PersistentContainer.Instance.PollVote;
                                foreach (var _vote in _pollVotes)
                                {
                                    if (_vote.Value)
                                    {
                                        _yes++;
                                    }
                                    else
                                    {
                                        _no++;
                                    }
                                }
                                _phrase561 = _phrase561.Replace("{YesVote}", _yes.ToString());
                                _phrase561 = _phrase561.Replace("{NoVote}", _no.ToString());
                                ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase561 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                return false;
                            }
                            if (Bank.IsEnabled && messageLowerCase == Bank.Command_bank)
                            {
                                Bank.CurrentBankAndId(_cInfo);
                                return false;
                            }
                            if (Bank.IsEnabled && messageLowerCase.StartsWith(Bank.Command_deposit + " "))
                            {
                                _message = messageLowerCase.Replace(Bank.Command_deposit + " ", "");
                                Bank.CheckLocation(_cInfo, _message, 1);
                                return false;
                            }
                            if (Bank.IsEnabled && messageLowerCase.StartsWith(Bank.Command_withdraw + " "))
                            {
                                _message = messageLowerCase.Replace(Bank.Command_withdraw + " ", "");
                                Bank.CheckLocation(_cInfo, _message, 2);
                                return false;
                            }
                            if (Bank.IsEnabled && Wallet.IsEnabled && Wallet.Bank_Transfers && messageLowerCase.StartsWith(Bank.Command_wallet_deposit + " "))
                            {
                                _message = messageLowerCase.Replace(Bank.Command_wallet_deposit + " ", "");
                                Bank.CheckLocation(_cInfo, _message, 3);
                                return false;
                            }
                            if (Bank.IsEnabled && Wallet.IsEnabled && Wallet.Bank_Transfers && messageLowerCase.StartsWith(Bank.Command_wallet_withdraw + " "))
                            {
                                _message = messageLowerCase.Replace(Bank.Command_wallet_withdraw + " ", "");
                                Bank.CheckLocation(_cInfo, _message, 4);
                                return false;
                            }
                            if (Bank.IsEnabled && Bank.Player_Transfers && messageLowerCase.StartsWith(Bank.Command_transfer + " "))
                            {
                                _message = messageLowerCase.Replace(Bank.Command_transfer + " ", "");
                                Bank.Transfer(_cInfo, _message);
                                return false;
                            }
                            if (Event.Invited && messageLowerCase == Event.Command_join)
                            {
                                Event.AddPlayer(_cInfo);
                                return false;
                            }
                            if (InfoTicker.IsEnabled && messageLowerCase == InfoTicker.Command_infoticker)
                            {
                                if (!InfoTicker.ExemptionList.Contains(_cInfo.playerId))
                                {
                                    InfoTicker.ExemptionList.Add(_cInfo.playerId);
                                    Phrases.Dict.TryGetValue(941, out string _phrase941);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase941 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else
                                {
                                    InfoTicker.ExemptionList.Remove(_cInfo.playerId);
                                    Phrases.Dict.TryGetValue(942, out string _phrase942);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase942 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                            if (Session.IsEnabled && messageLowerCase == Session.Command_session)
                            {
                                Session.Exec(_cInfo);
                                return false;
                            }
                            if (Waypoints.IsEnabled)
                            {
                                if (messageLowerCase == Waypoints.Command_go_way)
                                {
                                    if (Waypoints.Invite.ContainsKey(_cInfo.entityId))
                                    {
                                        Waypoints.FriendWaypoint(_cInfo);
                                        return false;
                                    }
                                }
                                if (messageLowerCase == Waypoints.Command_waypoint || messageLowerCase == Waypoints.Command_way || messageLowerCase == Waypoints.Command_wp)
                                {
                                    Waypoints.List(_cInfo);
                                    return false;
                                }
                                if (_message.StartsWith(Waypoints.Command_fwaypoint + " "))
                                {
                                    _message = messageLowerCase.Replace(Waypoints.Command_fwaypoint + " ", "");
                                    if (_message != " " || _message != "")
                                    {
                                        Waypoints.TeleDelay(_cInfo, _message, true);
                                        return false;
                                    }
                                }
                                else if (_message.StartsWith(Waypoints.Command_fway + " "))
                                {
                                    _message = messageLowerCase.Replace(Waypoints.Command_fway + " ", "");
                                    if (_message != " " || _message != "")
                                    {
                                        Waypoints.TeleDelay(_cInfo, _message, true);
                                        return false;
                                    }
                                }
                                else if (_message.StartsWith(Waypoints.Command_fwp + " "))
                                {
                                    _message = messageLowerCase.Replace(Waypoints.Command_fwp + " ", "");
                                    if (_message != " " || _message != "")
                                    {
                                        Waypoints.TeleDelay(_cInfo, _message, true);
                                        return false;
                                    }
                                }
                                if (messageLowerCase.StartsWith(Waypoints.Command_waypoint_save + " "))
                                {
                                    _message = messageLowerCase.Replace(Waypoints.Command_waypoint_save + " ", "");
                                    Waypoints.SaveClaimCheck(_cInfo, _message);
                                    return false;
                                }
                                else if (messageLowerCase.StartsWith(Waypoints.Command_way_save + " "))
                                {
                                    _message = messageLowerCase.Replace(Waypoints.Command_way_save + " ", "");
                                    Waypoints.SaveClaimCheck(_cInfo, _message);
                                    return false;
                                }
                                else if (messageLowerCase.StartsWith(Waypoints.Command_ws + " "))
                                {
                                    _message = messageLowerCase.Replace(Waypoints.Command_ws + " ", "");
                                    Waypoints.SaveClaimCheck(_cInfo, _message);
                                    return false;
                                }
                                if (messageLowerCase.StartsWith(Waypoints.Command_waypoint_del + " "))
                                {
                                    _message = messageLowerCase.Replace(Waypoints.Command_waypoint_del + " ", "");
                                    Waypoints.DelPoint(_cInfo, _message);
                                    return false;
                                }
                                else if (messageLowerCase.StartsWith(Waypoints.Command_way_del + " "))
                                {
                                    _message = messageLowerCase.Replace(Waypoints.Command_way_del + " ", "");
                                    Waypoints.DelPoint(_cInfo, _message);
                                    return false;
                                }
                                else if (messageLowerCase.StartsWith(Waypoints.Command_wd + " "))
                                {
                                    _message = messageLowerCase.Replace(Waypoints.Command_wd + " ", "");
                                    Waypoints.DelPoint(_cInfo, _message);
                                    return false;
                                }
                                if (messageLowerCase.StartsWith(Waypoints.Command_waypoint + " "))
                                {
                                    _message = messageLowerCase.Replace(Waypoints.Command_waypoint + " ", "");
                                    Waypoints.TeleDelay(_cInfo, _message, false);
                                    return false;
                                }
                                else if (messageLowerCase.StartsWith(Waypoints.Command_way + " "))
                                {
                                    _message = messageLowerCase.Replace(Waypoints.Command_way + " ", "");
                                    Waypoints.TeleDelay(_cInfo, _message, false);
                                    return false;
                                }
                                else if (messageLowerCase.StartsWith(Waypoints.Command_wp + " "))
                                {
                                    _message = messageLowerCase.Replace(Waypoints.Command_wp + " ", "");
                                    Waypoints.TeleDelay(_cInfo, _message, false);
                                    return false;
                                }
                            }
                            if (Whisper.IsEnabled && (messageLowerCase.StartsWith(Whisper.Command_pmessage) || messageLowerCase.StartsWith(Whisper.Command_pm)))
                            {
                                Whisper.Send(_cInfo, _message);
                                return false;
                            }
                            if (Whisper.IsEnabled && (messageLowerCase.StartsWith(Whisper.Command_rmessage) || messageLowerCase.StartsWith(Whisper.Command_rm)))
                            {
                                Whisper.Reply(_cInfo, _message);
                                return false;
                            }
                            if (CustomCommands.IsEnabled && CustomCommands.Dict.ContainsKey(_message = messageLowerCase))
                            {
                                CustomCommands.Exec(_cInfo, messageLowerCase);
                                return false;
                            }
                            if (ReservedSlots.IsEnabled && messageLowerCase == ReservedSlots.Command_reserved)
                            {
                                ReservedSlots.ReservedStatus(_cInfo);
                                return false;
                            }
                            if (Prayer.IsEnabled && messageLowerCase == Prayer.Command_pray)
                            {
                                Prayer.Exec(_cInfo);
                                return false;
                            }
                            if (ScoutPlayer.IsEnabled && (messageLowerCase == ScoutPlayer.Command_scoutplayer || messageLowerCase == ScoutPlayer.Command_scout))
                            {
                                ScoutPlayer.Exec(_cInfo);
                                return false;
                            }
                            if (Hardcore.IsEnabled && messageLowerCase == Hardcore.Command_hardcore_on)
                            {
                                if (Hardcore.Optional)
                                {
                                    if (PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreEnabled)
                                    {
                                        EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                        if (_player != null)
                                        {
                                            int _deaths = XUiM_Player.GetDeaths(_player);
                                            int _lifeTime = (int)_player.lifetime;
                                            string[] _harcorestats = { _cInfo.playerName, _player.Score.ToString(), _lifeTime.ToString(), _deaths.ToString(), "0" };
                                            PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreStats = _harcorestats;
                                            PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreEnabled = true;
                                            PersistentContainer.DataChange = true;
                                            Phrases.Dict.TryGetValue(601, out string _phrase601);
                                            _phrase601 = _phrase601.Replace("{Command_Prefix1}", Chat_Command_Prefix1);
                                            _phrase601 = _phrase601.Replace("{Command_hardcore}", Hardcore.Command_hardcore);
                                            ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase601 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                    }
                                    else
                                    {
                                        Phrases.Dict.TryGetValue(602, out string _phrase602);
                                        _phrase602 = _phrase602.Replace("{Command_Prefix1}", Chat_Command_Prefix1);
                                        _phrase602 = _phrase602.Replace("{Command_hardcore}", Hardcore.Command_hardcore);
                                        ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase602 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue(602, out string _phrase602);
                                    _phrase602 = _phrase602.Replace("{Command_Prefix1}", Chat_Command_Prefix1);
                                    _phrase602 = _phrase602.Replace("{Command_hardcore}", Hardcore.Command_hardcore);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase602 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                            if (Hardcore.IsEnabled && messageLowerCase == Hardcore.Command_hardcore)
                            {
                                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                if (_player != null)
                                {
                                    if (Hardcore.Optional)
                                    {
                                        if (PersistentContainer.Instance.Players[_cInfo.playerId].HardcoreEnabled)
                                        {
                                            Hardcore.Check(_cInfo, _player);
                                        }
                                        else
                                        {
                                            Phrases.Dict.TryGetValue(600, out string _phrase600);
                                            ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase600 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                    }
                                    else
                                    {
                                        Hardcore.Check(_cInfo, _player);
                                    }
                                }
                                return false;
                            }
                            if (ExitCommand.IsEnabled && (messageLowerCase == ExitCommand.Command_exit || messageLowerCase == ExitCommand.Command_quit))
                            {
                                if (ExitCommand.Players.ContainsKey(_cInfo.entityId))
                                {
                                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                    if (_player != null)
                                    {
                                        ExitCommand.Players[_cInfo.entityId] = _player.position;
                                        Timers.ExitWithCommand(_cInfo.entityId, ExitCommand.Exit_Time);
                                        Phrases.Dict.TryGetValue(674, out string _phrase674);
                                        _phrase674 = _phrase674.Replace("{Time}", ExitCommand.Exit_Time.ToString());
                                        ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase674 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                                else if (GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo) <= ExitCommand.Admin_Level)
                                {
                                    ExitCommand.Disconnect(_cInfo);
                                }
                                return false;
                            }
                            if (ChatColorPrefix.IsEnabled)
                            {
                                if (messageLowerCase == ChatColorPrefix.Command_ccp && ChatColorPrefix.Players.ContainsKey(_cInfo.playerId))
                                {
                                    ChatColorPrefix.ShowColorAndExpiry(_cInfo);
                                    return false;
                                }
                                else if (ChatColorPrefix.Custom_Color && messageLowerCase.StartsWith(ChatColorPrefix.Command_ccpr + " ") == ChatColorPrefix.Players.ContainsKey(_cInfo.playerId))
                                {
                                    _message = messageLowerCase.Replace(Waypoints.Command_wp + " ", "");
                                    ChatColorPrefix.SetPrefixColor(_cInfo, messageLowerCase);
                                    return false;
                                }
                                else if (ChatColorPrefix.Rotate && messageLowerCase == ChatColorPrefix.Command_ccpr)
                                {
                                    ChatColorPrefix.RotatePrefixColor(_cInfo);
                                    return false;
                                }
                                else if (ChatColorPrefix.Custom_Color && messageLowerCase.StartsWith(ChatColorPrefix.Command_ccnr + " ") == ChatColorPrefix.Players.ContainsKey(_cInfo.playerId))
                                {

                                    ChatColorPrefix.SetNameColor(_cInfo, messageLowerCase);
                                    return false;
                                }
                                else if (ChatColorPrefix.Rotate && messageLowerCase == ChatColorPrefix.Command_ccnr && ChatColorPrefix.Players.ContainsKey(_cInfo.playerId))
                                {
                                    ChatColorPrefix.RotateNameColor(_cInfo);
                                    return false;
                                }
                            }
                        }
                        if (AdminChat.IsEnabled && _message.ToLower().StartsWith(AdminChat.Command_admin + " "))
                        {
                            _message = _message.ToLower().Replace(AdminChat.Command_admin + " ", "");
                            AdminChat.SendAdmins(_cInfo, _message);
                            return false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ChatHook.Hook: {0}", e.Message));
            }
            return true;
        }

        public static void PrepMessage(ClientInfo _cInfo, string _message, int _senderId, string _mainName, string _prefix, string _nameColor, string _prefixColor, EChatType _type, List<int> _recipientEntityIds)
        {
            try
            {
                if (_type == EChatType.Friends)
                {
                    _prefix = _prefix.Insert(0, "(Friends)");
                    if (!string.IsNullOrEmpty(Friend_Chat_Color) && Friend_Chat_Color.StartsWith("[") && Friend_Chat_Color.EndsWith("]"))
                    {
                        _prefix = _prefix.Insert(0, Friend_Chat_Color);
                    }
                    _prefix = _prefix.Insert(_prefix.Length, "[-]");
                }
                else if (_type == EChatType.Party)
                {
                    _prefix = _prefix.Insert(0, "(Party)");
                    if (!string.IsNullOrEmpty(Party_Chat_Color) && Party_Chat_Color.StartsWith("[") && Party_Chat_Color.EndsWith("]"))
                    {
                        _prefix = _prefix.Insert(0, Party_Chat_Color);
                    }
                    _prefix = _prefix.Insert(_prefix.Length, "[-]");
                }
                else if (!string.IsNullOrEmpty(_prefixColor) && _prefixColor.StartsWith("[") && _prefixColor.EndsWith("]"))
                {
                    if (_prefixColor.Contains(","))
                    {
                        int _prefixCount = _prefix.Count();
                        string[] _prefixColorSplit = _prefixColor.Split(',');
                        for (int i = _prefixColorSplit.Length; i >= 1; i--)
                        {
                            if (i - 1 < _prefixCount)
                            {
                                _prefix = _prefix.Insert(i - 1, _prefixColorSplit[i - 1]);
                            }
                        }
                    }
                    else
                    {
                        _prefix = _prefix.Insert(0, _prefixColor);
                    }
                    _prefix = _prefix.Insert(_prefix.Length, "[-]");
                }
                if (!string.IsNullOrEmpty(_mainName) && _nameColor.StartsWith("[") && _nameColor.EndsWith("]"))
                {
                    if (_nameColor.Contains(","))
                    {
                        int _nameCount = _mainName.Count();
                        string[] _nameColorSplit = _nameColor.Split(',');
                        for (int i = _nameColorSplit.Length; i >= 1; i--)
                        {
                            if (i - 1 < _nameCount)
                            {
                                _mainName = _mainName.Insert(i - 1, _nameColorSplit[i - 1]);
                            }
                        }
                    }
                    else
                    {
                        _mainName = _mainName.Insert(0, _nameColor);
                    }
                    _mainName = _mainName.Insert(_mainName.Length, "[-]");
                }
                if (Message_Color_Enabled)
                {
                    _message = _message.Insert(0, Message_Color);
                    _message = _message.Insert(_message.Length, "[-]");
                }
                else
                {
                    _message = _message.Insert(0, "[FFFFFF]");
                    _message = _message.Insert(_message.Length, "[-]");
                }
                if (_prefix.Length > 0)
                {
                    ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0} {1}", _prefix, _mainName), _type, _recipientEntityIds);
                }
                else
                {
                    ChatMessage(_cInfo, _message, _senderId, _mainName, _type, _recipientEntityIds);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ChatHook.PrepMessage: {0}", e.Message));
            }
        }

        public static void ChatMessage(ClientInfo _cInfo, string _message, int _senderId, string _name, EChatType _type, List<int> _recipientEntityIds)
        {
            try
            {
                if (_type == EChatType.Whisper)
                {
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, _senderId, _message, _name, false, null));
                }
                else
                {
                    if (_cInfo != null && Mute.IsEnabled && Mute.PrivateMutes.ContainsKey(_cInfo.entityId))
                    {
                        if (_recipientEntityIds != null)
                        {
                            if (_recipientEntityIds.Count == 0)
                            {
                                List<EntityPlayer> _players = PersistentOperations.PlayerList();
                                for (int i = 0; i < _players.Count; i++)
                                {
                                    _recipientEntityIds.Add(_players[i].entityId);
                                }
                            }
                        }
                        else
                        {
                            List<int> _recipients = new List<int>();
                            List<EntityPlayer> _players = PersistentOperations.PlayerList();
                            for (int i = 0; i < _players.Count; i++)
                            {
                                _recipients.Add(_players[i].entityId);
                            }
                            _recipientEntityIds = _recipients;
                        }
                        if (Mute.PrivateMutes.TryGetValue(_cInfo.entityId, out List<int> _muted))
                        {
                            for (int i = 0; i < _muted.Count; i++)
                            {
                                if (_recipientEntityIds != null && _recipientEntityIds.Contains(_muted[i]))
                                {
                                    _recipientEntityIds.Remove(_muted[i]);
                                    if (_recipientEntityIds.Count == 0)
                                    {
                                        _recipientEntityIds = null;
                                    }
                                }
                            }
                        }
                    }
                    if (_type == EChatType.Global)
                    {
                        GameManager.Instance.ChatMessageServer(_cInfo, EChatType.Global, -1, _message, _name, false, _recipientEntityIds);
                    }
                    else if (_type == EChatType.Friends)
                    {
                        GameManager.Instance.ChatMessageServer(_cInfo, EChatType.Friends, -1, _message, _name, false, _recipientEntityIds);
                    }
                    else if (_type == EChatType.Party)
                    {
                        GameManager.Instance.ChatMessageServer(_cInfo, EChatType.Party, -1, _message, _name, false, _recipientEntityIds);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ChatHook.ChatMessage: {0}", e.Message));
            }
        }
    }
}