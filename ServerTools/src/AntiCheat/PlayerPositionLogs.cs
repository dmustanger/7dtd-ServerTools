using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;

namespace ServerTools
{
    class PlayerPositionLogs
    {
        private static int timerInstanceCount = 0;
        public static bool IsEnabled = false;
        private static System.Timers.Timer timerPosLogs = new System.Timers.Timer();

        public static void PlayerPositionLogsDir()
        {
            if (!Directory.Exists(API.GamePath + "/PlayerPositionLogs"))
            {
                Directory.CreateDirectory(API.GamePath + "/PlayerPositionLogs");
            }
        }

        public static void PlayerPositionLogsStart()
        {
            timerInstanceCount++;
            if (timerInstanceCount <= 1)
            {
                int d = 60000;
                timerPosLogs.Interval = d;
                timerPosLogs.Start();
                timerPosLogs.Elapsed += new ElapsedEventHandler(Move_Log);
            }
        }

        public static void PlayerPositionLogsStop()
        {
            timerPosLogs.Stop();
        }

        public static void Move_Log(object sender, ElapsedEventArgs e)
        {
            string _file = string.Format("PlayerPositionLog{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
            string _filepath = string.Format("{0}/PlayerPositionLogs/{1}", API.GamePath, _file);

            int _playerCount = ConnectionManager.Instance.ClientCount();
            if (_playerCount > 0)
            {
                World world = GameManager.Instance.World;
                var enumerator = world.Entities.list;
                foreach (var ent in enumerator)
                {
                    if (ent.IsClientControlled())
                    {
                        var x = (int)ent.position.x;
                        var y = (int)ent.position.y;
                        var z = (int)ent.position.z;

                        List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
                        foreach (var _cInfo in _cInfoList)
                        {
                            if (ent.entityId == _cInfo.entityId)
                            {
                                using (StreamWriter sw = new StreamWriter(_filepath, true))
                                {
                                    sw.WriteLine(string.Format("{0} {1}:{2} POS: {3} X {4} Y {5} Z", DateTime.Now, _cInfo.playerName, _cInfo.playerId, x, y, z));
                                    sw.Flush();
                                    sw.Close();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
