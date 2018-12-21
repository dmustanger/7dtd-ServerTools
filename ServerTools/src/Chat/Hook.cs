﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ServerTools
{
    public class ChatHook
    {
        public static bool ChatFlood = false, Admin_Name_Coloring = false, Donator_Name_Coloring = false;
        public static bool Special_Player_Name_Coloring = false, Normal_Player_Name_Coloring = false;
        public static bool Reserved_Check = false;
        public static string Admin_Color = "[FF0000]", Mod_Color = "[008000]";
        public static string Don_Color1 = "[009000]", Don_Color2 = "[FF66CC]", Don_Color3 = "[E9C918]";
        public static string Special_Player_Color = "[ADAD85]", Normal_Player_Color = "[00B3B3]";
        public static string Admin_Prefix = "(ADMIN)", Mod_Prefix = "(MOD)";
        public static string Don_Prefix1 = "(DON)", Don_Prefix2 = "(DON)", Don_Prefix3 = "(DON)";
        public static string Special_Player_Prefix = "(SPECIAL)";
        public static string Normal_Player_Prefix = "(NOOB)";
        public static string Friend_Chat_Color = "[33CC33]", Party_Chat_Color = "[FFCC00]", Player_Name_Color = "[00FF00]";
        public static int Admin_Level = 0, Mod_Level = 1;
        public static int Don_Level1 = 100, Don_Level2 = 101, Don_Level3 = 102;
        public static string Special_Players_List = "76561191234567891,76561191987654321";
        public static int Max_Length = 250, Messages_Per_Min = 5;
        public static bool ChatCommandPrivateEnabled = false, ChatCommandPublicEnabled = false;
        public static string Command_Private = "/", Command_Public = "!";
        private static string filepath = string.Format("{0}/ServerTools.bin", GameUtils.GetSaveGameDir());
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
                            _sql = string.Format("UPDATE Players SET messageCount = '{0}' WHERE steamid = '{1}'", _count + 1, _cInfo.playerId);
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
                    if (ClanManager.IsEnabled && !_message.StartsWith("@") && _mainName != LoadConfig.Server_Response_Name && _senderId != 33 && !_message.StartsWith(Command_Private) && !_message.StartsWith(Command_Public) && ClanManager.ClanMember.Contains(_cInfo.playerId))
                    {
                        string _sql = string.Format("SELECT clanname FROM Players WHERE steamid = '{0}'", _cInfo.playerId);
                        DataTable _result = SQL.TQuery(_sql);
                        string _clanname = _result.Rows[0].ItemArray.GetValue(0).ToString();
                        _result.Dispose();
                        _mainName = string.Format("({0}) {1}", _clanname, _mainName);
                        ChatMessage(_cInfo, _message, _senderId, _mainName, EChatType.Whisper, null);
                        return false;
                    }
                    if (Badwords.Invalid_Name)
                    {
                        bool _hasBadName = false;
                        string _playerName1 = _mainName.ToLower();
                        foreach (string _word in Badwords.List)
                        {
                            if (_playerName1.Contains(_word))
                            {
                                _hasBadName = true;
                            }
                        }
                        if (_hasBadName)
                        {
                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", invalid Name-No Commands[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            return false;
                        }
                    }
                    if (Badwords.IsEnabled)
                    {
                        bool _hasBadWord = false;
                        string _message1 = _message.ToLower();
                        foreach (string _word in Badwords.List)
                        {
                            if (_message1.Contains(_word))
                            {
                                string _replace = "";
                                for (int i = 0; i < _word.Length; i++)
                                {
                                    _replace = string.Format("{0}*", _replace);
                                }
                                _message1 = _message1.Replace(_word, _replace);
                                _hasBadWord = true;
                            }
                        }
                        if (_hasBadWord)
                        {
                            ChatMessage(_cInfo, _message1, _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                            return false;
                        }
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
                        if (_message.StartsWith("w ") || _message.StartsWith("W ") || _message.StartsWith("pm ") || _message.StartsWith("PM "))
                        {
                            if (CustomCommands.IsEnabled)
                            {
                                Whisper.Send(_cInfo, _message);
                                return false;
                            }
                        }
                        if (_message.StartsWith("r ") || _message.StartsWith("R ") || _message.StartsWith("RE ") || _message.StartsWith("re "))
                        {
                            if (CustomCommands.IsEnabled)
                            {
                                Whisper.Reply(_cInfo, _message);
                                return false;
                            }
                        }
                        if (TeleportHome.IsEnabled && _message.ToLower() == "sethome")
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
                        if (TeleportHome.IsEnabled && _message.ToLower() == "home")
                        {

                            TeleportHome.Check(_cInfo, _mainName, _announce);
                            return false;
                        }
                        if (TeleportHome.IsEnabled && _message.ToLower() == "fhome")
                        {

                            TeleportHome.FCheck(_cInfo, _mainName, _announce);
                            return false;
                        }
                        if (TeleportHome.IsEnabled && _message.ToLower() == "delhome")
                        {

                            TeleportHome.DelHome(_cInfo, _mainName, _announce);
                            return false;
                        }
                        if (TeleportHome.Set_Home2_Enabled && _message.ToLower() == "sethome2")
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
                            else if (TeleportHome.Set_Home2_Enabled && !TeleportHome.Set_Home2_Donor_Only)
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
                        if (TeleportHome.IsEnabled && _message.ToLower() == "home2")
                        {
                            if (TeleportHome.Set_Home2_Enabled && TeleportHome.Set_Home2_Donor_Only && ReservedSlots.IsEnabled)
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
                            else if (TeleportHome.Set_Home2_Enabled && !TeleportHome.Set_Home2_Donor_Only)
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
                        if (TeleportHome.IsEnabled && _message.ToLower() == "fhome2")
                        {
                            if (TeleportHome.Set_Home2_Enabled && TeleportHome.Set_Home2_Donor_Only && ReservedSlots.IsEnabled)
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
                            else if (TeleportHome.Set_Home2_Enabled && !TeleportHome.Set_Home2_Donor_Only)
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
                        if (TeleportHome.IsEnabled && _message.ToLower() == "delhome2")
                        {
                            if (TeleportHome.Set_Home2_Enabled && TeleportHome.Set_Home2_Donor_Only && ReservedSlots.IsEnabled)
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
                            else if (TeleportHome.Set_Home2_Enabled && !TeleportHome.Set_Home2_Donor_Only)
                            {
                                TeleportHome.DelHome2(_cInfo, _mainName, _announce);
                            }
                            return false;
                        }
                        if (TeleportHome.IsEnabled && _message.ToLower() == "go")
                        {
                            if (TeleportHome.Invite.ContainsKey(_cInfo.entityId))
                            {
                                TeleportHome.FriendHome(_cInfo);
                                return false;
                            }
                        }
                        if (Waypoint.IsEnabled && _message.ToLower() == "go")
                        {
                            if (Waypoint.Invite.ContainsKey(_cInfo.entityId))
                            {
                                Waypoint.FriendWaypoint(_cInfo);
                                return false;
                            }
                        }
                        if (Hardcore.IsEnabled && _message.ToLower() == "top3")
                        {
                            Hardcore.TopThree(_cInfo, _announce);
                            return false;
                        }
                        if (Hardcore.IsEnabled && _message.ToLower() == "score")
                        {
                            Hardcore.Score(_cInfo, _announce);
                            return false;
                        }
                        if (AdminChat.IsEnabled)
                        {
                            if (_message.ToLower().StartsWith("mute ") || _message.ToLower().StartsWith("unmute "))
                            {
                                if (_message.ToLower().StartsWith("mute "))
                                {
                                    MutePlayer.Add(_cInfo, _message);
                                }
                                if (_message.ToLower().StartsWith("unmute "))
                                {
                                    MutePlayer.Remove(_cInfo, _message);
                                }
                                return false;
                            }
                        }
                        if (_message.ToLower() == "commands")
                        {
                            string _commands1 = CustomCommands.GetChatCommands1(_cInfo);
                            string _commands2 = CustomCommands.GetChatCommands2(_cInfo);
                            string _commands3 = CustomCommands.GetChatCommands3(_cInfo);
                            string _commands4 = CustomCommands.GetChatCommands4(_cInfo);
                            string _commands5 = CustomCommands.GetChatCommands5(_cInfo);
                            string _commandsCustom = CustomCommands.GetChatCommandsCustom(_cInfo);
                            string _commandsAdmin = CustomCommands.GetChatCommandsAdmin(_cInfo);
                            if (_announce)
                            {
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands1, _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands2, _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands3, _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands4, _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                if (CustomCommands.IsEnabled)
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commandsCustom, _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                                if (Admin.PermissionLevel <= Admin_Level)
                                {
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commandsAdmin, _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands1, _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands2, _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands3, _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _commands4, _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                if (CustomCommands.IsEnabled)
                                {
                                    string _chatMessage = LoadConfig.Chat_Response_Color + _commandsCustom;
                                    ChatMessage(_cInfo, _chatMessage, _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                                if (Admin.PermissionLevel <= Admin_Level)
                                {
                                    string _chatMessage = LoadConfig.Chat_Response_Color + _commandsAdmin;
                                    ChatMessage(_cInfo, _chatMessage, _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            return false;
                        }
                        if (Day7.IsEnabled && (_message.ToLower() == "day7" || _message.ToLower() == "day"))
                        {
                            Day7.GetInfo(_cInfo, _announce);
                            return false;
                        }
                        if (Bloodmoon.IsEnabled && (_message.ToLower() == "bloodmoon" || _message.ToLower() == "bm"))
                        {
                            Bloodmoon.GetBloodmoon(_cInfo, _announce);
                            return false;
                        }
                        if (Suicide.IsEnabled && (_message.ToLower() == "killme" || _message.ToLower() == "wrist" || _message.ToLower() == "hang" || _message.ToLower() == "suicide"))
                        {
                            if (_announce)
                            {
                                Suicide.CheckPlayer(_cInfo, _announce);
                            }
                            else
                            {
                                Suicide.CheckPlayer(_cInfo, _announce);
                            }
                            return false;
                        }
                        if (Gimme.IsEnabled && (_message.ToLower() == "gimme" || _message.ToLower() == "gimmie"))
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
                        if (Jail.IsEnabled && (_message.ToLower() == "setjail" || _message.ToLower().StartsWith("jail ") || _message.ToLower().StartsWith("unjail ")))
                        {
                            if (_message.ToLower() == "setjail")
                            {
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message, _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                Jail.SetJail(_cInfo);
                            }
                            else if (_message.ToLower().StartsWith("jail "))
                            {
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message, _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                Jail.PutInJail(_cInfo, _message);
                            }
                            else if (_message.ToLower().StartsWith("unjail "))
                            {
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message, _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                Jail.RemoveFromJail(_cInfo, _message);
                            }
                            return false;
                        }
                        if (NewSpawnTele.IsEnabled && _message.ToLower() == "setspawn")
                        {
                            if (_announce)
                            {
                                NewSpawnTele.SetNewSpawnTele(_cInfo);
                            }
                            else
                            {
                                NewSpawnTele.SetNewSpawnTele(_cInfo);
                                return false;
                            }
                        }
                        if (Animals.IsEnabled && (_message.ToLower() == "trackanimal" || _message.ToLower() == "track"))
                        {
                            Animals.Checkplayer(_cInfo, _announce, _mainName);
                            return false;
                        }
                        if (FirstClaimBlock.IsEnabled && _message.ToLower() == "claim")
                        {
                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", checking your claim block status.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            FirstClaimBlock.firstClaim(_cInfo);
                            return false;
                        }
                        if (ClanManager.IsEnabled && (_message.ToLower().StartsWith("clanadd") || _message.ToLower() == "clandel" || _message.ToLower().StartsWith("claninvite") || _message.ToLower() == "clanaccept" || _message.ToLower() == "clandecline" || _message.ToLower().StartsWith("clanremove") || _message.ToLower().StartsWith("clanpromote") || _message.ToLower().StartsWith("clandemote") || _message.ToLower().StartsWith("clan") || _message.ToLower() == "clancommands" || _message.ToLower().StartsWith("clanrename")))
                        {
                            if (_message.ToLower() == "clancommands")
                            {
                                string _clanCommands = ClanManager.GetChatCommands(_cInfo);
                                ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _clanCommands, _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                return false;
                            }
                            if (_message.ToLower().StartsWith("clanadd"))
                            {
                                if (_message.ToLower() == "clanadd")
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", usage: /clanadd clanName[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else
                                {
                                    _message = _message.Replace("clanadd ", "");
                                    ClanManager.AddClan(_cInfo, _message);
                                }
                            }
                            if (_message.ToLower() == "clandel")
                            {
                                ClanManager.RemoveClan(_cInfo);
                            }
                            if (_message.ToLower().StartsWith("claninvite"))
                            {
                                if (_message.ToLower() == "claninvite")
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", usage: /claninvite playerName[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else
                                {
                                    _message = _message.Replace("claninvite ", "");
                                    ClanManager.InviteMember(_cInfo, _message);
                                }
                            }
                            if (_message.ToLower() == "clanaccept")
                            {
                                ClanManager.InviteAccept(_cInfo);
                                return false;
                            }
                            if (_message.ToLower() == "clandecline")
                            {
                                ClanManager.InviteDecline(_cInfo);
                            }
                            if (_message.ToLower().StartsWith("clanremove"))
                            {
                                if (_message.ToLower() == "clanremove")
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", usage: /clanremove playerName[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else
                                {
                                    _message = _message.Replace("clanremove ", "");
                                    ClanManager.RemoveMember(_cInfo, _message);
                                }
                            }
                            if (_message.ToLower().StartsWith("clanpromote"))
                            {
                                if (_message.ToLower() == "clanpromote")
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", usage: /clanpromote playerName[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else
                                {
                                    _message = _message.Replace("clanpromote ", "");
                                    ClanManager.PromoteMember(_cInfo, _message);
                                }
                            }
                            if (_message.ToLower().StartsWith("clandemote"))
                            {
                                if (_message.ToLower() == "clandemote")
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", usage: /clandemote playerName[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else
                                {
                                    _message = _message.Replace("clandemote ", "");
                                    ClanManager.DemoteMember(_cInfo, _message);
                                }
                            }
                            if (_message.ToLower() == "clanleave")
                            {
                                ClanManager.LeaveClan(_cInfo);
                            }
                            if (_message.ToLower().StartsWith("clan "))
                            {
                                if (_message.ToLower() == "clan")
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", usage: /clan message[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else
                                {
                                    _message = _message.ToLower().Replace("clan ", "");
                                    ClanManager.Clan(_cInfo, _message);
                                }
                            }
                            if (_message.ToLower().StartsWith("clanrename"))
                            {
                                if (_message.ToLower() == "clanrename")
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", usage: /clanrename newName[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                                else
                                {
                                    _message = _message.Replace("clanrename ", "");
                                    ClanManager.ClanRename(_cInfo, _message);
                                }
                            }
                            return false;
                        }
                        if (Reserved_Check && _message.ToLower() == "reserved")
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
                        if (VoteReward.IsEnabled && _message.ToLower() == "reward")
                        {
                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", checking for your vote.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            VoteReward.Check(_cInfo);
                            return false;
                        }
                        if (AutoShutdown.IsEnabled && _message.ToLower() == "shutdown")
                        {
                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", checking for the next shutdown time.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            AutoShutdown.CheckNextShutdown(_cInfo, _announce);
                            return false;
                        }
                        if (AdminList.IsEnabled && _message.ToLower() == "admin")
                        {
                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", listing online administrators and moderators.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            AdminList.List(_cInfo, _announce, _mainName);
                            return false;
                        }
                        if (Travel.IsEnabled && _message.ToLower() == "travel")
                        {
                            Travel.Check(_cInfo, _announce, _mainName);
                            return false;
                        }
                        if (Zones.IsEnabled && _message.ToLower() == "return")
                        {
                            if (Zones.Victim.ContainsKey(_cInfo.entityId))
                            {
                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", sending you to your death point.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                Zones.ReturnToPosition(_cInfo);
                                return false;
                            }
                        }
                        if (MarketChat.IsEnabled && (_message.ToLower() == "marketback" || _message.ToLower() == "mback"))
                        {
                            if (MarketChat.MarketPlayers.Contains(_cInfo.entityId))
                            {
                                MarketChat.SendBack(_cInfo, _mainName);
                                return false;
                            }
                        }
                        if (LobbyChat.IsEnabled && (_message.ToLower() == "lobbyback" || _message.ToLower() == "lback"))
                        {
                            if (LobbyChat.LobbyPlayers.Contains(_cInfo.entityId))
                            {
                                LobbyChat.SendBack(_cInfo, _mainName);
                                return false;
                            }
                        }
                        if (Zones.IsEnabled && Jail.IsEnabled && _message.ToLower() == "forgive")
                        {
                            if (Zones.Forgive.ContainsKey(_cInfo.entityId))
                            {
                                Jail.Forgive(_cInfo);
                                return false;
                            }
                        }
                        if (Wallet.IsEnabled && _message.ToLower() == "wallet")
                        {
                            Wallet.WalletValue(_cInfo, _mainName);
                            return false;
                        }
                        if (Shop.IsEnabled && _message.ToLower() == "shop")
                        {
                            Shop.PosCheck(_cInfo, _mainName, _message, 1);
                            return false;
                        }
                        if (Shop.IsEnabled && _message.ToLower().StartsWith("shop "))
                        {
                            _message = _message.ToLower().Replace("shop ", "");
                            Shop.PosCheck(_cInfo, _mainName, _message, 2);
                            return false;
                        }
                        if (Shop.IsEnabled && _message.ToLower() == "buy")
                        {
                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", usage: /buy # or /buy # #[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            return false;
                        }
                        if (Shop.IsEnabled && _message.ToLower().StartsWith("buy "))
                        {
                            _message = _message.ToLower().Replace("buy ", "");
                            Shop.PosCheck(_cInfo, _mainName, _message, 3);
                            return false;
                        }
                        if (FriendTeleport.IsEnabled && _message.ToLower().StartsWith("friend"))
                        {
                            if (_message.ToLower() == "friend")
                            {
                                FriendTeleport.ListFriends(_cInfo, _message);
                            }
                            else
                            {
                                _message = _message.ToLower().Replace("friend ", "");
                                FriendTeleport.CheckDelay(_cInfo, _message, _announce);
                            }
                            return false;
                        }
                        if (FriendTeleport.IsEnabled && _message.ToLower() == ("accept"))
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
                        if (DeathSpot.IsEnabled && _message.ToLower() == ("died"))
                        {
                            DeathSpot.DeathDelay(_cInfo, _announce, _mainName);
                            return false;
                        }
                        if (WeatherVote.IsEnabled && _message.ToLower() == "weathervote")
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
                        if (WeatherVote.IsEnabled && _message.ToLower() == "clear")
                        {
                            if (WeatherVote.VoteOpen)
                            {
                                if (!WeatherVote.snow.Contains(_cInfo.entityId) && !WeatherVote.clear.Contains(_cInfo.entityId) && !WeatherVote.rain.Contains(_cInfo.entityId))
                                {
                                    WeatherVote.clear.Add(_cInfo.entityId);
                                    ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + "Vote cast for clear.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Global, null);
                                }
                                else
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", you have already voted.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                ChatMessage(_cInfo, _cInfo.playerName + ", there is no active weather vote.Type / weather in chat to open a new vote.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            return false;
                        }
                        if (WeatherVote.IsEnabled && _message.ToLower() == "rain")
                        {
                            if (WeatherVote.VoteOpen)
                            {
                                if (!WeatherVote.snow.Contains(_cInfo.entityId) && !WeatherVote.clear.Contains(_cInfo.entityId) && !WeatherVote.rain.Contains(_cInfo.entityId))
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
                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", there is no active weather vote.Type / weather in chat to open a new vote.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            return false;
                        }
                        if (WeatherVote.IsEnabled && _message.ToLower() == "snow")
                        {
                            if (WeatherVote.VoteOpen)
                            {
                                if (!WeatherVote.snow.Contains(_cInfo.entityId) && !WeatherVote.clear.Contains(_cInfo.entityId) && !WeatherVote.rain.Contains(_cInfo.entityId))
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
                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + ", there is no active weather vote.Type /weather in chat to open a new vote.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                            }
                            return false;
                        }
                        if (RestartVote.IsEnabled && _message.ToLower() == "restartvote")
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
                        if (MuteVote.IsEnabled && _message.ToLower().StartsWith("mutevote"))
                        {
                            if (!KickVote.VoteOpen && !RestartVote.VoteOpen && !MuteVote.VoteOpen && !NightVote.VoteOpen)
                            {
                                if (_message.ToLower() == ("mutevote"))
                                {
                                    MuteVote.List(_cInfo);
                                }
                                else
                                {
                                    _message = _message.ToLower().Replace("mutevote ", "");
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
                        if (KickVote.IsEnabled && _message.ToLower().StartsWith("kickvote"))
                        {
                            if (!KickVote.VoteOpen && !RestartVote.VoteOpen && !MuteVote.VoteOpen && !NightVote.VoteOpen)
                            {
                                if (_message.ToLower() == ("kickvote"))
                                {
                                    KickVote.List(_cInfo);
                                }
                                else if (_message.ToLower().StartsWith("kickvote "))
                                {
                                    _message = _message.ToLower().Replace("kickvote ", "");
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
                        if (NightVote.IsEnabled && _message.ToLower() == "nightvote")
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
                        if (_message.ToLower() == "yes")
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
                        if (AuctionBox.IsEnabled && _message.ToLower() == "auction")
                        {
                            AuctionBox.AuctionList(_cInfo);
                            return false;
                        }
                        if (AuctionBox.IsEnabled && _message.ToLower() == "auction cancel")
                        {
                            AuctionBox.CancelAuction(_cInfo);
                            return false;
                        }
                        if (AuctionBox.IsEnabled && _message.ToLower().StartsWith("auction buy "))
                        {
                            if (AuctionBox.No_Admins)
                            {
                                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                                if (Admin.PermissionLevel > Admin_Level)
                                {
                                    _message = _message.ToLower().Replace("auction buy ", "");
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
                                                ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you have used an auction item # that does not exist or has sold. Type /auction.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
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
                                _message = _message.ToLower().Replace("auction buy ", "");
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
                                            ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " you have used an auction item # that does not exist or has sold. Type /auction.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                        }
                                        _result.Dispose();
                                    }
                                }
                            }
                            return false;
                        }
                        if (AuctionBox.IsEnabled && _message.ToLower().StartsWith("auction sell"))
                        {
                            if (AuctionBox.No_Admins)
                            {
                                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                                if (Admin.PermissionLevel > Admin_Level)
                                {
                                    _message = _message.ToLower().Replace("auction sell ", "");
                                    AuctionBox.Delay(_cInfo, _message);
                                }
                                else
                                {
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color + " the auction is disabled for your tier.[-]", _senderId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                _message = _message.ToLower().Replace("auction sell ", "");
                                AuctionBox.Delay(_cInfo, _message);
                            }
                            return false;
                        }
                        if (Fps.IsEnabled && _message.ToLower() == "fps")
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
                        if (Loc.IsEnabled && _message.ToLower() == "loc")
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
                            if (VehicleTeleport.Bike && _message.ToLower() == "bike")
                            {
                                VehicleTeleport.VehicleDelay(_cInfo, _mainName, 1);
                                return false;
                            }
                            if (VehicleTeleport.Mini_Bike && _message.ToLower() == "minibike")
                            {
                                VehicleTeleport.VehicleDelay(_cInfo, _mainName, 2);
                                return false;
                            }
                            if (VehicleTeleport.Motor_Bike && _message.ToLower() == "motorbike")
                            {
                                VehicleTeleport.VehicleDelay(_cInfo, _mainName, 3);
                                return false;
                            }
                            if (VehicleTeleport.Jeep && _message.ToLower() == "jeep")
                            {
                                VehicleTeleport.VehicleDelay(_cInfo, _mainName, 4);
                                return false;
                            }
                            if (VehicleTeleport.Gyro && _message.ToLower() == "gyro")
                            {
                                VehicleTeleport.VehicleDelay(_cInfo, _mainName, 5);
                                return false;
                            }
                        }
                        if (Report.IsEnabled && _message.ToLower().StartsWith("report"))
                        {
                            _message = _message.ToLower().Replace("report ", "");
                            Report.Check(_cInfo, _message);
                            return false;
                        }
                        if (Bounties.IsEnabled && _message.ToLower() == "bounty")
                        {
                            Bounties.BountyList(_cInfo, _mainName);
                            return false;
                        }
                        if (Bounties.IsEnabled && _message.ToLower().StartsWith("bounty"))
                        {
                            _message = _message.ToLower().Replace("bounty ", "");
                            Bounties.NewBounty(_cInfo, _message, _mainName);
                            return false;
                        }
                        if (Lottery.IsEnabled && _message.ToLower() == "lottery")
                        {
                            Lottery.Response(_cInfo);
                            return false;
                        }
                        if (Lottery.IsEnabled && _message.ToLower() == "lottery enter")
                        {
                            Lottery.EnterLotto(_cInfo);
                            return false;
                        }
                        if (Lottery.IsEnabled && _message.ToLower().StartsWith("lottery"))
                        {
                            _message = _message.Replace("lottery ", "");
                            Lottery.NewLotto(_cInfo, _message, _mainName);
                            return false;
                        }
                        if (NewSpawnTele.IsEnabled && NewSpawnTele.Return && _message.ToLower() == "ready")
                        {
                            NewSpawnTele.ReturnPlayer(_cInfo);
                            return false;
                        }
                        if (LobbyChat.IsEnabled && _message.ToLower() == "setlobby")
                        {
                            SetLobby.Set(_cInfo);
                            return false;
                        }
                        if (LobbyChat.IsEnabled && _message.ToLower() == "lobby")
                        {
                            LobbyChat.Delay(_cInfo, _mainName, _announce);
                            return false;
                        }
                        if (PlayerList.IsEnabled && _message.ToLower() == "list")
                        {
                            PlayerList.Exec(_cInfo, _mainName);
                            return false;
                        }
                        if (Stuck.IsEnabled && _message.ToLower() == "stuck")
                        {
                            Stuck.Delay(_cInfo, _mainName, _announce);
                            return false;
                        }
                        if (_message.ToLower() == "pollyes")
                        {
                            PollConsole.VoteYes(_cInfo);
                            return false;
                        }
                        if (_message.ToLower() == "pollno")
                        {
                            PollConsole.VoteNo(_cInfo);
                            return false;
                        }
                        if (_message.ToLower() == "poll")
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
                                    ChatMessage(_cInfo, ChatHook.Player_Name_Color + _cInfo.playerName + LoadConfig.Chat_Response_Color +_phrase926 + "[-]", _senderId, _cInfo.playerName, EChatType.Global, null);
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
                        if (Bank.IsEnabled && _message.ToLower() == "bank")
                        {
                            Bank.Check(_cInfo);
                            return false;
                        }
                        if (Bank.IsEnabled && _message.ToLower().StartsWith("deposit"))
                        {
                            _message = _message.ToLower().Replace("deposit ", "");
                            Bank.CheckLocation(_cInfo, _message, 1);
                            return false;
                        }
                        if (Bank.IsEnabled && _message.ToLower().StartsWith("withdraw"))
                        {
                            _message = _message.ToLower().Replace("withdraw ", "");
                            Bank.CheckLocation(_cInfo, _message, 2);
                            return false;
                        }
                        if (Bank.IsEnabled && _message.ToLower().StartsWith("wallet deposit"))
                        {
                            _message = _message.ToLower().Replace("wallet deposit ", "");
                            Bank.CheckLocation(_cInfo, _message, 3);
                            return false;
                        }
                        if (Bank.IsEnabled && _message.ToLower().StartsWith("wallet withdraw"))
                        {
                            _message = _message.ToLower().Replace("wallet withdraw ", "");
                            Bank.CheckLocation(_cInfo, _message, 4);
                            return false;
                        }
                        if (Bank.IsEnabled && _message.ToLower().StartsWith("transfer "))
                        {
                            _message = _message.ToLower().Replace("transfer ", "");
                            Bank.Transfer(_cInfo, _message);
                            return false;
                        }
                        if (Event.Invited && _message.ToLower() == "event")
                        {
                            Event.AddPlayer(_cInfo);
                            return false;
                        }
                        if (Mogul.IsEnabled && _message.ToLower() == "mogul")
                        {
                            if (Wallet.IsEnabled || Bank.IsEnabled)
                            {
                                Mogul.TopThree(_cInfo, _announce);
                                return false;
                            }
                        }
                        if (MarketChat.IsEnabled && _message.ToLower() == "setmarket")
                        {
                            SetMarket.Set(_cInfo);
                            return false;
                        }
                        if (MarketChat.IsEnabled && _message.ToLower() == "market")
                        {
                            MarketChat.Delay(_cInfo, _mainName, _announce);
                            return false;
                        }
                        if (InfoTicker.IsEnabled && _message.ToLower() == "infoticker")
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
                        if (Session.IsEnabled && _message.ToLower() == "session")
                        {
                            Session.Exec(_cInfo);
                            return false;
                        }
                        if (Waypoint.IsEnabled && (_message.ToLower() == "waypoint" || _message.ToLower() == "way" || _message.ToLower() == "wp"))
                        {
                            Waypoint.List(_cInfo);
                            return false;
                        }
                        else if (Waypoint.IsEnabled && (_message.StartsWith("waypoint ") || _message.StartsWith("way ") || _message.StartsWith("wp ")))
                        {
                            if (_message.ToLower().StartsWith("waypoint"))
                            {
                                _message = _message.ToLower().Replace("waypoint ", "");
                            }
                            else if (_message.ToLower().StartsWith("way"))
                            {
                                _message = _message.ToLower().Replace("way ", "");
                            }
                            else
                            {
                                _message = _message.ToLower().Replace("wp ", "");
                            }
                            Waypoint.Delay(_cInfo, _message);
                            return false;
                        }
                        if (Waypoint.IsEnabled && _message.StartsWith("fway "))
                        {
                            string _waypointNumber = _message.ToLower().Replace("fway ", "");
                            if (_waypointNumber != " " || _waypointNumber != "")
                            {
                                Waypoint.FDelay(_cInfo, _waypointNumber);
                                return false;
                            }
                        }
                        if (Waypoint.IsEnabled && (_message.ToLower().StartsWith("waypointsave") || _message.ToLower().StartsWith("waysave") || _message.ToLower().StartsWith("ws")))
                        {
                            if (_message.ToLower().StartsWith("waysave"))
                            {
                                _message = _message.ToLower().Replace("waysave ", "");
                            }
                            else
                            {
                                _message = _message.ToLower().Replace("ws ", "");
                            }
                            Waypoint.SaveClaimCheck(_cInfo, _message);
                            return false;
                        }
                        if (Waypoint.IsEnabled && (_message.ToLower().StartsWith("waypointdel") || _message.ToLower().StartsWith("waydel") || _message.ToLower().StartsWith("wd")))
                        {
                            if (_message.ToLower().StartsWith("waypointdel"))
                            {
                                _message = _message.ToLower().Replace("waypointdel ", "");
                            }
                            else if (_message.ToLower().StartsWith("waydel"))
                            {
                                _message = _message.ToLower().Replace("waydel ", "");
                            }
                            else
                            {
                                _message = _message.ToLower().Replace("wd ", "");
                            }
                            Waypoint.DelPoint(_cInfo, _message);
                            return false;
                        }
                        if (MarketChat.IsEnabled && _message.ToLower() == "market")
                        {
                            MarketChat.Delay(_cInfo, _mainName, _announce);
                            return false;
                        }
                        _message = _message.ToLower();
                        if (CustomCommands.IsEnabled && CustomCommands.Dict.ContainsKey(_message))
                        {
                            CustomCommands.CheckCustomDelay(_cInfo, _message, _mainName, _announce);
                            return false;
                        }
                    }
                }
                if (AdminChat.IsEnabled && _message.StartsWith("@"))
                {
                    if (_message.StartsWith("@admins ") || _message.StartsWith("@ADMINS "))
                    {
                        AdminChat.SendAdmins(_cInfo, _message);
                        return false;
                    }
                    if (_message.StartsWith("@all ") || _message.StartsWith("@ALL "))
                    {
                        AdminChat.SendAll(_cInfo, _message);
                        return false;
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
                if (Party_Chat_Color.StartsWith("[") && Party_Chat_Color.EndsWith("]"))
                {
                    _message = _message.Insert(0, Party_Chat_Color);
                }
                _cInfo.SendPackage(new NetPackageChat(EChatType.Whisper, 33, _message, _name, false, null));
            }
        }
    }
}