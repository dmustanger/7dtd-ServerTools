
using System.Collections.Generic;
using System.Timers;

namespace ServerTools
{
    class PveProtection
    {
        private static int timerInstanceCount = 0;
        public static bool IsEnabled = false;
        public static int AdminLevel = 0;
        private static System.Timers.Timer timerPveProtection = new System.Timers.Timer();

        public static void PlayerStatCheckTimerStart()
        {
            timerInstanceCount++;
            if (timerInstanceCount <= 1)
            {
                int d = 2000;
                timerPveProtection.Interval = d;
                timerPveProtection.Start();
                timerPveProtection.Elapsed += new ElapsedEventHandler(PvEProt);
            }
        }

        public static void PlayerStatCheckTimerStop()
        {
            timerPveProtection.Stop();
        }

        public static void PvEProt(object sender, ElapsedEventArgs e)
        {
            int _playerCount = ConnectionManager.Instance.ClientCount();
            if (_playerCount > 0)
            {
                World world = GameManager.Instance.World;
                var enumerator = world.Players.list;
                foreach (var ent in enumerator)
                {
                    if (ent.IsClientControlled())
                    {
                        List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                        foreach (var _cInfo in _cInfoList)
                        {
                            GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId);
                            AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                            if (Admin.PermissionLevel > AdminLevel)
                            {
                                if (ent.entityId == _cInfo.entityId)
                                {
                                    var p_KilledPlayers = ent.KilledPlayers;
                                    var p_KilledZombies = ent.KilledZombies;
                                    var p_Score = ent.Score;
                                    var p_Died = ent.Died;
                                    var p_GetDamagedTarget = ent.GetDamagedTarget();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
