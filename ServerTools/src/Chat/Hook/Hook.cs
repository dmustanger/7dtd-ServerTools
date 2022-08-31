using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerTools
{
    public class ChatHook
    {
        public static bool ChatFlood = false, Normal_Player_Color_Prefix = false, Message_Color_Enabled = false, Alter_Message_Color = false, Role_Play = false, Passthrough = true;
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
                    if (Mute.IsEnabled && Mute.Mutes.Contains(_cInfo.PlatformId.CombinedString) || Mute.Mutes.Contains(_cInfo.CrossplatformId.CombinedString))
                    {
                        if (Mute.Block_Commands && (_message.StartsWith(Chat_Command_Prefix1) || _message.StartsWith(Chat_Command_Prefix2)))
                        {
                            Phrases.Dict.TryGetValue("Mute10", out string _phrase);
                            ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return false;
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("Mute11", out string _phrase);
                            ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            return false;
                        }
                    }
                    if (Badwords.IsEnabled)
                    {
                        if (Badwords.Invalid_Name)
                        {
                            for (int i = 0; i < Badwords.Dict.Count; i++)
                            {
                                if (_mainName.ToLower().Contains(Badwords.Dict[i]))
                                {
                                    _mainName = _mainName.ToLower().Replace(Badwords.Dict[i], "***");
                                }
                            }
                        }
                        for (int i = 0; i < Badwords.Dict.Count; i++)
                        {
                            if (_message.ToLower().Contains(Badwords.Dict[i]))
                            {
                                _message = _message.ToLower().Replace(Badwords.Dict[i], "***");
                            }
                        }
                    }
                    if (!Jail.IsEnabled || !Jail.Jailed.Contains(_cInfo.CrossplatformId.CombinedString))
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
                                ChatLog.Exec(_message, _mainName, _type);
                            }
                            if (ChatFlood)
                            {
                                if (_message.Length >= Max_Length)
                                {
                                    Phrases.Dict.TryGetValue("ChatFloodProtection2", out string _phrase);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    return false;
                                }
                                else if (Messages_Per_Min > 1)
                                {
                                    if (ChatFloodLock.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                                    {
                                        ChatFloodLock.TryGetValue(_cInfo.CrossplatformId.CombinedString, out DateTime _lockTime);
                                        TimeSpan varTime = DateTime.Now - _lockTime;
                                        double fractionalSeconds = varTime.TotalSeconds;
                                        if ((int)fractionalSeconds >= Wait_Time)
                                        {
                                            ChatFloodLock.Remove(_cInfo.CrossplatformId.CombinedString);
                                        }
                                        else
                                        {
                                            Phrases.Dict.TryGetValue("ChatFloodProtection1", out string _phrase);
                                            ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                            return false;
                                        }
                                    }
                                    if (ChatFloodCount.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                                    {
                                        ChatFloodCount.TryGetValue(_cInfo.CrossplatformId.CombinedString, out int _count);
                                        ChatFloodTime.TryGetValue(_cInfo.CrossplatformId.CombinedString, out DateTime _chatTime);
                                        TimeSpan varTime = DateTime.Now - _chatTime;
                                        double fractionalSeconds = varTime.TotalSeconds;
                                        if ((int)fractionalSeconds >= 60)
                                        {
                                            ChatFloodCount[_cInfo.CrossplatformId.CombinedString] = 1;
                                            ChatFloodTime[_cInfo.CrossplatformId.CombinedString] = DateTime.Now;
                                        }
                                        else
                                        {
                                            ChatFloodCount[_cInfo.CrossplatformId.CombinedString] += 1;
                                            if (ChatFloodCount[_cInfo.CrossplatformId.CombinedString] == Messages_Per_Min + 1)
                                            {
                                                ChatFloodCount.Remove(_cInfo.CrossplatformId.CombinedString);
                                                ChatFloodTime.Remove(_cInfo.CrossplatformId.CombinedString);
                                                ChatFloodLock.Add(_cInfo.CrossplatformId.CombinedString, DateTime.Now);
                                                Phrases.Dict.TryGetValue("ChatFloodProtection1", out string phrase);
                                                ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                                return false;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ChatFloodCount.Add(_cInfo.CrossplatformId.CombinedString, 1);
                                        ChatFloodTime.Add(_cInfo.CrossplatformId.CombinedString, DateTime.Now);
                                    }
                                }
                            }
                            if (ChatColor.IsEnabled && (ChatColor.Players.ContainsKey(_cInfo.PlatformId.CombinedString) || ChatColor.Players.ContainsKey(_cInfo.CrossplatformId.CombinedString)))
                            {
                                if (ChatColor.ExpireDate.TryGetValue(_cInfo.PlatformId.CombinedString, out DateTime dt))
                                {
                                    if (DateTime.Now < dt)
                                    {
                                        ChatColor.Players.TryGetValue(_cInfo.PlatformId.CombinedString, out string[] colorPrefix);
                                        if (ClanManager.IsEnabled && ClanManager.ClanMember.Contains(_cInfo.CrossplatformId.CombinedString))
                                        {
                                            string clanName = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanName;
                                            PrepMessage(_cInfo, _message, _senderId, _mainName, colorPrefix[1], clanName, colorPrefix[3], _type, _recipientEntityIds);
                                        }
                                        else
                                        {
                                            PrepMessage(_cInfo, _message, _senderId, _mainName, colorPrefix[1], colorPrefix[2], colorPrefix[3], _type, _recipientEntityIds);
                                        }
                                        return false;
                                    }
                                }
                                else if (ChatColor.ExpireDate.TryGetValue(_cInfo.CrossplatformId.CombinedString, out dt))
                                {
                                    if (DateTime.Now < dt)
                                    {
                                        ChatColor.Players.TryGetValue(_cInfo.CrossplatformId.CombinedString, out string[] colorPrefix);
                                        if (ClanManager.IsEnabled && ClanManager.ClanMember.Contains(_cInfo.CrossplatformId.CombinedString))
                                        {
                                            string clanName = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanName;
                                            PrepMessage(_cInfo, _message, _senderId, _mainName, colorPrefix[1], clanName, colorPrefix[3], _type, _recipientEntityIds);
                                        }
                                        else
                                        {
                                            PrepMessage(_cInfo, _message, _senderId, _mainName, colorPrefix[1], colorPrefix[2], colorPrefix[3], _type, _recipientEntityIds);
                                        }
                                        return false;
                                    }
                                }
                            }
                            else if (ClanManager.IsEnabled && ClanManager.ClanMember.Contains(_cInfo.CrossplatformId.CombinedString))
                            {
                                if (!string.IsNullOrEmpty(PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanName))
                                {
                                    string clanName = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].ClanName;
                                    if (Normal_Player_Color_Prefix)
                                    {
                                        PrepMessage(_cInfo, _message, _senderId, _mainName, Normal_Player_Name_Color, clanName, Normal_Player_Prefix_Color, _type, _recipientEntityIds);
                                    }
                                    else
                                    {
                                        PrepMessage(_cInfo, _message, _senderId, _mainName, "", clanName, "", _type, _recipientEntityIds);
                                    }
                                    return false;
                                }
                            }
                            else if (Normal_Player_Color_Prefix)
                            {
                                PrepMessage(_cInfo, _message, _senderId, _mainName, Normal_Player_Name_Color, Normal_Player_Prefix, Normal_Player_Prefix_Color, _type, _recipientEntityIds);
                                return false;
                            }
                        }
                        if ((_message.StartsWith(Chat_Command_Prefix1) || _message.StartsWith(Chat_Command_Prefix2)) && !PersistentOperations.BlockChatCommands.Contains(_cInfo))
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
                                    Phrases.Dict.TryGetValue("MuteVote10", out string _phrase);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                            if (messageLowerCase == PersistentOperations.Command_commands)
                            {
                                PersistentOperations.CommandsList(_cInfo);
                                if (CustomCommands.IsEnabled)
                                {
                                    CustomCommands.CustomCommandList(_cInfo);
                                }
                                if (GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo.PlatformId) <= Admin_Level ||
                                    GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo.CrossplatformId) <= Admin_Level)
                                {
                                    PersistentOperations.AdminCommandList(_cInfo);
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
                            if (Jail.IsEnabled && messageLowerCase == Jail.Command_set)
                            {
                                Jail.SetJail(_cInfo);
                                return false;
                            }
                            if (NewSpawnTele.IsEnabled && messageLowerCase == NewSpawnTele.Command_setspawn)
                            {
                                NewSpawnTele.SetNewSpawnTele(_cInfo);
                                return false;
                            }
                            if (AnimalTracking.IsEnabled && (messageLowerCase == AnimalTracking.Command_trackanimal || messageLowerCase == AnimalTracking.Command_track))
                            {
                                AnimalTracking.Exec(_cInfo);
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
                                    Phrases.Dict.TryGetValue("Clan44", out string _phrase);
                                    _phrase = _phrase.Replace("{Command_Prefix1}", Chat_Command_Prefix1);
                                    _phrase = _phrase.Replace("{Command_add}", ClanManager.Command_add);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                                    Phrases.Dict.TryGetValue("Clan45", out string _phrase);
                                    _phrase = _phrase.Replace("{Command_Prefix1}", Chat_Command_Prefix1);
                                    _phrase = _phrase.Replace("{Command_invite}", ClanManager.Command_invite);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                                    Phrases.Dict.TryGetValue("Clan46", out string _phrase);
                                    _phrase = _phrase.Replace("{Command_Prefix1}", Chat_Command_Prefix1);
                                    _phrase = _phrase.Replace("{Command_remove}", ClanManager.Command_remove);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                                    Phrases.Dict.TryGetValue("Clan47", out string _phrase);
                                    _phrase = _phrase.Replace("{Command_Prefix1}", Chat_Command_Prefix1);
                                    _phrase = _phrase.Replace("{Command_promote}", ClanManager.Command_promote);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else if (messageLowerCase.StartsWith(ClanManager.Command_promote))
                                {
                                    _message = _message.Replace(ClanManager.Command_promote + " ", "");
                                    ClanManager.PromoteMember(_cInfo, _message);
                                    return false;
                                }
                                else if (messageLowerCase == ClanManager.Command_demote)
                                {
                                    Phrases.Dict.TryGetValue("Clan48", out string _phrase);
                                    _phrase = _phrase.Replace("{Command_Prefix1}", Chat_Command_Prefix1);
                                    _phrase = _phrase.Replace("{Command_demote}", ClanManager.Command_demote);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                                    Phrases.Dict.TryGetValue("Clan49", out string _phrase);
                                    _phrase = _phrase.Replace("{Command_Prefix1}", Chat_Command_Prefix1);
                                    _phrase = _phrase.Replace("{Command_chat}", ClanManager.Command_chat);
                                    _phrase = _phrase.Replace("{Command_cc}", ClanManager.Command_cc);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else if (messageLowerCase == ClanManager.Command_chat || messageLowerCase == ClanManager.Command_cc && ClanManager.ClanMember.Contains(_cInfo.CrossplatformId.CombinedString))
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
                                    Phrases.Dict.TryGetValue("Clan50", out string _phrase);
                                    _phrase = _phrase.Replace("{Command_Prefix1}", Chat_Command_Prefix1);
                                    _phrase = _phrase.Replace("{Command_rename}", ClanManager.Command_rename);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                            if (Voting.IsEnabled && messageLowerCase == Voting.Command_reward)
                            {
                                Voting.Check(_cInfo);
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
                            if (Shop.IsEnabled && Wallet.IsEnabled)
                            {
                                if (messageLowerCase.StartsWith(Shop.Command_shop_buy + " "))
                                {
                                    _message = _message.Replace(Shop.Command_shop_buy + " ", "");
                                    if (!_message.Contains(" "))
                                    {
                                        Shop.PosCheck(_cInfo, _message, 3, 1);
                                    }
                                    else if (_message.Contains(" "))
                                    {
                                        string[] _split = _message.Split(' ');
                                        string _id = _split[0];
                                        if (int.TryParse(_split[1], out int _count))
                                        {
                                            Shop.PosCheck(_cInfo, _id, 3, _count);
                                        }
                                        else
                                        {
                                            Phrases.Dict.TryGetValue("Shop13", out string phrase);
                                            phrase = phrase.Replace("{Command_Prefix1}", Chat_Command_Prefix1);
                                            phrase = phrase.Replace("{Command_shop_buy}", Shop.Command_shop_buy);
                                            ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                    }
                                    return false;
                                }
                                if (messageLowerCase.StartsWith(Shop.Command_shop + " "))
                                {
                                    string _category = messageLowerCase.Replace(Shop.Command_shop + " ", "");
                                    Shop.PosCheck(_cInfo, _category, 2, 0);
                                    return false;
                                }
                                if (messageLowerCase == Shop.Command_shop)
                                {
                                    Shop.PosCheck(_cInfo, _message, 1, 0);
                                    return false;
                                }
                            }
                            if (FriendTeleport.IsEnabled && messageLowerCase == FriendTeleport.Command_friend)
                            {
                                FriendTeleport.ListFriends(_cInfo);
                                return false;
                            }
                            if (FriendTeleport.IsEnabled && messageLowerCase.StartsWith(FriendTeleport.Command_friend + " "))
                            {
                                DateTime _date = DateTime.Now;
                                if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastFriendTele != null)
                                {
                                    _date = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].LastFriendTele;
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
                                int timepassed = (int)fractionalSeconds;
                                if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay && ReservedSlots.Dict.ContainsKey(_cInfo.PlatformId.CombinedString) || ReservedSlots.Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString))
                                {
                                    if (ReservedSlots.Dict.TryGetValue(_cInfo.PlatformId.CombinedString, out DateTime dt))
                                    {
                                        if (DateTime.Now < dt)
                                        {
                                            int newTime = timepassed / 2;
                                            timepassed = newTime;
                                        }
                                    }
                                    else if (ReservedSlots.Dict.TryGetValue(_cInfo.CrossplatformId.CombinedString, out dt))
                                    {
                                        if (DateTime.Now < dt)
                                        {
                                            int newTime = timepassed / 2;
                                            timepassed = newTime;
                                        }
                                    }
                                }
                                if (timepassed <= 120)
                                {
                                    FriendTeleport.TeleFriend(_cInfo, _dictValue);
                                    FriendTeleport.Dict.Remove(_cInfo.entityId);
                                    FriendTeleport.Dict1.Remove(_cInfo.entityId);
                                }
                                else
                                {
                                    FriendTeleport.Dict.Remove(_cInfo.entityId);
                                    FriendTeleport.Dict1.Remove(_cInfo.entityId);
                                    Phrases.Dict.TryGetValue("FriendTeleport12", out string _phrase);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                            if (Died.IsEnabled && messageLowerCase == Died.Command_died)
                            {
                                Died.Exec(_cInfo);
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
                                    Phrases.Dict.TryGetValue("RestartVote11", out string _phrase);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                                    Phrases.Dict.TryGetValue("KickVote12", out string _phrase);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                                        Phrases.Dict.TryGetValue("KickVote13", out string _phrase);
                                        _phrase = _phrase.Replace("{Value}", KickVote.Kick.Count.ToString());
                                        _phrase = _phrase.Replace("{VotesNeeded}", KickVote.Votes_Needed.ToString());
                                        ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                    else
                                    {
                                        Phrases.Dict.TryGetValue("KickVote14", out string _phrase);
                                        ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                    return false;
                                }
                                else if (RestartVote.VoteOpen)
                                {
                                    if (!RestartVote.Restart.Contains(_cInfo.entityId))
                                    {
                                        RestartVote.Restart.Add(_cInfo.entityId);
                                        Phrases.Dict.TryGetValue("RestartVote13", out string _phrase);
                                        _phrase = _phrase.Replace("{Value}", RestartVote.Restart.Count.ToString());
                                        _phrase = _phrase.Replace("{VotesNeeded}", RestartVote.Votes_Needed.ToString());
                                        ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                    else
                                    {
                                        Phrases.Dict.TryGetValue("RestartVote14", out string _phrase);
                                        ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                    return false;
                                }
                                else if (MuteVote.VoteOpen)
                                {
                                    if (!MuteVote.Votes.Contains(_cInfo.entityId))
                                    {
                                        MuteVote.Votes.Add(_cInfo.entityId);
                                        Phrases.Dict.TryGetValue("MuteVote11", out string _phrase);
                                        _phrase = _phrase.Replace("{Value}", MuteVote.Votes.Count.ToString());
                                        _phrase = _phrase.Replace("{VotesNeeded}", MuteVote.Votes_Needed.ToString());
                                        ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                    else
                                    {
                                        Phrases.Dict.TryGetValue("MuteVote12", out string _phrase);
                                        ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                                        if (GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo.PlatformId) <= Admin_Level ||
                                            GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo.CrossplatformId) <= Admin_Level)
                                        {
                                            Phrases.Dict.TryGetValue("Auction13", out string phrase);
                                            ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                    }
                                    _message = messageLowerCase.Replace(Auction.Command_auction_buy + " ", "");
                                    {
                                        if (int.TryParse(_message, out int purchase))
                                        {
                                            if (Auction.AuctionItems.ContainsKey(purchase))
                                            {
                                                Auction.WalletCheck(_cInfo, purchase);
                                            }
                                            else
                                            {
                                                Phrases.Dict.TryGetValue("Auction14", out string phrase);
                                                ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue("Auction15", out string _phrase);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                            if (Auction.IsEnabled && Wallet.IsEnabled && messageLowerCase.StartsWith(Auction.Command_auction_sell + " "))
                            {
                                if (Wallet.IsEnabled)
                                {
                                    if (Auction.No_Admins)
                                    {
                                        if (GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo.PlatformId) > Admin_Level ||
                                            GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo.CrossplatformId) > Admin_Level)
                                        {
                                            _message = messageLowerCase.Replace(Auction.Command_auction_sell + " ", "");
                                            Auction.SellItem(_cInfo, _message);
                                        }
                                        else
                                        {
                                            Phrases.Dict.TryGetValue("Auction13", out string phrase);
                                            ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                    }
                                    else
                                    {
                                        _message = messageLowerCase.Replace(Auction.Command_auction_sell + " ", "");
                                        Auction.SellItem(_cInfo, _message);
                                    }
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue("Auction15", out string _phrase);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                            if (Fps.IsEnabled && messageLowerCase == Fps.Command_fps)
                            {
                                Fps.Exec(_cInfo);
                                return false;
                            }
                            if (Loc.IsEnabled && messageLowerCase == Loc.Command_loc)
                            {
                                Loc.Exec(_cInfo);
                                return false;
                            }
                            if (VehicleRecall.IsEnabled)
                            {
                                if (messageLowerCase.StartsWith(VehicleRecall.Command_vehicle_remove + " "))
                                {
                                    messageLowerCase = messageLowerCase.Replace(VehicleRecall.Command_vehicle_remove + " ", "");
                                    VehicleRecall.RemoveVehicle(_cInfo, messageLowerCase);
                                    return false;
                                }
                                if (messageLowerCase.StartsWith(VehicleRecall.Command_vehicle_save))
                                {
                                    VehicleRecall.SaveVehicle(_cInfo);
                                    return false;
                                }
                                if (messageLowerCase.StartsWith(VehicleRecall.Command_vehicle + " "))
                                {
                                    messageLowerCase = messageLowerCase.Replace(VehicleRecall.Command_vehicle + " ", "");
                                    VehicleRecall.Exec(_cInfo, messageLowerCase);
                                    return false;
                                }
                                else if (messageLowerCase == VehicleRecall.Command_vehicle)
                                {
                                    VehicleRecall.List(_cInfo);
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
                            if (PlayerList.IsEnabled && messageLowerCase == PlayerList.Command_playerlist || messageLowerCase == PlayerList.Command_plist)
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
                                Phrases.Dict.TryGetValue("Poll2", out string _phrase);
                                _phrase = _phrase.Replace("{Message}", _pollData[2]);
                                ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                Phrases.Dict.TryGetValue("Poll1", out _phrase);
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
                                _phrase = _phrase.Replace("{YesVote}", _yes.ToString());
                                _phrase = _phrase.Replace("{NoVote}", _no.ToString());
                                ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                                if (!InfoTicker.ExemptionList.Contains(_cInfo.CrossplatformId.CombinedString))
                                {
                                    InfoTicker.ExemptionList.Add(_cInfo.CrossplatformId.CombinedString);
                                    Phrases.Dict.TryGetValue("InfoTicker1", out string _phrase);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else
                                {
                                    InfoTicker.ExemptionList.Remove(_cInfo.CrossplatformId.CombinedString);
                                    Phrases.Dict.TryGetValue("InfoTicker2", out string _phrase);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
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
                            if (CustomCommands.IsEnabled && CustomCommands.Dict.ContainsKey(messageLowerCase))
                            {
                                CustomCommands.InitiateCommand(_cInfo, messageLowerCase);
                                return false;
                            }
                            if (messageLowerCase == PersistentOperations.Command_expire)
                            {
                                if (ReservedSlots.IsEnabled && (ReservedSlots.Dict.ContainsKey(_cInfo.PlatformId.CombinedString) || ReservedSlots.Dict.ContainsKey(_cInfo.CrossplatformId.CombinedString)))
                                {
                                    ReservedSlots.ReservedStatus(_cInfo, _cInfo.PlatformId, _cInfo.CrossplatformId);
                                }
                                if (ChatColor.IsEnabled && (ChatColor.Players.ContainsKey(_cInfo.PlatformId.CombinedString) || ChatColor.Players.ContainsKey(_cInfo.CrossplatformId.CombinedString)))
                                {
                                    ChatColor.ShowColorAndExpiry(_cInfo);
                                }
                                if (LoginNotice.IsEnabled && (LoginNotice.Dict1.ContainsKey(_cInfo.PlatformId.CombinedString) || LoginNotice.Dict1.ContainsKey(_cInfo.CrossplatformId.CombinedString)))
                                {
                                    LoginNotice.LoginStatus(_cInfo);
                                }
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
                                    EntityPlayer player = PersistentOperations.GetEntityPlayer(_cInfo.entityId);
                                    if (player != null)
                                    {
                                        string[] stats = { _cInfo.playerName, "0", "0" };
                                        PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreStats = stats;
                                        PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreEnabled = true;
                                        PersistentContainer.DataChange = true;
                                        Phrases.Dict.TryGetValue("Hardcore11", out string _phrase);
                                        _phrase = _phrase.Replace("{Command_Prefix1}", Chat_Command_Prefix1);
                                        _phrase = _phrase.Replace("{Command_hardcore}", Hardcore.Command_hardcore);
                                        ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                    else
                                    {
                                        Phrases.Dict.TryGetValue("Hardcore12", out string _phrase);
                                        _phrase = _phrase.Replace("{Command_Prefix1}", Chat_Command_Prefix1);
                                        _phrase = _phrase.Replace("{Command_hardcore}", Hardcore.Command_hardcore);
                                        ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue("Hardcore12", out string _phrase);
                                    _phrase = _phrase.Replace("{Command_Prefix1}", Chat_Command_Prefix1);
                                    _phrase = _phrase.Replace("{Command_hardcore}", Hardcore.Command_hardcore);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                            if (Hardcore.IsEnabled && messageLowerCase == Hardcore.Command_hardcore)
                            {
                                EntityPlayer player = PersistentOperations.GetEntityPlayer(_cInfo.entityId);
                                if (player != null)
                                {
                                    if (Hardcore.Optional)
                                    {
                                        if (PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].HardcoreEnabled)
                                        {
                                            Hardcore.Check(_cInfo, player);
                                        }
                                        else
                                        {
                                            Phrases.Dict.TryGetValue("Hardcore10", out string _phrase);
                                            ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                    }
                                    else
                                    {
                                        Hardcore.Check(_cInfo, player);
                                    }
                                }
                                return false;
                            }
                            if (ExitCommand.IsEnabled && (messageLowerCase == ExitCommand.Command_exit || messageLowerCase == ExitCommand.Command_quit))
                            {
                                if (ExitCommand.Players.ContainsKey(_cInfo.entityId))
                                {
                                    EntityPlayer player = PersistentOperations.GetEntityPlayer(_cInfo.entityId);
                                    if (player != null)
                                    {
                                        ExitCommand.Players[_cInfo.entityId] = player.position;
                                        Timers.ExitWithCommand(_cInfo.entityId, ExitCommand.Exit_Time);
                                        Phrases.Dict.TryGetValue("ExitCommand4", out string phrase);
                                        phrase = phrase.Replace("{Time}", ExitCommand.Exit_Time.ToString());
                                        ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                                else if (GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo.PlatformId) <= ExitCommand.Admin_Level ||
                                    GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo.CrossplatformId) <= ExitCommand.Admin_Level)
                                {
                                    ExitCommand.Disconnect(_cInfo);
                                }
                                return false;
                            }
                            if (ChatColor.IsEnabled)
                            {
                                if (messageLowerCase == ChatColor.Command_ccc && (ChatColor.Players.ContainsKey(_cInfo.PlatformId.CombinedString) || ChatColor.Players.ContainsKey(_cInfo.CrossplatformId.CombinedString)))
                                {
                                    ChatColor.ShowColorAndExpiry(_cInfo);
                                    return false;
                                }
                                else if (ChatColor.Custom_Color && messageLowerCase.StartsWith(ChatColor.Command_ccpr + " ") && (ChatColor.Players.ContainsKey(_cInfo.PlatformId.CombinedString) || ChatColor.Players.ContainsKey(_cInfo.CrossplatformId.CombinedString)))
                                {
                                    messageLowerCase = messageLowerCase.Replace(ChatColor.Command_ccpr + " ", "");
                                    ChatColor.SetPrefixColor(_cInfo, messageLowerCase);
                                    return false;
                                }
                                else if (ChatColor.Rotate && messageLowerCase == ChatColor.Command_ccpr)
                                {
                                    ChatColor.RotatePrefixColor(_cInfo);
                                    return false;
                                }
                                else if (ChatColor.Custom_Color && messageLowerCase.StartsWith(ChatColor.Command_ccnr + " ") && (ChatColor.Players.ContainsKey(_cInfo.PlatformId.CombinedString) || ChatColor.Players.ContainsKey(_cInfo.CrossplatformId.CombinedString)))
                                {
                                    messageLowerCase = messageLowerCase.Replace(ChatColor.Command_ccnr + " ", "");
                                    ChatColor.SetNameColor(_cInfo, messageLowerCase);
                                    return false;
                                }
                                else if (ChatColor.Rotate && messageLowerCase == ChatColor.Command_ccnr && (ChatColor.Players.ContainsKey(_cInfo.PlatformId.CombinedString) || ChatColor.Players.ContainsKey(_cInfo.CrossplatformId.CombinedString)))
                                {
                                    ChatColor.RotateNameColor(_cInfo);
                                    return false;
                                }
                            }
                            if (Gamble.IsEnabled && Wallet.IsEnabled)
                            {
                                if (messageLowerCase == Gamble.Command_gamble)
                                {
                                    Gamble.Exec(_cInfo);
                                    return false;
                                }
                                else if (messageLowerCase == Gamble.Command_gamble_bet)
                                {
                                    Gamble.Bet(_cInfo);
                                    return false;
                                }
                                else if (messageLowerCase == Gamble.Command_gamble_payout)
                                {
                                    Gamble.Payout(_cInfo);
                                    return false;
                                }
                            }
                            if (AutoPartyInvite.IsEnabled)
                            {
                                if (messageLowerCase == AutoPartyInvite.Command_party)
                                {
                                    AutoPartyInvite.Party(_cInfo);
                                    return false;
                                }
                                else if (messageLowerCase.StartsWith(AutoPartyInvite.Command_party_add + " "))
                                {
                                    messageLowerCase = messageLowerCase.Replace(AutoPartyInvite.Command_party_add + " ", "");
                                    AutoPartyInvite.Add(_cInfo, messageLowerCase);
                                    return false;
                                }
                                else if (messageLowerCase.StartsWith(AutoPartyInvite.Command_party_remove + " "))
                                {
                                    messageLowerCase = messageLowerCase.Replace(AutoPartyInvite.Command_party_remove + " ", "");
                                    AutoPartyInvite.Remove(_cInfo, messageLowerCase);
                                    return false;
                                }
                            }
                            if (AllocsMap.IsEnabled)
                            {
                                if (messageLowerCase == AllocsMap.Command_map)
                                {
                                    AllocsMap.Exec(_cInfo);
                                    return false;
                                }
                            }
                            if (DiscordLink.IsEnabled)
                            {
                                if (messageLowerCase == DiscordLink.Command_discord)
                                {
                                    DiscordLink.Exec(_cInfo);
                                    return false;
                                }
                            }
                            if (Voting.IsEnabled)
                            {
                                if (messageLowerCase == Voting.Command_vote)
                                {
                                    Voting.Exec(_cInfo);
                                    return false;
                                }
                            }
                            if (BlockPickup.IsEnabled)
                            {
                                if (messageLowerCase == BlockPickup.Command_pickup)
                                {
                                    BlockPickup.Exec(_cInfo);
                                    return false;
                                }
                            }
                            if (Wall.IsEnabled)
                            {
                                if (messageLowerCase == Wall.Command_wall)
                                {
                                    Wall.Exec(_cInfo);
                                    return false;
                                }
                            }
                            if (Bed.IsEnabled)
                            {
                                if (messageLowerCase == Bed.Command_bed)
                                {
                                    Bed.Exec(_cInfo);
                                    return false;
                                }
                            }
                            if (RIO.IsEnabled && WebAPI.IsEnabled)
                            {
                                if (messageLowerCase == RIO.Command_rio)
                                {
                                    RIO.Exec(_cInfo);
                                    return false;
                                }
                            }
                            if (messageLowerCase == PersistentOperations.Command_overlay)
                            {
                                PersistentOperations.Overlay(_cInfo);
                                return false;
                            }
                            if (!Passthrough)
                            {
                                return false;
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

        private static void PrepMessage(ClientInfo _cInfo, string _message, int _senderId, string _mainName, string _nameColor, string _prefix, string _prefixColor, EChatType _type, List<int> _recipientEntityIds)
        {
            try
            {
                if (_type == EChatType.Friends)
                {
                    _prefix = _prefix.Insert(0, "(Friends)");
                    if (Friend_Chat_Color.StartsWith("[") && Friend_Chat_Color.EndsWith("]"))
                    {
                        _prefix = _prefix.Insert(0, Friend_Chat_Color);
                    }
                    _prefix = _prefix.Insert(_prefix.Length, "[-]");
                }
                else if (_type == EChatType.Party)
                {
                    _prefix = _prefix.Insert(0, "(Party)");
                    if (Party_Chat_Color.StartsWith("[") && Party_Chat_Color.EndsWith("]"))
                    {
                        _prefix = _prefix.Insert(0, Party_Chat_Color);
                    }
                    _prefix = _prefix.Insert(_prefix.Length, "[-]");
                }
                else if (_prefix != "" && _prefixColor.StartsWith("[") && _prefixColor.EndsWith("]"))
                {
                    if (_prefixColor.Contains(","))
                    {
                        bool complete = false;
                        int prefixIndex = 0;
                        int colorCount = 0;
                        string lastColor = "";
                        string[] colors = _prefixColor.Split(',');
                        for (int i = 0; i < 20; i++)
                        {
                            if (complete)
                            {
                                break;
                            }
                            for (int j = 0; j < colors.Length; j++)
                            {
                                if (prefixIndex < _prefix.Length)
                                {
                                    if (_prefix[prefixIndex] != ' ')
                                    {
                                        if (lastColor != colors[j])
                                        {
                                            _prefix = _prefix.Insert(prefixIndex, colors[j]);
                                            prefixIndex += 9;
                                            colorCount++;
                                            lastColor = colors[j];
                                        }
                                        else
                                        {
                                            prefixIndex += 1;
                                        }
                                    }
                                }
                                else
                                {
                                    for (int k = 0; k < colorCount; k++)
                                    {
                                        _prefix = _prefix.Insert(prefixIndex, "[-]");
                                    }
                                    complete = true;
                                    break;
                                }
                            }
                            if (!complete)
                            {
                                if (prefixIndex < _prefix.Length)
                                {
                                    colors.Reverse();
                                }
                                else
                                {
                                    for (int k = 0; k < colorCount; k++)
                                    {
                                        _prefix = _prefix.Insert(prefixIndex, "[-]");
                                    }
                                    complete = true;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        _prefix = _prefix.Insert(0, _prefixColor);
                        _prefix = _prefix.Insert(_prefix.Length, "[-]");
                    }
                }
                if (_nameColor != "" && _nameColor.StartsWith("[") && _nameColor.EndsWith("]"))
                {
                    if (_nameColor.Contains(","))
                    {
                        bool complete = false;
                        int nameIndex = 0;
                        int colorCount = 0;
                        string lastColor = "";
                        string[] colors = _nameColor.Split(',');
                        for (int i = 0; i < 20; i++)
                        {
                            if (complete)
                            {
                                break;
                            }
                            for (int j = 0; j < colors.Length; j++)
                            {
                                if (nameIndex < _mainName.Length)
                                {
                                    if (_mainName[nameIndex] != ' ')
                                    {
                                        if (lastColor != colors[j])
                                        {
                                            _mainName = _mainName.Insert(nameIndex, colors[j]);
                                            nameIndex += 9;
                                            colorCount++;
                                            lastColor = colors[j];
                                        }
                                        else
                                        {
                                            nameIndex += 1;
                                        }
                                    }
                                    else
                                    {
                                        nameIndex += 1;
                                    }
                                }
                                else
                                {
                                    for (int k = 0; k < colorCount; k++)
                                    {
                                        _mainName = _mainName.Insert(nameIndex, "[-]");
                                    }
                                    complete = true;
                                    break;
                                }
                            }
                            if (!complete)
                            {
                                if (nameIndex < _mainName.Length)
                                {
                                    colors.Reverse();
                                }
                                else
                                {
                                    for (int k = 0; k < colorCount; k++)
                                    {
                                        _mainName = _mainName.Insert(nameIndex, "[-]");
                                    }
                                    complete = true;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        _mainName = _mainName.Insert(0, _nameColor);
                        _mainName = _mainName.Insert(_mainName.Length, "[-]");
                    }
                }
                if (Message_Color_Enabled && Message_Color != "" && Message_Color.StartsWith("[") && Message_Color.EndsWith("]"))
                {
                    _message = _message.Insert(0, Message_Color);
                    _message = _message.Insert(_message.Length, "[-]");
                }
                if (_prefix != "")
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
                if (_message.Contains("U+") || _name.Contains("U+"))
                {
                    return;
                }
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
                                List<EntityPlayer> players = PersistentOperations.ListPlayers();
                                for (int i = 0; i < players.Count; i++)
                                {
                                    _recipientEntityIds.Add(players[i].entityId);
                                }
                            }
                        }
                        else
                        {
                            List<int> recipients = new List<int>();
                            List<EntityPlayer> players = PersistentOperations.ListPlayers();
                            for (int i = 0; i < players.Count; i++)
                            {
                                recipients.Add(players[i].entityId);
                            }
                            _recipientEntityIds = recipients;
                        }
                        if (Mute.PrivateMutes.TryGetValue(_cInfo.entityId, out List<int> muted))
                        {
                            for (int i = 0; i < muted.Count; i++)
                            {
                                if (_recipientEntityIds != null && _recipientEntityIds.Contains(muted[i]))
                                {
                                    _recipientEntityIds.Remove(muted[i]);
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
                        if (BotResponse.IsEnabled)
                        {
                            string messageToLower = _message.ToLower();
                            List<string> responses = BotResponse.Dict.Keys.ToList();
                            for (int i = 0; i < responses.Count; i++)
                            {
                                if (responses[i] == messageToLower)
                                {
                                    BotResponse.Dict.TryGetValue(responses[i], out string response);
                                    if (BotResponse.Whisper)
                                    {
                                        GameManager.Instance.ChatMessageServer(_cInfo, EChatType.Whisper, -1, Config.Chat_Response_Color + response + "[-]", Config.Server_Response_Name, false, null);
                                    }
                                    else
                                    {
                                        GameManager.Instance.ChatMessageServer(_cInfo, EChatType.Global, -1, Config.Chat_Response_Color + response + "[-]", Config.Server_Response_Name, false, null);
                                    }
                                    break;
                                }
                            }
                        }
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