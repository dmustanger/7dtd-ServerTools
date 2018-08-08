using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    class TeleportDelay
    {
        public static bool TeleQue = false, PvP_Check = false, Zombie_Check = false;
        private static Dictionary<ClientInfo, int[]> que = new Dictionary<ClientInfo, int[]>();
        private static Dictionary<ClientInfo, DateTime> queTime = new Dictionary<ClientInfo, DateTime>();

        public static void TeleportQue(ClientInfo _cInfo, int _x, int _y, int _z)
        {
            int[] _xyz = { _x, _y, _z };
            que.Add(_cInfo, _xyz);
            queTime.Add(_cInfo, DateTime.Now);
            TeleQue = true;
        }

        public static void Que()
        {
            if (que.Count > 0)
            {
                foreach (KeyValuePair<ClientInfo, int[]> _cInfoPos in que)
                {
                    DateTime _time;
                    queTime.TryGetValue(_cInfoPos.Key, out _time);
                    TimeSpan varTime = DateTime.Now - _time;
                    double fractionalSeconds = varTime.TotalSeconds;
                    int _timepassed = (int)fractionalSeconds;
                    if (_timepassed >= 4)
                    {
                        TeleportPlayer(_cInfoPos);
                        que.Remove(_cInfoPos.Key);
                        queTime.Remove(_cInfoPos.Key);
                    }
                }
            }
            else
            {
                TeleQue = false;
            }
        }

        public static void TeleportPlayer(KeyValuePair<ClientInfo, int[]> _cInfoPos)
        {
            _cInfoPos.Key.SendPackage(new NetPackageTeleportPlayer(new Vector3(_cInfoPos.Value[0], _cInfoPos.Value[1], _cInfoPos.Value[2]), false));
        }
    }
}
