using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ServerTools
{
    class UnderWater
    {
        public static bool IsEnabled = false, InOperation = false;
        private static List<int> Flag = new List<int>();

        public static void Exec()
        {
            if (ConnectionManager.Instance.ClientCount() > 0)
            {
                List<ClientInfo> ClientInfoList = ConnectionManager.Instance.Clients.List.ToList();
                for (int i = 0; i < ClientInfoList.Count; i++)
                {
                    ClientInfo _cInfo = ClientInfoList[i];
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    int x = (int)_player.position.x;
                    int y = (int)_player.position.y;
                    int z = (int)_player.position.z;
                    if (WaterCheck(_cInfo, _player, x, y, z))
                    {
                        if (Flag.Contains(_cInfo.entityId))
                        {
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(new Vector3(x, -1, z), null, false));
                            Flag.Remove(_cInfo.entityId);
                        }
                        else
                        {
                            Flag.Add(_cInfo.entityId);
                        }
                    }
                    else
                    {
                        if (Flag.Contains(_cInfo.entityId))
                        {
                            Flag.Remove(_cInfo.entityId);
                        }
                    }
                }
            }
        }

        public static bool WaterCheck(ClientInfo _cInfo, EntityPlayer _player, int x, int y, int z)
        {
            for (int i = x - 1; i <= (x + 1); i++)
            {
                for (int j = z - 1; j <= (z + 1); j++)
                {
                    for (int k = y - 0; k <= (y + 2); k++)
                    {
                        BlockValue Block = GameManager.Instance.World.GetBlock(new Vector3i(i, k, j));
                        string _blockName = Block.Block.GetBlockName();
                        if (_blockName != "waterMoving" || _blockName != "water")
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}
