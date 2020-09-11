using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class DonorConsole : ConsoleCmdAbstract
    {

        public override string GetDescription()
        {
            return "[ServerTools] - Add or delete a player from the reserved slots list and chat color prefix List.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. Donor add <steamId/entityId/playerName> <name> <group> <prefix> <nameColor> <prefixColor> <days to expire>\n" +
                "  2. Donor remove <steamId>\n" +
                "1. Adds a steam Id to the reserved slots list and chat color prefix list\n" +
                "2. Removes a steam id from the reserved slots list and chat color prefix list\n";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Donor", "don", "st-don" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 2 && _params.Count != 8)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2 or 8, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count == 2)
                    {
                        if (!ReservedSlots.Dict.ContainsKey(_params[1]))
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not remove id from the reserved slots list. {0} is not in the list.", _params[1]));
                        }
                        else
                        {
                            ReservedSlots.Dict.Remove(_params[1]);
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed id {0} from the reserved slots list.", _params[1]));
                        }
                        if (!ChatColorPrefix.Dict.ContainsKey(_params[1]))
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not remove id from the chat color prefix list. {0} is not in the list.", _params[1]));
                        }
                        else
                        {
                            ChatColorPrefix.Dict.Remove(_params[1]);
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed id {0} from the chat color prefix list.", _params[1]));
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found {0}.", _params.Count));
                    }
                }
                else if (_params[0].ToLower().Equals("add"))
                {
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
                        if (!_params[5].Contains("[") || !_params[5].Contains("]"))
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid days to expire: {0}", _params[7]));
                            return;
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
                        if (ReservedSlots.Dict.ContainsKey(_params[1]))
                        {
                            ReservedSlots.Dict[_params[1]] = _expireDate;
                            ReservedSlots.Dict1[_params[1]] = _params[2];
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Updated id {0}, with the name of {1}, expiring on {2} from the Reserved Slots list.", _params[1], _params[2], _expireDate.ToString()));
                        }
                        else
                        {
                            ReservedSlots.Dict.Add(_params[1], _expireDate);
                            ReservedSlots.Dict1.Add(_params[1], _params[2]);
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added id {0}, with the name of {1} expiring on {2} from the Reserved Slots list.", _params[1], _params[2], _expireDate.ToString()));
                        }
                        ReservedSlots.UpdateXml();
                        string[] _c = new string[] { _params[2], _params[3], _params[4], _params[5], _params[6] };
                        if (ChatColorPrefix.Dict.ContainsKey(_params[1]))
                        {
                            ChatColorPrefix.Dict[_params[1]] = _c;
                            ChatColorPrefix.Dict1[_params[1]] = _expireDate;
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Updated id {0}, with the name of {1} to the group {2} using prefix {3}, name color {4} and prefix color {5} that expires {6} to the chat color prefix list.", _cInfo.playerId, _params[2], _params[3], _params[4], _params[5], _params[6], _expireDate.ToString()));
                        }
                        else
                        {
                            ChatColorPrefix.Dict.Add(_params[1], _c);
                            ChatColorPrefix.Dict1.Add(_params[1], _expireDate);
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added id {0}, with the name of {1} to the group {2} using prefix {3}, name color {4} and prefix color {5} that expires {6} to the chat color prefix list.", _cInfo.playerId, _params[2], _params[3], _params[4], _params[5], _params[6], _expireDate.ToString()));
                        }
                        ChatColorPrefix.UpdateXml();
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 8, found {0}.", _params.Count));
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument {0}", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in DonorConsole.Execute: {0}", e.Message));
            }
        }
    }
}