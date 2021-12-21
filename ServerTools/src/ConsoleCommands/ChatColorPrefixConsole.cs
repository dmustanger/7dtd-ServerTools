using System;
using System.Collections.Generic;

namespace ServerTools
{
    class ChatColorPrefixConsole : ConsoleCmdAbstract
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
                "  3. st-ccp add <Id/entityId/playerName> <nameColor> <prefix> <prefixColor> <daysToExpire>\n" +
                "  4. st-ccp add <Id/entityId/playerName> <daysToExpire>\n" +
                "  5. st-ccp edit <Id/entityId/playerName> <nameColor> <prefix> <prefixColor>\n" +
                "  6. st-ccp edit <Id/entityId/playerName> <daysToExpire>\n" +
                "  7. st-ccp remove <Id/entityId/playerName>\n" +
                "  8. st-ccp list\n" +
                "1. Turn off chat color prefix\n" +
                "2. Turn on chat color prefix\n" +
                "3. Adds a player to the list. If they already exist on the list it will replace the entry but add to the expiration time\n" +
                "4. Adds a player to the list with no colors or prefix. If they already exist on the list it will replace the entry but add to the expiration time\n" +
                "5. Edit a player's prefix and colors\n" +
                "6. Edit a player's expiry date\n" +
                "7. Removes a player from the list\n" +
                "8. Shows all players on the list\n" +
                "*Note*     Using the entity id or player name to add a player will only work if they are online. Use their steam id when offline" +
                "*Note*     The colors must be entered as 6 digit HTML color code or color names from the color list. Example FF0000, FFFF00, Red, Rainbow";
        }

        public override string[] GetCommands()
        {
            return new string[] { "st-ChatColorPrefix", "ccp", "st-ccp" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1 && _params.Count != 2 && _params.Count != 3 && _params.Count != 5 && _params.Count != 6)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, 2, 3, 5 or 6, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("off"))
                {
                    if (ChatColor.IsEnabled)
                    {
                        ChatColor.IsEnabled = false;
                        Config.WriteXml();
                        Config.LoadXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Chat color prefix has been set to off"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Chat color prefix is already off"));
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
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Chat color prefix has been set to on"));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Chat color prefix is already on"));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("add"))
                {
                    string id = "", playerName = "";
                    ClientInfo cInfo = PersistentOperations.GetClientInfoFromNameOrId(_params[1]);
                    if (cInfo != null)
                    {
                        id = cInfo.PlatformId.CombinedString;
                        playerName = cInfo.playerName;
                    }
                    else
                    {
                        PlayerDataFile pdf = PersistentOperations.GetPlayerDataFileFromId(_params[1]);
                        if (pdf != null)
                        {
                            id = _params[1];
                            playerName = pdf.ecd.entityName;
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to locate player '{0}' online or offline. Use steam id for offline player", _params[1]));
                            return;
                        }
                    }
                    if (_params.Count == 3)
                    {
                        if (!double.TryParse(_params[2], out double daysToExpire))
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to add player. Invalid days to expire: '{0}'", _params[2]));
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
                        string[] c = new string[] { playerName, "", "", "" };
                        if (ChatColor.ExpireDate.ContainsKey(id))
                        {
                            ChatColor.ExpireDate.TryGetValue(id, out DateTime oldDate);
                            if (daysToExpire > 0d)
                            {
                                oldDate.AddDays(daysToExpire);
                            }
                            else
                            {
                                oldDate.AddDays(18250d);
                            }
                            ChatColor.Players[id] = c;
                            ChatColor.ExpireDate[id] = oldDate;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Added player id '{0}' with name '{1}' using no colors or prefix. Expiration set to '{2}'", id, playerName, expiryDate.ToString()));
                            ChatColor.UpdateXml();
                        }
                        else
                        {
                            ChatColor.Players.Add(id, c);
                            ChatColor.ExpireDate.Add(id, expiryDate);
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Added player id '{0}' with name '{1}' using no colors or prefix. Expiration set to '{2}'", id, playerName, expiryDate.ToString()));
                            ChatColor.UpdateXml();
                        }
                        return;
                    }
                    else if (_params.Count == 6)
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
                        if (!double.TryParse(_params[5], out double daysToExpire))
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to add player. Invalid days to expire: '{0}'", _params[5]));
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
                        if (ChatColor.ExpireDate.ContainsKey(id))
                        {
                            ChatColor.ExpireDate.TryGetValue(id, out DateTime oldDate);
                            if (daysToExpire > 0d)
                            {
                                oldDate.AddDays(daysToExpire);
                            }
                            else
                            {
                                oldDate.AddDays(18250d);
                            }
                            ChatColor.Players[id] = c;
                            ChatColor.ExpireDate[id] = oldDate;
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Added player id '{0}' with name '{1}' using color '{2}' and prefix '{3}' using color '{4}'. Expiration set to '{5}'", id, playerName, colorTag1, _params[3], colorTag2, expiryDate.ToString()));
                            ChatColor.UpdateXml();
                        }
                        else
                        {
                            ChatColor.Players.Add(id, c);
                            ChatColor.ExpireDate.Add(id, expiryDate);
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Added player id '{0}' with name '{1}' using color '{2}' and prefix '{3}' using color '{4}'. Expiration set to '{5}'", id, playerName, colorTag1, _params[3], colorTag2, expiryDate.ToString()));
                            ChatColor.UpdateXml();
                        }
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 1, 2, 3, 5 or 6, found {0}", _params.Count));
                        return;
                    }
                    
                }
                else if (_params[0].ToLower().Equals("edit"))
                {
                    string id = "", playerName = "";
                    ClientInfo cInfo = PersistentOperations.GetClientInfoFromNameOrId(_params[1]);
                    if (cInfo != null)
                    {
                        id = cInfo.CrossplatformId.CombinedString;
                        playerName = cInfo.playerName;
                    }
                    else
                    {
                        PlayerDataFile pdf = PersistentOperations.GetPlayerDataFileFromId(_params[1]);
                        if (pdf != null)
                        {
                            id = _params[1];
                            playerName = pdf.ecd.entityName;
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to locate player '{0}' online or offline. Use steam id for offline player", _params[1]));
                            return;
                        }
                    }
                    if (_params.Count == 3)
                    {
                        if (ChatColor.Players.ContainsKey(id))
                        {
                            if (!int.TryParse(_params[2], out int daysToExpire))
                            {
                                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to edit player. Invalid integer '{0}'", _params[2]));
                                return;
                            }
                            ChatColor.ExpireDate[id] = DateTime.Now.AddDays(daysToExpire);
                            ChatColor.UpdateXml();
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Edited chat color prefix for player id '{0}'. Expiry date set to '{1}'", id, DateTime.Now.AddDays(daysToExpire)));
                            return;
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to edit player id '{0}'. They are not on the chat color prefix list", _params[1]));
                            return;
                        }
                    }
                    else if (_params.Count == 5)
                    {
                        if (ChatColor.Players.ContainsKey(id))
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
                            ChatColor.Players[id] = c;
                            ChatColor.UpdateXml();
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Edited chat color prefix for player id '{0}'. Name color set to '{1}' and prefix set to '{2}' using color '{3}'", id, colorTag1, _params[3], colorTag2));
                            return;
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to edit player id '{0}'. They are not on the chat color prefix list", _params[1]));
                            return;
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 3 or 5, found '{0}'", _params.Count));
                        return;
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count != 2)
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Wrong number of arguments, expected 2, found '{0}'", _params.Count));
                        return;
                    }
                    string steamId = "";
                    ClientInfo cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                    if (cInfo != null)
                    {
                        steamId = cInfo.PlatformId.ReadablePlatformUserIdentifier;
                    }
                    else
                    {
                        PlayerDataFile pdf = PersistentOperations.GetPlayerDataFileFromId(_params[1]);
                        if (pdf != null)
                        {
                            steamId = _params[1];
                        }
                        else
                        {
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to locate player '{0}' online or offline. Use steam id for offline player", _params[1]));
                            return;
                        }
                    }
                    if (ChatColor.Players.ContainsKey(steamId))
                    {
                        ChatColor.Players.Remove(steamId);
                        ChatColor.ExpireDate.Remove(steamId);
                        ChatColor.UpdateXml();
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Removed player id '{0}' from the chat color prefix list", _params[1]));
                        return;
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Unable to remove player id '{0}'. They are not on the chat color prefix list", _params[1]));
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
                            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Player id '{0}' named '{1}' using color '{2}' and prefix '{3}' using color '{4}' expires '{5}'", player.Key, player.Value[0], player.Value[1], player.Value[2], player.Value[3], expiry));
                        }
                    }
                    else
                    {
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[SERVERTOOLS] No players on the chat color list");
                    }
                }
                else
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("[SERVERTOOLS] Invalid argument '{0}'", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in ChatColorPrefixConsole.Execute: {0}", e.Message));
            }
        }
    }
}