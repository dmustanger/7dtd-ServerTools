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
                string _phrase200 = "{PlayerName} you do not have permissions to use this command.";
                if (Phrases._Phrases.TryGetValue(200, out _phrase200))
                {
                    _phrase200 = _phrase200.Replace("{PlayerName}", _sender.playerName);
                }
                _sender.SendPackage(new NetPackageGameMessage(string.Format("{0}{1}[-]", CustomCommands._chatcolor, _phrase200), "Server"));
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
                string _phrase200 = "{PlayerName} you do not have permissions to use this command.";
                if (Phrases._Phrases.TryGetValue(200, out _phrase200))
                {
                    _phrase200 = _phrase200.Replace("{PlayerName}", _cInfo.playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{0}{1}[-]", CustomCommands._chatcolor, _phrase200), "Server"));
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
                string _phrase200 = "{PlayerName} you do not have permissions to use this command.";
                if (Phrases._Phrases.TryGetValue(200, out _phrase200))
                {
                    _phrase200 = _phrase200.Replace("{PlayerName}", _cInfo.playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{0}{1}[-]", CustomCommands._chatcolor, _phrase200), "Server"));
            }
            else
            {
                AdminToolsClientInfo _admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                if (_admin.PermissionLevel > PermLevelNeededforMute)
                {
                    string _phrase200 = "{PlayerName} you do not have permissions to use this command.";
                    if (Phrases._Phrases.TryGetValue(200, out _phrase200))
                    {
                        _phrase200 = _phrase200.Replace("{PlayerName}", _cInfo.playerName);
                    }
                    _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{0}{1}[-]", CustomCommands._chatcolor, _phrase200), "Server"));
                }
                else
                {
                    ClientInfo _PlayertoMute = ConsoleHelper.ParseParamIdOrName(_PlayerName);
                    if (_PlayertoMute == null)
                    {
                        string _phrase201 = "{AdminPlayerName} player {PlayerName} was not found.";
                        if (Phrases._Phrases.TryGetValue(201, out _phrase201))
                        {
                            _phrase201 = _phrase201.Replace("{AdminPlayerName}", _cInfo.playerName);
                            _phrase201 = _phrase201.Replace("{PlayerName}", _PlayerName);
                        }
                        _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase201, CustomCommands._chatcolor), "Server"));
                    }
                    else
                    {
                        if (MutedPlayersList.Contains(_PlayertoMute.playerId))
                        {
                            string _phrase202 = "{AdminPlayerName} player {MutedPlayerName} is already muted.";
                            if (Phrases._Phrases.TryGetValue(202, out _phrase202))
                            {
                                _phrase202 = _phrase202.Replace("{AdminPlayerName}", _cInfo.playerName);
                                _phrase202 = _phrase202.Replace("{MutedPlayerName}", _PlayertoMute.playerName);
                            }
                            _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase202, CustomCommands._chatcolor), "Server"));
                        }
                        else
                        {
                            MutedPlayers.Add(_PlayertoMute.playerId, DateTime.Now);
                            string _phrase203 = "{AdminPlayerName} you have muted {MutedPlayerName}.";
                            if (Phrases._Phrases.TryGetValue(203, out _phrase203))
                            {
                                _phrase203 = _phrase203.Replace("{AdminPlayerName}", _cInfo.playerName);
                                _phrase203 = _phrase203.Replace("{MutedPlayerName}", _PlayertoMute.playerName);
                            }
                            _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase203, CustomCommands._chatcolor), "Server"));
                        }
                    }     
                }
            }
        }

        public static void UnMutePlayer(ClientInfo _cInfo, string _PlayerName)
        {
            if (!GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId))
            {
                string _phrase200 = "{PlayerName} you do not have permissions to use this command.";
                if (Phrases._Phrases.TryGetValue(200, out _phrase200))
                {
                    _phrase200 = _phrase200.Replace("{PlayerName}", _cInfo.playerName);
                }
                _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{0}{1}[-]", CustomCommands._chatcolor, _phrase200), "Server"));
            }
            else
            {
                AdminToolsClientInfo _admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                if (_admin.PermissionLevel > PermLevelNeededforMute)
                {
                    string _phrase200 = "{PlayerName} you do not have permissions to use this command.";
                    if (Phrases._Phrases.TryGetValue(200, out _phrase200))
                    {
                        _phrase200 = _phrase200.Replace("{PlayerName}", _cInfo.playerName);
                    }
                    _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{0}{1}[-]", CustomCommands._chatcolor, _phrase200), "Server"));
                }
                else
                {
                    ClientInfo _PlayertoUnMute = ConsoleHelper.ParseParamIdOrName(_PlayerName);
                    if (_PlayertoUnMute == null)
                    {
                        string _phrase201 = "{AdminPlayerName} player {PlayerName} was not found.";
                        if (Phrases._Phrases.TryGetValue(201, out _phrase201))
                        {
                            _phrase201 = _phrase201.Replace("{AdminPlayerName}", _cInfo.playerName);
                            _phrase201 = _phrase201.Replace("{PlayerName}", _PlayerName);
                        }
                        _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase201, CustomCommands._chatcolor), "Server"));
                    }
                    else
                    {
                        if (!MutedPlayersList.Contains(_PlayertoUnMute.playerId))
                        {
                            string _phrase204 = "{AdminPlayerName} player {PlayerName} is not muted.";
                            if (Phrases._Phrases.TryGetValue(204, out _phrase204))
                            {
                                _phrase204 = _phrase204.Replace("{AdminPlayerName}", _cInfo.playerName);
                                _phrase204 = _phrase204.Replace("{PlayerName}", _PlayertoUnMute.playerName);
                            }
                            _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase204, CustomCommands._chatcolor), "Server"));
                        }
                        else
                        {
                            MutedPlayers.Remove(_PlayertoUnMute.playerId);
                            string _phrase205 = "{AdminPlayerName} you have unmuted {UnMutedPlayerName}.";
                            if (Phrases._Phrases.TryGetValue(205, out _phrase205))
                            {
                                _phrase205 = _phrase205.Replace("{AdminPlayerName}", _cInfo.playerName);
                                _phrase205 = _phrase205.Replace("{UnMutedPlayerName}", _PlayertoUnMute.playerName);
                            }
                            _cInfo.SendPackage(new NetPackageGameMessage(string.Format("{1}{0}[-]", _phrase205, CustomCommands._chatcolor), "Server"));
                        }
                    }
                }
            }
        }
    }
}