using System.Collections.Generic;

namespace ServerTools
{
    class AdminList
    {
        public static bool IsEnabled = false;
        public static int Admin_Level = 0, Mod_Level = 1;
        public static string Command_adminlist = "adminlist";

        public static void List(ClientInfo _cInfo)
        {
            List<string> admins = new List<string>();
            List<string> mods = new List<string>();
            List<ClientInfo> clientList = GeneralOperations.ClientList();
            if (clientList != null)
            {
                ClientInfo cInfoAdmin;
                for (int i = 0; i < clientList.Count; i++)
                {
                    cInfoAdmin = clientList[i];
                    int userPlatformID = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfoAdmin.PlatformId);
                    int userCrossplatformID = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(cInfoAdmin.CrossplatformId);
                    if (userPlatformID <= Admin_Level ||
                        userCrossplatformID <= Admin_Level)
                    {
                        admins.Add(cInfoAdmin.playerName);
                    }
                    if ((userPlatformID > Admin_Level &&
                        userCrossplatformID > Admin_Level) &&
                        userPlatformID <= Mod_Level ||
                        userCrossplatformID <= Mod_Level)
                    {
                        mods.Add(cInfoAdmin.playerName);
                    }
                }
            }
            Response(_cInfo, admins, mods);
        }

        public static void Response(ClientInfo _cInfo, List<string> admins, List<string> mods)
        {
            if (admins.Count == 0 && mods.Count == 0)
            {
                Phrases.Dict.TryGetValue("AdminList3", out string phrase);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            else
            { 
                if (admins.Count > 0)
                {
                    string adminList = string.Join(", ", admins.ToArray());
                    Phrases.Dict.TryGetValue("AdminList1", out string phrase1);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + adminList + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
                if (mods.Count > 0)
                {
                    string modList = string.Join(", ", mods.ToArray());
                    Phrases.Dict.TryGetValue("AdminList2", out string phrase2);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase2 + modList + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
        }
    }
}
