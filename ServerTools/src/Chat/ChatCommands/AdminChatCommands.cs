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
                _sender.SendPackage(new NetPackageGameMessage(string.Format("{0}You do not have permissions to use this command.[-]", CustomCommands._chatcolor), "Server"));
            }
            else
            {
                List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                foreach (ClientInfo _cInfo in _cInfoList)
                {
                    if (GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId) && _sender.playerId != _cInfo.playerId)
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(string.Format("[FF0080]{0}[-]", _message), _sender.playerName));
                    }
                }
            }
        }

        public static void SendAll(ClientInfo _sender, string _message)
        {
            if (!GameManager.Instance.adminTools.IsAdmin(_sender.playerId))
            {
                _sender.SendPackage(new NetPackageGameMessage(string.Format("{0}You do not have permissions to use this command.[-]", CustomCommands._chatcolor), "Server"));
            }
            else
            {
                SdtdConsole.Instance.ExecuteSync(string.Format("say \"[FF8000]{0}[-]\"", _message), _sender);
            }
        }
    }
}