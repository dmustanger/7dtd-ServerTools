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
                "  1. st-don add <steamId> <group> <prefix> <nameColor> <prefixColor> <days to expire>\n" +
                "  2. st-don remove <steamId>\n" +
                "1. Adds a steam Id to the reserved slots list and chat color prefix list\n" +
                "2. Removes a steam id from the reserved slots list and chat color prefix list\n" +
                "Note using the entity id or player name to add a player will only work if they are online. Use their steam id when offline" +
                "Note if you want a blank prefix or color enter ** without brackets" +
                "Note the colors must be entered as 6 digit HTML color codes. Example [FF0000] or [FFFF00]";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Donor", "don", "st-don" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 2 && _params.Count != 7)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2 or 7, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count == 2)
                    {
                        if (_params[1].Length != 17)
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not remove invalid steam id: {0}", _params[1]));
                            return;
                        }
                        if (!ReservedSlots.Dict.ContainsKey(_params[1]))
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not remove id from the reserved slots list. {0} is not in the list.", _params[1]));
                        }
                        else
                        {
                            ReservedSlots.Dict.Remove(_params[1]);
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed id {0} from the reserved slots list.", _params[1]));
                        }
                        if (!ChatColor.Players.ContainsKey(_params[1]))
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not remove id from the chat color prefix list. {0} is not in the list.", _params[1]));
                        }
                        else
                        {
                            ChatColor.Players.Remove(_params[1]);
                            ChatColor.ExpireDate.Remove(_params[1]);
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
                        if (_params[1].Length != 17)
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not add invalid steam id {0}", _params[1]));
                            return;
                        }
                        else
                        {
                            _cInfo = PersistentOperations.GetClientInfoFromSteamId(_params[1]);
                            if (_cInfo == null)
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
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not add id {0} to the chat color prefix list. Group {1} already exists on the list. Create a new entry or add them to this group", _params[1], _params[2]));
                                    return;
                                }
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
                            if (ReservedSlots.Dict.ContainsKey(_params[1]))
                            {
                                ReservedSlots.Dict[_params[1]] = _expiryDate;
                                ReservedSlots.Dict1[_params[1]] = _cInfo.playerName;
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Updated id {0}, with the name of {1}, expires at {2} to the Reserved Slots list.", _params[1], _cInfo.playerName, _expiryDate.ToString()));
                            }
                            else
                            {
                                ReservedSlots.Dict.Add(_params[1], _expiryDate);
                                ReservedSlots.Dict1.Add(_params[1], _cInfo.playerName);
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added id {0}, with the name of {1} expires at {2} to the Reserved Slots list.", _params[1], _cInfo.playerName, _expiryDate.ToString()));
                            }
                            ReservedSlots.UpdateXml();
                            string[] _c = new string[] { _cInfo.playerName, _params[2], _params[3], _params[4], _params[5] };
                            if (ChatColor.Players.ContainsKey(_params[1]))
                            {
                                ChatColor.Players[_params[1]] = _c;
                                ChatColor.ExpireDate[_params[1]] = _expiryDate;
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Updated id {0}, with the name of {1} to the group {2} using prefix {3}, name color {4} and prefix color {5} that expires {6} to the chat color prefix list.", _params[1], _cInfo.playerName, _params[2], _params[3], _params[4], _params[5], _expiryDate.ToString()));
                            }
                            else
                            {
                                ChatColor.Players.Add(_params[1], _c);
                                ChatColor.ExpireDate.Add(_params[1], _expiryDate);
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added id {0}, with the name of {1} to the group {2} using prefix {3}, name color {4} and prefix color {5} that expires {6} to the chat color prefix list.", _params[1], _cInfo.playerName, _params[2], _params[3], _params[4], _params[5], _expiryDate.ToString()));
                            }
                            ChatColor.UpdateXml();
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 7, found {0}.", _params.Count));
                        }
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