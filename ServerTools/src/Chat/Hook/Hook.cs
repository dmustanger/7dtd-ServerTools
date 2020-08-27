using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerTools
{
    public class ChatHook
    {
        public static bool ChatFlood = false, Normal_Player_Color_Prefix = false, Message_Color_Enabled = false;
        public static string Normal_Player_Name_Color = "[00B3B3]", Normal_Player_Prefix = "NOOB", Friend_Chat_Color = "[33CC33]", Party_Chat_Color = "[FFCC00]",
            Command_Private = "/", Command_Public = "!", Message_Color = "[FFFFFF]", Normal_Player_Prefix_Color = "[FFFFFF]";
        public static int Admin_Level = 0, Mod_Level = 1, Max_Length = 250, Messages_Per_Min = 8, Wait_Time = 60;
        private static Dictionary<string, int> ChatFloodCount = new Dictionary<string, int>();
        private static Dictionary<string, DateTime> ChatFloodTime = new Dictionary<string, DateTime>();
        private static Dictionary<string, DateTime> ChatFloodLock = new Dictionary<string, DateTime>();

        public static bool Hook(ClientInfo _cInfo, EChatType _type, int _senderId, string _message, string _mainName, bool _localizeMain, List<int> _recipientEntityIds)
        {
            if (!string.IsNullOrEmpty(_message) && _cInfo != null && _mainName != LoadConfig.Server_Response_Name)
            {
                if (Mute.IsEnabled && Mute.Mutes.Contains(_cInfo.playerId))
                {
                    if (Mute.Block_Commands && (_message.StartsWith(Command_Private) || _message.StartsWith(Command_Public)))
                    {
                        ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You are muted and blocked from commands.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        return false;
                    }
                    else if ((_message.StartsWith(Command_Private) || _message.StartsWith(Command_Public)) && !LoadTriggers.TriggerList.Contains(_message))
                    {
                        ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You are muted.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        return false;
                    }
                    else if ((!_message.StartsWith(Command_Private) || !_message.StartsWith(Command_Public)))
                    {
                        ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You are muted.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        return false;
                    }
                }
                if (Badwords.IsEnabled)
                {
                    if (Badwords.Invalid_Name)
                    {
                        for (int i = 0; i < Badwords.Words.Count; i++)
                        {
                            if (_mainName.ToLower().Contains(Badwords.Words[i]))
                            {
                                _mainName = "***";
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
                    if (_message.StartsWith("  "))
                    {
                        _message.Substring(2);
                    }
                    if (!_message.StartsWith("@") && _senderId != -1 && !_message.StartsWith(Command_Private) && !_message.StartsWith(Command_Public))
                    {
                        if (ChatLog.IsEnabled)
                        {
                            ChatLog.Exec(_message, _mainName);
                        }
                        if (ChatFlood)
                        {
                            if (_message.Length >= Max_Length)
                            {
                                if (!Phrases.Dict.TryGetValue(971, out string _phrase971))
                                {
                                    _phrase971 = "Message is too long.";
                                }
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase971 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                return false;
                            }
                            else if (Messages_Per_Min > 1)
                            {
                                if (ChatFloodLock.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _lockTime;
                                    ChatFloodLock.TryGetValue(_cInfo.playerId, out _lockTime);
                                    TimeSpan varTime = DateTime.Now - _lockTime;
                                    double fractionalSeconds = varTime.TotalSeconds;
                                    if ((int)fractionalSeconds >= Wait_Time)
                                    {
                                        ChatFloodLock.Remove(_cInfo.playerId);
                                    }
                                    else
                                    {
                                        string _phrase970;
                                        if (!Phrases.Dict.TryGetValue(970, out _phrase970))
                                        {
                                            _phrase970 = "You have sent too many messages in too short a time. Your chat function is locked temporarily.";
                                        }
                                        ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase970 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                            string _phrase970;
                                            if (!Phrases.Dict.TryGetValue(970, out _phrase970))
                                            {
                                                _phrase970 = "You have sent too many messages in one minute. Your chat function is locked temporarily.";
                                            }
                                            ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase970 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                        if (Message_Color_Enabled && !_message.Contains("[******]") && Message_Color.StartsWith("[") && Message_Color.EndsWith("]"))
                        {
                            if (Message_Color.Contains(","))
                            {
                                int _messageCount = _message.Count();
                                string[] _mssageColorSplit = Message_Color.Split(',');
                                for (int i = _mssageColorSplit.Length; i >= 1; i--)
                                {
                                    if (i - 1 < _messageCount)
                                    {
                                        _message = _message.Insert(i - 1, _mssageColorSplit[i - 1]);
                                    }
                                }
                            }
                            else
                            {
                                _message = _message.Insert(0, Message_Color);
                            }
                        }
                        if (!_mainName.Contains("[******]"))
                        {
                            if (ChatColorPrefix.IsEnabled && ChatColorPrefix.Dict.ContainsKey(_cInfo.playerId))
                            {
                                ChatColorPrefix.Dict1.TryGetValue(_cInfo.playerId, out DateTime _dt);
                                if (DateTime.Now < _dt)
                                {
                                    ChatColorPrefix.Dict.TryGetValue(_cInfo.playerId, out string[] _colorPrefix);
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
                                    ChatColorPrefix.Dict.Remove(_cInfo.playerId);
                                    ChatColorPrefix.Dict1.Remove(_cInfo.playerId);
                                    ChatColorPrefix.UpdateXml();
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Your chat color prefix time has expired.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    return false;
                                }
                            }
                            if (ClanManager.IsEnabled && ClanManager.ClanMember.Contains(_cInfo.playerId))
                            {
                                if (!string.IsNullOrEmpty(PersistentContainer.Instance.Players[_cInfo.playerId].ClanName))
                                {
                                    string _clanName = PersistentContainer.Instance.Players[_cInfo.playerId].ClanName;
                                    if (Normal_Player_Color_Prefix && (Normal_Player_Name_Color != "" || Normal_Player_Prefix_Color != ""))
                                    {
                                        PrepMessage(_cInfo, _message, _senderId, _mainName, _clanName, Normal_Player_Name_Color, Normal_Player_Prefix_Color, _type, _recipientEntityIds);
                                    }
                                    else
                                    {
                                        PrepMessage(_cInfo, _message, _senderId, _mainName, _clanName, "", "", _type, _recipientEntityIds);
                                    }
                                    return false;
                                }
                            }
                            if (Normal_Player_Color_Prefix && (Normal_Player_Name_Color != "" || Normal_Player_Prefix_Color != ""))
                            {
                                PrepMessage(_cInfo, _message, _senderId, _mainName, Normal_Player_Prefix, Normal_Player_Name_Color, Normal_Player_Prefix_Color, _type, _recipientEntityIds);
                                return false;
                            }
                        }
                    }
                    if (_message.StartsWith(Command_Private) || _message.StartsWith(Command_Public))
                    {
                        if (ChatCommandLog.IsEnabled)
                        {
                            ChatCommandLog.Exec(_message, _cInfo);
                        }
                        if (_message.StartsWith(Command_Public))
                        {
                            _message = _message.Replace(Command_Public, "");
                        }
                        else if (_message.StartsWith(Command_Private))
                        {
                            _message = _message.Replace(Command_Private, "");
                        }
                        if (TeleportHome.IsEnabled && _message.ToLower() == TeleportHome.Command1)
                        {
                            if (Zones.IsEnabled && !Zones.Set_Home)
                            {
                                if (!Zones.ZoneExit.ContainsKey(_cInfo.entityId))
                                {
                                    TeleportHome.SetHome1(_cInfo);
                                }
                                else
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You can not use sethome inside a zone.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                TeleportHome.SetHome1(_cInfo);
                            }
                            return false;
                        }
                        if (TeleportHome.IsEnabled && _message.ToLower() == TeleportHome.Command2)
                        {

                            TeleportHome.Exec1(_cInfo);
                            return false;
                        }
                        if (TeleportHome.IsEnabled && _message.ToLower() == TeleportHome.Command3)
                        {

                            TeleportHome.FExec1(_cInfo);
                            return false;
                        }
                        if (TeleportHome.IsEnabled && _message.ToLower() == TeleportHome.Command4)
                        {

                            TeleportHome.DelHome1(_cInfo);
                            return false;
                        }
                        if (TeleportHome.Set_Home2_Enabled && _message.ToLower() == TeleportHome.Command5)
                        {
                            if (TeleportHome.Set_Home2_Reserved_Only && ReservedSlots.IsEnabled)
                            {
                                if (!Zones.Set_Home)
                                {
                                    if (!Zones.ZoneExit.ContainsKey(_cInfo.entityId))
                                    {
                                        if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                        {
                                            ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out DateTime _dt);
                                            if (DateTime.Now < _dt)
                                            {

                                                TeleportHome.SetHome2(_cInfo);
                                            }
                                            else
                                            {
                                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Your reserved status has expired Command is unavailable.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                            }
                                        }
                                        else
                                        {
                                            ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You are not listed as a reserved player. Command is unavailable.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                    }
                                    else
                                    {
                                        ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You can not use sethome2 in a protected zone.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                                else
                                {
                                    if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                    {
                                        ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out DateTime _dt);
                                        if (DateTime.Now < _dt)
                                        {
                                            TeleportHome.SetHome2(_cInfo);
                                        }
                                        else
                                        {
                                            ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Your reserved status has expired Command is unavailable.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                    }
                                    else
                                    {
                                        ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You are not listed as a reserved player. Command is unavailable.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                            }
                            else if (!TeleportHome.Set_Home2_Reserved_Only)
                            {
                                if (!Zones.Set_Home)
                                {
                                    if (!Zones.ZoneExit.ContainsKey(_cInfo.entityId))
                                    {
                                        TeleportHome.SetHome2(_cInfo);
                                    }
                                    else
                                    {
                                        ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You can not use sethome2 in a protected zone.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                                else
                                {
                                    TeleportHome.SetHome2(_cInfo);
                                }
                            }
                            return false;
                        }
                        if (TeleportHome.Set_Home2_Enabled && _message.ToLower() == TeleportHome.Command6)
                        {
                            if (TeleportHome.Set_Home2_Reserved_Only && ReservedSlots.IsEnabled)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out DateTime _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        TeleportHome.Exec2(_cInfo);
                                    }
                                    else
                                    {
                                        ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Your reserved status has expired. Command is unavailable.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                                else
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You are not on the reserved list, please donate or contact an admin.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else if (!TeleportHome.Set_Home2_Reserved_Only)
                            {
                                TeleportHome.Exec2(_cInfo);
                            }
                            else
                            {
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Home2 is not enabled.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            return false;
                        }
                        if (TeleportHome.Set_Home2_Enabled && _message.ToLower() == TeleportHome.Command7)
                        {
                            if (TeleportHome.Set_Home2_Reserved_Only && ReservedSlots.IsEnabled)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out DateTime _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        TeleportHome.FExec2(_cInfo);
                                    }
                                    else
                                    {
                                        ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Your reserved status has expired. Command is unavailable.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                                else
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You are not on the reserved list, please donate or contact an admin.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else if (!TeleportHome.Set_Home2_Reserved_Only)
                            {
                                TeleportHome.FExec2(_cInfo);
                            }
                            else
                            {
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Home2 is not enabled.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            return false;
                        }
                        if (TeleportHome.Set_Home2_Enabled && _message.ToLower() == TeleportHome.Command8)
                        {
                            if (TeleportHome.Set_Home2_Reserved_Only && ReservedSlots.IsEnabled)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out DateTime _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        TeleportHome.DelHome2(_cInfo);
                                    }
                                    else
                                    {
                                        ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Your reserved status has expired. Command is unavailable.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                                else
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You are not on the reserved list, please donate or contact an admin.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else if (!TeleportHome.Set_Home2_Reserved_Only)
                            {
                                TeleportHome.DelHome2(_cInfo);
                            }
                            return false;
                        }
                        if (TeleportHome.IsEnabled && _message.ToLower() == TeleportHome.Command9)
                        {
                            if (TeleportHome.Invite.ContainsKey(_cInfo.entityId))
                            {
                                TeleportHome.FriendHome(_cInfo);
                                return false;
                            }
                        }
                        if (Waypoints.IsEnabled && _message.ToLower() == Waypoints.Command10)
                        {
                            if (Waypoints.Invite.ContainsKey(_cInfo.entityId))
                            {
                                Waypoints.FriendWaypoint(_cInfo);
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
                                if (!Phrases.Dict.TryGetValue(824, out string _phrase824))
                                {
                                    _phrase824 = "There is a vote already open.";
                                }
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase824 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                Jail.SetJail(_cInfo);
                            }
                            else if (_message.ToLower().StartsWith(Jail.Command27))
                            {
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                Jail.PutInJail(_cInfo, _message);
                            }
                            else if (_message.ToLower().StartsWith(Jail.Command28))
                            {
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                            ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Checking your claim block status.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            FirstClaimBlock.firstClaim(_cInfo);
                            return false;
                        }
                        if (ClanManager.IsEnabled)
                        {
                            if (_message.ToLower() == ClanManager.Command33)
                            {
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Usage: " + ChatHook.Command_Private + ClanManager.Command33 + " clanName[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Usage: " + ChatHook.Command_Private + ClanManager.Command35 + " playerName[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Usage: " + ChatHook.Command_Private + ClanManager.Command38 + " playerName[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Usage: " + ChatHook.Command_Private + ClanManager.Command39 + " playerName[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else if (_message.ToLower().StartsWith(ClanManager.Command39))
                            {
                                _message = _message.Replace(ClanManager.Command39 + " ", "");
                                ClanManager.PromoteMember(_cInfo, _message);
                                return false;
                            }
                            else if (_message.ToLower() == ClanManager.Command40)
                            {
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Usage: " + ChatHook.Command_Private + ClanManager.Command40 + " playerName[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _clanCommands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                return false;
                            }
                            else if (_message.ToLower() == ClanManager.Command43 || _message.ToLower() == ClanManager.Command124)
                            {
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Usage: " + ChatHook.Command_Private + ClanManager.Command43 + " message or " + ChatHook.Command_Private + ClanManager.Command124 + " message[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Usage: " + ChatHook.Command_Private + ClanManager.Command44 + " NewName[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _clanlist, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                return false;
                            }
                        }
                        if (VoteReward.IsEnabled && _message.ToLower() == VoteReward.Command46)
                        {
                            ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Checking for your vote.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            VoteReward.Check(_cInfo);
                            return false;
                        }
                        if (Shutdown.IsEnabled && _message.ToLower() == Shutdown.Command47)
                        {
                            ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Checking for the next shutdown time.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            Shutdown.NextShutdown(_cInfo);
                            return false;
                        }
                        if (AdminList.IsEnabled && _message.ToLower() == AdminList.Command48)
                        {
                            ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Listing online administrators and moderators.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            AdminList.List(_cInfo, _mainName);
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
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Sending you to your death point.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                        if (Shop.IsEnabled && Wallet.IsEnabled && _message.ToLower() == Shop.Command57)//list category
                        {
                            if (_message.ToLower() == Shop.Command57)
                            {
                                Shop.PosCheck(_cInfo, _message, 1, 0);
                            }
                            return false;
                        }
                        if (Shop.IsEnabled && Wallet.IsEnabled && _message.ToLower().StartsWith(Shop.Command57 + " "))//show specific category
                        {
                            string _category = _message.ToLower().Replace(Shop.Command57 + " ", "");
                            if (Shop.categories.Contains(_category))
                            {
                                Shop.PosCheck(_cInfo, _category, 2, 0);
                                return false;
                            }
                            else if (_message.ToLower().StartsWith(Shop.Command58 + " "))
                            {
                                string _buyCount = _message.Replace(Shop.Command58 + " ", "");
                                if (_buyCount.ToLower().Length == 1)
                                {
                                    Shop.PosCheck(_cInfo, _buyCount, 3, 1);
                                }
                                else
                                {
                                    string a = _buyCount.Split(' ').First();
                                    string b = _buyCount.Split(' ').Last();
                                    int _count;
                                    if (int.TryParse(b, out _count))
                                    {
                                        Shop.PosCheck(_cInfo, a, 3, _count);
                                    }
                                    else
                                    {
                                        string _phrase620;
                                        if (!Phrases.Dict.TryGetValue(620, out _phrase620))
                                        {
                                            _phrase620 = "The item amount # you are trying to buy is not an integer. Please input {CommandPrivate}{Command58} 1 2 for example.";
                                        }
                                        _phrase620 = _phrase620.Replace("{CommandPrivate}", ChatHook.Command_Private);
                                        _phrase620 = _phrase620.Replace("{Command58}", Shop.Command58);
                                        ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase620 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                                return false;
                            }
                        }
                        if (FriendTeleport.IsEnabled && _message.ToLower() == FriendTeleport.Command59)
                        {
                            FriendTeleport.ListFriends(_cInfo, _message);
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
                                FriendTeleport.Checks(_cInfo, _message);
                            }
                            else
                            {
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            return false;
                        }
                        if (FriendTeleport.IsEnabled && _message.ToLower() == FriendTeleport.Command60 && FriendTeleport.Dict.ContainsKey(_cInfo.entityId))
                        {
                            int _dictValue;
                            FriendTeleport.Dict.TryGetValue(_cInfo.entityId, out _dictValue);
                            DateTime _dict1Value;
                            FriendTeleport.Dict1.TryGetValue(_cInfo.entityId, out _dict1Value);
                            TimeSpan varTime = DateTime.Now - _dict1Value;
                            double fractionalSeconds = varTime.TotalSeconds;
                            int _timepassed = (int)fractionalSeconds;
                            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay && ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                            {
                                DateTime _dt;
                                ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                if (DateTime.Now < _dt)
                                {
                                    int _newTime = _timepassed / 2;
                                    _timepassed = _newTime;
                                }
                            }
                            if (_timepassed <= 60)
                            {
                                FriendTeleport.TeleFriend(_cInfo, _dictValue);
                                FriendTeleport.Dict.Remove(_cInfo.entityId);
                                FriendTeleport.Dict1.Remove(_cInfo.entityId);
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Your friend's teleport request was accepted.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                FriendTeleport.Dict.Remove(_cInfo.entityId);
                                FriendTeleport.Dict1.Remove(_cInfo.entityId);
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Your friend's teleport request has expired.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            return false;
                        }
                        if (DeathSpot.IsEnabled && _message.ToLower() == DeathSpot.Command61)
                        {
                            DeathSpot.Exec(_cInfo);
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
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "A weather vote has already begun.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Vote cast for clear.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                                else
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You have already voted.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "There is no active weather vote. Type " + ChatHook.Command_Private + WeatherVote.Command62 + " in chat to open a new vote.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Vote cast for rain.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                                else
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You have already voted.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "There is no active weather vote. Type " + ChatHook.Command_Private + WeatherVote.Command62 + " in chat to open a new vote.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Vote cast for snow.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                                else
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You have already voted.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "There is no active weather vote. Type " + ChatHook.Command_Private + WeatherVote.Command62 + " in chat to open a new vote.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                string _phrase824;
                                if (!Phrases.Dict.TryGetValue(824, out _phrase824))
                                {
                                    _phrase824 = "There is a vote already open.";
                                }
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase824 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                string _phrase824;
                                if (!Phrases.Dict.TryGetValue(824, out _phrase824))
                                {
                                    _phrase824 = "There is a vote already open.";
                                }
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase824 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                    string _phrase825;
                                    if (!Phrases.Dict.TryGetValue(825, out _phrase825))
                                    {
                                        _phrase825 = "There are now {VoteCount} of {VotesNeeded} votes.";
                                    }
                                    _phrase825 = _phrase825.Replace("{VoteCount}", KickVote.Kick.Count.ToString());
                                    _phrase825 = _phrase825.Replace("{VotesNeeded}", KickVote.Votes_Needed.ToString());
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase825 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                                else
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You have already voted.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                            else if (RestartVote.VoteOpen)
                            {
                                if (!RestartVote.Restart.Contains(_cInfo.entityId))
                                {
                                    RestartVote.Restart.Add(_cInfo.entityId);
                                    string _phrase825;
                                    if (!Phrases.Dict.TryGetValue(825, out _phrase825))
                                    {
                                        _phrase825 = "There are now {VoteCount} of {VotesNeeded} votes.";
                                    }
                                    _phrase825 = _phrase825.Replace("{VoteCount}", RestartVote.Restart.Count.ToString());
                                    _phrase825 = _phrase825.Replace("{VotesNeeded}", RestartVote.Votes_Needed.ToString());
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase825 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                                else
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You have already voted.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                            else if (MuteVote.VoteOpen)
                            {
                                if (!MuteVote.Votes.Contains(_cInfo.entityId))
                                {
                                    MuteVote.Votes.Add(_cInfo.entityId);
                                    string _phrase825;
                                    if (!Phrases.Dict.TryGetValue(825, out _phrase825))
                                    {
                                        _phrase825 = "There are now {VoteCount} of {VotesNeeded} votes.";
                                    }
                                    _phrase825 = _phrase825.Replace("{VoteCount}", MuteVote.Votes.Count.ToString());
                                    _phrase825 = _phrase825.Replace("{VotesNeeded}", MuteVote.Votes_Needed.ToString());
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase825 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                                else
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You have already voted.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                        }
                        if (AuctionBox.IsEnabled && _message.ToLower() == AuctionBox.Command71)
                        {
                            AuctionBox.AuctionList(_cInfo);
                            return false;
                        }
                        if (AuctionBox.IsEnabled && _message.ToLower().StartsWith(AuctionBox.Command72 + " "))
                        {
                            _message = _message.ToLower().Replace(AuctionBox.Command72 + " ", "");
                            AuctionBox.CancelAuction(_cInfo, _message);
                            return false;
                        }
                        if (AuctionBox.IsEnabled && _message.ToLower().StartsWith(AuctionBox.Command73 + " "))
                        {
                            if (Wallet.IsEnabled)
                            {
                                if (AuctionBox.No_Admins)
                                {
                                    if (GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo) <= Admin_Level)
                                    {
                                        string _chatMessage = LoadConfig.Chat_Response_Color + "The auction is disabled for your tier.[-]";
                                        ChatMessage(_cInfo, _chatMessage, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                                _message = _message.ToLower().Replace(AuctionBox.Command73 + " ", "");
                                {
                                    int _purchase;
                                    if (int.TryParse(_message, out _purchase))
                                    {
                                        if (AuctionBox.AuctionItems.ContainsKey(_purchase))
                                        {
                                            AuctionBox.WalletCheck(_cInfo, _purchase);
                                        }
                                        else
                                        {
                                            ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You have used an auction item # that does not exist or has sold. Type " + ChatHook.Command_Private + AuctionBox.Command71 + ".[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Can not run command. Wallet is not enabled" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            return false;
                        }
                        if (AuctionBox.IsEnabled && Wallet.IsEnabled && _message.ToLower().StartsWith(AuctionBox.Command74 + " "))
                        {
                            if (Wallet.IsEnabled)
                            {
                                if (AuctionBox.No_Admins)
                                {
                                    if (GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo) > Admin_Level)
                                    {
                                        _message = _message.ToLower().Replace(AuctionBox.Command74 + " ", "");
                                        AuctionBox.CheckBox(_cInfo, _message);
                                    }
                                    else
                                    {
                                        ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "The auction is disabled for your tier.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                                else
                                {
                                    _message = _message.ToLower().Replace(AuctionBox.Command74 + " ", "");
                                    AuctionBox.CheckBox(_cInfo, _message);
                                }
                            }
                            else
                            {
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Can not run command. Wallet is not enabled" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                            PlayerList.Exec(_cInfo, _mainName);
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
                            string _phrase926;
                            if (!Phrases.Dict.TryGetValue(926, out _phrase926))
                            {
                                _phrase926 = "Poll: {Message}";
                            }
                            _phrase926 = _phrase926.Replace("{Message}", _pollData[2]);
                            string _phrase813;
                            if (!Phrases.Dict.TryGetValue(813, out _phrase813))
                            {
                                _phrase813 = "The pole is at yes {YesVote} / no {NoVote} votes.";
                            }
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
                            _phrase813 = _phrase813.Replace("{YesVote}", _yes.ToString());
                            _phrase813 = _phrase813.Replace("{NoVote}", _no.ToString());
                            ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase926 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase813 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                        if (Bank.IsEnabled && Wallet.IsEnabled && _message.ToLower().StartsWith(Bank.Command97 + " "))
                        {
                            if (Wallet.Bank_Transfers)
                            {
                                _message = _message.ToLower().Replace(Bank.Command97 + " ", "");
                                Bank.CheckLocation(_cInfo, _message, 3);
                                return false;
                            }
                            else
                            {
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "The server has wallet to bank account transfers turned off.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        if (Bank.IsEnabled && Wallet.IsEnabled && Wallet.Bank_Transfers && _message.ToLower().StartsWith(Bank.Command98 + " "))
                        {
                            if (Wallet.Bank_Transfers)
                            {
                                _message = _message.ToLower().Replace(Bank.Command98 + " ", "");
                                Bank.CheckLocation(_cInfo, _message, 4);
                                return false;
                            }
                            else
                            {
                                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "The server has bank account to wallet transfers turned off.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
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
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You have turned off infoticker messages until the server restarts.", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                InfoTicker.ExemptionList.Remove(_cInfo.playerId);
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You have turned on infoticker messages.", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            return false;
                        }
                        if (Session.IsEnabled && _message.ToLower() == Session.Command105)
                        {
                            Session.Exec(_cInfo);
                            return false;
                        }
                        if (Waypoints.IsEnabled && (_message.ToLower() == Waypoints.Command106 || _message.ToLower() == Waypoints.Command107 || _message.ToLower() == Waypoints.Command108))
                        {
                            Waypoints.List(_cInfo);
                            return false;
                        }
                        if (Waypoints.IsEnabled)
                        {
                            string _waypointName = "";
                            if (_message.StartsWith(Waypoints.Command109 + " "))
                            {
                                _waypointName = _message.ToLower().Replace(Waypoints.Command109 + " ", "");
                                if (_waypointName != " " || _waypointName != "")
                                {
                                    Waypoints.TeleDelay(_cInfo, _waypointName, true);
                                    return false;
                                }
                            }
                            else if (_message.StartsWith(Waypoints.Command110 + " "))
                            {
                                _waypointName = _message.ToLower().Replace(Waypoints.Command110 + " ", "");
                                if (_waypointName != " " || _waypointName != "")
                                {
                                    Waypoints.TeleDelay(_cInfo, _waypointName, true);
                                    return false;
                                }
                            }
                            else if (_message.StartsWith(Waypoints.Command111 + " "))
                            {
                                _waypointName = _message.ToLower().Replace(Waypoints.Command111 + " ", "");
                                if (_waypointName != " " || _waypointName != "")
                                {
                                    Waypoints.TeleDelay(_cInfo, _waypointName, true);
                                    return false;
                                }
                            }
                        }
                        if (Waypoints.IsEnabled)
                        {
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
                        }
                        if (Waypoints.IsEnabled)
                        {
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
                        }
                        if (Waypoints.IsEnabled)
                        {
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
                                        PersistentContainer.Instance.Save();
                                        ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You are now in hardcore mode with limited lives remaining. Type " + ChatHook.Command_Private + Hardcore.Command127 + " to check how many lives remain." + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                                else
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You are already signed up for hardcore mode. Type " + ChatHook.Command_Private + Hardcore.Command127 + " to check how many lives remain." + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You are already signed up for hardcore mode. Type " + ChatHook.Command_Private + Hardcore.Command127 + " to check how many lives remain." + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                        ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You are not signed up for hardcore mode. Type " + ChatHook.Command_Private + Hardcore.Command128 + " to enter hardcore mode. This can not be undone." + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                                else
                                {
                                    Hardcore.Check(_cInfo, _player);
                                }
                            }
                            return false;
                        }
                        if (BattleLogger.IsEnabled && (_message.ToLower() == BattleLogger.Command131 || _message.ToLower() == BattleLogger.Command132))
                        {

                            if (BattleLogger.Exit.Contains(_cInfo.playerId) && !BattleLogger.ExitPos.ContainsKey(_cInfo.playerId))
                            {
                                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                if (_player != null)
                                {
                                    BattleLogger.ExitPos.Add(_cInfo.playerId, _player.position);
                                    Timers.BattleLogPlayerExit(_cInfo.playerId);
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Please wait 15 seconds for disconnection and do not move" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else if(GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo) <= BattleLogger.Admin_Level)
                            {
                                BattleLogger.Disconnect(_cInfo);
                            }
                            return false;
                        }
                    }
                    if (AdminChat.IsEnabled && _message.ToLower().StartsWith("@" + AdminChat.Command118))
                    {
                        AdminChat.SendAdmins(_cInfo, _message);
                        return false;
                    }
                }
            }
            return true;
        }

        public static void PrepMessage(ClientInfo _cInfo, string _message, int _senderId, string _mainName, string _prefix, string _nameColor, string _prefixColor, EChatType _type, List<int> _recipientEntityIds)
        {
            if (!Message_Color_Enabled)
            {
                _message = _message.Insert(0, "[FFFFFF]");
            }
            if (_type == EChatType.Friends)
            {
                _prefix = _prefix.Insert(0, "(Friends)");
            }
            else if (_type == EChatType.Party)
            {
                _prefix = _prefix.Insert(0, "(Party)");
            }
            if (_prefixColor.StartsWith("[") && _prefixColor.EndsWith("]") && _prefix.Length > 0)
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
            }
            if (_nameColor.StartsWith("[") && _nameColor.EndsWith("]") && _mainName.Length > 0)
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
            }
            if (_prefix.Length > 0)
            {
                ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0}[-] {1}[-]", _prefix, _mainName), _type, _recipientEntityIds);
            }
            else
            {
                ChatMessage(_cInfo, _message, _senderId, _mainName, _type, _recipientEntityIds);
            }
        }

        public static void ChatMessage(ClientInfo _cInfo, string _message, int _senderId, string _name, EChatType _type, List<int> _recipientEntityIds)
        {
            if (_type == EChatType.Whisper)
            {
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, _senderId, _message, _name, false, null));
            }
            else if (_type == EChatType.Global)
            {
                if (_cInfo != null)
                {
                    if (Mute.IsEnabled)
                    {
                        List<int> _mutedPlayers;
                        if (Mute.PrivateMutes.Count > 0)
                        {
                            List<ClientInfo> _clientList = PersistentOperations.ClientList();
                            if (_clientList != null && _clientList.Count > 0)
                            {
                                for (int i = 0; i < _clientList.Count; i++)
                                {
                                    ClientInfo _cInfo2 = _clientList[i];
                                    if (_cInfo2 != null)
                                    {
                                        if (Mute.PrivateMutes.ContainsKey(_cInfo2.entityId) || Mute.PrivateMutes.ContainsKey(_cInfo.entityId))
                                        {
                                            if (Mute.PrivateMutes.TryGetValue(_cInfo2.entityId, out _mutedPlayers))
                                            {
                                                if (_mutedPlayers.Contains(_cInfo.entityId))
                                                {
                                                    continue;
                                                }
                                            }
                                            if (Mute.PrivateMutes.TryGetValue(_cInfo.entityId, out _mutedPlayers))
                                            {
                                                if (_mutedPlayers.Contains(_cInfo2.entityId))
                                                {
                                                    continue;
                                                }
                                            }
                                        }
                                        _cInfo2.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, _senderId, _message, _name, false, _recipientEntityIds));
                                    }
                                }
                            }
                        }
                        else
                        {
                            GameManager.Instance.ChatMessageServer(_cInfo, EChatType.Global, -1, _message, _name, false, _recipientEntityIds);
                        }
                    }
                    else
                    {
                        GameManager.Instance.ChatMessageServer(_cInfo, EChatType.Global, -1, _message, _name, false, _recipientEntityIds);
                    }
                }
                else
                {
                    GameManager.Instance.ChatMessageServer(_cInfo, EChatType.Global, -1, _message, _name, false, _recipientEntityIds);
                }
            }
            else if (_type == EChatType.Friends)
            {
                if (_recipientEntityIds != null && _recipientEntityIds.Count > 0)
                {
                    if (Friend_Chat_Color.StartsWith("[") && Friend_Chat_Color.EndsWith("]"))
                    {
                        _message = _message.Insert(0, Friend_Chat_Color);
                    }
                    for (int i = 0; i < _recipientEntityIds.Count; i++)
                    {
                        int _recipient = _recipientEntityIds[i];
                        ClientInfo _cInfo2 = PersistentOperations.GetClientInfoFromEntityId(_recipient);
                        if (_cInfo2 != null)
                        {
                            _cInfo2.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, _senderId, _message, _name, false, null));
                        }
                    }
                }
            }
            else if (_type == EChatType.Party)
            {
                if (_recipientEntityIds != null && _recipientEntityIds.Count > 0)
                {
                    if (Party_Chat_Color.StartsWith("[") && Party_Chat_Color.EndsWith("]"))
                    {
                        _message = _message.Insert(0, Party_Chat_Color);
                    }
                    for (int i = 0; i < _recipientEntityIds.Count; i++)
                    {
                        int _recipient = _recipientEntityIds[i];
                        ClientInfo _cInfo2 = PersistentOperations.GetClientInfoFromEntityId(_recipient);
                        if (_cInfo2 != null)
                        {
                            _cInfo2.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, _senderId, _message, _name, false, null));
                        }
                    }
                }
            }
        }
    }
}