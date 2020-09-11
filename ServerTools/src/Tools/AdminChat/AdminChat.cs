using System.Collections.Generic;

namespace ServerTools
{
    public class AdminChat
    {
        public static bool IsEnabled = false;
        public static string Command118 = "@admin";

        public static void SendAdmins(ClientInfo _sender, string _message)
        {
            List<ClientInfo> _cInfoList = PersistentOperations.ClientList();
            for (int i = 0; i < _cInfoList.Count; i++)
            {
                ClientInfo _cInfo = _cInfoList[i];
                if (_cInfo != null && GameManager.Instance.adminTools.GetUserPermissionLevel(_cInfo) <= ChatHook.Mod_Level && _sender.playerId != _cInfo.playerId)
                {
                    ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", _sender.entityId, _sender.playerName, EChatType.Whisper, null);
                }
            }
        }
    }
}