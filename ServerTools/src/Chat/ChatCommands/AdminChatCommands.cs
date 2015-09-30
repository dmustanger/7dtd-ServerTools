using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class AdminChat
    {
        public static bool IsEnabled = false;
        public static int PermLevelNeededforMute = 0;
        private static SortedDictionary<string, DateTime> MutedPlayers = new SortedDictionary<string, DateTime>();

        public static List<string> MutedPlayersList
        {
            get { return new List<string>(MutedPlayers.Keys); }
        }

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
                    if (GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId))
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(string.Format("[FF0080]{0}[-]", _message), _sender.playerName));
                    }
                }
            }
        }

        public static void SendAll(ClientInfo _cInfo, string _message)
        {
            string[] _commands = {"say"};
            if (!GameManager.Instance.adminTools.CommandAllowedFor(_commands, _cInfo.playerId))
            {
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{0}You do not have permissions to use this command.[-]", CustomCommands._chatcolor), "Server"));
            }
            else
            {
                SdtdConsole.Instance.ExecuteSync(string.Format("say \"[FF8000]{0}[-]\"", _message), _cInfo);
            }
        }

        public static void MutePlayer(ClientInfo _cInfo, string _PlayerName)
        {
            if (!GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId))
            {
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{0}You do not have permissions to use this command.[-]", CustomCommands._chatcolor), "Server"));
            }
            else
            {
                AdminToolsClientInfo _admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                if (_admin.PermissionLevel > PermLevelNeededforMute)
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{0}You do not have permissions to use this command.[-]", CustomCommands._chatcolor), "Server"));
                }
                else
                {
                    ClientInfo _PlayertoMute = ConsoleHelper.ParseParamIdOrName(_PlayerName);
                    if (_PlayertoMute == null)
                    {
                        string _reason = "Player name not found.";
                        _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
                    }
                    else
                    {
                        if (MutedPlayersList.Contains(_PlayertoMute.playerId))
                        {
                            string _reason = string.Format("Player {0} is already muted.", _PlayerName);
                            _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
                        }
                        else
                        {
                            MutedPlayers.Add(_PlayertoMute.playerId, DateTime.Now);
                            string _reason = string.Format("You have muted {0}.", _PlayerName);
                            _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
                        }
                    }     
                }
            }
        }

        public static void UnMutePlayer(ClientInfo _cInfo, string _PlayerName)
        {
            if (!GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId))
            {
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{0}You do not have permissions to use this command.[-]", CustomCommands._chatcolor), "Server"));
            }
            else
            {
                AdminToolsClientInfo _admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                if (_admin.PermissionLevel > PermLevelNeededforMute)
                {
                    _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{0}You do not have permissions to use this command.[-]", CustomCommands._chatcolor), "Server"));
                }
                else
                {
                    ClientInfo _PlayertoUnMute = ConsoleHelper.ParseParamIdOrName(_PlayerName);
                    if (_PlayertoUnMute == null)
                    {
                        string _reason = "Player name not found.";
                        _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
                    }
                    else
                    {
                        if (!MutedPlayersList.Contains(_PlayertoUnMute.playerId))
                        {
                            string _reason = "Player is not muted.";
                            _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
                        }
                        else
                        {
                            MutedPlayers.Remove(_PlayertoUnMute.playerId);
                            string _reason = string.Format("You have unmuted {0}.", _PlayerName);
                            _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _reason, CustomCommands._chatcolor), "Server"));
                        }
                    }
                }
            }
        }
    }
}