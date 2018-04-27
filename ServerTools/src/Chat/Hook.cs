using System;
using System.Collections.Generic;
using System.Text;

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
        public static int Admin_Level = 0, Mod_Level = 1;
        public static int Don_Level1 = 100, Don_Level2 = 101, Don_Level3 = 102;
        public static string Special_Players_List = "76561191234567891,76561191987654321";
        public static bool ChatCommandPrivateEnabled = false, ChatCommandPublicEnabled = false;
        public static string Command_Private = "/", Command_Public = "!";
        private static string filepath = string.Format("{0}/ServerTools.bin", GameUtils.GetSaveGameDir());
        private static SortedDictionary<string, DateTime> Dict = new SortedDictionary<string, DateTime>();
        private static SortedDictionary<string, string> Dict1 = new SortedDictionary<string, string>();
        public static List<string> SpecialPlayers = new List<string>();
        private static List<string> SpecialPlayersColorOff = new List<string>();

        public static void SpecialIdCheck()
        {
            if (Special_Player_Name_Coloring)
            {
                SpecialPlayers.Clear();
                var s_Id = Special_Players_List.Split(',');
                foreach (var specialId in s_Id)
                {
                    SpecialPlayers.Add(specialId.ToString());
                }
            }
        }

        public static bool Hook(ClientInfo _cInfo, string _message, string _playerName, string _secondaryName, bool _localizeSecondary)
        {
            if (!string.IsNullOrEmpty(_message) && _cInfo != null && _playerName != Config.Server_Response_Name && _secondaryName != "ServerTools")
            {
                if (ChatFlood)
                {
                    if (_message.Length > 500)
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Message too long.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                        return false;
                    }
                }
                if (ChatLog.IsEnabled)
                {
                    ChatLog.Log(_message, _playerName);
                }
                if (MutePlayer.IsEnabled)
                {
                    if (MutePlayer.Mutes.Contains(_cInfo.playerId))
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, "You are muted.", Config.Server_Response_Name, false, "ServerTools", false));
                        return false;
                    }
                }
                if (!Jail.Jailed.Contains(_cInfo.playerId))
                {
                    if (Admin_Name_Coloring && !_message.StartsWith("@") && _secondaryName != "ServerTools1" && _secondaryName != "Coppis" && !_message.StartsWith(Command_Private) && !_message.StartsWith(Command_Public) && GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId) && !AdminChatColor.AdminColorOff.Contains(_cInfo.playerId))
                    {
                        AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                        if (Admin.PermissionLevel <= Admin_Level)
                        {
                            if (!ClanManager.ClanMember.Contains(_cInfo.playerId))
                            {
                                if (Admin_Prefix != "")
                                {
                                    _playerName = string.Format("{0}{1} {2}[-]", Admin_Color, Admin_Prefix, _playerName);
                                    GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, _message, _playerName, false, "ServerTools1", false);
                                    return false;
                                }
                                else
                                {
                                    _playerName = string.Format("{0}{1}[-]", Admin_Color, _playerName);
                                    GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, _message, _playerName, false, "ServerTools1", false);
                                    return false;
                                }
                            }
                            else
                            {
                                _playerName = string.Format("{0}({1}) {2}[-]", Admin_Color, PersistentContainer.Instance.Players[_cInfo.playerId, false].ClanName, _playerName);
                                GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, _message, _playerName, false, "ServerTools1", false);
                                return false;
                            }
                        }
                        if (Admin.PermissionLevel > Admin_Level & Admin.PermissionLevel <= Mod_Level)
                        {
                            if (!ClanManager.ClanMember.Contains(_cInfo.playerId))
                            {
                                if (Mod_Prefix != "")
                                {
                                    _playerName = string.Format("{0}{1} {2}[-]", Mod_Color, Mod_Prefix, _playerName);
                                    GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, _message, _playerName, false, "ServerTools1", false);
                                    return false;
                                }
                                else
                                {
                                    _playerName = string.Format("{0}{1}[-]", Mod_Color, _playerName);
                                    GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, _message, _playerName, false, "ServerTools1", false);
                                    return false;
                                }
                            }
                            else
                            {
                                _playerName = string.Format("{0}({1}) {2}[-]", Mod_Color, PersistentContainer.Instance.Players[_cInfo.playerId, false].ClanName, _playerName);
                                GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, _message, _playerName, false, "ServerTools1", false);
                                return false;
                            }
                        }
                    }
                    if (Donator_Name_Coloring && !_message.StartsWith("@") && _secondaryName != "ServerTools1" && _secondaryName != "Coppis" && !_message.StartsWith(Command_Private) && !_message.StartsWith(Command_Public))
                    {
                        AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                        if (Admin.PermissionLevel == Don_Level1)
                        {
                            if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                            {
                                DateTime _dt;
                                ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                if (DateTime.Now < _dt)
                                {
                                    if (!ClanManager.ClanMember.Contains(_cInfo.playerId))
                                    {
                                        if (Don_Prefix1 != "")
                                        {
                                            _playerName = string.Format("{0}{1} {2}[-]", Don_Color1, Don_Prefix1, _playerName);
                                            GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, _message, _playerName, false, "ServerTools1", false);
                                            return false;
                                        }
                                        else
                                        {
                                            _playerName = string.Format("{0}{1}[-]", Don_Color1, _playerName);
                                            GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, _message, _playerName, false, "ServerTools1", false);
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        _playerName = string.Format("{0}({1}) {2}[-]", Don_Color1, PersistentContainer.Instance.Players[_cInfo.playerId, false].ClanName, _playerName);
                                        GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, _message, _playerName, false, "ServerTools1", false);
                                        return false;
                                    }
                                }
                            }
                        }
                        if (Admin.PermissionLevel == Don_Level2)
                        {
                            if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                            {
                                DateTime _dt;
                                ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                if (DateTime.Now < _dt)
                                {
                                    if (!ClanManager.ClanMember.Contains(_cInfo.playerId))
                                    {
                                        if (Don_Prefix2 != "")
                                        {
                                            _playerName = string.Format("{0}{1} {2}[-]", Don_Color2, Don_Prefix2, _playerName);
                                            GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, _message, _playerName, false, "ServerTools1", false);
                                            return false;
                                        }
                                        else
                                        {
                                            _playerName = string.Format("{0}{1}[-]", Don_Color2, _playerName);
                                            GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, _message, _playerName, false, "ServerTools1", false);
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        _playerName = string.Format("{0}({1}) {2}[-]", Don_Color2, PersistentContainer.Instance.Players[_cInfo.playerId, false].ClanName, _playerName);
                                        GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, _message, _playerName, false, "ServerTools1", false);
                                        return false;
                                    }
                                }
                            }
                        }
                        if (Admin.PermissionLevel == Don_Level3)
                        {
                            if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                            {
                                DateTime _dt;
                                ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                if (DateTime.Now < _dt)
                                {
                                    if (!ClanManager.ClanMember.Contains(_cInfo.playerId))
                                    {
                                        if (Don_Prefix3 != "")
                                        {
                                            _playerName = string.Format("{0}{1} {2}[-]", Don_Color3, Don_Prefix3, _playerName);
                                            GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, _message, _playerName, false, "ServerTools1", false);
                                            return false;
                                        }
                                        else
                                        {
                                            _playerName = string.Format("{0}{1}[-]", Don_Color3, _playerName);
                                            GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, _message, _playerName, false, "ServerTools1", false);
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        _playerName = string.Format("{0}({1}) {2}[-]", Don_Color2, PersistentContainer.Instance.Players[_cInfo.playerId, false].ClanName, _playerName);
                                        GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, _message, _playerName, false, "ServerTools1", false);
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                    if (Special_Player_Name_Coloring && !_message.StartsWith("@") && _secondaryName != "ServerTools1" && _secondaryName != "Coppis" && !_message.StartsWith(Command_Private) && !_message.StartsWith(Command_Public) && SpecialPlayers.Contains(_cInfo.playerId) && !SpecialPlayersColorOff.Contains(_cInfo.playerId))
                    {
                        if (!ClanManager.ClanMember.Contains(_cInfo.playerId))
                        {
                            if (Special_Player_Prefix != "")
                            {
                                _playerName = string.Format("{0}{1} {2}[-]", Special_Player_Color, Special_Player_Prefix, _playerName);
                                GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, _message, _playerName, false, "ServerTools1", false);
                                return false;
                            }
                            else
                            {
                                _playerName = string.Format("{0}{1}[-]", Special_Player_Color, _playerName);
                                GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, _message, _playerName, false, "ServerTools1", false);
                                return false;
                            }
                        }
                        else
                        {
                            _playerName = string.Format("{0}({1}) {2}[-]", Special_Player_Color, PersistentContainer.Instance.Players[_cInfo.playerId, false].ClanName, _playerName);
                            GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, _message, _playerName, false, "ServerTools1", false);
                            return false;
                        }
                    }
                    if (Normal_Player_Name_Coloring && !_message.StartsWith("@") && _secondaryName != "ServerTools1" && _secondaryName != "Coppis" && !_message.StartsWith(Command_Private) && !_message.StartsWith(Command_Public))
                    {
                        if (!ClanManager.ClanMember.Contains(_cInfo.playerId))
                        {
                            if (Normal_Player_Prefix != "")
                            {
                                _playerName = string.Format("{0}{1} {2}[-]", Normal_Player_Color, Normal_Player_Prefix, _playerName);
                                GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, _message, _playerName, false, "ServerTools1", false);
                                return false;
                            }
                            else
                            {
                                _playerName = string.Format("{0}{1}[-]", Normal_Player_Color, _playerName);
                                GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, _message, _playerName, false, "ServerTools1", false);
                                return false;
                            }
                        }
                        else
                        {
                            _playerName = string.Format("{0}({1}) {2}[-]", Normal_Player_Color, PersistentContainer.Instance.Players[_cInfo.playerId, false].ClanName, _playerName);
                            GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, _message, _playerName, false, "ServerTools1", false);
                            return false;
                        }
                    }
                    if (ClanManager.IsEnabled && !_message.StartsWith("@") && _secondaryName != "ServerTools1" && _secondaryName != "Coppis" && !_message.StartsWith(Command_Private) && !_message.StartsWith(Command_Public) && ClanManager.ClanMember.Contains(_cInfo.playerId))
                    {
                        _playerName = string.Format("({0}) {1}[-]", PersistentContainer.Instance.Players[_cInfo.playerId, false].ClanName, _playerName);
                        GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, _message, _playerName, false, "ServerTools1", false);
                        return false;
                    }
                    if (Badwords.Invalid_Name)
                    {
                        bool _hasBadName = false;
                        string _playerName1 = _playerName.ToLower();
                        foreach (string _word in Badwords.List)
                        {
                            if (_playerName1.Contains(_word))
                            {
                                _hasBadName = true;
                            }
                        }
                        if (_hasBadName)
                        {
                            GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, _message, "Invalid Name-No Commands", false, "ServerTools", false);
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
                            GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, _message1, Config.Server_Response_Name, false, "ServerTools", false);
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
                    if ((_message.StartsWith("[") && _message.Contains("]")) && (_message.Contains(Command_Private) || _message.Contains(Command_Public)) || _message.StartsWith(Command_Private) || _message.StartsWith(Command_Public))
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
                        _message = _message.ToLower();
                        if (TeleportHome.IsEnabled && _message == "sethome")
                        {
                            if (!Zones.Set_Home)
                            {
                                if (!Players.ZoneFlag.ContainsKey(_cInfo.entityId))
                                {
                                    TeleportHome.SetHome(_cInfo, _playerName, _announce);
                                }
                                else
                                {
                                    if (_announce)
                                    {
                                        GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}You can not use sethome in a protected zone.[-]", _message), Config.Server_Response_Name, false, "ServerTools", true);
                                    }
                                    else
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You can not use sethome in a protected zone.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                }
                            }
                            else
                            {

                                TeleportHome.SetHome(_cInfo, _playerName, _announce);
                            }
                            return false;
                        }
                        if (TeleportHome.IsEnabled && _message == "home")
                        {

                            TeleportHome.TeleHome(_cInfo, _playerName, _announce);
                            return false;
                        }
                        if (TeleportHome.IsEnabled && _message == "fhome")
                        {

                            TeleportHome.FTeleHome(_cInfo, _playerName, _announce);
                            return false;
                        }
                        if (TeleportHome.IsEnabled && _message == "delhome")
                        {

                            TeleportHome.DelHome(_cInfo, _playerName, _announce);
                            return false;
                        }
                        if (TeleportHome.Set_Home2_Enabled && _message == "sethome2")
                        {
                            if (TeleportHome.Set_Home2_Donor_Only && ReservedSlots.IsEnabled)
                            {
                                if (!Zones.Set_Home)
                                {
                                    if (!Players.ZoneFlag.ContainsKey(_cInfo.entityId))
                                    {
                                        if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                        {
                                            DateTime _dt;
                                            ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                            if (DateTime.Now < _dt)
                                            {

                                                TeleportHome.SetHome2(_cInfo, _playerName, _announce);
                                            }
                                            else
                                            {
                                                if (_announce)
                                                {
                                                    GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}Your reserved status has expired. Command is unavailable.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", true);
                                                }
                                                else
                                                {
                                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Your reserved status has expired Command is unavailable..[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (_announce)
                                            {
                                                GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}You are not listed as a reserved player. Command is unavailable.[-]", _message), Config.Server_Response_Name, false, "ServerTools", true);
                                            }
                                            else
                                            {
                                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You are not listed as a reserved player. Command is unavailable.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (_announce)
                                        {
                                            GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}You can not use sethome2 in a protected zone.[-]", _message), Config.Server_Response_Name, false, "ServerTools", true);
                                        }
                                        else
                                        {
                                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You can not use sethome2 in a protected zone.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
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
                                            TeleportHome.SetHome2(_cInfo, _playerName, _announce);
                                        }
                                        else
                                        {
                                            if (_announce)
                                            {
                                                GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}Your reserved status has expired. Command is unavailable.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", true);
                                            }
                                            else
                                            {
                                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Your reserved status has expired Command is unavailable..[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (_announce)
                                        {
                                            GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}You are not listed as a reserved player. Command is unavailable.[-]", _message), Config.Server_Response_Name, false, "ServerTools", true);
                                        }
                                        else
                                        {
                                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You are not listed as a reserved player. Command is unavailable.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                        }
                                    }
                                }
                            }
                            else if (TeleportHome.Set_Home2_Enabled && !TeleportHome.Set_Home2_Donor_Only)
                            {
                                if (!Zones.Set_Home)
                                {
                                    if (!Players.ZoneFlag.ContainsKey(_cInfo.entityId))
                                    {
                                        TeleportHome.SetHome2(_cInfo, _playerName, _announce);
                                    }
                                    else
                                    {
                                        if (_announce)
                                        {
                                            GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}You can not use sethome2 in a protected zone.[-]", _message), Config.Server_Response_Name, false, "ServerTools", true);
                                        }
                                        else
                                        {
                                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You can not use sethome2 in a protected zone.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                        }
                                    }
                                }
                                else
                                {
                                    TeleportHome.SetHome2(_cInfo, _playerName, _announce);
                                }
                            }
                            return false;
                        }
                        if (TeleportHome.IsEnabled && _message == "home2")
                        {
                            if (TeleportHome.Set_Home2_Enabled && TeleportHome.Set_Home2_Donor_Only && ReservedSlots.IsEnabled)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        TeleportHome.TeleHome2(_cInfo, _playerName, _announce);
                                    }
                                    else
                                    {
                                        if (_announce)
                                        {
                                            GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}Your reserved status has expired. Command is unavailable.[-]", Config.Chat_Response_Color), _playerName, false, "ServerTools", true);
                                        }
                                        else
                                        {
                                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Your reserved status has expired. Command is unavailable.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                        }
                                    }
                                }
                                else
                                {
                                    if (_announce)
                                    {
                                        GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}You are not on the reserved list, please donate or contact an admin.[-]", Config.Chat_Response_Color), _playerName, false, "ServerTools", true);
                                    }
                                    else
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You are not on the reserved list, please donate or contact an admin.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                }
                            }
                            else if (TeleportHome.Set_Home2_Enabled && !TeleportHome.Set_Home2_Donor_Only)
                            {
                                TeleportHome.TeleHome2(_cInfo, _playerName, _announce);
                            }
                            else
                            {
                                if (_announce)
                                {
                                    GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}Home2 is not enabled.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", true);
                                }
                                else
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Home2 is not enabled.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                            }
                            return false;
                        }
                        if (TeleportHome.IsEnabled && _message == "fhome2")
                        {
                            if (TeleportHome.Set_Home2_Enabled && TeleportHome.Set_Home2_Donor_Only && ReservedSlots.IsEnabled)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        TeleportHome.FTeleHome2(_cInfo, _playerName, _announce);
                                    }
                                    else
                                    {
                                        if (_announce)
                                        {
                                            GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}Your reserved status has expired. Command is unavailable.[-]", Config.Chat_Response_Color), _playerName, false, "ServerTools", true);
                                        }
                                        else
                                        {
                                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Your reserved status has expired. Command is unavailable.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                        }
                                    }
                                }
                                else
                                {
                                    if (_announce)
                                    {
                                        GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}You are not on the reserved list, please donate or contact an admin.[-]", Config.Chat_Response_Color), _playerName, false, "ServerTools", true);
                                    }
                                    else
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You are not on the reserved list, please donate or contact an admin.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                }
                            }
                            else if (TeleportHome.Set_Home2_Enabled && !TeleportHome.Set_Home2_Donor_Only)
                            {
                                TeleportHome.FTeleHome2(_cInfo, _playerName, _announce);
                            }
                            else
                            {
                                if (_announce)
                                {
                                    GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}Home2 is not enabled.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", true);
                                }
                                else
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Home2 is not enabled.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                            }
                            return false;
                        }
                        if (TeleportHome.IsEnabled && _message == "delhome2")
                        {
                            if (TeleportHome.Set_Home2_Enabled && TeleportHome.Set_Home2_Donor_Only && ReservedSlots.IsEnabled)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        TeleportHome.DelHome2(_cInfo, _playerName, _announce);
                                    }
                                    else
                                    {
                                        if (_announce)
                                        {
                                            GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}Your reserved status has expired. Command is unavailable.[-]", Config.Chat_Response_Color), _playerName, false, "ServerTools", true);
                                        }
                                        else
                                        {
                                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Your reserved status has expired. Command is unavailable.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                        }
                                    }
                                }
                                else
                                {
                                    if (_announce)
                                    {
                                        GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}You are not on the reserved list, please donate or contact an admin.[-]", Config.Chat_Response_Color), _playerName, false, "ServerTools", true);
                                    }
                                    else
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You are not on the reserved list, please donate or contact an admin.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                }
                            }
                            else if (TeleportHome.Set_Home2_Enabled && !TeleportHome.Set_Home2_Donor_Only)
                            {
                                TeleportHome.DelHome2(_cInfo, _playerName, _announce);
                            }
                            return false;
                        }
                        if (TeleportHome.IsEnabled && _message == "go")
                        {
                            if (TeleportHome.Invite.ContainsKey(_cInfo.entityId))
                            {
                                TeleportHome.FriendHome(_cInfo);
                                return false;
                            }
                        }
                        if (Hardcore.IsEnabled && _message == "top3")
                        {
                            Hardcore.TopThree(_cInfo, _announce);
                            return false;
                        }
                        if (Hardcore.IsEnabled && _message == "score")
                        {
                            Hardcore.Score(_cInfo, _announce);
                            return false;
                        }
                        if (AdminChat.IsEnabled)
                        {
                            if (_message.StartsWith("mute ") || _message.StartsWith("unmute "))
                            {
                                if (_message.StartsWith("mute "))
                                {
                                    MutePlayer.Add(_cInfo, _message);
                                }
                                if (_message.StartsWith("unmute "))
                                {
                                    MutePlayer.Remove(_cInfo, _message);
                                }
                                return false;
                            }
                        }
                        if (CustomCommands.IsEnabled && _message == "commands")
                        {
                            string _commands1 = CustomCommands.GetChatCommands1(_cInfo);
                            string _commands2 = CustomCommands.GetChatCommands2(_cInfo);
                            string _commands3 = CustomCommands.GetChatCommands3(_cInfo);
                            string _commands4 = CustomCommands.GetChatCommands4(_cInfo);
                            string _commandsCustom = CustomCommands.GetChatCommandsCustom(_cInfo);
                            string _commandsAdmin = CustomCommands.GetChatCommandsAdmin(_cInfo);
                            if (_announce)
                            {
                                GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, _commands1, Config.Server_Response_Name, false, "ServerTools", false);
                                GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, _commands2, Config.Server_Response_Name, false, "ServerTools", false);
                                GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, _commands3, Config.Server_Response_Name, false, "ServerTools", false);
                                GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, _commands4, Config.Server_Response_Name, false, "ServerTools", false);
                                if (CustomCommands.IsEnabled)
                                {
                                    GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, _commandsCustom, Config.Server_Response_Name, false, "ServerTools", false);
                                }
                                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                                if (Admin.PermissionLevel <= Admin_Level)
                                {
                                    GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, _commandsAdmin, Config.Server_Response_Name, false, "ServerTools", false);
                                }
                            }
                            else
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, _commands1, Config.Server_Response_Name, false, "ServerTools", false));
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, _commands2, Config.Server_Response_Name, false, "ServerTools", false));
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, _commands3, Config.Server_Response_Name, false, "ServerTools", false));
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, _commands4, Config.Server_Response_Name, false, "ServerTools", false));
                                if (CustomCommands.IsEnabled)
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, _commandsCustom, Config.Server_Response_Name, false, "ServerTools", false));
                                }
                                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                                if (Admin.PermissionLevel <= Admin_Level)
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, _commandsAdmin, Config.Server_Response_Name, false, "ServerTools", false));
                                }
                            }
                            return false;
                        }
                        if (Day7.IsEnabled && (_message == "day7" || _message == "day"))
                        {
                            if (_announce)
                            {
                                GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}{1}", Command_Public, _message), Config.Server_Response_Name, false, "ServerTools", true);
                            }
                            Day7.GetInfo(_cInfo, _announce);
                            return false;
                        }
                        if (Bloodmoon.IsEnabled && (_message == "bloodmoon" || _message == "bm"))
                        {
                            Bloodmoon.GetBloodmoon(_cInfo, _announce);
                            return false;
                        }
                        if (Suicide.IsEnabled && (_message == "killme" || _message == "wrist" || _message == "hang" || _message == "suicide"))
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
                        if (Gimme.IsEnabled && (_message == "gimme" || _message == "gimmie"))
                        {
                            if (Gimme.Always_Show_Response)
                            {
                                GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}{1}", Command_Public, _message), Config.Server_Response_Name, false, "ServerTools", true);
                                Gimme.Checkplayer(_cInfo, true, _playerName);
                            }
                            else
                            {
                                Gimme.Checkplayer(_cInfo, _announce, _playerName);
                            }
                            return false;
                        }
                        if (Jail.IsEnabled && (_message == "setjail" || _message.StartsWith("jail ") || _message.StartsWith("unjail ")))
                        {
                            if (_message == "setjail")
                            {
                                if (_announce)
                                {
                                    GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}{1}", Command_Public, _message), Config.Server_Response_Name, false, "ServerTools", true);
                                }
                                Jail.SetJail(_cInfo);
                            }
                            if (_message.StartsWith("jail "))
                            {
                                if (_announce)
                                {
                                    GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}{1}", Command_Public, _message), Config.Server_Response_Name, false, "ServerTools", true);
                                }
                                Jail.PutInJail(_cInfo, _message);
                            }
                            else
                            {
                                if (_announce)
                                {
                                    GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}{1}", Command_Public, _message), Config.Server_Response_Name, false, "ServerTools", true);
                                }
                                Jail.RemoveFromJail(_cInfo, _message);
                            }
                            return false;
                        }
                        if (NewSpawnTele.IsEnabled && _message == "setspawn")
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
                        if (Animals.IsEnabled && (_message == "trackanimal" || _message == "track"))
                        {
                            Animals.Checkplayer(_cInfo, _announce, _playerName);
                            return false;
                        }
                        if (FirstClaimBlock.IsEnabled && _message == "claim")
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Checking your claim block status.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                            FirstClaimBlock.firstClaim(_cInfo);
                            return false;
                        }
                        if (ClanManager.IsEnabled && (_message.StartsWith("clanadd") || _message == "clandel" || _message.StartsWith("claninvite") || _message == "clanaccept" || _message == "clandecline" || _message.StartsWith("clanremove") || _message.StartsWith("clanpromote") || _message.StartsWith("clandemote") || _message.StartsWith("clan") || _message == "clancommands"))
                        {
                            if (_message == "clancommands")
                            {
                                ClanManager.GetChatCommands(_cInfo);
                            }
                            if (_message.StartsWith("clanadd"))
                            {
                                if (_message == "clanadd")
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Usage: /clanadd clanName[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                                else
                                {
                                    _message = _message.Replace("clanadd ", "");
                                    ClanManager.AddClan(_cInfo, _message);
                                }
                            }
                            if (_message == "clandel")
                            {
                                ClanManager.RemoveClan(_cInfo);
                            }
                            if (_message.StartsWith("claninvite"))
                            {
                                if (_message == "claninvite")
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Usage: /claninvite playerName[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                                else
                                {
                                    _message = _message.Replace("claninvite ", "");
                                    ClanManager.InviteMember(_cInfo, _message);
                                }
                            }
                            if (_message == "clanaccept")
                            {
                                ClanManager.InviteAccept(_cInfo);
                                return false;
                            }
                            if (_message == "clandecline")
                            {
                                ClanManager.InviteDecline(_cInfo);
                            }
                            if (_message.StartsWith("clanremove"))
                            {
                                if (_message == "clanremove")
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Usage: /clanremove playerName[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                                else
                                {
                                    _message = _message.Replace("clanremove ", "");
                                    ClanManager.RemoveMember(_cInfo, _message);
                                }
                            }
                            if (_message.StartsWith("clanpromote"))
                            {
                                if (_message == "clanpromote")
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Usage: /clanpromote playerName[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                                else
                                {
                                    _message = _message.Replace("clanpromote ", "");
                                    ClanManager.PromoteMember(_cInfo, _message);
                                }
                            }
                            if (_message.StartsWith("clandemote"))
                            {
                                if (_message == "clandemote")
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Usage: /clandemote playerName[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                                else
                                {
                                    _message = _message.Replace("clandemote ", "");
                                    ClanManager.DemoteMember(_cInfo, _message);
                                }
                            }
                            if (_message == "clanleave")
                            {
                                ClanManager.LeaveClan(_cInfo);
                            }
                            if (_message.StartsWith("clan"))
                            {
                                if (_message == "clan")
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Usage: /clan message[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                                else
                                {
                                    _message = _message.Replace("clan ", "");
                                    ClanManager.Clan(_cInfo, _message);
                                }
                            }
                            return false;
                        }
                        if (Donator_Name_Coloring && _message == "doncolor")
                        {
                            if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                            {
                                GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId);
                                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                                if (Admin.PermissionLevel <= Mod_Level)
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Sorry {1}, your chat color can not be changed as a moderator or administrator.[-]", Config.Chat_Response_Color, _playerName), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                    else
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, your reserved status expired on {2}. Moderators and Admins can not change their chat color.[-]", Config.Chat_Response_Color, _playerName, _dt), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                }
                                if (Admin.PermissionLevel == Don_Level1)
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        int dl2 = Don_Level2;
                                        SdtdConsole.Instance.ExecuteSync(string.Format("admin add {0} {1}", _cInfo.entityId, dl2), _cInfo);
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, your chat color has been switched.[-]", Config.Chat_Response_Color, _playerName), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                    else
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, your reserved status expired on {2}. Command is unavailable.[-]", Config.Chat_Response_Color, _playerName, _dt), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                }
                                if (Admin.PermissionLevel == Don_Level2)
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        int dl3 = Don_Level3;
                                        SdtdConsole.Instance.ExecuteSync(string.Format("admin add {0} {1}", _cInfo.entityId, dl3), _cInfo);
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, your chat color has been switched.[-]", Config.Chat_Response_Color, _playerName), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                    else
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, your reserved status expired on {2}. Command is unavailable.[-]", Config.Chat_Response_Color, _playerName, _dt), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                }
                                if (Admin.PermissionLevel == Don_Level3)
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        SdtdConsole.Instance.ExecuteSync(string.Format("admin remove {0}", _cInfo.entityId), _cInfo);
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, your chat color has been turned off.[-]", Config.Chat_Response_Color, _playerName), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                    else
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, your reserved status expired on {2}. Command is unavailable.[-]", Config.Chat_Response_Color, _playerName, _dt), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                }
                                else
                                {
                                    int dl1 = Don_Level1;
                                    SdtdConsole.Instance.ExecuteSync(string.Format("admin add {0} {1}", _cInfo.entityId, Don_Level1), _cInfo);
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, your chat color has been turned on.[-]", Config.Chat_Response_Color, _playerName), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                            }
                            return false;
                        }
                        if (Reserved_Check && _message == "reserved")
                        {
                            if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                            {
                                DateTime _dt;
                                if (ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt))
                                {
                                    if (DateTime.Now < _dt)
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, your reserved status expires on {2}.[-]", Config.Chat_Response_Color, _playerName, _dt), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                    else
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, your reserved status has expired on {2}.[-]", Config.Chat_Response_Color, _playerName, _dt), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                }
                            }
                            else
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You have not donated {1}. Expiration date unavailable.[-]", Config.Chat_Response_Color, _playerName), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                            return false;
                        }
                        if (Special_Player_Name_Coloring && _message == "spcolor")
                        {
                            if (SpecialPlayers.Contains(_cInfo.playerId))
                            {
                                if (!SpecialPlayersColorOff.Contains(_cInfo.playerId))
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, your chat color has been turned off.[-]", Config.Chat_Response_Color, _playerName), Config.Server_Response_Name, false, "ServerTools", false));
                                    SpecialPlayersColorOff.Add(_cInfo.playerId);
                                }
                                else
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}, your chat color has been turned on.[-]", Config.Chat_Response_Color, _playerName), Config.Server_Response_Name, false, "ServerTools", false));
                                    SpecialPlayersColorOff.Remove(_cInfo.playerId);
                                }
                            }
                            return false;
                        }
                        if (VoteReward.IsEnabled && _message == "reward")
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Checking for your vote.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                            VoteReward.Check(_cInfo);
                            return false;
                        }
                        if (AutoShutdown.IsEnabled && _message == "shutdown")
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Checking for the next shutdown time.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                            AutoShutdown.CheckNextShutdown(_cInfo, _announce);
                            return false;
                        }
                        if (AdminList.IsEnabled && _message == "admin")
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Listing online administrators and moderators.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                            AdminList.List(_cInfo, _announce, _playerName);
                            return false;
                        }
                        if (Travel.IsEnabled && _message == "travel")
                        {
                            Travel.Check(_cInfo, _announce, _playerName);
                            return false;
                        }
                        if (Zones.IsEnabled && _message == "return")
                        {
                            if (Players.Victim.ContainsKey(_cInfo.entityId))
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Sending you to your death point.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                Zones.ReturnToPosition(_cInfo);
                                return false;
                            }
                        }
                        if (Zones.IsEnabled && Jail.IsEnabled && _message == "forgive")
                        {
                            if (Players.Forgive.ContainsKey(_cInfo.entityId))
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Your killer has been forgiven.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                Jail.Forgive(_cInfo);
                                return false;
                            }
                        }
                        if (WeatherVote.IsEnabled && _message == "weather")
                        {
                            if (WeatherVote.VoteNew)
                            {
                                if (!WeatherVote.VoteOpen)
                                {
                                    WeatherVote.CallForVote1();
                                }
                                else
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}A weather vote has already begun.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                            }
                            else
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}A weather vote can only begin every {1} minutes.[-]", Config.Chat_Response_Color, Timers.Weather_Vote_Delay), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                            return false;
                        }
                        if (WeatherVote.IsEnabled && _message == "clear")
                        {
                            if (WeatherVote.VoteOpen)
                            {
                                if (!WeatherVote.snow.Contains(_cInfo.entityId) && !WeatherVote.clear.Contains(_cInfo.entityId) && !WeatherVote.rain.Contains(_cInfo.entityId))
                                {
                                    WeatherVote.clear.Add(_cInfo.entityId);
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Vote cast for {1}.[-]", Config.Chat_Response_Color, _message), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                                else
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You have already voted.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                            }
                            else
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}There is no active weather vote. Type /weather in chat to open a new vote.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                            return false;
                        }
                        if (WeatherVote.IsEnabled && _message == "rain")
                        {
                            if (WeatherVote.VoteOpen)
                            {
                                if (!WeatherVote.snow.Contains(_cInfo.entityId) && !WeatherVote.clear.Contains(_cInfo.entityId) && !WeatherVote.rain.Contains(_cInfo.entityId))
                                {
                                    WeatherVote.rain.Add(_cInfo.entityId);
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Vote cast for {1}.[-]", Config.Chat_Response_Color, _message), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                                else
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You have already voted.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                            }
                            else
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}There is no active weather vote. Type /weather in chat to open a new vote.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                            return false;
                        }
                        if (WeatherVote.IsEnabled && _message == "snow")
                        {
                            if (WeatherVote.VoteOpen)
                            {
                                if (!WeatherVote.snow.Contains(_cInfo.entityId) && !WeatherVote.clear.Contains(_cInfo.entityId) && !WeatherVote.rain.Contains(_cInfo.entityId))
                                {
                                    WeatherVote.snow.Add(_cInfo.entityId);
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Vote cast for {1}.[-]", Config.Chat_Response_Color, _message), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                                else
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You have already voted.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                            }
                            else
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}There is no active weather vote. Type /weather in chat to open a new vote.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                            return false;
                        }
                        if (_message == "wallet")
                        {
                            Wallet.WalletValue(_cInfo, _playerName);
                            return false;
                        }
                        if (Shop.IsEnabled && _message == "shop")
                        {
                            Shop.Check(_cInfo, _playerName);
                            return false;
                        }
                        if (Shop.IsEnabled && _message.StartsWith("buy"))
                        {
                            if (_message == "buy")
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Usage: /buy #[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                            else
                            {
                                _message = _message.Replace("buy ", "");
                                Shop.BuyCheck(_cInfo, _message, _playerName);
                            }
                            return false;
                        }
                        if (FriendTeleport.IsEnabled && _message.StartsWith("friend"))
                        {
                            if (_message == "friend")
                            {
                                FriendTeleport.ListFriends(_cInfo, _message);
                            }
                            else
                            {
                                _message = _message.Replace("friend ", "");
                                FriendTeleport.CheckDelay(_cInfo, _message, _announce);
                            }
                            return false;
                        }
                        if (FriendTeleport.IsEnabled && _message == ("accept"))
                        {
                            if (FriendTeleport.Dict.ContainsKey(_cInfo.entityId))
                            {
                                int _dictValue;
                                if (FriendTeleport.Dict.TryGetValue(_cInfo.entityId, out _dictValue))
                                {
                                    DateTime _dict1Value;
                                    if (FriendTeleport.Dict1.TryGetValue(_cInfo.entityId, out _dict1Value))
                                    {
                                        TimeSpan varTime = DateTime.Now - _dict1Value;
                                        double fractionalSeconds = varTime.TotalSeconds;
                                        int _timepassed = (int)fractionalSeconds;
                                        if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                                        {
                                            if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                            {
                                                DateTime _dt;
                                                ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                                if (DateTime.Now < _dt)
                                                {
                                                    int _newTime = _timepassed / 2;
                                                    _timepassed = _newTime;
                                                }
                                            }
                                        }
                                        if (_timepassed <= 30)
                                        {
                                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Your friends teleport request was accepted.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                            FriendTeleport.TeleFriend(_cInfo, _dictValue);
                                            FriendTeleport.Dict.Remove(_cInfo.entityId);
                                            FriendTeleport.Dict1.Remove(_cInfo.entityId);
                                        }
                                        else
                                        {
                                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Your friends teleport request has expired.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                            FriendTeleport.Dict.Remove(_cInfo.entityId);
                                            FriendTeleport.Dict1.Remove(_cInfo.entityId);
                                        }
                                    }
                                }
                                return false;
                            }
                        }
                        if (DeathSpot.IsEnabled && _message == ("died"))
                        {
                            DeathSpot.DeathDelay(_cInfo, _announce, _playerName);
                            return false;
                        }
                        if (RestartVote.IsEnabled && _message == "restart")
                        {
                            if (!KickVote.VoteOpen && !RestartVote.VoteOpen && !MuteVote.VoteOpen)
                            {
                                if (RestartVote.VoteNew)
                                {
                                    RestartVote.CallForVote1(_cInfo);
                                }
                            }
                            else
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}A vote is open. Wait for it to finish and try again.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                            return false;
                        }
                        if (MuteVote.IsEnabled && _message == "mute")
                        {
                            if (!KickVote.VoteOpen && !RestartVote.VoteOpen && !MuteVote.VoteOpen && !NightVote.VoteOpen)
                            {
                                MuteVote.List(_cInfo);
                            }
                            else
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}A vote is open. Wait for it to finish and try again.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                            return false;
                        }
                        if (MuteVote.IsEnabled && _message.StartsWith("mute"))
                        {
                            if (!KickVote.VoteOpen && !RestartVote.VoteOpen && !MuteVote.VoteOpen && !NightVote.VoteOpen)
                            {
                                _message = _message.Replace("mute ", "");
                                {
                                    MuteVote.Vote(_cInfo, _message);
                                }
                            }
                            else
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}A vote is open. Wait for it to finish and try again.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                            return false;
                        }
                        if (KickVote.IsEnabled && _message == "kick")
                        {
                            if (!KickVote.VoteOpen && !RestartVote.VoteOpen && !MuteVote.VoteOpen && !NightVote.VoteOpen)
                            {
                                KickVote.List(_cInfo);
                            }
                            else
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}A vote is open. Wait for it to finish and try again.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                            return false;
                        }
                        if (_message.StartsWith("kick"))
                        {
                            if (!KickVote.VoteOpen && !RestartVote.VoteOpen && !MuteVote.VoteOpen && !NightVote.VoteOpen)
                            {
                                _message = _message.Replace("kick ", "");
                                {
                                    KickVote.Vote(_cInfo, _message);
                                }
                            }
                            else
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}A vote is open. Wait for it to finish and try again.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                            return false;
                        }
                        if (NightVote.IsEnabled && _message == "night")
                        {
                            if (!KickVote.VoteOpen && !RestartVote.VoteOpen && !MuteVote.VoteOpen && !NightVote.VoteOpen)
                            {
                                NightVote.Vote(_cInfo);
                            }
                            else
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}A vote is open. Wait for it to finish and try again.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                            return false;
                        }
                        if (_message == "yes")
                        {
                            if (KickVote.VoteOpen || RestartVote.VoteOpen || MuteVote.VoteOpen || NightVote.VoteOpen)
                            {
                                if (KickVote.IsEnabled)
                                {
                                    if (!KickVote.Kick.Contains(_cInfo.entityId))
                                    {
                                        KickVote.Kick.Add(_cInfo.entityId);
                                        GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}{1} of 8 votes to kick", Config.Chat_Response_Color, KickVote.Kick.Count), Config.Server_Response_Name, false, "ServerTools", true);
                                    }
                                    else
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You have already voted.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                }
                                if (RestartVote.IsEnabled)
                                {
                                    if (!RestartVote.Restart.Contains(_cInfo.entityId))
                                    {
                                        RestartVote.Restart.Add(_cInfo.entityId);
                                        GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}{1} of {2} votes to restart[-]", Config.Chat_Response_Color, RestartVote.Restart.Count, RestartVote.Minimum_Players / 2 + 1), Config.Server_Response_Name, false, "ServerTools", true);
                                    }
                                    else
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You have already voted.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                }
                                if (MuteVote.IsEnabled)
                                {
                                    if (!MuteVote.Mute.Contains(_cInfo.entityId))
                                    {
                                        MuteVote.Mute.Add(_cInfo.entityId);
                                        GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}{1} of 8 votes to mute", Config.Chat_Response_Color, MuteVote.Mute.Count), Config.Server_Response_Name, false, "ServerTools", true);
                                    }
                                    else
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You have already voted.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                }
                                if (NightVote.IsEnabled)
                                {
                                    if (!NightVote.Night.Contains(_cInfo.entityId))
                                    {
                                        NightVote.Night.Add(_cInfo.entityId);
                                        GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}{1} of 8 votes to skip the night", Config.Chat_Response_Color, NightVote.Night.Count), Config.Server_Response_Name, false, "ServerTools", true);
                                    }
                                    else
                                    {
                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You have already voted.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                    }
                                }
                                return false;
                            }
                        }
                        if (AuctionBox.IsEnabled && _message == "auction")
                        {
                            AuctionBox.AuctionList(_cInfo);
                            return false;
                        }
                        if (AuctionBox.IsEnabled && _message == "auction cancel")
                        {
                            AuctionBox.CancelAuction(_cInfo);
                            return false;
                        }
                        if (AuctionBox.IsEnabled && _message.StartsWith("auction buy"))
                        {
                            if (AuctionBox.No_Admins)
                            {
                                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                                if (Admin.PermissionLevel > Admin_Level)
                                {
                                    _message = _message.Replace("auction buy ", "");
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
                                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You have used an auction item # that does not exist or has sold. Type /auction.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                                
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}The auction is disabled for your tier.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                            }
                            else
                            {
                                _message = _message.Replace("auction buy ", "");
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
                                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You have used an auction item # that does not exist or has sold. Type /auction.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                        }
                                    }
                                }
                            }
                            return false;
                        }
                        if (AuctionBox.IsEnabled && _message.StartsWith("auction sell"))
                        {
                            if (AuctionBox.No_Admins)
                            {
                                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                                if (Admin.PermissionLevel > Admin_Level)
                                {
                                    _message = _message.Replace("auction sell ", "");
                                    AuctionBox.Delay(_cInfo, _message);
                                }
                                else
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}The auction is disabled for your tier.[-]", Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                            }
                            else
                            {
                                _message = _message.Replace("auction sell ", "");
                                AuctionBox.Delay(_cInfo, _message);
                            }
                            return false;
                        }
                        if (Fps.IsEnabled && _message == "fps")
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
                        if (Loc.IsEnabled && _message == "loc")
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
                        if (BikeReturn.IsEnabled && _message == "bike")
                        {
                            BikeReturn.BikeDelay(_cInfo, _playerName);
                            return false;
                        }
                        if (Report.IsEnabled && _message.StartsWith("report"))
                        {
                            _message = _message.Replace("report ", "");
                            Report.Check(_cInfo, _message);
                            return false;
                        }
                        if (Bounties.IsEnabled && _message == "bounty")
                        {
                            Bounties.BountyList(_cInfo, _playerName);
                            return false;
                        }
                        if (Bounties.IsEnabled && _message.StartsWith("bounty"))
                        {
                            _message = _message.Replace("bounty ", "");
                            Bounties.NewBounty(_cInfo, _message, _playerName);
                            return false;
                        }
                        if (Lottery.IsEnabled && _message == "lottery")
                        {
                            Lottery.Response(_cInfo);
                            return false;
                        }
                        if (Lottery.IsEnabled && _message == "lottery enter")
                        {
                            Lottery.EnterLotto(_cInfo);
                            return false;
                        }
                        if (Lottery.IsEnabled && _message.StartsWith("lottery"))
                        {
                            _message = _message.Replace("lottery ", "");
                            Lottery.NewLotto(_cInfo, _message, _playerName);
                            return false;
                        }
                        if (NewSpawnTele.IsEnabled && NewSpawnTele.Return && _message == "ready")
                        {
                            NewSpawnTele.ReturnPlayer(_cInfo);
                            return false;
                        }
                        if (LobbyChat.IsEnabled && _message == "setlobby")
                        {
                            SetLobby.Set(_cInfo);
                            return false;
                        }
                        if (LobbyChat.IsEnabled && _message == "lobby")
                        {
                            LobbyChat.Delay(_cInfo, _playerName, _announce);
                            return false;
                        }
                        if (LobbyChat.IsEnabled && LobbyChat.Return && _message == "return")
                        {
                            if (LobbyChat.LobbyPlayers.Contains(_cInfo.entityId))
                            {
                                LobbyChat.SendBack(_cInfo, _playerName);
                                return false;
                            }
                        }
                        if (PlayerList.IsEnabled && _message == "list")
                        {
                            PlayerList.Exec(_cInfo, _playerName);
                            return false;
                        }
                        if (Stuck.IsEnabled && _message == "stuck")
                        {
                            Stuck.Delay(_cInfo, _playerName, _announce);
                            return false;
                        }
                        if (_message == "pollyes")
                        {
                            if (PersistentContainer.Instance.PollOpen)
                            {
                                if (!Poll.PolledYes.Contains(_cInfo.entityId) && !Poll.PolledNo.Contains(_cInfo.entityId))
                                {
                                    Poll.VoteYes(_cInfo);
                                }
                                else
                                {
                                    string _phrase812;
                                    if (!Phrases.Dict.TryGetValue(812, out _phrase812))
                                    {
                                        _phrase812 = "{PlayerName} you have already voted on the poll";
                                    }
                                    _phrase812 = _phrase812.Replace("{PlayerName}", _playerName);
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase812), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                                return false;
                            }
                        }
                        if (_message == "pollno")
                        {
                            if (PersistentContainer.Instance.PollOpen)
                            {
                                if (!Poll.PolledYes.Contains(_cInfo.entityId) && !Poll.PolledNo.Contains(_cInfo.entityId))
                                {
                                    Poll.VoteNo(_cInfo);
                                }
                                else
                                {
                                    string _phrase812;
                                    if (!Phrases.Dict.TryGetValue(812, out _phrase812))
                                    {
                                        _phrase812 = "{PlayerName} you have already voted on the poll.";
                                    }
                                    _phrase812 = _phrase812.Replace("{PlayerName}", _playerName);
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase812), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                                return false;
                            }
                        }
                        if (_message == "poll")
                        {
                            if (PersistentContainer.Instance.PollOpen)
                            {
                                string _phrase926;
                                if (!Phrases.Dict.TryGetValue(926, out _phrase926))
                                {
                                    _phrase926 = "Poll: {Message}";
                                }
                                _phrase926 = _phrase926.Replace("{Message}", PersistentContainer.Instance.PollMessage);
                                string _phrase813;
                                if (!Phrases.Dict.TryGetValue(813, out _phrase813))
                                {
                                    _phrase813 = "Currently, the pole is yes {YesCount} / no {NoCount}.";
                                }
                                _phrase813 = _phrase813.Replace("{YesCount}", PersistentContainer.Instance.PollYes.ToString());
                                _phrase813 = _phrase813.Replace("{NoCount}", PersistentContainer.Instance.PollNo.ToString());
                                if (_announce)
                                {
                                    GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase926), Config.Server_Response_Name, false, "ServerTools", true);
                                    GameManager.Instance.GameMessageServer(null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase813), Config.Server_Response_Name, false, "ServerTools", true);
                                }
                                else
                                {
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase926), Config.Server_Response_Name, false, "ServerTools", false));
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase813), Config.Server_Response_Name, false, "ServerTools", false));
                                }
                                return false;
                            }
                        }
                        if (Bank.IsEnabled && _message == "bank")
                        {
                            Bank.Check(_cInfo);
                            return false;
                        }
                        if (Bank.IsEnabled && _message.StartsWith("deposit"))
                        {
                            _message = _message.Replace("deposit ", "");
                            Bank.Deposit(_cInfo, _message);
                            return false;
                        }
                        if (Bank.IsEnabled && _message.StartsWith("withdraw"))
                        {
                            _message = _message.Replace("withdraw ", "");
                            Bank.Withdraw(_cInfo, _message);
                            return false;
                        }
                        if (Bank.IsEnabled && _message.StartsWith("wallet deposit"))
                        {
                            _message = _message.Replace("wallet deposit ", "");
                            Bank.WalletDeposit(_cInfo, _message);
                            return false;
                        }
                        if (Bank.IsEnabled && _message.StartsWith("wallet withdraw"))
                        {
                            _message = _message.Replace("wallet withdraw ", "");
                            Bank.WalletWithdraw(_cInfo, _message);
                            return false;
                        }
                        if (Bank.IsEnabled && _message.StartsWith("transfer "))
                        {
                            _message = _message.Replace("transfer ", "");
                            Bank.Transfer(_cInfo, _message);
                            return false;
                        }
                        if (Event.Invited && _message == "event")
                        {
                            Event.AddPlayer(_cInfo);
                            return false;
                        }
                        if (CustomCommands.IsEnabled && CustomCommands.Dict.ContainsKey(_message))
                        {
                            CustomCommands.CheckCustomDelay(_cInfo, _message, _playerName, _announce);
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
    }
}