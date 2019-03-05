using System;
using System.Collections.Generic;
using System.Data;

namespace ServerTools
{
    public class ChatHook
    {
        public static bool ChatFlood = false;
        public static bool Normal_Player_Name_Coloring = false;
        public static string Normal_Player_Color = "[00B3B3]";
        public static string Normal_Player_Prefix = "(NOOB)";
        public static string Friend_Chat_Color = "[33CC33]", Party_Chat_Color = "[FFCC00]", Player_Name_Color = "[00FF00]";
        public static int Admin_Level = 0, Mod_Level = 1;
        public static int Max_Length = 250, Messages_Per_Min = 5;
        public static bool ChatCommandPrivateEnabled = false, ChatCommandPublicEnabled = false;
        public static string Command_Private = "/", Command_Public = "!";
        private static SortedDictionary<string, DateTime> Dict = new SortedDictionary<string, DateTime>();
        private static SortedDictionary<string, string> Dict1 = new SortedDictionary<string, string>();

        public static bool Hook(ClientInfo _cInfo, EChatType _type, int _senderId, string _message, string _mainName, bool _localizeMain, List<int> _recipientEntityIds)
        {
            if (!string.IsNullOrEmpty(_message) && _cInfo != null && _mainName != LoadConfig.Server_Response_Name)
            {
                if (ChatFlood)
                {
                    if (_message.Length > Max_Length)
                    {
                        string _phrase971;
                        if (!Phrases.Dict.TryGetValue(971, out _phrase971))
                        {
                            _phrase971 = " message is too long.";
                        }
                        ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color  + _phrase971 + "[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        return false;
                    }
                    string _sql = string.Format("SELECT messageCount, messageTime FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                    DataTable _result = SQL.TQuery(_sql);
                    int _count;
                    int.TryParse(_result.Rows[0].ItemArray.GetValue(0).ToString(), out _count);
                    DateTime _chatTime;
                    DateTime.TryParse(_result.Rows[0].ItemArray.GetValue(1).ToString(), out _chatTime);
                    _result.Dispose();
                    TimeSpan varTime = DateTime.Now - _chatTime;
                    double fractionalSeconds = varTime.TotalSeconds;
                    int _timepassed = (int)fractionalSeconds;
                    if (_count >= Messages_Per_Min)
                    {
                        if (_timepassed < 60)
                        {
                            string _phrase970;
                            if (!Phrases.Dict.TryGetValue(970, out _phrase970))
                            {
                                _phrase970 = " you have sent too many messages in one minute.";
                            }
                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color  + _phrase970 + "[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            return false;
                        }
                        else
                        {
                            _sql = string.Format("UPDATE Players SET messageCount = 1, messageTime = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
                            SQL.FastQuery(_sql);
                        }
                    }
                    else
                    {
                        if (_timepassed < 60)
                        {
                            _sql = string.Format("UPDATE Players SET messageCount = {0} WHERE steamid = '{1}'", _count + 1, _cInfo.playerId);
                            SQL.FastQuery(_sql);
                        }
                        else
                        {
                            _sql = string.Format("UPDATE Players SET messageCount = 1, messageTime = '{0}' WHERE steamid = '{1}'", DateTime.Now, _cInfo.playerId);
                            SQL.FastQuery(_sql);
                        }
                    }
                    
                }
                if (ChatLog.IsEnabled)
                {
                    ChatLog.Log(_message, _mainName);
                }
                if (MutePlayer.IsEnabled)
                {
                    if (MutePlayer.Mutes.Contains(_cInfo.playerId))
                    {
                        ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", you are muted.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        return false;
                    }
                }
                if (Badwords.Invalid_Name)
                {
                    bool _hasBadName = false;
                    string _mainName1 = _mainName.ToLower();
                    foreach (string _word in Badwords.List)
                    {
                        if (_mainName1.Contains(_word))
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
                        ChatMessage(_cInfo, ChatHook.Player_Name_Color + _mainName + LoadConfig.Chat_Response_Color + ", invalid Name-No Commands[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                        return false;
                    }
                }
                if (Badwords.IsEnabled)
                {
                    string _message1 = _message.ToLower();
                    foreach (string _word in Badwords.List)
                    {
                        if (_message1.Contains(_word))
                        {
                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _mainName + LoadConfig.Chat_Response_Color + ", invalid word used: " + _word + "[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            return false;
                        }
                    }
                }
                if (!Jail.Jailed.Contains(_cInfo.playerId))
                {
                    if (ChatColorPrefix.IsEnabled && !_message.StartsWith("@") && _mainName != LoadConfig.Server_Response_Name && _senderId != 33 && !_message.StartsWith(Command_Private) && !_message.StartsWith(Command_Public) && ChatColorPrefix.dict.ContainsKey(_cInfo.playerId))
                    {
                        string[] _colorPrefix;
                        if (ChatColorPrefix.dict.TryGetValue(_cInfo.playerId, out _colorPrefix))
                        {
                            if (!ClanManager.ClanMember.Contains(_cInfo.playerId))
                            {
                                if (_colorPrefix[2] != "")
                                {
                                    if (_type == EChatType.Friends)
                                    {
                                        ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0}(Friends){1} {2}[-]", _colorPrefix[3], _colorPrefix[2], _mainName), _type, _recipientEntityIds);
                                    }
                                    else if (_type == EChatType.Party)
                                    {
                                        ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0}(Party){1} {2}[-]", _colorPrefix[3], _colorPrefix[2], _mainName), _type, _recipientEntityIds);
                                    }
                                    else
                                    {
                                        ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0}{1} {2}[-]", _colorPrefix[3], _colorPrefix[2], _mainName), _type, _recipientEntityIds);
                                    }
                                }
                                else
                                {
                                    if (_type == EChatType.Friends)
                                    {
                                        ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0}(Friends){1}[-]", _colorPrefix[3], _mainName), _type, _recipientEntityIds);
                                    }
                                    else if (_type == EChatType.Party)
                                    {
                                        ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0}(Party){1}[-]", _colorPrefix[3], _mainName), _type, _recipientEntityIds);
                                    }
                                    else
                                    {
                                        ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0}{1}[-]", _colorPrefix[3], _mainName), _type, _recipientEntityIds);
                                    }
                                    
                                }
                            }
                            else
                            {
                                string _sql = string.Format("SELECT clanname FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                                DataTable _result = SQL.TQuery(_sql);
                                string _clanname = _result.Rows[0].ItemArray.GetValue(0).ToString();
                                _result.Dispose();
                                if (_colorPrefix[2] != "")
                                {
                                    if (_type == EChatType.Friends)
                                    {
                                        ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0}(Friends)({1}){2} {3}[-]", _colorPrefix[3], _colorPrefix[2], _mainName), _type, _recipientEntityIds);
                                    }
                                    else if (_type == EChatType.Party)
                                    {
                                        ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0}(Party)({1}){2} {3}[-]", _colorPrefix[3], _colorPrefix[2], _mainName), _type, _recipientEntityIds);
                                    }
                                    else
                                    {
                                        ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0}({1}){2} {3}[-]", _colorPrefix[3], _colorPrefix[2], _mainName), _type, _recipientEntityIds);
                                    }
                                }
                                else
                                {
                                    if (_type == EChatType.Friends)
                                    {
                                        ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0}(Friends)({1}) {2}[-]", _colorPrefix[3], _clanname, _mainName), _type, _recipientEntityIds);
                                    }
                                    else if (_type == EChatType.Party)
                                    {
                                        ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0}(Party)({1}) {2}[-]", _colorPrefix[3], _clanname, _mainName), _type, _recipientEntityIds);
                                    }
                                    else
                                    {
                                        ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0}({1}) {2}[-]", _colorPrefix[3], _clanname, _mainName), _type, _recipientEntityIds);
                                    }
                                }
                            }
                        }
                        return false;
                    }
                    if (Normal_Player_Name_Coloring && !_message.StartsWith("@") && _mainName != LoadConfig.Server_Response_Name && _senderId != 33 && !_message.StartsWith(Command_Private) && !_message.StartsWith(Command_Public))
                    {
                        if (!ClanManager.ClanMember.Contains(_cInfo.playerId))
                        {
                            if (Normal_Player_Prefix != "")
                            {
                                if (_type == EChatType.Friends)
                                {
                                    ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0}(Friends){1} {2}[-]", Normal_Player_Color, Normal_Player_Prefix, _mainName), _type, _recipientEntityIds);
                                }
                                else if (_type == EChatType.Party)
                                {
                                    ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0}(Party){1} {2}[-]", Normal_Player_Color, Normal_Player_Prefix, _mainName), _type, _recipientEntityIds);
                                }
                                else
                                {
                                    ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0}{1} {2}[-]", Normal_Player_Color, Normal_Player_Prefix, _mainName), _type, _recipientEntityIds);
                                }
                            }
                            else
                            {
                                if (_type == EChatType.Friends)
                                {
                                    ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0}(Friends){1}[-]", Normal_Player_Color, _mainName), _type, _recipientEntityIds);
                                }
                                else if (_type == EChatType.Party)
                                {
                                    ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0}(Party){1}[-]", Normal_Player_Color, _mainName), _type, _recipientEntityIds);
                                }
                                else
                                {
                                    ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0}{1}[-]", Normal_Player_Color, _mainName), _type, _recipientEntityIds);
                                }
                            }
                        }
                        else
                        {
                            string _sql = string.Format("SELECT clanname FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                            DataTable _result = SQL.TQuery(_sql);
                            string _clanname = _result.Rows[0].ItemArray.GetValue(0).ToString();
                            _result.Dispose();
                            if (Normal_Player_Prefix != "")
                            {
                                if (_type == EChatType.Friends)
                                {
                                    ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0}(Friends)({1}){2} {3}[-]", Normal_Player_Color, _clanname, Normal_Player_Prefix, _mainName), _type, _recipientEntityIds);
                                }
                                else if (_type == EChatType.Party)
                                {
                                    ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0}(Party)({1}){2} {3}[-]", Normal_Player_Color, _clanname, Normal_Player_Prefix, _mainName), _type, _recipientEntityIds);
                                }
                                else
                                {
                                    ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0}({1}){2} {3}[-]", Normal_Player_Color, _clanname, Normal_Player_Prefix, _mainName), _type, _recipientEntityIds);
                                }
                            }
                            else
                            {
                                if (_type == EChatType.Friends)
                                {
                                    ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0}(Friends)({1}) {2}[-]", Normal_Player_Color, _clanname, _mainName), _type, _recipientEntityIds);
                                }
                                else if (_type == EChatType.Party)
                                {
                                    ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0}(Party)({1}) {2}[-]", Normal_Player_Color, _clanname, _mainName), _type, _recipientEntityIds);
                                }
                                else
                                {
                                    ChatMessage(_cInfo, _message, _senderId, _mainName = string.Format("{0}({1}) {2}[-]", Normal_Player_Color, _clanname, _mainName), _type, _recipientEntityIds);
                                }
                                
                            }
                        }
                        return false;
                    }
                    if (ClanManager.IsEnabled && !_message.StartsWith("@") && _mainName != LoadConfig.Server_Response_Name && _senderId != 33 && !_message.StartsWith(Command_Private) && !_message.StartsWith(Command_Public) && !_message.StartsWith(ClanManager.Command43) && !_message.StartsWith(ClanManager.Command124) && ClanManager.ClanMember.Contains(_cInfo.playerId))
                    {
                        string _sql = string.Format("SELECT clanname FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                        DataTable _result = SQL.TQuery(_sql);
                        string _clanname = _result.Rows[0].ItemArray.GetValue(0).ToString();
                        _result.Dispose();
                        _mainName = string.Format("({0}) {1}", _clanname, _mainName);
                        ChatMessage(_cInfo, _message, _senderId, _mainName, EChatType.Whisper, null);
                        return false;
                    }
                    if (_message.StartsWith(" "))
                    {
                        _message.Substring(1);
                    }
                    if (_message.StartsWith("  "))
                    {
                        _message.Substring(2);
                    }
                    if (_message.StartsWith("[") && _message.Contains("]") || _message.Contains(Command_Private) || _message.Contains(Command_Public) || _message.StartsWith(Command_Private) || _message.StartsWith(Command_Public))
                    {
                        if (_message.StartsWith("[") && _message.Contains("]"))
                        {
                            _message = _message.Replace("(\\[.*\\] )", "");
                        }
                        if (_message.StartsWith("(") && _message.Contains(")"))
                        {
                            _message = _message.Replace("(\\(.*\\) )", "");
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
                        if (Whisper.IsEnabled && _message.ToLower().StartsWith(Whisper.Command122))
                        {
                            Whisper.Send(_cInfo, _message);
                            return false;
                        }
                        if (Whisper.IsEnabled && _message.ToLower().StartsWith(Whisper.Command123))
                        {
                            Whisper.Reply(_cInfo, _message);
                            return false;
                        }
                        if (TeleportHome.IsEnabled && _message.ToLower() == TeleportHome.Command1)
                        {
                            if (Zones.IsEnabled && !Zones.Set_Home)
                            {
                                if (!Zones.ZoneExit.ContainsKey(_cInfo.entityId))
                                {
                                    TeleportHome.SetHome(_cInfo, _mainName, _announce);
                                }
                                else
                                {
                                    if (_announce)
                                    {
                                        ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", you can not use sethome in a protected zone.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                    }
                                    else
                                    {
                                        ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", you can not use sethome in a protected zone.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                            }
                            else
                            {
                                TeleportHome.SetHome(_cInfo, _mainName, _announce);
                            }
                            return false;
                        }
                        if (TeleportHome.IsEnabled && _message.ToLower() == TeleportHome.Command2)
                        {

                            TeleportHome.Check(_cInfo, _mainName, _announce);
                            return false;
                        }
                        if (TeleportHome.IsEnabled && _message.ToLower() == TeleportHome.Command3)
                        {

                            TeleportHome.FCheck(_cInfo, _mainName, _announce);
                            return false;
                        }
                        if (TeleportHome.IsEnabled && _message.ToLower() == TeleportHome.Command4)
                        {

                            TeleportHome.DelHome(_cInfo, _mainName, _announce);
                            return false;
                        }
                        if (TeleportHome.Set_Home2_Enabled && _message.ToLower() == TeleportHome.Command5)
                        {
                            if (TeleportHome.Set_Home2_Donor_Only && ReservedSlots.IsEnabled)
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

                                                TeleportHome.SetHome2(_cInfo, _mainName, _announce);
                                            }
                                            else
                                            {
                                                if (_announce)
                                                {
                                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", your reserved status has expired Command is unavailable.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                                }
                                                else
                                                {
                                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", your reserved status has expired Command is unavailable.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (_announce)
                                            {
                                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", you are not listed as a reserved player. Command is unavailable.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                            }
                                            else
                                            {
                                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", you are not listed as a reserved player. Command is unavailable.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (_announce)
                                        {
                                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", you can not use sethome2 in a protected zone.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                        }
                                        else
                                        {
                                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", you can not use sethome2 in a protected zone.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                            TeleportHome.SetHome2(_cInfo, _mainName, _announce);
                                        }
                                        else
                                        {
                                            if (_announce)
                                            {
                                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", your reserved status has expired Command is unavailable.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                            }
                                            else
                                            {
                                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", your reserved status has expired Command is unavailable.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (_announce)
                                        {
                                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", you are not listed as a reserved player. Command is unavailable.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                        }
                                        else
                                        {
                                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", you are not listed as a reserved player. Command is unavailable.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                    }
                                }
                            }
                            else if (!TeleportHome.Set_Home2_Donor_Only)
                            {
                                if (!Zones.Set_Home)
                                {
                                    if (!Zones.ZoneExit.ContainsKey(_cInfo.entityId))
                                    {
                                        TeleportHome.SetHome2(_cInfo, _mainName, _announce);
                                    }
                                    else
                                    {
                                        if (_announce)
                                        {
                                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", you can not use sethome2 in a protected zone.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                        }
                                        else
                                        {
                                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", you can not use sethome2 in a protected zone.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                    }
                                }
                                else
                                {
                                    TeleportHome.SetHome2(_cInfo, _mainName, _announce);
                                }
                            }
                            return false;
                        }
                        if (TeleportHome.Set_Home2_Enabled && _message.ToLower() == TeleportHome.Command6)
                        {
                            if (TeleportHome.Set_Home2_Donor_Only && ReservedSlots.IsEnabled)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        TeleportHome.Check2(_cInfo, _mainName, _announce);
                                    }
                                    else
                                    {
                                        if (_announce)
                                        {
                                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", your reserved status has expired. Command is unavailable.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                        }
                                        else
                                        {
                                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", your reserved status has expired. Command is unavailable.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                    }
                                }
                                else
                                {
                                    if (_announce)
                                    {
                                        ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", you are not on the reserved list, please donate or contact an admin.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                    }
                                    else
                                    {
                                        ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", you are not on the reserved list, please donate or contact an admin.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                            }
                            else if (!TeleportHome.Set_Home2_Donor_Only)
                            {
                                TeleportHome.Check2(_cInfo, _mainName, _announce);
                            }
                            else
                            {
                                if (_announce)
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", home2 is not enabled.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                                else
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", home2 is not enabled.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            return false;
                        }
                        if (TeleportHome.Set_Home2_Enabled && _message.ToLower() == TeleportHome.Command7)
                        {
                            if (TeleportHome.Set_Home2_Donor_Only && ReservedSlots.IsEnabled)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        TeleportHome.FCheck2(_cInfo, _mainName, _announce);
                                    }
                                    else
                                    {
                                        if (_announce)
                                        {
                                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", your reserved status has expired. Command is unavailable.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                        }
                                        else
                                        {
                                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", your reserved status has expired. Command is unavailable.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                    }
                                }
                                else
                                {
                                    if (_announce)
                                    {
                                        ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", you are not on the reserved list, please donate or contact an admin.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                    }
                                    else
                                    {
                                        ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", you are not on the reserved list, please donate or contact an admin.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                            }
                            else if (!TeleportHome.Set_Home2_Donor_Only)
                            {
                                TeleportHome.FCheck2(_cInfo, _mainName, _announce);
                            }
                            else
                            {
                                if (_announce)
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", home2 is not enabled.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                                else
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", home2 is not enabled.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            return false;
                        }
                        if (TeleportHome.Set_Home2_Enabled && _message.ToLower() == TeleportHome.Command8)
                        {
                            if (TeleportHome.Set_Home2_Donor_Only && ReservedSlots.IsEnabled)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        TeleportHome.DelHome2(_cInfo, _mainName, _announce);
                                    }
                                    else
                                    {
                                        if (_announce)
                                        {
                                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", your reserved status has expired. Command is unavailable.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                        }
                                        else
                                        {
                                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", your reserved status has expired. Command is unavailable.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                    }
                                }
                                else
                                {
                                    if (_announce)
                                    {
                                        ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", you are not on the reserved list, please donate or contact an admin.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                    }
                                    else
                                    {
                                        ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", you are not on the reserved list, please donate or contact an admin.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                            }
                            else if (!TeleportHome.Set_Home2_Donor_Only)
                            {
                                TeleportHome.DelHome2(_cInfo, _mainName, _announce);
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
                        if (AdminChat.IsEnabled)
                        {
                            if (_message.ToLower().StartsWith(MutePlayer.Command13) || _message.ToLower().StartsWith(MutePlayer.Command14))
                            {
                                if (_message.ToLower().StartsWith(MutePlayer.Command13 + " "))
                                {
                                    MutePlayer.Add(_cInfo, _message);
                                }
                                if (_message.ToLower().StartsWith(MutePlayer.Command14 + " "))
                                {
                                    MutePlayer.Remove(_cInfo, _message);
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
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands1, _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                if (_commands2.EndsWith("More Commands:"))
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands2, _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                                if (_commands3.EndsWith("More Commands:"))
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands3, _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                                if (_commands4.EndsWith("More Commands:"))
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands4, _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                                if (_commands5.EndsWith("More Commands:"))
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands5, _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                                if (_commands6.EndsWith("More Commands:"))
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands6, _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                                if (CustomCommands.IsEnabled)
                                {
                                    if (_commandsCustom.EndsWith("Custom commands are:"))
                                    {
                                        ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commandsCustom, _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                    }
                                }
                                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                                if (Admin.PermissionLevel <= Admin_Level)
                                {
                                    if (_commandsAdmin.EndsWith("Admin commands are:"))
                                    {
                                        _commandsAdmin = string.Format("{0}Sorry, there are no admin chat commands.", LoadConfig.Chat_Response_Color);
                                    }
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commandsAdmin, _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                            }
                            else
                            {
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands1, _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                if (!_commands2.EndsWith("More Commands:"))
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands2, _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                if (!_commands3.EndsWith("More Commands:"))
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands3, _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                if (!_commands4.EndsWith("More Commands:"))
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands4, _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                if (!_commands5.EndsWith("More Commands:"))
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands5, _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                if (!_commands6.EndsWith("More Commands:"))
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands6, _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                if (CustomCommands.IsEnabled)
                                {
                                    if (!_commandsCustom.EndsWith("Custom commands are:"))
                                    {
                                        ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commandsCustom, _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                                if (Admin.PermissionLevel <= Admin_Level)
                                {
                                    if (_commandsAdmin.EndsWith("Admin commands are:"))
                                    {
                                        _commandsAdmin = string.Format("{0}Sorry, there are no admin chat commands.", LoadConfig.Chat_Response_Color);
                                    }
                                    ChatMessage(_cInfo, _commandsAdmin, _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                            Bloodmoon.GetBloodmoon(_cInfo, _announce);
                            return false;
                        }
                        if (Suicide.IsEnabled && (_message.ToLower() == Suicide.Command20 || _message.ToLower() == Suicide.Command21 || _message.ToLower() == Suicide.Command22 || _message.ToLower() == Suicide.Command23))
                        {
                            Suicide.CheckPlayer(_cInfo, _announce);
                            return false;
                        }
                        if (Gimme.IsEnabled && (_message.ToLower() == Gimme.Command24 || _message.ToLower() == Gimme.Command25))
                        {
                            if (Gimme.Always_Show_Response)
                            {
                                ChatMessage(_cInfo, _message, _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                Gimme.Checkplayer(_cInfo, true, _mainName);
                            }
                            else
                            {
                                Gimme.Checkplayer(_cInfo, _announce, _mainName);
                            }
                            return false;
                        }
                        if (Jail.IsEnabled && (_message.ToLower() == Jail.Command26 || _message.ToLower().StartsWith(Jail.Command27) || _message.ToLower().StartsWith(Jail.Command28)))
                        {
                            if (_message.ToLower() == Jail.Command26)
                            {
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message, _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                Jail.SetJail(_cInfo);
                            }
                            else if (_message.ToLower().StartsWith(Jail.Command27))
                            {
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message, _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                Jail.PutInJail(_cInfo, _message);
                            }
                            else if (_message.ToLower().StartsWith(Jail.Command28))
                            {
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message, _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                            Animals.Checkplayer(_cInfo, _announce, _mainName);
                            return false;
                        }
                        if (FirstClaimBlock.IsEnabled && _message.ToLower() == FirstClaimBlock.Command32)
                        {
                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", checking your claim block status.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            FirstClaimBlock.firstClaim(_cInfo);
                            return false;
                        }
                        if (ClanManager.IsEnabled && (_message.ToLower().StartsWith(ClanManager.Command33) || _message.ToLower() == ClanManager.Command34 || _message.ToLower().StartsWith(ClanManager.Command35) || _message.ToLower() == ClanManager.Command36 || _message.ToLower() == ClanManager.Command37 || _message.ToLower().StartsWith(ClanManager.Command38) || _message.ToLower().StartsWith(ClanManager.Command39) || _message.ToLower().StartsWith(ClanManager.Command40) || _message.ToLower().StartsWith(ClanManager.Command41) || _message.ToLower() == ClanManager.Command42 || _message.ToLower().StartsWith(ClanManager.Command43) || _message.ToLower().StartsWith(ClanManager.Command44) || _message.ToLower().StartsWith(ClanManager.Command124) || _message.ToLower() == ClanManager.Command125))
                        {
                            if (_message.ToLower().StartsWith(ClanManager.Command33))
                            {
                                if (_message.ToLower() == ClanManager.Command33)
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", usage: " + ChatHook.Command_Private + ClanManager.Command33 + " clanName[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", usage: " + ChatHook.Command_Private + ClanManager.Command35 + " playerName[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", usage: " + ChatHook.Command_Private + ClanManager.Command38 + " playerName[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", usage: " + ChatHook.Command_Private + ClanManager.Command39 + " playerName[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", usage: " + ChatHook.Command_Private + ClanManager.Command40 + " playerName[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _clanCommands, _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                return false;
                            }
                            if (_message.ToLower().StartsWith(ClanManager.Command43) || _message.ToLower().StartsWith(ClanManager.Command124) && ClanManager.ClanMember.Contains(_cInfo.playerId))
                            {
                                if (_message.ToLower() == ClanManager.Command43 || _message.ToLower() == ClanManager.Command124)
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", usage: " + ChatHook.Command_Private + ClanManager.Command43 + " message or " + ChatHook.Command_Private + ClanManager.Command124 + " message[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", usage: " + ChatHook.Command_Private + ClanManager.Command44 + " newName[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else
                                {
                                    _message = _message.Replace(ClanManager.Command44 + " ", "");
                                    ClanManager.ClanRename(_cInfo, _message);
                                }
                                return false;
                            }
                            if (_message.ToLower() == ClanManager.Command125)
                            {
                                string _clanlist = ClanManager.GetClanList();
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _clanlist, _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                return false;
                            }
                        }
                        if (ReservedSlots.Reserved_Check && _message.ToLower() == ReservedSlots.Command45)
                        {
                            if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                            {
                                DateTime _dt;
                                if (ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt))
                                {
                                    if (DateTime.Now < _dt)
                                    {
                                        string _chatMessage = ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", your reserved status expires on {DateTime}.[-]";
                                        _chatMessage = _chatMessage.Replace("{DateTime}", _dt.ToString());
                                        ChatMessage(_cInfo, _chatMessage, _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                    else
                                    {
                                        string _chatMessage = ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", your reserved status has expired on {DateTime}.[-]";
                                        _chatMessage = _chatMessage.Replace("{DateTime}", _dt.ToString());
                                        ChatMessage(_cInfo, _chatMessage, _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    }
                                }
                            }
                            else
                            {
                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", you have not donated. Expiration date unavailable.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            return false;
                        }
                        if (VoteReward.IsEnabled && _message.ToLower() == VoteReward.Command46)
                        {
                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", checking for your vote.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            VoteReward.Check(_cInfo);
                            return false;
                        }
                        if (AutoShutdown.IsEnabled && _message.ToLower() == AutoShutdown.Command47)
                        {
                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", checking for the next shutdown time.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            AutoShutdown.CheckNextShutdown(_cInfo, _announce);
                            return false;
                        }
                        if (AdminList.IsEnabled && _message.ToLower() == AdminList.Command48)
                        {
                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", listing online administrators and moderators.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            AdminList.List(_cInfo, _announce, _mainName);
                            return false;
                        }
                        if (Travel.IsEnabled && _message.ToLower() == Travel.Command49)
                        {
                            Travel.Check(_cInfo, _announce, _mainName);
                            return false;
                        }
                        if (Zones.IsEnabled && _message.ToLower() == Zones.Command50)
                        {
                            if (Zones.Victim.ContainsKey(_cInfo.entityId))
                            {
                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", sending you to your death point.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                Zones.ReturnToPosition(_cInfo);
                                return false;
                            }
                        }
                        if (MarketChat.IsEnabled && (_message.ToLower() == MarketChat.Command51 || _message.ToLower() == MarketChat.Command52))
                        {
                            if (MarketChat.MarketPlayers.Contains(_cInfo.entityId))
                            {
                                MarketChat.SendBack(_cInfo, _mainName);
                                return false;
                            }
                        }
                        if (LobbyChat.IsEnabled && (_message.ToLower() == LobbyChat.Command53 || _message.ToLower() == LobbyChat.Command54))
                        {
                            if (LobbyChat.LobbyPlayers.Contains(_cInfo.entityId))
                            {
                                LobbyChat.SendBack(_cInfo, _mainName);
                                return false;
                            }
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
                            Wallet.WalletValue(_cInfo, _mainName);
                            return false;
                        }
                        if (Shop.IsEnabled && _message.ToLower().StartsWith(Shop.Command57))
                        {
                            if (_message.ToLower() == (Shop.Command57))
                            {
                                Shop.PosCheck(_cInfo, _mainName, _message, 1);
                            }
                            else
                            {
                                _message = _message.ToLower().Replace(Shop.Command57 + " ", "");
                                Shop.PosCheck(_cInfo, _mainName, _message, 2);
                            }
                            return false;
                        }
                        if (Shop.IsEnabled && _message.ToLower().StartsWith(Shop.Command58))
                        {
                            if (_message.ToLower() == (Shop.Command58))
                            {
                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", usage: " + ChatHook.Command_Private + Shop.Command58 + " # or " + ChatHook.Command_Private + Shop.Command58 + " # #[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                _message = _message.ToLower().Replace(Shop.Command58 + " ", "");
                                Shop.PosCheck(_cInfo, _mainName, _message, 3);
                            }
                            return false;
                        }
                        if (FriendTeleport.IsEnabled && _message.ToLower().StartsWith(FriendTeleport.Command59))
                        {
                            if (_message.ToLower() == FriendTeleport.Command59)
                            {
                                FriendTeleport.ListFriends(_cInfo, _message);
                            }
                            else
                            {
                                _message = _message.ToLower().Replace(FriendTeleport.Command59 + " ", "");
                                FriendTeleport.CheckDelay(_cInfo, _message, _announce);
                            }
                            return false;
                        }
                        if (FriendTeleport.IsEnabled && _message.ToLower() == (FriendTeleport.Command60))
                        {
                            if (FriendTeleport.Dict.ContainsKey(_cInfo.entityId))
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
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", your friend's teleport request was accepted.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else
                                {
                                    FriendTeleport.Dict.Remove(_cInfo.entityId);
                                    FriendTeleport.Dict1.Remove(_cInfo.entityId);
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", your friend's teleport request has expired.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                        }
                        if (DeathSpot.IsEnabled && _message.ToLower() == (DeathSpot.Command61))
                        {
                            DeathSpot.DeathDelay(_cInfo, _announce, _mainName);
                            return false;
                        }
                        if (WeatherVote.IsEnabled && _message.ToLower() == WeatherVote.Command62)
                        {
                            if (!WeatherVote.VoteOpen)
                            {
                                WeatherVote.CallForVote1(_cInfo);
                            }
                            else
                            {
                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", a weather vote has already begun.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            return false;
                        }
                        if (WeatherVote.IsEnabled && _message.ToLower() == WeatherVote.Command63)
                        {
                            if (WeatherVote.VoteOpen)
                            {
                                if (!WeatherVote.snow.Contains(_cInfo.entityId) && !WeatherVote.sun.Contains(_cInfo.entityId) && !WeatherVote.rain.Contains(_cInfo.entityId))
                                {
                                    WeatherVote.sun.Add(_cInfo.entityId);
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Vote cast for clear.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                                else
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", you have already voted.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                ChatMessage(_cInfo, _cInfo.playerName + ", there is no active weather vote. Type " + ChatHook.Command_Private + WeatherVote.Command62 + " in chat to open a new vote.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            return false;
                        }
                        if (WeatherVote.IsEnabled && _message.ToLower() == WeatherVote.Command64)
                        {
                            if (WeatherVote.VoteOpen)
                            {
                                if (!WeatherVote.snow.Contains(_cInfo.entityId) && !WeatherVote.sun.Contains(_cInfo.entityId) && !WeatherVote.rain.Contains(_cInfo.entityId))
                                {
                                    WeatherVote.rain.Add(_cInfo.entityId);
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Vote cast for rain.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                                else
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", you have already voted.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", there is no active weather vote. Type " + ChatHook.Command_Private + WeatherVote.Command62 + " in chat to open a new vote.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            return false;
                        }
                        if (WeatherVote.IsEnabled && _message.ToLower() == WeatherVote.Command65)
                        {
                            if (WeatherVote.VoteOpen)
                            {
                                if (!WeatherVote.snow.Contains(_cInfo.entityId) && !WeatherVote.sun.Contains(_cInfo.entityId) && !WeatherVote.rain.Contains(_cInfo.entityId))
                                {
                                    WeatherVote.snow.Add(_cInfo.entityId);
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Vote cast for snow.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                                else
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", you have already voted.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", there is no active weather vote. Type " + ChatHook.Command_Private + WeatherVote.Command62 + " in chat to open a new vote.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            return false;
                        }
                        if (RestartVote.IsEnabled && _message.ToLower() == RestartVote.Command66)
                        {
                            if (!KickVote.VoteOpen && !RestartVote.VoteOpen && !MuteVote.VoteOpen && !NightVote.VoteOpen)
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
                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase824 + "[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            return false;
                        }
                        if (MuteVote.IsEnabled && _message.ToLower().StartsWith(MuteVote.Command67))
                        {
                            if (!KickVote.VoteOpen && !RestartVote.VoteOpen && !MuteVote.VoteOpen && !NightVote.VoteOpen)
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
                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase824 + "[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            return false;
                        }
                        if (KickVote.IsEnabled && _message.ToLower().StartsWith(KickVote.Command68))
                        {
                            if (!KickVote.VoteOpen && !RestartVote.VoteOpen && !MuteVote.VoteOpen && !NightVote.VoteOpen)
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
                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase824 + "[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            return false;
                        }
                        if (NightVote.IsEnabled && _message.ToLower() == NightVote.Command69)
                        {
                            if (!KickVote.VoteOpen && !RestartVote.VoteOpen && !MuteVote.VoteOpen && !NightVote.VoteOpen)
                            {
                                NightVote.Vote(_cInfo);
                            }
                            else
                            {
                                string _phrase824;
                                if (!Phrases.Dict.TryGetValue(824, out _phrase824))
                                {
                                    _phrase824 = " there is a vote already open.";
                                }
                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + _phrase824 + "[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase825 + "[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                                else
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you have already voted.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase825 + "[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                                else
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you have already voted.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                            if (MuteVote.IsEnabled && MuteVote.VoteOpen)
                            {
                                if (!MuteVote.Mute.Contains(_cInfo.entityId))
                                {
                                    MuteVote.Mute.Add(_cInfo.entityId);
                                    string _phrase825;
                                    if (!Phrases.Dict.TryGetValue(825, out _phrase825))
                                    {
                                        _phrase825 = "There are now {VoteCount} of {VotesNeeded} votes.";
                                    }
                                    _phrase825 = _phrase825.Replace("{VoteCount}", MuteVote.Mute.Count.ToString());
                                    _phrase825 = _phrase825.Replace("{VotesNeeded}", MuteVote.Votes_Needed.ToString());
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase825 + "[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                                else
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you have already voted.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                            if (NightVote.IsEnabled && NightVote.VoteOpen)
                            {
                                if (!NightVote.Night.Contains(_cInfo.entityId))
                                {
                                    NightVote.Night.Add(_cInfo.entityId);
                                    string _phrase825;
                                    if (!Phrases.Dict.TryGetValue(825, out _phrase825))
                                    {
                                        _phrase825 = "There are now {VoteCount} of {VotesNeeded} votes.";
                                    }
                                    _phrase825 = _phrase825.Replace("{VoteCount}", NightVote.Night.Count.ToString());
                                    _phrase825 = _phrase825.Replace("{VotesNeeded}", NightVote.Votes_Needed.ToString());
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase825 + "[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                                else
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you have already voted.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                        }
                        if (AuctionBox.IsEnabled && _message.ToLower() == AuctionBox.Command71)
                        {
                            AuctionBox.AuctionList(_cInfo);
                            return false;
                        }
                        if (AuctionBox.IsEnabled && _message.ToLower() == AuctionBox.Command72)
                        {
                            AuctionBox.CancelAuction(_cInfo);
                            return false;
                        }
                        if (AuctionBox.IsEnabled && _message.ToLower().StartsWith(AuctionBox.Command73 + " "))
                        {
                            if (AuctionBox.No_Admins)
                            {
                                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                                if (Admin.PermissionLevel > Admin_Level)
                                {
                                    _message = _message.ToLower().Replace(AuctionBox.Command73 + " ", "");
                                    {
                                        int _purchase;
                                        if (int.TryParse(_message, out _purchase))
                                        {
                                            string _sql = string.Format("SELECT * FROM Auction WHERE auctionid = {0}", _purchase);
                                            DataTable _result = SQL.TQuery(_sql);
                                            if (_result.Rows.Count > 0)
                                            {
                                                AuctionBox.WalletCheck(_cInfo, _purchase);
                                            }
                                            else
                                            {
                                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you have used an auction item # that does not exist or has sold. Type " + ChatHook.Command_Private + AuctionBox.Command71 + ".[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                            }
                                            _result.Dispose();
                                        }
                                    }
                                }
                                else
                                {
                                    string _chatMessage = ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " the auction is disabled for your tier.[-]";
                                    ChatMessage(_cInfo, _chatMessage, _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                _message = _message.ToLower().Replace(AuctionBox.Command73 + " ", "");
                                {
                                    int _purchase;
                                    if (int.TryParse(_message, out _purchase))
                                    {
                                        string _sql = string.Format("SELECT steamid FROM Auction WHERE auctionid = {0}", _purchase);
                                        DataTable _result = SQL.TQuery(_sql);
                                        if (_result.Rows.Count > 0)
                                        {
                                            AuctionBox.WalletCheck(_cInfo, _purchase);
                                        }
                                        else
                                        {
                                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you have used an auction item # that does not exist or has sold. Type " + ChatHook.Command_Private + AuctionBox.Command71 + ".[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                        _result.Dispose();
                                    }
                                }
                            }
                            return false;
                        }
                        if (AuctionBox.IsEnabled && _message.ToLower().StartsWith(AuctionBox.Command74))
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
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " the auction is disabled for your tier.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                            if (_announce)
                            {
                                Fps.FPS(_cInfo, _announce);
                            }
                            else
                            {
                                Fps.FPS(_cInfo, _announce);
                                return false;
                            }
                        }
                        if (Loc.IsEnabled && _message.ToLower() == Loc.Command76)
                        {
                            if (_announce)
                            {
                                Loc.Exec(_cInfo);
                            }
                            else
                            {
                                Loc.Exec(_cInfo);
                                return false;
                            }
                        }
                        if (VehicleTeleport.IsEnabled)
                        {
                            if (VehicleTeleport.Bike && _message.ToLower() == VehicleTeleport.Command77)
                            {
                                VehicleTeleport.VehicleDelay(_cInfo, _mainName, 1);
                                return false;
                            }
                            if (VehicleTeleport.Mini_Bike && _message.ToLower() == VehicleTeleport.Command78)
                            {
                                VehicleTeleport.VehicleDelay(_cInfo, _mainName, 2);
                                return false;
                            }
                            if (VehicleTeleport.Motor_Bike && _message.ToLower() == VehicleTeleport.Command79)
                            {
                                VehicleTeleport.VehicleDelay(_cInfo, _mainName, 3);
                                return false;
                            }
                            if (VehicleTeleport.Jeep && _message.ToLower() == VehicleTeleport.Command80)
                            {
                                VehicleTeleport.VehicleDelay(_cInfo, _mainName, 4);
                                return false;
                            }
                            if (VehicleTeleport.Gyro && _message.ToLower() == VehicleTeleport.Command81)
                            {
                                VehicleTeleport.VehicleDelay(_cInfo, _mainName, 5);
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
                            Bounties.BountyList(_cInfo, _mainName);
                            return false;
                        }
                        if (Bounties.IsEnabled && _message.ToLower().StartsWith(Bounties.Command83 + " "))
                        {
                            _message = _message.ToLower().Replace(Bounties.Command83 + " ", "");
                            Bounties.NewBounty(_cInfo, _message, _mainName);
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
                        if (LobbyChat.IsEnabled && _message.ToLower() == SetLobby.Command87)
                        {
                            SetLobby.Set(_cInfo);
                            return false;
                        }
                        if (LobbyChat.IsEnabled && _message.ToLower() == LobbyChat.Command88)
                        {
                            LobbyChat.Delay(_cInfo, _mainName, _announce);
                            return false;
                        }
                        if (PlayerList.IsEnabled && _message.ToLower() == PlayerList.Command89)
                        {
                            PlayerList.Exec(_cInfo, _mainName);
                            return false;
                        }
                        if (Stuck.IsEnabled && _message.ToLower() == Stuck.Command90)
                        {
                            Stuck.Delay(_cInfo, _mainName, _announce);
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
                            DataTable _result = SQL.TQuery(_sql);
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
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase926 + "[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase813 + "[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                return false;
                            }
                        }
                        if (Bank.IsEnabled && _message.ToLower() == Bank.Command94)
                        {
                            Bank.Check(_cInfo);
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
                        if (Bank.IsEnabled && _message.ToLower().StartsWith(Bank.Command97 + " "))
                        {
                            _message = _message.ToLower().Replace(Bank.Command97 + " ", "");
                            Bank.CheckLocation(_cInfo, _message, 3);
                            return false;
                        }
                        if (Bank.IsEnabled && _message.ToLower().StartsWith(Bank.Command98 + " "))
                        {
                            _message = _message.ToLower().Replace(Bank.Command98 + " ", "");
                            Bank.CheckLocation(_cInfo, _message, 4);
                            return false;
                        }
                        if (Bank.IsEnabled && _message.ToLower().StartsWith(Bank.Command99 + " "))
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
                        if (Mogul.IsEnabled && _message.ToLower() == Mogul.Command101)
                        {
                            if (Wallet.IsEnabled || Bank.IsEnabled)
                            {
                                Mogul.TopThree(_cInfo, _announce);
                                return false;
                            }
                        }
                        if (MarketChat.IsEnabled && _message.ToLower() == MarketChat.Command102)
                        {
                            SetMarket.Set(_cInfo);
                            return false;
                        }
                        if (MarketChat.IsEnabled && _message.ToLower() == MarketChat.Command103)
                        {
                            MarketChat.Delay(_cInfo, _mainName, _announce);
                            return false;
                        }
                        if (InfoTicker.IsEnabled && _message.ToLower() == InfoTicker.Command104)
                        {
                            if (!InfoTicker.exemptionList.Contains(_cInfo.playerId))
                            {
                                InfoTicker.exemptionList.Add(_cInfo.playerId);
                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you have turned off infoticker messages until the server restarts.", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                InfoTicker.exemptionList.Remove(_cInfo.playerId);
                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you have turned on infoticker messages.", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                            Waypoint.Delay(_cInfo, _message);
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
                        if (Whisper.IsEnabled && (_message.ToLower() == Whisper.Command120 || _message.ToLower() == Whisper.Command121))
                        {
                            Whisper.Send(_cInfo, _message);
                            return false;
                        }
                        if (Whisper.IsEnabled && (_message.ToLower() == Whisper.Command122 || _message.ToLower() == Whisper.Command123))
                        {
                            Whisper.Reply(_cInfo, _message);
                            return false;
                        }
                        _message = _message.ToLower();
                        if (CustomCommands.IsEnabled && CustomCommands.Dict.ContainsKey(_message))
                        {
                            CustomCommands.CheckCustomDelay(_cInfo, _message, _mainName, _announce);
                            return false;
                        }
                    }
                    if (AdminChat.IsEnabled && (_message.ToLower().StartsWith("@" + AdminChat.Command118 + " ") || _message.ToLower().StartsWith("@" + AdminChat.Command119 + " ")))
                    {
                        if (_message.StartsWith("@" + AdminChat.Command118 + " "))
                        {
                            AdminChat.SendAdmins(_cInfo, _message);
                            return false;
                        }
                        if (_message.StartsWith("@" + AdminChat.Command119 + " "))
                        {
                            AdminChat.SendAll(_cInfo, _message);
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public static void ChatMessage(ClientInfo _cInfo, string _message, int _senderId, string _name, EChatType _type, List<int> _recipientEntityIds)
        {
            if (_type == EChatType.Whisper)
            {
                _cInfo.SendPackage(new NetPackageChat(EChatType.Whisper, 33, _message, _name, false, null));
            }
            else if (_type == EChatType.Global)
            {
                GameManager.Instance.ChatMessageServer(_cInfo, EChatType.Global, 33, _message, _name, false, _recipientEntityIds);
            }
            else if (_type == EChatType.Friends)
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                World world = GameManager.Instance.World;
                List<EntityPlayer> _playerList = world.Players.list;
                for (int i = 0; i < _playerList.Count; i++)
                {
                    EntityPlayer _player2 = _playerList[i];
                    if (_player != null)
                    {
                        if (_player.IsFriendsWith(_player2))
                        {
                            ClientInfo _cInfo2 = ConnectionManager.Instance.Clients.ForEntityId(_player2.entityId);
                            if (Friend_Chat_Color.StartsWith("[") && Friend_Chat_Color.EndsWith("]"))
                            {
                                _message = _message.Insert(0, Friend_Chat_Color);
                            }
                            _cInfo2.SendPackage(new NetPackageChat(EChatType.Whisper, 33, _message, _name, false, null));
                        }
                    }
                }
                if (Friend_Chat_Color.StartsWith("[") && Friend_Chat_Color.EndsWith("]"))
                {
                    _message = _message.Insert(0, Friend_Chat_Color);
                }
                _cInfo.SendPackage(new NetPackageChat(EChatType.Whisper, 33, _message, _name, false, null));
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
                            _cInfo2.SendPackage(new NetPackageChat(EChatType.Whisper, 33, _message, _name, false, null));
                        }
                    }
                }
            }
        }
    }
}