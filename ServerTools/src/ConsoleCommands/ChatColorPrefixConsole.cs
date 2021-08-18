using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class ChatColorPrefixConsole : ConsoleCmdAbstract
    {

        public override string GetDescription()
        {
            return "[ServerTools] - Enable, add, delete a player from the chat color prefix list.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. st-ccp off\n" +
                "  2. st-ccp on\n" +
                "  3. st-ccp add <steamId/entityId/playerName> <group> <prefix> <nameColor> <prefixColor> <daysToExpire>\n" +
                "  4. st-ccp add <steamId/entityId/playerName> <group> <daysToExpire>\n" +
                "  5. st-ccp add <steamId/entityId/playerName> <group>\n" +
                "  6. st-ccp edit <steamId> <prefix> <nameColor> <prefixColor>\n" +
                "  7. st-ccp edit <steamId> <daysToExpire>\n" +
                "  8. st-ccp remove <steamId/group>\n" +
                "  9. st-ccp list\n" +
                "1. Turn off chat color prefix\n" +
                "2. Turn on chat color prefix\n" +
                "3. Adds a player to a new group with custom prefix and colors on to the list\n" +
                "4. Adds a player to an existing group on the list with the group's prefix and colors, but a custom expiry date\n" +
                "5. Adds a player to an existing group on the list with the group's prefix, colors and expiry date\n" +
                "6. Edits a specific player on the list to the prefix, name color and prefix color given\n" +
                "7. Edits a specific player on the list to the expiry date given\n" +
                "8. Removes a player or entire group from the chat color prefix list\n" +
                "9. Lists all players in the chat color prefix list\n" +
                "Note using the entity id or player name to add a player will only work if they are online. Use their steam id when offline" +
                "Note if you want a blank prefix or color enter ** without brackets" +
                "Note the colors must be entered as 6 digit HTML color codes. Example [FF0000] or [FFFF00]";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-ChatColorPrefix", "ccp", "st-ccp" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 || (_params.Count > 5 && _params.Count != 7))
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1 to 5, or 7, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (ChatColor.IsEnabled)
                    {
                        ChatColor.IsEnabled = false;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Chat color prefix has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Chat color prefix is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!ChatColor.IsEnabled)
                    {
                        ChatColor.IsEnabled = true;
                        Config.WriteXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Chat color prefix has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Chat color prefix is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("add"))
                {
                    ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                    if (_cInfo == null)
                    {
                        if (int.TryParse(_params[1], out int _entId))
                        {
                            _cInfo = PersistentOperations.GetClientInfoFromEntityId(_entId);
                            if (_cInfo == null)
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not find player data attached to {0}", _params[1]));
                                return;
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not find player data attached to {0}", _params[1]));
                            return;
                        }
                    }
                    if (_params.Count == 7)
                    {
                        foreach (var group in ChatColor.Players)
                        {
                            if (group.Value[1] == _params[2])
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not add id {0} to the chat color prefix list. Group {1} already exists on the list. Create a new entry or add them to this group", _cInfo.playerId, _params[2]));
                                return;
                            }
                        }
                        if (!double.TryParse(_params[6], out double _daysToExpire))
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid days to expire: {0}", _params[6]));
                            return;
                        }
                        DateTime _expiryDate;
                        if (_daysToExpire > 0d)
                        {
                            _expiryDate = DateTime.Now.AddDays(_daysToExpire);
                        }
                        else
                        {
                            _expiryDate = DateTime.Now.AddDays(18250d);
                        }
                        if ((!_params[4].Contains("[") || !_params[4].Contains("]")) && _params[4] != "**")
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not add id {0} to the chat color prefix list. Name color must be in HTML format, example [FFFFFF] with the brackets included. Found: {1}", _params[1], _params[4]));
                            return;
                        }
                        if ((!_params[5].Contains("[") || !_params[5].Contains("]")) && _params[5] != "**")
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not add id {0} to the chat color prefix list. Prefix color must be in HTML format, example [FFFFFF] with the brackets included. Found: {1}", _params[1], _params[5]));
                            return;
                        }
                        string[] _c = new string[] { _cInfo.playerName, _params[2], _params[3], _params[4], _params[5] };
                        if (ChatColor.Players.ContainsKey(_cInfo.playerId))
                        {
                            ChatColor.Players[_cInfo.playerId] = _c;
                            ChatColor.ExpireDate[_cInfo.playerId] = _expiryDate;
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Updated id {0} with the name of {1} to the group {2} using prefix {3}, name color {4} and prefix color {5} that expires at {6} from the chat color prefix list.", _cInfo.playerId, _cInfo.playerName, _params[2], _params[3], _params[4], _params[5], _expiryDate.ToString()));
                        }
                        else
                        {
                            ChatColor.Players.Add(_cInfo.playerId, _c);
                            ChatColor.ExpireDate.Add(_cInfo.playerId, _expiryDate);
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added id {0} with the name of {1} to the group {2} using prefix {3}, name color {4} and prefix color {5} that expires at {6} from the chat color prefix list.", _cInfo.playerId, _cInfo.playerName, _params[2], _params[3], _params[4], _params[5], _expiryDate.ToString()));
                        }
                        ChatColor.UpdateXml();
                        return;
                    }
                    else if (_params.Count == 4)
                    {
                        foreach (var group in ChatColor.Players)
                        {
                            if (group.Value[1] == _params[2])
                            {
                                if (!double.TryParse(_params[3], out double _daysToExpire))
                                {
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not add id {0} to the chat color prefix list. Invalid days to expire: {1}", _cInfo.playerId, _params[3]));
                                    return;
                                }
                                DateTime _expiryDate;
                                if (_daysToExpire > 0d)
                                {
                                    _expiryDate = DateTime.Now.AddDays(_daysToExpire);
                                }
                                else
                                {
                                    _expiryDate = DateTime.Now.AddDays(18250d);
                                }
                                string[] _c = new string[] { _cInfo.playerName, group.Value[1], group.Value[2], group.Value[3], group.Value[4] };
                                if (ChatColor.Players.ContainsKey(_cInfo.playerId))
                                {
                                    ChatColor.Players[_cInfo.playerId] = _c;
                                    ChatColor.ExpireDate[_cInfo.playerId] = _expiryDate;
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Updated id {0} with the name of {1} to the group {2} using prefix {3}, name color {4} and prefix color {5} that expires at {6} from the chat color prefix list.", _cInfo.playerId, _cInfo.playerName, group.Value[1], group.Value[2], group.Value[3], group.Value[4], _expiryDate.ToString()));
                                }
                                else
                                {
                                    ChatColor.Players.Add(_cInfo.playerId, _c);
                                    ChatColor.ExpireDate.Add(_cInfo.playerId, _expiryDate);
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added id {0} with the name of {1} to the group {2} using prefix {3}, name color {4} and prefix color {5} that expires at {6} from the chat color prefix list.", _cInfo.playerId, _cInfo.playerName, group.Value[1], group.Value[2], group.Value[3], group.Value[4], _expiryDate.ToString()));
                                }
                                ChatColor.UpdateXml();
                                return;
                            }
                        }
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] No group with the name {0} was found.", _params[2]));
                        return;
                    }
                    else if (_params.Count == 3)
                    {
                        foreach (var group in ChatColor.Players)
                        {
                            if (group.Value[1] == _params[2])
                            {
                                string[] _c = new string[] { _cInfo.playerName, group.Value[1], group.Value[2], group.Value[3], group.Value[4] };
                                ChatColor.ExpireDate.TryGetValue(group.Key, out DateTime _expiryDate);
                                if (ChatColor.Players.ContainsKey(_cInfo.playerId))
                                {
                                    ChatColor.Players[_cInfo.playerId] = _c;
                                    ChatColor.ExpireDate[_cInfo.playerId] = _expiryDate;
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Updated id {0} with the name of {1} to the group {2} using prefix {3}, name color {4} and prefix color {5} that expires at {6} from the chat color prefix list.", _cInfo.playerId, _cInfo.playerName, group.Value[1], group.Value[2], group.Value[3], group.Value[4], _expiryDate.ToString()));
                                }
                                else
                                {
                                    ChatColor.Players.Add(_cInfo.playerId, _c);
                                    ChatColor.ExpireDate.Add(_cInfo.playerId, _expiryDate);
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added id {0} with the name of {1} to the group {2} using prefix {3}, name color {4} and prefix color {5} that expires at {6} from the chat color prefix list.", _cInfo.playerId, _cInfo.playerName, group.Value[1], group.Value[2], group.Value[3], group.Value[4], _expiryDate.ToString()));
                                }
                                ChatColor.UpdateXml();
                                return;
                            }
                        }
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] No group with the name {0} was found.", _params[3]));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("edit"))
                {
                    if (_params.Count != 3 && _params.Count != 5)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 3 or 5, found {0}", _params.Count));
                        return;
                    }
                    if (_params[1].Length != 17)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid steam id {0}", _params[1]));
                        return;
                    }
                    if (ChatColor.Players.ContainsKey(_params[1]))
                    {
                        if (_params.Count == 5)
                        {
                            ChatColor.Players.TryGetValue(_params[1], out string[] _ccp);
                            _ccp[2] = _params[2];
                            _ccp[3] = _params[3];
                            _ccp[4] = _params[4];
                            ChatColor.Players[_params[1]] = _ccp;
                            ChatColor.ExpireDate.TryGetValue(_params[1], out DateTime _expiry);
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Edited id {0} with the name of {1} to the group {2} using prefix {3}, name color {4} and prefix color {5} that expires at {6} from the chat color prefix list.", _params[1], _ccp[0], _ccp[1], _ccp[2], _ccp[3], _ccp[4], _expiry.ToString()));
                        }
                        else
                        {
                            if (!double.TryParse(_params[2], out double _daysToExpire))
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid days to expire: {0}", _params[2]));
                                return;
                            }
                            DateTime _expiryDate;
                            if (_daysToExpire > 0d)
                            {
                                _expiryDate = DateTime.Now.AddDays(_daysToExpire);
                            }
                            else
                            {
                                _expiryDate = DateTime.Now.AddDays(18250d);
                            }
                            ChatColor.ExpireDate[_params[1]] = _expiryDate;
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Edited id {0} to expire at {1}", _params[1], _params[2]));
                        }
                        ChatColor.UpdateXml();
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Player {0} was not found on the chat color prefix list. Unable to edit", _params[1]));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found {0}.", _params.Count));
                        return;
                    }
                    foreach (var group in ChatColor.Players)
                    {
                        if (group.Value[1] == _params[1])
                        {
                            ChatColor.Players.Remove(group.Key);
                            ChatColor.ExpireDate.Remove(group.Key);
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed {0} named {1} with group {2} from the chat color prefix list.", group.Key, group.Value[0], _params[1]));
                        }
                        if (group.Key == _params[1])
                        {
                            ChatColor.Players.Remove(group.Key);
                            ChatColor.ExpireDate.Remove(group.Key);
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed {0} named {1} with group {2} from the chat color prefix list.", _params[1], group.Value[0], group.Value[1]));
                        }
                        if (group.Value[0] == _params[1])
                        {
                            ChatColor.Players.Remove(group.Key);
                            ChatColor.ExpireDate.Remove(group.Key);
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed {0} named {1} with group {2} from the chat color prefix list.", group.Key, _params[1], group.Value[1]));
                        }
                        ChatColor.UpdateXml();
                    }
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Completed removing id and groups matching {0} from the chat color prefix list.", _params[1]));
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    foreach (var group in ChatColor.Players)
                    {
                        if (group.Value[2] == "")
                        {
                            group.Value[2] = "**";
                        }
                        if (group.Value[3] == "")
                        {
                            group.Value[3] = "**";
                        }
                        ChatColor.ExpireDate.TryGetValue(group.Key, out DateTime _expiry);
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Id {0} named {1} with group {2} prefix {3} color {4} expires {5}.", group.Key, group.Value[0], group.Value[1], group.Value[2], group.Value[3], _expiry));
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ChatColorPrefixConsole.Execute: {0}", e.Message));
            }
        }
    }
}