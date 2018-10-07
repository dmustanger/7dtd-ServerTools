using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class ChatColorPrefixConsole : ConsoleCmdAbstract
    {

        public override string GetDescription()
        {
            return "[ServerTools]-Enable, add, delete a player from the chat color prefix list.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. ChatColorPrefix off\n" +
                "  2. ChatColorPrefix on\n" +
                "  3. ChatColorPrefix add <steamId/entityId/playerName> <name> <group> <prefix> <color> <days to expire>\n" +
                "  4. ChatColorPrefix add <steamId/entityId/playerName> <name> <group> <days to expire>\n" +
                "  5. ChatColorPrefix add <steamId/entityId/playerName> <name> <group>\n" +
                "  6. ChatColorPrefix remove <steamId/playerName/group>\n" +
                "  7. ChatColorPrefix list\n" +
                "1. Turn off chat color prefix\n" +
                "2. Turn on chat color prefix\n" +
                "3. Adds a steam Id to a new group with custom prefix and color on the chat color prefix list\n" +
                "4. Adds a steam Id to an existing group with the prefix, color of that group but a custom expiry date\n" +
                "5. Adds a steam Id to an existing group with their prefix, color and expiry date\n" +
                "6. Removes a steam Id or entire group from the chat color prefix list\n" +
                "7. Lists all steam Id in the chat color prefix list\n" +
                "Note if you want a blank prefix or color enter **" +
                "Note the color must be entered as a 6 digit HTML color code";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-ChatColorPrefix", "chatcolorprefix", "ccp" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 || _params.Count > 7)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1 to 7, found {0}.", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    ChatColorPrefix.IsEnabled = false;
                    SdtdConsole.Instance.Output(string.Format("Chat color prefix has been set to off"));
                    return;
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    ChatColorPrefix.IsEnabled = true;
                    SdtdConsole.Instance.Output(string.Format("Chat color prefix has been set to on"));
                    return;
                }
                else if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count < 4 || _params.Count > 7)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 4 to 7, found {0}.", _params.Count));
                        return;
                    }
                    string _steamId = "";
                    if (_params[1].Length == 17)
                    {
                        _steamId = _params[1];
                    }
                    else
                    {
                        ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                        if (_cInfo != null)
                        {
                            _steamId = _cInfo.playerId;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Can not add entity Id or name {0}. They are offline. Use their steam Id instead.", _params[1]));
                            return;
                        }
                    }
                    if (ChatColorPrefix.dict.ContainsKey(_steamId))
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not add Id. {0} is already in the chat color prefix list. Remove them first.", _params[1]));
                        return;
                    }
                    if (_params.Count == 7)
                    {
                        foreach (var group in ChatColorPrefix.dict)
                        {
                            if (group.Value[1] == _params[3])
                            {
                                SdtdConsole.Instance.Output(string.Format("Can not add Id {0} to the chat color prefix list. Group {1} already exists on the list. Create a new entry or add them to this group", _params[1], _params[3]));
                                return;
                            }
                        }
                        double _daysToExpire;
                        if (!double.TryParse(_params[6], out _daysToExpire))
                        {
                            SdtdConsole.Instance.Output(string.Format("Invalid days to expire: {0}", _params[6]));
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
                        string _color = "";
                        if (!_params[5].StartsWith("[") && !_params[5].StartsWith("**"))
                        {
                            _color = "[" + _params[5] + "]";
                        }
                        else
                        {
                            _color = _params[5];
                        }
                        string[] _c = new string[] { _params[2], _params[3], _params[4], _color };
                        ChatColorPrefix.dict.Add(_steamId, _c);
                        ChatColorPrefix.dict1.Add(_steamId, _expireDate);
                        SdtdConsole.Instance.Output(string.Format("Added Id {0} with the name of {1} to the group {2} with prefix {3} and color {4} that expires {5} to the chat color prefix list.", _steamId, _params[2], _params[3], _params[4], _color, _expireDate.ToString()));
                        ChatColorPrefix.UpdateXml();
                        return;
                    }
                    else if (_params.Count == 5)
                    {
                        foreach (var group in ChatColorPrefix.dict)
                        {
                            if (group.Value[1] == _params[3])
                            {
                                string[] _c = { _params[2], _params[3], group.Value[2], group.Value[3] };
                                double _daysToExpire2;
                                if (!double.TryParse(_params[4], out _daysToExpire2))
                                {
                                    SdtdConsole.Instance.Output(string.Format("Invalid days to expire: {0}", _params[4]));
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
                                ChatColorPrefix.dict.Add(_steamId, _c);
                                ChatColorPrefix.dict1.Add(_steamId, _expireDate2);
                                SdtdConsole.Instance.Output(string.Format("Added Id {0} with the name of {1} to the group {2} with prefix {3} and color {4} that expires {5} to the chat color prefix list.", _steamId, _params[2], _params[3], group.Value[2], group.Value[3], _expireDate2.ToString()));
                                ChatColorPrefix.UpdateXml();
                                return;
                            }
                        }
                        SdtdConsole.Instance.Output(string.Format("No group with the name {0} was found.", _params[3]));
                        return;
                    }
                    else if (_params.Count == 4)
                    {
                        foreach (var group in ChatColorPrefix.dict)
                        {
                            if (group.Value[1] == _params[3])
                            {
                                string[] _c = new string[] { _params[2], _params[3], group.Value[2], group.Value[3] };
                                DateTime _dt;
                                ChatColorPrefix.dict1.TryGetValue(group.Key, out _dt);
                                ChatColorPrefix.dict.Add(_steamId, _c);
                                ChatColorPrefix.dict1.Add(_steamId, _dt);
                                SdtdConsole.Instance.Output(string.Format("Added Id {0} with the name of {1} to the group {2} with prefix {3} and color {4} that expires {5} to the chat color prefix list.", _steamId, _params[2], _params[3], group.Value[2], group.Value[3], _dt.ToString()));
                                ChatColorPrefix.UpdateXml();
                                return;
                            }
                        }
                        SdtdConsole.Instance.Output(string.Format("No group with the name {0} was found.", _params[3]));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("remove") || _params[0].ToLower().Equals("delete") || _params[0].ToLower().Equals("del"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}.", _params.Count));
                        return;
                    }
                    foreach (var group in ChatColorPrefix.dict)
                    {
                        if (group.Value[1] == _params[1])
                        {
                            ChatColorPrefix.dict.Remove(group.Key);
                            ChatColorPrefix.dict1.Remove(group.Key);
                            SdtdConsole.Instance.Output(string.Format("Removed {0} named {1} with group {2} from the chat color prefix list.", group.Key, group.Value[0], _params[1]));
                        }
                        if (group.Key == _params[1])
                        {
                            ChatColorPrefix.dict.Remove(group.Key);
                            ChatColorPrefix.dict1.Remove(group.Key);
                            SdtdConsole.Instance.Output(string.Format("Removed {0} named {1} with group {2} from the chat color prefix list.", _params[1], group.Value[0], group.Value[1]));
                        }
                        if (group.Value[0] == _params[1])
                        {
                            ChatColorPrefix.dict.Remove(group.Key);
                            ChatColorPrefix.dict1.Remove(group.Key);
                            SdtdConsole.Instance.Output(string.Format("Removed {0} named {1} with group {2} from the chat color prefix list.", group.Key, _params[1], group.Value[1]));
                        }
                        ChatColorPrefix.UpdateXml();
                    }
                    SdtdConsole.Instance.Output(string.Format("Completed removing id and groups matching {0} from the chat color prefix list.", _params[1]));
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    foreach (var group in ChatColorPrefix.dict)
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
                        ChatColorPrefix.dict1.TryGetValue(group.Key, out _dt);
                        SdtdConsole.Instance.Output(string.Format("Id {0} named {1} with group {2} prefix {3} color {4} expires {5}.", group.Key, group.Value[0], group.Value[1], group.Value[2], group.Value[3], _dt));
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ChatColorPrefixConsole.Run: {0}.", e));
            }
        }
    }
}