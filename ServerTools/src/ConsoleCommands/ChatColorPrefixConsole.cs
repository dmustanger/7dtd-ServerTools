using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class ChatColorPrefixConsole : ConsoleCmdAbstract
    {

        public override string GetDescription()
        {
            return "[ServerTools] - Enable, add, edit, delete a player from the chat color prefix list.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. st-ccp off\n" +
                "  2. st-ccp on\n" +
                "  3. st-ccp add <steamId/entityId/playerName> <nameColor> <prefix> <prefixColor> <daysToExpire>\n" +
                "  4. st-ccp edit <steamId/entityId/playerName> <nameColor> <prefix> <prefixColor>\n" +
                "  5. st-ccp edit <steamId/entityId/playerName> <daysToExpire>\n" +
                "  6. st-ccp remove <steamId/entityId/playerName>\n" +
                "  7. st-ccp list\n" +
                "1. Turn off chat color prefix\n" +
                "2. Turn on chat color prefix\n" +
                "3. Adds a player to the list. If they already exist on the list it will replace the entry but add to the expiration time\n" +
                "4. Edits a player's prefix and colors\n" +
                "5. Edits a player's expiry date\n" +
                "6. Removes a player from the list\n" +
                "7. Shows all players on the list\n" +
                "*Note*     Using the entity id or player name to add a player will only work if they are online. Use their steam id when offline" +
                "*Note*     The colors must be entered as 6 digit HTML color code or color names from the color list. Example [FF0000], [FFFF00], Red, Rainbow";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-ChatColorPrefix", "ccp", "st-ccp" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count == 0 || _params.Count == 4 || _params.Count >= 7)
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1 to 3, 5, or 6, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (ChatColor.IsEnabled)
                    {
                        ChatColor.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Chat color prefix has been set to off"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Chat color prefix is already off"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("on"))
                {
                    if (!ChatColor.IsEnabled)
                    {
                        ChatColor.IsEnabled = true;
                        Config.WriteXml();
                        Config.LoadXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Chat color prefix has been set to on"));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Chat color prefix is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count != 6)
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 6, found '{0}'", _params.Count));
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
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to locate player '{0}' online or offline. Use steam id for offline player", _params[1]));
                            return;
                        }
                    }
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
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added player id '{0}' with name '{1}' using color '{2}' and prefix '{3}' using color '{4}'. Expiration set to '{5}'", steamId, playerName, colorTag1, _params[3], colorTag2, expiryDate.ToString()));
                                ChatColor.UpdateXml();
                            }
                            else
                            {
                                ChatColor.Players.Add(steamId, c);
                                ChatColor.ExpireDate.Add(steamId, expiryDate);
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Added player id '{0}' with name '{1}' using color '{2}' and prefix '{3}' using color '{4}'. Expiration set to '{5}'", steamId, playerName, colorTag1, _params[3], colorTag2, expiryDate.ToString()));
                                ChatColor.UpdateXml();
                            }
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid prefix color: '{0}'", _params[4]));
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid name color: '{0}'", _params[2]));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("edit"))
                {
                    if (_params.Count == 3)
                    {
                        string steamId = "";
                        ClientInfo cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                        if (cInfo != null)
                        {
                            steamId = cInfo.playerId;
                        }
                        else
                        {
                            PlayerDataFile pdf = PersistentOperations.GetPlayerDataFileFromSteamId(_params[1]);
                            if (pdf != null)
                            {
                                steamId = _params[1];
                            }
                            else
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to locate player '{0}' online or offline. Use steam id for offline player", _params[1]));
                                return;
                            }
                        }
                        if (ChatColor.Players.ContainsKey(steamId))
                        {
                            if (!int.TryParse(_params[2], out int daysToExpire))
                            {
                                SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to edit player. Invalid integer '{0}'", _params[2]));
                                return;
                            }
                            ChatColor.ExpireDate[steamId] = DateTime.Now.AddDays(daysToExpire);
                            ChatColor.UpdateXml();
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Edited chat color prefix for player id '{0}'. Expiry date set to '{1}'", steamId, DateTime.Now.AddDays(daysToExpire)));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to edit player id '{0}'. They are not on the chat color prefix list", _params[1]));
                            return;
                        }
                    }
                    else if (_params.Count == 5)
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
                        if (ChatColor.Players.ContainsKey(steamId))
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

                                    string[] c = new string[] { playerName, colorTag1, _params[3], colorTag2 };
                                    ChatColor.Players[steamId] = c;
                                    ChatColor.UpdateXml();
                                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Edited chat color prefix for player id '{0}'. Name color set to '{1}' and prefix set to '{2}' using color '{3}'", steamId, colorTag1, _params[3], colorTag2));
                                    return;
                                }
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to edit player id '{0}'. They are not on the chat color prefix list", _params[1]));
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 3 or 5, found '{0}'", _params.Count));
                        return;
                    }
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
                        PlayerDataFile pdf = PersistentOperations.GetPlayerDataFileFromSteamId(_params[1]);
                        if (pdf != null)
                        {
                            steamId = _params[1];
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to locate player '{0}' online or offline. Use steam id for offline player", _params[1]));
                            return;
                        }
                    }
                    if (ChatColor.Players.ContainsKey(steamId))
                    {
                        ChatColor.Players.Remove(steamId);
                        ChatColor.ExpireDate.Remove(steamId);
                        ChatColor.UpdateXml();
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Removed player id '{0}' from the chat color prefix list", _params[1]));
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Unable to remove player id '{0}'. They are not on the chat color prefix list", _params[1]));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (ChatColor.Players.Count > 0)
                    {
                        foreach (var player in ChatColor.Players)
                        {
                            ChatColor.ExpireDate.TryGetValue(player.Key, out DateTime expiry);
                            SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Player id '{0}' named '{1}' using color '{2}' and prefix '{3}' using color '{4}' expires '{5}'", player.Key, player.Value[0], player.Value[1], player.Value[2], player.Value[3], expiry));
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("[SERVERTOOLS] No players on the chat color list");
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ChatColorPrefixConsole.Execute: {0}", e.Message));
            }
        }
    }
}