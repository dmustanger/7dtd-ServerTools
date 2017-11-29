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
                string _phrase200;
                if (!Phrases.Dict.TryGetValue(200, out _phrase200))
                {
                    _phrase200 = "{PlayerName} you do not have permissions to use this command.";
                }
                _phrase200 = _phrase200.Replace("{PlayerName}", _sender.playerName);
                _sender.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _phrase200), "Server", false, "", false));
            }
            else
            {
                List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                foreach (ClientInfo _cInfo in _cInfoList)
                {
                    if (GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId))
                    {
                        AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                        if (Admin.PermissionLevel <= ChatHook.ModLevel)
                        {
                            _message = _message.Replace("@ADMINS ", "");
                            _message = _message.Replace("@admins ", "");
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("[FF0080]{0}[-]", _message), _sender.playerName, false, "", false));
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
                string _phrase200;
                if (!Phrases.Dict.TryGetValue(200, out _phrase200))
                {
                    _phrase200 = "{PlayerName} you do not have permissions to use this command.";
                }
                _phrase200 = _phrase200.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _phrase200), "Server", false, "", false));
            }
            else
            {
                _message = _message.Replace("@ALL ", "");
                _message = _message.Replace("@all ", "");
                SdtdConsole.Instance.ExecuteSync(string.Format("say \"[FF8000]{0}[-]\"", _message), _cInfo);
            }
        }
    }
}