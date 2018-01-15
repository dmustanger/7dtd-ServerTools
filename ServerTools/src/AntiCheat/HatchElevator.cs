using System.Collections.Generic;
using System.Timers;

namespace ServerTools
{
    class HatchElevator
    {
        private static int timerInstanceCount = 0;
        public static bool IsEnabled = false;
        private static SortedDictionary<int, int> Flag = new SortedDictionary<int, int>();
        private static SortedDictionary<int, int> LastPositionY = new SortedDictionary<int, int>();
        private static System.Timers.Timer timerHatch = new System.Timers.Timer();

        public static void HatchElevatorTimerStart()
        {
            timerInstanceCount++;
            if (timerInstanceCount <= 1)
            {
                int d = 1000;
                timerHatch.Interval = d;
                timerHatch.Start();
                timerHatch.Elapsed += new ElapsedEventHandler(AutoHatchCheck);
            }
        }

        public static void HatchElevatorTimerStop()
        {
            timerHatch.Stop();
        }

        public static void AutoHatchCheck(object sender, ElapsedEventArgs e)
        {           
            if (ConnectionManager.Instance.ClientCount() > 0)
            {
                World world = GameManager.Instance.World;
                List<EntityPlayer>.Enumerator enumerator2 = world.Players.list.GetEnumerator();
                using (List<EntityPlayer>.Enumerator enumerator = enumerator2)
                    while (enumerator.MoveNext())
                    {
                        EntityPlayer ep = enumerator.Current;
                        var playerUsingHatchElevator = HatchCheck(ep);
                        if (playerUsingHatchElevator == true)
                        {
                            if (!Flag.ContainsKey(ep.entityId))
                            {
                                Flag.Add(ep.entityId, 1);
                            }
                            else
                            {
                                int _flag = 0;
                                if (Flag.TryGetValue(ep.entityId, out _flag))
                                {
                                    Flag.Remove(ep.entityId);
                                    Flag.Add(ep.entityId, _flag + 1);
                                }
                            }
                        }
                        else
                        {
                            if (Flag.ContainsKey(ep.entityId))
                            {
                                Flag.Remove(ep.entityId);
                            }
                        }
                    }
            }
        }

        public static bool HatchCheck(EntityPlayer ep)
        {
            int x = (int)ep.position.x;
            int y = (int)ep.position.y;
            int z = (int)ep.position.z;

            int Id = ep.entityId;

            if (LastPositionY.ContainsKey(Id))
            {
                for (int i = (x - 2); i <= (x + 2); i++)
                {
                    for (int j = (z - 2); j <= (z + 2); j++)
                    {
                        for (int k = (y - 1); k <= (y + 1); k++)
                        {
                            BlockValue Block = GameManager.Instance.World.GetBlock(new Vector3i(i, k, j));
                            if (Block.Block.blockID == 788 || Block.Block.blockID == 389 || Block.Block.blockID == 949)
                            {
                                continue;
                            }
                            if (Block.Block.blockID == 1251 || Block.Block.blockID == 1252 || Block.Block.blockID == 1253 ||
                                Block.Block.blockID == 1463 || Block.Block.blockID == 1464 || Block.Block.blockID == 1465 ||
                                Block.Block.blockID == 1469 || Block.Block.blockID == 1470 || Block.Block.blockID == 1471)
                            {
                                if (Flag.ContainsKey(Id))
                                {
                                    int _lastY;
                                    if (LastPositionY.TryGetValue(Id, out _lastY))
                                    {
                                        int heightChange = (_lastY - y);
                                        if (heightChange < -6)
                                        {
                                            Log.Out(string.Format("[SERVERTOOLS] Detected {0} using a hatch elevator. Applied stun and broke leg", ep.entityId));
                                            ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForEntityId(ep.entityId);
                                            _cInfo.SendPackage(new NetPackageConsoleCmdClient("buff " + "brokenLeg", true));
                                            _cInfo.SendPackage(new NetPackageConsoleCmdClient("buff " + "stunned", true));
                                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You are stunned and have broken your leg while smashing yourself through hatches. Ouch![-]", Config.ChatColor), "Server", false, "ServerTools", false));
                                            LastPositionY.Remove(Id);
                                            LastPositionY.Add(Id, y);
                                            return false;
                                        }
                                        else
                                        {
                                            LastPositionY.Remove(Id);
                                            LastPositionY.Add(Id, y);
                                            return true;
                                        }
                                    } 
                                }
                                else
                                {
                                    LastPositionY.Remove(Id);
                                    LastPositionY.Add(Id, y);
                                    return true;
                                }
                            }
                        }
                    }
                }
                return false;
            }
            else
            {
                LastPositionY.Remove(Id);
                LastPositionY.Add(Id, y);
                return false;
            }
        }
    }
}
