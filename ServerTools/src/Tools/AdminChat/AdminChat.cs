using System.Collections.Generic;

namespace ServerTools
{
    public class AdminChat
    {
        public static bool IsEnabled = false;
        public static string Command118 = "admin";

        public static void SendAdmins(ClientInfo _sender, string _message)
        {
            if (GameManager.Instance.adminTools.CommandAllowedFor(new string[] { _message }, _sender.playerId) || GameManager.Instance.adminTools.GetAdminToolsClientInfo(_sender.playerId).PermissionLevel <= ChatHook.Mod_Level)
            {
                List<ClientInfo> _cInfoList = PersistentOperations.ClientList();
                for (int i = 0; i < _cInfoList.Count; i++)
                {
                    ClientInfo _cInfo = _cInfoList[i];
                    if (_cInfo != null)
                    {
                        if (GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId).PermissionLevel <= ChatHook.Mod_Level)
                        {
                            _message = _message.Replace("@" + Command118 + " ", "");
                            ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", _sender.entityId, _sender.playerName, EChatType.Whisper, null);
                        }
                    }
                }
            }
        }
    }
}