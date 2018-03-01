using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class ReservedSlot : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools]-Add, Remove and View steamids on the ReservedSlots list.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. reservedslot add <steamId/EntityId> <playerName> <days to expire>\n" +
                   "  2. reservedslot remove <steamId/EntityId>\n" +
                   "  3. reservedslot list\n" +
                   "1. Adds a steamID to the Reserved Slots list\n" +
                   "2. Removes a steamID from the Reserved Slots list\n" +
                   "3. Lists all steamIDs that have a Reserved Slot";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-ReservedSlot", "reservedslot", "rs" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1 || _params.Count != 2 || _params.Count != 4)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, 2 or 4, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count != 4)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 4, found {0}.", _params.Count));
                        return;
                    }
                    if (_params[1].Length < 1 || _params[1].Length > 17)
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not add Id: Invalid Id {0}", _params[1]));
                        return;
                    }
                    ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                    if (_cInfo != null)
                    {
                        if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                        {
                            SdtdConsole.Instance.Output(string.Format("Can not add Id. {0} is already in the Reserved Slots list.", _params[1]));
                            return;
                        }
                        double _daysToExpire;
                        if (!double.TryParse(_params[3], out _daysToExpire))
                        {
                            SdtdConsole.Instance.Output(string.Format("Invalid days to expire: {0}", _params[3]));
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
                        ReservedSlots.Dict.Add(_cInfo.playerId, _expireDate);
                        ReservedSlots.Dict1.Add(_cInfo.playerId, _params[2]);
                        SdtdConsole.Instance.Output(string.Format("Added Id {0} with the name of {1} that expires on {2} to the Reserved Slots list.", _params[1], _params[2], _expireDate.ToString()));
                        ReservedSlots.UpdateXml();
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    if (_params[1].Length < 1 || _params[1].Length > 17)
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not add Id: Invalid Id {0}", _params[1]));
                        return;
                    }
                    ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                    if (_cInfo != null)
                    {
                        if (!ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                        {
                            SdtdConsole.Instance.Output(string.Format("Id {0} was not found in .", _params[1]));
                            return;
                        }
                        ReservedSlots.Dict.Remove(_cInfo.playerId);
                        ReservedSlots.Dict1.Remove(_cInfo.playerId);
                        SdtdConsole.Instance.Output(string.Format("Removed Id {0} from Reserved Slots list.", _params[1]));
                        ReservedSlots.UpdateXml();
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}.", _params.Count));
                        return;
                    }
                    if (ReservedSlots.Dict.Count < 1)
                    {
                        SdtdConsole.Instance.Output("There are no steamIds on the Reserved Slots list.");
                        return;
                    }
                    foreach (KeyValuePair<string, DateTime> _key in ReservedSlots.Dict)
                    {
                        string _name;
                        if (ReservedSlots.Dict1.TryGetValue(_key.Key, out _name))
                        {
                            SdtdConsole.Instance.Output(string.Format("{0} {1} {2}", _key.Key, _name, _key.Value));
                        }
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ReservedSlot.Run: {0}.", e));
            }
        }
    }
}
