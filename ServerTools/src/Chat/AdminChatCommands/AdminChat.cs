using System.Collections.Generic;
using System.Linq;

namespace ServerTools
{
    public class AdminChat
    {
        public static bool IsEnabled = false;

        public static void SendAdmins(ClientInfo _sender, string _message)
        {
            if (!GameManager.Instance.adminTools.IsAdmin(_sender.playerId))
            {
                string _phrase107;
                if (!Phrases.Dict.TryGetValue(107, out _phrase107))
                {
                    _phrase107 = "you do not have permissions to use this command.";
                }
                ChatHook.ChatMessage(_sender, LoadConfig.Chat_Response_Color + _sender.playerName + ", " + _phrase107 + "[-]", _sender.entityId, _sender.playerName, EChatType.Whisper);
            }
            else
            {
                List<ClientInfo> _cInfoList = ConnectionManager.Instance.Clients.List.ToList();
                foreach (ClientInfo _cInfo in _cInfoList)
                {
                    if (GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId))
                    {
                        AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                        if (Admin.PermissionLevel <= ChatHook.Mod_Level)
                        {
                            _message = _message.Replace("@ADMINS ", "");
                            _message = _message.Replace("@admins ", "");
                            ChatHook.ChatMessage(_sender, LoadConfig.Chat_Response_Color + _sender.playerName + ": " + _message + "[-]", _sender.entityId, _sender.playerName, EChatType.Whisper);
                        }
                    }
                }
            }
        }

        public static void SendAll(ClientInfo _cInfo, string _message)
        {
            string[] _commands = { "say" };
            if (!GameManager.Instance.adminTools.CommandAllowedFor(_commands, _cInfo.playerId))
            {
                string _phrase107;
                if (!Phrases.Dict.TryGetValue(107, out _phrase107))
                {
                    _phrase107 = "you do not have permissions to use this command.";
                }
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _cInfo.playerName + ", " + _phrase107 + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Whisper);
            }
            else
            {
                _message = _message.Replace("@ALL ", "");
                _message = _message.Replace("@all ", "");
                ChatHook.ChatMessage(_cInfo, LoadConfig.Chat_Response_Color + _message + "[-]", _cInfo.entityId, LoadConfig.Server_Response_Name, EChatType.Global);
            }
        }
    }
}