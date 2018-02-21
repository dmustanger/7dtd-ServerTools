
using System;
using System.Collections.Generic;
using System.Timers;

namespace ServerTools
{
    class WorldRadius
    {
        private static int timerInstanceCount = 0;
        public static bool IsEnabled = false, ZoneWarnings = false;
        public static int AdminLevel = 0, WorldSize = 10000;
        private static System.Timers.Timer timerZoneProtection = new System.Timers.Timer();
        public static string PZone1 = "0,75,0";
        public static string PZone2 = "0,75,0";
        public static string PZone3 = "0,75,0";
        public static string PZone4 = "0,75,0";
        private static SortedDictionary<int, int[]> LastPosition = new SortedDictionary<int, int[]>();
        private static List<int> ZoneEdge = new List<int>();


        public static void ZoneProtectionTimerStart()
        {
            timerInstanceCount++;
            if (timerInstanceCount <= 1)
            {
                int d = 2000;
                timerZoneProtection.Interval = d;
                timerZoneProtection.Start();
                timerZoneProtection.Elapsed += new ElapsedEventHandler(PvEProt);
            }
        }

        public static void ZoneProtectionTimerStop()
        {
            timerZoneProtection.Stop();
        }

        public static void PvEProt(object sender, ElapsedEventArgs e)
        {
            if (ConnectionManager.Instance.ClientCount() > 0)
            {
                World world = GameManager.Instance.World;
                var enumerator = world.Players.list;
                foreach (var _player in enumerator)
                {
                    Vector3i _playerVector = new Vector3i(_player.GetPosition());
                    int player_Pos_X = _playerVector.x, player_Pos_Z = _playerVector.z;
                    int[] XZ = { player_Pos_X, player_Pos_Z };
                    if (LastPosition.ContainsKey(_player.entityId))
                    {
                        ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForEntityId(_player.entityId);
                        if (player_Pos_X < 0)
                        {
                            player_Pos_X = (player_Pos_X * -1);
                        }
                        if (player_Pos_Z < 0)
                        {
                            player_Pos_Z = (player_Pos_Z * -1);
                        }
                        var playerRadius = Math.Sqrt((Math.Sqrt(player_Pos_X)) + (Math.Sqrt(player_Pos_Z)));
                        var worldRadius = Math.Sqrt(Math.Sqrt(WorldSize));                       
                        if (ZoneWarnings & !ZoneEdge.Contains(_player.entityId))
                        {
                            var worldRadiusWarning = Math.Sqrt(Math.Sqrt(WorldSize - 75));
                            if (playerRadius >= worldRadiusWarning)
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}You are near the edge of the world {1}[-]", Config.ChatResponseColor, _cInfo.playerName), "Server", false, "", false));
                                ZoneEdge.Add(_player.entityId);
                            }
                        }
                        if (ZoneWarnings & ZoneEdge.Contains(_player.entityId))
                        {
                            var worldRadiusWarning = Math.Sqrt(Math.Sqrt(WorldSize - 75));
                            if (playerRadius < worldRadiusWarning)
                            {
                                ZoneEdge.Remove(_player.entityId);
                            }
                        }
                        if (playerRadius > worldRadius)
                        {
                            if (_player.Spawned)
                            {
                                int[] _xz;
                                if (LastPosition.TryGetValue(_player.entityId, out _xz))
                                {
                                    int _x = _xz[0], _z = _xz[1];
                                    if (_x < 0)
                                    {
                                        _x = (_x + 5);
                                    }
                                    if (_x > 0)
                                    {
                                        _x = (_x - 5);
                                    }
                                    if (_z < 0)
                                    {
                                        _z = (_z + 5);
                                    }
                                    if (_z > 0)
                                    {
                                        _z = (_z - 5);
                                    }
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}Reached world border limit[-]", Config.ChatResponseColor, WorldSize), "Server", false, "", false));
                                    SdtdConsole.Instance.ExecuteSync(string.Format("tele {0} {1} {2} {3}", _cInfo.entityId, _x, -1, _z), (ClientInfo)null);
                                }
                            }
                        }
                        else
                        {
                            LastPosition.Remove(_player.entityId);
                            LastPosition.Add(_player.entityId, XZ);
                        }
                    }
                    else
                    {
                        LastPosition.Add(_player.entityId, XZ);
                    }
                }
            }
        }
    }
}
