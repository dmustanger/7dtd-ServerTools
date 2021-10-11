using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class DonorConsole : ConsoleCmdAbstract
    {

        public override string GetDescription()
        {
            return "[ServerTools] - Add or remove a player from the reserved slots and chat color prefix list at the same time";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. st-don add <steamId/entityId/playerName> <nameColor> <prefix> <prefixColor> <daysToExpire>\n" +
                "  2. st-don edit <steamId/entityId/playerName> <daysToExpire>\n" +
                "  3. st-don remove <steamId/entityId/playerName>\n" +
                "1. Add a player to the reserved slots list and chat color prefix list\n" +
                "2. Edit a player's expiry date\n" +
                "3. Remove a player from the reserved slots list and chat color prefix list\n" +
                "*Note*     Using the entity id or player name to add a player will only work if they are online. Use their steam id when offline" +
                "*Note*     The colors must be entered as 6 digit HTML color codes or colors from the ColorList.xml. Example [FF0000] or [FFFF00],[FFCC00] or Red";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Donor", "don", "st-don" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 2 && _params.Count != 3 && _params.Count != 6)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, 3 or 6, found '{0}'", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count == 6)
                    {
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
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to locate player '{0}' online or offline. Use steam id for offline player", _params[1]));
                                return;
                            }
                        }
                        if (ChatColor.IsEnabled)
                        {
                            if (ChatColor.Players.ContainsKey(steamId))
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Player id '{0}' is already on the chat color prefix list. You can edit their entry or remove them first", steamId));
                            }
                            else if (steamId != "")
                            {
                                string colorTag1 = _params[2];
                                if (ColorList.Colors.ContainsKey(_params[2]))
                                {
                                    ColorList.Colors.TryGetValue(_params[2], out colorTag1);
                                }
                                if ((colorTag1.Contains("[") && colorTag1.Contains("]")) || colorTag1 == "")
                                {
                                    string colorTag2 = _params[4];
                                    if (ColorList.Colors.ContainsKey(_params[4]))
                                    {
                                        ColorList.Colors.TryGetValue(_params[4], out colorTag2);
                                    }
                                    if ((colorTag2.Contains("[") && colorTag2.Contains("]")) || colorTag2 == "")
                                    {
                                        if (!double.TryParse(_params[5], out double daysToExpire))
                                        {
                                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to add player. Invalid days to expire: '{0}'", _params[5]));
                                            return;
                                        }
                                        DateTime expiryDate;
                                        if (daysToExpire > 0d)
                                        {
                                            expiryDate = DateTime.Now.AddDays(daysToExpire);
                                        }
                                        else
                                        {
                                            expiryDate = DateTime.Now.AddDays(18250d);
                                        }
                                        string[] c = new string[] { playerName, colorTag1, _params[3], colorTag2 };
                                        ChatColor.Players.Add(steamId, c);
                                        ChatColor.ExpireDate.Add(steamId, expiryDate);
                                        ChatColor.UpdateXml();
                                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added player id '{0}' with name '{1}' using color '{2}' and prefix '{3}' using color '{4}'. Expiration set to '{5}'", steamId, playerName, colorTag1, _params[3], colorTag2, expiryDate.ToString()));
                                    }
                                }
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Chat color is not enabled. Unable to add player to list"));
                        }
                        if (ReservedSlots.IsEnabled)
                        {
                            if (ReservedSlots.Dict.ContainsKey(steamId))
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Player id '{0}' is already on the reserved slots list. You can edit their entry or remove them first", steamId));
                            }
                            else if (steamId != "")
                            {
                                if (!double.TryParse(_params[5], out double daysToExpire))
                                {
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to add player. Invalid days to expire: '{0}'", _params[5]));
                                    return;
                                }
                                DateTime expiryDate;
                                if (daysToExpire > 0d)
                                {
                                    expiryDate = DateTime.Now.AddDays(daysToExpire);
                                }
                                else
                                {
                                    expiryDate = DateTime.Now.AddDays(18250d);
                                }
                                ReservedSlots.Dict.Add(steamId, expiryDate);
                                ReservedSlots.Dict1.Add(steamId, playerName);
                                ReservedSlots.UpdateXml();
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added player id '{0}' with name '{1}'. Expiration set to '{2}'", steamId, playerName, expiryDate.ToString()));
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Reserved slots is not enabled. Unable to add player to list"));
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 6, found '{0}'", _params.Count));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("edit"))
                {
                    if (_params.Count != 3)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 3, found '{0}'", _params.Count));
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
                    if (!double.TryParse(_params[2], out double daysToExpire))
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to edit player. Invalid days to expire: '{0}'", _params[2]));
                        return;
                    }
                    DateTime expiryDate;
                    if (daysToExpire > 0d)
                    {
                        expiryDate = DateTime.Now.AddDays(daysToExpire);
                    }
                    else
                    {
                        expiryDate = DateTime.Now.AddDays(18250d);
                    }
                    if (ChatColor.ExpireDate.ContainsKey(steamId))
                    {
                        ChatColor.ExpireDate[steamId] = expiryDate;
                        ChatColor.UpdateXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Edited player id '{0}' in chat color prefix. Expiry set to '{1}'", steamId, expiryDate));
                    }
                    if (ReservedSlots.Dict.ContainsKey(steamId))
                    {
                        ReservedSlots.Dict[steamId] = expiryDate;
                        ReservedSlots.UpdateXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Edited player id '{0}' in reserved slots. Expiry set to '{1}'", steamId, expiryDate));
                    }
                    return;
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found '{0}'", _params.Count));
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
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed player id '{0}' from the reserved slots list", steamId));
                    }
                    if (ChatColor.Players.ContainsKey(steamId))
                    {
                        ChatColor.Players.Remove(steamId);
                        ChatColor.ExpireDate.Remove(steamId);
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed player id '{0}' from the chat color prefix list", steamId));
                    }
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in DonorConsole.Execute: {0}", e.Message));
            }
        }
    }
}