using System;
using System.Collections.Generic;
using ServerTools.AntiCheat;

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
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not add Id: Invalid Id {0}", _params[1]));
                        return;
                    }
                    ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                    if (_cInfo != null)
                    {
                        if (CredentialCheck.OmittedPlayers.ContainsKey(_cInfo.playerId))
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not add Id. {0} is already in the Family Share list.", _params[1]));
                            return;
                        }
                        CredentialCheck.OmittedPlayers.Add(_cInfo.playerId, _cInfo.playerName);
                        CredentialCheck.UpdateXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added Id {0} with the name of {1} to the Family Share list.", _params[1], _params[2]));
                    }
                    else
                    {
                        if (_params[1].Length > 17)
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not add Id: Invalid Id {0}", _params[1]));
                            return;
                        }
                        if (CredentialCheck.OmittedPlayers.ContainsKey(_params[1]))
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not add Id. {0} is already in the Family Share list.", _params[1]));
                            return;
                        }
                        CredentialCheck.OmittedPlayers.Add(_params[1], _params[2]);
                        CredentialCheck.UpdateXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added Id {0} with the name of {1} to the Family Share list.", _params[1], _params[2]));
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
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not remove Id: Invalid Id {0}", _params[1]));
                        return;
                    }
                    ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                    if (_cInfo != null)
                    {
                        if (!AntiCheat.CredentialCheck.OmittedPlayers.ContainsKey(_cInfo.playerId))
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Id {0} was not found on the Family Share list.", _params[1]));
                            return;
                        }
                        AntiCheat.CredentialCheck.OmittedPlayers.Remove(_cInfo.playerId);
                        AntiCheat.CredentialCheck.UpdateXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed Id {0} from Family Share list.", _params[1]));
                    }
                    else
                    {
                        if (AntiCheat.CredentialCheck.OmittedPlayers.ContainsKey(_params[1]))
                        {
                            ServerTools.AntiCheat.CredentialCheck.OmittedPlayers.Remove(_params[1]);
                            AntiCheat.CredentialCheck.UpdateXml();
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed Id {0} from Family Share list.", _params[1]));
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Id {0} was not found on the Family Share list.", _params[1]));
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}.", _params.Count));
                        return;
                    }
                    if (AntiCheat.CredentialCheck.OmittedPlayers.Count == 0)
                    {
                        SdtdConsole.Instance.Output("[SERVERTOOLS] There are no players on the Family Share list.");
                        return;
                    }
                    else
                    {
                        foreach (var _key in AntiCheat.CredentialCheck.OmittedPlayers)
                        {
                            string _name;
                            if (AntiCheat.CredentialCheck.OmittedPlayers.TryGetValue(_key.Key, out _name))
                            {
                                SdtdConsole.Instance.Output(string.Format("{0} {1}", _key.Key, _name));
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
