using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class ReservedSlotConsole : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "[ServerTools] - Enable, disable, add, edit, remove and view the reserved slots list.";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                   "  1. st-rs off\n" +
                   "  2. st-rs on\n" +
                   "  3. st-rs add <steamId/entityId/playerName> <daysToExpire>\n" +
                   "  4. st-rs edit <steamId/entityId/playerName> <daysToExpire>\n" +
                   "  5. st-rs remove <steamId/entityId/playerName>\n" +
                   "  6. st-rs list\n" +
                   "1. Turn off reserved slots\n" +
                   "2. Turn on reserved slots\n" +
                   "3. Adds a player to the reserved slots list\n" +
                   "4. Edits a player on the reserved slots list\n" +
                   "5. Removes a player from the reserved slots list\n" +
                   "6. Lists all players on the reserved slots list";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-ReservedSlots", "rs", "st-rs" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 || _params.Count > 3)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1 to 3, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (ReservedSlots.IsEnabled)
                    {
                        ReservedSlots.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Reserved slots has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Reserved slots is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!ReservedSlots.IsEnabled)
                    {
                        ReservedSlots.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Reserved slots has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Reserved slots is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count != 3)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 3, found {0}", _params.Count));
                        return;
                    }
                    string steamId = "", playerName = "";
                    ClientInfo cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                    if (cInfo != null)
                    {
                        steamId = cInfo.playerId;
                        playerName = cInfo.playerName;
                    }
                    else
                    {
                        PlayerDataFile pdf = PersistentOperations.GetPlayerDataFileFromSteamId(_params[1]);
                        if (pdf != null)
                        {
                            steamId = _params[1];
                            playerName = pdf.ecd.entityName;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to locate player {0} online or offline. Use steam id for offline player", _params[1]));
                            return;
                        }
                    }
                    if (ReservedSlots.Dict.ContainsKey(steamId))
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not add player id {0}. This id is already on the reserved slots list", steamId));
                        return;
                    }
                    if (!double.TryParse(_params[2], out double daysToExpire))
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid days to expire: {0}", _params[2]));
                        return;
                    }
                    DateTime expireDate;
                    if (daysToExpire > 0d)
                    {
                        expireDate = DateTime.Now.AddDays(daysToExpire);
                    }
                    else
                    {
                        expireDate = DateTime.Now.AddDays(18250d);
                    }
                    ReservedSlots.Dict.Add(steamId, expireDate);
                    ReservedSlots.Dict1.Add(steamId, playerName);
                    ReservedSlots.UpdateXml();
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added player id {0} named {1} with expiry '{2}' to the reserved slots list", steamId, playerName, expireDate.ToString()));
                    return;
                }
                else if (_params[0].ToLower().Equals("edit"))
                {
                    if (_params.Count != 3)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 3, found {0}", _params.Count));
                        return;
                    }
                    string steamId = "", playerName = "";
                    ClientInfo cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                    if (cInfo != null)
                    {
                        steamId = cInfo.playerId;
                        playerName = cInfo.playerName;
                    }
                    else
                    {
                        PlayerDataFile pdf = PersistentOperations.GetPlayerDataFileFromSteamId(_params[1]);
                        if (pdf != null)
                        {
                            steamId = _params[1];
                            playerName = pdf.ecd.entityName;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to locate player {0} online or offline. Use steam id for offline player", _params[1]));
                            return;
                        }
                    }
                    if (!ReservedSlots.Dict.ContainsKey(steamId))
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Can not edit player id {0}. This id is not on the reserved slots list", steamId));
                        return;
                    }
                    if (!double.TryParse(_params[2], out double daysToExpire))
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid days to expire: {0}", _params[2]));
                        return;
                    }
                    DateTime expireDate;
                    if (daysToExpire > 0d)
                    {
                        expireDate = DateTime.Now.AddDays(daysToExpire);
                    }
                    else
                    {
                        expireDate = DateTime.Now.AddDays(18250d);
                    }
                    ReservedSlots.Dict[steamId] = expireDate;
                    ReservedSlots.Dict1[steamId] = playerName;
                    ReservedSlots.UpdateXml();
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Edited player id {0} named {1} to expiry '{2}' on the reserved slots list", steamId, playerName, expireDate.ToString()));
                    return;
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }
                    string steamId = "";
                    ClientInfo cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                    if (cInfo != null)
                    {
                        steamId = cInfo.playerId;
                    }
                    else
                    {
                        steamId = _params[1];
                    }
                    if (ReservedSlots.Dict.ContainsKey(steamId))
                    {
                        ReservedSlots.Dict.Remove(steamId);
                        ReservedSlots.Dict1.Remove(steamId);
                        ReservedSlots.UpdateXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed player id {0} from the reserved slots list", steamId));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to remove player id {0}. Id was not on the reserved slots list", _params[1]));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found {0}", _params.Count));
                        return;
                    }
                    if (ReservedSlots.Dict.Count == 0)
                    {
                        SdtdConsole.Instance.Output("[SERVERTOOLS] There are no players on the Reserved slots list");
                        return;
                    }
                    else
                    {
                        foreach (var key in ReservedSlots.Dict)
                        {
                            if (ReservedSlots.Dict1.TryGetValue(key.Key, out string name))
                            {
                                SdtdConsole.Instance.Output(string.Format("Player id {0} named {1} expires '{2}'", key.Key, name, key.Value));
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
                Log.Out(string.Format("[SERVERTOOLS] Error in ReservedSlotConsole.Execute: {0}", e.Message));
            }
        }
    }
}
