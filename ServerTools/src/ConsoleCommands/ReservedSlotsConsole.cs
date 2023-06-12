using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class ReservedSlotConsole : ConsoleCmdAbstract
    {
        protected override string getDescription()
        {
            return "[ServerTools] - Enable, disable, add, edit, remove and view the reserved slots list.";
        }

        protected override string getHelp()
        {
            return "Usage:\n" +
                   "  1. st-rs off\n" +
                   "  2. st-rs on\n" +
                   "  3. st-rs add <EOS/EntityId/PlayerName> <DaysToExpire>\n" +
                   "  4. st-rs edit <EOS/EntityId/PlayerName> <DaysToExpire>\n" +
                   "  5. st-rs remove <EOS/EntityId/PlayerName>\n" +
                   "  6. st-rs list\n" +
                   "1. Turn off reserved slots\n" +
                   "2. Turn on reserved slots\n" +
                   "3. Adds a player to the reserved slots list\n" +
                   "4. Edits a player on the reserved slots list\n" +
                   "5. Removes a player from the reserved slots list\n" +
                   "6. Lists all players on the reserved slots list";
        }

        protected override string[] getCommands()
        {
            return new string[] { "st-ReservedSlots", "rs", "st-rs" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 || _params.Count > 3)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1 to 3, found '{0}'", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (ReservedSlots.IsEnabled)
                    {
                        ReservedSlots.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Reserved slots has been set to off"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Reserved slots is already off"));
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
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Reserved slots has been set to on"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Reserved slots is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count != 3)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 3, found '{0}'", _params.Count));
                        return;
                    }
                    string id = "", playerName = "";
                    ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_params[1]);
                    if (cInfo != null)
                    {
                        id = cInfo.CrossplatformId.CombinedString;
                        playerName = cInfo.playerName;
                    }
                    else
                    {
                        PersistentPlayerData ppd = GeneralOperations.GetPersistentPlayerDataFromId(_params[1]);
                        if (ppd != null)
                        {
                            id = ppd.UserIdentifier.CombinedString;
                            playerName = ppd.PlayerName;
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to locate '{0}' online or offline. Use EOS id for offline player", _params[1]));
                            return;
                        }
                    }
                    if (ReservedSlots.Dict.ContainsKey(id))
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Can not add id '{0}'. Id is already on the reserved slots list", id));
                        return;
                    }
                    if (!double.TryParse(_params[2], out double daysToExpire))
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid days to expire '{0}'", _params[2]));
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
                    ReservedSlots.Dict.Add(id, expireDate);
                    ReservedSlots.Dict1.Add(id, playerName);
                    ReservedSlots.UpdateXml();
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Added id '{0}' named '{1}' with expiry '{2}' to the reserved slots list", id, playerName, expireDate.ToString()));
                    return;
                }
                else if (_params[0].ToLower().Equals("edit"))
                {
                    if (_params.Count != 3)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 3, found '{0}'", _params.Count));
                        return;
                    }
                    string id = "", playerName = "";
                    ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_params[1]);
                    if (cInfo != null)
                    {
                        id = cInfo.CrossplatformId.CombinedString;
                        playerName = cInfo.playerName;
                    }
                    else if (_params[1].Contains("_"))
                    {
                        PersistentPlayerData ppd = GeneralOperations.GetPersistentPlayerDataFromId(_params[1]);
                        if (ppd != null)
                        {
                            id = ppd.UserIdentifier.CombinedString;
                            playerName = ppd.PlayerName;
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to locate '{0}' online or offline. Use EOS id for offline players", _params[1]));
                            return;
                        }
                    }
                    if (!ReservedSlots.Dict.ContainsKey(id))
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Can not edit id '{0}'. This id is not on the reserved slots list", id));
                        return;
                    }
                    if (!double.TryParse(_params[2], out double daysToExpire))
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid days to expire '{0}'", _params[2]));
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
                    ReservedSlots.Dict[id] = expireDate;
                    ReservedSlots.Dict1[id] = playerName;
                    ReservedSlots.UpdateXml();
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Edited id '{0}' named '{1}' to expiry '{2}' on the reserved slots list", id, playerName, expireDate.ToString()));
                    return;
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count != 2)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found '{0}'", _params.Count));
                        return;
                    }
                    string id = "";
                    ClientInfo cInfo = GeneralOperations.GetClientInfoFromNameOrId(_params[1]);
                    if (cInfo != null)
                    {
                        id = cInfo.CrossplatformId.CombinedString;
                    }
                    else if (_params[1].Contains("_"))
                    {
                        PersistentPlayerData ppd = GeneralOperations.GetPersistentPlayerDataFromId(_params[1]);
                        if (ppd != null)
                        {
                            id = ppd.UserIdentifier.CombinedString;
                        }
                    }
                    if (ReservedSlots.Dict.ContainsKey(id))
                    {
                        ReservedSlots.Dict.Remove(id);
                        ReservedSlots.Dict1.Remove(id);
                        ReservedSlots.UpdateXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Removed id '{0}' from the reserved slots list", id));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to remove id '{0}'. Id was not on the reserved slots list", _params[1]));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (_params.Count != 1)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                        return;
                    }
                    if (ReservedSlots.Dict.Count == 0)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] There are no players on the Reserved slots list");
                        return;
                    }
                    else
                    {
                        foreach (var key in ReservedSlots.Dict)
                        {
                            if (ReservedSlots.Dict1.TryGetValue(key.Key, out string name))
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Id '{0}' named '{1}' expires '{2}'", key.Key, name, key.Value));
                            }
                        }
                    }
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ReservedSlotConsole.Execute: {0}", e.Message));
            }
        }
    }
}
