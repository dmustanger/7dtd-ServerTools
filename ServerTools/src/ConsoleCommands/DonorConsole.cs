using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class DonorConsole : ConsoleCmdAbstract
    {

        public override string GetDescription()
        {
            return "[ServerTools]-Add or delete a player from the reserved slots list and chat color prefix list.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. Donor add <steamId> <name> <group> <prefix> <color> <days to expire>\n" +
                "  2. Donor remove <steamId>\n" +
                "1. Adds a steam Id to the reserved slots list and chat color prefix list\n" +
                "2. Removes a steam id from the reserved slots list and chat color prefix list\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Donor", "donor", "don" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count == 2)
                    {
                        if (!ReservedSlots.Dict.ContainsKey(_params[1]))
                        {
                            SdtdConsole.Instance.Output(string.Format("Can not remove Id from the reserved slots list. {0} is not in the list.", _params[1]));
                        }
                        else
                        {
                            ReservedSlots.Dict.Remove(_params[1]);
                            SdtdConsole.Instance.Output(string.Format("Remove Id {0} from the reserved slots list.", _params[1]));
                        }
                        if (!ChatColorPrefix.dict.ContainsKey(_params[1]))
                        {
                            SdtdConsole.Instance.Output(string.Format("Can not remove Id from the chat color prefix list. {0} is not in the list.", _params[1]));
                        }
                        else
                        {
                            ChatColorPrefix.dict.Remove(_params[1]);
                            SdtdConsole.Instance.Output(string.Format("Remove Id {0} from the chat color prefix list.", _params[1]));
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}.", _params.Count));
                    }
                }
                else if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count == 7)
                    {
                        if (ReservedSlots.Dict.ContainsKey(_params[1]))
                        {
                            SdtdConsole.Instance.Output(string.Format("Can not add Id to the reserved slots list. {0} is in the list.", _params[1]));
                        }
                        else
                        {
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
                            ReservedSlots.Dict.Add(_params[1], _expireDate);
                            ReservedSlots.Dict1.Add(_params[1], _params[2]);
                            SdtdConsole.Instance.Output(string.Format("Added Id {0} with the name of {1} that expires on {2} to the Reserved Slots list.", _params[1], _params[2], _expireDate.ToString()));
                            ReservedSlots.UpdateXml();
                        }
                        if (ChatColorPrefix.dict.ContainsKey(_params[1]))
                        {
                            SdtdConsole.Instance.Output(string.Format("Can not add Id to the chat color prefix list. {0} is in the list.", _params[1]));
                        }
                        else
                        {
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
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 7, found {0}.", _params.Count));
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in DonorConsole.Run: {0}.", e));
            }
        }
    }
}