using System;
using System.Collections.Generic;
using System.Timers;

namespace ServerTools
{
    class UndergroundCheck : ConsoleCmdAbstract
    {
        private static int timerInstanceCount = 0;
        public static bool IsEnabled = false;
        public static bool KillPlayer = false;
        public static bool Announce = false;
        public static bool JailEnabled = false;
        public static bool KickEnabled = false;
        public static bool BanEnabled = false;
        public static int AdminLevel = 0;       
        private static SortedDictionary<string, int> Flag = new SortedDictionary<string, int>();
        private static SortedDictionary<int, string> uLastPositionXZ = new SortedDictionary<int, string>();
        private static System.Timers.Timer timerUnderground = new System.Timers.Timer();

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
                        ClientInfo ci = ConsoleHelper.ParseParamIdOrName(_params[0]);
                        if (ci == null)
                        {
                            SdtdConsole.Instance.Output("Playername or entity/steamid id not found.");
                            return;
                        }
                        EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
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

        public static void UndergroundTimerStart()
        {
            timerInstanceCount++;
            if (timerInstanceCount <= 1)
            {
                int d = 2500;
                timerUnderground.Interval = d;
                timerUnderground.Start();
                timerUnderground.Elapsed += new ElapsedEventHandler(AutoUndergroundCheck);
            }
        }

        public static void UndergroundTimerStop()
        {
            timerUnderground.Stop();
        }

        public static void AutoUndergroundCheck(object sender, ElapsedEventArgs e)
        {
            List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
            foreach (var _cInfo in _cInfoList)
            {
                GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId);
                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                if (Admin.PermissionLevel > AdminLevel)
                {
                    World world = GameManager.Instance.World;
                    List<EntityPlayer>.Enumerator enumerator2 = world.Players.list.GetEnumerator();
                    using (List<EntityPlayer>.Enumerator enumerator = enumerator2)
                        while (enumerator.MoveNext())
                        {
                            EntityPlayer ep = enumerator.Current;
                            if (ep.entityId == _cInfo.entityId && !ep.AttachedToEntity)
                            {
                                var playerInGround = autoGetPlayerUnderground(ep);
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
                                            {
                                                Flag.Remove(_cInfo.playerId);
                                                Flag.Add(_cInfo.playerId, _flag + 1);
                                            }
                                            if (_flag > 1)
                                            {
                                                Log.Warning("[SERVERTOOLS] Detected {0}, Steam Id {1}, flying underground. ", _cInfo.playerName, _cInfo.steamId);

                                                int x = (int)ep.position.x;
                                                int y = (int)ep.position.y;
                                                int z = (int)ep.position.z;

                                                if (Announce)
                                                {
                                                    GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0} has been detected flying underground[-]", _cInfo.playerName), "Server", false, "", false);
                                                    if (_flag == 4)
                                                    {
                                                        Flag.Remove(_cInfo.playerId);
                                                        if (Admin.PermissionLevel <= AdminLevel && ep.entityId != _cInfo.entityId)
                                                        {
                                                            SdtdConsole.Instance.ExecuteSync(string.Format("pm {0} \"Detected {1} flying underground @ {2} {3} {4}\"", _cInfo.playerId, ep.EntityName, x, y, z), _cInfo);
                                                        }
                                                    }
                                                }
                                                if (JailEnabled)
                                                {
                                                    Flag.Remove(_cInfo.playerId);
                                                    GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0} has been jailed for flying underground[-]", _cInfo.playerName), "Server", false, "", false);
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("jail add {0}", _cInfo.playerId), _cInfo);
                                                }
                                                if (KillPlayer)
                                                {
                                                    Flag.Remove(_cInfo.playerId);
                                                    GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0} has been killed for flying underground[-]", _cInfo.playerName), "Server", false, "", false);
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("kill {0}", _cInfo.playerId), _cInfo);
                                                }
                                                if (KickEnabled)
                                                {
                                                    Flag.Remove(_cInfo.playerId);
                                                    GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0} has been kicked for flying underground[-]", _cInfo.playerName), "Server", false, "", false);
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Auto detection has kicked you for flying\"", _cInfo.playerId), _cInfo);
                                                }
                                                if (BanEnabled)
                                                {
                                                    Flag.Remove(_cInfo.playerId);
                                                    GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0} has been banned for flying underground[-]", _cInfo.playerName), "Server", false, "", false);
                                                    SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 5 years \"Auto detection has banned you for flying\"", _cInfo.playerId), _cInfo);
                                                }
                                                SdtdConsole.Instance.ExecuteSync(string.Format("tele {0} {1} -1 {2}", ep.entityId, x, z), _cInfo);
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

        public static bool autoGetPlayerUnderground(EntityPlayer ep)
        {
            int x = (int)ep.position.x;
            int y = (int)ep.position.y;
            int z = (int)ep.position.z;

            int Id = ep.entityId;
            string xz = (x.ToString() + z.ToString());

            if (uLastPositionXZ.ContainsKey(Id))
            {
                string last_xz_pos;
                uLastPositionXZ.TryGetValue(Id, out last_xz_pos);
                if (last_xz_pos != xz)
                {
                    uLastPositionXZ.Remove(Id);
                    uLastPositionXZ.Add(Id, xz);
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
                                    Block.Block.blockID == 961 || Block.Block.blockID == 962)
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
    }
}