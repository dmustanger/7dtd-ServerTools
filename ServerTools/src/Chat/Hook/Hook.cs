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
                            if (Homes.IsEnabled)
                            {
                                if (_message.ToLower() == Homes.Command1)
                                {
                                    Homes.List(_cInfo);
                                    return false;
                                }
                                else if (_message.ToLower().StartsWith(Homes.Command2 + " "))
                                {
                                    _message = _message.ToLower().Replace(Homes.Command2 + " ", "");
                                    Homes.TeleDelay(_cInfo, _message, true);
                                    return false;
                                }
                                else if (_message.ToLower().StartsWith(Homes.Command3 + " "))
                                {
                                    _message = _message.ToLower().Replace(Homes.Command3 + " ", "");
                                    Homes.SaveClaimCheck(_cInfo, _message);
                                    return false;
                                }
                                else if (_message.ToLower().StartsWith(Homes.Command4 + " "))
                                {
                                    _message = _message.ToLower().Replace(Homes.Command4 + " ", "");
                                    Homes.DelHome(_cInfo, _message);
                                    return false;
                                }
                                else if (_message.ToLower() == Homes.Command5)
                                {
                                    if (Homes.Invite.ContainsKey(_cInfo.entityId))
                                    {
                                        Homes.FriendHome(_cInfo);
                                        return false;
                                    }
                                }
                                else if (_message.ToLower().StartsWith(Homes.Command6 + " "))
                                {
                                    _message = _message.ToLower().Replace(Homes.Command6 + " ", "");
                                    Homes.SaveClaimCheck(_cInfo, _message);
                                    return false;
                                }
                                else if (_message.ToLower().StartsWith(Homes.Command1 + " "))
                                {
                                    _message = _message.ToLower().Replace(Homes.Command1 + " ", "");
                                    Homes.TeleDelay(_cInfo, _message, false);
                                    return false;
                                }
                            }
                            if (Hardcore.IsEnabled && _message.ToLower() == Hardcore.Command11)
                            {
                                Hardcore.TopThree(_cInfo);
                                return false;
                            }
                            if (Hardcore.IsEnabled && _message.ToLower() == Hardcore.Command12)
                            {
                                Hardcore.Score(_cInfo);
                                return false;
                            }
                            if (Hardcore.IsEnabled && Hardcore.Max_Extra_Lives > 0 && Wallet.IsEnabled && _message.ToLower() == Hardcore.Command101)
                            {
                                Hardcore.BuyLife(_cInfo);
                                return false;
                            }
                            if (MuteVote.IsEnabled && Mute.IsEnabled && _message.ToLower().StartsWith(MuteVote.Command67))
                            {
                                if (!KickVote.VoteOpen && !RestartVote.VoteOpen && !MuteVote.VoteOpen)
                                {
                                    if (_message.ToLower() == MuteVote.Command67)
                                    {
                                        MuteVote.List(_cInfo);
                                    }
                                    else
                                    {
                                        _message = _message.ToLower().Replace(MuteVote.Command67 + " ", "");
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
                            if (Mute.IsEnabled && _message.ToLower() == Mute.Command119)
                            {
                                Mute.List(_cInfo);
                                return false;
                            }
                            if (Mute.IsEnabled && _message.ToLower().StartsWith(Mute.Command13 + " "))
                            {
                                Mute.Add(_cInfo, _message);
                                return false;
                            }
                            if (Mute.IsEnabled && _message.ToLower().StartsWith(Mute.Command14 + " "))
                            {
                                Mute.Remove(_cInfo, _message);
                                return false;
                            }
                            if (_message.ToLower() == CustomCommands.Command15)
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
                            if (Day7.IsEnabled && (_message.ToLower() == Day7.Command16 || _message.ToLower() == Day7.Command17))
                            {
                                Day7.GetInfo(_cInfo);
                                return false;
                            }
                            if (Bloodmoon.IsEnabled && (_message.ToLower() == Bloodmoon.Command18 || _message.ToLower() == Bloodmoon.Command19))
                            {
                                Bloodmoon.Exec(_cInfo);
                                return false;
                            }
                            if (Suicide.IsEnabled && (_message.ToLower() == Suicide.Command20 || _message.ToLower() == Suicide.Command21 || _message.ToLower() == Suicide.Command22 || _message.ToLower() == Suicide.Command23))
                            {
                                Suicide.Exec(_cInfo);
                                return false;
                            }
                            if (Gimme.IsEnabled && (_message.ToLower() == Gimme.Command24 || _message.ToLower() == Gimme.Command25))
                            {
                                Gimme.Exec(_cInfo);
                                return false;
                            }
                            if (Jail.IsEnabled && (_message.ToLower() == Jail.Command26 || _message.ToLower().StartsWith(Jail.Command27) || _message.ToLower().StartsWith(Jail.Command28)))
                            {
                                if (_message.ToLower() == Jail.Command26)
                                {
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _message, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    Jail.SetJail(_cInfo);
                                }
                                else if (_message.ToLower().StartsWith(Jail.Command27))
                                {
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _message, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    Jail.PutInJail(_cInfo, _message);
                                }
                                else if (_message.ToLower().StartsWith(Jail.Command28))
                                {
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _message, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    Jail.RemoveFromJail(_cInfo, _message);
                                }
                                return false;
                            }
                            if (NewSpawnTele.IsEnabled && _message.ToLower() == NewSpawnTele.Command29)
                            {
                                NewSpawnTele.SetNewSpawnTele(_cInfo);
                                return false;
                            }
                            if (Animals.IsEnabled && (_message.ToLower() == Animals.Command30 || _message.ToLower() == Animals.Command31))
                            {
                                Animals.Exec(_cInfo);
                                return false;
                            }
                            if (FirstClaimBlock.IsEnabled && _message.ToLower() == FirstClaimBlock.Command32)
                            {
                                FirstClaimBlock.FirstClaim(_cInfo);
                                return false;
                            }
                            if (ClanManager.IsEnabled)
                            {
                                if (_message.ToLower() == ClanManager.Command33)
                                {
                                    Phrases.Dict.TryGetValue(114, out string _phrase114);
                                    _phrase114 = _phrase114.Replace("{CommandPrivate}", Chat_Command_Prefix1);
                                    _phrase114 = _phrase114.Replace("{Command33}", ClanManager.Command33);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase114 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    return false;
                                }
                                else if (_message.ToLower().StartsWith(ClanManager.Command33))
                                {
                                    _message = _message.Replace(ClanManager.Command33 + " ", "");
                                    ClanManager.AddClan(_cInfo, _message);
                                    return false;
                                }
                                else if (_message.ToLower() == ClanManager.Command34)
                                {
                                    ClanManager.RemoveClan(_cInfo);
                                    return false;
                                }
                                else if (_message.ToLower() == ClanManager.Command35)
                                {
                                    Phrases.Dict.TryGetValue(115, out string _phrase115);
                                    _phrase115 = _phrase115.Replace("{CommandPrivate}", Chat_Command_Prefix1);
                                    _phrase115 = _phrase115.Replace("{Command35}", ClanManager.Command35);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase115 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    return false;
                                }
                                else if (_message.ToLower().StartsWith(ClanManager.Command35))
                                {
                                    _message = _message.Replace(ClanManager.Command35 + " ", "");
                                    ClanManager.InviteMember(_cInfo, _message);
                                    return false;
                                }
                                else if (_message.ToLower() == ClanManager.Command36)
                                {
                                    ClanManager.InviteAccept(_cInfo);
                                    return false;
                                }
                                else if (_message.ToLower() == ClanManager.Command37)
                                {
                                    ClanManager.InviteDecline(_cInfo);
                                    return false;
                                }
                                else if (_message.ToLower() == ClanManager.Command38)
                                {
                                    Phrases.Dict.TryGetValue(116, out string _phrase116);
                                    _phrase116 = _phrase116.Replace("{CommandPrivate}", Chat_Command_Prefix1);
                                    _phrase116 = _phrase116.Replace("{Command38}", ClanManager.Command38);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase116 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    return false;
                                }
                                else if (_message.ToLower().StartsWith(ClanManager.Command38))
                                {
                                    _message = _message.Replace(ClanManager.Command38 + " ", "");
                                    ClanManager.RemoveMember(_cInfo, _message);
                                    return false;
                                }
                                else if (_message.ToLower() == ClanManager.Command39)
                                {
                                    Phrases.Dict.TryGetValue(117, out string _phrase117);
                                    _phrase117 = _phrase117.Replace("{CommandPrivate}", Chat_Command_Prefix1);
                                    _phrase117 = _phrase117.Replace("{Command39}", ClanManager.Command39);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase117 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else if (_message.ToLower().StartsWith(ClanManager.Command39))
                                {
                                    _message = _message.Replace(ClanManager.Command39 + " ", "");
                                    ClanManager.PromoteMember(_cInfo, _message);
                                    return false;
                                }
                                else if (_message.ToLower() == ClanManager.Command40)
                                {
                                    Phrases.Dict.TryGetValue(118, out string _phrase118);
                                    _phrase118 = _phrase118.Replace("{CommandPrivate}", Chat_Command_Prefix1);
                                    _phrase118 = _phrase118.Replace("{Command40}", ClanManager.Command40);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase118 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    return false;
                                }
                                else if (_message.ToLower().StartsWith(ClanManager.Command40))
                                {
                                    _message = _message.Replace(ClanManager.Command40 + " ", "");
                                    ClanManager.DemoteMember(_cInfo, _message);
                                    return false;
                                }
                                else if (_message.ToLower() == ClanManager.Command41)
                                {
                                    ClanManager.LeaveClan(_cInfo);
                                    return false;
                                }
                                else if (_message.ToLower() == ClanManager.Command42)
                                {
                                    string _clanCommands = ClanManager.GetChatCommands(_cInfo);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _clanCommands, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    return false;
                                }
                                else if (_message.ToLower() == ClanManager.Command43 || _message.ToLower() == ClanManager.Command124)
                                {
                                    Phrases.Dict.TryGetValue(119, out string _phrase119);
                                    _phrase119 = _phrase119.Replace("{CommandPrivate}", Chat_Command_Prefix1);
                                    _phrase119 = _phrase119.Replace("{Command43}", ClanManager.Command43);
                                    _phrase119 = _phrase119.Replace("{Command124}", ClanManager.Command124);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase119 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else if (_message.ToLower() == ClanManager.Command43 || _message.ToLower() == ClanManager.Command124 && ClanManager.ClanMember.Contains(_cInfo.playerId))
                                {
                                    if (_message.ToLower().StartsWith(ClanManager.Command43))
                                    {
                                        _message = _message.ToLower().Replace(ClanManager.Command43 + " ", "");
                                        ClanManager.Clan(_cInfo, _message);
                                    }
                                    else
                                    {
                                        _message = _message.ToLower().Replace(ClanManager.Command124 + " ", "");
                                        ClanManager.Clan(_cInfo, _message);
                                    }
                                    return false;
                                }
                                else if (_message.ToLower() == ClanManager.Command44)
                                {
                                    Phrases.Dict.TryGetValue(120, out string _phrase120);
                                    _phrase120 = _phrase120.Replace("{CommandPrivate}", Chat_Command_Prefix1);
                                    _phrase120 = _phrase120.Replace("{Command44}", ClanManager.Command44);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase120 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else if (_message.ToLower().StartsWith(ClanManager.Command44))
                                {
                                    _message = _message.Replace(ClanManager.Command44 + " ", "");
                                    ClanManager.ClanRename(_cInfo, _message);
                                    return false;
                                }
                                else if (_message.ToLower().StartsWith(ClanManager.Command45))
                                {
                                    _message = _message.Replace(ClanManager.Command45 + " ", "");
                                    ClanManager.RequestToJoinClan(_cInfo, _message);
                                    return false;
                                }
                                else if (_message.ToLower() == ClanManager.Command125)
                                {
                                    string _clanlist = ClanManager.GetClanList();
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _clanlist, -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    return false;
                                }
                            }
                            if (VoteReward.IsEnabled && _message.ToLower() == VoteReward.Command46)
                            {
                                VoteReward.Check(_cInfo);
                                return false;
                            }
                            if (Shutdown.IsEnabled && _message.ToLower() == Shutdown.Command47)
                            {
                                Shutdown.NextShutdown(_cInfo);
                                return false;
                            }
                            if (AdminList.IsEnabled && _message.ToLower() == AdminList.Command48)
                            {
                                AdminList.List(_cInfo);
                                return false;
                            }
                            if (Travel.IsEnabled && _message.ToLower() == Travel.Command49)
                            {
                                Travel.Exec(_cInfo);
                                return false;
                            }
                            if (Zones.IsEnabled && _message.ToLower() == Zones.Command50)
                            {
                                if (Zones.Victim.ContainsKey(_cInfo.entityId))
                                {
                                    Zones.ReturnToPosition(_cInfo);
                                    return false;
                                }
                            }
                            if (Market.IsEnabled && (_message.ToLower() == Market.Command51 || _message.ToLower() == Market.Command52))
                            {
                                if (Market.MarketPlayers.Contains(_cInfo.entityId))
                                {
                                    Market.SendBack(_cInfo);
                                    return false;
                                }
                            }
                            if (Market.IsEnabled && _message.ToLower() == Market.Command102)
                            {
                                Market.Set(_cInfo);
                                return false;
                            }
                            if (Market.IsEnabled && _message.ToLower() == Market.Command103)
                            {
                                Market.Exec(_cInfo);
                                return false;
                            }
                            if (Lobby.IsEnabled && (_message.ToLower() == Lobby.Command53 || _message.ToLower() == Lobby.Command54))
                            {
                                if (Lobby.LobbyPlayers.Contains(_cInfo.entityId))
                                {
                                    Lobby.SendBack(_cInfo);
                                    return false;
                                }
                            }
                            if (Lobby.IsEnabled && _message.ToLower() == Lobby.Command87)
                            {
                                Lobby.Set(_cInfo);
                                return false;
                            }
                            if (Lobby.IsEnabled && _message.ToLower() == Lobby.Command88)
                            {
                                Lobby.Exec(_cInfo);
                                return false;
                            }
                            if (Zones.IsEnabled && Jail.IsEnabled && _message.ToLower() == Jail.Command55)
                            {
                                if (Zones.Forgive.ContainsKey(_cInfo.entityId))
                                {
                                    Jail.Forgive(_cInfo);
                                    return false;
                                }
                            }
                            if (Wallet.IsEnabled && _message.ToLower() == Wallet.Command56)
                            {
                                Wallet.CurrentValue(_cInfo);
                                return false;
                            }
                            if (Shop.IsEnabled && Wallet.IsEnabled && _message.ToLower().StartsWith(Shop.Command58 + " "))
                            {
                                _message = _message.Replace(Shop.Command58 + " ", "");
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
                                        _phrase353 = _phrase353.Replace("{CommandPrivate}", Chat_Command_Prefix1);
                                        _phrase353 = _phrase353.Replace("{Command58}", Shop.Command58);
                                        ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase353 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                                return false;
                            }
                            if (Shop.IsEnabled && Wallet.IsEnabled && _message.ToLower().StartsWith(Shop.Command57 + " "))//show specific category
                            {
                                string _category = _message.ToLower().Replace(Shop.Command57 + " ", "");
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
                            if (Shop.IsEnabled && Wallet.IsEnabled && _message.ToLower() == Shop.Command57)//show all categories
                            {
                                Shop.PosCheck(_cInfo, _message, 1, 0);
                                return false;
                            }
                            if (FriendTeleport.IsEnabled && _message.ToLower() == FriendTeleport.Command59)
                            {
                                FriendTeleport.ListFriends(_cInfo);
                                return false;
                            }
                            if (FriendTeleport.IsEnabled && _message.ToLower().StartsWith(FriendTeleport.Command59 + " "))
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
                                    _message = _message.ToLower().Replace(FriendTeleport.Command59 + " ", "");
                                    FriendTeleport.Exec(_cInfo, _message);
                                }
                                else
                                {
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + "" + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                            if (FriendTeleport.IsEnabled && _message.ToLower() == FriendTeleport.Command60 && FriendTeleport.Dict.ContainsKey(_cInfo.entityId))
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
                            if (Died.IsEnabled && _message.ToLower() == Died.Command61)
                            {
                                Died.Exec(_cInfo);
                                return false;
                            }
                            if (WeatherVote.IsEnabled && _message.ToLower() == WeatherVote.Command62)
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
                            if (WeatherVote.IsEnabled && _message.ToLower() == WeatherVote.Command63)
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
                                    _phrase889 = _phrase889.Replace("{CommandPrivate}", Chat_Command_Prefix1);
                                    _phrase889 = _phrase889.Replace("{Command62}", WeatherVote.Command62);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase889 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                            if (WeatherVote.IsEnabled && _message.ToLower() == WeatherVote.Command64)
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
                                    _phrase889 = _phrase889.Replace("{CommandPrivate}", Chat_Command_Prefix1);
                                    _phrase889 = _phrase889.Replace("{Command62}", WeatherVote.Command62);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase889 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                            if (WeatherVote.IsEnabled && _message.ToLower() == WeatherVote.Command65)
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
                                    _phrase889 = _phrase889.Replace("{CommandPrivate}", Chat_Command_Prefix1);
                                    _phrase889 = _phrase889.Replace("{Command62}", WeatherVote.Command62);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase889 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                            if (RestartVote.IsEnabled && _message.ToLower() == RestartVote.Command66)
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
                            if (KickVote.IsEnabled && _message.ToLower().StartsWith(KickVote.Command68))
                            {
                                if (!KickVote.VoteOpen && !RestartVote.VoteOpen && !MuteVote.VoteOpen)
                                {
                                    if (_message.ToLower() == KickVote.Command68)
                                    {
                                        KickVote.List(_cInfo);
                                    }
                                    else
                                    {
                                        _message = _message.ToLower().Replace(KickVote.Command68 + " ", "");
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
                            if ((KickVote.IsEnabled || RestartVote.IsEnabled || MuteVote.IsEnabled) && _message.ToLower() == RestartVote.Command70)
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
                            if (Auction.IsEnabled && _message.ToLower() == Auction.Command71)
                            {
                                Auction.AuctionList(_cInfo);
                                return false;
                            }
                            if (Auction.IsEnabled && _message.ToLower().StartsWith(Auction.Command72 + " "))
                            {
                                _message = _message.ToLower().Replace(Auction.Command72 + " ", "");
                                Auction.CancelAuction(_cInfo, _message);
                                return false;
                            }
                            if (Auction.IsEnabled && _message.ToLower().StartsWith(Auction.Command73 + " "))
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
                                    _message = _message.ToLower().Replace(Auction.Command73 + " ", "");
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
                            if (Auction.IsEnabled && Wallet.IsEnabled && _message.ToLower().StartsWith(Auction.Command74 + " "))
                            {
                                if (Wallet.IsEnabled)
                                {
                                    if (Auction.No_Admins)
                                    {
                                        if (GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo) > Admin_Level)
                                        {
                                            _message = _message.ToLower().Replace(Auction.Command74 + " ", "");
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
                                        _message = _message.ToLower().Replace(Auction.Command74 + " ", "");
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
                            if (Fps.IsEnabled && _message.ToLower() == Fps.Command75)
                            {
                                Fps.FPS(_cInfo);
                                return false;
                            }
                            if (Loc.IsEnabled && _message.ToLower() == Loc.Command76)
                            {
                                Loc.Exec(_cInfo);
                                return false;
                            }
                            if (VehicleTeleport.IsEnabled)
                            {
                                if (VehicleTeleport.Bike && _message.ToLower() == VehicleTeleport.Command77)
                                {
                                    VehicleTeleport.Exec(_cInfo, 1);
                                    return false;
                                }
                                else if (VehicleTeleport.Mini_Bike && _message.ToLower() == VehicleTeleport.Command78)
                                {
                                    VehicleTeleport.Exec(_cInfo, 2);
                                    return false;
                                }
                                else if (VehicleTeleport.Motor_Bike && _message.ToLower() == VehicleTeleport.Command79)
                                {
                                    VehicleTeleport.Exec(_cInfo, 3);
                                    return false;
                                }
                                else if (VehicleTeleport.Jeep && _message.ToLower() == VehicleTeleport.Command80)
                                {
                                    VehicleTeleport.Exec(_cInfo, 4);
                                    return false;
                                }
                                else if (VehicleTeleport.Gyro && _message.ToLower() == VehicleTeleport.Command81)
                                {
                                    VehicleTeleport.Exec(_cInfo, 5);
                                    return false;
                                }
                            }
                            if (Report.IsEnabled && _message.ToLower().StartsWith(Report.Command82 + " "))
                            {
                                _message = _message.ToLower().Replace(Report.Command82 + " ", "");
                                Report.Check(_cInfo, _message);
                                return false;
                            }
                            if (Bounties.IsEnabled && _message.ToLower() == Bounties.Command83)
                            {
                                Bounties.BountyList(_cInfo);
                                return false;
                            }
                            if (Bounties.IsEnabled && _message.ToLower().StartsWith(Bounties.Command83 + " "))
                            {
                                _message = _message.ToLower().Replace(Bounties.Command83 + " ", "");
                                Bounties.NewBounty(_cInfo, _message);
                                return false;
                            }
                            if (Lottery.IsEnabled && _message.ToLower() == Lottery.Command84)
                            {
                                Lottery.Response(_cInfo);
                                return false;
                            }
                            if (Lottery.IsEnabled && _message.ToLower() == Lottery.Command85)
                            {
                                Lottery.EnterLotto(_cInfo);
                                return false;
                            }
                            if (Lottery.IsEnabled && _message.ToLower().StartsWith(Lottery.Command84 + " "))
                            {
                                _message = _message.Replace(Lottery.Command84 + " ", "");
                                Lottery.NewLotto(_cInfo, _message, _mainName);
                                return false;
                            }
                            if (NewSpawnTele.IsEnabled && NewSpawnTele.Return && _message.ToLower() == NewSpawnTele.Command86)
                            {
                                NewSpawnTele.ReturnPlayer(_cInfo);
                                return false;
                            }
                            if (PlayerList.IsEnabled && _message.ToLower() == PlayerList.Command89)
                            {
                                PlayerList.Exec(_cInfo);
                                return false;
                            }
                            if (Stuck.IsEnabled && _message.ToLower() == Stuck.Command90)
                            {
                                Stuck.Exec(_cInfo);
                                return false;
                            }
                            if (Poll.IsEnabled && PersistentContainer.Instance.PollOpen && _message.ToLower() == Poll.Command91)
                            {
                                Poll.Yes(_cInfo);
                                return false;
                            }
                            if (Poll.IsEnabled && PersistentContainer.Instance.PollOpen && _message.ToLower() == Poll.Command92)
                            {
                                Poll.No(_cInfo);
                                return false;
                            }
                            if (Poll.IsEnabled && PersistentContainer.Instance.PollOpen && _message.ToLower() == Poll.Command93)
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
                            if (Bank.IsEnabled && _message.ToLower() == Bank.Command94)
                            {
                                Bank.CurrentBankAndId(_cInfo);
                                return false;
                            }
                            if (Bank.IsEnabled && _message.ToLower().StartsWith(Bank.Command95 + " "))
                            {
                                _message = _message.ToLower().Replace(Bank.Command95 + " ", "");
                                Bank.CheckLocation(_cInfo, _message, 1);
                                return false;
                            }
                            if (Bank.IsEnabled && _message.ToLower().StartsWith(Bank.Command96 + " "))
                            {
                                _message = _message.ToLower().Replace(Bank.Command96 + " ", "");
                                Bank.CheckLocation(_cInfo, _message, 2);
                                return false;
                            }
                            if (Bank.IsEnabled && Wallet.IsEnabled && Wallet.Bank_Transfers && _message.ToLower().StartsWith(Bank.Command97 + " "))
                            {
                                _message = _message.ToLower().Replace(Bank.Command97 + " ", "");
                                Bank.CheckLocation(_cInfo, _message, 3);
                                return false;
                            }
                            if (Bank.IsEnabled && Wallet.IsEnabled && Wallet.Bank_Transfers && _message.ToLower().StartsWith(Bank.Command98 + " "))
                            {
                                _message = _message.ToLower().Replace(Bank.Command98 + " ", "");
                                Bank.CheckLocation(_cInfo, _message, 4);
                                return false;
                            }
                            if (Bank.IsEnabled && Bank.Player_Transfers && _message.ToLower().StartsWith(Bank.Command99 + " "))
                            {
                                _message = _message.ToLower().Replace(Bank.Command99 + " ", "");
                                Bank.Transfer(_cInfo, _message);
                                return false;
                            }
                            if (Event.Invited && _message.ToLower() == Event.Command100)
                            {
                                Event.AddPlayer(_cInfo);
                                return false;
                            }
                            if (InfoTicker.IsEnabled && _message.ToLower() == InfoTicker.Command104)
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
                            if (Session.IsEnabled && _message.ToLower() == Session.Command105)
                            {
                                Session.Exec(_cInfo);
                                return false;
                            }
                            if (Waypoints.IsEnabled)
                            {
                                if (_message.ToLower() == Waypoints.Command10)
                                {
                                    if (Waypoints.Invite.ContainsKey(_cInfo.entityId))
                                    {
                                        Waypoints.FriendWaypoint(_cInfo);
                                        return false;
                                    }
                                }
                                if (_message.ToLower() == Waypoints.Command106 || _message.ToLower() == Waypoints.Command107 || _message.ToLower() == Waypoints.Command108)
                                {
                                    Waypoints.List(_cInfo);
                                    return false;
                                }
                                if (_message.StartsWith(Waypoints.Command109 + " "))
                                {
                                    _message = _message.ToLower().Replace(Waypoints.Command109 + " ", "");
                                    if (_message != " " || _message != "")
                                    {
                                        Waypoints.TeleDelay(_cInfo, _message, true);
                                        return false;
                                    }
                                }
                                else if (_message.StartsWith(Waypoints.Command110 + " "))
                                {
                                    _message = _message.ToLower().Replace(Waypoints.Command110 + " ", "");
                                    if (_message != " " || _message != "")
                                    {
                                        Waypoints.TeleDelay(_cInfo, _message, true);
                                        return false;
                                    }
                                }
                                else if (_message.StartsWith(Waypoints.Command111 + " "))
                                {
                                    _message = _message.ToLower().Replace(Waypoints.Command111 + " ", "");
                                    if (_message != " " || _message != "")
                                    {
                                        Waypoints.TeleDelay(_cInfo, _message, true);
                                        return false;
                                    }
                                }
                                if (_message.ToLower().StartsWith(Waypoints.Command112 + " "))
                                {
                                    _message = _message.ToLower().Replace(Waypoints.Command112 + " ", "");
                                    Waypoints.SaveClaimCheck(_cInfo, _message);
                                    return false;
                                }
                                else if (_message.ToLower().StartsWith(Waypoints.Command113 + " "))
                                {
                                    _message = _message.ToLower().Replace(Waypoints.Command113 + " ", "");
                                    Waypoints.SaveClaimCheck(_cInfo, _message);
                                    return false;
                                }
                                else if (_message.ToLower().StartsWith(Waypoints.Command114 + " "))
                                {
                                    _message = _message.ToLower().Replace(Waypoints.Command114 + " ", "");
                                    Waypoints.SaveClaimCheck(_cInfo, _message);
                                    return false;
                                }
                                if (_message.ToLower().StartsWith(Waypoints.Command115 + " "))
                                {
                                    _message = _message.ToLower().Replace(Waypoints.Command115 + " ", "");
                                    Waypoints.DelPoint(_cInfo, _message);
                                    return false;
                                }
                                else if (_message.ToLower().StartsWith(Waypoints.Command116 + " "))
                                {
                                    _message = _message.ToLower().Replace(Waypoints.Command116 + " ", "");
                                    Waypoints.DelPoint(_cInfo, _message);
                                    return false;
                                }
                                else if (_message.ToLower().StartsWith(Waypoints.Command117 + " "))
                                {
                                    _message = _message.ToLower().Replace(Waypoints.Command117 + " ", "");
                                    Waypoints.DelPoint(_cInfo, _message);
                                    return false;
                                }
                                if (_message.ToLower().StartsWith(Waypoints.Command106 + " "))
                                {
                                    _message = _message.ToLower().Replace(Waypoints.Command106 + " ", "");
                                    Waypoints.TeleDelay(_cInfo, _message, false);
                                    return false;
                                }
                                else if (_message.ToLower().StartsWith(Waypoints.Command107 + " "))
                                {
                                    _message = _message.ToLower().Replace(Waypoints.Command107 + " ", "");
                                    Waypoints.TeleDelay(_cInfo, _message, false);
                                    return false;
                                }
                                else if (_message.ToLower().StartsWith(Waypoints.Command108 + " "))
                                {
                                    _message = _message.ToLower().Replace(Waypoints.Command108 + " ", "");
                                    Waypoints.TeleDelay(_cInfo, _message, false);
                                    return false;
                                }
                            }
                            if (Whisper.IsEnabled && (_message.ToLower().StartsWith(Whisper.Command120) || _message.ToLower().StartsWith(Whisper.Command121)))
                            {
                                Whisper.Send(_cInfo, _message);
                                return false;
                            }
                            if (Whisper.IsEnabled && (_message.ToLower().StartsWith(Whisper.Command122) || _message.ToLower().StartsWith(Whisper.Command123)))
                            {
                                Whisper.Reply(_cInfo, _message);
                                return false;
                            }
                            if (CustomCommands.IsEnabled && CustomCommands.Dict.ContainsKey(_message = _message.ToLower()))
                            {
                                CustomCommands.Exec(_cInfo, _message.ToLower());
                                return false;
                            }
                            if (ReservedSlots.IsEnabled && _message.ToLower() == ReservedSlots.Command69)
                            {
                                ReservedSlots.ReservedStatus(_cInfo);
                                return false;
                            }
                            if (Prayer.IsEnabled && _message.ToLower() == Prayer.Command126)
                            {
                                Prayer.Exec(_cInfo);
                                return false;
                            }
                            if (ScoutPlayer.IsEnabled && (_message.ToLower() == ScoutPlayer.Command129 || _message.ToLower() == ScoutPlayer.Command130))
                            {
                                ScoutPlayer.Exec(_cInfo);
                                return false;
                            }
                            if (Hardcore.IsEnabled && _message.ToLower() == Hardcore.Command128)
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
                                            _phrase601 = _phrase601.Replace("{CommandPrivate}", Chat_Command_Prefix1);
                                            _phrase601 = _phrase601.Replace("{Command127}", Hardcore.Command127);
                                            ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase601 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                    }
                                    else
                                    {
                                        Phrases.Dict.TryGetValue(602, out string _phrase602);
                                        _phrase602 = _phrase602.Replace("{CommandPrivate}", Chat_Command_Prefix1);
                                        _phrase602 = _phrase602.Replace("{Command127}", Hardcore.Command127);
                                        ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase602 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue(602, out string _phrase602);
                                    _phrase602 = _phrase602.Replace("{CommandPrivate}", Chat_Command_Prefix1);
                                    _phrase602 = _phrase602.Replace("{Command127}", Hardcore.Command127);
                                    ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase602 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                            if (Hardcore.IsEnabled && _message.ToLower() == Hardcore.Command127)
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
                            if (ExitCommand.IsEnabled && (_message.ToLower() == ExitCommand.Command131 || _message.ToLower() == ExitCommand.Command132))
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
                            //if (_message.ToLower().StartsWith(Test.Command999))
                            //{
                            //    Test.Exec(_cInfo);
                            //    return false;
                            //}
                        }
                        if (AdminChat.IsEnabled && _message.ToLower().StartsWith(AdminChat.Command118 + " "))
                        {
                            _message = _message.ToLower().Replace(AdminChat.Command118 + " ", "");
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