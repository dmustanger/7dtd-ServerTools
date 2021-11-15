using System.Collections.Generic;

namespace ServerTools
{
    public class AdminChat
    {
        public static bool IsEnabled = false;
        public static string Command_admin = "@admin";

        public static void SendAdmins(ClientInfo _sender, string _message)
        {
            List<ClientInfo> clientList = PersistentOperations.ClientList();
            if (clientList != null)
            {
                for (int i = 0; i < clientList.Count; i++)
                {
                    ClientInfo cInfo = clientList[i];
                    if (cInfo != null && GameManager.Instance.adminTools.GetUserPermissionLevel(cInfo) <= ChatHook.Mod_Level && _sender.playerId != cInfo.playerId)
                    {
                        ChatHook.ChatMessage(cInfo, Config.Chat_Response_Color + _message + "[-]", _sender.entityId, _sender.playerName, EChatType.Whisper, null);
                    }
                }
            }
        }
    }
}