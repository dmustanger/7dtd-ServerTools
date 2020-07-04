using System.Collections.Generic;
using System.Linq;

namespace ServerTools
{
    class AdminList
    {
        public static bool IsEnabled = false;
        public static int Admin_Level = 0, Mod_Level = 1;
        public static string Command48 = "adminlist";
        private static List<string> Admins = new List<string>();
        private static List<string> Mods = new List<string>();

        public static void List(ClientInfo _cInfo, string _playerName)
        {
            Admins.Clear();
            Mods.Clear();
            List<ClientInfo> ClientInfoList = PersistentOperations.ClientList();
            for (int i = 0; i < ClientInfoList.Count; i++)
            {
                ClientInfo _cInfoAdmins = ClientInfoList[i];
                GameManager.Instance.adminTools.IsAdmin(_cInfoAdmins);
                GameManager.Instance.adminTools.GetAdmins().TryGetValue(_cInfo.playerId, out AdminToolsClientInfo Admin);
                if (Admin.PermissionLevel <= Admin_Level)
                {
                    Admins.Add(_cInfoAdmins.playerName);
                }
                if (Admin.PermissionLevel > Admin_Level & Admin.PermissionLevel <= Mod_Level)
                {
                    Mods.Add(_cInfoAdmins.playerName);
                }
            }
            Response(_cInfo, _playerName);
        }

        public static void Response(ClientInfo _cInfo, string _playerName)
        {
            string _adminList = string.Join(", ", Admins.ToArray());
            string _modList = string.Join(", ", Mods.ToArray());
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
            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase725 + _adminList + ".[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _phrase726 + _modList + ".[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper, null);
        }
    }
}
