using System;
using System.Collections.Generic;

namespace ServerTools
{
    class FamilyShareConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Add, remove and view steam id on the family share list.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-fs add <steamId/entityId> <playerName>\n" +
                   "  2. st-fs remove <steamId/entityId>\n" +
                   "  3. st-fs list\n" +
                   "1. Adds a steamID to the Family Share list\n" +
                   "2. Removes a steamID from the Family Share list\n" +
                   "3. Lists all steamIDs from the Family Share list";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-FamilyShare", "fs", "st-fs" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 || _params.Count > 3)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, to 3, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count != 3)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 3, found {0}.", _params.Count));
                        return;
                    }
                    if (_params[1].Length < 1 || _params[1].Length > 17)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not add. Invalid id {0}", _params[1]));
                        return;
                    }
                    ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                    if (_cInfo != null)
                    {
                        if (Credentials.Dict.ContainsKey(_cInfo.playerId))
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not add. {0} is already in the family share list", _params[1]));
                            return;
                        }
                        Credentials.Dict.Add(_cInfo.playerId, _cInfo.playerName);
                        Credentials.UpdateXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added id {0} with the name of {1} to the family share list", _params[1], _params[2]));
                    }
                    else
                    {
                        if (_params[1].Length > 17)
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not add. Invalid Id: {0}", _params[1]));
                            return;
                        }
                        if (Credentials.Dict.ContainsKey(_params[1]))
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not add. Id {0} is already in the family share list", _params[1]));
                            return;
                        }
                        Credentials.Dict.Add(_params[1], _params[2]);
                        Credentials.UpdateXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added Id {0} with the name of {1} to the family share list", _params[1], _params[2]));
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    if (_params[1].Length < 1 || _params[1].Length > 17)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not remove. Invalid id {0}", _params[1]));
                        return;
                    }
                    ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                    if (_cInfo != null)
                    {
                        if (!Credentials.Dict.ContainsKey(_cInfo.playerId))
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Id {0} was not found on the family share list", _params[1]));
                            return;
                        }
                        Credentials.Dict.Remove(_cInfo.playerId);
                        Credentials.UpdateXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed id {0} from family share list", _params[1]));
                    }
                    else
                    {
                        if (Credentials.Dict.ContainsKey(_params[1]))
                        {
                            Credentials.Dict.Remove(_params[1]);
                            Credentials.UpdateXml();
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed id {0} from family share list", _params[1]));
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Id {0} was not found on the family share list", _params[1]));
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count));
                        return;
                    }
                    if (Credentials.Dict.Count == 0)
                    {
                        SdtdConsole.Instance.Output("[SERVERTOOLS] There are no players on the family share list");
                        return;
                    }
                    else
                    {
                        foreach (var key in Credentials.Dict)
                        {
                            if (Credentials.Dict.TryGetValue(key.Key, out string name))
                            {
                                SdtdConsole.Instance.Output(string.Format("{0} {1}", key.Key, name));
                            }
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
                Log.Out(string.Format("[SERVERTOOLS] Error in FamilyShareConsole.Execute: {0}", e.Message));
            }
        }
    }
}
