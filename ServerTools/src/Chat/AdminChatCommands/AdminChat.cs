using System.Collections.Generic;

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
                    _phrase107 = "{PlayerName} you do not have permissions to use this command.";
                }
                _phrase107 = _phrase107.Replace("{PlayerName}", _sender.playerName);
                _sender.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase107), Config.Server_Response_Name, false, "ServerTools", false));
            }
            else
            {
                List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                foreach (ClientInfo _cInfo in _cInfoList)
                {
                    if (GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId))
                    {
                        AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                        if (Admin.PermissionLevel <= ChatHook.Mod_Level)
                        {
                            _message = _message.Replace("@ADMINS ", "");
                            _message = _message.Replace("@admins ", "");
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _message), _sender.playerName, false, "", false));
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
                    _phrase107 = "{PlayerName} you do not have permissions to use this command.";
                }
                _phrase107 = _phrase107.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase107), Config.Server_Response_Name, false, "ServerTools", false));
            }
            else
            {
                _message = _message.Replace("@ALL ", "");
                _message = _message.Replace("@all ", "");
                SdtdConsole.Instance.ExecuteSync(string.Format("say \"{0}{1}[-]\"", Config.Chat_Response_Color, _message), (ClientInfo)null);
            }
        }
    }
}