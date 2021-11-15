using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class DonorConsole : ConsoleCmdAbstract
    {

        public override string GetDescription()
        {
            return "[ServerTools] - Add, edit or remove a player from the reserved slots and chat color prefix list at the same time";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. st-don add <steamId/entityId/playerName> <nameColor> <prefix> <prefixColor> <daysToExpire>\n" +
                "  2. st-don add <steamId/entityId/playerName> <daysToExpire>\n" +
                "  3. st-don edit <steamId/entityId/playerName> <daysToExpire>\n" +
                "  4. st-don remove <steamId/entityId/playerName>\n" +
                "1. Adds a player to the reserved slots list and chat color list. If they already exist on the lists it will replace the entry but add to the expiration time\n" +
                "2. Adds a player to the reserved slots list and chat color list but with no colors or prefix. If they already exist on the lists it will replace the entry but add to the expiration time\n" +
                "3. Edit a player's expiry date\n" +
                "4. Remove a player from the reserved slots and chat color lists\n" +
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
                    if (_params.Count == 3)
                    {
                        if (!double.TryParse(_params[2], out double daysToExpire))
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to add player. Invalid days to expire: '{0}'", _params[2]));
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
                        if (ChatColor.IsEnabled)
                        {
                            string[] c = new string[] { playerName, "", "", "" };
                            if (ChatColor.ExpireDate.ContainsKey(steamId))
                            {
                                ChatColor.ExpireDate.TryGetValue(steamId, out DateTime oldDate);
                                if (daysToExpire > 0d)
                                {
                                    oldDate.AddDays(daysToExpire);
                                }
                                else
                                {
                                    oldDate.AddDays(18250d);
                                }
                                ChatColor.Players[steamId] = c;
                                ChatColor.ExpireDate[steamId] = oldDate;
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added player id '{0}' with name '{1}' using no colors or prefix. Expiration set to '{2}' on the chat color list", steamId, playerName, expiryDate.ToString()));
                                ChatColor.UpdateXml();
                            }
                            else
                            {
                                ChatColor.Players.Add(steamId, c);
                                ChatColor.ExpireDate.Add(steamId, expiryDate);
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added player id '{0}' with name '{1}' using no colors or prefix. Expiration set to '{2}' on the chat color list", steamId, playerName, expiryDate.ToString()));
                                ChatColor.UpdateXml();
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output("[SERVERTOOLS] Chat color is not enabled. Unable to add player to list");
                        }
                        if (ReservedSlots.IsEnabled)
                        {
                            if (ReservedSlots.Dict.ContainsKey(steamId))
                            {
                                ReservedSlots.Dict.TryGetValue(steamId, out DateTime oldDate);
                                if (daysToExpire > 0d)
                                {
                                    oldDate.AddDays(daysToExpire);
                                }
                                else
                                {
                                    oldDate.AddDays(18250d);
                                }
                                ReservedSlots.Dict[steamId] = oldDate;
                                ReservedSlots.Dict1[steamId] = playerName;
                                ReservedSlots.UpdateXml();
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added player id '{0}' with name '{1}'. Expiration set to '{2}' on the reserved list", steamId, playerName, expiryDate.ToString()));
                            }
                            else
                            {
                                ReservedSlots.Dict.Add(steamId, expiryDate);
                                ReservedSlots.Dict1.Add(steamId, playerName);
                                ReservedSlots.UpdateXml();
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added player id '{0}' with name '{1}'. Expiration set to '{2}' on the reserved list", steamId, playerName, expiryDate.ToString()));
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output("[SERVERTOOLS] Reserved slots is not enabled. Unable to add player to list");
                        }
                    }
                    else if (_params.Count == 6)
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
                        if (ChatColor.IsEnabled)
                        {
                            string colorTag1;
                            if (ColorList.Colors.ContainsKey(_params[2].ToLower()))
                            {
                                ColorList.Colors.TryGetValue(_params[2].ToLower(), out colorTag1);
                            }
                            else
                            {
                                colorTag1 = _params[2].ToUpper();
                                if (colorTag1 != "" && !colorTag1.StartsWith("["))
                                {
                                    colorTag1 = colorTag1.Insert(0, "[");
                                    colorTag1 = colorTag1.Insert(colorTag1.Length, "]");
                                }
                            }
                            string colorTag2;
                            if (ColorList.Colors.ContainsKey(_params[4].ToLower()))
                            {
                                ColorList.Colors.TryGetValue(_params[4].ToLower(), out colorTag2);
                            }
                            else
                            {
                                colorTag2 = _params[4].ToUpper();
                                if (colorTag2 != "" && !colorTag2.StartsWith("["))
                                {
                                    colorTag2 = colorTag2.Insert(0, "[");
                                    colorTag2 = colorTag2.Insert(colorTag2.Length, "]");
                                }
                            }
                            string[] c = new string[] { playerName, colorTag1, _params[3], colorTag2 };
                            if (ChatColor.ExpireDate.ContainsKey(steamId))
                            {
                                ChatColor.ExpireDate.TryGetValue(steamId, out DateTime oldDate);
                                if (daysToExpire > 0d)
                                {
                                    oldDate.AddDays(daysToExpire);
                                }
                                else
                                {
                                    oldDate.AddDays(18250d);
                                }
                                ChatColor.Players[steamId] = c;
                                ChatColor.ExpireDate[steamId] = oldDate;
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added player id '{0}' with name '{1}' using color '{2}' and prefix '{3}' using color '{4}'. Expiration set to '{5}' on the chat color list", steamId, playerName, colorTag1, _params[3], colorTag2, expiryDate.ToString()));
                                ChatColor.UpdateXml();
                            }
                            else
                            {
                                ChatColor.Players.Add(steamId, c);
                                ChatColor.ExpireDate.Add(steamId, expiryDate);
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added player id '{0}' with name '{1}' using color '{2}' and prefix '{3}' using color '{4}'. Expiration set to '{5}' on the chat color list", steamId, playerName, colorTag1, _params[3], colorTag2, expiryDate.ToString()));
                                ChatColor.UpdateXml();
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output("[SERVERTOOLS] Chat color is not enabled. Unable to add player to list");
                        }
                        if (ReservedSlots.IsEnabled)
                        {
                            if (ReservedSlots.Dict.ContainsKey(steamId))
                            {
                                ReservedSlots.Dict.TryGetValue(steamId, out DateTime oldDate);
                                if (daysToExpire > 0d)
                                {
                                    oldDate.AddDays(daysToExpire);
                                }
                                else
                                {
                                    oldDate.AddDays(18250d);
                                }
                                ReservedSlots.Dict[steamId] = oldDate;
                                ReservedSlots.Dict1[steamId] = playerName;
                                ReservedSlots.UpdateXml();
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added player id '{0}' with name '{1}'. Expiration set to '{2}' on the reserved list", steamId, playerName, expiryDate.ToString()));
                            }
                            else
                            {
                                ReservedSlots.Dict.Add(steamId, expiryDate);
                                ReservedSlots.Dict1.Add(steamId, playerName);
                                ReservedSlots.UpdateXml();
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added player id '{0}' with name '{1}'. Expiration set to '{2}' on the reserved list", steamId, playerName, expiryDate.ToString()));
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output("[SERVERTOOLS] Reserved slots is not enabled. Unable to add player to list");
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 3 or 6, found '{0}'", _params.Count));
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
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Edited player id '{0}'. Expiry set to '{1}' on the reserved slots list", steamId, expiryDate));
                    }
                    if (ReservedSlots.Dict.ContainsKey(steamId))
                    {
                        ReservedSlots.Dict[steamId] = expiryDate;
                        ReservedSlots.UpdateXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Edited player id '{0}'. Expiry set to '{1}' on the reserved slots list", steamId, expiryDate));
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
                    if (ChatColor.Players.ContainsKey(steamId))
                    {
                        ChatColor.Players.Remove(steamId);
                        ChatColor.ExpireDate.Remove(steamId);
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed player id '{0}' from the chat color prefix list", steamId));
                    }
                    if (ReservedSlots.Dict.ContainsKey(steamId))
                    {
                        ReservedSlots.Dict.Remove(steamId);
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed player id '{0}' from the reserved slots list", steamId));
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