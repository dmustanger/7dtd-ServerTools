using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ServerTools
{
    public class ChatHook
    {
        public static bool ChatFlood = false, Normal_Player_Chat_Prefix = false, Message_Color_Enabled = false;
        public static string Normal_Player_Color = "[00B3B3]", Normal_Player_Prefix = "NOOB", Friend_Chat_Color = "[33CC33]", Party_Chat_Color = "[FFCC00]", Player_Name_Color = "[00FF00]",
            Command_Private = "/", Command_Public = "!", Message_Color = "[FFFFFF]";
        public static int Admin_Level = 0, Mod_Level = 1, Max_Length = 250, Messages_Per_Min = 8, Wait_Time = 60;
        private static SortedDictionary<string, DateTime> Dict = new SortedDictionary<string, DateTime>();
        private static SortedDictionary<string, string> Dict1 = new SortedDictionary<string, string>();
        private static Dictionary<int, int> ChatFloodPlayerList = new Dictionary<int, int>();
        private static Dictionary<int, DateTime> ChatFloodTimeList = new Dictionary<int, DateTime>();
        private static Dictionary<int, DateTime> ChatFloodLock = new Dictionary<int, DateTime>();

        public static bool Hook(ClientInfo _cInfo, EChatType _type, int _senderId, string _message, string _mainName, bool _localizeMain, List<int> _recipientEntityIds)
        {
            if (!string.IsNullOrEmpty(_message) && _cInfo != null && _mainName != LoadConfig.Server_Response_Name)
            {
                if (Mute.IsEnabled && Mute.Mutes.Contains(_cInfo.playerId))
                {
                    if (Mute.Block_Commands && (_message.StartsWith(Command_Private) || _message.StartsWith(Command_Public)))
                    {
                        ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you are muted and blocked from commands.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        return false;
                    }
                    else if ((_message.StartsWith(Command_Private) || _message.StartsWith(Command_Public)) && !LoadTriggers.TriggerList.Contains(_message))
                    {
                        ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you are muted.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        return false;
                    }
                    else if ((!_message.StartsWith(Command_Private) || !_message.StartsWith(Command_Public)))
                    {
                        ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you are muted.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        return false;
                    }
                }
                if (Badwords.Invalid_Name)
                {
                    bool _hasBadName = false;
                    string _mainName1 = _mainName.ToLower();
                    foreach (string _word in Badwords.List)
                    {
                        if (_mainName1.Contains(_word) || _mainName1.Contains(_word.ToUpper()))
                        {
                            string _replace = "";
                            for (int i = 0; i < _word.Length; i++)
                            {
                                _replace = string.Format("{0}*", _replace);
                            }
                            _mainName = _mainName.Replace(_word, _replace);
                            _mainName = _mainName.Replace(_word.ToUpper(), _replace);
                            _hasBadName = true;
                        }
                    }
                    if (_hasBadName)
                    {
                        ChatMessage(_cInfo, ChatHook.Player_Name_Color + _mainName + LoadConfig.Chat_Response_Color + " your name is invalid. No Commands are available until [-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        return false;
                    }
                }
                if (Badwords.IsEnabled)
                {
                    foreach (string _word in Badwords.List)
                    {
                        if (_message.ToLower().Contains(_word))
                        {
                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _mainName + LoadConfig.Chat_Response_Color + " invalid word used in your message: " + _word + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            return false;
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
                                string _phrase971;
                                if (!Phrases.Dict.TryGetValue(971, out _phrase971))
                                {
                                    _phrase971 = " message is too long.";
                                }
                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase971 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                return false;
                            }
                            else if (Messages_Per_Min > 1)
                            {
                                if (ChatFloodLock.ContainsKey(_cInfo.entityId))
                                {
                                    DateTime _lockTime;
                                    ChatFloodLock.TryGetValue(_cInfo.entityId, out _lockTime);
                                    TimeSpan varTime = DateTime.Now - _lockTime;
                                    double fractionalSeconds = varTime.TotalSeconds;
                                    int _timepassed = (int)fractionalSeconds;
                                    if (_timepassed > Wait_Time)
                                    {
                                        ChatFloodPlayerList.Remove(_cInfo.entityId);
                                        ChatFloodTimeList.Remove(_cInfo.entityId);
                                        ChatFloodLock.Remove(_cInfo.entityId);
                                    }
                                    else
                                    {
                                        string _phrase970;
                                        if (!Phrases.Dict.TryGetValue(970, out _phrase970))
                                        {
                                            _phrase970 = " you have sent too many messages in too short a time. Your chat function is locked temporarily.";
                                        }
                                        ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase970 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                        return false;
                                    }
                                }
                                int _count = 0;
                                DateTime _chatTime = new DateTime();
                                if (ChatFloodPlayerList.ContainsKey(_cInfo.entityId))
                                {
                                    ChatFloodPlayerList.TryGetValue(_cInfo.entityId, out _count);
                                    ChatFloodTimeList.TryGetValue(_cInfo.entityId, out _chatTime);
                                    TimeSpan varTime = DateTime.Now - _chatTime;
                                    double fractionalSeconds = varTime.TotalSeconds;
                                    int _timepassed = (int)fractionalSeconds;
                                    if (_timepassed > 60)
                                    {
                                        ChatFloodTimeList[_cInfo.entityId] = DateTime.Now;
                                    }
                                }
                                else
                                {
                                    ChatFloodPlayerList.Add(_cInfo.entityId, 1);
                                    ChatFloodTimeList.Add(_cInfo.entityId, DateTime.Now);
                                }
                                if (_count + 1 >= Messages_Per_Min)
                                {
                                    if (_count + 1 == Messages_Per_Min)
                                    {
                                        ChatFloodPlayerList[_cInfo.entityId] = _count + 1;
                                        ChatFloodLock.Add(_cInfo.entityId, DateTime.Now);
                                    }
                                    string _phrase970;
                                    if (!Phrases.Dict.TryGetValue(970, out _phrase970))
                                    {
                                        _phrase970 = " you have sent too many messages in too short a time. Your chat function is locked temporarily.";
                                    }
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase970 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    return false;
                                }
                                else
                                {
                                    ChatFloodPlayerList[_cInfo.entityId] = _count + 1;
                                }
                            }
                        }
                        if (ChatColorPrefix.IsEnabled && ChatColorPrefix.dict.ContainsKey(_cInfo.playerId))
                        {
                            DateTime _dt;
                            ChatColorPrefix.dict1.TryGetValue(_cInfo.playerId, out _dt);
                            if (DateTime.Now < _dt)
                            {
                                string[] _colorPrefix;
                                ChatColorPrefix.dict.TryGetValue(_cInfo.playerId, out _colorPrefix);
                                if (ClanManager.IsEnabled && ClanManager.ClanMember.Contains(_cInfo.playerId))
                                {
                                    string _clanName = PersistentContainer.Instance.Players[_cInfo.playerId].ClanName;
                                    PrepMessage(_cInfo, _message, _senderId, _mainName, _colorPrefix[3], _clanName, _type, _recipientEntityIds);
                                    return false;
                                }
                                else
                                {
                                    PrepMessage(_cInfo, _message, _senderId, _mainName, _colorPrefix[3], _colorPrefix[2], _type, _recipientEntityIds);
                                    return false;
                                }
                            }
                            else
                            {
                                ChatColorPrefix.dict.Remove(_cInfo.playerId);
                                ChatColorPrefix.dict1.Remove(_cInfo.playerId);
                                ChatColorPrefix.UpdateXml();
                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " your chat color prefix time has expired.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        if (ClanManager.IsEnabled)
                        {
                            if (ClanManager.ClanMember.Contains(_cInfo.playerId))
                            {
                                string _clanName = PersistentContainer.Instance.Players[_cInfo.playerId].ClanName;
                                if (Normal_Player_Chat_Prefix)
                                {
                                    PrepMessage(_cInfo, _message, _senderId, _mainName, Normal_Player_Color, _clanName, _type, _recipientEntityIds);
                                    return false;
                                }
                                else
                                {
                                    PrepMessage(_cInfo, _message, _senderId, _mainName, "", _clanName, _type, _recipientEntityIds);
                                }
                                return false;
                            }
                        }
                        if (Normal_Player_Chat_Prefix)
                        {
                            PrepMessage(_cInfo, _message, _senderId, _mainName, Normal_Player_Color, Normal_Player_Prefix, _type, _recipientEntityIds);
                            return false;
                        }
                        if (Message_Color_Enabled)
                        {
                            _message = string.Format("{0}{1}[-]", Message_Color, _message);
                            ChatMessage(_cInfo, _message, -1, _cInfo.playerName, EChatType.Global, null);
                            return false;
                        }
                    }
                    if (_message.StartsWith(Command_Private) || _message.StartsWith(Command_Public))
                    {
                        if (CommandLog.IsEnabled)
                        {
                            CommandLog.Exec(_message, _mainName);
                        }
                        bool _announce = false;
                        if (_message.StartsWith(Command_Public))
                        {
                            _announce = true;
                            _message = _message.Replace(Command_Public, "");
                        }
                        if (_message.StartsWith(Command_Private))
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
                                    if (_announce)
                                    {
                                        ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you can not use sethome inside a zone.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                    }
                                    else
                                    {
                                        ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you can not use sethome inside a zone.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
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
                                            DateTime _dt;
                                            ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                            if (DateTime.Now < _dt)
                                            {

                                                TeleportHome.SetHome2(_cInfo);
                                            }
                                            else
                                            {
                                                if (_announce)
                                                {
                                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " your reserved status has expired Command is unavailable.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                                }
                                                else
                                                {
                                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " your reserved status has expired Command is unavailable.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (_announce)
                                            {
                                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you are not listed as a reserved player. Command is unavailable.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                            }
                                            else
                                            {
                                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you are not listed as a reserved player. Command is unavailable.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (_announce)
                                        {
                                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you can not use sethome2 in a protected zone.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                        }
                                        else
                                        {
                                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you can not use sethome2 in a protected zone.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                    }
                                }
                                else
                                {
                                    if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                    {
                                        DateTime _dt;
                                        ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                        if (DateTime.Now < _dt)
                                        {
                                            TeleportHome.SetHome2(_cInfo);
                                        }
                                        else
                                        {
                                            if (_announce)
                                            {
                                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " your reserved status has expired Command is unavailable.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                            }
                                            else
                                            {
                                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " your reserved status has expired Command is unavailable.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (_announce)
                                        {
                                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you are not listed as a reserved player. Command is unavailable.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                        }
                                        else
                                        {
                                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you are not listed as a reserved player. Command is unavailable.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                        }
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
                                        if (_announce)
                                        {
                                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you can not use sethome2 in a protected zone.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                        }
                                        else
                                        {
                                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you can not use sethome2 in a protected zone.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                        }
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
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        TeleportHome.Exec2(_cInfo);
                                    }
                                    else
                                    {
                                        if (_announce)
                                        {
                                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " your reserved status has expired. Command is unavailable.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                        }
                                        else
                                        {
                                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " your reserved status has expired. Command is unavailable.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                    }
                                }
                                else
                                {
                                    if (_announce)
                                    {
                                        ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you are not on the reserved list, please donate or contact an admin.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                    }
                                    else
                                    {
                                        ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you are not on the reserved list, please donate or contact an admin.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                            }
                            else if (!TeleportHome.Set_Home2_Reserved_Only)
                            {
                                TeleportHome.Exec2(_cInfo);
                            }
                            else
                            {
                                if (_announce)
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " home2 is not enabled.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                                else
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " home2 is not enabled.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            return false;
                        }
                        if (TeleportHome.Set_Home2_Enabled && _message.ToLower() == TeleportHome.Command7)
                        {
                            if (TeleportHome.Set_Home2_Reserved_Only && ReservedSlots.IsEnabled)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        TeleportHome.FExec2(_cInfo);
                                    }
                                    else
                                    {
                                        if (_announce)
                                        {
                                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " your reserved status has expired. Command is unavailable.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                        }
                                        else
                                        {
                                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " your reserved status has expired. Command is unavailable.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                    }
                                }
                                else
                                {
                                    if (_announce)
                                    {
                                        ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you are not on the reserved list, please donate or contact an admin.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                    }
                                    else
                                    {
                                        ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you are not on the reserved list, please donate or contact an admin.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                            }
                            else if (!TeleportHome.Set_Home2_Reserved_Only)
                            {
                                TeleportHome.FExec2(_cInfo);
                            }
                            else
                            {
                                if (_announce)
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " home2 is not enabled.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                                else
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " home2 is not enabled.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            return false;
                        }
                        if (TeleportHome.Set_Home2_Enabled && _message.ToLower() == TeleportHome.Command8)
                        {
                            if (TeleportHome.Set_Home2_Reserved_Only && ReservedSlots.IsEnabled)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        TeleportHome.DelHome2(_cInfo);
                                    }
                                    else
                                    {
                                        if (_announce)
                                        {
                                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " your reserved status has expired. Command is unavailable.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                        }
                                        else
                                        {
                                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " your reserved status has expired. Command is unavailable.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                    }
                                }
                                else
                                {
                                    if (_announce)
                                    {
                                        ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you are not on the reserved list, please donate or contact an admin.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                    }
                                    else
                                    {
                                        ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you are not on the reserved list, please donate or contact an admin.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
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
                        if (Waypoint.IsEnabled && _message.ToLower() == Waypoint.Command10)
                        {
                            if (Waypoint.Invite.ContainsKey(_cInfo.entityId))
                            {
                                Waypoint.FriendWaypoint(_cInfo);
                                return false;
                            }
                        }
                        if (Hardcore.IsEnabled && _message.ToLower() == Hardcore.Command11)
                        {
                            Hardcore.TopThree(_cInfo, _announce);
                            return false;
                        }
                        if (Hardcore.IsEnabled && _message.ToLower() == Hardcore.Command12)
                        {
                            Hardcore.Score(_cInfo, _announce);
                            return false;
                        }
                        if (Hardcore.IsEnabled && Hardcore.Max_Extra_Lives > 0 && Wallet.IsEnabled && _message.ToLower() == Hardcore.Command101)
                        {
                            Hardcore.BuyLife(_cInfo);
                            return false;
                        }
                        if (Mute.IsEnabled)
                        {
                            if (_message.ToLower().StartsWith(Mute.Command13) || _message.ToLower().StartsWith(Mute.Command14))
                            {
                                if (_message.ToLower().StartsWith(Mute.Command13 + " "))
                                {
                                    Mute.Add(_cInfo, _message);
                                }
                                if (_message.ToLower().StartsWith(Mute.Command14 + " "))
                                {
                                    Mute.Remove(_cInfo, _message);
                                }
                                return false;
                            }
                        }
                        if (_message.ToLower() == CustomCommands.Command15)
                        {
                            string _commands1 = CustomCommands.GetChatCommands1(_cInfo);
                            string _commands2 = CustomCommands.GetChatCommands2(_cInfo);
                            string _commands3 = CustomCommands.GetChatCommands3(_cInfo);
                            string _commands4 = CustomCommands.GetChatCommands4(_cInfo);
                            string _commands5 = CustomCommands.GetChatCommands5(_cInfo);
                            string _commands6 = CustomCommands.GetChatCommands6(_cInfo);
                            string _commandsCustom = CustomCommands.GetChatCommandsCustom(_cInfo);
                            string _commandsAdmin = CustomCommands.GetChatCommandsAdmin(_cInfo);
                            if (_announce)
                            {
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands1, -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                if (_commands2.EndsWith("More Commands:"))
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands2, -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                                if (_commands3.EndsWith("More Commands:"))
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands3, -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                                if (_commands4.EndsWith("More Commands:"))
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands4, -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                                if (_commands5.EndsWith("More Commands:"))
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands5, -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                                if (_commands6.EndsWith("More Commands:"))
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands6, -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                                if (CustomCommands.IsEnabled)
                                {
                                    if (_commandsCustom.EndsWith("Custom commands are:"))
                                    {
                                        ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commandsCustom, -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                    }
                                }
                                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                                if (Admin.PermissionLevel <= Admin_Level)
                                {
                                    if (_commandsAdmin.EndsWith("Admin commands are:"))
                                    {
                                        _commandsAdmin = string.Format("{0}Sorry, there are no admin chat commands.", LoadConfig.Chat_Response_Color);
                                    }
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commandsAdmin, -1, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                            }
                            else
                            {
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands1, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                if (!_commands2.EndsWith("More Commands:"))
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands2, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                if (!_commands3.EndsWith("More Commands:"))
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands3, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                if (!_commands4.EndsWith("More Commands:"))
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands4, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                if (!_commands5.EndsWith("More Commands:"))
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands5, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                if (!_commands6.EndsWith("More Commands:"))
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands6, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                if (CustomCommands.IsEnabled)
                                {
                                    if (!_commandsCustom.EndsWith("Custom commands are:"))
                                    {
                                        ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commandsCustom, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                                if (Admin.PermissionLevel <= Admin_Level)
                                {
                                    if (_commandsAdmin.EndsWith("Admin commands are:"))
                                    {
                                        _commandsAdmin = string.Format("{0}Sorry, there are no admin chat commands.", LoadConfig.Chat_Response_Color);
                                    }
                                    ChatMessage(_cInfo, _commandsAdmin, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            return false;
                        }
                        if (Day7.IsEnabled && (_message.ToLower() == Day7.Command16 || _message.ToLower() == Day7.Command17))
                        {
                            Day7.GetInfo(_cInfo, _announce);
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
                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " checking your claim block status.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            FirstClaimBlock.firstClaim(_cInfo);
                            return false;
                        }
                        if (ClanManager.IsEnabled && (_message.ToLower().StartsWith(ClanManager.Command33) || _message.ToLower() == ClanManager.Command34 || _message.ToLower().StartsWith(ClanManager.Command35) || _message.ToLower() == ClanManager.Command36 || _message.ToLower() == ClanManager.Command37 || _message.ToLower().StartsWith(ClanManager.Command38) || _message.ToLower().StartsWith(ClanManager.Command39) || _message.ToLower().StartsWith(ClanManager.Command40) || _message.ToLower().StartsWith(ClanManager.Command41) || _message.ToLower() == ClanManager.Command42 || _message.ToLower().StartsWith(ClanManager.Command43) || _message.ToLower().StartsWith(ClanManager.Command44) || _message.ToLower().StartsWith(ClanManager.Command45) || _message.ToLower().StartsWith(ClanManager.Command124) || _message.ToLower() == ClanManager.Command125))
                        {
                            if (_message.ToLower().StartsWith(ClanManager.Command33))
                            {
                                if (_message.ToLower() == ClanManager.Command33)
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", usage: " + ChatHook.Command_Private + ClanManager.Command33 + " clanName[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else
                                {
                                    _message = _message.Replace(ClanManager.Command33 + " ", "");
                                    ClanManager.AddClan(_cInfo, _message);
                                }
                                return false;
                            }
                            if (_message.ToLower() == ClanManager.Command34)
                            {
                                ClanManager.RemoveClan(_cInfo);
                                return false;
                            }
                            if (_message.ToLower().StartsWith(ClanManager.Command35))
                            {
                                if (_message.ToLower() == ClanManager.Command35)
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", usage: " + ChatHook.Command_Private + ClanManager.Command35 + " playerName[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else
                                {
                                    _message = _message.Replace(ClanManager.Command35 + " ", "");
                                    ClanManager.InviteMember(_cInfo, _message);
                                }
                                return false;
                            }
                            if (_message.ToLower() == ClanManager.Command36)
                            {
                                ClanManager.InviteAccept(_cInfo);
                                return false;
                            }
                            if (_message.ToLower() == ClanManager.Command37)
                            {
                                ClanManager.InviteDecline(_cInfo);
                                return false;
                            }
                            if (_message.ToLower().StartsWith(ClanManager.Command38))
                            {
                                if (_message.ToLower() == ClanManager.Command38)
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", usage: " + ChatHook.Command_Private + ClanManager.Command38 + " playerName[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else
                                {
                                    _message = _message.Replace(ClanManager.Command38 + " ", "");
                                    ClanManager.RemoveMember(_cInfo, _message);
                                }
                                return false;
                            }
                            if (_message.ToLower().StartsWith(ClanManager.Command39))
                            {
                                if (_message.ToLower() == ClanManager.Command39)
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", usage: " + ChatHook.Command_Private + ClanManager.Command39 + " playerName[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else
                                {
                                    _message = _message.Replace(ClanManager.Command39 + " ", "");
                                    ClanManager.PromoteMember(_cInfo, _message);
                                }
                                return false;
                            }
                            if (_message.ToLower().StartsWith(ClanManager.Command40))
                            {
                                if (_message.ToLower() == ClanManager.Command40)
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", usage: " + ChatHook.Command_Private + ClanManager.Command40 + " playerName[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else
                                {
                                    _message = _message.Replace(ClanManager.Command40 + " ", "");
                                    ClanManager.DemoteMember(_cInfo, _message);
                                }
                                return false;
                            }
                            if (_message.ToLower() == ClanManager.Command41)
                            {
                                ClanManager.LeaveClan(_cInfo);
                                return false;
                            }
                            if (_message.ToLower() == ClanManager.Command42)
                            {
                                string _clanCommands = ClanManager.GetChatCommands(_cInfo);
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _clanCommands, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                return false;
                            }
                            if (_message.ToLower().StartsWith(ClanManager.Command43) || _message.ToLower().StartsWith(ClanManager.Command124) && ClanManager.ClanMember.Contains(_cInfo.playerId))
                            {
                                if (_message.ToLower() == ClanManager.Command43 || _message.ToLower() == ClanManager.Command124)
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", usage: " + ChatHook.Command_Private + ClanManager.Command43 + " message or " + ChatHook.Command_Private + ClanManager.Command124 + " message[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else
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
                                }
                                return false;
                            }
                            if (_message.ToLower().StartsWith(ClanManager.Command44))
                            {
                                if (_message.ToLower() == ClanManager.Command44)
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", usage: " + ChatHook.Command_Private + ClanManager.Command44 + " newName[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else
                                {
                                    _message = _message.Replace(ClanManager.Command44 + " ", "");
                                    ClanManager.ClanRename(_cInfo, _message);
                                }
                                return false;
                            }
                            if (_message.ToLower().StartsWith(ClanManager.Command45))
                            {
                                _message = _message.Replace(ClanManager.Command45 + " ", "");
                                ClanManager.RequestToJoinClan(_cInfo, _message);
                                return false;
                            }
                            if (_message.ToLower() == ClanManager.Command125)
                            {
                                string _clanlist = ClanManager.GetClanList();
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _clanlist, -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                return false;
                            }
                        }
                        if (VoteReward.IsEnabled && _message.ToLower() == VoteReward.Command46)
                        {
                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " checking for your vote.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            VoteReward.Check(_cInfo);
                            return false;
                        }
                        if (AutoShutdown.IsEnabled && _message.ToLower() == AutoShutdown.Command47)
                        {
                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " checking for the next shutdown time.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            AutoShutdown.NextShutdown(_cInfo);
                            return false;
                        }
                        if (AdminList.IsEnabled && _message.ToLower() == AdminList.Command48)
                        {
                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " listing online administrators and moderators.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            AdminList.List(_cInfo, _announce, _mainName);
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
                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " sending you to your death point.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                        if (Shop.IsEnabled && Wallet.IsEnabled && _message.ToLower().StartsWith(Shop.Command58 + " "))//buy specific item and count
                        {
                            _message = _message.ToLower().Replace(Shop.Command58 + " ", "");
                            if (_message.ToLower().Length == 1)
                            {
                                Log.Out(string.Format("shop message string 2 = {0}", _message));
                                Shop.PosCheck(_cInfo, _message, 3, 1);
                            }
                            else if (_message.ToLower().Length == 2)
                            {
                                Log.Out(string.Format("shop message string 3 = {0}", _message));
                                string a = _message.Split(' ').First();
                                string b = _message.Split(' ').Last();
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
                                        _phrase620 = " the item amount # you are trying to buy is not an integer. Please input {CommandPrivate}{Command58} 1 2 for example.";
                                    }
                                    _phrase620 = _phrase620.Replace("{CommandPrivate}", ChatHook.Command_Private);
                                    _phrase620 = _phrase620.Replace("{Command58}", Shop.Command58);
                                    ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase620 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", usage: " + ChatHook.Command_Private + Shop.Command58 + " # or " + ChatHook.Command_Private + Shop.Command58 + " # #[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            return false;
                        }
                        if (Shop.IsEnabled && Wallet.IsEnabled && _message.ToLower().StartsWith(Shop.Command57 + " "))//show specific category
                        {
                            _message = _message.ToLower().Replace(Shop.Command57 + " ", "");
                            Log.Out(string.Format("shop message string 4 = {0}", _message));
                            Shop.PosCheck(_cInfo, _message, 2, 0);
                            return false;
                        }
                        if (FriendTeleport.IsEnabled && _message.ToLower().StartsWith(FriendTeleport.Command59 + " "))
                        {
                            _message = _message.ToLower().Replace(FriendTeleport.Command59 + " ", "");
                            FriendTeleport.Checks(_cInfo, _message);
                            return false;
                        }
                        if (FriendTeleport.IsEnabled && _message.ToLower() == FriendTeleport.Command59)
                        {
                            FriendTeleport.ListFriends(_cInfo, _message);
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
                            if (_timepassed <= 30)
                            {
                                FriendTeleport.TeleFriend(_cInfo, _dictValue);
                                FriendTeleport.Dict.Remove(_cInfo.entityId);
                                FriendTeleport.Dict1.Remove(_cInfo.entityId);
                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " your friend's teleport request was accepted.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                FriendTeleport.Dict.Remove(_cInfo.entityId);
                                FriendTeleport.Dict1.Remove(_cInfo.entityId);
                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " your friend's teleport request has expired.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            return false;
                        }
                        if (DeathSpot.IsEnabled && _message.ToLower() == (DeathSpot.Command61))
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
                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " a weather vote has already begun.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you have already voted.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                ChatMessage(_cInfo, _cInfo.playerName + " there is no active weather vote. Type " + ChatHook.Command_Private + WeatherVote.Command62 + " in chat to open a new vote.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you have already voted.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " there is no active weather vote. Type " + ChatHook.Command_Private + WeatherVote.Command62 + " in chat to open a new vote.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you have already voted.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " there is no active weather vote. Type " + ChatHook.Command_Private + WeatherVote.Command62 + " in chat to open a new vote.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                    _phrase824 = " there is a vote already open.";
                                }
                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase824 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            return false;
                        }
                        if (MuteVote.IsEnabled && Mute.IsEnabled && _message.ToLower().StartsWith(MuteVote.Command67))
                        {
                            if (!KickVote.VoteOpen && !RestartVote.VoteOpen && !MuteVote.VoteOpen)
                            {
                                if (_message.ToLower() == (MuteVote.Command67))
                                {
                                    MuteVote.List(_cInfo);
                                }
                                else if (_message.ToLower().StartsWith(MuteVote.Command67 + " "))
                                {
                                    _message = _message.ToLower().Replace(MuteVote.Command67 + " ", "");
                                    {
                                        MuteVote.Vote(_cInfo, _message);
                                    }
                                }
                            }
                            else
                            {
                                string _phrase824;
                                if (!Phrases.Dict.TryGetValue(824, out _phrase824))
                                {
                                    _phrase824 = " there is a vote already open.";
                                }
                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase824 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            return false;
                        }
                        if (KickVote.IsEnabled && _message.ToLower().StartsWith(KickVote.Command68))
                        {
                            if (!KickVote.VoteOpen && !RestartVote.VoteOpen && !MuteVote.VoteOpen)
                            {
                                if (_message.ToLower() == (KickVote.Command68))
                                {
                                    KickVote.List(_cInfo);
                                }
                                else if (_message.ToLower().StartsWith(KickVote.Command68 + " "))
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
                                    _phrase824 = " there is a vote already open.";
                                }
                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase824 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            return false;
                        }
                        if (_message.ToLower() == RestartVote.Command70)
                        {
                            if (KickVote.IsEnabled && KickVote.VoteOpen)
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
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you have already voted.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                            if (RestartVote.IsEnabled && RestartVote.VoteOpen)
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
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you have already voted.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                            if (MuteVote.IsEnabled && MuteVote.VoteOpen)
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
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you have already voted.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                        }
                        if (AuctionBox.IsEnabled && Wallet.IsEnabled && _message.ToLower() == AuctionBox.Command71)
                        {
                            AuctionBox.AuctionList(_cInfo);
                            return false;
                        }
                        if (AuctionBox.IsEnabled && Wallet.IsEnabled && _message.ToLower() == AuctionBox.Command72)
                        {
                            AuctionBox.CancelAuction(_cInfo);
                            return false;
                        }
                        if (AuctionBox.IsEnabled && Wallet.IsEnabled && _message.ToLower().StartsWith(AuctionBox.Command73 + " "))
                        {
                            if (AuctionBox.No_Admins)
                            {
                                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                                if (Admin.PermissionLevel <= Admin_Level)
                                {
                                    string _chatMessage = ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " the auction is disabled for your tier.[-]";
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
                                        ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you have used an auction item # that does not exist or has sold. Type " + ChatHook.Command_Private + AuctionBox.Command71 + ".[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                            }
                            return false;
                        }
                        if (AuctionBox.IsEnabled && Wallet.IsEnabled && _message.ToLower().StartsWith(AuctionBox.Command74 + " "))
                        {
                            if (AuctionBox.No_Admins)
                            {
                                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                                if (Admin.PermissionLevel > Admin_Level)
                                {
                                    _message = _message.ToLower().Replace(AuctionBox.Command74 + " ", "");
                                    AuctionBox.Delay(_cInfo, _message);
                                }
                                else
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " the auction is disabled for your tier.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                _message = _message.ToLower().Replace(AuctionBox.Command74 + " ", "");
                                AuctionBox.Delay(_cInfo, _message);
                            }
                            return false;
                        }
                        if (Fps.IsEnabled && _message.ToLower() == Fps.Command75)
                        {
                            Fps.FPS(_cInfo, _announce);
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
                        if (_message.ToLower() == PollConsole.Command91)
                        {
                            PollConsole.VoteYes(_cInfo);
                            return false;
                        }
                        if (_message.ToLower() == PollConsole.Command92)
                        {
                            PollConsole.VoteNo(_cInfo);
                            return false;
                        }
                        if (_message.ToLower() == PollConsole.Command93)
                        {
                            string _sql = "SELECT pollMessage, pollYes, pollNo FROM Polls WHERE pollOpen = 'true'";
                            DataTable _result = SQL.TypeQuery(_sql);
                            if (_result.Rows.Count > 0)
                            {
                                string _phrase926;
                                if (!Phrases.Dict.TryGetValue(926, out _phrase926))
                                {
                                    _phrase926 = "Poll: {Message}";
                                }
                                string _pollMessage = _result.Rows[0].ItemArray.GetValue(0).ToString();
                                _phrase926 = _phrase926.Replace("{Message}", _pollMessage);
                                string _phrase813;
                                if (!Phrases.Dict.TryGetValue(813, out _phrase813))
                                {
                                    _phrase813 = "Currently, the pole is yes {YesCount} / no {NoCount}.";
                                }
                                int _pollYes;
                                int.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _pollYes);
                                int _pollNo;
                                int.TryParse(_result.Rows[0].ItemArray.GetValue(2).ToString(), out _pollNo);
                                _phrase813 = _phrase813.Replace("{YesCount}", _pollYes.ToString());
                                _phrase813 = _phrase813.Replace("{NoCount}", _pollNo.ToString());
                                if (_announce)
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase926 + "[-]", _senderId, _cInfo.playerName, EChatType.Global, null);
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase813 + "[-]", _senderId, _cInfo.playerName, EChatType.Global, null);
                                }
                                else
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase926 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase813 + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
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
                                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " the server has wallet to bank account transfers turned off.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                ChatHook.ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " the server has bank account to wallet transfers turned off.[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                            if (!InfoTicker.exemptionList.Contains(_cInfo.playerId))
                            {
                                InfoTicker.exemptionList.Add(_cInfo.playerId);
                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you have turned off infoticker messages until the server restarts.", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                InfoTicker.exemptionList.Remove(_cInfo.playerId);
                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you have turned on infoticker messages.", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            return false;
                        }
                        if (Session.IsEnabled && _message.ToLower() == Session.Command105)
                        {
                            Session.Exec(_cInfo);
                            return false;
                        }
                        if (Waypoint.IsEnabled && (_message.ToLower() == Waypoint.Command106 || _message.ToLower() == Waypoint.Command107 || _message.ToLower() == Waypoint.Command108))
                        {
                            Waypoint.List(_cInfo);
                            return false;
                        }
                        else if (Waypoint.IsEnabled && (_message.StartsWith(Waypoint.Command106 + " ") || _message.StartsWith(Waypoint.Command107 + " ") || _message.StartsWith(Waypoint.Command108 + " ")))
                        {
                            if (_message.ToLower().StartsWith(Waypoint.Command106 + " "))
                            {
                                _message = _message.ToLower().Replace(Waypoint.Command106 + " ", "");
                            }
                            else if (_message.ToLower().StartsWith(Waypoint.Command107 + " "))
                            {
                                _message = _message.ToLower().Replace(Waypoint.Command107 + " ", "");
                            }
                            else
                            {
                                _message = _message.ToLower().Replace(Waypoint.Command108 + " ", "");
                            }
                            Waypoint.TeleDelay(_cInfo, _message);
                            return false;
                        }
                        if (Waypoint.IsEnabled && (_message.StartsWith(Waypoint.Command109 + " ") || _message.StartsWith(Waypoint.Command110 + " ") || _message.StartsWith(Waypoint.Command111 + " ")))
                        {
                            string _waypointName = "";
                            if (_message.StartsWith(Waypoint.Command109 + " "))
                            {
                                _waypointName = _message.ToLower().Replace(Waypoint.Command109 + " ", "");
                                if (_waypointName != " " || _waypointName != "")
                                {
                                    Waypoint.FDelay(_cInfo, _waypointName);
                                    return false;
                                }
                            }
                            else if (_message.StartsWith(Waypoint.Command110 + " "))
                            {
                                _waypointName = _message.ToLower().Replace(Waypoint.Command110 + " ", "");
                                if (_waypointName != " " || _waypointName != "")
                                {
                                    Waypoint.FDelay(_cInfo, _waypointName);
                                    return false;
                                }
                            }
                            else if (_message.StartsWith(Waypoint.Command111 + " "))
                            {
                                _waypointName = _message.ToLower().Replace(Waypoint.Command111 + " ", "");
                                if (_waypointName != " " || _waypointName != "")
                                {
                                    Waypoint.FDelay(_cInfo, _waypointName);
                                    return false;
                                }
                            }
                        }
                        if (Waypoint.IsEnabled && (_message.ToLower().StartsWith(Waypoint.Command112 + " ") || _message.ToLower().StartsWith(Waypoint.Command113 + " ") || _message.ToLower().StartsWith(Waypoint.Command114 + " ")))
                        {
                            if (_message.ToLower().StartsWith(Waypoint.Command112 + " "))
                            {
                                _message = _message.ToLower().Replace(Waypoint.Command112 + " ", "");
                            }
                            else if (_message.ToLower().StartsWith(Waypoint.Command113 + " "))
                            {
                                _message = _message.ToLower().Replace(Waypoint.Command113 + " ", "");
                            }
                            else
                            {
                                _message = _message.ToLower().Replace(Waypoint.Command114 + " ", "");
                            }
                            Waypoint.SaveClaimCheck(_cInfo, _message);
                            return false;
                        }
                        if (Waypoint.IsEnabled && (_message.ToLower().StartsWith(Waypoint.Command115 + " ") || _message.ToLower().StartsWith(Waypoint.Command116 + " ") || _message.ToLower().StartsWith(Waypoint.Command117 + " ")))
                        {
                            if (_message.ToLower().StartsWith(Waypoint.Command115 + " "))
                            {
                                _message = _message.ToLower().Replace(Waypoint.Command115 + " ", "");
                            }
                            else if (_message.ToLower().StartsWith(Waypoint.Command116 + " "))
                            {
                                _message = _message.ToLower().Replace(Waypoint.Command116 + " ", "");
                            }
                            else
                            {
                                _message = _message.ToLower().Replace(Waypoint.Command117 + " ", "");
                            }
                            Waypoint.DelPoint(_cInfo, _message);
                            return false;
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
                        if (Hardcore.IsEnabled && Hardcore.Optional && _message.ToLower() == Hardcore.Command128)
                        {
                            if (!PersistentContainer.Instance.Players[_cInfo.playerId].Hardcore)
                            {
                                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                int _deaths = XUiM_Player.GetDeaths(_player);
                                string _sql = string.Format("SELECT * FROM Hardcore WHERE steamid = '{0}'", _cInfo.playerId);
                                DataTable _result = SQL.TypeQuery(_sql);
                                if (_result.Rows.Count == 0)
                                {
                                    SQL.FastQuery(string.Format("INSERT INTO Hardcore (steamid, playerName, deaths) VALUES ('{0}', '{1}', {2})", _cInfo.playerId, _cInfo.playerName, _deaths), null);
                                }
                                else
                                {
                                    _sql = string.Format("UPDATE Hardcore SET deaths = {0} WHERE steamid = '{1}'", _deaths, _cInfo.playerId);
                                    SQL.FastQuery(_sql, "Chathook");
                                }
                                _result.Dispose();
                                PersistentContainer.Instance.Players[_cInfo.playerId].Hardcore = true;
                                PersistentContainer.Instance.Save();
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You are now in hardcore mode with limited lives remaining. Type " + ChatHook.Command_Private + Hardcore.Command127 + " to check how many remain." + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You have already signed up for hardcore mode. Type " + ChatHook.Command_Private + Hardcore.Command127 + " to check how many remain." + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            return false;
                        }
                        if (Hardcore.IsEnabled && _message.ToLower() == Hardcore.Command127)
                        {
                            if (Hardcore.Optional)
                            {
                                if (PersistentContainer.Instance.Players[_cInfo.playerId].Hardcore)
                                {
                                    Hardcore.Alert(_cInfo);
                                }
                                else
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "You have not signed up for hardcore mode. Type " + ChatHook.Command_Private + Hardcore.Command128 + " to enter hardcore mode. This can not be undone." + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                Hardcore.Alert(_cInfo);
                            }
                            return false;
                        }
                        if (BattleLogger.IsEnabled && (_message.ToLower() == BattleLogger.Command131 || _message.ToLower() == BattleLogger.Command132) && BattleLogger.Players.ContainsKey(_cInfo.playerId))
                        {
                            Timers.BattleLogPlayerExit(_cInfo.playerId);
                            ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Please wait 15 seconds for disconnection and do not move" + "[-]", -1, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            return false;
                        }
                        if (_message.ToLower() == "sttest")
                        {
                            Log.Out("ST Test detected");
                            PersistentOperations.TestExec(_cInfo);
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

        public static void PrepMessage(ClientInfo _cInfo, string _message, int _senderId, string _mainName, string _param1, string _param2, EChatType _type, List<int> _recipientEntityIds)
        {
            if (Message_Color_Enabled)
            {
                _message = string.Format("{0}{1}[-]", Message_Color, _message);
            }
            if (_param1 != "")
            {
                if (_type == EChatType.Friends)
                {
                    if (_param2 == "")
                    {
                        ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0}(Friends) {1}[-]", _param1, _mainName), _type, _recipientEntityIds);
                    }
                    else
                    {
                        ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0}(Friends){1} {2}[-]", _param1, _param2, _mainName), _type, _recipientEntityIds);
                    }
                }
                else if (_type == EChatType.Party)
                {
                    if (_param2 == "")
                    {
                        ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0}(Party) {1}[-]", _param1, _mainName), _type, _recipientEntityIds);
                    }
                    else
                    {
                        ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0}(Party){1} {2}[-]", _param1, _param2, _mainName), _type, _recipientEntityIds);
                    }
                }
                else
                {
                    if (_param2 == "")
                    {
                        ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0}{1}[-]", _param1, _mainName), _type, _recipientEntityIds);
                    }
                    else
                    {
                        ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0}{1} {2}[-]", _param1, _param2, _mainName), _type, _recipientEntityIds);
                    }
                }
            }
            else
            {
                if (_type == EChatType.Friends)
                {
                    ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("(Friends){0} {1}[-]", _param2, _mainName), _type, _recipientEntityIds);
                }
                else if (_type == EChatType.Party)
                {
                    ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("(Party){0} {1}[-]", _param2, _mainName), _type, _recipientEntityIds);
                }
                else
                {
                    ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0} {1}[-]", _param2, _mainName), _type, _recipientEntityIds);
                }
            }
        }

        public static void ChatMessage(ClientInfo _cInfo, string _message, int _senderId, string _name, EChatType _type, List<int> _recipientEntityIds)
        {
            if (_type == EChatType.Whisper)
            {
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, _message, _name, false, null));
            }
            else if (_type == EChatType.Global)
            {
                GameManager.Instance.ChatMessageServer(_cInfo, EChatType.Global, -1, _message, _name, false, _recipientEntityIds);
            }
            else if (_type == EChatType.Friends)
            {
                EntityPlayer _player = PersistentOperations.GetEntityPlayer(_cInfo.playerId);
                if (_player != null)
                {
                    List<EntityPlayer> _playerList = PersistentOperations.PlayerList();
                    for (int i = 0; i < _playerList.Count; i++)
                    {
                        EntityPlayer _player2 = _playerList[i];
                        if (_player != null && _player.IsFriendsWith(_player2))
                        {
                            ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForEntityId(_player2.entityId);
                            if (Friend_Chat_Color.StartsWith("[") && Friend_Chat_Color.EndsWith("]"))
                            {
                                _message = _message.Insert(0, Friend_Chat_Color);
                            }
                            _cInfo2.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, _message, _name, false, null));
                        }
                    }
                }
                if (Friend_Chat_Color.StartsWith("[") && Friend_Chat_Color.EndsWith("]"))
                {
                    _message = _message.Insert(0, Friend_Chat_Color);
                }
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, _message, _name, false, null));
            }
            else if (_type == EChatType.Party)
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                List<EntityPlayer> _party = _player.Party.MemberList;
                for (int i = 0; i < _party.Count; i++)
                {
                    Entity _member = _party[i];
                    if (_member != null)
                    {
                        ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForEntityId(_member.entityId);
                        if (_cInfo2 != null)
                        {
                            if (Party_Chat_Color.StartsWith("[") && Party_Chat_Color.EndsWith("]"))
                            {
                                _message = _message.Insert(0, Party_Chat_Color);
                            }
                            _cInfo2.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, _message, _name, false, null));
                        }
                    }
                }
            }
        }
    }
}