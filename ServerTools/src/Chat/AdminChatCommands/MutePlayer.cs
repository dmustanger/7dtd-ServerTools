using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class MutePlayer
    {
        public static int PermLevelNeededforMute = 0;
        public static SortedDictionary<string, DateTime> Dict = new SortedDictionary<string, DateTime>();

        public static void Add(ClientInfo _cInfo, string _playerName)
        {
            if (!GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId))
            {
                string _phrase200 = "{PlayerName} you do not have permissions to use this command.";
                if (!Phrases.Dict.TryGetValue(200, out _phrase200))
                {
                    Log.Out("[SERVERTOOLS] Phrase 200 not found using default.");
                }
                _phrase200 = _phrase200.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _phrase200), "Server", false, "", false));
            }
            else
            {
                AdminToolsClientInfo _admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                if (_admin.PermissionLevel > PermLevelNeededforMute)
                {
                    string _phrase200 = "{PlayerName} you do not have permissions to use this command.";
                    if (!Phrases.Dict.TryGetValue(200, out _phrase200))
                    {
                        Log.Out("[SERVERTOOLS] Phrase 200 not found using default.");
                    }
                    _phrase200 = _phrase200.Replace("{PlayerName}", _cInfo.playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _phrase200), "Server", false, "", false));
                }
                else
                {
                    _playerName = _playerName.Replace("mute ", "");
                    ClientInfo _PlayertoMute = ConsoleHelper.ParseParamIdOrName(_playerName);
                    if (_PlayertoMute == null)
                    {
                        string _phrase201 = "{AdminPlayerName} player {PlayerName} was not found.";
                        if (!Phrases.Dict.TryGetValue(201, out _phrase201))
                        {
                            Log.Out("[SERVERTOOLS] Phrase 201 not found using default.");
                        }
                        _phrase201 = _phrase201.Replace("{AdminPlayerName}", _cInfo.playerName);
                        _phrase201 = _phrase201.Replace("{PlayerName}", _playerName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase201, CustomCommands.ChatColor), "Server", false, "", false));
                    }
                    else
                    {
                        if (Dict.ContainsKey(_PlayertoMute.playerId))
                        {
                            string _phrase202 = "{AdminPlayerName} player {MutedPlayerName} is already muted.";
                            if (!Phrases.Dict.TryGetValue(202, out _phrase202))
                            {
                                Log.Out("[SERVERTOOLS] Phrase 202 not found using default.");
                            }
                            _phrase202 = _phrase202.Replace("{AdminPlayerName}", _cInfo.playerName);
                            _phrase202 = _phrase202.Replace("{MutedPlayerName}", _PlayertoMute.playerName);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase202, CustomCommands.ChatColor), "Server", false, "", false));
                        }
                        else
                        {
                            Dict.Add(_PlayertoMute.playerId, DateTime.Now);
                            string _phrase203 = "{AdminPlayerName} you have muted {MutedPlayerName}.";
                            if (!Phrases.Dict.TryGetValue(203, out _phrase203))
                            {
                                Log.Out("[SERVERTOOLS] Phrase 203 not found using default.");
                            }
                            _phrase203 = _phrase203.Replace("{AdminPlayerName}", _cInfo.playerName);
                            _phrase203 = _phrase203.Replace("{MutedPlayerName}", _PlayertoMute.playerName);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase203, CustomCommands.ChatColor), "Server", false, "", false));
                        }
                    }
                }
            }
        }

        public static void Remove(ClientInfo _cInfo, string _playerName)
        {
            if (!GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId))
            {
                string _phrase200 = "{PlayerName} you do not have permissions to use this command.";
                if (!Phrases.Dict.TryGetValue(200, out _phrase200))
                {
                    Log.Out("[SERVERTOOLS] Phrase 200 not found using default.");
                }
                _phrase200 = _phrase200.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _phrase200), "Server", false, "", false));
            }
            else
            {
                AdminToolsClientInfo _admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                if (_admin.PermissionLevel > PermLevelNeededforMute)
                {
                    string _phrase200 = "{PlayerName} you do not have permissions to use this command.";
                    if (!Phrases.Dict.TryGetValue(200, out _phrase200))
                    {
                        Log.Out("[SERVERTOOLS] Phrase 200 not found using default.");
                    }
                    _phrase200 = _phrase200.Replace("{PlayerName}", _cInfo.playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", CustomCommands.ChatColor, _phrase200), "Server", false, "", false));
                }
                else
                {
                    _playerName = _playerName.Replace("unmute ", "");
                    ClientInfo _PlayertoUnMute = ConsoleHelper.ParseParamIdOrName(_playerName);
                    if (_PlayertoUnMute == null)
                    {
                        string _phrase201 = "{AdminPlayerName} player {PlayerName} was not found.";
                        if (!Phrases.Dict.TryGetValue(201, out _phrase201))
                        {
                            Log.Out("[SERVERTOOLS] Phrase 201 not found using default.");
                        }
                        _phrase201 = _phrase201.Replace("{AdminPlayerName}", _cInfo.playerName);
                        _phrase201 = _phrase201.Replace("{PlayerName}", _playerName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase201, CustomCommands.ChatColor), "Server", false, "", false));
                    }
                    else
                    {
                        if (!Dict.ContainsKey(_PlayertoUnMute.playerId))
                        {
                            string _phrase204 = "{AdminPlayerName} player {PlayerName} is not muted.";
                            if (!Phrases.Dict.TryGetValue(204, out _phrase204))
                            {
                                Log.Out("[SERVERTOOLS] Phrase 204 not found using default.");
                            }
                            _phrase204 = _phrase204.Replace("{AdminPlayerName}", _cInfo.playerName);
                            _phrase204 = _phrase204.Replace("{PlayerName}", _PlayertoUnMute.playerName);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase204, CustomCommands.ChatColor), "Server", false, "", false));
                        }
                        else
                        {
                            Dict.Remove(_PlayertoUnMute.playerId);
                            string _phrase205 = "{AdminPlayerName} you have unmuted {UnMutedPlayerName}.";
                            if (!Phrases.Dict.TryGetValue(205, out _phrase205))
                            {
                                Log.Out("[SERVERTOOLS] Phrase 205 not found using default.");
                            }
                            _phrase205 = _phrase205.Replace("{AdminPlayerName}", _cInfo.playerName);
                            _phrase205 = _phrase205.Replace("{UnMutedPlayerName}", _PlayertoUnMute.playerName);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase205, CustomCommands.ChatColor), "Server", false, "", false));
                        }
                    }
                }
            }
        }
    }
}