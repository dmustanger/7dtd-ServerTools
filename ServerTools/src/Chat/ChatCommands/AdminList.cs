using System.Collections.Generic;

namespace ServerTools
{
    class AdminList
    {
        public static bool IsEnabled = false;
        public static int Admin_Level = 0, Mod_Level = 1;
        private static List<string> Admins = new List<string>();
        private static List<string> Mods = new List<string>();

        public static void List(ClientInfo _cInfo, bool _announce, string _playerName)
        {
            Admins.Clear();
            Mods.Clear();
            List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
            for (int i = 0; i < _cInfoList.Count; i++)
            {
                ClientInfo _cInfoAdmins = _cInfoList[i];
                if (!AdminChatColorConsole.AdminColorOff.Contains(_cInfoAdmins.playerId))
                {
                    GameManager.Instance.adminTools.IsAdmin(_cInfoAdmins.playerId);
                    AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfoAdmins.playerId);
                    if (Admin.PermissionLevel <= Admin_Level)
                    {
                        Admins.Add(_cInfoAdmins.playerName);
                    }
                    if (Admin.PermissionLevel > Admin_Level & Admin.PermissionLevel <= Mod_Level)
                    {
                        Mods.Add(_cInfoAdmins.playerName);
                    }
                }
            }
            Response(_cInfo, _announce, _playerName);
        }

        public static void Response(ClientInfo _cInfo, bool _announce, string _playerName)
        {
            string _adminList = string.Join(", ", Admins.ToArray());
            string _modList = string.Join(", ", Mods.ToArray());
            if (_announce)
            {
                string _phrase725;
                if (!Phrases.Dict.TryGetValue(725, out _phrase725))
                {
                    _phrase725 = "Server admins in game: [FF8000]";
                }
                string _phrase726;
                if (!Phrases.Dict.TryGetValue(726, out _phrase726))
                {
                    _phrase726 = "Server moderators in game: [FF8000]";
                }
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}{2}.[-]", Config.Chat_Response_Color, _phrase725, _adminList), Config.Server_Response_Name, false, "", false);
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}{2}.[-]", Config.Chat_Response_Color, _phrase726, _modList), Config.Server_Response_Name, false, "", false);
            }
            else
            {
                string _phrase725;
                if (!Phrases.Dict.TryGetValue(725, out _phrase725))
                {
                    _phrase725 = "Server admins in game: [FF8000]";
                }
                string _phrase726;
                if (!Phrases.Dict.TryGetValue(726, out _phrase726))
                {
                    _phrase726 = "Server moderators in game: [FF8000]";
                }
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}{2}.[-]", Config.Chat_Response_Color, _phrase725, _adminList), Config.Server_Response_Name, false, "ServerTools", false));
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}{2}.[-]", Config.Chat_Response_Color, _phrase726, _modList), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }
    }
}
