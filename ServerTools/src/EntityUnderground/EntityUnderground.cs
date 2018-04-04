using System.Collections.Generic;
using System.Linq;

namespace ServerTools
{
    class EntityUnderground
    {
        public static bool IsEnabled = false, Alert_Admin = false;
        public static int Admin_Level = 0;
        private static SortedDictionary<int, int> Flag = new SortedDictionary<int, int>();

        public static bool getEntityUnderground(Entity ent)
        {
            int x = (int)ent.position.x;
            int y = (int)ent.position.y;
            int z = (int)ent.position.z;

            for (int i = x - 1; i <= (x + 1); i++)
            {
                for (int j = z - 1; j <= (z + 1); j++)
                {
                    for (int k = y - 1; k <= (y + 1); k++)
                    {
                        BlockValue Block = GameManager.Instance.World.GetBlock(new Vector3i(i, k, j));
                        if (Block.type == BlockValue.Air.type || ent.IsInElevator() || ent.IsInWater() || Block.Block.blockID == 1562 || Block.Block.blockID == 631 ||
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
                                    Block.Block.blockID == 1253)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public static void AutoEntityUnderground()
        {
            if (IsEnabled)
            {
                if (ConnectionManager.Instance.ClientCount() > 0)
                {
                    World world = GameManager.Instance.World;
                    List<Entity> EntityList = world.Entities.list;
                    for (int i = 0; i < EntityList.Count; i++)
                    {
                        Entity ent = EntityList[i];
                        if (!ent.IsClientControlled())
                        {
                            var entityInGround = getEntityUnderground(ent);
                            if (entityInGround == true)
                            {
                                if (!Flag.ContainsKey(ent.entityId))
                                {
                                    Flag.Add(ent.entityId, 1);
                                }
                                else
                                {
                                    var x = (int)ent.position.x;
                                    var y = (int)ent.position.y;
                                    var z = (int)ent.position.z;

                                    int _flag = 0;
                                    if (Flag.TryGetValue(ent.entityId, out _flag))
                                    {
                                        {
                                            Flag.Remove(ent.entityId);
                                            Flag.Add(ent.entityId, _flag + 1);
                                        }
                                        if (_flag > 1)
                                        {
                                            SdtdConsole.Instance.ExecuteSync(string.Format("telee {0} {1} -1 {2}", ent.entityId, x, z), null);
                                        }
                                        if (_flag > 9)
                                        {
                                            if (Alert_Admin)
                                            {
                                                List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                                                for (int j = 0; j < _cInfoList.Count; j++)
                                                {
                                                    ClientInfo _cInfo = _cInfoList[j];
                                                    GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId);
                                                    AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                                                    if (Admin.PermissionLevel <= Admin_Level)
                                                    {
                                                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Detected entity # {1} underground @ {2} {3} {4}. Teleported it to the surface.[-]", Config.Chat_Response_Color, ent.entityId, x, y, z), Config.Server_Response_Name, false, "ServerTools", false));
                                                    }
                                                }
                                            }
                                            Flag.Remove(ent.entityId);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (Flag.ContainsKey(ent.entityId))
                                {
                                    Flag.Remove(ent.entityId);
                                }
                            }
                        }
                        if (ent.IsClientControlled())
                        {
                            if (ent.IsStuck)
                            {
                                if (!Flag.ContainsKey(ent.entityId))
                                {
                                    Flag[ent.entityId] = 1;
                                }
                                else
                                {
                                    var x = (int)ent.position.x;
                                    var y = (int)ent.position.y;
                                    var z = (int)ent.position.z;
                                    int _flag = 0;
                                    if (Flag.TryGetValue(ent.entityId, out _flag))
                                    {
                                        {
                                            Flag[ent.entityId] = _flag + 1;
                                        }
                                        if (_flag > 2)
                                        {
                                            SdtdConsole.Instance.ExecuteSync(string.Format("teleportplayer {0} {1} -1 {2}", ent.entityId, x, z), (ClientInfo)null);
                                            Log.Out("Detected player was stuck underground and teleported them to the surface. Their position was {1} {2} {3}", ent.entityId, x, y, z);
                                            List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                                            for (int j = 0; j < _cInfoList.Count; j++)
                                            {
                                                ClientInfo _cInfo = _cInfoList[j];
                                                GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId);
                                                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                                                if (Admin.PermissionLevel <= Admin_Level)
                                                {
                                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Detected player # {1} stuck underground @ {2} {3} {4}. Teleported them to the surface.[-]", Config.Chat_Response_Color, ent.entityId, x, y, z), Config.Server_Response_Name, false, "ServerTools", false));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            Flag.Remove(ent.entityId);
                        }
                    }
                }
            }
        }
    }
}
