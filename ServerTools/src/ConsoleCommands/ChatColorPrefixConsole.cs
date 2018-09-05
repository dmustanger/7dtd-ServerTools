using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

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
                "  3. ChatColorPrefix add <steamId/entityId> <name> <group> <prefix> <color> <days to expire>\n" +
                "  4. ChatColorPrefix add <steamId/entityId> <name> <group>\n" +
                "  5. ChatColorPrefix del <steamId/entityId/group>\n" +
                "  6. ChatColorPrefix list\n" +
                "1. Turn off chat color prefix\n" +
                "2. Turn on chat color prefix\n" +
                "3. Adds a steam Id on to the chat color prefix list. Set true to require being a reserved player\n" +
                "3. Adds a steam Id to an existing group on the chat color prefix list\n" +
                "5. Removes a steam Id or an entire group in the chat color prefix list\n" +
                "6. Lists all steam Id in the chat color prefix list" +
                "*Note the color must be entered as a 6 digit HTML color code*" +
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
                    if (_params.Count != 4)
                    {
                        if (_params.Count != 7)
                        {
                            SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 4 or 7, found {0}.", _params.Count));
                            return;
                        }
                    }
                    if (_params[1].Length < 1 || _params[1].Length > 17)
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not add Id: Invalid Id {0}.", _params[1]));
                        return;
                    }
                    if (ChatColorPrefix.dict.ContainsKey(_params[1]))
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not add Id. {0} is already in the chat color prefix list. Remove them first.", _params[1]));
                        return;
                    }
                    if (_params.Count == 4)
                    {
                        double _daysToExpire;
                        if (!double.TryParse(_params[7], out _daysToExpire))
                        {
                            SdtdConsole.Instance.Output(string.Format("Invalid days to expire: {0}", _params[7]));
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
                        string[] _c = new string[] { _params[4], _params[5], _params[6] };
                        ChatColorPrefix.dict.Add(_params[1], _c);
                        ChatColorPrefix.dict1.Add(_params[1], _expireDate);
                        SdtdConsole.Instance.Output(string.Format("Added Id {0} with the name of {1} to the group {2} with prefix {3} and color {4} that expires {5} to the chat color prefix list.", _params[1], _params[2], _params[4], _params[5], _params[6], _expireDate.ToString()));
                        ReservedSlots.UpdateXml();
                    }
                    else
                    {

                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}.", _params.Count));
                        return;
                    }
                    if (_params[1].Length != 17)
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not remove player Id: Invalid steam Id {0}.", _params[1]));
                        return;
                    }
                    else
                    {
                        if (!ChatColorPrefix.dict.ContainsKey(_params[1]))
                        {
                            SdtdConsole.Instance.Output(string.Format("Player with steam Id {0} is not on the chat color prefix list. ", _params[1]));
                            return;
                        }
                        else
                        {
                            
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    
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