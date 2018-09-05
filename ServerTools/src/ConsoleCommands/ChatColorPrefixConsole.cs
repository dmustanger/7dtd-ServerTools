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
                "  3. ChatColorPrefix add <steamId> <name> <group> <prefix> <color> <days to expire>\n" +
                "  4. ChatColorPrefix add <steamId> <name> <group> <days to expire>\n" +
                "  5. ChatColorPrefix remove <steamId/group>\n" +
                "  6. ChatColorPrefix list\n" +
                "1. Turn off chat color prefix\n" +
                "2. Turn on chat color prefix\n" +
                "3. Adds a steam Id to an existing group on the chat color prefix list\n" +
                "4. Adds a steam Id to a new group on the chat color prefix list\n" +
                "5. Removes a steam Id or entire group from the chat color prefix list\n" +
                "6. Lists all steam Id in the chat color prefix list\n" +
                "*Note the color must be entered as a 6 digit HTML color code*";
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
                    if (_params.Count < 5 || _params.Count > 7)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 5 or 7, found {0}.", _params.Count));
                        return;
                    }
                    if (ChatColorPrefix.dict.ContainsKey(_params[1]))
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not add Id. {0} is already in the chat color prefix list. Remove them first.", _params[1]));
                        return;
                    }
                    if (_params.Count == 5)
                    {
                        if (_params[1].Length != 17)
                        {
                            SdtdConsole.Instance.Output(string.Format("Can not add Id: Invalid Id {0}.", _params[1]));
                            return;
                        }
                        foreach (var group in ChatColorPrefix.dict)
                        {
                            if (group.Value[1] == _params[3])
                            {
                                string[] _c = new string[] { _params[2], _params[3], group.Value[2], group.Value[3] };
                                DateTime _dt;
                                if (ChatColorPrefix.dict1.TryGetValue(group.Key, out _dt))
                                {
                                    ChatColorPrefix.dict.Add(_params[1], _c);
                                    ChatColorPrefix.dict1.Add(_params[1], _dt);
                                    SdtdConsole.Instance.Output(string.Format("Added Id {0} with the name of {1} to the group {2} with prefix {3} and color {4} that expires {5} to the chat color prefix list.", _params[1], _params[2], _params[3], group.Value[2], group.Value[3], _dt.ToString()));
                                    ChatColorPrefix.UpdateXml();
                                    return;
                                }
                            }
                        }
                        SdtdConsole.Instance.Output(string.Format("Can not add Id {0} to {1}. This group does not exist on the list. Create a new entry", _params[1], _params[3]));
                        return;
                    }
                    else if (_params.Count == 7)
                    {
                        foreach (var group in ChatColorPrefix.dict)
                        {
                            if (group.Value[1] == _params[3])
                            {
                                SdtdConsole.Instance.Output(string.Format("Can not add Id {0} to the chat color prefix list. Group {1} already exists on the list. Create a new entry", _params[1], _params[3]));
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
                        if (!_params[5].StartsWith("["))
                        {
                            _color = "[" + _params[5] + "]";
                        }
                        else
                        {
                            _color = _params[5];
                        }
                        string[] _c = new string[] { _params[2], _params[3], _params[4], _color };
                        ChatColorPrefix.dict.Add(_params[1], _c);
                        ChatColorPrefix.dict1.Add(_params[1], _expireDate);
                        SdtdConsole.Instance.Output(string.Format("Added Id {0} with the name of {1} to the group {2} with prefix {3} and color {4} that expires {5} to the chat color prefix list.", _params[1], _params[2], _params[3], _params[4], _color, _expireDate.ToString()));
                        ChatColorPrefix.UpdateXml();
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 5 or 7, found {0}.", _params.Count));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
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
                            SdtdConsole.Instance.Output(string.Format("Removed {0} named {1} with group {2} from the chat color prefix list.", group.Key, group.Value[0], _params[1]));
                        }
                        if (group.Key == _params[1])
                        {
                            SdtdConsole.Instance.Output(string.Format("Removed {0} named {1} with group {2} from the chat color prefix list.", _params[1], group.Value[0], group.Value[1]));
                        }
                    }
                    SdtdConsole.Instance.Output(string.Format("Completed removing id and groups matching {0} from the chat color prefix list.", _params[1]));
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    foreach (var group in ChatColorPrefix.dict)
                    {
                        SdtdConsole.Instance.Output(string.Format("Id {0} named {1} with group {2} prefix {3} color {4} expires {5}.", group.Key, group.Value[0], group.Value[1], group.Value[2], group.Value[3], group.Value[4]));
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