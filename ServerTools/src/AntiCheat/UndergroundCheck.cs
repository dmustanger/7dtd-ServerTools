using System;
using System.Collections.Generic;
using System.IO;

namespace ServerTools
{
    class UndergroundCheck : ConsoleCmdAbstract
    {
        public static bool IsEnabled = false, Kill_Player = false, Announce = false, Jail_Enabled = false, Kick_Enabled = false, Ban_Enabled = false;
        public static int Admin_Level = 0, Max_Ping = 300, Days_Before_Log_Delete = 5;
        public static Dictionary<string, int> Flag = new Dictionary<string, int>();
        public static Dictionary<int, int[]> uLastPositionXZ = new Dictionary<int, int[]>();

        public override string GetDescription()
        {
            return "Check if a player is underground with no clip or stuck";
        }

        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. uc <steam id / player name / entity id> \n" +
                "  2. uc \n\n" +
                "1. List 1 player underground\n" +
                "2. List all players underground\n";
        }

        public override string[] GetCommands()
        {
            return new string[] { "undergroundcheck", "uc" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count > 1)
                {
                    SdtdConsole.Instance.Output("Wrong number of arguments, expected 0 or 1, found " + _params.Count + ".");
                    return;
                }
                else
                {
                    if (_params.Count == 1)
                    {
                        ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[0]);
                        if (_cInfo == null)
                        {
                            SdtdConsole.Instance.Output("Playername or entity/steamid id not found.");
                            return;
                        }
                        EntityPlayer ep = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                        SdtdConsole.Instance.Output("UC: entity_id=" + ep.entityId + " isUnderGround=" + getPlayerUnderground(ep));
                    }
                    else
                    {
                        World world = GameManager.Instance.World;
                        List<EntityPlayer>.Enumerator enumerator2 = world.Players.list.GetEnumerator();
                        using (List<EntityPlayer>.Enumerator enumerator = enumerator2)
                        {
                            while (enumerator.MoveNext())
                            {
                                EntityPlayer ep = enumerator.Current;
                                SdtdConsole.Instance.Output("UC: entity_id=" + ep.entityId + " isUnderGround=" + getPlayerUnderground(ep));
                            }
                        }
                    }

                }
            }
            catch (Exception e)
            {
                Log.Out("Error in UndergroundCheck.Run: " + e);
            }
        }

        public bool getPlayerUnderground(EntityPlayer ep)
        {
            int x = (int)ep.position.x;
            int y = (int)ep.position.y;
            int z = (int)ep.position.z;
            for (int i = x - 2; i <= (x + 2); i++)
            {
                for (int j = z - 2; j <= (z + 2); j++)
                {
                    for (int k = y - 3; k <= (y + 2); k++)
                    {
                        BlockValue Block = GameManager.Instance.World.GetBlock(new Vector3i(i, k, j));
                        if (Block.type == BlockValue.Air.type || ep.IsInElevator() || ep.IsInWater())
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public static void DetectionLogsDir()
        {
            if (!Directory.Exists(API.GamePath + "/DetectionLogs"))
            {
                Directory.CreateDirectory(API.GamePath + "/DetectionLogs");
            }
            string[] files = Directory.GetFiles(API.GamePath + "/DetectionLogs");
            int _daysBeforeDeleted = (Days_Before_Log_Delete * -1);
            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                if (fi.CreationTime < DateTime.Now.AddDays(_daysBeforeDeleted))
                {
                    fi.Delete();
                }
            }
        }

        public static void AutoUndergroundCheck()
        {
            if (ConnectionManager.Instance.ClientCount() > 0)
            {
                List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                foreach (var _cInfo in _cInfoList)
                {
                    GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId);
                    AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                    if (Admin.PermissionLevel > Admin_Level)
                    {
                        World world = GameManager.Instance.World;
                        List<EntityPlayer>.Enumerator enumerator2 = world.Players.list.GetEnumerator();
                        using (List<EntityPlayer>.Enumerator enumerator = enumerator2)
                            while (enumerator.MoveNext())
                            {
                                EntityPlayer ep = enumerator.Current;
                                if (ep.entityId == _cInfo.entityId && !ep.AttachedToEntity)
                                {
                                    var playerInGround = autoGetPlayerUnderground(ep, _cInfo);
                                    if (playerInGround == true)
                                    {
                                        if (!Flag.ContainsKey(_cInfo.playerId))
                                        {
                                            Flag.Add(_cInfo.playerId, 1);
                                        }
                                        else
                                        {
                                            int _flag = 0;
                                            if (Flag.TryGetValue(_cInfo.playerId, out _flag))
                                            {
                                                int x = (int)ep.position.x;
                                                int y = (int)ep.position.y;
                                                int z = (int)ep.position.z;
                                                {
                                                    Flag.Remove(_cInfo.playerId);
                                                    Flag.Add(_cInfo.playerId, _flag + 1);
                                                }
                                                if (_flag == 2)
                                                {
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("tele {0} {1} -1 {2}", ep.entityId, x, z), (ClientInfo)null);
                                                    Penalty(_cInfo);
                                                    Log.Warning(string.Format("[SERVERTOOLS] Detected {0}, Steam Id {1}, flying underground @ {2} {3} {4}. ", _cInfo.playerName, _cInfo.steamId, x, y, z));
                                                    string _file = string.Format("DetectionLog_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
                                                    string _filepath = string.Format("{0}/DetectionLogs/{1}", API.GamePath, _file);
                                                    using (StreamWriter sw = new StreamWriter(_filepath, true))
                                                    {
                                                        sw.WriteLine(string.Format("Detected {0}, Steam Id {1}, flying underground @ {2} {3} {4}. ", _cInfo.playerName, _cInfo.steamId, x, y, z));
                                                        sw.WriteLine();
                                                        sw.Flush();
                                                        sw.Close();
                                                    }                                                    
                                                    List<ClientInfo> _cInfoList2 = ConnectionManager.Instance.GetClients();
                                                    foreach (var _cInfo2 in _cInfoList2)
                                                    {
                                                        AdminToolsClientInfo Admin2 = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo2.playerId);
                                                        if (Admin2.PermissionLevel <= Admin_Level)
                                                        {
                                                            string _phrase710;
                                                            if (!Phrases.Dict.TryGetValue(710, out _phrase710))
                                                            {
                                                                _phrase710 = "Detected {PlayerName} flying underground @ {X} {Y} {Z}.";
                                                            }
                                                            _phrase710 = _phrase710.Replace("{PlayerName}", _cInfo.playerName);
                                                            _phrase710 = _phrase710.Replace("{X}", x.ToString());
                                                            _phrase710 = _phrase710.Replace("{Y}", y.ToString());
                                                            _phrase710 = _phrase710.Replace("{Z}", z.ToString());
                                                            _cInfo2.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase710), "Server", false, "", false));
                                                        }
                                                    }
                                                    Flag.Remove(_cInfo.playerId);
                                                    if (Announce)
                                                    {
                                                        string _phrase711;
                                                        if (!Phrases.Dict.TryGetValue(711, out _phrase711))
                                                        {
                                                            _phrase711 = "{PlayerName} has been detected flying underground.";
                                                        }
                                                        _phrase711 = _phrase711.Replace("{PlayerName}", _cInfo.playerName);
                                                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase711), "Server", false, "", false);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (Flag.ContainsKey(_cInfo.playerId))
                                        {
                                            Flag.Remove(_cInfo.playerId);
                                        }
                                    }
                                }
                            }
                    }
                }
            }
        }

        public static bool autoGetPlayerUnderground(EntityPlayer ep, ClientInfo _cInfo)
        {
            int x = (int)ep.position.x;
            int y = (int)ep.position.y;
            int z = (int)ep.position.z;

            int Id = ep.entityId;
            int[] xz = { x, z };

            if (uLastPositionXZ.ContainsKey(Id))
            {
                int[] last_xz_pos;
                uLastPositionXZ.TryGetValue(Id, out last_xz_pos);
                if (last_xz_pos != xz)
                {
                    uLastPositionXZ.Remove(Id);
                    uLastPositionXZ.Add(Id, xz);
                    if (_cInfo.ping < Max_Ping)
                    {
                        for (int i = x - 3; i <= (x + 3); i++)
                        {
                            for (int j = z - 3; j <= (z + 3); j++)
                            {
                                for (int k = y - 4; k <= (y + 3); k++)
                                {
                                    BlockValue Block = GameManager.Instance.World.GetBlock(new Vector3i(i, k, j));
                                    if (Block.type == BlockValue.Air.type || ep.IsInElevator() || ep.IsInWater() || Block.Block.blockID == 1562 || Block.Block.blockID == 631 ||
                                        Block.Block.blockID == 642 || Block.Block.blockID == 643 || Block.Block.blockID == 1248 || Block.Block.blockID == 1249 || Block.Block.blockID == 1250 ||
                                        Block.Block.blockID == 1449 || Block.Block.blockID == 1450 || Block.Block.blockID == 1451 || Block.Block.blockID == 93 || Block.Block.blockID == 1251 ||
                                        Block.Block.blockID == 1469 || Block.Block.blockID == 1470 || Block.Block.blockID == 1471 || Block.Block.blockID == 901 || Block.Block.blockID == 902 ||
                                        Block.Block.blockID == 903 || Block.Block.blockID == 94 || Block.Block.blockID == 1934 || Block.Block.blockID == 749 || Block.Block.blockID == 257 ||
                                        Block.Block.blockID == 258 || Block.Block.blockID == 259 || Block.Block.blockID == 260 || Block.Block.blockID == 261 || Block.Block.blockID == 262 ||
                                        Block.Block.blockID == 527 || Block.Block.blockID == 528 || Block.Block.blockID == 529 || Block.Block.blockID == 530 || Block.Block.blockID == 531 ||
                                        Block.Block.blockID == 532 || Block.Block.blockID == 533 || Block.Block.blockID == 534 || Block.Block.blockID == 713 || Block.Block.blockID == 714 ||
                                        Block.Block.blockID == 759 || Block.Block.blockID == 760 || Block.Block.blockID == 761 || Block.Block.blockID == 762 || Block.Block.blockID == 763 ||
                                        Block.Block.blockID == 764 || Block.Block.blockID == 853 || Block.Block.blockID == 854 || Block.Block.blockID == 855 || Block.Block.blockID == 856 ||
                                        Block.Block.blockID == 869 || Block.Block.blockID == 870 || Block.Block.blockID == 884 || Block.Block.blockID == 959 || Block.Block.blockID == 960 ||
                                        Block.Block.blockID == 961 || Block.Block.blockID == 962 || Block.Block.blockID == 826 || Block.Block.blockID == 900 || Block.Block.blockID == 1252 ||
                                        Block.Block.blockID == 1253 || Block.Block.blockID == 389 || Block.Block.blockID == 788 || Block.Block.blockID == 949)
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    Flag.Remove(_cInfo.playerId);
                    uLastPositionXZ.Remove(Id);
                    uLastPositionXZ.Add(Id, xz);
                    return false;
                }
            }
            else
            {
                uLastPositionXZ.Remove(Id);
                uLastPositionXZ.Add(Id, xz);
                return false;
            }
        }

        public static void Penalty(ClientInfo _cInfo)
        {
            if (Jail_Enabled)
            {
                Flag.Remove(_cInfo.playerId);
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} has been jailed for flying underground[-]", Config.Chat_Response_Color, _cInfo.playerName), "Server", false, "", false);
                SdtdConsole.Instance.ExecuteSync(string.Format("jail add {0}", _cInfo.playerId), (ClientInfo)null);
            }
            if (Kill_Player)
            {
                Flag.Remove(_cInfo.playerId);
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} has been killed for flying underground[-]", Config.Chat_Response_Color, _cInfo.playerName), "Server", false, "", false);
                SdtdConsole.Instance.ExecuteSync(string.Format("kill {0}", _cInfo.playerId), (ClientInfo)null);
            }
            if (Kick_Enabled)
            {
                Flag.Remove(_cInfo.playerId);
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} has been kicked for flying underground[-]", Config.Chat_Response_Color, _cInfo.playerName), "Server", false, "", false);
                SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for flying\"", _cInfo.playerId), (ClientInfo)null);
            }
            if (Ban_Enabled)
            {
                Flag.Remove(_cInfo.playerId);
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1} has been banned for flying underground[-]", Config.Chat_Response_Color, _cInfo.playerName), "Server", false, "", false);
                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for flying\"", _cInfo.playerId), (ClientInfo)null);
            }
        }
    }
}