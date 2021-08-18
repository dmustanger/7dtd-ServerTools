using System.Collections.Generic;

namespace ServerTools
{
    class AdminList
    {
        public static bool IsEnabled = false;
        public static int Admin_Level = 0, Mod_Level = 1;
        public static string Command_adminlist = "adminlist";
        private static List<string> Admins = new List<string>();
        private static List<string> Mods = new List<string>();

        public static void List(ClientInfo _cInfo)
        {
            Admins.Clear();
            Mods.Clear();
            List<ClientInfo> ClientInfoList = PersistentOperations.ClientList();
            for (int i = 0; i < ClientInfoList.Count; i++)
            {
                ClientInfo _cInfoAdmins = ClientInfoList[i];
                if (GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo) <= Admin_Level)
                {
                    Admins.Add(_cInfoAdmins.playerName);
                }
                if (GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo) > Admin_Level && GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo) <= Mod_Level)
                {
                    Mods.Add(_cInfoAdmins.playerName);
                }
            }
            Response(_cInfo);
        }

        public static void Response(ClientInfo _cInfo)
        {
            string _adminList = string.Join(", ", Admins.ToArray());
            string _modList = string.Join(", ", Mods.ToArray());
            Phrases.Dict.TryGetValue("AdminList1", out string _phrase1);
            Phrases.Dict.TryGetValue("AdminList2", out string _phrase2);
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase1 + _adminList + ".[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase2 + _modList + ".[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
        }
    }
}
