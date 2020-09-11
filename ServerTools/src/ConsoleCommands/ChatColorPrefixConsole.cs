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
                "  1. ccp off\n" +
                "  2. ccp on\n" +
                "  3. ccp add <steamId/entityId/playerName> <name> <group> <prefix> <nameColor> <prefixColor> <daysToExpire>\n" +
                "  4. ccp add <steamId/entityId/playerName> <name> <group> <daysToExpire>\n" +
                "  5. ccp add <steamId/entityId/playerName> <name> <group>\n" +
                "  6. ccp remove <steamId/playerName/group>\n" +
                "  7. ccp list\n" +
                "1. Turn off chat color prefix\n" +
                "2. Turn on chat color prefix\n" +
                "3. Adds a steam Id to a new group with custom prefix and colors on the chat color prefix list\n" +
                "4. Adds a steam Id to an existing group on the list with the group's prefix and colors, but a custom expiry date\n" +
                "5. Adds a steam Id to an existing group on the list with the group's prefix, colors and expiry date\n" +
                "6. Removes a steam Id or entire group from the chat color prefix list\n" +
                "7. Lists all steam Id in the chat color prefix list\n" +
                "Note if you want a blank prefix or color enter **" +
                "Note the color must be entered as a 6 digit HTML color code. Example [FF0000]";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-ChatColorPrefix", "ccp", "st-ccp" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1 && _params.Count != 2 && _params.Count != 4 && _params.Count != 5 && _params.Count != 8)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, 2, 4, 5 or 8, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (ChatColorPrefix.IsEnabled)
                    {
                        ChatColorPrefix.IsEnabled = false;
                        LoadConfig.WriteXml();
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
                    if (!ChatColorPrefix.IsEnabled)
                    {
                        ChatColorPrefix.IsEnabled = true;
                        LoadConfig.WriteXml();
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
                    if (_params.Count != 4 && _params.Count != 5 && _params.Count != 8)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 4, 5 or 8, found {0}", _params.Count));
                        return;
                    }
                    ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                    if (_cInfo == null)
                    {
                        if (_params[1].Length == 17)
                        {
                            _cInfo = PersistentOperations.GetClientInfoFromSteamId(_params[1]);
                            if (_cInfo == null)
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not find player data attached to {0}", _params[1]));
                                return;
                            }
                        }
                        else
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
                    }
                    if (_params.Count == 8)
                    {
                        foreach (var group in ChatColorPrefix.Dict)
                        {
                            if (group.Value[1] == _params[3])
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not add id {0} to the chat color prefix list. Group {1} already exists on the list. Create a new entry or add them to this group", _cInfo.playerId, _params[3]));
                                return;
                            }
                        }
                        double _daysToExpire;
                        if (!double.TryParse(_params[7], out _daysToExpire))
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid days to expire: {0}", _params[7]));
                            return;
                        }
                        DateTime _expireDate;
                        if (_daysToExpire > 0d)
                        {
                            _expireDate = DateTime.Now.AddDays(_daysToExpire);
                        }
                        else
                        {
                            _expireDate = DateTime.Now.AddDays(18250d);
                        }
                        if ((!_params[5].Contains("[") || !_params[5].Contains("]")) && _params[5] != "**")
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not add id {0} to the chat color prefix list. Name color must be in HTML format, example [FFFFFF] with the brackets included. Found: {1}", _params[1], _params[5]));
                            return;
                        }
                        if ((!_params[6].Contains("[") || !_params[6].Contains("]")) && _params[6] != "**")
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not add id {0} to the chat color prefix list. Prefix color must be in HTML format, example [FFFFFF] with the brackets included. Found: {1}", _params[1], _params[6]));
                            return;
                        }
                        string[] _c = new string[] { _params[2], _params[3], _params[4], _params[5], _params[6] };
                        if (ChatColorPrefix.Dict.ContainsKey(_params[1]))
                        {
                            ChatColorPrefix.Dict[_cInfo.playerId] = _c;
                            ChatColorPrefix.Dict1[_cInfo.playerId] = _expireDate;
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Updated id {0} with the name of {1} to the group {2} using prefix {3}, name color {4} and prefix color {5} that expires {6} to the chat color prefix list.", _cInfo.playerId, _params[2], _params[3], _params[4], _params[5], _params[6], _expireDate.ToString()));
                        }
                        else
                        {
                            ChatColorPrefix.Dict.Add(_cInfo.playerId, _c);
                            ChatColorPrefix.Dict1.Add(_cInfo.playerId, _expireDate);
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added id {0} with the name of {1} to the group {2} using prefix {3}, name color {4} and prefix color {5} that expires {6} to the chat color prefix list.", _cInfo.playerId, _params[2], _params[3], _params[4], _params[5], _params[6], _expireDate.ToString()));
                        }
                        ChatColorPrefix.UpdateXml();
                        return;
                    }
                    else if (_params.Count == 5)
                    {
                        foreach (var group in ChatColorPrefix.Dict)
                        {
                            if (group.Value[1] == _params[3])
                            {
                                double _daysToExpire2;
                                if (!double.TryParse(_params[4], out _daysToExpire2))
                                {
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid days to expire: {0}", _params[4]));
                                    return;
                                }
                                DateTime _expireDate2;
                                if (_daysToExpire2 > 0d)
                                {
                                    _expireDate2 = DateTime.Now.AddDays(_daysToExpire2);
                                }
                                else
                                {
                                    _expireDate2 = DateTime.Now.AddDays(18250d);
                                }
                                string[] _c = new string[] { _params[2], group.Value[1], group.Value[2], group.Value[3], group.Value[4] };
                                if (ChatColorPrefix.Dict.ContainsKey(_cInfo.playerName))
                                {
                                    ChatColorPrefix.Dict[_cInfo.playerId] = _c;
                                    ChatColorPrefix.Dict1[_cInfo.playerId] = _expireDate2;
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Updated id {0} with the name of {1} to the group {2} using prefix {3}, name color {4} and prefix color {5} that expires {6} to the chat color prefix list.", _cInfo.playerId, _params[2], group.Value[1], group.Value[2], group.Value[3], group.Value[4], _expireDate2.ToString()));
                                }
                                else
                                {
                                    ChatColorPrefix.Dict.Add(_cInfo.playerId, _c);
                                    ChatColorPrefix.Dict1.Add(_cInfo.playerId, _expireDate2);
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added id {0} with the name of {1} to the group {2} using prefix {3}, name color {4} and prefix color {5} that expires {6} to the chat color prefix list.", _cInfo.playerId, _params[2], group.Value[1], group.Value[2], group.Value[3], group.Value[4], _expireDate2.ToString()));
                                }
                                ChatColorPrefix.UpdateXml();
                                return;
                            }
                        }
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] No group with the name {0} was found.", _params[3]));
                        return;
                    }
                    else if (_params.Count == 4)
                    {
                        foreach (var group in ChatColorPrefix.Dict)
                        {
                            if (group.Value[1] == _params[3])
                            {
                                string[] _c = new string[] { _params[2], _params[3], group.Value[2], group.Value[3], group.Value[4] };
                                ChatColorPrefix.Dict1.TryGetValue(group.Key, out DateTime _dt);
                                ChatColorPrefix.Dict.Add(_cInfo.playerId, _c);
                                ChatColorPrefix.Dict1.Add(_cInfo.playerId, _dt);
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added Id {0} with the name of {1} to the group {2} using prefix {3} name color {4} and prefix color {5} that expires {6} to the chat color prefix list.", _cInfo.playerId, _params[2], _params[3], group.Value[2], group.Value[3], group.Value[4], _dt.ToString()));
                                ChatColorPrefix.UpdateXml();
                                return;
                            }
                        }
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] No group with the name {0} was found.", _params[3]));
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
                    foreach (var group in ChatColorPrefix.Dict)
                    {
                        if (group.Value[1] == _params[1])
                        {
                            ChatColorPrefix.Dict.Remove(group.Key);
                            ChatColorPrefix.Dict1.Remove(group.Key);
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed {0} named {1} with group {2} from the chat color prefix list.", group.Key, group.Value[0], _params[1]));
                        }
                        if (group.Key == _params[1])
                        {
                            ChatColorPrefix.Dict.Remove(group.Key);
                            ChatColorPrefix.Dict1.Remove(group.Key);
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed {0} named {1} with group {2} from the chat color prefix list.", _params[1], group.Value[0], group.Value[1]));
                        }
                        if (group.Value[0] == _params[1])
                        {
                            ChatColorPrefix.Dict.Remove(group.Key);
                            ChatColorPrefix.Dict1.Remove(group.Key);
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed {0} named {1} with group {2} from the chat color prefix list.", group.Key, _params[1], group.Value[1]));
                        }
                        ChatColorPrefix.UpdateXml();
                    }
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Completed removing id and groups matching {0} from the chat color prefix list.", _params[1]));
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    foreach (var group in ChatColorPrefix.Dict)
                    {
                        if (group.Value[2] == "")
                        {
                            group.Value[2] = "**";
                        }
                        if (group.Value[3] == "")
                        {
                            group.Value[3] = "**";
                        }
                        DateTime _dt;
                        ChatColorPrefix.Dict1.TryGetValue(group.Key, out _dt);
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Id {0} named {1} with group {2} prefix {3} color {4} expires {5}.", group.Key, group.Value[0], group.Value[1], group.Value[2], group.Value[3], _dt));
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